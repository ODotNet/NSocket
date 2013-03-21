using SocketLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NSocket.ServerMonitor
{
    public partial class MainForm : Form
    {
        SocketListener listener;
        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            this.btnStop.Enabled = false;
            this.tbPort.Text = "7890";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int port = -1;
            if (int.TryParse(this.tbPort.Text, out port) && port > 0)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;

                listener = new SocketLib.SocketListener(3, 32768, (p) => { return p; });
                listener.OnMsgReceived += listener_OnMsgReceived;
                listener.OnSended += listener_OnSended;
                listener.StartListenThread += listener_StartListenThread;
                listener.ClientAccepted += listener_ClientAccepted;
                listener.Init();
                listener.Start(6754);
                OutputLog(string.Format("Start Listenning..."));
            }
            else
            {
                MessageBox.Show("Wrong tcp port");
            }
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
            OutputLog(string.Format("Received:{0} {1}", uid, info));
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

            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                  {
                      outputAction();
                  });
            }
            else
            {
                outputAction();
            }
        }
    }
}
