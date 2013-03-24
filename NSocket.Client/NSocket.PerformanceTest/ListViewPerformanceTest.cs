using ComponentOwl.BetterListView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NSocket.PerformanceTest
{
    public partial class ListViewPerformanceTest : Form
    {
        public ListViewPerformanceTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadItems(10000);
        }

        private void LoadItems(int itemCount)
        {
            DateTime dtStart = DateTime.Now;

            //this.labelStatus.Text = "Loading items...";

            //this.labelStatus.Refresh();

            //this.buttonLoad1KItems.Enabled = false;
            //this.buttonLoad10KItems.Enabled = false;
            //this.buttonClear.Enabled = true;

            this.listView.BeginUpdate();

            // allocate an array of items
            //BetterListViewItem[] items = new BetterListViewItem[itemCount];

            for (int indexItem = 0; indexItem < itemCount; indexItem++)
            {
                // create a new item
                BetterListViewItem item = new BetterListViewItem(
                    new[]
                        {
                            "Some Item",
                            Convert.ToString(indexItem + 1)
                        });

                item.SubItems[1].AlignHorizontal = TextAlignmentHorizontal.Right;

                // put item into array
                //items[indexItem] = item;
                this.listView.Items.Add(item);
            }

            // add created items to Better ListView in a single call
            //this.listView.Items.AddRange(items);

            //this.listView.Enabled = true;

            this.listView.EndUpdate();

            MessageBox.Show(String.Format(
                            "Items loaded in  {0:N0}  milliseconds.",
                            (int)Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds)));
        }
    }
}
