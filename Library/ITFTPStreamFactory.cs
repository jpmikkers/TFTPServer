using System.IO;

namespace CodePlex.JPMikkers.TFTP;

public interface ITFTPStreamFactory
{
    Stream GetReadStream(string filename);
    Stream GetWriteStream(string filename, long length);
}
