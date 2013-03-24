using ComponentOwl.BetterListView;
namespace NSocket.PerformanceTest
{
    partial class ListViewPerformanceTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listView = new ComponentOwl.BetterListView.BetterListView();
            this.betterListViewColumnHeader1 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader2 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.listView)).BeginInit();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new object[] {
            this.betterListViewColumnHeader1,
            this.betterListViewColumnHeader2});
            this.listView.Location = new System.Drawing.Point(12, 60);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(704, 356);
            this.listView.TabIndex = 3;
            // 
            // betterListViewColumnHeader1
            // 
            this.betterListViewColumnHeader1.AlignHorizontal = ComponentOwl.BetterListView.TextAlignmentHorizontal.Center;
            this.betterListViewColumnHeader1.Name = "betterListViewColumnHeader1";
            this.betterListViewColumnHeader1.Text = "Name";
            // 
            // betterListViewColumnHeader2
            // 
            this.betterListViewColumnHeader2.AlignHorizontal = ComponentOwl.BetterListView.TextAlignmentHorizontal.Center;
            this.betterListViewColumnHeader2.Name = "betterListViewColumnHeader2";
            this.betterListViewColumnHeader2.Text = "Number";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(550, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ListViewPerformanceTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 428);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listView);
            this.Name = "ListViewPerformanceTest";
            this.Text = "ListViewPerformanceTest";
            ((System.ComponentModel.ISupportInitialize)(this.listView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentOwl.BetterListView.BetterListView listView;
        private BetterListViewColumnHeader betterListViewColumnHeader1;
        private BetterListViewColumnHeader betterListViewColumnHeader2;
        private System.Windows.Forms.Button button1;
    }
}