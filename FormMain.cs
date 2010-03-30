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
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.Configuration;
using System.Reflection;
using CodePlex.JPMikkers.TFTP;

namespace CodePlex.JPMikkers
{
    public partial class FormMain : Form
    {
        private TFTPServerConfiguration m_ServerConfiguration;
        private TFTPServer m_Server = new TFTPServer();

        public FormMain()
        {
            InitializeComponent();
            m_Server.OnStop += new Action<ITFTPServer, Exception>(m_Server_OnStop);

            if (Properties.Settings.Default.ServerConfiguration == null)
            {
                var serverConfiguration = new TFTPServerConfiguration();
                serverConfiguration.AllowRead = m_Server.AllowRead;
                serverConfiguration.AllowWrite = m_Server.AllowWrite;
                serverConfiguration.AutoCreateDirectories = m_Server.AutoCreateDirectories;
                serverConfiguration.DontFragment = m_Server.DontFragment;
                serverConfiguration.EndPoint = m_Server.EndPoint;
                serverConfiguration.Retries = m_Server.Retries;
                serverConfiguration.RootPath = m_Server.RootPath;
                serverConfiguration.SinglePort = m_Server.SinglePort;
                serverConfiguration.Timeout = m_Server.ResponseTimeout;
                serverConfiguration.Ttl = m_Server.Ttl;
                Properties.Settings.Default.ServerConfiguration = serverConfiguration;
                Properties.Settings.Default.Save();
            }
            m_ServerConfiguration = Properties.Settings.Default.ServerConfiguration;
            Bind();
        }

        private void m_Server_OnStop(ITFTPServer arg1, Exception arg2)
        {
            this.Marshal(()=>UpdateStatus());
        }

        private void Bind()
        {
            textBoxAddress.DataBindings.Clear();
            textBoxPort.DataBindings.Clear();
            textBoxRootPath.DataBindings.Clear();
            textBoxAddress.DataBindings.Add("Text", m_ServerConfiguration, "EndPoint.Address");
            textBoxPort.DataBindings.Add("Text", m_ServerConfiguration, "EndPoint.Port");
            textBoxRootPath.DataBindings.Add("Text", m_ServerConfiguration, "RootPath");
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            m_Server.EndPoint = m_ServerConfiguration.EndPoint;
            m_Server.SinglePort = m_ServerConfiguration.SinglePort;
            m_Server.Ttl = (short)m_ServerConfiguration.Ttl;
            m_Server.DontFragment = m_ServerConfiguration.DontFragment;
            m_Server.RootPath = m_ServerConfiguration.RootPath;
            m_Server.AutoCreateDirectories = m_ServerConfiguration.AutoCreateDirectories;
            m_Server.AllowRead = m_ServerConfiguration.AllowRead;
            m_Server.AllowWrite = m_ServerConfiguration.AllowWrite;
            m_Server.ResponseTimeout = m_ServerConfiguration.Timeout;
            m_Server.Retries = m_ServerConfiguration.Retries;
            try
            {
                m_Server.Start();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            UpdateStatus();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            m_Server.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormSettings f = new FormSettings();
            f.Configuration = m_ServerConfiguration;
            if (f.ShowDialog(this)==DialogResult.OK)
            {
                m_ServerConfiguration = f.Configuration;
                Bind();
                Properties.Settings.Default.ServerConfiguration = m_ServerConfiguration;
                Properties.Settings.Default.Save();
            }
        }

        private void UpdateStatus()
        {
            toolStripStatusLabel1.Text = m_Server.Active ? "Active" : "Stopped";
        }
    }
}
