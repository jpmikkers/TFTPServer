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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.Configuration;
using System.Reflection;

namespace TFTPServer
{
    public partial class FormMain : Form
    {
        private TFTPServerConfiguration m_ServerConfiguration;
        private TFTPServer m_Server;

        public FormMain()
        {
            InitializeComponent();
            if (Properties.Settings.Default.ServerConfiguration == null)
            {
                Properties.Settings.Default.ServerConfiguration = new TFTPServerConfiguration();
                Properties.Settings.Default.Save();
            }
            m_ServerConfiguration = Properties.Settings.Default.ServerConfiguration;
            Bind();
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
            if (m_Server == null)
            {
                m_Server = new TFTPServer(
                    m_ServerConfiguration.EndPoint,
                    m_ServerConfiguration.SinglePort,
                    (short)m_ServerConfiguration.Ttl,
                    m_ServerConfiguration.DontFragment,
                    m_ServerConfiguration.RootPath,
                    m_ServerConfiguration.AutoCreateDirectories,
                    m_ServerConfiguration.AllowRead,
                    m_ServerConfiguration.AllowWrite, 
                    m_ServerConfiguration.Timeout,
                    m_ServerConfiguration.Retries);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (m_Server != null)
            {
                m_Server.Dispose();
                m_Server = null;
            }
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
    }
}
