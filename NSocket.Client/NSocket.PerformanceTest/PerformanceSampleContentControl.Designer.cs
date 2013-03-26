namespace ComponentOwl.BetterListView.Samples.CSharp
{
    partial class PerformanceSampleContentControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonLoad1KItems = new System.Windows.Forms.Button();
            this.buttonLoad10KItems = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.listView = new ComponentOwl.BetterListView.BetterListView();
            this.betterListViewColumnHeader1 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.betterListViewColumnHeader2 = new ComponentOwl.BetterListView.BetterListViewColumnHeader();
            this.labelStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.listView)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonLoad1KItems
            // 
            this.buttonLoad1KItems.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonLoad1KItems.Location = new System.Drawing.Point(8, 8);
            this.buttonLoad1KItems.Name = "buttonLoad1KItems";
            this.buttonLoad1KItems.Size = new System.Drawing.Size(160, 32);
            this.buttonLoad1KItems.TabIndex = 0;
            this.buttonLoad1KItems.Text = "Load 1.000 items";
            this.buttonLoad1KItems.UseVisualStyleBackColor = true;
            this.buttonLoad1KItems.Click += new System.EventHandler(this.ButtonLoad1KItemsClick);
            // 
            // buttonLoad10KItems
            // 
            this.buttonLoad10KItems.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonLoad10KItems.Location = new System.Drawing.Point(176, 8);
            this.buttonLoad10KItems.Name = "buttonLoad10KItems";
            this.buttonLoad10KItems.Size = new System.Drawing.Size(160, 32);
            this.buttonLoad10KItems.TabIndex = 1;
            this.buttonLoad10KItems.Text = "Load 10.000 items";
            this.buttonLoad10KItems.UseVisualStyleBackColor = true;
            this.buttonLoad10KItems.Click += new System.EventHandler(this.ButtonLoad10KItemsClick);
            // 
            // buttonClear
            // 
            this.buttonClear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClear.Location = new System.Drawing.Point(344, 8);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(160, 32);
            this.buttonClear.TabIndex = 2;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.ButtonClearClick);
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new object[] {
            this.betterListViewColumnHeader1,
            this.betterListViewColumnHeader2});
            this.listView.Location = new System.Drawing.Point(8, 80);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(496, 224);
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
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Location = new System.Drawing.Point(8, 48);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(496, 23);
            this.labelStatus.TabIndex = 4;
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PerformanceSampleContentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonLoad10KItems);
            this.Controls.Add(this.buttonLoad1KItems);
            this.Name = "PerformanceSampleContentControl";
            this.Size = new System.Drawing.Size(512, 312);
            ((System.ComponentModel.ISupportInitialize)(this.listView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonLoad1KItems;
        private System.Windows.Forms.Button buttonLoad10KItems;
        private System.Windows.Forms.Button buttonClear;
        private BetterListView listView;
        private BetterListViewColumnHeader betterListViewColumnHeader1;
        private BetterListViewColumnHeader betterListViewColumnHeader2;
        private System.Windows.Forms.Label labelStatus;
    }
}
