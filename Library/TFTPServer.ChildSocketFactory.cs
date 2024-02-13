using System;
using System.Net;

namespace Baksteen.Net.TFTP.Server;

public partial class TFTPServer
{
    private class ChildSocketFactory : IChildSocketFactory
    {
        private readonly TFTPServer _parent;

        public ChildSocketFactory(TFTPServer parent)
        {
            _parent = parent;
        }

        public (bool ownSocket, IUDPSocket socket) CreateSocket(IPEndPoint remoteEndPoint, int packetSize)
        {
            return _parent.SinglePort ? 
                (
                    false, 
                    _parent._socket.Fork(remoteEndPoint)
                ) : 
                (
                    true,
                    new FilteredUDPSocket(
                        _parent._udpSocketFactory.Create(
                            new IPEndPoint(_parent._serverEndPoint.Address, 0),
                            packetSize,
                            _parent.DontFragment,
                            _parent.Ttl
                    ),
                    remoteEndPoint)
                );
        }
    }
}
