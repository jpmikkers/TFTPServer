using System.Net;

namespace Baksteen.Net.TFTP.Server;

public interface IChildSocketFactory
{
    public (bool ownSocket, IUDPSocket socket) CreateSocket(IPEndPoint remoteEndPoint,int packetSize);
}
