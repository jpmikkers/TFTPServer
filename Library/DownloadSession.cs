namespace Baksteen.Net.TFTP.Server;
using System;
using System.Collections.Generic;
using System.Net;

internal class WindowEntry
{
    public bool IsData;
    public int Length;
    public ArraySegment<byte> Segment;
}

internal class DownloadSession : TFTPSession
{
    private long _position;
    private readonly ushort _windowSize;

    private readonly List<WindowEntry> _window = new List<WindowEntry>();

    public DownloadSession(
        TFTPServer parent,
        UDPSocket socket,
        IPEndPoint remoteEndPoint,
        Dictionary<string, string> requestedOptions,
        string filename,
        ushort windowSize,
        UDPSocket.OnReceiveDelegate onReceive)
        : base(parent, socket, remoteEndPoint, requestedOptions, filename, onReceive, 0)
    {
        _windowSize = windowSize;
    }

    public override void Start()
    {
        var sessionLogConfiguration = new SessionLogEntry.TConfiguration()
        {
            FileLength = -1,
            Filename = _filename,
            IsUpload = false,
            LocalEndPoint = _localEndPoint,
            RemoteEndPoint = _remoteEndPoint,
            StartTime = DateTime.Now,
            WindowSize = _windowSize
        };

        try
        {
            lock(_lock)
            {
                try
                {
                    _stream = _parent.GetReadStream(_filename);
                    _length = _stream.Length;
                    sessionLogConfiguration.FileLength = _length;
                    _position = 0;
                }
                catch(Exception e)
                {
                    TFTPServer.SendError(_socket, _remoteEndPoint, TFTPServer.ErrorCode.FileNotFound, e.Message);
                    throw;
                }
                finally
                {
                    _sessionLog = _parent.SessionLog.CreateSession(sessionLogConfiguration);
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
                    SendOptionsAck();
                }
                else
                {
                    _blockNumber = 1;
                    SendData();
                }
            }
        }
        catch(Exception e)
        {
            Stop(true, e);
        }
    }

    protected override void SendResponse()
    {
        // resend blocks in the window
        for(int t = 0; t < _window.Count; t++)
        {
            TFTPServer.Send(_socket, _remoteEndPoint, _window[t].Segment);
        }
        StartTimer();
    }

    private void SendOptionsAck()
    {
        var seg = TFTPServer.GetOptionsAckPacket(_acceptedOptions);
        _window.Add(new WindowEntry() { IsData = false, Length = 0, Segment = seg });
        TFTPServer.Send(_socket, _remoteEndPoint, seg);
        _blockRetry = 0;
        StartTimer();
    }

    private void SendData()
    {
        // fill the window up to the window size & send all the new packets
        while(_window.Count < _windowSize && !_lastBlock)
        {
            byte[] buffer = new byte[_currentBlockSize];
            int dataSize = _stream.Read(buffer, 0, _currentBlockSize);
            var seg = TFTPServer.GetDataPacket((ushort)(_blockNumber + _window.Count), buffer, dataSize);
            _window.Add(new WindowEntry() { IsData = true, Length = dataSize, Segment = seg });
            _lastBlock = (dataSize < _currentBlockSize);
            TFTPServer.Send(_socket, _remoteEndPoint, seg);
            _blockRetry = 0;
            StartTimer();
        }
    }

    private bool WindowContainsBlock(ushort blocknr)
    {
        // rollover safe way of checking: _BlockNumber <= blocknr < (_BlockNumber+_Window.Count)
        return ((ushort)(blocknr - _blockNumber)) < _window.Count;
    }

    public override void ProcessAck(ushort blockNr)
    {
        bool isComplete = false;

        lock(_lock)
        {
            if(WindowContainsBlock(blockNr))
            {
                while(WindowContainsBlock(blockNr))
                {
                    if(_window[0].IsData) _position += _window[0].Length;
                    _blockNumber++;
                    _window.RemoveAt(0);
                }

                _sessionLog.Progress(_position);

                SendData();

                if(_window.Count == 0)
                {
                    // Everything was acked
                    isComplete = true;
                    StopTimer();
                    _sessionLog.Complete();
                }
            }
        }

        if(isComplete)
        {
            Stop(true, null);
        }
    }
}
