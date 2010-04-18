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
            this.SuspendLayout();
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.Location = new System.Drawing.Point(69, 34);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(225, 20);
            this.textBoxAddress.TabIndex = 1;
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(69, 60);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(59, 20);
            this.textBoxPort.TabIndex = 4;
            // 
            // textBoxRootPath
            // 
            this.textBoxRootPath.Location = new System.Drawing.Point(69, 138);
            this.textBoxRootPath.Name = "textBoxRootPath";
            this.textBoxRootPath.Size = new System.Drawing.Size(272, 20);
            this.textBoxRootPath.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 141);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Root path";
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(219, 217);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 14;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(300, 217);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonPickAddress
            // 
            this.buttonPickAddress.Location = new System.Drawing.Point(300, 32);
            this.buttonPickAddress.Name = "buttonPickAddress";
            this.buttonPickAddress.Size = new System.Drawing.Size(75, 23);
            this.buttonPickAddress.TabIndex = 2;
            this.buttonPickAddress.Text = "Pick";
            this.buttonPickAddress.UseVisualStyleBackColor = true;
            this.buttonPickAddress.Click += new System.EventHandler(this.buttonPickAddress_Click);
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Location = new System.Drawing.Point(347, 136);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(28, 23);
            this.buttonSelectPath.TabIndex = 10;
            this.buttonSelectPath.Text = "...";
            this.buttonSelectPath.UseVisualStyleBackColor = true;
            this.buttonSelectPath.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxAllowReads
            // 
            this.checkBoxAllowReads.AutoSize = true;
            this.checkBoxAllowReads.Location = new System.Drawing.Point(69, 164);
            this.checkBoxAllowReads.Name = "checkBoxAllowReads";
            this.checkBoxAllowReads.Size = new System.Drawing.Size(85, 17);
            this.checkBoxAllowReads.TabIndex = 11;
            this.checkBoxAllowReads.Text = "Allow Reads";
            this.checkBoxAllowReads.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowWrites
            // 
            this.checkBoxAllowWrites.AutoSize = true;
            this.checkBoxAllowWrites.Location = new System.Drawing.Point(69, 187);
            this.checkBoxAllowWrites.Name = "checkBoxAllowWrites";
            this.checkBoxAllowWrites.Size = new System.Drawing.Size(84, 17);
            this.checkBoxAllowWrites.TabIndex = 12;
            this.checkBoxAllowWrites.Text = "Allow Writes";
            this.checkBoxAllowWrites.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoCreateSubDirectories
            // 
            this.checkBoxAutoCreateSubDirectories.AutoSize = true;
            this.checkBoxAutoCreateSubDirectories.Location = new System.Drawing.Point(175, 187);
            this.checkBoxAutoCreateSubDirectories.Name = "checkBoxAutoCreateSubDirectories";
            this.checkBoxAutoCreateSubDirectories.Size = new System.Drawing.Size(189, 17);
            this.checkBoxAutoCreateSubDirectories.TabIndex = 13;
            this.checkBoxAutoCreateSubDirectories.Text = "Automatically create subdirectories";
            this.checkBoxAutoCreateSubDirectories.UseVisualStyleBackColor = true;
            // 
            // checkBoxSinglePortMode
            // 
            this.checkBoxSinglePortMode.AutoSize = true;
            this.checkBoxSinglePortMode.Location = new System.Drawing.Point(134, 62);
            this.checkBoxSinglePortMode.Name = "checkBoxSinglePortMode";
            this.checkBoxSinglePortMode.Size = new System.Drawing.Size(105, 17);
            this.checkBoxSinglePortMode.TabIndex = 5;
            this.checkBoxSinglePortMode.Text = "Single port mode";
            this.checkBoxSinglePortMode.UseVisualStyleBackColor = true;
            // 
            // textBoxTTL
            // 
            this.textBoxTTL.Location = new System.Drawing.Point(69, 86);
            this.textBoxTTL.Name = "textBoxTTL";
            this.textBoxTTL.Size = new System.Drawing.Size(59, 20);
            this.textBoxTTL.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "TTL";
            // 
            // textBoxTimeout
            // 
            this.textBoxTimeout.Location = new System.Drawing.Point(69, 112);
            this.textBoxTimeout.Name = "textBoxTimeout";
            this.textBoxTimeout.Size = new System.Drawing.Size(59, 20);
            this.textBoxTimeout.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Timeout";
            // 
            // textBoxRetries
            // 
            this.textBoxRetries.Location = new System.Drawing.Point(218, 112);
            this.textBoxRetries.Name = "textBoxRetries";
            this.textBoxRetries.Size = new System.Drawing.Size(59, 20);
            this.textBoxRetries.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(172, 115);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Retries";
            // 
            // checkBoxDontFragment
            // 
            this.checkBoxDontFragment.AutoSize = true;
            this.checkBoxDontFragment.Location = new System.Drawing.Point(245, 62);
            this.checkBoxDontFragment.Name = "checkBoxDontFragment";
            this.checkBoxDontFragment.Size = new System.Drawing.Size(136, 17);
            this.checkBoxDontFragment.TabIndex = 5;
            this.checkBoxDontFragment.Text = "Don\'t fragment packets";
            this.checkBoxDontFragment.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(131, 115);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "ms";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(131, 89);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "hops";
            // 
            // checkBoxConvertPathSeparator
            // 
            this.checkBoxConvertPathSeparator.AutoSize = true;
            this.checkBoxConvertPathSeparator.Location = new System.Drawing.Point(175, 165);
            this.checkBoxConvertPathSeparator.Name = "checkBoxConvertPathSeparator";
            this.checkBoxConvertPathSeparator.Size = new System.Drawing.Size(170, 17);
            this.checkBoxConvertPathSeparator.TabIndex = 13;
            this.checkBoxConvertPathSeparator.Text = "Convert path separator \'/\' to \'\\\'";
            this.checkBoxConvertPathSeparator.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(27, 11);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(35, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Name";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(69, 8);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(225, 20);
            this.textBoxName.TabIndex = 1;
            // 
            // FormSettings
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(389, 249);
            this.Controls.Add(this.checkBoxConvertPathSeparator);
            this.Controls.Add(this.checkBoxAutoCreateSubDirectories);
            this.Controls.Add(this.checkBoxAllowWrites);
            this.Controls.Add(this.checkBoxDontFragment);
            this.Controls.Add(this.checkBoxSinglePortMode);
            this.Controls.Add(this.checkBoxAllowReads);
            this.Controls.Add(this.buttonSelectPath);
            this.Controls.Add(this.buttonPickAddress);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxRootPath);
            this.Controls.Add(this.textBoxRetries);
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
    }
}