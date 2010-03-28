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
    public partial class FormPickAdapter : Form
    {
        private IPAddress m_Address;

        public IPAddress Address
        {
            get { return m_Address; }
        }

        public FormPickAdapter()
        {
            InitializeComponent();

            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            Console.WriteLine("Interface information for {0}.{1}     ",
                    computerProperties.HostName, computerProperties.DomainName);

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            comboBox1.DisplayMember = "Description";
            foreach (NetworkInterface adapter in nics)
            {
                comboBox1.Items.Add(adapter);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0 && comboBox1.SelectedIndex< comboBox1.Items.Count)
            {
                comboBox2.SelectedIndex = -1;
                comboBox2.Items.Clear();
                NetworkInterface adapter = (NetworkInterface)comboBox1.SelectedItem;
               // comboBox2.DisplayMember = "Address";

                foreach (var uni in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        comboBox2.Items.Add(uni.Address);
                    }
                }

                foreach (var uni in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (uni.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        comboBox2.Items.Add(uni.Address);
                    }
                }

                comboBox2.SelectedIndex = 0;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            m_Address = (IPAddress)comboBox2.SelectedItem;
        }
    }
}
