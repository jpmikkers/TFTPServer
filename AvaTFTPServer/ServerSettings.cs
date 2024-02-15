using Baksteen.Net.TFTP.Server;
using System;
using System.Net;

namespace AvaTFTPServer;

public record class ServerSettings
{
    public IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 0);
    public string RootPath { get; set; } = string.Empty;
    public bool AllowDownloads { get; set; }
    public bool AllowUploads { get; set; }
    public bool AutoCreateDirectories { get; set; }
    public bool ConvertPathSeparator { get; set; }
    public bool SinglePort { get; set; }
    public short TimeToLive { get; set; }
    public bool DontFragment { get; set; }
    public int ResponseTimeout { get; set; }
    public int Retries { get; set; }
    public int WindowSize { get; set; }

    public TFTPServer.Configuration ToServerConfig() => new()
    {
        DontFragment = DontFragment,
        ConvertPathSeparator = ConvertPathSeparator,
        AutoCreateDirectories = AutoCreateDirectories,
        AllowWrite = AllowUploads,
        AllowRead = AllowDownloads,
        EndPoint = EndPoint,
        ResponseTimeout = TimeSpan.FromSeconds(ResponseTimeout),
        Retries = Retries,
        RootPath = RootPath,
        SinglePort = SinglePort,
        Ttl = TimeToLive,
        WindowSize = (ushort)WindowSize,
    };

    //public static ServerSettings CreateFromServer(TFTPServer server)
    //{
    //    return new ServerSettings
    //    {
    //        WindowSize = server.WindowSize,
    //        TimeToLive = server.Ttl,
    //        AllowDownloads = server.AllowRead,
    //        AllowUploads = server.AllowWrite,
    //        AutoCreateDirectories = server.AutoCreateDirectories,
    //        ConvertPathSeparator = server.ConvertPathSeparator,
    //        DontFragment = server.DontFragment,
    //        EndPoint = server.EndPoint,
    //        ResponseTimeout = (int)server.ResponseTimeout.TotalSeconds,
    //        Retries = server.Retries,
    //        RootPath = server.RootPath,
    //        SinglePort = server.SinglePort,
    //    };
    //}

    //public void ApplyToServer(TFTPServer server)
    //{
    //    server.EndPoint = EndPoint;
    //    server.ResponseTimeout = TimeSpan.FromSeconds(ResponseTimeout);
    //    server.AllowRead = AllowDownloads;
    //    server.AllowWrite = AllowUploads;
    //    server.AutoCreateDirectories = AutoCreateDirectories;
    //    server.ConvertPathSeparator = ConvertPathSeparator;
    //    server.DontFragment = DontFragment;
    //    server.SinglePort = SinglePort;
    //    server.RootPath = RootPath;
    //    server.Retries = Retries;
    //    server.Ttl = TimeToLive;
    //    server.WindowSize = (ushort)WindowSize;
    //}
}
