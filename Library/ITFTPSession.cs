namespace Baksteen.Net.TFTP.Server;
using System;
using System.Net;

internal interface ITFTPSession : IDisposable
{
    void Start();
    void Stop();
    void ProcessAck(ushort blockNr);
    void ProcessData(ushort blockNr, ArraySegment<byte> data);
    void ProcessError(ushort code, string msg);
    IPEndPoint LocalEndPoint { get; }
    IPEndPoint RemoteEndPoint { get; }
    string Filename { get; }
}
