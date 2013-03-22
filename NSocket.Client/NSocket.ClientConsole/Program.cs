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

            for (int i = 0; i < 100; i++)
            {
                NSocket.SocketLib.NSocketRebot rebot = new SocketLib.NSocketRebot(addressList[addressList.Length - 1], 7890, 1024);
                rebot.Name = "#" + i.ToString();
                rebot.SendMessage = "HELLO WORLD";
                rebot.Start();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
