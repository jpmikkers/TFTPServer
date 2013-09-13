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
using System.Diagnostics;
using System.Threading;
using CodePlex.JPMikkers.TFTP;

namespace TFTPServerApp
{
    public class TFTPServerResurrector : IDisposable
    {
        private const int RetryTime = 30000;
        private readonly object m_Lock;
        private bool m_Disposed;
        private TFTPServerConfiguration m_Config;
        private EventLog m_EventLog;

        private TFTPServer m_Server;
        private Timer m_RetryTimer;

        public TFTPServerResurrector(TFTPServerConfiguration config, EventLog eventLog)
        {
            m_Lock = new object();
            m_Disposed = false;
            m_Config = config;
            m_EventLog = eventLog;
            m_RetryTimer = new Timer(new TimerCallback(x => Resurrect()));
            Resurrect();
        }

        ~TFTPServerResurrector()
        {
            Dispose(false);
        }

        private void Resurrect()
        {
            lock (m_Lock)
            {
                if (!m_Disposed)
                {
                    try
                    {
                        m_Server = new TFTPServer();
                        m_Server.Name = m_Config.Name;
                        m_Server.EndPoint = m_Config.EndPoint;
                        m_Server.SinglePort = m_Config.SinglePort;
                        m_Server.Ttl = (short)m_Config.Ttl;
                        m_Server.DontFragment = m_Config.DontFragment;
                        m_Server.RootPath = m_Config.RootPath;
                        m_Server.AutoCreateDirectories = m_Config.AutoCreateDirectories;
                        m_Server.AllowRead = m_Config.AllowRead;
                        m_Server.AllowWrite = m_Config.AllowWrite;
                        m_Server.ResponseTimeout = m_Config.Timeout;
                        m_Server.Retries = m_Config.Retries;
                        m_Server.ConvertPathSeparator = m_Config.ConvertPathSeparator;
                        m_Server.WindowSize = m_Config.WindowSize;

                        foreach(var alt in m_Config.Alternatives)
                        {
                            TFTPServer.ConfigurationAlternative tftpAlt = alt.IsRegularExpression ? TFTPServer.ConfigurationAlternative.CreateRegex(alt.Filter) : TFTPServer.ConfigurationAlternative.CreateWildcard(alt.Filter);
                            tftpAlt.WindowSize = alt.WindowSize;
                            m_Server.ConfigurationAlternatives.Add(tftpAlt);
                        }

                        m_Server.OnStatusChange += server_OnStatusChange;
                        m_Server.OnTrace += server_OnTrace;
                        m_Server.Start();
                    }
                    catch (Exception)
                    {
                        CleanupAndRetry();
                    }
                }
            }
        }

        private void Log(EventLogEntryType entryType, string msg)
        {
            m_EventLog.WriteEntry(string.Format("{0} : {1}",m_Config.Name,msg),entryType);
        }

        private void server_OnTrace(object sender, TFTPTraceEventArgs e)
        {
            Log(EventLogEntryType.Information,e.Message);
        }

        private void server_OnStatusChange(object sender, TFTPStopEventArgs e)
        {
            TFTPServer server = (TFTPServer)sender;

            if (server.Active)
            {
                Log(EventLogEntryType.Information, string.Format("{0} transfers in progress", server.ActiveTransfers));
            }
            else
            {
                if (e.Reason != null)
                {
                    Log(EventLogEntryType.Error, string.Format("Stopped, reason: {0}", e.Reason));
                }
                CleanupAndRetry();
            }
        }

        private void CleanupAndRetry()
        {
            lock (m_Lock)
            {
                if (!m_Disposed)
                {
                    // stop server
                    if (m_Server != null)
                    {
                        m_Server.OnStatusChange -= server_OnStatusChange;
                        m_Server.OnTrace -= server_OnTrace;
                        m_Server.Dispose();
                        m_Server = null;
                    }
                    // initiate retry timer
                    m_RetryTimer.Change(RetryTime, Timeout.Infinite);
                }
            }
        }

        protected void Dispose(bool disposing)
        {
            lock (m_Lock)
            {
                if (!m_Disposed)
                {
                    m_Disposed = true;

                    m_RetryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_RetryTimer.Dispose();

                    if (m_Server != null)
                    {
                        m_Server.OnStatusChange -= server_OnStatusChange;
                        m_Server.Dispose();
                        m_Server.OnTrace -= server_OnTrace;
                        m_Server = null;
                    }
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion
    }
}
