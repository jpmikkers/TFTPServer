using System;
using System.IO;

namespace CodePlex.JPMikkers.TFTP;

public partial class TFTPServer
{
    private class TFTPStreamFactory : ITFTPStreamFactory
    {
        private readonly TFTPServer _parent;

        public TFTPStreamFactory(TFTPServer parent)
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
                string dir = Path.GetDirectoryName(targetPath);
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            return File.OpenWrite(targetPath);
        }
    }
}
