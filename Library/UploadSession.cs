﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static CodePlex.JPMikkers.TFTP.TFTPServer;

namespace CodePlex.JPMikkers.TFTP;

internal class UploadSession : TFTPSession
{
    private long _position;
    private ReadOnlyMemory<byte> _window;

    public UploadSession(
        ITFTPLiveSessionInfo info,
        ITFTPStreamFactory streamFactory,
        IChildSocketFactory childSocketFactory,
        IPEndPoint remoteEndPoint,
        Dictionary<string, string> requestedOptions,
        TimeSpan responseTimeout,
        int maxRetries,
        string filename)
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
        _info.FileLength = -1;
        _info.Filename = _filename;
        _info.IsUpload = true;
        _info.LocalEndPoint = _localEndPoint;
        _info.RemoteEndPoint = _remoteEndPoint;
        _info.StartTime = DateTime.Now;
        _info.WindowSize = 1;
    }

    protected override async Task MainTask(CancellationToken cancellationToken)
    {
        Console.WriteLine($"starting Uploadsession loop");

        try
        {
            _length = _requestedOptions.ContainsKey(TFTPServer.Option_TransferSize) ? Int64.Parse(_requestedOptions[TFTPServer.Option_TransferSize]) : -1;
            _info.FileLength = _length;
            _stream = _streamFactory.GetWriteStream(_filename, _length);
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
            // rfc2349: in Write Request packets, the size of the file, in octets, is specified in the 
            // request and echoed back in the OACK
            _acceptedOptions.Add(TFTPServer.Option_TransferSize, _requestedOptions[TFTPServer.Option_TransferSize]);
        }

        if(_acceptedOptions.Count > 0)
        {
            // send options ack, client will respond with block number 1
            _window = TFTPServer.GetOptionsAckPacket(_acceptedOptions);
        }
        else
        {
            // send ack for current block, client will respond with next block
            _window = TFTPServer.GetDataAckPacket(_blockNumber);
        }

        await _socket.Send(_remoteEndPoint, _window, cancellationToken);
        ResetRetries();

        bool done = false;

        while(!done)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tup = await RetryingReceive(cancellationToken, ct => _socket.Send(_remoteEndPoint, _window, ct));

            MemoryStream ms = new(tup.Data.ToArray());
            ushort opCode = ReadUInt16(ms);

            switch((Opcode)opCode)
            {
                case Opcode.WriteRequest:
                    // unexpected writerequest, perhaps the client retried because our OptionsAck or
                    // DataAck packet was lost, ignore and let the retry mechanism handle this
                    break;

                case Opcode.ReadRequest:
                    // upload session already exists, and we're getting a readrequest?
                    await SendError(_socket, _remoteEndPoint, ErrorCode.IllegalOperation, "session already in progress", cancellationToken);
                    throw new IOException($"Unexpected ReadRequest during an upload session");

                case Opcode.Data:
                    done = await ProcessData(ReadUInt16(ms), tup.Data[(int)ms.Position..], cancellationToken);
                    break;

                case Opcode.Ack:
                case Opcode.OptionsAck:
                    // we don't expect Ack or OptionsAck during UploadSession.. ignore?
                    break;

                case Opcode.Error:
                    _dally = false;
                    ushort code = ReadUInt16(ms);
                    string msg = ReadZString(ms);
                    ProcessError(code, msg);
                    break;

                default:
                    await SendError(_socket, _remoteEndPoint, ErrorCode.IllegalOperation, "Unknown opcode", cancellationToken);
                    throw new IOException($"Remote side responded with unknown opcode {opCode}");
            }
        }
    }

    private async Task<bool> ProcessData(ushort blockNr, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        bool isComplete = false;

        if(blockNr == ((ushort)(_blockNumber + 1)))
        {
            _blockNumber = blockNr;
            _lastBlock = data.Length < _currentBlockSize;
            isComplete = _lastBlock;
            _stream.Write(data.Span);

            _position += data.Length;
            _info.Progress(_position);

            // send ack for current block, client will respond with next block
            _window = TFTPServer.GetDataAckPacket(_blockNumber);
            await _socket.Send(_remoteEndPoint, _window, cancellationToken);

            if(_lastBlock)
            {
                _info.Complete();
            }
            else
            {
                ResetRetries();
            }
        }
        return isComplete;
    }
}
