using System.Net.Sockets;

namespace NSocket.SocketLib
{
    public class NSocketSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        internal string UID;
        private string Property;
        internal NSocketSocketAsyncEventArgs(string property)
        {
            this.Property = property;
        }

        public Socket Socket
        {
            get
            {
                return this.UserToken as Socket;
            }
            set
            {
                this.UserToken = value;
            }
        }
    }
}
