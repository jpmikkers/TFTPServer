/*

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
    internal abstract class TFTPSession : ITFTPSession
    {
        protected volatile bool _disposed = false;
        private Timer _timer;
        private int _responseTimeout;
        protected object _lock = new object();
        protected SessionLog.ISession _sessionLog;
        protected TFTPServer _parent;
        protected Stream _stream;
        protected UDPSocket _socket;
        protected bool _ownSocket;
        protected IPEndPoint _localEndPoint;
        protected IPEndPoint _remoteEndPoint;
        protected int _socketDisposeDelay;
        protected long _length;
        protected int _currentBlockSize;
        protected bool _lastBlock;
        protected ushort _blockNumber;
        protected ushort _blockRetry;
        protected Dictionary<string, string> _requestedOptions;
        protected string _filename;
        protected Dictionary<string, string> _acceptedOptions = new Dictionary<string, string>();

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

        private void OnTimer(object state)
        {
            bool notify = false;

            lock (_lock)
            {
                if (!_disposed)
                {
                    if (_blockRetry < _parent._maxRetries)
                    {
                        _blockRetry++;
                        SendResponse();
                    }
                    else
                    {
                        TFTPServer.SendError(_socket, _remoteEndPoint, TFTPServer.ErrorCode.Undefined, "Timeout");
                        notify = true;
                    }
                }
            }

            if (notify)
            {
                Stop(true,new TimeoutException("Remote side didn't respond in time"));
            }
        }

        protected void StartTimer()
        {
            _timer.Change(_responseTimeout, Timeout.Infinite);
        }

        protected void StopTimer()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public TFTPSession(TFTPServer parent, UDPSocket socket, IPEndPoint remoteEndPoint, Dictionary<string, string> requestedOptions, string filename, UDPSocket.OnReceiveDelegate onReceive, int socketDisposeDelay)
        {
            _parent = parent;
            _currentBlockSize = TFTPServer.DefaultBlockSize;
            _responseTimeout = _parent._responseTimeout;
            _remoteEndPoint = remoteEndPoint;
            _timer = new Timer(new System.Threading.TimerCallback(OnTimer), this, Timeout.Infinite, Timeout.Infinite);
            _requestedOptions = requestedOptions;
            _filename = filename;
            _socketDisposeDelay = socketDisposeDelay;

            _length = 0;
            _lastBlock = false;
            _blockNumber = 0;
            _blockRetry = 0;

            foreach (var kvp in _requestedOptions)
            {
                switch (kvp.Key)
                {
                    case TFTPServer.Option_Multicast:
                        // not supported
                        break;

                    case TFTPServer.Option_Timeout:
                        //Console.WriteLine("Timeout of {0}", kvp.Value);
                        int requestedTimeout = int.Parse(kvp.Value);
                        // rfc2349 : valid values range between "1" and "255" seconds
                        if (requestedTimeout >= 1 && requestedTimeout <= 255)
                        {
                            _responseTimeout = requestedTimeout * 1000;
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
                        if (requestedBlockSize >= 8 && requestedBlockSize <= 65464)
                        {
                            _currentBlockSize = Math.Min(TFTPServer.MaxBlockSize, requestedBlockSize);
                            _acceptedOptions.Add(TFTPServer.Option_BlockSize, _currentBlockSize.ToString());
                        }
                        break;
                }
            }

            if (socket != null)
            {
                _ownSocket = false;
                _socket = socket;
            }
            else
            {
                _ownSocket = true;
                _socket = new UDPSocket(
                    new IPEndPoint(_parent._serverEndPoint.Address, 0), 
                    _currentBlockSize + 4, 
                    _parent._dontFragment, 
                    _parent._Ttl, 
                    onReceive, 
                    (sender, reason) => { Stop(true, reason); });
            }
            _localEndPoint = (IPEndPoint)_socket.LocalEndPoint;
        }

        protected void Stop(bool dally,Exception reason)
        {
            bool notify = false;

            lock (_lock)
            {
                if (!_disposed)
                {
                    if (_sessionLog != null)
                    {
                        _sessionLog.Stop(reason);
                    }

                    _disposed = true;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timer.Dispose();

                    if (_ownSocket)
                    {
                        if (_socket != null)
                        {
                            if (dally && _socket.SendPending)
                            {
                                DelayedDisposer.QueueDelayedDispose(_socket, _socketDisposeDelay);
                            }
                            else
                            {
                                _socket.Dispose();
                            }
                        }
                    }

                    if (_stream != null)
                    {
                        if (_stream.CanWrite) _stream.Flush();
                        _stream.Close();
                        _stream = null;
                    }

                    notify = true;
                }
            }

            if (notify)
            {
                _parent.TransferComplete(this, reason);
            }
        }

        protected abstract void SendResponse();

        #region ITransferSession Members

        public abstract void Start();

        public void Stop()
        {
            Stop(false, new Exception("Parent stopped"));
        }

        public virtual void ProcessAck(ushort blockNr) { }
        public virtual void ProcessData(ushort blockNr, ArraySegment<byte> data) { }

        public void ProcessError(ushort code, string msg)
        {
            Stop(false,new IOException($"Remote side responded with error code {code}, message '{msg}'"));
        }

        #endregion

        ~TFTPSession()
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop(disposing, null);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}
