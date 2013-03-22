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

        private NSocketRebotStatus status;
        /// <summary>
        /// Rebot Status
        /// </summary>
        public NSocketRebotStatus Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    Console.WriteLine(Name + "Status Change to " + status.ToString());
                }
            }
        }

        private int receivedLength = 0;
        public int ReceivedLength
        {
            get { return receivedLength; }
        }

        private int sendLength = 0;
        public int SendLength
        {
            get { return sendLength; }
        }

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
            this.Status = NSocketRebotStatus.Stop;
            this.PerformanceWatch = new System.Diagnostics.Stopwatch();
            this.AutoSendEvent = new AutoResetEvent(false);
        }

        void client_DataSendedEvent(byte[] data, int offSet, int length)
        {
            this.sendLength += length;
            Console.WriteLine("Receive:{0}", length);
        }

        void client_DateReceivedEvent(byte[] data, int offSet, int length)
        {
            PerformanceWatch.Stop();
            this.DelayTime = PerformanceWatch.ElapsedMilliseconds;
            //AutoSendEvent.Set();
            Console.WriteLine("Relase one handle");
            this.receivedLength += length;
            Console.WriteLine("Send:{0}", length);
        }

        void client_OnSended(bool successorfalse)
        {

        }

        void client_ServerEvent(System.Net.Sockets.SocketError obj)
        {
            this.Status = NSocketRebotStatus.Error;
            if (obj == System.Net.Sockets.SocketError.ConnectionReset
                || obj == System.Net.Sockets.SocketError.OperationAborted
                )
            {
                Console.WriteLine("Server Stoped");
                this.Status = NSocketRebotStatus.Stop;
            }
            else
            {
                Console.WriteLine(obj.ToString());
            }
        }

        public void Start()
        {
            if (client.Connect())
            {
                this.Status = NSocketRebotStatus.Running;
                client.Listen();//Start listen server's response.
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    AutoRun();//Start run
                });
            }
            else
            {
                this.Status = NSocketRebotStatus.Error;
            }
        }

        public void Stop()
        {
            this.Status = NSocketRebotStatus.Stop;
            this.client.Disconnect();
        }

        /// <summary>
        /// 每隔1秒发送一条消息
        /// </summary>
        private void AutoRun()
        {
            while (true)
            {
                Console.WriteLine("Start Wait...");
                if (this.Status == NSocketRebotStatus.Running)
                {
                    Console.WriteLine("Get One handle");
                    this.PerformanceWatch.Restart();
                    var message = "HELLO WORLD";
                    message = String.Format("[length={0}]{1}", message.Length, message);
                    Byte[] sendBuffer = Encoding.Unicode.GetBytes(message);
                    this.client.Send(sendBuffer);
                }
                else if (this.Status == NSocketRebotStatus.Stop)
                {
                    return;
                }
                else
                {
                    //AutoSendEvent.Set();
                }
            }
        }

        public long DelayTime { get; private set; }
    }

    public enum NSocketRebotStatus
    {
        Stop,
        Running,
        Error
    }
}
