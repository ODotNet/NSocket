using System;
using System.Net.Sockets;

namespace NSocket.SocketLib
{
    internal class ClientItem : IDisposable
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

        public string UID { get; set; }

        public ClientItem(string uid, Socket socket)
        {
            this.UID = uid;
            ReceiveSAEA = new NSocketSocketAsyncEventArgs("receive") { UID = uid, UserToken = socket };
            SendSAEA = new NSocketSocketAsyncEventArgs("send") { UID = uid, UserToken = socket };
        }

        #region IDisposable Members

        public void Dispose()
        {
            ReceiveSAEA.Dispose();
            SendSAEA.Dispose();
        }
        #endregion
    }
}
