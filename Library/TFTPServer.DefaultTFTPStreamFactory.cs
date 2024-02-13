using System;
using System.IO;

namespace Baksteen.Net.TFTP.Server;

public partial class TFTPServer
{
    private class DefaultTFTPStreamFactory : ITFTPStreamFactory
    {
        private readonly TFTPServer _parent;

        public DefaultTFTPStreamFactory(TFTPServer parent)
        {
            _parent = parent;
        }

        public Stream GetReadStream(string filename)
        {
            if(!_parent._allowRead)
            {
                throw new UnauthorizedAccessException("Reading not allowed");
            }
            string targetPath = _parent.GetLocalFilename(filename);
            //Console.WriteLine("Getting read stream for file '{0}'", targetPath);
            return File.OpenRead(targetPath);
        }

        public Stream GetWriteStream(string filename, long length)
        {
            if(!_parent._allowWrite)
            {
                throw new UnauthorizedAccessException("Writing not allowed");
            }

            string targetPath = _parent.GetLocalFilename(filename);
            //Console.WriteLine("Getting write stream for file '{0}', size {1}", targetPath, length);

            if(_parent._autoCreateDirectories)
            {
                var dir = Path.GetDirectoryName(targetPath);
                if(!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            return File.OpenWrite(targetPath);
        }
    }
}
