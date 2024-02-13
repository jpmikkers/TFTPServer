using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Baksteen.Net.TFTP.Server.UDPSocket;

namespace Baksteen.Net.TFTP.Server;

public interface IUDPSocketFactory
{
    IUDPSocket Create(IPEndPoint localEndPoint, int packetSize, bool dontFragment, short ttl);
}
