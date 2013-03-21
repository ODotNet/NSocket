using SocketLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace NSocket.ServerConsole
{
    class Program
    {
        static SocketListener listener;
        static void Main(string[] args)
        {
            listener = new SocketLib.SocketListener(5, 1024, IP);
            listener.OnMsgReceived += listener_OnMsgReceived;
            listener.OnSended += listener_OnSended;
            listener.StartListenThread += listener_StartListenThread;
            listener.Init();
            listener.Start(6754);
            //listener.Listen();

            Console.WriteLine("Press any key to exit....");
            Console.ReadKey();
        }

        static void listener_OnSended(string uid, string exception)
        {
            Console.WriteLine("Sended: {0} {1}", uid, exception);
        }

        static void listener_OnMsgReceived(string uid, string info)
        {
            Console.WriteLine("Received:{0} {1}", uid, info);
            listener.Send(uid, info);
        }

        static void listener_StartListenThread()
        {
            Console.WriteLine("Start Listenning...");
        }

        static string IP(string ip)
        {
            return ip;
        }
    }
}
