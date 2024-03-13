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

    public static ServerSettings Defaults { 
        get{
            var template = new TFTPServer.Configuration();
            return new()
            {
                AllowDownloads = template.AllowRead,
                AllowUploads = template.AllowWrite,
                AutoCreateDirectories = template.AutoCreateDirectories,
                ConvertPathSeparator = template.ConvertPathSeparator,
                DontFragment = template.DontFragment,
                EndPoint = template.EndPoint,
                ResponseTimeout = (int)template.ResponseTimeout.TotalSeconds,
                Retries = template.Retries,
                RootPath = template.RootPath,
                SinglePort = template.SinglePort,
                TimeToLive = template.Ttl,
                WindowSize = template.WindowSize,
            };
        }
    }
}
