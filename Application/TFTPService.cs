using CodePlex.JPMikkers.TFTP;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;

namespace TFTPServerApp
{
    public partial class TFTPService : ServiceBase
    {
        private readonly object _sync = new object();
        private readonly EventLog _eventLog;
        private TFTPServerConfigurationList _configuration;
        private List<TFTPServerResurrector> _servers;
        private ServiceHost _selfHost;

        private static readonly object s_serviceInstanceLock = new object();
        private static TFTPService s_serviceInstance;

        public static TFTPService Instance
        {
            get
            {
                lock(s_serviceInstanceLock)
                {
                    return s_serviceInstance;
                }
            }
            private set
            {
                lock(s_serviceInstanceLock)
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
            catch(CommunicationException)
            {
                _selfHost.Abort();
            }

            Instance = null;

            lock(_sync)
            {
                foreach(var server in _servers)
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
            lock(_sync)
            {
                return _servers.Select(x => x.Server).ToList();
            }
        }
    }
}
