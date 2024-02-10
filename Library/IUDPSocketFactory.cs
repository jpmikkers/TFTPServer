using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static CodePlex.JPMikkers.TFTP.UDPSocket;

namespace CodePlex.JPMikkers.TFTP;

public interface IUDPSocketFactory
{
    IUDPSocket Create(IPEndPoint localEndPoint, int packetSize, bool dontFragment, short ttl);
}
