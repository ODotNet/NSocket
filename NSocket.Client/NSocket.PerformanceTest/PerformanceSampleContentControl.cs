// -----------------------------------------------------------------------
// <copyright file="PerformanceSampleContentControl.cs" company="ComponentOwl.com">
//     Copyright © 2010-2013 ComponentOwl.com. All rights reserved.
// </copyright>
// <author>Libor Tinka</author>
// -----------------------------------------------------------------------

namespace ComponentOwl.BetterListView.Samples.CSharp
{
    #region Usings

    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    #endregion

    [ToolboxItem(false)]
    public partial class PerformanceSampleContentControl : UserControl
    {
        public BetterListView ListView
        {
            get
            {
                return this.listView;
            }
        }

        private bool SmoothColumnResize
        {
            set
            {
                this.listView.BeginUpdate();

                foreach (BetterListViewColumnHeader columnHeader in this.listView.Columns)
                {
                    columnHeader.SmoothResize = value;
                }

                this.listView.EndUpdate();
            }
        }

        public PerformanceSampleContentControl()
        {
            InitializeComponent();

            this.buttonLoad1KItems.Enabled = true;
            this.buttonLoad10KItems.Enabled = true;
            this.buttonClear.Enabled = false;
            this.listView.Enabled = false;
        }

        private void ButtonLoad1KItemsClick(object sender, EventArgs e)
        {
            LoadItems(1000);

            SmoothColumnResize = true;
        }

        private void ButtonLoad10KItemsClick(object sender, EventArgs e)
        {
            LoadItems(10000);

            SmoothColumnResize = false;
        }

        private void ButtonClearClick(object sender, EventArgs e)
        {
            this.listView.Items.Clear();

            this.buttonLoad1KItems.Enabled = true;
            this.buttonLoad10KItems.Enabled = true;
            this.buttonClear.Enabled = false;
            this.listView.Enabled = false;

            this.labelStatus.Text = String.Empty;
        }

        private void LoadItems(int itemCount)
        {
            DateTime dtStart = DateTime.Now;

            this.labelStatus.Text = "Loading items...";

            this.labelStatus.Refresh();

            this.buttonLoad1KItems.Enabled = false;
            this.buttonLoad10KItems.Enabled = false;
            this.buttonClear.Enabled = true;

            this.listView.BeginUpdate();

            // allocate an array of items
            BetterListViewItem[] items = new BetterListViewItem[itemCount];

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
                items[indexItem] = item;
            }

            // add created items to Better ListView in a single call
            this.listView.Items.AddRange(items);

            this.listView.Enabled = true;

            this.listView.EndUpdate();

            this.labelStatus.Text = String.Format(
                "Items loaded in  {0:N0}  milliseconds.",
                (int)Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds));
        }
    }
}