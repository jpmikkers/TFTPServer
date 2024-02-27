using System;
using System.Net;

namespace Baksteen.Net.TFTP.Server;

public record class TFTPSessionStartInfo
{
    public long FileLength { get; set; }
    public string Filename { get; set; } = string.Empty;
    public bool IsUpload { get; set; }
    public IPEndPoint LocalEndPoint { get; set; } = new IPEndPoint(IPAddress.Loopback, 0);
    public IPEndPoint RemoteEndPoint { get; set; } = new IPEndPoint(IPAddress.Loopback, 0);
    public DateTime StartTimeUtc { get; set; }
    public int WindowSize { get; set; }
}
