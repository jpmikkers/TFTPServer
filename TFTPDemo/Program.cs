
using Baksteen.Net.TFTP.Client;
using CodePlex.JPMikkers.TFTP;
using System.Net;

var server = new TFTPServer(null,null);
server.OnTrace += Server_OnTrace;

void Server_OnTrace(object? sender, TFTPTraceEventArgs e)
{
    Console.WriteLine($"SERVER: {e.Message}");
}

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
        BlockSize = 512, 
        DontFragment = true, 
        ResponseTimeout = TimeSpan.FromMicroseconds(2000), 
        Ttl = 1 ,
        ProgressInterval = TimeSpan.FromSeconds(2.0), 
        Retries = 10, 
    });

for(int t = 0; t < 10; t++)
{
    Console.WriteLine("before upload");
    client.Upload("moogabooga.txt", new MemoryStream(new byte[1024 * 1024]));
    Console.WriteLine("after upload");

    Console.WriteLine("before download");
    client.Download("moogabooga.txt", new MemoryStream());
    Console.WriteLine("after download");
}

Console.ReadLine();
server.Stop();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
