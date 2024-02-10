using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CodePlex.JPMikkers.TFTP;

public class RetryingWindowHelper
{
    private readonly IUDPSocket _socket;
    private readonly IPEndPoint _remoteEndPoint;
    private readonly int _maxRetries;
    private readonly List<ReadOnlyMemory<byte>> _packets = [];
    private readonly Stopwatch _lastSendStopwatch = new();
    private int _retryCount;

    public RetryingWindowHelper(IUDPSocket socket, IPEndPoint remoteEndPoint, int maxRetries)
    {
        _socket = socket;
        _remoteEndPoint = remoteEndPoint;
        _maxRetries = maxRetries;
    }

    public async Task Send(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
    {
        _packets.Clear();
        _packets.Add(packet);
        await SendPacketList(cancellationToken);
        ResetRetries();
    }

    public async Task AppendSend(ReadOnlyMemory<byte> packet, CancellationToken cancellationToken)
    {
        _packets.Add(packet);
        await _socket.Send(_remoteEndPoint, packet, cancellationToken);
        ResetRetries();
    }

    private async Task SendPacketList(CancellationToken cancellationToken)
    {
        foreach(var packet in _packets) await _socket.Send(_remoteEndPoint, packet, cancellationToken);
    }

    private void ResetRetries()
    {
        _retryCount = 0;
        _lastSendStopwatch.Restart();
    }

    public async Task<UDPMessage> RetryingReceive(TimeSpan responseTimeout, CancellationToken cancellationToken)
    {
        while(true)
        {
            try
            {
                return await _socket.ReceiveWithTimeout(cancellationToken, responseTimeout - _lastSendStopwatch.Elapsed);
            }
            catch(TimeoutException)
            {
                if(_packets.Count>0 && _retryCount < _maxRetries)
                {
                    await SendPacketList(cancellationToken);
                    _retryCount++;
                    _lastSendStopwatch.Restart();
                }
                else
                {
                    await TFTPServer.SendError(_socket, _remoteEndPoint, TFTPServer.ErrorCode.Undefined, "Timeout", cancellationToken);
                    throw;
                }
            }
        }
    }
}
