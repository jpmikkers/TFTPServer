using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TFTPServerApp
{
    public partial class FormSettings : Form
    {
        private TFTPServerConfiguration _configuration;

        public TFTPServerConfiguration Configuration
        {
            get
            {
                return _configuration.Clone();
            }
            set
            {
                _configuration = value.Clone();
                Bind();
            }
        }

        public FormSettings()
        {
            InitializeComponent();
            textBoxWindowSize.Validating += new CancelEventHandler(textBoxWindowSize_Validating);
            textBoxWindowSize.Validated += new EventHandler(textBoxWindowSize_Validated);
            toolTip1.SetToolTip(textBoxWindowSize,
                "The number of packets to send in bulk, speeding up the file transfer rate.\r\n" +
                "This is an advanced option, only use a value greater than 1 if you've\r\n" +
                "tested that your TFTP client can cope with windowed transfers.\r\n" +
                "(default: 1)");
        }

        void textBoxWindowSize_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError(textBoxWindowSize, "");
        }

        private void textBoxWindowSize_Validating(object sender, CancelEventArgs e)
        {
            ushort value;

            e.Cancel = true;
            if(ushort.TryParse(textBoxWindowSize.Text, out value))
            {
                if(value > 0 && value <= 32) e.Cancel = false;
            }

            if(e.Cancel)
            {
                this.errorProvider1.SetError(textBoxWindowSize, "value must be between 1 and 32 (default: 1)");
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                _configuration.RootPath = folderBrowserDialog1.SelectedPath;
                Bind();
            }
        }

        private void Bind()
        {
            textBoxName.DataBindings.Clear();
            textBoxAddress.DataBindings.Clear();
            textBoxPort.DataBindings.Clear();
            textBoxRootPath.DataBindings.Clear();
            textBoxTTL.DataBindings.Clear();
            textBoxTimeout.DataBindings.Clear();
            textBoxRetries.DataBindings.Clear();
            textBoxWindowSize.DataBindings.Clear();

            checkBoxAllowReads.DataBindings.Clear();
            checkBoxAllowWrites.DataBindings.Clear();
            checkBoxAutoCreateSubDirectories.DataBindings.Clear();
            checkBoxSinglePortMode.DataBindings.Clear();
            checkBoxDontFragment.DataBindings.Clear();
            checkBoxConvertPathSeparator.DataBindings.Clear();

            BindingSource bs = new BindingSource(_configuration, null);
            checkBoxAllowReads.DataBindings.Add("Checked", bs, "AllowRead");
            checkBoxAllowWrites.DataBindings.Add("Checked", bs, "AllowWrite");
            checkBoxSinglePortMode.DataBindings.Add("Checked", bs, "SinglePort");
            checkBoxAutoCreateSubDirectories.DataBindings.Add("Checked", bs, "AutoCreateDirectories");
            checkBoxDontFragment.DataBindings.Add("Checked", bs, "DontFragment");
            checkBoxConvertPathSeparator.DataBindings.Add("Checked", bs, "ConvertPathSeparator");
            textBoxTTL.DataBindings.Add("Text", bs, "Ttl");
            textBoxName.DataBindings.Add("Text", bs, "Name");
            textBoxAddress.DataBindings.Add("Text", bs, "EndPoint.Address");
            textBoxPort.DataBindings.Add("Text", bs, "EndPoint.Port");
            textBoxRootPath.DataBindings.Add("Text", bs, "RootPath");
            textBoxTimeout.DataBindings.Add("Text", bs, "Timeout");
            textBoxRetries.DataBindings.Add("Text", bs, "Retries");
            textBoxWindowSize.DataBindings.Add("Text", bs, "WindowSize");
        }

        private void buttonPickAddress_Click(object sender, EventArgs e)
        {
            FormPickAdapter f = new FormPickAdapter();
            if(f.ShowDialog(this) == DialogResult.OK)
            {
                _configuration.EndPoint = new SerializableIPEndPoint(new System.Net.IPEndPoint(f.Address, _configuration.EndPoint.Port));
                Bind();
            }
        }

        private void buttonAlternatives_Click(object sender, EventArgs e)
        {
            FormAlternatives f = new FormAlternatives();
            f.Configuration = _configuration.Alternatives;

            if(f.ShowDialog(this) == DialogResult.OK)
            {
                _configuration.Alternatives = f.Configuration;
                Bind();
            }
        }
    }
}
