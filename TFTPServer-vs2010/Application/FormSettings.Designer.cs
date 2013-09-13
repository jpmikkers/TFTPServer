namespace TFTPServerApp
{
    partial class FormSettings
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
            this.textBoxAddress = new System.Windows.Forms.TextBox();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.textBoxRootPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonPickAddress = new System.Windows.Forms.Button();
            this.buttonSelectPath = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.checkBoxAllowReads = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowWrites = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoCreateSubDirectories = new System.Windows.Forms.CheckBox();
            this.checkBoxSinglePortMode = new System.Windows.Forms.CheckBox();
            this.textBoxTTL = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxTimeout = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxRetries = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxDontFragment = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBoxConvertPathSeparator = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxWindowSize = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.buttonAlternatives = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.Location = new System.Drawing.Point(72, 35);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(225, 20);
            this.textBoxAddress.TabIndex = 3;
            this.toolTip1.SetToolTip(this.textBoxAddress, "IP Address to serve from.");
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(72, 62);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(59, 20);
            this.textBoxPort.TabIndex = 6;
            this.toolTip1.SetToolTip(this.textBoxPort, "Port to serve from. (default: 69)");
            // 
            // textBoxRootPath
            // 
            this.textBoxRootPath.Location = new System.Drawing.Point(72, 173);
            this.textBoxRootPath.Name = "textBoxRootPath";
            this.textBoxRootPath.Size = new System.Drawing.Size(272, 20);
            this.textBoxRootPath.TabIndex = 22;
            this.toolTip1.SetToolTip(this.textBoxRootPath, "Path of the directory to serve.");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 176);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Root path";
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(222, 247);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 28;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(303, 247);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 29;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonPickAddress
            // 
            this.buttonPickAddress.Location = new System.Drawing.Point(303, 33);
            this.buttonPickAddress.Name = "buttonPickAddress";
            this.buttonPickAddress.Size = new System.Drawing.Size(75, 23);
            this.buttonPickAddress.TabIndex = 4;
            this.buttonPickAddress.Text = "Pick";
            this.buttonPickAddress.UseVisualStyleBackColor = true;
            this.buttonPickAddress.Click += new System.EventHandler(this.buttonPickAddress_Click);
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Location = new System.Drawing.Point(350, 171);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(28, 23);
            this.buttonSelectPath.TabIndex = 23;
            this.buttonSelectPath.Text = "...";
            this.buttonSelectPath.UseVisualStyleBackColor = true;
            this.buttonSelectPath.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxAllowReads
            // 
            this.checkBoxAllowReads.AutoSize = true;
            this.checkBoxAllowReads.Location = new System.Drawing.Point(72, 199);
            this.checkBoxAllowReads.Name = "checkBoxAllowReads";
            this.checkBoxAllowReads.Size = new System.Drawing.Size(85, 17);
            this.checkBoxAllowReads.TabIndex = 24;
            this.checkBoxAllowReads.Text = "Allow Reads";
            this.checkBoxAllowReads.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowWrites
            // 
            this.checkBoxAllowWrites.AutoSize = true;
            this.checkBoxAllowWrites.Location = new System.Drawing.Point(72, 222);
            this.checkBoxAllowWrites.Name = "checkBoxAllowWrites";
            this.checkBoxAllowWrites.Size = new System.Drawing.Size(84, 17);
            this.checkBoxAllowWrites.TabIndex = 26;
            this.checkBoxAllowWrites.Text = "Allow Writes";
            this.checkBoxAllowWrites.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoCreateSubDirectories
            // 
            this.checkBoxAutoCreateSubDirectories.AutoSize = true;
            this.checkBoxAutoCreateSubDirectories.Location = new System.Drawing.Point(178, 222);
            this.checkBoxAutoCreateSubDirectories.Name = "checkBoxAutoCreateSubDirectories";
            this.checkBoxAutoCreateSubDirectories.Size = new System.Drawing.Size(189, 17);
            this.checkBoxAutoCreateSubDirectories.TabIndex = 27;
            this.checkBoxAutoCreateSubDirectories.Text = "Automatically create subdirectories";
            this.checkBoxAutoCreateSubDirectories.UseVisualStyleBackColor = true;
            // 
            // checkBoxSinglePortMode
            // 
            this.checkBoxSinglePortMode.AutoSize = true;
            this.checkBoxSinglePortMode.Location = new System.Drawing.Point(137, 64);
            this.checkBoxSinglePortMode.Name = "checkBoxSinglePortMode";
            this.checkBoxSinglePortMode.Size = new System.Drawing.Size(105, 17);
            this.checkBoxSinglePortMode.TabIndex = 7;
            this.checkBoxSinglePortMode.Text = "Single port mode";
            this.checkBoxSinglePortMode.UseVisualStyleBackColor = true;
            // 
            // textBoxTTL
            // 
            this.textBoxTTL.Location = new System.Drawing.Point(72, 89);
            this.textBoxTTL.Name = "textBoxTTL";
            this.textBoxTTL.Size = new System.Drawing.Size(59, 20);
            this.textBoxTTL.TabIndex = 10;
            this.toolTip1.SetToolTip(this.textBoxTTL, "How many hops should the TFTP packet survive. (default: -1)");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 92);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "TTL";
            // 
            // textBoxTimeout
            // 
            this.textBoxTimeout.Location = new System.Drawing.Point(72, 116);
            this.textBoxTimeout.Name = "textBoxTimeout";
            this.textBoxTimeout.Size = new System.Drawing.Size(59, 20);
            this.textBoxTimeout.TabIndex = 13;
            this.toolTip1.SetToolTip(this.textBoxTimeout, "Response timeout.");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Timeout";
            // 
            // textBoxRetries
            // 
            this.textBoxRetries.Location = new System.Drawing.Point(221, 116);
            this.textBoxRetries.Name = "textBoxRetries";
            this.textBoxRetries.Size = new System.Drawing.Size(59, 20);
            this.textBoxRetries.TabIndex = 16;
            this.toolTip1.SetToolTip(this.textBoxRetries, "Maximum number of retries after a response timeout.");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(175, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Retries";
            // 
            // checkBoxDontFragment
            // 
            this.checkBoxDontFragment.AutoSize = true;
            this.checkBoxDontFragment.Location = new System.Drawing.Point(248, 64);
            this.checkBoxDontFragment.Name = "checkBoxDontFragment";
            this.checkBoxDontFragment.Size = new System.Drawing.Size(136, 17);
            this.checkBoxDontFragment.TabIndex = 8;
            this.checkBoxDontFragment.Text = "Don\'t fragment packets";
            this.checkBoxDontFragment.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(134, 119);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "ms";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(134, 92);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "hops";
            // 
            // checkBoxConvertPathSeparator
            // 
            this.checkBoxConvertPathSeparator.AutoSize = true;
            this.checkBoxConvertPathSeparator.Location = new System.Drawing.Point(178, 200);
            this.checkBoxConvertPathSeparator.Name = "checkBoxConvertPathSeparator";
            this.checkBoxConvertPathSeparator.Size = new System.Drawing.Size(170, 17);
            this.checkBoxConvertPathSeparator.TabIndex = 25;
            this.checkBoxConvertPathSeparator.Text = "Convert path separator \'/\' to \'\\\'";
            this.checkBoxConvertPathSeparator.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(30, 11);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(35, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Name";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(72, 8);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(225, 20);
            this.textBoxName.TabIndex = 1;
            this.toolTip1.SetToolTip(this.textBoxName, "Name of the group of settings for this server.");
            // 
            // textBoxWindowSize
            // 
            this.textBoxWindowSize.Location = new System.Drawing.Point(72, 144);
            this.textBoxWindowSize.Name = "textBoxWindowSize";
            this.textBoxWindowSize.Size = new System.Drawing.Size(59, 20);
            this.textBoxWindowSize.TabIndex = 18;
            this.toolTip1.SetToolTip(this.textBoxWindowSize, "Number of packets to send in bulk, speeding up transfers");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(2, 147);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "Window size";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(134, 147);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(45, 13);
            this.label11.TabIndex = 19;
            this.label11.Text = "packets";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // buttonAlternatives
            // 
            this.buttonAlternatives.Location = new System.Drawing.Point(191, 143);
            this.buttonAlternatives.Name = "buttonAlternatives";
            this.buttonAlternatives.Size = new System.Drawing.Size(153, 23);
            this.buttonAlternatives.TabIndex = 20;
            this.buttonAlternatives.Text = "Alternative configurations";
            this.toolTip1.SetToolTip(this.buttonAlternatives, "Click here to set alternative configurations for specific files or directories.");
            this.buttonAlternatives.UseVisualStyleBackColor = true;
            this.buttonAlternatives.Click += new System.EventHandler(this.buttonAlternatives_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 8000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 100;
            // 
            // FormSettings
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(388, 280);
            this.Controls.Add(this.checkBoxConvertPathSeparator);
            this.Controls.Add(this.checkBoxAutoCreateSubDirectories);
            this.Controls.Add(this.checkBoxAllowWrites);
            this.Controls.Add(this.checkBoxDontFragment);
            this.Controls.Add(this.checkBoxSinglePortMode);
            this.Controls.Add(this.checkBoxAllowReads);
            this.Controls.Add(this.buttonSelectPath);
            this.Controls.Add(this.buttonAlternatives);
            this.Controls.Add(this.buttonPickAddress);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxRootPath);
            this.Controls.Add(this.textBoxRetries);
            this.Controls.Add(this.textBoxWindowSize);
            this.Controls.Add(this.textBoxTimeout);
            this.Controls.Add(this.textBoxTTL);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.textBoxAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxAddress;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.TextBox textBoxRootPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonPickAddress;
        private System.Windows.Forms.Button buttonSelectPath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox checkBoxAllowReads;
        private System.Windows.Forms.CheckBox checkBoxAllowWrites;
        private System.Windows.Forms.CheckBox checkBoxAutoCreateSubDirectories;
        private System.Windows.Forms.CheckBox checkBoxSinglePortMode;
        private System.Windows.Forms.TextBox textBoxTTL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxTimeout;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxRetries;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxDontFragment;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBoxConvertPathSeparator;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxWindowSize;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Button buttonAlternatives;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}