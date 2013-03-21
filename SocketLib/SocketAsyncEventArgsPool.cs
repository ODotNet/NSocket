using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SocketLib
{
    internal sealed class SocketAsyncEventArgsPool:IDisposable
    {
        internal Stack<SocketAsyncEventArgsWithId> pool;
        internal IDictionary<string, SocketAsyncEventArgsWithId> busypool;
        private string[] keys;
        
        internal Int32 Count
        {
            get
            {
                lock (this.pool)
                {
                    return this.pool.Count;
                }
            }
        }
        internal string[] OnlineUID
        {
            get
            {
                lock (this.busypool)
                {
                    busypool.Keys.CopyTo(keys, 0);
                }
                return keys;
            }
        }

        internal SocketAsyncEventArgsPool(Int32 capacity)
        {
            keys = new string[capacity];
            this.pool = new Stack<SocketAsyncEventArgsWithId>(capacity);
            this.busypool = new Dictionary<string, SocketAsyncEventArgsWithId>(capacity);
        }

        internal SocketAsyncEventArgsWithId Pop(string uid)
        {
            if (uid == string.Empty || uid == "")
                return null;
            SocketAsyncEventArgsWithId si = null;
            lock (this.pool)
            {
                si = this.pool.Pop();
            }
            si.UID = uid;
            si.State = true;    //mark the state of pool is not the initial step
            busypool.Add(uid, si);
            return si;
        }
        internal void Push(SocketAsyncEventArgsWithId item)
        {
            if (item == null)
                throw new ArgumentNullException("SocketAsyncEventArgsWithId对象为空");
            if (item.State == true)
            {
                if (busypool.Keys.Count != 0)
                {
                    if (busypool.Keys.Contains(item.UID))
                        busypool.Remove(item.UID);
                    else
                        throw new ArgumentException("SocketAsyncEventWithId不在忙碌队列中");
                }
                else
                    throw new ArgumentException("忙碌队列为空");
            }
            item.UID = "-1";
            item.State = false;
            lock (this.pool)
            {
                this.pool.Push(item);
            }
        }
        internal SocketAsyncEventArgsWithId FindByUID(string uid)
        {
            if (uid == string.Empty || uid == "")
                return null;
            SocketAsyncEventArgsWithId si = null;
            foreach (string key in this.OnlineUID)
            {
                if (key == uid)
                {
                    si = busypool[uid];
                    break;
                }
            }
            return si;
        }
        internal bool BusyPoolContains(string uid)
        {
            lock (this.busypool)
            {
                return busypool.Keys.Contains(uid);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            pool.Clear();
            busypool.Clear();
            pool = null;
            busypool = null;
        }

        #endregion
    }
    internal sealed class SocketAsyncEventArgsWithId:IDisposable
    {
        private string uid = "-1";
        private bool state = false;
        private MySocketAsyncEventArgs receivesaea;
        private MySocketAsyncEventArgs sendsaea;
        internal string UID
        {
            get { return uid; }
            set
            {
                uid = value;
                ReceiveSAEA.UID = value;
                SendSAEA.UID = value;
            }
        }
        internal bool State 
        {
            get { return state; }
            set { this.state = value; }
        }
        internal MySocketAsyncEventArgs ReceiveSAEA
        {
            set { receivesaea = value; }
            get { return receivesaea; }
        }
        internal MySocketAsyncEventArgs SendSAEA
        {
            set { sendsaea = value; }
            get { return sendsaea; }
        }

        //constructor
        internal SocketAsyncEventArgsWithId()
        {
            ReceiveSAEA = new MySocketAsyncEventArgs("Receive");
            SendSAEA = new MySocketAsyncEventArgs("Send");
        }

        #region IDisposable Members

        public void Dispose()
        {
            ReceiveSAEA.Dispose();
            SendSAEA.Dispose();
        }

        #endregion
    }
    internal sealed class MySocketAsyncEventArgs : SocketAsyncEventArgs{
        internal string UID;
        private string Property;
        internal MySocketAsyncEventArgs(string property){
            this.Property = property;
        }
    }
}
