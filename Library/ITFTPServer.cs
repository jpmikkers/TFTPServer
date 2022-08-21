namespace Baksteen.Net.TFTP.Server;
using System;
using System.Net;

public class TFTPTraceEventArgs : EventArgs
{
    public string Message { get; set; }
}

public class TFTPStopEventArgs : EventArgs
{
    public Exception Reason { get; set; }
}

public interface ITFTPServer : IDisposable
{
    event EventHandler<TFTPTraceEventArgs> OnTrace;
    event EventHandler<TFTPStopEventArgs> OnStatusChange;

    string Name { get; set; }
    IPEndPoint EndPoint { get; set; }
    bool SinglePort { get; set; }
    short Ttl { get; set; }
    bool DontFragment { get; set; }

    int ResponseTimeout { get; set; }
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
