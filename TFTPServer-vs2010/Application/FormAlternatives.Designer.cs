namespace TFTPServerApp
{
    partial class FormAlternatives
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAlternatives));
            this.comboBoxSelectAlternative = new System.Windows.Forms.ComboBox();
            this.buttonNewAlternative = new System.Windows.Forms.Button();
            this.buttonDeleteAlternative = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonMatchResult = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxTestString = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxWindowSize = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxFilterMode = new System.Windows.Forms.ComboBox();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxSelectAlternative
            // 
            this.comboBoxSelectAlternative.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSelectAlternative.FormattingEnabled = true;
            this.comboBoxSelectAlternative.Location = new System.Drawing.Point(9, 14);
            this.comboBoxSelectAlternative.Name = "comboBoxSelectAlternative";
            this.comboBoxSelectAlternative.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSelectAlternative.TabIndex = 0;
            this.comboBoxSelectAlternative.SelectedIndexChanged += new System.EventHandler(this.comboBoxSelectAlternative_SelectedIndexChanged);
            // 
            // buttonNewAlternative
            // 
            this.buttonNewAlternative.Location = new System.Drawing.Point(9, 41);
            this.buttonNewAlternative.Name = "buttonNewAlternative";
            this.buttonNewAlternative.Size = new System.Drawing.Size(121, 23);
            this.buttonNewAlternative.TabIndex = 1;
            this.buttonNewAlternative.Text = "Add new";
            this.buttonNewAlternative.UseVisualStyleBackColor = true;
            this.buttonNewAlternative.Click += new System.EventHandler(this.buttonNewAlternative_Click);
            // 
            // buttonDeleteAlternative
            // 
            this.buttonDeleteAlternative.Location = new System.Drawing.Point(9, 70);
            this.buttonDeleteAlternative.Name = "buttonDeleteAlternative";
            this.buttonDeleteAlternative.Size = new System.Drawing.Size(121, 23);
            this.buttonDeleteAlternative.TabIndex = 2;
            this.buttonDeleteAlternative.Text = "Delete selected";
            this.buttonDeleteAlternative.UseVisualStyleBackColor = true;
            this.buttonDeleteAlternative.Click += new System.EventHandler(this.buttonDeleteAlternative_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(485, 181);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(404, 181);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonMatchResult);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxTestString);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.textBoxWindowSize);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBoxFilterMode);
            this.groupBox1.Controls.Add(this.textBoxFilter);
            this.groupBox1.Location = new System.Drawing.Point(136, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 127);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // buttonMatchResult
            // 
            this.buttonMatchResult.Location = new System.Drawing.Point(293, 99);
            this.buttonMatchResult.Name = "buttonMatchResult";
            this.buttonMatchResult.Size = new System.Drawing.Size(119, 23);
            this.buttonMatchResult.TabIndex = 9;
            this.buttonMatchResult.Text = "Does not match!";
            this.buttonMatchResult.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Test String";
            // 
            // textBoxTestString
            // 
            this.textBoxTestString.Location = new System.Drawing.Point(76, 101);
            this.textBoxTestString.Name = "textBoxTestString";
            this.textBoxTestString.Size = new System.Drawing.Size(211, 20);
            this.textBoxTestString.TabIndex = 8;
            this.textBoxTestString.TextChanged += new System.EventHandler(this.textBoxTestString_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(141, 78);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(45, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "packets";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 78);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 13);
            this.label10.TabIndex = 4;
            this.label10.Text = "Window size";
            // 
            // textBoxWindowSize
            // 
            this.textBoxWindowSize.Location = new System.Drawing.Point(76, 75);
            this.textBoxWindowSize.Name = "textBoxWindowSize";
            this.textBoxWindowSize.Size = new System.Drawing.Size(59, 20);
            this.textBoxWindowSize.TabIndex = 5;
            this.textBoxWindowSize.Validated += new System.EventHandler(this.textBoxWindowSize_Validated);
            this.textBoxWindowSize.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxWindowSize_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Filter";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mode";
            // 
            // comboBoxFilterMode
            // 
            this.comboBoxFilterMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilterMode.FormattingEnabled = true;
            this.comboBoxFilterMode.Items.AddRange(new object[] {
            "Wildcard",
            "Regex"});
            this.comboBoxFilterMode.Location = new System.Drawing.Point(76, 21);
            this.comboBoxFilterMode.Name = "comboBoxFilterMode";
            this.comboBoxFilterMode.Size = new System.Drawing.Size(110, 21);
            this.comboBoxFilterMode.TabIndex = 1;
            this.comboBoxFilterMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxFilterMode_SelectedIndexChanged);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Location = new System.Drawing.Point(76, 48);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(211, 20);
            this.textBoxFilter.TabIndex = 3;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            this.textBoxFilter.Validated += new System.EventHandler(this.textBoxFilter_Validated);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(9, 142);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(389, 59);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // FormAlternatives
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(568, 213);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonDeleteAlternative);
            this.Controls.Add(this.buttonNewAlternative);
            this.Controls.Add(this.comboBoxSelectAlternative);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormAlternatives";
            this.Text = "Alternative Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxSelectAlternative;
        private System.Windows.Forms.Button buttonNewAlternative;
        private System.Windows.Forms.Button buttonDeleteAlternative;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxFilterMode;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.Button buttonMatchResult;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxTestString;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBoxWindowSize;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.TextBox textBox1;
    }
}