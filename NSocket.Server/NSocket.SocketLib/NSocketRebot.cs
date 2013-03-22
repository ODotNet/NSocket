using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

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

        /// <summary>
        /// Socket Client
        /// </summary>
        SocketClient client;

        public string SendMessage { get; set; }

        private System.Threading.Timer AutoRunTimer;

        public NSocketRebot(IPAddress serverAddress, int port, int messageBuffer)
        {
            client = new SocketClient(serverAddress, port, messageBuffer);
            client.OnMsgReceived += client_OnMsgReceived;
            client.ServerEvent += client_ServerEvent;
            client.OnSended += client_OnSended;
            this.Status = NSocketRebotStatus.Stop;
        }

        void client_OnSended(bool successorfalse)
        {

        }

        void client_ServerEvent(System.Net.Sockets.SocketError obj)
        {
            this.Status = NSocketRebotStatus.Error;
            if (obj == System.Net.Sockets.SocketError.ConnectionReset)
            {
                Console.WriteLine("Server Stoped");
            }
            else
            {
                Console.WriteLine(obj.ToString());
            }
        }

        void client_OnMsgReceived(string message)
        {
            Console.WriteLine("{0}:{1}", this.Name, message);
        }

        public void Start()
        {
            if (client.Connect())
            {
                this.Status = NSocketRebotStatus.Running;
                client.Listen();//Start listen server's response.
                AutoRun();//Start run
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
            AutoRunTimer.Change(0, 0);
        }

        /// <summary>
        /// 每隔1秒发送一条消息
        /// </summary>
        private void AutoRun()
        {
            AutoRunTimer = new System.Threading.Timer((o) =>
              {
                  if (this.Status == NSocketRebotStatus.Running)
                      this.client.Send(SendMessage);
              }, null, 0, 200);
        }
    }

    public enum NSocketRebotStatus
    {
        Stop,
        Running,
        Error
    }
}
