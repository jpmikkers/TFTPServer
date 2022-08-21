using Baksteen.Net.TFTP.Server;
using System.Net;

namespace MinimalDemo
{
    /// <summary>
    /// Minimal ad-hoc TFTP server demo
    /// it just serves the contents of the directory the executable is in, at 127.0.0.1:69
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var server = new TFTPServer
            {
                AllowRead = true,
                AllowWrite = true,
                ConvertPathSeparator = true,
                Name = "demo",
                RootPath = Path.GetDirectoryName(Environment.ProcessPath),
                SinglePort = true,
                EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 69),
            };

            server.OnTrace += (s, e) => Console.WriteLine($"Trace: {e.Message}");
            server.Start();
            Console.WriteLine($"Serving '{server.RootPath}' at {server.EndPoint}");
            Console.ReadLine();
            server.Stop();
        }
    }
}