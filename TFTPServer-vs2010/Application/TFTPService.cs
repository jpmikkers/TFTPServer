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
        private object m_Sync = new object();
        private EventLog m_EventLog;
        private TFTPServerConfigurationList m_Configuration;
        private List<TFTPServerResurrector> m_Servers;
        private ServiceHost m_SelfHost;

        private static object m_ServiceInstanceLock = new object();
        private static TFTPService m_ServiceInstance;

        public static TFTPService Instance
        {
            get
            {
                lock (m_ServiceInstanceLock)
                {
                    return m_ServiceInstance;
                }
            }
            private set
            {
                lock (m_ServiceInstanceLock)
                {
                    m_ServiceInstance = value;
                }
            }
        }

        public TFTPService()
        {
            InitializeComponent();
            m_EventLog = new EventLog(Program.CustomEventLog, ".", Program.CustomEventSource);
        }

        protected override void OnStart(string[] args)
        {
            lock (m_Sync)
            {
                m_Configuration = TFTPServerConfigurationList.Read(Program.GetConfigurationPath());
                m_Servers = new List<TFTPServerResurrector>();

                foreach (var config in m_Configuration)
                {
                    m_Servers.Add(new TFTPServerResurrector(config, m_EventLog));
                }
            }
            Instance = this;

            m_SelfHost = new ServiceHost(typeof(TFTPServiceContractImpl));

            try
            {
                NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                m_SelfHost.AddServiceEndpoint(typeof(ITFTPServiceContract), binding, "net.pipe://localhost/JPMikkers/TFTPServer/Service");
                m_SelfHost.Open();
            }
            catch (CommunicationException)
            {
                m_SelfHost.Abort();
            }
        }

        protected override void OnStop()
        {
            try
            {
                // Close the ServiceHostBase to shutdown the service.
                m_SelfHost.Close();
            }
            catch (CommunicationException)
            {
                m_SelfHost.Abort();
            }

            Instance = null;

            lock (m_Sync)
            {
                foreach (var server in m_Servers)
                {
                    server.Dispose();
                }
                m_Servers.Clear();
            }
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }

        public List<TFTPServer> GetServers()
        {
            lock (m_Sync)
            {
                return m_Servers.Select(x => x.Server).ToList();
            }
        }
    }
}
