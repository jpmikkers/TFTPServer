﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Baksteen.Net.TFTP.Server;

public class UDPSocket : IUDPSocket
{
    // See: http://stackoverflow.com/questions/5199026/c-sharp-async-udp-listener-socketexception

    const uint IOC_IN = 0x80000000;
    const uint IOC_VENDOR = 0x18000000;
    const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

    private bool _disposed;                            // true => object is disposed
    private readonly bool _IPv6;                                // true => it's an IPv6 connection
    private readonly Socket _socket;                            // The active socket
    private readonly int _packetSize;                           // size of packets we'll try to receive
    private readonly IPEndPoint _localEndPoint;

    public IPEndPoint LocalEndPoint => _localEndPoint;

    #region constructors destructor
    public UDPSocket(IPEndPoint localEndPoint, int packetSize, bool dontFragment, short ttl)
    {
        _packetSize = packetSize;
        _disposed = false;

        _IPv6 = (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6);
        _socket = new Socket(localEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        _socket.SendBufferSize = 65536;
        _socket.ReceiveBufferSize = 65536;
        if(!_IPv6) _socket.DontFragment = dontFragment;
        if(ttl >= 0)
        {
            _socket.Ttl = ttl;
        }
        _socket.Bind(localEndPoint);

        _localEndPoint = (IPEndPoint)_socket.LocalEndPoint!;

        try
        {
            _socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        }
        catch(PlatformNotSupportedException)
        {
        }
    }

    ~UDPSocket()
    {
        try
        {
            Dispose(false);
        }
        catch
        {
            // never let any exception escape the finalizer, or else your process will be killed.
        }
    }

    #endregion

    #region public methods, properties

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Implements <see cref="IDisposable.Dispose"/>
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region private methods, properties

    protected virtual void Dispose(bool disposing)
    {
        if(disposing)
        {
            if(!_disposed)
            {
                _disposed = true;

                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                }
                catch(Exception)
                {
                    // socket tends to complain a lot during close. just eat those exceptions.
                }
            }
        }
    }

    public async Task<UDPMessage> Receive(CancellationToken cancellationToken)
    {
        try
        {
            var mem = new Memory<byte>(new byte[_packetSize]);
            var result = await _socket.ReceiveFromAsync(mem, new IPEndPoint(IPAddress.Any, 0), cancellationToken);

            if(result.RemoteEndPoint is IPEndPoint endpoint)
            {
                return new(endpoint, mem[..result.ReceivedBytes]);
            }
            else
            {
                throw new InvalidCastException("unexpected endpoint type");
            }
        }
        catch(SocketException ex) when(ex.SocketErrorCode == SocketError.MessageSize)
        {
            // someone tried to send a message bigger than _maxPacketSize
            // discard it, and start receiving the next packet
            throw new UDPSocketException($"{nameof(Receive)} error: {ex.Message}", ex) { IsFatal = false };
        }
        catch(SocketException ex) when(ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            // ConnectionReset is reported when the remote port wasn't listening.
            // Since we're using UDP messaging we don't care about this -> continue receiving.
            throw new UDPSocketException($"{nameof(Receive)} error: {ex.Message}", ex) { IsFatal = false };
        }
        catch(Exception ex)
        {
            // everything else is fatal
            throw new UDPSocketException($"{nameof(Receive)} error: {ex.Message}", ex) { IsFatal = true };
        }
    }

    /// <summary>
    /// Sends a packet of bytes to the specified EndPoint using an UDP datagram.
    /// </summary>
    /// <param name="endPoint">Target for the data</param>
    /// <param name="msg">Data to send</param>
    public async Task Send(IPEndPoint endPoint, ReadOnlyMemory<byte> msg, CancellationToken cancellationToken)
    {
        try
        {
            await _socket.SendToAsync(msg, endPoint, cancellationToken);
        }
        catch(Exception ex)
        {
            throw new UDPSocketException($"{nameof(Send)}", ex) { IsFatal = true };
        }
    }

    #endregion
}
