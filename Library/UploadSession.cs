﻿/*

Copyright (c) 2010 Jean-Paul Mikkers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace CodePlex.JPMikkers.TFTP
{
    internal class UploadSession : TFTPSession
    {
        private long _position;
        private ArraySegment<byte> _window;

        public UploadSession(TFTPServer parent, UDPSocket socket, IPEndPoint remoteEndPoint, Dictionary<string, string> requestedOptions, string filename, UDPSocket.OnReceiveDelegate onReceive)
            : base(parent,socket,remoteEndPoint,requestedOptions,filename, onReceive, 1000)
        {
        }

        public override void Start()
        {
            var sessionLogConfiguration = new SessionLogEntry.TConfiguration()
                                             {
                                                 FileLength = -1,
                                                 Filename = _filename,
                                                 IsUpload = true,
                                                 LocalEndPoint = _localEndPoint,
                                                 RemoteEndPoint = _remoteEndPoint,
                                                 StartTime = DateTime.Now,
                                                 WindowSize = 1
                                             };
            try
            {
                lock (_lock)
                {
                    try
                    {
                        _length = _requestedOptions.ContainsKey(TFTPServer.Option_TransferSize) ? Int64.Parse(_requestedOptions[TFTPServer.Option_TransferSize]) : -1;
                        sessionLogConfiguration.FileLength = _length;
                        _stream = _parent.GetWriteStream(_filename, _length);
                        _position = 0;
                    }
                    catch (Exception e)
                    {
                        TFTPServer.SendError(_socket, _remoteEndPoint, TFTPServer.ErrorCode.FileNotFound, e.Message);
                        throw;
                    }
                    finally
                    {
                        // always create a SessionLog (even if the file couldn't be opened), so Stop() will have somewhere to store its errors
                        _sessionLog = _parent.SessionLog.CreateSession(sessionLogConfiguration);
                    }

                    // handle tsize option
                    if (_requestedOptions.ContainsKey(TFTPServer.Option_TransferSize))
                    {
                        // rfc2349: in Write Request packets, the size of the file, in octets, is specified in the 
                        // request and echoed back in the OACK
                        _acceptedOptions.Add(TFTPServer.Option_TransferSize, _requestedOptions[TFTPServer.Option_TransferSize]);
                    }

                    if (_acceptedOptions.Count > 0)
                    {
                        // send options ack, client will respond with block number 1
                        _window = TFTPServer.GetOptionsAckPacket(_acceptedOptions);
                    }
                    else
                    {
                        // send ack for current block, client will respond with next block
                        _window = TFTPServer.GetDataAckPacket(_blockNumber);
                    }

                    TFTPServer.Send(_socket, _remoteEndPoint, _window);
                    _blockRetry = 0;
                    StartTimer();
                }
            }
            catch (Exception e)
            {
                Stop(true, e);
            }
        }

        protected override void SendResponse()
        {
            TFTPServer.Send(_socket, _remoteEndPoint, _window);
            StartTimer();
        }

        public override void ProcessData(ushort blockNr, ArraySegment<byte> data)
        {
            bool isComplete = false;

            lock (_lock)
            {
                if (blockNr == ((ushort)(_blockNumber+1)))
                {
                    _blockNumber = blockNr;
                    _lastBlock = data.Count < _currentBlockSize;
                    isComplete = _lastBlock;
                    _stream.Write(data.Array, data.Offset, data.Count);

                    _position += data.Count;
                    _sessionLog.Progress(_position);

                    // send ack for current block, client will respond with next block
                    _window = TFTPServer.GetDataAckPacket(_blockNumber);
                    TFTPServer.Send(_socket, _remoteEndPoint, _window);
                    _blockRetry = 0;

                    if (_lastBlock)
                    {
                        StopTimer();
                        _sessionLog.Complete();
                    }
                    else
                    {
                        StartTimer();
                    }
                }
            }

            if (isComplete)
            {
                Stop(true, null);
            }
        }
    }
}
