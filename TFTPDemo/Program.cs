
using Baksteen.Net.TFTP.Client;
using CodePlex.JPMikkers.TFTP;
using Microsoft.Extensions.Logging;
using System.Net;

using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());

var server = new TFTPServer(loggerFactory.CreateLogger<TFTPServer>(),null,null);

server.EndPoint = new System.Net.IPEndPoint(IPAddress.Parse("192.168.1.120"), 69);
server.Name = "myserver";
server.SinglePort = true;
//server.SinglePort = false;
server.Retries = 10;
server.ResponseTimeout = TimeSpan.FromSeconds(2);
server.RootPath = Path.GetFullPath(".");
Console.WriteLine($"path is {server.RootPath}");

server.Ttl = 1;
server.WindowSize = 1;
server.DontFragment = true;
server.AllowRead = true;
server.AllowWrite = true;
server.ConvertPathSeparator = true;
server.Start();

var client = new TFTPClient(server.EndPoint,
    new TFTPClient.Settings() {
        BlockSize = 1024, 
        DontFragment = true, 
        ResponseTimeout = TimeSpan.FromMicroseconds(2000), 
        Ttl = 1 ,
        ProgressInterval = TimeSpan.FromSeconds(2.0), 
        Retries = 10, 
    });

for(int t = 0; t < 10; t++)
{
    client.Upload("moogabooga.txt", new MemoryStream(new byte[1024 * 1024]));
    client.Download("moogabooga.txt", new MemoryStream());
}

Console.ReadLine();
server.Stop();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
