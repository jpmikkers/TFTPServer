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
    public partial class TFTPServer
    {
        private interface ISession : IDisposable
        {
            void ProcessAck(ushort blockNr);
            void ProcessData(ushort blockNr, System.IO.Stream ms);
            void ProcessError(ushort code, string msg);
            IPEndPoint RemoteEndPoint { get; }
            string Filename { get; }
        }

        private abstract class SessionBase : ISession
        {
            protected volatile bool m_Disposed = false;

            protected object m_Lock = new object();
            protected TFTPServer m_Parent;
            protected Stream m_Stream;
            protected UDPSocket m_Socket;
            protected bool m_OwnSocket;
            protected IPEndPoint m_RemoteEndPoint;
            protected Timer m_Timer;
            protected int m_ResponseTimeout;
            protected int m_SocketDisposeDelay;

            protected long m_Length;
            protected int m_CurrentBlockSize;
            protected bool m_FirstBlock;
            protected bool m_LastBlock;
            protected ushort m_BlockNumber;
            protected ushort m_BlockRetry;

            protected Dictionary<string, string> m_RequestedOptions;
            protected string m_Filename;

            protected Dictionary<string, string> m_AcceptedOptions = new Dictionary<string, string>();

            public IPEndPoint RemoteEndPoint
            {
                get
                {
                    return m_RemoteEndPoint;
                }
            }

            public string Filename
            {
                get
                {
                    return m_Filename;
                }
            }

            private void OnTimer(object state)
            {
                bool isComplete = false;

                lock (m_Lock)
                {
                    if (!m_Disposed)
                    {
                        if (m_BlockRetry < m_Parent.m_MaxRetries)
                        {
                            m_BlockRetry++;
                            SendResponse();
                        }
                        else
                        {
                            SendError(m_Socket, m_RemoteEndPoint, ErrorCode.Undefined, "Timeout");
                            isComplete = true;
                        }
                    }
                }

                if (isComplete)
                {
                    m_Parent.TransferComplete(this, new TimeoutException("Remote side didn't respond in time"));
                }
            }

            protected abstract void SendResponse();

            public SessionBase(TFTPServer parent, UDPSocket socket, IPEndPoint remoteEndPoint, Dictionary<string, string> requestedOptions, string filename, UDPSocket.OnReceiveDelegate onReceive, int socketDisposeDelay)
            {
                m_Parent = parent;
                m_CurrentBlockSize = TFTPServer.DefaultBlockSize;
                m_ResponseTimeout = m_Parent.m_ResponseTimeout;
                m_RemoteEndPoint = remoteEndPoint;
                m_Timer = new Timer(new System.Threading.TimerCallback(OnTimer), this, Timeout.Infinite, Timeout.Infinite);
                m_RequestedOptions = requestedOptions;
                m_Filename = filename;
                m_SocketDisposeDelay = socketDisposeDelay;

                m_Length = 0;
                m_FirstBlock = true;
                m_LastBlock = false;
                m_BlockNumber = 0;
                m_BlockRetry = 0;

                foreach (var kvp in m_RequestedOptions)
                {
                    switch (kvp.Key)
                    {
                        case "multicast":
                            // not supported
                            break;

                        case "timeout":
                            Console.WriteLine("Timeout of {0}", kvp.Value);
                            int requestedTimeout = int.Parse(kvp.Value);
                            // rfc2349 : valid values range between "1" and "255" seconds
                            if (requestedTimeout >= 1 && requestedTimeout <= 255)
                            {
                                m_ResponseTimeout = requestedTimeout * 1000;
                                m_AcceptedOptions.Add("timeout", kvp.Value);
                            }
                            break;

                        case "tsize":
                            // handled in inherited classes
                            break;

                        case "blksize":
                            Console.WriteLine("Blocksize of {0}", kvp.Value);
                            int requestedBlockSize = int.Parse(kvp.Value);
                            // rfc2348 : valid values range between "8" and "65464" octets, inclusive
                            if (requestedBlockSize >= 8 && requestedBlockSize <= 65464)
                            {
                                m_CurrentBlockSize = Math.Min(MaxBlockSize, requestedBlockSize);
                                m_AcceptedOptions.Add("blksize", m_CurrentBlockSize.ToString());
                            }
                            break;
                    }
                }

                if (socket != null)
                {
                    m_OwnSocket = false;
                    m_Socket = socket;
                }
                else
                {
                    m_OwnSocket = true;
                    m_Socket = new UDPSocket(new IPEndPoint(m_Parent.m_ServerEndPoint.Address, 0),m_CurrentBlockSize+4,m_Parent.m_DontFragment,m_Parent.m_Ttl, onReceive, OnStop );
                }
            }

            private void OnStop(UDPSocket sender, Exception reason)
            {
                lock (m_Lock)
                {
                    if (!m_Disposed)
                    {
                        Console.WriteLine("OnStop() : {0}", reason);
                        m_Parent.TransferComplete(this, reason);
                    }
                }
            }

            #region ITransferSession Members

            public virtual void ProcessAck(ushort blockNr) { }
            public virtual void ProcessData(ushort blockNr, Stream ms) { }

            public virtual void ProcessError(ushort code, string msg)
            {
                Console.WriteLine("transfer aborted due to Error {0} Msg {1}", code, msg);
                m_Parent.TransferComplete(this, new IOException(string.Format("Remote side responsed with error code {0}, message '{1}'",code,msg)));
            }

            #endregion

            ~SessionBase()
            {
                Dispose(false);
            }

            protected virtual void Dispose(bool disposing)
            {
                lock (m_Lock)
                {
                    if (!m_Disposed)
                    {
                        m_Disposed = true;
                        m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
                        m_Timer.Dispose();

                        if (m_OwnSocket)
                        {
                            if (m_Socket != null)
                            {
                                if (disposing && m_Socket.SendPending)
                                {
                                    DelayedDisposer.QueueDelayedDispose(m_Socket, m_SocketDisposeDelay);
                                }
                                else
                                {
                                    m_Socket.Dispose();
                                }
                            }
                        }

                        if (m_Stream != null)
                        {
                            if (m_Stream.CanWrite) m_Stream.Flush();
                            m_Stream.Close();
                            m_Stream = null;
                        }

                        // binnen de lock de parent aanroepen.. hmmmmmm
                        if (m_Parent != null)
                        {
                            m_Parent.TransferComplete(this, null);
                            m_Parent = null;
                        }
                    }
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Dispose(true);
            }
        }
    }
}
