using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NSocket.SocketLib
{
    public class SocketListener
    {
        private readonly int ReceiveBufferSize;
        private Socket listenSocket;

        public event Action<string> ClientConnected;
        public event Action<string> clientDisconnected;

        public string[] OnlineClients { get { return this.clientPool.OnlineUID; } }

        /// <summary>
        /// 接收到信息时的事件委托
        /// </summary>
        /// <param name="info"></param>
        public delegate void ReceiveMsgHandler(string uid, string info);

        /// <summary>
        /// 接收到信息时的事件
        /// </summary>
        public event ReceiveMsgHandler OnMsgReceived;
        private System.Threading.Timer WasteClientMonitor;
        private int numConcurrence;
        private NSocketClientPool clientPool;
        private Semaphore semaphoreAcceptedClients;

        /// <summary>
        /// 初始化服务器端
        /// </summary>
        public SocketListener(int receiveBufferSize, int numConcurrence)
        {
            this.ReceiveBufferSize = receiveBufferSize;
            //this.numConnections = 0;
            this.numConcurrence = numConcurrence;
            //this.bufferManager = new BufferManager(receiveBufferSize * numConcurrence * opsToPreAlloc, receiveBufferSize);
            this.clientPool = new NSocketClientPool(numConcurrence);
            this.semaphoreAcceptedClients = new Semaphore(numConcurrence, numConcurrence);
            //handler = new RequestHandler();
            //this.GetIDByIP = GetIDByIP;
        }

        /// <summary>
        /// 服务端初始化
        /// </summary>
        public void Init()
        {
            this.WasteClientMonitor = new System.Threading.Timer(WasteClientMonitorHandler, null, 1000 * 5, 1000 * 5);//Clean waste client period is 1 min.
            //this.bufferManager.InitBuffer();
            NSocketSAEAItem clientItem;
            for (Int32 i = 0; i < this.numConcurrence; i++)
            {
                clientItem = new NSocketSAEAItem();
                clientItem.ReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
                clientItem.SendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
                //只给接收的SocketAsyncEventArgs设置缓冲区
                //this.bufferManager.SetBuffer(readWriteEventArgWithId.ReceiveSAEA);
                this.clientPool.Push(clientItem);
            }
            //serverstate = ServerState.Inited;
        }

        private void WasteClientMonitorHandler(object state)
        {
            var currentExpriedTime = DateTime.Now.AddMilliseconds(-15 * 1000);

            var wasteClientList = this.clientPool.busyPool.Where(p => p.Value.LastMessageTime <= currentExpriedTime).Select(p => p.Key).ToList();//Old date < new date
            var wasteClientKeys = new string[wasteClientList.Count];
            wasteClientList.CopyTo(wasteClientKeys);
            foreach (var wasteClientKey in wasteClientKeys)
            {
                CloseClientConnection(wasteClientKey);
            }
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="data">端口号</param>
        public void Start(int port)
        {
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

            this.listenSocket.Listen(1);
            this.StartAccept(null);
        }

        #region Accept

        /// <summary>
        /// 接受客户端的连接请求
        /// </summary>
        /// <param name="acceptEventArg"></param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                byte[] acceptBuffer = new byte[this.ReceiveBufferSize];
                acceptEventArg.SetBuffer(acceptBuffer, 0, acceptBuffer.Length);
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
                acceptEventArg.AcceptSocket = null;
            this.semaphoreAcceptedClients.WaitOne();//控制服务器端总链接,但是即使达到链接的上限，Socket还是会接收Client Connect, 一旦有Client Disconnect, 之前排队的就会丢失.

            if (this.listenSocket != null)//Check listen socket status before next accept preparation
            {
                Boolean willRaiseEvent = this.listenSocket.AcceptAsync(acceptEventArg);
                Console.WriteLine("TID: #{0} prepare to accept next client", System.Threading.Thread.CurrentThread.ManagedThreadId);
                if (!willRaiseEvent)
                {
                    this.ProcessAccept(acceptEventArg);
                }
            }
        }

        /// <summary>
        /// 客户端连接请求处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("TID: #{0} accept a client", System.Threading.Thread.CurrentThread.ManagedThreadId);
            this.StartAccept(e);
            ThreadPool.QueueUserWorkItem((o) => { this.ProcessAccept(e); });
        }

        #endregion

        #region Receive
        /// <summary>
        /// 客户端连接请求处理方法
        /// </summary>
        /// <param name="e"></param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Accept)    //检查上一次操作是否是Accept，不是就返回
                return;
            if (e.BytesTransferred <= 0)    //检查发送的长度是否大于0,不是就返回
                return;

            string UID = Guid.NewGuid().ToString();//使用Guid作为客户端请求的ID, 不使用Client IP的原因是允许同一个IP建立多个连接.

            NSocketSAEAItem readEventArgsWithId = this.clientPool.Pop(UID, e.AcceptSocket);// new NSocketSAEAItem(UID, e.AcceptSocket);
            //readEventArgsWithId.ReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            //readEventArgsWithId.SendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            byte[] acceptBuffer = new byte[1024];
            readEventArgsWithId.ReceiveSAEA.SetBuffer(acceptBuffer, 0, acceptBuffer.Length);
            ClientConnected(UID);
            ReceiveListen(readEventArgsWithId.ReceiveSAEA);

        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("TID: #{0} receive a message", System.Threading.Thread.CurrentThread.ManagedThreadId);
            var SAEA = e as NSocket.SocketLib.NSocketSocketAsyncEventArgs;

            if (this.clientPool.busyPool.ContainsKey(SAEA.UID))
            {
                this.clientPool.busyPool[SAEA.UID].LastMessageTime = DateTime.Now;
            }
            else
            {
                Console.WriteLine("Couldn't found {0} in client lists", SAEA.UID);
            }

            if (e.LastOperation != SocketAsyncOperation.Receive)
                return;

            if (e.SocketError == SocketError.ConnectionReset)
            {
                CloseClientConnection(SAEA.UID);
                clientDisconnected(SAEA.UID);//Client Close.

            }
            else if (e.SocketError == SocketError.Disconnecting)
            {
                this.CloseClientConnection(SAEA.UID);
                OnMsgReceived(SAEA.UID, "Closed by client");
            }
            else if (e.SocketError == SocketError.Success)
            {
                ProcessReceive(SAEA);
                ReceiveListen(SAEA);
            }
            else if (e.SocketError == SocketError.OperationAborted || e.SocketError == SocketError.ConnectionAborted)
            {
                Console.WriteLine("Client Socket Aborted");
            }
            else
            {
                throw new Exception(e.SocketError.ToString());
            }
        }

        private void ReceiveListen(NSocketSocketAsyncEventArgs e)
        {
            var socket = e.Socket;
            if (socket.Connected)
            {
                Boolean willRaiseEvent = socket.ReceiveAsync(e);
                Console.WriteLine("TID: #{0} prepare to receive next", System.Threading.Thread.CurrentThread.ManagedThreadId);
                if (!willRaiseEvent)
                    ProcessReceive(e);
            }
        }

        private void ProcessReceive(NSocketSocketAsyncEventArgs e)
        {
            //OnMsgReceived(e.UID, e.SocketError.ToString());
            if (e.BytesTransferred > 0)
            {
                Int32 byteTransferred = e.BytesTransferred;
                string received = Encoding.Unicode.GetString(e.Buffer, e.Offset, byteTransferred);
                OnMsgReceived(e.UID, received);
                //检查消息的准确性
                //string[] msg = handler.GetActualString(received);
                //foreach (string m in msg)
                //    OnMsgReceived(e.UID, m);
                //可以在这里设一个停顿来实现间隔时间段监听，这里的停顿是单个用户间的监听间隔
                //发送一个异步接受请求，并获取请求是否为成功
            }
            else
            {
                Console.WriteLine(e.BytesTransferred);
            }
        }

        #endregion

        #region Send
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            //ThreadPool.QueueUserWorkItem((o) => { ProcessSend(e as NSocketSocketAsyncEventArgs); });
        }

        //private void ProcessSend(SocketAsyncEventArgs e)
        //{
        //    if (e.LastOperation != SocketAsyncOperation.Send)
        //        return;
        //}

        public void Send(string uid, string msg)
        {
            if (uid == string.Empty || uid == "" || msg == string.Empty || msg == "" || !this.clientPool.BusyPoolContains(uid))
                return;
            var socketWithId = this.clientPool.FindByUID(uid).SendSAEA;
            if (socketWithId == null)
            {
                //说明用户已经断开  
                //100   发送成功
                //200   发送失败
                //300   用户不在线
                //其它  表示异常的信息
                //OnSended(uid, "300");
                OnMsgReceived(uid, "is offline");
            }
            else
            {
                var e = socketWithId;
                if (e.SocketError == SocketError.Success)
                {
                    int i = 0;
                    try
                    {
                        string message = @"[lenght=" + msg.Length + @"]" + msg;
                        byte[] sendbuffer = Encoding.Unicode.GetBytes(message);
                        e.SetBuffer(sendbuffer, 0, sendbuffer.Length);
                        Boolean willRaiseEvent = e.Socket.SendAsync(e);

                        if (!willRaiseEvent)
                        {
                            Console.WriteLine("Send failure, resend");
                            //this.ProcessSend(e);
                        }
                        else
                        {
                            Console.WriteLine("TID: #{0} sended a message", System.Threading.Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (i <= 5)
                        {
                            i++;
                            //如果发送出现异常就延迟0.01秒再发
                            //Thread.Sleep(10);
                            //Send(uid, msg);
                        }
                        else
                        {
                            //OnSended(uid, ex.ToString());
                        }
                    }
                }
                else
                {
                    //OnSended(uid, "200");
                    this.CloseClientConnection(uid);
                }
            }
        }

        #endregion

        #region Stop Listener

        private void CloseClientConnection(string uid)
        {
            if (uid == string.Empty || uid == "")
                return;
            var client = this.clientPool.FindByUID(uid);
            if (client == null)
                return;
            try
            {
                this.clientPool.Push(client);
                this.semaphoreAcceptedClients.Release();
            }
            catch (Exception ex)
            {
                //客户端已经关闭
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            string[] clientKeys = new string[this.clientPool.OnlineUID.Length];
            this.OnlineClients.CopyTo(clientKeys, 0);

            foreach (var clientKey in clientKeys)
            {
                CloseClientConnection(clientKey);
            }

            if (listenSocket != null)
            {
                listenSocket.Close();
                if (listenSocket.Connected)
                    listenSocket.Disconnect(false);
                listenSocket.Dispose();

                listenSocket = null;
            }
        }

        #endregion
    }
}
