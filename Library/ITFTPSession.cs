using System;
using System.Net;

namespace CodePlex.JPMikkers.TFTP
{
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
}
