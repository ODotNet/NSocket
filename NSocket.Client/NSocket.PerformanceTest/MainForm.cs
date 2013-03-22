using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NSocket.PerformanceTest
{
    public partial class MainForm : Form
    {
        SocketLib.SocketClient client;
        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            this.btnStop.Enabled = false;
            this.tbServerIP.Text = "127.0.0.1";
            this.tbPort.Text = "7890";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.btnStart.Enabled = false;
            int port = -1;
            if (int.TryParse(this.tbPort.Text, out port) && port <= 0)
            {
                MessageBox.Show("Wrong tcp port");
            }
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Network UnAvailable");
            }

            IPHostEntry host = Dns.GetHostEntry(tbServerIP.Text);
            IPAddress[] addressList = host.AddressList;

            if (addressList.Length < 1)
            {
                MessageBox.Show("Didn't found any server");
            }

            client = new SocketLib.SocketClient(addressList[addressList.Length - 1], port, 1024);
            client.StartListenThread += client_StartListenThread;
            client.OnMsgReceived += client_OnMsgReceived;
            client.OnSended += client_OnSended;
            OuputLog(string.Format("Start Connect {0}:{1}", addressList[addressList.Length - 1], port));
            if (client.Connect())
            {
                client.Listen();
                this.btnStop.Enabled = true; //Enable stop button that can stop client.
            }
            else
            {
                OuputLog("Server Connection Failure");
                this.btnStart.Enabled = true;//Connect Failure, Enable start button to retry.
            }
        }

        private void client_OnSended(bool successorfalse)
        {
            //throw new NotImplementedException();
        }

        private void client_OnMsgReceived(string info)
        {
            //throw new NotImplementedException();
            OuputLog(string.Format("Receveid: {0}", info));
            //this.client.Send(string.Format("I am {0}", Dns.GetHostName()));
        }

        private void client_StartListenThread()
        {
            OuputLog("Start Listen....");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                OuputLog("Stop Connect....");
                client.Disconnect();
                this.btnStart.Enabled = true;
                this.btnStop.Enabled = false;
            }
        }

        void OuputLog(string format, params object[] objs)
        {
            this.Invoke((MethodInvoker)delegate
              {
                  tbLog.AppendText(string.Format(format, objs));
                  tbLog.AppendText(Environment.NewLine);
              });
        }
    }
}
