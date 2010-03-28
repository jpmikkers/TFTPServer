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
using System.Net.Sockets;

namespace TFTPServer
{
    public delegate void OnSessionsChangeDelegate(TFTPServer sender);

    public partial class TFTPServer : IDisposable
    {
        private UDPSocket m_Socket;
        private readonly IPEndPoint m_ServerEndPoint;
        private readonly bool m_IPv6;
        private readonly short m_Ttl;
        private readonly bool m_UseSinglePort;
        private readonly bool m_DontFragment;

        private readonly string m_RootPath;
        private readonly bool m_AllowRead;
        private readonly bool m_AllowWrite;
        private readonly bool m_AutoCreateDirectories;

        private volatile bool m_Disposed = false;

        private Dictionary<IPEndPoint, ISession> m_Sessions;

        private const int MaxBlockSize = 65464+4;
        private const int DefaultBlockSize = 512;
        
        private int m_MaxRetries = 5;
        private int m_ResponseTimeout = 2000;
       
        private enum Opcode : ushort
        {
            ReadRequest = 1,
            WriteRequest,
            Data,
            Ack,
            Error,
            OptionsAck
        }

        private enum Mode
        {
            NetAscii,
            Octet,
            Mail
        }

        private enum ErrorCode : ushort
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

        private string GetLocalFilename(string filename)
        {
            string result = Path.GetFullPath(Path.Combine(m_RootPath, filename));
            if (!result.StartsWith(m_RootPath))
            {
                throw new ArgumentException("Illegal filename");
            }
            return result;
        }

        private Stream GetReadStream(string filename)
        {
            if (!m_AllowRead)
            {
                throw new UnauthorizedAccessException("Reading not allowed");
            }
            string targetPath = GetLocalFilename(filename);
            //Console.WriteLine("Getting read stream for file '{0}'", targetPath);
            return File.OpenRead(targetPath);
        }

        private Stream GetWriteStream(string filename,long length)
        {
            if (!m_AllowWrite)
            {
                throw new UnauthorizedAccessException("Writing not allowed");
            }

            string targetPath = GetLocalFilename(filename);
            //Console.WriteLine("Getting write stream for file '{0}', size {1}", targetPath, length);

            if (m_AutoCreateDirectories)
            {
                string dir = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            return File.OpenWrite(targetPath);
        }

        public TFTPServer(IPEndPoint endPoint, bool useSinglePort, short ttl, bool dontFragment, string rootPath, bool autoCreateDirectories, bool allowRead, bool allowWrite, int timeOut, int retries)
        {
            m_ResponseTimeout = timeOut;
            m_MaxRetries = retries;
            m_DontFragment = dontFragment;
            m_AllowRead = allowRead;
            m_AllowWrite = allowWrite;
            m_AutoCreateDirectories = autoCreateDirectories;
            m_RootPath = Path.GetFullPath(rootPath);
            //Console.WriteLine("RootPath: {0}", m_RootPath);
            m_Ttl = ttl;
            m_UseSinglePort = useSinglePort;
            m_ServerEndPoint = endPoint;
            m_Sessions = new Dictionary<IPEndPoint, ISession>();
            m_IPv6 = (m_ServerEndPoint.AddressFamily == AddressFamily.InterNetworkV6);

            m_Socket = new UDPSocket(m_ServerEndPoint, MaxBlockSize, m_DontFragment, m_Ttl, OnReceive, OnStop);
        }

        private void OnReceive(UDPSocket sender, IPEndPoint endPoint, ArraySegment<byte> data)
        {
            if (m_Disposed) return;

            try
            {
                int packetSize = data.Count;
                MemoryStream ms = new MemoryStream(data.Array, data.Offset, data.Count, false, true);

                lock (m_Sessions)
                {
                    ISession session;
                    ushort opCode = ReadUInt16(ms);

                    if (m_Sessions.TryGetValue(endPoint, out session))
                    {
                        switch ((Opcode)opCode)
                        {
                            case Opcode.ReadRequest:
                                // session already exists, and we're getting a new readrequest?? what to do??
                                SendError(sender, endPoint, ErrorCode.IllegalOperation, "Session already in progress");
                                break;

                            case Opcode.WriteRequest:
                                // session already exists, and we're getting a new writerequest?? what to do??
                                SendError(sender, endPoint, ErrorCode.IllegalOperation, "Session already in progress");
                                break;

                            case Opcode.Data:
                                session.ProcessData(ReadUInt16(ms), ms);
                                break;

                            case Opcode.Ack:
                                session.ProcessAck(ReadUInt16(ms));
                                break;

                            case Opcode.Error:
                                ushort code = ReadUInt16(ms);
                                string msg = ReadZString(ms);
                                //Console.WriteLine("Received error:{0} {1}", code, msg);
                                m_Sessions[endPoint].ProcessError(code, msg);
                                break;

                            case Opcode.OptionsAck:
                                break;

                            default:
                                throw new InvalidDataException(string.Format("Invalid opcode {0}", opCode));
                        }
                    }
                    else // session==null
                    {
                        switch ((Opcode)opCode)
                        {
                            case Opcode.ReadRequest:
                                {
                                    string filename = ReadZString(ms);
                                    Mode mode = ReadMode(ms);
                                    var requestedOptions = ReadOptions(ms);
                                    new DownloadSession(this, m_UseSinglePort ? m_Socket : null, endPoint, requestedOptions, filename, OnReceive);
                                }
                                break;

                            case Opcode.WriteRequest:
                                {
                                    string filename = ReadZString(ms);
                                    Mode mode = ReadMode(ms);
                                    var requestedOptions = ReadOptions(ms);
                                    new UploadSession(this, m_UseSinglePort ? m_Socket : null, endPoint, requestedOptions, filename, OnReceive);
                                }
                                break;

                            default:
                                SendError(m_Socket, endPoint, (ushort)ErrorCode.UnknownTransferID, "Unknown transfer ID");
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception in TFTPServer : {0}", e);
            }
        }

        private void OnStop(UDPSocket sender, Exception reason)
        {
            //Console.WriteLine("OnStop in TFTPServer : {0}", reason);
        }

        ~TFTPServer()
        {
            Dispose(false);
        }

        private void TransferStart(ISession session)
        {
            lock (m_Sessions)
            {
                m_Sessions.Add(session.RemoteEndPoint, session);
            }
        }

        private void TransferComplete(ISession session, Exception reason)
        {
            lock (m_Sessions)
            {
                if (m_Sessions.ContainsKey(session.RemoteEndPoint))
                {
                    m_Sessions.Remove(session.RemoteEndPoint);
                    session.Dispose();
                }
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                try
                {
                    m_Disposed = true;
                    m_Socket.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion
    }
}
