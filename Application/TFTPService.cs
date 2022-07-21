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
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Threading;
using CodePlex.JPMikkers.TFTP;
using System.Collections;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace TFTPServerApp
{
    public partial class TFTPService : ServiceBase
    {
        private object _sync = new object();
        private EventLog _eventLog;
        private TFTPServerConfigurationList _configuration;
        private List<TFTPServerResurrector> _servers;
        private ServiceHost _selfHost;

        private static object s_serviceInstanceLock = new object();
        private static TFTPService s_serviceInstance;

        public static TFTPService Instance
        {
            get
            {
                lock (s_serviceInstanceLock)
                {
                    return s_serviceInstance;
                }
            }
            private set
            {
                lock (s_serviceInstanceLock)
                {
                    s_serviceInstance = value;
                }
            }
        }

        public TFTPService()
        {
            InitializeComponent();
            _eventLog = new EventLog(Program.CustomEventLog, ".", Program.CustomEventSource);
        }

        protected override void OnStart(string[] args)
        {
            lock(_sync)
            {
                _configuration = TFTPServerConfigurationList.Read(Program.GetConfigurationPath());
                _servers = new List<TFTPServerResurrector>();

                foreach(var config in _configuration)
                {
                    _servers.Add(new TFTPServerResurrector(config, _eventLog));
                }
            }
            Instance = this;

            _selfHost = new ServiceHost(typeof(TFTPServiceContractImpl));

            try
            {
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                _selfHost.AddServiceEndpoint(typeof(ITFTPServiceContract), binding, "net.pipe://localhost/JPMikkers/TFTPServer/Service");
                _selfHost.Open();
            }
            catch(CommunicationException)
            {
                _selfHost.Abort();
            }
        }

        protected override void OnStop()
        {
            try
            {
                // Close the ServiceHostBase to shutdown the service.
                _selfHost.Close();
            }
            catch (CommunicationException)
            {
                _selfHost.Abort();
            }

            Instance = null;

            lock (_sync)
            {
                foreach (var server in _servers)
                {
                    server.Dispose();
                }
                _servers.Clear();
            }
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }

        public List<TFTPServer> GetServers()
        {
            lock (_sync)
            {
                return _servers.Select(x => x.Server).ToList();
            }
        }
    }
}
