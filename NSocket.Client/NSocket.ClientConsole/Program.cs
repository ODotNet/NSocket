using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NSocket.ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress[] addressList = host.AddressList;
            SocketLib.SocketClient client = new SocketLib.SocketClient(addressList[addressList.Length - 1], 6754);
            client.StartListenThread += client_StartListenThread;
            client.OnMsgReceived += client_OnMsgReceived;
            client.OnSended += client_OnSended;
        RETRY:
            if (client.Connect())
            {
                client.Listen();
                string cmd = string.Empty;
                while ((cmd = Console.ReadLine().Trim()) != "Q")
                {
                    client.Send(cmd);
                }
            }
            else
            {
                Console.WriteLine("Server Connection Failure");
                Console.ReadKey();
                goto RETRY;
            }
        }

        static void client_OnSended(bool successorfalse)
        {
            Console.WriteLine(successorfalse);
        }

        static void client_OnMsgReceived(string info)
        {
            Console.WriteLine(info);
        }

        static void client_StartListenThread()
        {
            Console.WriteLine("Client start listenning...");
        }
    }
}
