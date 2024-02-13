using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Baksteen.Net.TFTP.Server;

internal interface ITFTPSession
{
    IPEndPoint LocalEndPoint { get; }
    IPEndPoint RemoteEndPoint { get; }
    string Filename { get; }

    Task Run(CancellationToken cancellationToken);
}
