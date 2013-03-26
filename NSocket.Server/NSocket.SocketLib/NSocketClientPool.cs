using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NSocket.SocketLib
{
    public class NSocketClientPool : IDisposable
    {
        internal Queue<NSocketSAEAItem> idlePool;
        internal IDictionary<string, NSocketSAEAItem> busyPool;

        internal Int32 Count
        {
            get
            {
                //lock (this.idlePool)
                //{
                return this.idlePool.Count;
                //}
            }
        }
        internal string[] OnlineUID
        {
            get
            {
                var keys = new string[busyPool.Count];
                busyPool.Keys.CopyTo(keys, 0);
                return keys.Where(p => p != null).ToArray();
            }
        }

        internal NSocketClientPool(Int32 capacity)
        {
            this.idlePool = new Queue<NSocketSAEAItem>(capacity);
            this.busyPool = new Dictionary<string, NSocketSAEAItem>(capacity);
        }

        internal NSocketSAEAItem Pop(string uid, Socket socket)
        {
            if (uid == string.Empty || uid == "")
                return null;
            NSocketSAEAItem si = null;
            lock (this.idlePool)
            {
                si = this.idlePool.Dequeue();
                si.UseTimes++;
            }
            si.UID = uid;
            si.ReceiveSAEA.UID = uid;
            si.SendSAEA.UID = uid;
            si.ReceiveSAEA.Socket = socket;
            si.SendSAEA.Socket = socket;
            busyPool.Add(uid, si);
            return si;
        }
        internal void Push(NSocketSAEAItem item)
        {
            if (item == null)
                throw new ArgumentNullException("SocketAsyncEventArgsWithId对象为空");


            if (busyPool.Keys.Contains(item.UID))
            {
                lock (busyPool)
                {
                    busyPool.Remove(item.UID);
                }
            }

            item.UID = "-1";
            item.Disconnect();
            lock (this.idlePool)
            {
                this.idlePool.Enqueue(item);
            }
        }
        internal NSocketSAEAItem FindByUID(string uid)
        {
            return busyPool.ContainsKey(uid) ? busyPool[uid] : null;
        }
        internal bool BusyPoolContains(string uid)
        {
            lock (this.busyPool)
            {
                return busyPool.Keys.Contains(uid);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            idlePool.Clear();
            busyPool.Clear();
            idlePool = null;
            busyPool = null;
        }

        #endregion
    }
}
