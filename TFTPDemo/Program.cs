
using Baksteen.Net.TFTP.Client;
using Baksteen.Net.TFTP.Server;
using Microsoft.Extensions.Logging;
using System.Net;

using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());

var config = new TFTPServer.Configuration
{
    EndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.120"), 69),
    SinglePort = true,
    Retries = 10,
    ResponseTimeout = TimeSpan.FromSeconds(2),
    RootPath = Path.GetFullPath("."),
    Ttl = 1,
    WindowSize = 1,
    DontFragment = true,
    AllowRead = true,
    AllowWrite = true,
    ConvertPathSeparator = true,
};

var server = await TFTPServer.CreateAndStart(loggerFactory.CreateLogger<TFTPServer>(),null,null,null,null,config);

var client = new TFTPClient(config.EndPoint,
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
await server.Stop();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
