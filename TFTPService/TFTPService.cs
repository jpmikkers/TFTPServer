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

namespace TFTPService
{
    public partial class TFTPService : ServiceBase
    {
        private TFTPServerConfigurationList m_Configuration;
        private List<TFTPServer> m_Servers;

        public TFTPService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            m_Configuration = TFTPServerConfigurationList.Read(Program.GetConfigurationPath());
            m_Servers = new List<TFTPServer>();

            foreach (var config in m_Configuration)
            {
                TFTPServer server = new TFTPServer();
                server.EndPoint = config.EndPoint;
                server.SinglePort = config.SinglePort;
                server.Ttl = (short)config.Ttl;
                server.DontFragment = config.DontFragment;
                server.RootPath = config.RootPath;
                server.AutoCreateDirectories = config.AutoCreateDirectories;
                server.AllowRead = config.AllowRead;
                server.AllowWrite = config.AllowWrite;
                server.ResponseTimeout = config.Timeout;
                server.Retries = config.Retries;
                server.ConvertPathSeparator = config.ConvertPathSeparator;
                server.OnStatusChange += server_OnStatusChange;
                server.OnTrace += server_OnTrace;
                m_Servers.Add(server);
            }

            foreach (var server in m_Servers)
            {
                try
                {
                    server.Start();
                }
                catch (Exception e)
                {
                    server.OnStatusChange -= server_OnStatusChange;
                    server.OnTrace -= server_OnTrace;
                }
            }
        }

        private void server_OnTrace(object sender, TFTPTraceEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(e.Message);
            eventLog1.WriteEntry(e.Message);
        }

        private void server_OnStatusChange(object sender, TFTPStopEventArgs e)
        {
            TFTPServer server = (TFTPServer)sender;
            //System.Diagnostics.Debug.WriteLine(string.Format("{0}, {1} transfers", server.Active ? "Active" : "Stopped", server.ActiveTransfers));
            eventLog1.WriteEntry(string.Format("{0}, {1} transfers", server.Active ? "Active" : "Stopped", server.ActiveTransfers));
        }

        protected override void OnStop()
        {
            foreach (var server in m_Servers)
            {
                server.Stop();
                server.OnStatusChange -= server_OnStatusChange;
                server.OnTrace -= server_OnTrace;
            }
            m_Servers.Clear();
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
        }
    }
}
