using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NSocket.SocketLib
{
    public class SocketListener
    {
        private readonly int ReceiveBufferSize;
        private Socket listenSocket;

        public event Action<string> ClientAccepted;

        private Dictionary<string, ClientItem> Clients;

        public List<string> OnlineClients { get { return Clients.Keys.ToList(); } }

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
        /// 初始化服务器端
        /// </summary>
        public SocketListener(Int32 receiveBufferSize)
        {
            this.ReceiveBufferSize = receiveBufferSize;
            //this.numConnections = 0;
            //this.numConcurrence = numConcurrence;
            //this.bufferManager = new BufferManager(receiveBufferSize * numConcurrence * opsToPreAlloc, receiveBufferSize);
            //this.readWritePool = new SocketAsyncEventArgsPool(numConcurrence);
            //this.semaphoreAcceptedClients = new Semaphore(numConcurrence, numConcurrence);
            //handler = new RequestHandler();
            //this.GetIDByIP = GetIDByIP;
        }

        /// <summary>
        /// 服务端初始化
        /// </summary>
        public void Init()
        {
            Clients = new Dictionary<string, ClientItem>();
            //this.bufferManager.InitBuffer();
            //ClientItem readWriteEventArgWithId;
            //for (Int32 i = 0; i < this.numConcurrence; i++)
            //{
            //    readWriteEventArgWithId = new ClientItem();
            //    readWriteEventArgWithId.ReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            //    readWriteEventArgWithId.SendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            //    //只给接收的SocketAsyncEventArgs设置缓冲区
            //    this.bufferManager.SetBuffer(readWriteEventArgWithId.ReceiveSAEA);
            //    this.readWritePool.Push(readWriteEventArgWithId);
            //}
            //serverstate = ServerState.Inited;
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
            this.listenSocket.Listen(100);
            this.StartAccept(null);

            //listenCTS = new CancellationTokenSource();
            ////listenThread = new Thread((o) => { Listen(); });
            ////listenThread.Start();
            //ThreadPool.QueueUserWorkItem((o) => { Listen(); });
            ////开始监听已连接用户的发送数据
            //StartListenThread();
            //serverstate = ServerState.Running;
            //listenerStopEvent.WaitOne();
        }

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
            //this.semaphoreAcceptedClients.WaitOne();//控制服务器端总链接,但是即使达到链接的上限，Socket还是会接收Client Connect, 一旦有Client Disconnect, 之前排队的就会丢失.
            Boolean willRaiseEvent = this.listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                this.ProcessAccept(acceptEventArg);
            }
        }

        /// <summary>
        /// 客户端连接请求处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

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

            ClientItem readEventArgsWithId = new ClientItem(UID, e.AcceptSocket);
            readEventArgsWithId.ReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            readEventArgsWithId.SendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            byte[] acceptBuffer = new byte[1024];
            readEventArgsWithId.ReceiveSAEA.SetBuffer(acceptBuffer, 0, acceptBuffer.Length);

            Boolean willRaiseEvent = (readEventArgsWithId.ReceiveSAEA.UserToken as Socket).ReceiveAsync(readEventArgsWithId.ReceiveSAEA);
            if (!willRaiseEvent)
                ProcessReceive(readEventArgsWithId.ReceiveSAEA);

            Clients.Add(UID, readEventArgsWithId);
            ClientAccepted(UID);
            this.StartAccept(e);
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e as NSocket.SocketLib.NSocketSocketAsyncEventArgs);
        }

        private void ProcessReceive(NSocketSocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive)
                return;

            if (e.SocketError == SocketError.ConnectionReset)
            {
                this.CloseClientSocket(e.UID);
                this.Clients.Remove(e.UID);
                OnMsgReceived(e.UID, "Closed by client");
            }
            else if (e.SocketError == SocketError.Success)
            {
                //OnMsgReceived(e.UID, e.SocketError.ToString());
                if (e.BytesTransferred > 0)
                {
                    if (e.SocketError == SocketError.Success)
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
                        Boolean willRaiseEvent = (e.UserToken as Socket).ReceiveAsync(e);
                        if (!willRaiseEvent)
                            ProcessReceive(e);
                    }
                }
            }
            else if (e.SocketError == SocketError.OperationAborted)
            {
                Console.WriteLine("Client Socket Aborted");
            }
            else
            {
                throw new Exception(e.SocketError.ToString());
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e as NSocketSocketAsyncEventArgs);
        }

        public void Send(string uid, string msg)
        {
            if (uid == string.Empty || uid == "" || msg == string.Empty || msg == "" || !this.Clients.ContainsKey(uid))
                return;
            SocketAsyncEventArgs socketWithId = this.Clients[uid].SendSAEA;
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
                SocketAsyncEventArgs e = socketWithId;
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
                    this.CloseClientSocket(uid);
                    this.Clients.Remove(uid);
                }
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Send)
                return;
        }

        private void CloseClientSocket(string uid)
        {
            if (uid == string.Empty || uid == "")
                return;
            SocketAsyncEventArgs saeaw = this.Clients[uid].ReceiveSAEA;
            if (saeaw == null)
                return;
            Socket s = saeaw.UserToken as Socket;
            //此处为何只关闭Receive, 而不关闭Send?
            try
            {
                s.Close();//Tell client(Client will received a ConnectionReset error);
                if (s.Connected) //If raised by server, need to discount client socket first.
                    s.Disconnect(false);

                s.Shutdown(SocketShutdown.Both);
                s.Dispose();
                saeaw.Dispose();//SAEA销毁
            }
            catch (Exception)
            {
                //客户端已经关闭
            }
            //this.semaphoreAcceptedClients.Release();
            //Interlocked.Decrement(ref this.numConnections);
            //this.readWritePool.Push(saeaw);

            //OnMsgReceived(uid, " discounnect");

        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            foreach (var client in this.Clients.Values)
            {

                //(client.ReceiveSAEA.UserToken as Socket).Disconnect(false);
                //client.Dispose();
                CloseClientSocket(client.UID);
            }

            if (listenSocket != null)
            {
                //listenSocket.Disconnect(false);
                listenSocket.Close();
                //listenSocket.Shutdown(SocketShutdown.Both);

                listenSocket = null;
            }
        }
    }
}
