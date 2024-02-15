using System;

namespace Baksteen.Net.TFTP.Server;

public interface ITFTPServerInfo
{
    void Started();
    void Stopped(Exception? ex);
}
