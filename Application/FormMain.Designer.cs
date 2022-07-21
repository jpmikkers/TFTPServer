namespace TFTPServerApp
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "iohi",
            "oihoh",
            "oih",
            "1099511627776",
            "1099511627776"}, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonElevate = new System.Windows.Forms.Button();
            this.timerServiceWatcher = new System.Windows.Forms.Timer(this.components);
            this.buttonConfigure = new System.Windows.Forms.Button();
            this.buttonAbout = new System.Windows.Forms.Button();
            this.buttonHistoryOneDay = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelFilter = new System.Windows.Forms.Label();
            this.buttonHistoryAll = new System.Windows.Forms.Button();
            this.buttonHistoryOneHour = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageTransfers = new System.Windows.Forms.TabPage();
            this.listViewTransfers = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageEventLog = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPageTransfers.SuspendLayout();
            this.tabPageEventLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(11, 15);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(100, 28);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "&Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(119, 15);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(100, 28);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "S&top";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonElevate
            // 
            this.buttonElevate.Location = new System.Drawing.Point(335, 15);
            this.buttonElevate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonElevate.Name = "buttonElevate";
            this.buttonElevate.Size = new System.Drawing.Size(100, 28);
            this.buttonElevate.TabIndex = 3;
            this.buttonElevate.Text = "&Elevate";
            this.buttonElevate.UseVisualStyleBackColor = true;
            this.buttonElevate.Click += new System.EventHandler(this.buttonElevate_Click);
            // 
            // timerServiceWatcher
            // 
            this.timerServiceWatcher.Enabled = true;
            this.timerServiceWatcher.Interval = 1000;
            this.timerServiceWatcher.Tick += new System.EventHandler(this.timerServiceWatcher_Tick);
            // 
            // buttonConfigure
            // 
            this.buttonConfigure.Location = new System.Drawing.Point(227, 15);
            this.buttonConfigure.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonConfigure.Name = "buttonConfigure";
            this.buttonConfigure.Size = new System.Drawing.Size(100, 28);
            this.buttonConfigure.TabIndex = 2;
            this.buttonConfigure.Text = "&Configure";
            this.buttonConfigure.UseVisualStyleBackColor = true;
            this.buttonConfigure.Click += new System.EventHandler(this.buttonConfigure_Click);
            // 
            // buttonAbout
            // 
            this.buttonAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAbout.Location = new System.Drawing.Point(1229, 9);
            this.buttonAbout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonAbout.Name = "buttonAbout";
            this.buttonAbout.Size = new System.Drawing.Size(84, 39);
            this.buttonAbout.TabIndex = 4;
            this.buttonAbout.Text = "About";
            this.buttonAbout.UseVisualStyleBackColor = true;
            this.buttonAbout.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonHistoryOneDay
            // 
            this.buttonHistoryOneDay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHistoryOneDay.Location = new System.Drawing.Point(1054, 11);
            this.buttonHistoryOneDay.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonHistoryOneDay.Name = "buttonHistoryOneDay";
            this.buttonHistoryOneDay.Size = new System.Drawing.Size(76, 28);
            this.buttonHistoryOneDay.TabIndex = 3;
            this.buttonHistoryOneDay.Text = "+1 day";
            this.buttonHistoryOneDay.UseVisualStyleBackColor = true;
            this.buttonHistoryOneDay.Click += new System.EventHandler(this.buttonShowHistory_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(1222, 11);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(76, 28);
            this.buttonClear.TabIndex = 5;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonStart);
            this.panel1.Controls.Add(this.buttonStop);
            this.panel1.Controls.Add(this.buttonAbout);
            this.panel1.Controls.Add(this.buttonConfigure);
            this.panel1.Controls.Add(this.buttonElevate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1324, 57);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.labelFilter);
            this.panel2.Controls.Add(this.buttonHistoryAll);
            this.panel2.Controls.Add(this.buttonHistoryOneHour);
            this.panel2.Controls.Add(this.buttonHistoryOneDay);
            this.panel2.Controls.Add(this.buttonClear);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(4, 371);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1308, 50);
            this.panel2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(872, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "More history:";
            // 
            // labelFilter
            // 
            this.labelFilter.AutoSize = true;
            this.labelFilter.Location = new System.Drawing.Point(7, 17);
            this.labelFilter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(143, 16);
            this.labelFilter.TabIndex = 0;
            this.labelFilter.Text = "Showing log starting at:";
            // 
            // buttonHistoryAll
            // 
            this.buttonHistoryAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHistoryAll.Location = new System.Drawing.Point(1138, 11);
            this.buttonHistoryAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonHistoryAll.Name = "buttonHistoryAll";
            this.buttonHistoryAll.Size = new System.Drawing.Size(76, 28);
            this.buttonHistoryAll.TabIndex = 4;
            this.buttonHistoryAll.Text = "All";
            this.buttonHistoryAll.UseVisualStyleBackColor = true;
            this.buttonHistoryAll.Click += new System.EventHandler(this.buttonHistoryAll_Click);
            // 
            // buttonHistoryOneHour
            // 
            this.buttonHistoryOneHour.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHistoryOneHour.Location = new System.Drawing.Point(970, 11);
            this.buttonHistoryOneHour.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonHistoryOneHour.Name = "buttonHistoryOneHour";
            this.buttonHistoryOneHour.Size = new System.Drawing.Size(76, 28);
            this.buttonHistoryOneHour.TabIndex = 2;
            this.buttonHistoryOneHour.Text = "+1 hour";
            this.buttonHistoryOneHour.UseVisualStyleBackColor = true;
            this.buttonHistoryOneHour.Click += new System.EventHandler(this.buttonHistoryOneHour_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 512);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1324, 26);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(117, 20);
            this.toolStripStatusLabel.Text = "Service Status : -";
            // 
            // eventLog1
            // 
            this.eventLog1.Log = "TFTPServerLog";
            this.eventLog1.SynchronizingObject = this;
            this.eventLog1.EntryWritten += new System.Diagnostics.EntryWrittenEventHandler(this.eventLog1_EntryWritten);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageTransfers);
            this.tabControl1.Controls.Add(this.tabPageEventLog);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 57);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1324, 455);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPageTransfers
            // 
            this.tabPageTransfers.BackColor = System.Drawing.Color.Transparent;
            this.tabPageTransfers.Controls.Add(this.listViewTransfers);
            this.tabPageTransfers.Location = new System.Drawing.Point(4, 25);
            this.tabPageTransfers.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageTransfers.Name = "tabPageTransfers";
            this.tabPageTransfers.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.tabPageTransfers.Size = new System.Drawing.Size(1316, 426);
            this.tabPageTransfers.TabIndex = 1;
            this.tabPageTransfers.Text = "Transfers";
            this.tabPageTransfers.UseVisualStyleBackColor = true;
            // 
            // listViewTransfers
            // 
            this.listViewTransfers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader9,
            this.columnHeader8,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader10});
            this.listViewTransfers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewTransfers.FullRowSelect = true;
            this.listViewTransfers.HideSelection = false;
            this.listViewTransfers.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listViewTransfers.Location = new System.Drawing.Point(7, 6);
            this.listViewTransfers.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listViewTransfers.Name = "listViewTransfers";
            this.listViewTransfers.OwnerDraw = true;
            this.listViewTransfers.ShowGroups = false;
            this.listViewTransfers.Size = new System.Drawing.Size(1302, 414);
            this.listViewTransfers.TabIndex = 0;
            this.listViewTransfers.UseCompatibleStateImageBehavior = false;
            this.listViewTransfers.View = System.Windows.Forms.View.Details;
            this.listViewTransfers.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewTransfers_DrawColumnHeader);
            this.listViewTransfers.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewTransfers_DrawSubItem);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Time";
            this.columnHeader1.Width = 132;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Up/Dn";
            this.columnHeader2.Width = 49;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "FileName";
            this.columnHeader3.Width = 165;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Server";
            this.columnHeader4.Width = 63;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Transferred";
            this.columnHeader5.Width = 90;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Transfersize";
            this.columnHeader9.Width = 90;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Speed";
            this.columnHeader8.Width = 80;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "State";
            this.columnHeader6.Width = 80;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Reason";
            this.columnHeader7.Width = 138;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Progress %";
            this.columnHeader10.Width = 81;
            // 
            // tabPageEventLog
            // 
            this.tabPageEventLog.BackColor = System.Drawing.Color.Transparent;
            this.tabPageEventLog.Controls.Add(this.textBox1);
            this.tabPageEventLog.Controls.Add(this.panel2);
            this.tabPageEventLog.Location = new System.Drawing.Point(4, 25);
            this.tabPageEventLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageEventLog.Name = "tabPageEventLog";
            this.tabPageEventLog.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPageEventLog.Size = new System.Drawing.Size(1316, 425);
            this.tabPageEventLog.TabIndex = 0;
            this.tabPageEventLog.Text = "Event Log";
            this.tabPageEventLog.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(4, 4);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(1308, 367);
            this.textBox1.TabIndex = 0;
            this.textBox1.WordWrap = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1324, 538);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(554, 235);
            this.Name = "FormMain";
            this.Text = "TFTP Server";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPageTransfers.ResumeLayout(false);
            this.tabPageEventLog.ResumeLayout(false);
            this.tabPageEventLog.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonElevate;
        private System.Windows.Forms.Timer timerServiceWatcher;
        private System.Windows.Forms.Button buttonConfigure;
        private System.Windows.Forms.Button buttonAbout;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonHistoryOneDay;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.Button buttonHistoryAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonHistoryOneHour;
        private System.Diagnostics.EventLog eventLog1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageEventLog;
        private System.Windows.Forms.TabPage tabPageTransfers;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listViewTransfers;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
    }
}