using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static Baksteen.Net.TFTP.Server.TFTPServer;

namespace Baksteen.Net.TFTP.Server;

internal class DownloadSession : TFTPSession
{
    private class WindowEntry
    {
        public bool IsData;
        public int Length;
        public ReadOnlyMemory<byte> Segment;
    }

    private long _position;
    private readonly ushort _windowSize;

    private readonly List<WindowEntry> _window = [];

    public DownloadSession(
        ITFTPSessionInfo info,
        ITFTPStreamFactory streamFactory,
        IChildSocketFactory childSocketFactory,
        IPEndPoint remoteEndPoint,
        Dictionary<string, string> requestedOptions,
        string filename,
        TimeSpan responseTimeout,
        int maxRetries,
        ushort windowSize)
        : base(
            info,
            streamFactory,
            childSocketFactory,
            remoteEndPoint,
            requestedOptions,
            filename,
            responseTimeout,
            maxRetries)
    {
        _windowSize = windowSize;

        _info.Start(new TFTPSessionStartInfo
        {
            FileLength = -1,
            Filename = _filename,
            IsUpload = false,
            LocalEndPoint = _localEndPoint,
            RemoteEndPoint = _remoteEndPoint,
            StartTimeUtc = DateTime.UtcNow,
            WindowSize = _windowSize,
        });
    }

    protected override async Task MainTask(CancellationToken cancellationToken)
    {
        try
        {
            _stream = _streamFactory.GetReadStream(_filename);
            _length = _stream.Length;
            _info.UpdateStart(new TFTPSessionUpdateInfo { FileLength = _length });
            _position = 0;
        }
        catch(Exception e)
        {
            await TFTPServer.SendError(_socket, _remoteEndPoint, TFTPServer.ErrorCode.FileNotFound, e.Message, cancellationToken);
            throw;
        }

        // handle tsize option
        if(_requestedOptions.ContainsKey(TFTPServer.Option_TransferSize))
        {
            if(_length >= 0)
            {
                _acceptedOptions.Add(TFTPServer.Option_TransferSize, _length.ToString());
            }
        }

        if(_acceptedOptions.Count > 0)
        {
            _blockNumber = 0;
            await SendOptionsAck(cancellationToken);
        }
        else
        {
            _blockNumber = 1;
            await FillWindowSendNewData(cancellationToken);
        }

        bool done = false;

        while(!done)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tup = await RetryingReceive(cancellationToken, async ct =>
            {
                foreach(var v in _window) await _socket.Send(_remoteEndPoint, v.Segment, ct);
            });

            MemoryStream ms = new(tup.Data.ToArray());

            ushort opCode = ReadUInt16(ms);

            switch((Opcode)opCode)
            {
                case Opcode.ReadRequest:
                    // unexpected readrequest, perhaps the client retried because our OptionsAck or
                    // Data packet was lost, ignore and let the retry mechanism handle this
                    break;

                case Opcode.WriteRequest:
                    // download session already exists, and we're getting a writerequest?
                    await SendError(_socket, _remoteEndPoint, ErrorCode.IllegalOperation, "session already in progress", cancellationToken);
                    throw new IOException($"Unexpected WriteRequest during a download session");

                case Opcode.Ack:
                    done = await ProcessAck(ReadUInt16(ms), cancellationToken);
                    break;

                case Opcode.Error:
                    _dally = false;
                    ushort code = ReadUInt16(ms);
                    string msg = ReadZString(ms);
                    ProcessError(code, msg);    // this throws..
                    break;

                case Opcode.Data:
                case Opcode.OptionsAck:
                    // we don't expect Data or OptionsAck during download session, ignore?
                    break;

                default:
                    await SendError(_socket, _remoteEndPoint, ErrorCode.IllegalOperation, "Unknown opcode", cancellationToken);
                    throw new IOException($"Remote side responded with unknown opcode {opCode}");
            }
        }

        // when the download is complete, the last packet the server received was an ack, so there are
        // no outgoing packets in progress. Thus, we can close the socket immediately
        _dally = false;
    }

    private async Task SendOptionsAck(CancellationToken cancellationToken)
    {
        var seg = TFTPServer.GetOptionsAckPacket(_acceptedOptions);
        _window.Add(new WindowEntry() { IsData = false, Length = 0, Segment = seg });
        await _socket.Send(_remoteEndPoint, seg, cancellationToken);
        ResetRetries();
    }

    private async Task FillWindowSendNewData(CancellationToken cancellationToken)
    {
        // fill the window up to the window size & send all the new packets
        while(_window.Count < _windowSize && !_lastBlock)
        {
            byte[] buffer = new byte[_currentBlockSize];
            int dataSize = _stream.Read(buffer, 0, _currentBlockSize);
            var seg = TFTPServer.GetDataPacket((ushort)(_blockNumber + _window.Count), buffer, dataSize);
            _window.Add(new WindowEntry() { IsData = true, Length = dataSize, Segment = seg });
            _lastBlock = (dataSize < _currentBlockSize);
            await _socket.Send(_remoteEndPoint, seg, cancellationToken);
            ResetRetries();
        }
    }

    private bool WindowContainsBlock(ushort blocknr)
    {
        // rollover safe way of checking: _BlockNumber <= blocknr < (_BlockNumber+_Window.Count)
        return ((ushort)(blocknr - _blockNumber)) < _window.Count;
    }

    public async Task<bool> ProcessAck(ushort blockNr, CancellationToken cancellationToken)
    {
        bool isComplete = false;

        if(WindowContainsBlock(blockNr))
        {
            while(WindowContainsBlock(blockNr))
            {
                if(_window[0].IsData) _position += _window[0].Length;
                _blockNumber++;
                _window.RemoveAt(0);
            }

            _info.Progress(_position);

            await FillWindowSendNewData(cancellationToken);

            if(_window.Count == 0)
            {
                // Everything was acked
                isComplete = true;
                _info.Complete();
            }
        }

        return isComplete;
    }
}
