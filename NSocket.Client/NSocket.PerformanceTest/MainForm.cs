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
        private System.Threading.Timer UIUpdateTimer;
        private Dictionary<string, NSocket.SocketLib.NSocketRebot> Rebots = new Dictionary<string, SocketLib.NSocketRebot>();

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            this.btnStop.Enabled = false;
            //this.tbServerIP.Text = "192.168.1.104";
            this.tbServerIP.Text = "127.0.0.1";
            this.tbPort.Text = "7890";
            this.tbClientNum.Text = "10";

            this.lvClients.View = View.Details;
            this.lvClients.GridLines = true;
            this.lvClients.FullRowSelect = true;
            this.lvClients.Columns.Add("", 0);
            this.lvClients.Columns.Add("NAME", 100);
            this.lvClients.Columns.Add("Delay", 100);

            this.lvClients.Columns.Add("Sended(byte)", 100);
            this.lvClients.Columns.Add("Received(byte)", 100);
            this.lvClients.Columns.Add("WorkStatus", 100);
            this.lvClients.Columns.Add("ConnStatus", 100);
            this.lvClients.Columns.Add("Success Send", 100);
            this.lvClients.Columns.Add("Failure Send", 100);
            this.lvClients.Columns.Add("RecTimes", 100);
            this.lvClients.Columns.Add("ConnTimes", 100);
            UIUpdateTimer = new System.Threading.Timer(UpdateUI, 0, 0, 1000);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.btnStart.Enabled = false;
            int port = -1;
            if (int.TryParse(this.tbPort.Text, out port) && port <= 0)
            {
                MessageBox.Show("Wrong tcp port");
                return;
            }

            int clientNums = -1;
            if (int.TryParse(this.tbClientNum.Text, out clientNums) && clientNums <= 0)
            {
                MessageBox.Show("Wrong client nums");
                return;
            }
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Network UnAvailable");
                return;
            }

            IPHostEntry host = Dns.GetHostEntry(tbServerIP.Text);
            IPAddress[] addressList = host.AddressList;

            if (addressList.Length < 1)
            {
                MessageBox.Show("Didn't found any server");
                return;
            }

            this.Rebots.Clear();//Clear old rebot data.
            this.lvClients.Items.Clear();//Clear List View Old Rebot Data.
            ThreadPool.QueueUserWorkItem((o) =>
            {
                for (int i = 0; i < clientNums; i++)
                {
                    NSocket.SocketLib.NSocketRebot rebot = new SocketLib.NSocketRebot(addressList[addressList.Length - 1], 7890, 1024);
                    rebot.Name = "#" + i.ToString();
                    rebot.SendMessage = "HELLO WORLD";
                    rebot.Start();
                    Rebots.Add(rebot.Name, rebot);
                }
            });
            this.btnStop.Enabled = true;
        }

        private void UpdateUI(object state)
        {
            Action updateAction;
            string[] rebotsKeys = new string[this.Rebots.Count];
            this.Rebots.Keys.CopyTo(rebotsKeys, 0);

            foreach (var rebotKey in rebotsKeys)
            {
                var rebot = this.Rebots[rebotKey];
                updateAction = () =>
                      {
                          if (this.lvClients.Items.ContainsKey(rebot.Name))
                          {
                              var item = this.lvClients.Items[rebot.Name];

                              this.lvClients.BeginUpdate();
                              //item.SubItems[1].Text = rebot.Name;
                              item.SubItems[2].Text = rebot.DelayTime.ToString();
                              item.SubItems[3].Text = rebot.SendLength.ToString();
                              item.SubItems[4].Text = rebot.ReceivedLength.ToString();
                              item.SubItems[5].Text = rebot.WorkStatus.ToString();
                              item.SubItems[6].Text = rebot.ConnectStatus.ToString();
                              item.SubItems[7].Text = rebot.SendSuccessTimes.ToString();
                              item.SubItems[8].Text = rebot.SendFailureTimes.ToString();
                              item.SubItems[9].Text = rebot.ReceivedTimes.ToString();
                              item.SubItems[10].Text = rebot.TryConnectTimes.ToString();
                              this.lvClients.EndUpdate();
                          }
                          else
                          {

                              this.lvClients.BeginUpdate();
                              ListViewItem item = new ListViewItem();
                              item.Name = rebot.Name;
                              item.SubItems.AddRange(new string[] { 
                                  rebot.Name, 
                                  rebot.DelayTime.ToString(), 
                                  rebot.SendLength.ToString(), 
                                  rebot.ReceivedLength.ToString(), 
                                  rebot.WorkStatus.ToString(),
                                  rebot.ConnectStatus.ToString(), 
                                  rebot.SendSuccessTimes.ToString(),
                                  rebot.SendFailureTimes.ToString(),
                                  rebot.ReceivedTimes.ToString(),
                                  rebot.TryConnectTimes.ToString()
                              });
                              this.lvClients.Items.Add(item);
                              this.lvClients.EndUpdate();
                          }
                      };
                UIThreadInvoke(updateAction);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            foreach (var rebot in this.Rebots.Values)
            {
                rebot.Stop();
            }

            this.btnStop.Enabled = false;
            this.btnStart.Enabled = true;
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
    }
}
