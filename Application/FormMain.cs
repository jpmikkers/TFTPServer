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
using System.ServiceProcess;
using System.Security;
using System.Security.Principal;
using System.IO;
using System.Diagnostics;

namespace TFTPServerApp
{
    public partial class FormMain : Form
    {
        private bool m_HasAdministrativeRight;
        private ServiceController m_Service;
        private DateTime m_TimeFilter;
        private EventLog m_EventLog;

        public FormMain(ServiceController service)
        {
            m_Service = service;
            m_HasAdministrativeRight = Program.HasAdministrativeRight();
            InitializeComponent();

            m_EventLog = new EventLog();
            m_EventLog.BeginInit();
            m_EventLog.EnableRaisingEvents = true;
            m_EventLog.Log = Program.CustomEventLog;
            m_EventLog.Source = Program.CustomEventSource;
            m_EventLog.SynchronizingObject = this;
            m_EventLog.EntryWritten += new System.Diagnostics.EntryWrittenEventHandler(this.eventLog1_EntryWritten);
            m_EventLog.EndInit();
                        
            UpdateServiceStatus();
            timerServiceWatcher.Enabled = true;
            SetTimeFilter(DateTime.Now);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void UpdateServiceStatus()
        {
            m_Service.Refresh();
            //System.Diagnostics.Debug.WriteLine(m_Service.Status.ToString());
            if (!Program.HasAdministrativeRight())
            {
                buttonStart.Enabled = false;
                buttonStop.Enabled = false;
                //buttonConfigure.Enabled = false;
                buttonElevate.Enabled = true;
            }
            else
            {
                buttonStart.Enabled = (m_Service.Status == ServiceControllerStatus.Stopped);
                buttonStop.Enabled = (m_Service.Status == ServiceControllerStatus.Running);
                //buttonConfigure.Enabled = true;
                buttonElevate.Enabled = false;
            }

            toolStripStatusLabel.Text = string.Format("Service status: {0}",m_Service.Status);
        }

        private void StartService()
        {
            try
            {
                m_Service.Start();
            }
            catch (Exception)
            {
            }
            UpdateServiceStatus();
        }

        private void StopService()
        {
            try
            {
                m_Service.Stop();
            }
            catch (Exception)
            {
            }
            UpdateServiceStatus();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            StartService();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopService();
        }

        private void buttonElevate_Click(object sender, EventArgs e)
        {
            if (Program.RunElevated(""))
            {
                Close();
            }
        }

        private void timerServiceWatcher_Tick(object sender, EventArgs e)
        {
            UpdateServiceStatus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog(this);
        }

        private void buttonConfigure_Click(object sender, EventArgs e)
        {
            FormConfigureOverview f = new FormConfigureOverview(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"JPMikkers\\TFTP Server\\Configuration.xml"));
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                UpdateServiceStatus();
                if (m_Service.Status == ServiceControllerStatus.Running)
                {
                    MessageBox.Show("The TFTP Service has to be restarted to enable the new settings.\r\n"+
                        "This will cause any transfers in progress to be aborted.\r\n"+
                        "Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                }
            }
        }

        private void eventLog1_EntryWritten(object sender, System.Diagnostics.EntryWrittenEventArgs e)
        {
            if(InvokeRequired)
            {
                BeginInvoke(new EntryWrittenEventHandler(eventLog1_EntryWritten), sender, e);
                return;
            }
            else
            {
                AddEventLogEntry(e.Entry);
            }
        }

        private void AddEventLogEntry(EventLogEntry entry)
        {
            if (entry.TimeGenerated > m_TimeFilter)
            {
                string entryType;
                switch (entry.EntryType)
                {
                    case EventLogEntryType.Error:
                        entryType = "ERROR";
                        break;

                    case EventLogEntryType.Warning:
                        entryType = "WARNING";
                        break;

                    default:
                    case EventLogEntryType.Information:
                        entryType = "INFO";
                        break;
                }
                textBox1.AppendText(entry.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss.fff") + " : " + entryType + " : " + entry.Message + "\r\n");
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            SetTimeFilter(DateTime.Now);
        }

        private void RebuildLog()
        {
            textBox1.Visible = false;
            //textBox1.Up
            textBox1.Clear();

            foreach (EventLogEntry entry in m_EventLog.Entries)
            {
                AddEventLogEntry(entry);
            }
            textBox1.Visible = true;
        }

        private void buttonShowHistory_Click(object sender, EventArgs e)
        {
            try
            {
                SetTimeFilter(m_TimeFilter.AddDays(-1));
            }
            catch (Exception)
            {
            }
        }

        private void buttonHistoryOneHour_Click(object sender, EventArgs e)
        {
            try
            {
                SetTimeFilter(m_TimeFilter.AddHours(-1));
            }
            catch (Exception)
            {
            }
        }

        private void SetTimeFilter(DateTime filter)
        {
            m_TimeFilter = filter;
            if (m_TimeFilter == DateTime.MinValue)
            {
                labelFilter.Text = string.Format("Showing all logging");
            }
            else
            {
                labelFilter.Text = string.Format("Showing log starting at: {0}", filter.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            RebuildLog();
        }

        private void buttonHistoryAll_Click(object sender, EventArgs e)
        {
            SetTimeFilter(DateTime.MinValue);
        }
    }
}
