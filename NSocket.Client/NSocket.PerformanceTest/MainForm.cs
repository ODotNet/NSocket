using ComponentOwl.BetterListView;
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
        private System.Windows.Forms.Timer UIUpdateTimer;
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
            this.tbClientNum.Text = "1";

            this.lvClients.View = ComponentOwl.BetterListView.BetterListViewView.Details;
            this.lvClients.GridLines = new ComponentOwl.BetterListView.BetterListViewGridLines();
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
            this.lvClients.Columns.Add("TryConnTimes", 100);
            this.lvClients.Columns.Add("ConnDelay", 100);
            this.lvClients.Columns.Add("TID", 30);
            UIUpdateTimer = new System.Windows.Forms.Timer(); //new System.Threading.Timer(UpdateUI, 0, 0, 2000);
            UIUpdateTimer.Tick += UIUpdateTimer_Tick;
            UIUpdateTimer.Interval = 1000;
            UIUpdateTimer.Start();
        }

        void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateUI(null);
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

            IPHostEntry host = Dns.GetHostByAddress(tbServerIP.Text);
            IPAddress[] addressList = host.AddressList;

            if (addressList.Length < 1)
            {
                MessageBox.Show("Didn't found any server");
                return;
            }

            this.Rebots.Clear();//Clear old rebot data.
            this.lvClients.Items.Clear();//Clear List View Old Rebot Data.

            for (int i = 0; i < clientNums; i++)
            {
                NSocket.SocketLib.NSocketRebot rebot = new SocketLib.NSocketRebot(addressList[addressList.Length - 1], 7890, 1024);
                rebot.Name = "#" + i.ToString();
                rebot.SendMessage = "HELLO WORLD";
                Rebots.Add(rebot.Name, rebot);
            }

            var rebotItems = new BetterListViewItem[this.Rebots.Keys.Count];

            this.lvClients.BeginUpdate();
            int rebotIndex = 0;
            foreach (var rebot in this.Rebots.Values)
            {
                var item = new ComponentOwl.BetterListView.BetterListViewItem(new[]{string.Empty, rebot.Name,                             
                            rebot.DelayTime.ToString(),                                      
                            rebot.SendLength.ToString(), 
                            rebot.ReceivedLength.ToString(),          
                            rebot.WorkStatus.ToString(),
                            rebot.ConnectStatus.ToString(),                                        
                            rebot.SendSuccessTimes.ToString(),                                       
                            rebot.SendFailureTimes.ToString(),                                        
                            rebot.ReceivedTimes.ToString(),
                            rebot.TryConnectTimes.ToString(),                                      
                            rebot.ConnectDelay.ToString(),
                            rebot.WorkThreadID.ToString() });
                item.Name = rebot.Name;
                rebotItems[rebotIndex++] = item;
            }
            this.lvClients.Items.AddRange(rebotItems);
            this.lvClients.EndUpdate();
            foreach (var rebot in this.Rebots.Values)
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    rebot.Start();
                });
            }

            this.btnStop.Enabled = true;
        }
        System.Diagnostics.Stopwatch swUIUpdate = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch swFindRebot = new System.Diagnostics.Stopwatch();

        private void UpdateUI(object state)
        {

            string[] rebotsKeys = new string[this.Rebots.Count];
            this.Rebots.Keys.CopyTo(rebotsKeys, 0);

            if (this.lvClients.Items.Count > 0)
            {
                swUIUpdate.Restart();
                this.lvClients.BeginUpdate();
                for (int i = this.lvClients.TopItemIndex; i <= this.lvClients.BottomItemIndex; i++)
                {
                    swFindRebot.Restart();

                    var item = this.lvClients.Items[i];
                    var rebot = this.Rebots[item.Name];
                    var rebotStrings = new[] {  rebot.Name,                             
                            rebot.DelayTime.ToString(),                                      
                            rebot.SendLength.ToString(), 
                            rebot.ReceivedLength.ToString(),          
                            rebot.WorkStatus.ToString(),
                            rebot.ConnectStatus.ToString(),                                        
                            rebot.SendSuccessTimes.ToString(),                                       
                            rebot.SendFailureTimes.ToString(),                                        
                            rebot.ReceivedTimes.ToString(),
                            rebot.TryConnectTimes.ToString(),                                      
                            rebot.ConnectDelay.ToString(),
                            rebot.WorkThreadID.ToString()};
                    swFindRebot.Stop();



                    //item.SubItems.Clear();
                    //item.SubItems.AddRange(rebotStrings);
                    item.SubItems[1].Text = rebot.Name;
                    item.SubItems[2].Text = rebot.DelayTime.ToString();
                    item.SubItems[3].Text = rebot.SendLength.ToString();
                    item.SubItems[4].Text = rebot.ReceivedLength.ToString();
                    item.SubItems[5].Text = rebot.WorkStatus.ToString();
                    item.SubItems[5].ForeColor = rebot.WorkStatus == SocketLib.NSocketRebotWorkStatus.Running ? Color.Green : Color.Red;
                    item.SubItems[6].Text = rebot.ConnectStatus.ToString();
                    item.SubItems[6].ForeColor =
                        rebot.ConnectStatus == SocketLib.NSocketRebotConnectStatus.Connected
                        ? Color.Green
                        : rebot.ConnectStatus == SocketLib.NSocketRebotConnectStatus.Connecting ? Color.Yellow : Color.Red;
                    item.SubItems[7].Text = rebot.SendSuccessTimes.ToString();
                    item.SubItems[8].Text = rebot.SendFailureTimes.ToString();
                    item.SubItems[9].Text = rebot.ReceivedTimes.ToString();
                    item.SubItems[10].Text = rebot.TryConnectTimes.ToString();
                    item.SubItems[11].Text = rebot.ConnectDelay.ToString();
                    item.SubItems[12].Text = rebot.WorkThreadID.ToString();


                }
                this.lvClients.EndUpdate();
                swUIUpdate.Stop();
            }

            Console.WriteLine("{0}: FindRebot: {1}, UpdateRebot:{2}", DateTime.Now.ToString("HH:mm:ss:ms"), swFindRebot.ElapsedMilliseconds, swUIUpdate.ElapsedMilliseconds);
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
