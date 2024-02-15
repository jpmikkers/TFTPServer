using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Baksteen.Net.TFTP.Server;


internal abstract class TFTPSession : ITFTPSession
{
    protected readonly ITFTPSessionInfo _info;
    protected readonly ITFTPStreamFactory _streamFactory;
    protected volatile bool _disposed = false;
    protected readonly TimeSpan _responseTimeout;
    private readonly int _maxRetries;
    protected Stream _stream = Stream.Null;
    protected IUDPSocket _socket;
    protected bool _ownSocket;
    protected IPEndPoint _localEndPoint;
    protected IPEndPoint _remoteEndPoint;
    protected readonly TimeSpan _socketDisposeDelay = TimeSpan.FromMilliseconds(500);
    protected long _length;
    protected int _currentBlockSize;
    protected bool _lastBlock;
    protected ushort _blockNumber;
    protected readonly Dictionary<string, string> _requestedOptions;
    protected string _filename;
    protected Dictionary<string, string> _acceptedOptions = [];
    protected CancellationTokenSource _cancellationTokenSource = new();
    protected bool _dally = true;

    public IPEndPoint LocalEndPoint
    {
        get
        {
            return _localEndPoint;
        }
    }

    public IPEndPoint RemoteEndPoint
    {
        get
        {
            return _remoteEndPoint;
        }
    }

    public string Filename
    {
        get
        {
            return _filename;
        }
    }

    public TFTPSession(
        ITFTPSessionInfo info,
        ITFTPStreamFactory streamFactory,
        IChildSocketFactory childSocketFactory,
        IPEndPoint remoteEndPoint,
        Dictionary<string, string> requestedOptions,
        string filename,
        TimeSpan responseTimeout,
        int maxRetries)
    {
        _info = info;
        _streamFactory = streamFactory;
        _currentBlockSize = TFTPServer.DefaultBlockSize;
        _responseTimeout = responseTimeout;
        _maxRetries = maxRetries;
        _remoteEndPoint = remoteEndPoint;
        _requestedOptions = requestedOptions;
        _filename = filename;

        _length = 0;
        _lastBlock = false;
        _blockNumber = 0;

        foreach(var kvp in _requestedOptions)
        {
            switch(kvp.Key)
            {
                case TFTPServer.Option_Multicast:
                    // not supported
                    break;

                case TFTPServer.Option_Timeout:
                    //Console.WriteLine("Timeout of {0}", kvp.Value);
                    int requestedTimeout = int.Parse(kvp.Value);
                    // rfc2349 : valid values range between "1" and "255" seconds
                    if(requestedTimeout >= 1 && requestedTimeout <= 255)
                    {
                        _responseTimeout = TimeSpan.FromSeconds(requestedTimeout);
                        _acceptedOptions.Add(TFTPServer.Option_Timeout, kvp.Value);
                    }
                    break;

                case TFTPServer.Option_TransferSize:
                    // handled in inherited classes
                    break;

                case TFTPServer.Option_BlockSize:
                    //Console.WriteLine("Blocksize of {0}", kvp.Value);
                    int requestedBlockSize = int.Parse(kvp.Value);
                    // rfc2348 : valid values range between "8" and "65464" octets, inclusive
                    if(requestedBlockSize >= 8 && requestedBlockSize <= 65464)
                    {
                        _currentBlockSize = Math.Min(TFTPServer.MaxBlockSize, requestedBlockSize);
                        _acceptedOptions.Add(TFTPServer.Option_BlockSize, _currentBlockSize.ToString());
                    }
                    break;
            }
        }

        (_ownSocket, _socket) = childSocketFactory.CreateSocket(remoteEndPoint, _currentBlockSize + 4);
        _localEndPoint = _socket.LocalEndPoint;
    }

    protected abstract Task MainTask(CancellationToken cancellationToken);

    public async Task Run(CancellationToken cancellationToken)
    {
        try
        {
            await MainTask(cancellationToken);
        }
        finally
        {
            if(_ownSocket && _dally)
            {
                _ = Task.Run(async () => { 
                    await Task.Delay(_socketDisposeDelay);
                    _socket.Dispose(); 
                }, CancellationToken.None);
            }
            else
            {
                _socket.Dispose();
            }

            if(_stream.CanWrite) _stream.Flush();
            _stream.Close();
            _stream = Stream.Null;
        }
    }

    protected static void ProcessError(ushort code, string msg)
    {
        throw new IOException($"Remote side responded with error code {code}, message '{msg}'");
    }

    protected Stopwatch _lastSendStopwatch = new Stopwatch();
    protected int _retryCount;

    protected void ResetRetries()
    {
        _retryCount = 0;
        _lastSendStopwatch.Restart();
    }

    protected async Task<UDPMessage> RetryingReceive(CancellationToken userCT, Func<CancellationToken,Task> retryAction)
    {
        while(true)
        {
            try
            {
                return await _socket.ReceiveWithTimeout(userCT, _responseTimeout - _lastSendStopwatch.Elapsed);
            }
            catch(TimeoutException)
            {
                if(_retryCount < _maxRetries)
                {
                    await retryAction(userCT);
                    _retryCount++;
                    _lastSendStopwatch.Restart();
                }
                else
                {
                    await TFTPServer.SendError(_socket, _remoteEndPoint, TFTPServer.ErrorCode.Undefined, "Timeout", userCT);
                    throw;
                }
            }
        }
    }
}
