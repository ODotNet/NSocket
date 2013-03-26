using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SocketLib
{
    public sealed partial class SocketListener
    {
        /// <summary>
        /// 开始监听线程的入口函数
        /// </summary>
        private void Listen()
        {
            //此处用loop而不使用thread是为了减少线程切换带来的开销，
            //但是如果在线数量达到一定程度，必然会导致处理延迟.
            while (true)
            {
                if (listenCTS.IsCancellationRequested)
                {
                    //Cancel listen thread.
                    return;
                }
                string[] keys = readWritePool.OnlineUID;
                SocketAsyncEventArgsWithId tempPoolItem = null;
                foreach (string uid in keys)
                {
                    if (uid != null && (tempPoolItem = readWritePool.busypool[uid]) != null)
                    {
                        if (tempPoolItem.ReceiveSAEA.LastOperation != SocketAsyncOperation.Receive)
                        {
                            Boolean willRaiseEvent = (tempPoolItem.ReceiveSAEA.UserToken as Socket).ReceiveAsync(tempPoolItem.ReceiveSAEA);
                            if (!willRaiseEvent)
                                ProcessReceive(tempPoolItem.ReceiveSAEA);
                        }
                    }
                }
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e as MySocketAsyncEventArgs);
        }

        private void ProcessReceive(MySocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive)
                return;
            OnMsgReceived(e.UID, e.SocketError.ToString());
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Int32 byteTransferred = e.BytesTransferred;
                    string received = Encoding.Unicode.GetString(e.Buffer, e.Offset, byteTransferred);
                    //检查消息的准确性
                    string[] msg = handler.GetActualString(received);
                    foreach (string m in msg)
                        OnMsgReceived(e.UID, m);
                    //可以在这里设一个停顿来实现间隔时间段监听，这里的停顿是单个用户间的监听间隔
                    //发送一个异步接受请求，并获取请求是否为成功
                    Boolean willRaiseEvent = (e.UserToken as Socket).ReceiveAsync(e);
                    if (!willRaiseEvent)
                        ProcessReceive(e);
                }
            }
            else
            {
                this.CloseClientSocket(e.UID);
            }
        }
    }
}
