using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLib
{
    public class SocketClient : IDisposable
    {
        /// <summary>
        /// 客户端连接Socket
        /// </summary>
        private Socket clientSocket;
        /// <summary>
        /// 连接状态
        /// </summary>
        private Boolean connected = false;
        /// <summary>
        /// 连接点
        /// </summary>
        private IPEndPoint hostEndPoint;
        /// <summary>
        /// 连接信号量
        /// </summary>
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false);
        /// <summary>
        /// 接受到数据时的委托
        /// </summary>
        /// <param name="info"></param>
        public delegate void ReceiveMsgHandler(string info);
        /// <summary>
        /// 接收到数据时调用的事件
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
        /// 发送信息完成的委托
        /// </summary>
        /// <param name="successorfalse"></param>
        public delegate void SendCompleted(bool successorfalse);
        /// <summary>
        /// 发送信息完成的事件
        /// </summary>
        public event SendCompleted OnSended;
        /// <summary>
        /// 监听接收的SocketAsyncEventArgs
        /// </summary>
        private SocketAsyncEventArgs listenerSocketAsyncEventArgs;

        /// <summary>
        /// 初始化客户端
        /// </summary>
        /// <param name="hostName">服务端地址{IP地址}</param>
        /// <param name="port">端口号</param>
        public SocketClient(String hostName, Int32 port)
        {
            IPHostEntry host = Dns.GetHostEntry(hostName);
            IPAddress[] addressList = host.AddressList;
            this.hostEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public SocketClient(IPAddress address, Int32 port)
        {
            this.hostEndPoint = new IPEndPoint(address, port);
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 连接服务端
        /// </summary>
        public bool Connect()
        {
            SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();

            connectArgs.UserToken = this.clientSocket;
            connectArgs.RemoteEndPoint = this.hostEndPoint;
            byte[] connectBuffer = new byte[1024];
            connectArgs.SetBuffer(connectBuffer, 0, connectBuffer.Length);
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
            clientSocket.ConnectAsync(connectArgs);
            //等待连接结果
            autoConnectEvent.WaitOne();
            SocketError errorCode = connectArgs.SocketError;
            if (errorCode == SocketError.Success)
            {
                listenerSocketAsyncEventArgs = new SocketAsyncEventArgs();
                byte[] receiveBuffer = new byte[1024];
                listenerSocketAsyncEventArgs.UserToken = clientSocket;
                listenerSocketAsyncEventArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                listenerSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);

                if (null != StartListenThread)
                    StartListenThread();
                return true;
            }
            else
                //throw new SocketException((Int32)errorCode);
                return false;
        }

        /// <summary>
        /// 开始监听线程的入口函数
        /// </summary>
        public void Listen()
        {
            (listenerSocketAsyncEventArgs.UserToken as Socket).ReceiveAsync(listenerSocketAsyncEventArgs);
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="message"></param>
        public void Send(String message)
        {
            if (this.connected)
            {
                message = String.Format("[length={0}]{1}", message.Length, message);
                Byte[] sendBuffer = Encoding.Unicode.GetBytes(message);
                SocketAsyncEventArgs senderSocketAsyncEventArgs = new SocketAsyncEventArgs();
                senderSocketAsyncEventArgs.UserToken = this.clientSocket;
                senderSocketAsyncEventArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                senderSocketAsyncEventArgs.RemoteEndPoint = this.hostEndPoint;
                senderSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);
                clientSocket.SendAsync(senderSocketAsyncEventArgs);
            }
            else
            {
                throw new SocketException((Int32)SocketError.NotConnected);
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            this.connected = false;
            clientSocket.Close();
            clientSocket.Dispose();
            //clientSocket.DisconnectAsync(new SocketAsyncEventArgs());
            //clientSocket.Dispose();

            //clientSocket.Close();
            //clientSocket.Disconnect(false);
            //clientSocket.Dispose();
        }


        /// <summary>
        /// 连接的完成方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoConnectEvent.Set();
            this.connected = (e.SocketError == SocketError.Success);
        }
        /// <summary>
        /// 接收的完成方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            //OnMsgReceived(string.Format("Server COMMAND: {0}", e.SocketError.ToString()));
            switch (e.SocketError)
            {
                case SocketError.AccessDenied:
                    break;
                case SocketError.AddressAlreadyInUse:
                    break;
                case SocketError.AddressFamilyNotSupported:
                    break;
                case SocketError.AddressNotAvailable:
                    break;
                case SocketError.AlreadyInProgress:
                    break;
                case SocketError.ConnectionAborted:
                    Console.WriteLine("Connection Aborted");
                    break;
                case SocketError.ConnectionRefused:
                    break;
                case SocketError.ConnectionReset:
                    Console.WriteLine("Server Side Reset");
                    break;
                case SocketError.DestinationAddressRequired:
                    break;
                case SocketError.Disconnecting:
                    break;
                case SocketError.Fault:
                    break;
                case SocketError.HostDown:
                    break;
                case SocketError.HostNotFound:
                    break;
                case SocketError.HostUnreachable:
                    break;
                case SocketError.IOPending:
                    break;
                case SocketError.InProgress:
                    break;
                case SocketError.Interrupted:
                    break;
                case SocketError.InvalidArgument:
                    break;
                case SocketError.IsConnected:
                    break;
                case SocketError.MessageSize:
                    break;
                case SocketError.NetworkDown:
                    break;
                case SocketError.NetworkReset:
                    break;
                case SocketError.NetworkUnreachable:
                    break;
                case SocketError.NoBufferSpaceAvailable:
                    break;
                case SocketError.NoData:
                    break;
                case SocketError.NoRecovery:
                    break;
                case SocketError.NotConnected:
                    break;
                case SocketError.NotInitialized:
                    break;
                case SocketError.NotSocket:
                    break;
                case SocketError.OperationAborted:
                    break;
                case SocketError.OperationNotSupported:
                    break;
                case SocketError.ProcessLimit:
                    break;
                case SocketError.ProtocolFamilyNotSupported:
                    break;
                case SocketError.ProtocolNotSupported:
                    break;
                case SocketError.ProtocolOption:
                    break;
                case SocketError.ProtocolType:
                    break;
                case SocketError.Shutdown:
                    break;
                case SocketError.SocketError:
                    break;
                case SocketError.SocketNotSupported:
                    break;
                case SocketError.Success:
                    string msg = Encoding.Unicode.GetString(e.Buffer, 0, e.BytesTransferred);
                    Listen();
                    OnMsgReceived(msg);
                    break;
                case SocketError.SystemNotReady:
                    break;
                case SocketError.TimedOut:
                    break;
                case SocketError.TooManyOpenSockets:
                    break;
                case SocketError.TryAgain:
                    break;
                case SocketError.TypeNotFound:
                    break;
                case SocketError.VersionNotSupported:
                    break;
                case SocketError.WouldBlock:
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// 发送的完成方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                OnSended(true);
            }
            else
            {
                OnSended(false);
                this.ProcessError(e);
            }
        }
        /// <summary>
        /// 处理错误
        /// </summary>
        /// <param name="e"></param>
        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    //client already closed
                }
                finally
                {
                    if (s.Connected)
                    {
                        s.Close();
                    }
                }
            }
            throw new SocketException((Int32)e.SocketError);
        }

        #region IDisposable Members
        public void Dispose()
        {
            autoConnectEvent.Close();
            if (this.clientSocket.Connected)
            {
                this.clientSocket.Close();
            }
        }
        #endregion
    }
}