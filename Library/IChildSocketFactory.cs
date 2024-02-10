using System.Net;

namespace CodePlex.JPMikkers.TFTP;

public interface IChildSocketFactory
{
    public (bool ownSocket, IUDPSocket socket) CreateSocket(IPEndPoint remoteEndPoint,int packetSize);
}
