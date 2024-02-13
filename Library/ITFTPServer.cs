using System;
using System.Net;

namespace Baksteen.Net.TFTP.Server;

public class TFTPStopEventArgs : EventArgs
{
    public Exception? Reason { get; set; }
}

public interface ITFTPServer : IDisposable
{
    event EventHandler<TFTPStopEventArgs?> OnStatusChange;

    string Name { get; set; }
    IPEndPoint EndPoint { get; set; }
    bool SinglePort { get; set; }
    short Ttl { get; set; }
    bool DontFragment { get; set; }

    TimeSpan ResponseTimeout { get; set; }
    int Retries { get; set; }

    string RootPath { get; set; }
    bool AutoCreateDirectories { get; set; }
    bool ConvertPathSeparator { get; set; }
    bool AllowRead { get; set; }
    bool AllowWrite { get; set; }

    ushort WindowSize { get; set; }

    bool Active { get; }
    int ActiveTransfers { get; }

    void Start();
    void Stop();
}
