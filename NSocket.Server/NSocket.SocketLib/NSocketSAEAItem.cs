using System;
using System.Net.Sockets;

namespace NSocket.SocketLib
{
    internal class NSocketSAEAItem : IDisposable
    {
        private NSocketSocketAsyncEventArgs receivesaea;
        private NSocketSocketAsyncEventArgs sendsaea;

        internal NSocketSocketAsyncEventArgs ReceiveSAEA
        {
            set { receivesaea = value; }
            get { return receivesaea; }
        }
        internal NSocketSocketAsyncEventArgs SendSAEA
        {
            set { sendsaea = value; }
            get { return sendsaea; }
        }

        public int UseTimes { get; set; }

        public DateTime LastMessageTime { get; set; }

        public string UID { get; set; }

        public NSocketSAEAItem(string uid = "", Socket socket = null)
        {
            this.UID = uid;
            ReceiveSAEA = new NSocketSocketAsyncEventArgs("receive") { UID = uid, UserToken = socket };
            SendSAEA = new NSocketSocketAsyncEventArgs("send") { UID = uid, UserToken = socket };
            this.LastMessageTime = DateTime.Now;
        }

        #region IDisposable Members

        /// <summary>
        /// Send disconnect message to client.
        /// </summary>
        public void Disconnect()
        {
            var s = ReceiveSAEA.Socket;
            if (s != null)
            {
                s.Shutdown(SocketShutdown.Receive);//Why only shutdown receive and send not.
                s.Close();//Tell client(Client will received a ConnectionReset error);
            }
        }

        public void Dispose()
        {
            Disconnect();
            ReceiveSAEA.Dispose();
            SendSAEA.Dispose();
        }
        #endregion
    }
}
