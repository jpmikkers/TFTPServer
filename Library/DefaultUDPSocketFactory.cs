﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Baksteen.Net.TFTP.Server;

internal class DefaultUDPSocketFactory : IUDPSocketFactory
{
    public IUDPSocket Create(IPEndPoint localEndPoint, int packetSize, bool dontFragment, short ttl)
    {
        return new UDPSocket(localEndPoint, packetSize, dontFragment, ttl);
    }
}
