using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baksteen.Net.TFTP.Server;

public partial class TFTPServer : IDisposable
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
    private readonly ILogger _logger;
    internal readonly IUDPSocketFactory _udpSocketFactory;
    private readonly ITFTPSessionInfoFactory _liveSessionInfoFactory;
    private readonly ITFTPServerInfo? _serverCallback;
    private readonly ITFTPStreamFactory _tftpStreamFactory;
    private readonly IChildSocketFactory _childSocketFactory;
    private readonly Configuration _configuration;
    private ForkableUDPSocket _socket = default!;

    internal const int MaxBlockSize = 65464 + 4;
    internal const int DefaultBlockSize = 512;
    private CancellationTokenSource _cancellationTokenSource = new();

    private bool _active = false;
    private Task? _mainTask;
    private readonly Dictionary<IPEndPoint, TFTPSessionRunner> _sessions = [];

    public bool Active
    {
        get
        {
            return _active;
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

    private TFTPServer(
        ILogger<TFTPServer> logger,
        ITFTPStreamFactory? streamFactory,
        IUDPSocketFactory? udpSocketFactory,
        ITFTPSessionInfoFactory? liveSessionInfoFactory,
        ITFTPServerInfo? serverCallback, 
        Configuration configuration)
    {
        _logger = logger;
        _udpSocketFactory = udpSocketFactory ?? new DefaultUDPSocketFactory();
        _liveSessionInfoFactory = liveSessionInfoFactory ?? new DefaultTFTPSessionInfoFactory();
        _serverCallback = serverCallback;
        _tftpStreamFactory = streamFactory ?? new DefaultTFTPStreamFactory(this);
        _childSocketFactory = new ChildSocketFactory(this);
        _configuration = configuration;
    }

    public static async Task<TFTPServer> CreateAndStart(
        ILogger<TFTPServer> logger,
        ITFTPStreamFactory? streamFactory,
        IUDPSocketFactory? udpSocketFactory,
        ITFTPSessionInfoFactory? liveSessionInfoFactory,
        ITFTPServerInfo? serverCallback,
        Configuration configuration)
    {
        var result = new TFTPServer(logger, streamFactory, udpSocketFactory, liveSessionInfoFactory, serverCallback, configuration);
        await result.Start();
        return result;
    }

    private async Task Start()
    {
        if(!_active)
        {
            try
            {
                int maxWorkerThreads, maxCompletionPortThreads;

                _logger.LogInformation("Starting TFTP server '{endpoint}'", _configuration.EndPoint);
                _active = true;
                _socket = new ForkableUDPSocket(_udpSocketFactory.Create(_configuration.EndPoint, MaxBlockSize, _configuration.DontFragment, _configuration.Ttl)); //, OnUDPReceive, OnUDPStop);
                _logger.LogInformation("TFTP Server start succeeded, serving at '{localendpoint}'", _socket.LocalEndPoint);
                ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
                _logger.LogTrace("Threadpool maxWorkerThreads={maxWorkerThreads} maxCompletionPortThreads={maxCompletionPortThreads}", maxWorkerThreads, maxCompletionPortThreads);
                _logger.LogTrace("GCSettings.IsServerGC={isservergc}", System.Runtime.GCSettings.IsServerGC);
                _mainTask = Task.Run(async () => { await MainTask(_cancellationTokenSource.Token); });
            }
            catch(Exception e)
            {
                _logger.LogError("TFTP Server start failed, reason {error}", e);
                _active = false;
                throw;
            }
        }
        await Task.CompletedTask;
    }

    public async Task Stop()
    {
        if(_active)
        {
            _logger.LogInformation("Stopping TFTP server '{endpoint}'", _configuration.EndPoint);
            _active = false;

            _cancellationTokenSource.Cancel();

            if(_mainTask is not null)
            {
                try 
                { 
                    await _mainTask; 
                } 
                catch 
                { 
                }
            }

            _socket.Dispose();
            _logger.LogInformation("Stopped");
        }
    }

    private async Task MainTask(CancellationToken cancellationToken)
    {
        _serverCallback?.Started();
        _logger.LogTrace("MainTask started");

        try
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                var msg = await _socket.Receive(cancellationToken).ConfigureAwait(false);
                (var endPoint, var data) = (msg.EndPoint, msg.Data);

                int packetSize = data.Length;
                MemoryStream ms = new(data.ToArray());

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
                            string filename = ReadZeroTerminatedString(ms, Encoding.ASCII);
                            if(_configuration.ConvertPathSeparator) filename = filename.Replace('/', '\\');
                            _ = ReadMode(ms);
                            var requestedOptions = ReadOptions(ms);

                            var session = new DownloadSession(
                                _liveSessionInfoFactory.Create(),
                                _tftpStreamFactory,
                                _childSocketFactory,
                                endPoint,
                                requestedOptions,
                                filename,
                                _configuration.ResponseTimeout,
                                _configuration.Retries,
                                _configuration.WindowSize
                            );

                            AddAndStartSession(session);
                        }
                        break;

                    case Opcode.WriteRequest:
                        if(!hasSession)
                        {
                            string filename = ReadZeroTerminatedString(ms, Encoding.ASCII);
                            if(_configuration.ConvertPathSeparator) filename = filename.Replace('/', '\\');
                            _ = ReadMode(ms);
                            var requestedOptions = ReadOptions(ms);

                            var session = new UploadSession(
                                _liveSessionInfoFactory.Create(),
                                _tftpStreamFactory,
                                _childSocketFactory,
                                endPoint,
                                requestedOptions,
                                _configuration.ResponseTimeout,
                                _configuration.Retries,
                                filename
                            );

                            AddAndStartSession(session);
                        }
                        break;

                    default:
                        await SendError(_socket, endPoint, (ushort)ErrorCode.UnknownTransferID, "Unknown transfer ID", _cancellationTokenSource.Token);
                        break;
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch(OperationCanceledException)
        {
            _logger.LogTrace("MainTask stopped");
            _serverCallback?.Stopped(null);
        }
        catch(Exception e)
        {
            _logger.LogError("MainTask failed, reason {error}", e);
            _serverCallback?.Stopped(e);
        }
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
        var realRootPath = Path.GetFullPath(_configuration.RootPath);
        string result = Path.GetFullPath(Path.Combine(realRootPath, StripRoot(filename)));
        if(!result.StartsWith(realRootPath))
        {
            throw new ArgumentException("Illegal filename");
        }
        return result;
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
            _ = Stop(); // TODO Fix this
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    #endregion

}
