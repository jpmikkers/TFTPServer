using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baksteen.Net.TFTP.Server;

public partial class TFTPServer : ITFTPServer
{
    internal enum ErrorCode : ushort
    {
        Undefined = 0,
        FileNotFound,
        AccessViolation,
        DiskFull,
        IllegalOperation,
        UnknownTransferID,
        FileAlreadyExists,
        NoSuchUser
    }

    private enum Mode
    {
        NetAscii,
        Octet,
        Mail
    }

    public enum Opcode : ushort
    {
        ReadRequest = 1,
        WriteRequest,
        Data,
        Ack,
        Error,
        OptionsAck
    }

    internal const string Option_Multicast = "multicast";
    internal const string Option_Timeout = "timeout";
    internal const string Option_TransferSize = "tsize";
    internal const string Option_BlockSize = "blksize";

    private string _name;
    private readonly ILogger _logger;
    internal readonly IUDPSocketFactory _udpSocketFactory;
    private readonly ITFTPLiveSessionInfoFactory _liveSessionInfoFactory;
    private readonly ITFTPStreamFactory _tftpStreamFactory;
    private readonly IChildSocketFactory _childSocketFactory;
    private ForkableUDPSocket _socket = default!;

    internal const int MaxBlockSize = 65464 + 4;
    internal const int DefaultBlockSize = 512;
    internal IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 69);
    internal short _Ttl = -1;
    internal bool _dontFragment = false;
    internal int _maxRetries = 5;
    private bool _useSinglePort = false;
    private string _rootPath = ".";
    private bool _allowRead = true;
    private bool _allowWrite = true;
    private bool _autoCreateDirectories = true;
    private bool _convertPathSeparator = true;
    private ushort _windowSize = 1;
    private readonly List<ConfigurationAlternative> _configurationAlternatives = new List<ConfigurationAlternative>();
    private CancellationTokenSource _cancellationTokenSource = new();

    private readonly object _sync = new object();
    private bool _active = false;
    private Task? _mainTask;
    private readonly Dictionary<IPEndPoint, TFTPSessionRunner> _sessions;

    public void Start()
    {
        lock(_sync)
        {
            if(!_active)
            {
                try
                {
                    int maxWorkerThreads, maxCompletionPortThreads;

                    Trace($"Starting TFTP server '{_serverEndPoint}'");
                    _active = true;
                    _socket = new ForkableUDPSocket(_udpSocketFactory.Create(_serverEndPoint, MaxBlockSize, _dontFragment, _Ttl)); //, OnUDPReceive, OnUDPStop);
                    Trace($"TFTP Server start succeeded, serving at '{_socket.LocalEndPoint}'");
                    System.Threading.ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
                    Trace($"Threadpool maxWorkerThreads={maxWorkerThreads} maxCompletionPortThreads={maxCompletionPortThreads}");
                    Trace($"GCSettings.IsServerGC={System.Runtime.GCSettings.IsServerGC}");
                    _mainTask = Task.Run(async () => { await MainTask(_cancellationTokenSource.Token); });
                }
                catch(Exception e)
                {
                    Trace($"TFTP Server start failed, reason '{e}'");
                    _active = false;
                    throw;
                }
            }
        }
    }

    public void Stop()
    {
        Stop(null);
    }

    private void Stop(Exception? reason)
    {
        bool notify = false;

        lock(_sync)
        {
            if(_active)
            {
                Trace($"Stopping TFTP server '{_serverEndPoint}'");
                _active = false;

                _cancellationTokenSource.Cancel();

                if(_mainTask is not null)
                {
                    try { _mainTask.GetAwaiter().GetResult(); } catch { };
                }

                notify = true;
                _socket.Dispose();
                Trace("Stopped");
            }
        }

        if(notify)
        {
            var data = new TFTPStopEventArgs();
            data.Reason = reason;
            OnStatusChange(this, data);
        }
    }

    private async Task MainTask(CancellationToken cancellationToken)
    {
        Trace("MainTask started");
        while(!cancellationToken.IsCancellationRequested)
        {
            var msg = await _socket.Receive(cancellationToken).ConfigureAwait(false);
            (var endPoint,var data) = (msg.EndPoint, msg.Data);

            bool notify = false;
            int packetSize = data.Length;
            MemoryStream ms = new MemoryStream(data.ToArray());

            ushort opCode = ReadUInt16(ms);

            bool hasSession = false;
            lock(_sessions)
            {
                hasSession = _sessions.ContainsKey(endPoint);
            }

            // no session in progress for the endpoint that sent the packet
            switch((Opcode)opCode)
            {
                case Opcode.ReadRequest:
                    if(!hasSession)
                    {
                        string filename = ReadZString(ms);
                        if(_convertPathSeparator) filename = filename.Replace('/', '\\');
                        _ = ReadMode(ms);
                        var requestedOptions = ReadOptions(ms);
                        ushort windowSize = GetWindowSize(filename);

                        var session = new DownloadSession(
                            _liveSessionInfoFactory.Create(),
                            _tftpStreamFactory,
                            _childSocketFactory,
                            endPoint,
                            requestedOptions,
                            filename,
                            ResponseTimeout,
                            _maxRetries,
                            windowSize
                        );

                        AddAndStartSession(session);
                        notify = true;
                    }
                    break;

                case Opcode.WriteRequest:
                    if(!hasSession)
                    {
                        string filename = ReadZString(ms);
                        if(_convertPathSeparator) filename = filename.Replace('/', '\\');
                        _ = ReadMode(ms);
                        var requestedOptions = ReadOptions(ms);

                        var session = new UploadSession(
                            _liveSessionInfoFactory.Create(),
                            _tftpStreamFactory,
                            _childSocketFactory,
                            endPoint,
                            requestedOptions,
                            ResponseTimeout,
                            _maxRetries,
                            filename
                        );

                        AddAndStartSession(session);
                        notify = true;
                    }
                    break;

                default:
                    await SendError(_socket, endPoint, (ushort)ErrorCode.UnknownTransferID, "Unknown transfer ID", _cancellationTokenSource.Token);
                    break;
            }

            if(notify)
            {
                OnStatusChange(this, null);
            }
        }
        Trace("MainTask stopped");
    }

    private void AddAndStartSession(ITFTPSession session)
    {
        var sr = new TFTPSessionRunner()
        {
            Session = session,
        };

        lock(_sessions)
        {
            _sessions.Add(sr.Session.RemoteEndPoint, sr);
        }

        sr.Start((x, ex) =>
        {
            lock(_sessions)
            {
                if(ex == null)
                {
                    Trace($"Completed transfer {(session is UploadSession ? "from" : "to")} '{session.RemoteEndPoint}'");
                }
                else
                {
                    Trace($"Aborted transfer {(session is UploadSession ? "from" : "to")} '{session.RemoteEndPoint}', reason '{ex}'");
                }
                _sessions.Remove(x.RemoteEndPoint);
            }
            OnStatusChange(this, null);
        }, _cancellationTokenSource.Token);

        if(session is UploadSession)
        {
            Trace($"Starting transfer of file '{sr.Session.Filename}' from remote '{sr.Session.RemoteEndPoint}' to local '{sr.Session.LocalEndPoint}'");
        }
        else
        {
            Trace($"Starting transfer of file '{sr.Session.Filename}' from local '{sr.Session.LocalEndPoint}' to remote '{sr.Session.RemoteEndPoint}'");
        }
    }

    private static string StripRoot(string filename)
    {
        // strip root from filename before calling Path.Combine(). Some clients like to prepend a leading backslash, resulting in an 'Illegal filename' error.
        if(Path.IsPathRooted(filename))
        {
            filename = filename.Substring(Path.GetPathRoot(filename)?.Length ?? 0);
        }
        return filename;
    }

    private string GetLocalFilename(string filename)
    {
        string result = Path.GetFullPath(Path.Combine(_rootPath, StripRoot(filename)));
        if(!result.StartsWith(_rootPath))
        {
            throw new ArgumentException("Illegal filename");
        }
        return result;
    }

    public TFTPServer(
        ILogger logger,
        ITFTPStreamFactory? streamFactory,
        IUDPSocketFactory? udpSocketFactory,
        ITFTPLiveSessionInfoFactory? liveSessionInfoFactory)
    {
        _name = "TFTPServer";
        _sessions = [];
        _logger = logger;
        _udpSocketFactory = udpSocketFactory ?? new DefaultUDPSocketFactory();
        _liveSessionInfoFactory = liveSessionInfoFactory ?? new DefaultTFTPLiveSessionInfoFactory();
        _tftpStreamFactory = streamFactory ?? new DefaultTFTPStreamFactory(this);
        _childSocketFactory = new ChildSocketFactory(this);
    }

    private ushort GetWindowSize(string filename)
    {
        if(_configurationAlternatives.Count > 0)
        {
            filename = StripRoot(filename);

            foreach(ConfigurationAlternative alternative in _configurationAlternatives)
            {
                if(alternative.Match(filename))
                {
                    return alternative.WindowSize;
                }
            }
        }
        return _windowSize;
    }

    private void OnUDPStop(UDPSocket sender, Exception reason)
    {
        Stop(reason);
    }

    internal void Trace(string msg)
    {
        _logger?.LogInformation(msg);
    }

    #region Dispose pattern

    ~TFTPServer()
    {
        try
        {
            Dispose(false);
        }
        catch
        {
            // never let any exception escape the finalizer, or else your process will be killed.
        }
    }

    protected void Dispose(bool disposing)
    {
        if(disposing)
        {
            Stop();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    #endregion

    public event EventHandler<TFTPStopEventArgs?> OnStatusChange = (sender, data) => { };

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public IPEndPoint EndPoint
    {
        get
        {
            return _serverEndPoint;
        }
        set
        {
            _serverEndPoint = value;
        }
    }

    public bool SinglePort
    {
        get
        {
            return _useSinglePort;
        }
        set
        {
            _useSinglePort = value;
        }
    }

    public short Ttl
    {
        get
        {
            return _Ttl;
        }
        set
        {
            _Ttl = value;
        }
    }

    public bool DontFragment
    {
        get
        {
            return _dontFragment;
        }
        set
        {
            _dontFragment = value;
        }
    }

    public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromSeconds(2);

    public int Retries
    {
        get
        {
            return _maxRetries;
        }
        set
        {
            _maxRetries = value;
        }
    }

    public string RootPath
    {
        get
        {
            return _rootPath;
        }
        set
        {
            _rootPath = Path.GetFullPath(value);
        }
    }

    public bool AutoCreateDirectories
    {
        get
        {
            return _autoCreateDirectories;
        }
        set
        {
            _autoCreateDirectories = value;
        }
    }

    public bool ConvertPathSeparator
    {
        get
        {
            return _convertPathSeparator;
        }
        set
        {
            _convertPathSeparator = value;
        }
    }

    public bool AllowRead
    {
        get
        {
            return _allowRead;
        }
        set
        {
            _allowRead = value;
        }
    }

    public bool AllowWrite
    {
        get
        {
            return _allowWrite;
        }
        set
        {
            _allowWrite = value;
        }
    }

    public ushort WindowSize
    {
        get
        {
            return _windowSize;
        }
        set
        {
            _windowSize = Clip<ushort>(value, 1, 32);
        }
    }

    public IList<ConfigurationAlternative> ConfigurationAlternatives
    {
        get
        {
            return _configurationAlternatives;
        }
    }

    public bool Active
    {
        get
        {
            lock(_sync)
            {
                return _active;
            }
        }
    }

    public int ActiveTransfers
    {
        get
        {
            lock(_sessions)
            {
                return _sessions.Count;
            }
        }
    }

    #region Static helpers

    internal static T Clip<T>(T value, T minValue, T maxValue) where T : IComparable<T>
    {
        T result;
        if(value.CompareTo(minValue) < 0)
            result = minValue;
        else if(value.CompareTo(maxValue) > 0)
            result = maxValue;
        else
            result = value;
        return result;
    }

    internal static Dictionary<string, string> ReadOptions(Stream s)
    {
        Dictionary<string, string> options = [];
        while(s.Position < s.Length)
        {
            string key = ReadZString(s).ToLower();
            string val = ReadZString(s).ToLower();
            options.Add(key, val);
        }
        return options;
    }

    internal static async Task SendError(IUDPSocket socket, IPEndPoint endPoint, ushort code, string message, CancellationToken cancellationToken)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.Error);
        WriteUInt16(ms, code);
        WriteZString(ms, message.Substring(0, Math.Min(message.Length, 256)));
        await socket.Send(endPoint, ms.ToArray(), cancellationToken);
    }

    internal static async Task SendError(IUDPSocket socket, IPEndPoint endPoint, ErrorCode code, string message, CancellationToken cancellationToken)
    {
        await SendError(socket, endPoint, (ushort)code, message, cancellationToken);
    }

    internal static ReadOnlyMemory<byte> GetDataAckPacket(ushort blockno)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.Ack);
        WriteUInt16(ms, blockno);
        return ms.ToArray();
    }

    internal static ReadOnlyMemory<byte> GetOptionsAckPacket(Dictionary<string, string> options)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.OptionsAck);
        foreach(var s in options)
        {
            WriteZString(ms, s.Key);
            WriteZString(ms, s.Value);
        }
        return ms.ToArray();
    }

    internal static ReadOnlyMemory<byte> GetDataPacket(ushort blockno, byte[] data, int dataSize)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.Data);
        WriteUInt16(ms, blockno);
        ms.Write(data, 0, dataSize);
        return ms.ToArray();
    }

    internal static string ReadZString(Stream s)
    {
        StringBuilder sb = new();
        int c = s.ReadByte();
        while(c != 0)
        {
            sb.Append((char)c);
            c = s.ReadByte();
        }
        return sb.ToString();
    }

    internal static void WriteZString(Stream s, string msg)
    {
        TextWriter tw = new StreamWriter(s, Encoding.ASCII);
        tw.Write(msg);
        tw.Flush();
        s.WriteByte(0);
    }

    private static Mode ReadMode(Stream s)
    {
        Mode result;
        switch(ReadZString(s).ToLower())
        {
            case "netascii":
                result = Mode.NetAscii;
                break;

            case "octet":
                result = Mode.Octet;
                break;

            case "mail":
                result = Mode.Mail;
                break;

            default:
                throw new InvalidDataException("Invalid mode");
        }
        return result;
    }

    internal static ushort ReadUInt16(Stream s)
    {
        BinaryReader br = new(s);
        return (ushort)IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
    }

    internal static void WriteUInt16(Stream s, ushort v)
    {
        BinaryWriter bw = new(s);
        bw.Write((ushort)IPAddress.HostToNetworkOrder((short)v));
    }
    #endregion
}
