using NSocket.SocketLib;
using System;
using System.Threading;
using System.Windows.Forms;

namespace NSocket.ServerMonitor
{
    public partial class MainForm : Form
    {
        SocketListener listener;
        System.Threading.Timer listenerMonitorTimer;
        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            this.btnStop.Enabled = false;
            this.tbPort.Text = "7890";
            this.listenerMonitorTimer = new System.Threading.Timer(ListenerMonitorHandler, null, 0, 500);
        }

        private void ListenerMonitorHandler(object o)
        {
            if (listener != null)
            {
                Action action = () =>
                {
                    this.tbOnlineUsers.Text = this.listener.OnlineClients.Length.ToString();
                };
                UIThreadInvoke(action);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int port = -1;
            if (int.TryParse(this.tbPort.Text, out port) && port > 0)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    listener = new SocketListener(1024, 100);
                    listener.OnMsgReceived += listener_OnMsgReceived;
                    //listener.OnSended += listener_OnSended;
                    //listener.StartListenThread += listener_StartListenThread;
                    listener.ClientConnected += listener_ClientAccepted;
                    listener.clientDisconnected += listener_clientDisconnected;
                    listener.Init();
                    listener.Start(port);
                });
            }
            else
            {
                MessageBox.Show("Wrong tcp port");
            }
        }

        void listener_clientDisconnected(string obj)
        {

        }

        void listener_ClientAccepted(string uid)
        {
            OutputLog(string.Format("Accepted: {0}", uid));
            listener.Send(uid, uid);
        }

        void listener_OnSended(string uid, string exception)
        {
            OutputLog(string.Format("Sended: {0} {1}", uid, exception));
        }

        void listener_OnMsgReceived(string uid, string info)
        {
            //OutputLog(string.Format("Received:{0} {1}", uid, info));
            listener.Send(uid, info);
        }

        void listener_StartListenThread()
        {
            OutputLog("Start Listenning...");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (listener != null)
            {
                listener.Stop();
                this.btnStop.Enabled = false;
                this.btnStart.Enabled = true;
            }
        }

        void OutputLog(string format, params object[] objs)
        {
            Action outputAction = () =>
            {
                txtLog.AppendText(string.Format(format, objs));
                txtLog.AppendText(Environment.NewLine);
            };

            UIThreadInvoke(outputAction);
        }

        void UIThreadInvoke(Action action)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { action(); });
            }
            else
            {
                action();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            listener.Stop();
        }
    }
}
