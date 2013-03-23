using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace NSocket.SocketLib
{
    public class NSocketRebot
    {
        /// <summary>
        /// Rebot Name
        /// </summary>
        public string Name { get; set; }

        #region Rebot Status
        private NSocketRebotWorkStatus workStatus;
        /// <summary>
        /// Rebot Status
        /// </summary>
        public NSocketRebotWorkStatus WorkStatus
        {
            get { return workStatus; }
            set
            {
                if (workStatus != value)
                {
                    workStatus = value;
                }
            }
        }

        private NSocketRebotConnectStatus connectStatus;
        /// <summary>
        /// Rebot Status
        /// </summary>
        public NSocketRebotConnectStatus ConnectStatus
        {
            get { return connectStatus; }
            set
            {
                if (connectStatus != value)
                {
                    connectStatus = value;
                }
            }
        }
        #endregion

        #region Receive & Send statistics
        private int receivedLength = 0;
        public int ReceivedLength
        {
            get { return receivedLength; }
        }

        private int receivedTimes = 0;
        public int ReceivedTimes
        {
            get { return receivedTimes; }
        }

        private int sendLength = 0;
        public int SendLength
        {
            get { return sendLength; }
        }

        private int sendTimes = 0;
        public int SendTimes
        {
            get { return sendTimes; }
        }
        private int sendSuccessTimes = 0;
        public int SendSuccessTimes
        {
            get { return sendSuccessTimes; }
        }

        private int sendFailureTimes = 0;
        public int SendFailureTimes
        {
            get { return sendFailureTimes; }
        }

        private int tryConnectTimesSinceLastTime = 0;
        public int TryConnectTimes
        {
            get { return tryConnectTimesSinceLastTime; }
        }
        #endregion

        /// <summary>
        /// Socket Client
        /// </summary>
        SocketClient client;

        System.Diagnostics.Stopwatch PerformanceWatch;

        AutoResetEvent AutoSendEvent;

        public string SendMessage { get; set; }

        public NSocketRebot(IPAddress serverAddress, int port, int messageBuffer)
        {
            client = new SocketClient(serverAddress, port, messageBuffer);
            client.DateReceivedEvent += client_DateReceivedEvent;
            client.DataSendedEvent += client_DataSendedEvent;
            client.ServerEvent += client_ServerEvent;
            client.OnSended += client_OnSended;
            this.WorkStatus = NSocketRebotWorkStatus.Stop;
            this.PerformanceWatch = new System.Diagnostics.Stopwatch();
            this.AutoSendEvent = new AutoResetEvent(false);
        }

        void client_DataSendedEvent(byte[] data, int offSet, int length)
        {
            System.Threading.Interlocked.Increment(ref this.sendTimes);
            this.sendLength += length;
            Console.WriteLine("Receive:{0}", length);
        }

        void client_DateReceivedEvent(byte[] data, int offSet, int length)
        {
            PerformanceWatch.Stop();
            System.Threading.Interlocked.Increment(ref this.receivedTimes);
            this.DelayTime = PerformanceWatch.ElapsedMilliseconds;
            //AutoSendEvent.Set();
            Console.WriteLine("Relase one handle");
            this.receivedLength += length;
            Console.WriteLine("Send:{0}", length);
        }

        void client_OnSended(bool successorfalse)
        {
            if (successorfalse)
            {
                System.Threading.Interlocked.Increment(ref this.sendSuccessTimes);
            }
            else
            {
                System.Threading.Interlocked.Increment(ref this.sendFailureTimes);
            }
        }

        void client_ServerEvent(System.Net.Sockets.SocketError obj)
        {
            if (obj == System.Net.Sockets.SocketError.ConnectionReset
                || obj == System.Net.Sockets.SocketError.OperationAborted
                )
            {
                Console.WriteLine("Server Stoped");
                this.ConnectStatus = NSocketRebotConnectStatus.NotConnected;
            }
            else
            {
                Console.WriteLine(obj.ToString());
            }
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                this.WorkStatus = NSocketRebotWorkStatus.Running;
                AutoRun();//Start run
            });
        }

        private void TryConnect()
        {
            this.ConnectStatus = NSocketRebotConnectStatus.Connecting;
            System.Threading.Interlocked.Increment(ref this.tryConnectTimesSinceLastTime);
            if (client.Connect())
            {
                this.ConnectStatus = NSocketRebotConnectStatus.Connected;
                System.Threading.Interlocked.Exchange(ref this.tryConnectTimesSinceLastTime, 0);
            }
            else
            {
                this.ConnectStatus = NSocketRebotConnectStatus.ConnectingFailure;
            }
        }

        public void Stop()
        {
            this.WorkStatus = NSocketRebotWorkStatus.Stop;
            this.ConnectStatus = NSocketRebotConnectStatus.NotConnected;
            this.client.Disconnect();
        }

        /// <summary>
        /// 每隔1秒发送一条消息
        /// </summary>
        private void AutoRun()
        {
            while (true)
            {
                if (this.WorkStatus == NSocketRebotWorkStatus.Running)
                {
                    if (IsServerConnectRequired())
                    {
                        TryConnect();
                    }
                    else
                    {
                        Console.WriteLine("Get One handle");
                        this.PerformanceWatch.Restart();
                        var message = "HELLO WORLD";
                        message = String.Format("[length={0}]{1}", message.Length, message);
                        Byte[] sendBuffer = Encoding.Unicode.GetBytes(message);
                        this.client.Send(sendBuffer);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private bool IsServerConnectRequired()
        {
            return this.ConnectStatus == NSocketRebotConnectStatus.ConnectingFailure || this.ConnectStatus == NSocketRebotConnectStatus.NotConnected;
        }

        public long DelayTime { get; private set; }
    }

    public enum NSocketRebotWorkStatus
    {
        Stop,
        Running,
    }

    public enum NSocketRebotConnectStatus
    {
        ConnectingFailure,
        Connecting,
        Connected,
        NotConnected
    }
}
