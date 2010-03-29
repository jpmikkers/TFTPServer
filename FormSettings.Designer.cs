namespace TFTPServer
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
            this.SuspendLayout();
            // 
            // textBoxAddress
            // 
            this.textBoxAddress.Location = new System.Drawing.Point(69, 12);
            this.textBoxAddress.Name = "textBoxAddress";
            this.textBoxAddress.Size = new System.Drawing.Size(225, 20);
            this.textBoxAddress.TabIndex = 1;
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(69, 38);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(59, 20);
            this.textBoxPort.TabIndex = 4;
            // 
            // textBoxRootPath
            // 
            this.textBoxRootPath.Location = new System.Drawing.Point(69, 116);
            this.textBoxRootPath.Name = "textBoxRootPath";
            this.textBoxRootPath.Size = new System.Drawing.Size(272, 20);
            this.textBoxRootPath.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Root path";
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(219, 195);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 14;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(300, 195);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonPickAddress
            // 
            this.buttonPickAddress.Location = new System.Drawing.Point(300, 10);
            this.buttonPickAddress.Name = "buttonPickAddress";
            this.buttonPickAddress.Size = new System.Drawing.Size(75, 23);
            this.buttonPickAddress.TabIndex = 2;
            this.buttonPickAddress.Text = "Pick";
            this.buttonPickAddress.UseVisualStyleBackColor = true;
            this.buttonPickAddress.Click += new System.EventHandler(this.buttonPickAddress_Click);
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Location = new System.Drawing.Point(347, 114);
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
            this.checkBoxAllowReads.Location = new System.Drawing.Point(69, 142);
            this.checkBoxAllowReads.Name = "checkBoxAllowReads";
            this.checkBoxAllowReads.Size = new System.Drawing.Size(85, 17);
            this.checkBoxAllowReads.TabIndex = 11;
            this.checkBoxAllowReads.Text = "Allow Reads";
            this.checkBoxAllowReads.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowWrites
            // 
            this.checkBoxAllowWrites.AutoSize = true;
            this.checkBoxAllowWrites.Location = new System.Drawing.Point(69, 165);
            this.checkBoxAllowWrites.Name = "checkBoxAllowWrites";
            this.checkBoxAllowWrites.Size = new System.Drawing.Size(84, 17);
            this.checkBoxAllowWrites.TabIndex = 12;
            this.checkBoxAllowWrites.Text = "Allow Writes";
            this.checkBoxAllowWrites.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoCreateSubDirectories
            // 
            this.checkBoxAutoCreateSubDirectories.AutoSize = true;
            this.checkBoxAutoCreateSubDirectories.Location = new System.Drawing.Point(175, 165);
            this.checkBoxAutoCreateSubDirectories.Name = "checkBoxAutoCreateSubDirectories";
            this.checkBoxAutoCreateSubDirectories.Size = new System.Drawing.Size(189, 17);
            this.checkBoxAutoCreateSubDirectories.TabIndex = 13;
            this.checkBoxAutoCreateSubDirectories.Text = "Automatically create subdirectories";
            this.checkBoxAutoCreateSubDirectories.UseVisualStyleBackColor = true;
            // 
            // checkBoxSinglePortMode
            // 
            this.checkBoxSinglePortMode.AutoSize = true;
            this.checkBoxSinglePortMode.Location = new System.Drawing.Point(134, 40);
            this.checkBoxSinglePortMode.Name = "checkBoxSinglePortMode";
            this.checkBoxSinglePortMode.Size = new System.Drawing.Size(105, 17);
            this.checkBoxSinglePortMode.TabIndex = 5;
            this.checkBoxSinglePortMode.Text = "Single port mode";
            this.checkBoxSinglePortMode.UseVisualStyleBackColor = true;
            // 
            // textBoxTTL
            // 
            this.textBoxTTL.Location = new System.Drawing.Point(69, 64);
            this.textBoxTTL.Name = "textBoxTTL";
            this.textBoxTTL.Size = new System.Drawing.Size(59, 20);
            this.textBoxTTL.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "TTL";
            // 
            // textBoxTimeout
            // 
            this.textBoxTimeout.Location = new System.Drawing.Point(69, 90);
            this.textBoxTimeout.Name = "textBoxTimeout";
            this.textBoxTimeout.Size = new System.Drawing.Size(59, 20);
            this.textBoxTimeout.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 93);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Timeout";
            // 
            // textBoxRetries
            // 
            this.textBoxRetries.Location = new System.Drawing.Point(218, 90);
            this.textBoxRetries.Name = "textBoxRetries";
            this.textBoxRetries.Size = new System.Drawing.Size(59, 20);
            this.textBoxRetries.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(172, 93);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Retries";
            // 
            // checkBoxDontFragment
            // 
            this.checkBoxDontFragment.AutoSize = true;
            this.checkBoxDontFragment.Location = new System.Drawing.Point(245, 40);
            this.checkBoxDontFragment.Name = "checkBoxDontFragment";
            this.checkBoxDontFragment.Size = new System.Drawing.Size(136, 17);
            this.checkBoxDontFragment.TabIndex = 5;
            this.checkBoxDontFragment.Text = "Don\'t fragment packets";
            this.checkBoxDontFragment.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(131, 93);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "ms";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(131, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "hops";
            // 
            // FormSettings
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(388, 229);
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
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxRootPath);
            this.Controls.Add(this.textBoxRetries);
            this.Controls.Add(this.textBoxTimeout);
            this.Controls.Add(this.textBoxTTL);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.textBoxAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.Text = "ms";
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
    }
}