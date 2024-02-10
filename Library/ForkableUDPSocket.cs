using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CodePlex.JPMikkers.TFTP;

public class ForkableUDPSocket : IUDPSocket
{
    private readonly IUDPSocket _socket;
    private readonly ConcurrentDictionary<IPEndPoint, ChildUDPSocketWrapper> _childSockets = new();

    private class ChildUDPSocketWrapper : IUDPSocket
    {
        private readonly ForkableUDPSocket _parent;
        private readonly IPEndPoint _peerEndPoint;
        private readonly Channel<UDPMessage> _channel;

        public IPEndPoint LocalEndPoint => _parent.LocalEndPoint;

        public ChildUDPSocketWrapper(ForkableUDPSocket parent, IPEndPoint peerEndPoint)
        {
            _parent = parent;
            _peerEndPoint = peerEndPoint;
            _channel = Channel.CreateUnbounded<UDPMessage>(
                new UnboundedChannelOptions 
                { 
                    AllowSynchronousContinuations = false, 
                    SingleReader = false, 
                    SingleWriter = false 
                });
        }

        public void Dispose()
        {
            _parent._childSockets.TryRemove(this._peerEndPoint, out _);
        }

        public async Task<UDPMessage> Receive(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task Send(IPEndPoint endPoint, ReadOnlyMemory<byte> msg, CancellationToken cancellationToken)
        {
            await _parent.Send(endPoint, msg, cancellationToken);
        }

        public async Task ForwardFromParent(IPEndPoint endPoint, ReadOnlyMemory<byte> msg)
        {
            await _channel.Writer.WriteAsync(new(endPoint,msg)).ConfigureAwait(false);
        }
    }

    public IUDPSocket Fork(IPEndPoint peerEndPoint)
    {
        var child = new ChildUDPSocketWrapper(this, peerEndPoint);
        _childSockets[peerEndPoint] = child;
        return child;
    }

    public ForkableUDPSocket(IUDPSocket socket)
    {
        _socket = socket;
    }

    public IPEndPoint LocalEndPoint => _socket.LocalEndPoint;

    public void Dispose()
    {
        // TODO: dispose children?
        _socket.Dispose();
    }

    public async Task<UDPMessage> Receive(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            var tmp = await _socket.Receive(cancellationToken);

            if(_childSockets.TryGetValue(tmp.EndPoint,out var child))
            {
                // its for one of my child sockets.. 
                await child.ForwardFromParent(tmp.EndPoint,tmp.Data);
                continue;
            }
            else
            {
                return tmp;
            }
        }
        throw new OperationCanceledException(cancellationToken);
    }

    public async Task Send(IPEndPoint endPoint, ReadOnlyMemory<byte> msg, CancellationToken cancellationToken)
    {
        await _socket.Send(endPoint, msg, cancellationToken).ConfigureAwait(false);
    }
}
