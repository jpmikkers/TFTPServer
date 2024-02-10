using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CodePlex.JPMikkers.TFTP;


[Serializable]
public class UDPSocketException : Exception
{
    public required bool IsFatal { get; init; }

    public UDPSocketException() 
    { 
    }

    public UDPSocketException(string message) : base(message)
    { 
    }

    public UDPSocketException(string message, Exception inner) : base(message, inner) 
    { 
    }
}

public record class UDPMessage(IPEndPoint EndPoint, ReadOnlyMemory<byte> Data);

public interface IUDPSocket : IDisposable
{
    IPEndPoint LocalEndPoint { get; }

    Task Send(IPEndPoint endPoint, ReadOnlyMemory<byte> msg, CancellationToken cancellationToken);
    Task<UDPMessage> Receive(CancellationToken cancellationToken);
}