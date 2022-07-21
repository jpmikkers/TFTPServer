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
        private readonly object _lock;
        private bool _disposed;
        private TFTPServerConfiguration _config;
        private EventLog _eventLog;

        private TFTPServer _server;
        private Timer _retryTimer;

        public TFTPServerResurrector(TFTPServerConfiguration config, EventLog eventLog)
        {
            _lock = new object();
            _disposed = false;
            _config = config;
            _eventLog = eventLog;
            _retryTimer = new Timer(new TimerCallback(x => Resurrect()));
            Resurrect();
        }

        ~TFTPServerResurrector()
        {
            Dispose(false);
        }

        public TFTPServer Server
        {
            get
            {
                lock (_lock)
                {
                    return _server;
                }
            }
        }

        private void Resurrect()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    try
                    {
                        _server = new TFTPServer();
                        _server.Name = _config.Name;
                        _server.EndPoint = _config.EndPoint;
                        _server.SinglePort = _config.SinglePort;
                        _server.Ttl = (short)_config.Ttl;
                        _server.DontFragment = _config.DontFragment;
                        _server.RootPath = _config.RootPath;
                        _server.AutoCreateDirectories = _config.AutoCreateDirectories;
                        _server.AllowRead = _config.AllowRead;
                        _server.AllowWrite = _config.AllowWrite;
                        _server.ResponseTimeout = _config.Timeout;
                        _server.Retries = _config.Retries;
                        _server.ConvertPathSeparator = _config.ConvertPathSeparator;
                        _server.WindowSize = _config.WindowSize;

                        foreach(var alt in _config.Alternatives)
                        {
                            TFTPServer.ConfigurationAlternative tftpAlt = alt.IsRegularExpression ? TFTPServer.ConfigurationAlternative.CreateRegex(alt.Filter) : TFTPServer.ConfigurationAlternative.CreateWildcard(alt.Filter);
                            tftpAlt.WindowSize = alt.WindowSize;
                            _server.ConfigurationAlternatives.Add(tftpAlt);
                        }

                        _server.OnStatusChange += server_OnStatusChange;
                        _server.OnTrace += server_OnTrace;
                        _server.Start();
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
            _eventLog.WriteEntry($"{_config.Name} : {msg}",entryType);
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
                Log(EventLogEntryType.Information, $"{server.ActiveTransfers} transfers in progress");
            }
            else
            {
                if (e.Reason != null)
                {
                    Log(EventLogEntryType.Error, $"Stopped, reason: {e.Reason}");
                }
                CleanupAndRetry();
            }
        }

        private void CleanupAndRetry()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    // stop server
                    if (_server != null)
                    {
                        _server.OnStatusChange -= server_OnStatusChange;
                        _server.OnTrace -= server_OnTrace;
                        _server.Dispose();
                        _server = null;
                    }
                    // initiate retry timer
                    _retryTimer.Change(RetryTime, Timeout.Infinite);
                }
            }
        }

        protected void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _disposed = true;

                    _retryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _retryTimer.Dispose();

                    if (_server != null)
                    {
                        _server.OnStatusChange -= server_OnStatusChange;
                        _server.Dispose();
                        _server.OnTrace -= server_OnTrace;
                        _server = null;
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
