using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLib
{
    public sealed class SocketListener : IDisposable
    {
        /// <summary>
        /// 缓冲区
        /// </summary>
        private BufferManager bufferManager;
        /// <summary>
        /// 服务器端Socket
        /// </summary>
        private Socket listenSocket;
        /// <summary>
        /// 服务同步锁
        /// </summary>
        private static ManualResetEvent mutex = new ManualResetEvent(false);
        /// <summary>
        /// 当前连接数
        /// </summary>
        private Int32 numConnections;
        /// <summary>
        /// 最大并发量
        /// </summary>
        private Int32 numConcurrence;
        /// <summary>
        /// 服务器状态
        /// </summary>
        private ServerState serverstate;
        /// <summary>
        /// 读取写入字节
        /// </summary>
        private const Int32 opsToPreAlloc = 1;
        /// <summary>
        /// Socket连接池
        /// </summary>
        private SocketAsyncEventArgsPool readWritePool;
        /// <summary>
        /// 并发控制信号量
        /// </summary>
        private Semaphore semaphoreAcceptedClients;
        /// <summary>
        /// 通信协议
        /// </summary>
        private RequestHandler handler;
        /// <summary>
        /// 回调委托
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public delegate string GetIDByIPFun(string IP);
        /// <summary>
        /// 回调方法实例
        /// </summary>
        private GetIDByIPFun GetIDByIP;
        /// <summary>
        /// 接收到信息时的事件委托
        /// </summary>
        /// <param name="info"></param>
        public delegate void ReceiveMsgHandler(string uid, string info);
        /// <summary>
        /// 接收到信息时的事件
        /// </summary>
        public event ReceiveMsgHandler OnMsgReceived;
        /// <summary>
        /// 开始监听数据的委托
        /// </summary>
        public delegate void StartListenHandler();
        /// <summary>
        /// 开始监听数据的事件
        /// </summary>
        public event StartListenHandler StartListenThread;
        /// <summary>
        /// 发送信息完成后的委托
        /// </summary>
        /// <param name="successorfalse"></param>
        public delegate void SendCompletedHandler(string uid, string exception);
        /// <summary>
        /// 发送信息完成后的事件
        /// </summary>
        public event SendCompletedHandler OnSended;

        /// <summary>
        /// 获取当前的并发数
        /// </summary>
        public Int32 NumConnections
        {
            get { return this.numConnections; }
        }
        /// <summary>
        /// 最大并发数
        /// </summary>
        public Int32 MaxConcurrence
        {
            get { return this.numConcurrence; }
        }
        /// <summary>
        /// 返回服务器状态
        /// </summary>
        public ServerState State
        {
            get
            {
                return serverstate;
            }
        }
        /// <summary>
        /// 获取当前在线用户的UID
        /// </summary>
        public string[] OnlineUID
        {
            get { return readWritePool.OnlineUID; }
        }

        /// <summary>
        /// 初始化服务器端
        /// </summary>
        /// <param name="numConcurrence">并发的连接数量(1000以上)</param>
        /// <param name="receiveBufferSize">每一个收发缓冲区的大小(32768)</param>
        public SocketListener(Int32 numConcurrence, Int32 receiveBufferSize, GetIDByIPFun GetIDByIP)
        {
            serverstate = ServerState.Initialing;
            this.numConnections = 0;
            this.numConcurrence = numConcurrence;
            this.bufferManager = new BufferManager(receiveBufferSize * numConcurrence * opsToPreAlloc, receiveBufferSize);
            this.readWritePool = new SocketAsyncEventArgsPool(numConcurrence);
            this.semaphoreAcceptedClients = new Semaphore(numConcurrence, numConcurrence);
            handler = new RequestHandler();
            this.GetIDByIP = GetIDByIP;
        }

        /// <summary>
        /// 服务端初始化
        /// </summary>
        public void Init()
        {
            this.bufferManager.InitBuffer();
            SocketAsyncEventArgsWithId readWriteEventArgWithId;
            for (Int32 i = 0; i < this.numConcurrence; i++)
            {
                readWriteEventArgWithId = new SocketAsyncEventArgsWithId();
                readWriteEventArgWithId.ReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
                readWriteEventArgWithId.SendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
                //只给接收的SocketAsyncEventArgs设置缓冲区
                this.bufferManager.SetBuffer(readWriteEventArgWithId.ReceiveSAEA);
                this.readWritePool.Push(readWriteEventArgWithId);
            }
            serverstate = ServerState.Inited;
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="data">端口号</param>
        public void Start(Object data)
        {
            Int32 port = (Int32)data;
            IPAddress[] addresslist = Dns.GetHostEntry(Environment.MachineName).AddressList;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {
                this.listenSocket.Bind(localEndPoint);
            }
            this.listenSocket.Listen(100);
            this.StartAccept(null);
            //ThreadPool.QueueUserWorkItem((o) => {  });
            ThreadPool.QueueUserWorkItem((o) => { Listen(); });
            //开始监听已连接用户的发送数据
            StartListenThread();
            serverstate = ServerState.Running;
            //mutex.WaitOne();
        }

        /// <summary>
        /// 开始监听线程的入口函数
        /// </summary>
        public void Listen()
        {
            while (true)
            {
                string[] keys = readWritePool.OnlineUID;
                foreach (string uid in keys)
                {
                    if (uid != null && readWritePool.busypool[uid].ReceiveSAEA.LastOperation != SocketAsyncOperation.Receive)
                    {
                        Boolean willRaiseEvent = (readWritePool.busypool[uid].ReceiveSAEA.UserToken as Socket).ReceiveAsync(readWritePool.busypool[uid].ReceiveSAEA);
                        if (!willRaiseEvent)
                            ProcessReceive(readWritePool.busypool[uid].ReceiveSAEA);
                    }
                }
            }
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="uid">要发送的用户的uid</param>
        /// <param name="msg">消息体</param>
        public void Send(string uid, string msg)
        {
            if (uid == string.Empty || uid == "" || msg == string.Empty || msg == "")
                return;
            SocketAsyncEventArgsWithId socketWithId = readWritePool.FindByUID(uid);
            if (socketWithId == null)
                //说明用户已经断开  
                //100   发送成功
                //200   发送失败
                //300   用户不在线
                //其它  表示异常的信息
                OnSended(uid, "300");
            else
            {
                MySocketAsyncEventArgs e = socketWithId.SendSAEA;
                if (e.SocketError == SocketError.Success)
                {
                    int i = 0;
                    try
                    {
                        string message = @"[lenght=" + msg.Length + @"]" + msg;
                        byte[] sendbuffer = Encoding.Unicode.GetBytes(message);
                        e.SetBuffer(sendbuffer, 0, sendbuffer.Length);
                        Boolean willRaiseEvent = (e.UserToken as Socket).SendAsync(e);
                        if (!willRaiseEvent)
                        {
                            this.ProcessSend(e);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (i <= 5)
                        {
                            i++;
                            //如果发送出现异常就延迟0.01秒再发
                            Thread.Sleep(10);
                            Send(uid, msg);
                        }
                        else
                        {
                            OnSended(uid, ex.ToString());
                        }
                    }
                }
                else
                {
                    OnSended(uid, "200");
                    this.CloseClientSocket(((MySocketAsyncEventArgs)e).UID);
                }
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (listenSocket != null)
                listenSocket.Close();
            listenSocket = null;
            Dispose();
            mutex.Set();
            serverstate = ServerState.Stoped;
        }


        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
                acceptEventArg.AcceptSocket = null;
            this.semaphoreAcceptedClients.WaitOne();
            Boolean willRaiseEvent = this.listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                this.ProcessAccept(acceptEventArg);
            }
        }
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Accept)    //检查上一次操作是否是Accept，不是就返回
                return;
            //if (e.BytesTransferred <= 0)    //检查发送的长度是否大于0,不是就返回
            //    return;
            string UID = GetIDByIP((e.AcceptSocket.RemoteEndPoint as IPEndPoint).Address.ToString());   //根据IP获取用户的UID
            if (UID == string.Empty || UID == null || UID == "")
                return;
            if (readWritePool.BusyPoolContains(UID))    //判断现在的用户是否已经连接，避免同一用户开两个连接
                return;
            SocketAsyncEventArgsWithId readEventArgsWithId = this.readWritePool.Pop(UID);
            readEventArgsWithId.ReceiveSAEA.UserToken = e.AcceptSocket;
            readEventArgsWithId.SendSAEA.UserToken = e.AcceptSocket;
            Interlocked.Increment(ref this.numConnections);
            this.StartAccept(e);
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive)
                return;
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Int32 byteTransferred = e.BytesTransferred;
                    string received = Encoding.Unicode.GetString(e.Buffer, e.Offset, byteTransferred);
                    //检查消息的准确性
                    string[] msg = handler.GetActualString(received);
                    foreach (string m in msg)
                        OnMsgReceived(((MySocketAsyncEventArgs)e).UID, m);
                    //可以在这里设一个停顿来实现间隔时间段监听，这里的停顿是单个用户间的监听间隔
                    //发送一个异步接受请求，并获取请求是否为成功
                    Boolean willRaiseEvent = (e.UserToken as Socket).ReceiveAsync(e);
                    if (!willRaiseEvent)
                        ProcessReceive(e);
                }
            }
            else
                this.CloseClientSocket(((MySocketAsyncEventArgs)e).UID);
        }
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Send)
                return;
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                    OnSended(((MySocketAsyncEventArgs)e).UID, "100");
                else
                    OnSended(((MySocketAsyncEventArgs)e).UID, "200");
            }
            else
                this.CloseClientSocket(((MySocketAsyncEventArgs)e).UID);
        }

        private void CloseClientSocket(string uid)
        {
            if (uid == string.Empty || uid == "")
                return;
            SocketAsyncEventArgsWithId saeaw = readWritePool.FindByUID(uid);
            if (saeaw == null)
                return;
            Socket s = saeaw.ReceiveSAEA.UserToken as Socket;
            try
            {
                s.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                //客户端已经关闭
            }
            this.semaphoreAcceptedClients.Release();
            Interlocked.Decrement(ref this.numConnections);
            this.readWritePool.Push(saeaw);
        }

        #region IDisposable Members

        public void Dispose()
        {
            bufferManager.Dispose();
            bufferManager = null;
            readWritePool.Dispose();
            readWritePool = null;
        }

        #endregion
    }
    public enum ServerState { Initialing, Inited, Ready, Running, Stoped }
}
