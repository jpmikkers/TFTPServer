/*

Copyright (c) 2010 Jean-Paul Mikkers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodePlex.JPMikkers.TFTP;

namespace TFTPService
{
    public partial class FormSettings : Form
    {
        private TFTPServerConfiguration m_Configuration;

        public TFTPServerConfiguration Configuration
        {
            get
            {
                return m_Configuration.Clone();
            }
            set
            {
                m_Configuration = value.Clone();
                Bind();
            }
        }

        public FormSettings()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                m_Configuration.RootPath = folderBrowserDialog1.SelectedPath;
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

            checkBoxAllowReads.DataBindings.Clear();
            checkBoxAllowWrites.DataBindings.Clear();
            checkBoxAutoCreateSubDirectories.DataBindings.Clear();
            checkBoxSinglePortMode.DataBindings.Clear();
            checkBoxDontFragment.DataBindings.Clear();
            checkBoxConvertPathSeparator.DataBindings.Clear();

            checkBoxAllowReads.DataBindings.Add("Checked", m_Configuration, "AllowRead");
            checkBoxAllowWrites.DataBindings.Add("Checked", m_Configuration, "AllowWrite");
            checkBoxSinglePortMode.DataBindings.Add("Checked", m_Configuration, "SinglePort");
            checkBoxAutoCreateSubDirectories.DataBindings.Add("Checked", m_Configuration, "AutoCreateDirectories");
            checkBoxDontFragment.DataBindings.Add("Checked", m_Configuration, "DontFragment");
            checkBoxConvertPathSeparator.DataBindings.Add("Checked", m_Configuration, "ConvertPathSeparator");
            textBoxTTL.DataBindings.Add("Text", m_Configuration, "Ttl");
            textBoxName.DataBindings.Add("Text", m_Configuration, "Name");
            textBoxAddress.DataBindings.Add("Text", m_Configuration, "EndPoint.Address");
            textBoxPort.DataBindings.Add("Text", m_Configuration, "EndPoint.Port");
            textBoxRootPath.DataBindings.Add("Text", m_Configuration, "RootPath");
            textBoxTimeout.DataBindings.Add("Text", m_Configuration, "Timeout");
            textBoxRetries.DataBindings.Add("Text", m_Configuration, "Retries");
        }

        private void buttonPickAddress_Click(object sender, EventArgs e)
        {
            FormPickAdapter f = new FormPickAdapter();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                m_Configuration.EndPoint = new SerializableIPEndPoint( new System.Net.IPEndPoint(f.Address, m_Configuration.EndPoint.Port) );
                Bind();
            }
        }
    }
}
