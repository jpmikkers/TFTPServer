using System.IO;

namespace Baksteen.Net.TFTP.Server;

public interface ITFTPStreamFactory
{
    Stream GetReadStream(string filename);
    Stream GetWriteStream(string filename, long length);
}
