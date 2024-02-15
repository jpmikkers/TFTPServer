using System;
using System.Net;

namespace Baksteen.Net.TFTP.Server;

public partial class TFTPServer
{
    public class Configuration
    {
        public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Loopback, 69);

        public bool SinglePort { get; set; } = false;

        public short Ttl { get; set; } = -1;

        public bool DontFragment { get; set; }

        public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromSeconds(2);

        public int Retries { get; set; } = 5;

        public string RootPath { get; set; } = ".";

        public bool AutoCreateDirectories { get; set; } = true;

        public bool ConvertPathSeparator { get; set; } = true;

        public bool AllowRead { get; set; } = true;

        public bool AllowWrite { get; set; } = true;

        public ushort WindowSize { get; set; } = 1;

    }
}
