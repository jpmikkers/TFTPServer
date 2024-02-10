using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CodePlex.JPMikkers.TFTP;

public class FilteredUDPSocket : IUDPSocket
{
    private readonly IUDPSocket _parent;
    private readonly IPEndPoint _remoteEndPoint;

    public FilteredUDPSocket(IUDPSocket parent, IPEndPoint remoteEndPoint)
    {
        _parent = parent;
        _remoteEndPoint = remoteEndPoint;
    }

    public IPEndPoint LocalEndPoint => _parent.LocalEndPoint;

    public void Dispose()
    {
        _parent.Dispose();
    }

    public async Task<UDPMessage> Receive(CancellationToken cancellationToken)
    {
        UDPMessage message;
        do
        {
            message = await _parent.Receive(cancellationToken);
        } 
        while(!message.EndPoint.Equals(_remoteEndPoint));
        return message;
    }

    public Task Send(IPEndPoint endPoint, ReadOnlyMemory<byte> msg, CancellationToken cancellationToken)
    {
        return _parent.Send(endPoint, msg, cancellationToken);
    }
}
