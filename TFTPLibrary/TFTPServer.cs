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
using System.Text;

namespace CodePlex.JPMikkers.TFTP
{
    public class TFTPServer : ITFTPServer
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

        private enum Opcode : ushort
        {
            ReadRequest = 1,
            WriteRequest,
            Data,
            Ack,
            Error,
            OptionsAck
        }

        private UDPSocket m_Socket;

        internal const int MaxBlockSize = 65464 + 4;
        internal const int DefaultBlockSize = 512;
        internal IPEndPoint m_ServerEndPoint = new IPEndPoint(IPAddress.Loopback, 69);
        internal short m_Ttl = -1;
        internal bool m_DontFragment = false;
        internal int m_MaxRetries = 5;
        internal int m_ResponseTimeout = 2000;
        private bool m_UseSinglePort = false;
        private string m_RootPath = ".";
        private bool m_AllowRead = true;
        private bool m_AllowWrite = true;
        private bool m_AutoCreateDirectories = true;

        private object m_Sync = new object();
        private bool m_Active = false;

        private Dictionary<IPEndPoint, ITFTPSession> m_Sessions;

        private void Stop(Exception reason)
        {
            bool notify = false;

            lock (m_Sync)
            {
                if (m_Active)
                {
                    m_Active = false;
                    notify = true;
                    m_Socket.Dispose();
                }
            }

            if (notify)
            {
                OnStop(this, reason);
            }
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

        internal Stream GetReadStream(string filename)
        {
            if (!m_AllowRead)
            {
                throw new UnauthorizedAccessException("Reading not allowed");
            }
            string targetPath = GetLocalFilename(filename);
            //Console.WriteLine("Getting read stream for file '{0}'", targetPath);
            return File.OpenRead(targetPath);
        }

        internal Stream GetWriteStream(string filename,long length)
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

        public TFTPServer()
        {
            m_Sessions = new Dictionary<IPEndPoint, ITFTPSession>();
        }

        private void OnUDPReceive(UDPSocket sender, IPEndPoint endPoint, ArraySegment<byte> data)
        {
            try
            {
                int packetSize = data.Count;
                MemoryStream ms = new MemoryStream(data.Array, data.Offset, data.Count, false, true);

                lock (m_Sessions)
                {
                    ITFTPSession session;
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
                                    new DownloadSession(this, m_UseSinglePort ? m_Socket : null, endPoint, requestedOptions, filename, OnUDPReceive);
                                }
                                break;

                            case Opcode.WriteRequest:
                                {
                                    string filename = ReadZString(ms);
                                    Mode mode = ReadMode(ms);
                                    var requestedOptions = ReadOptions(ms);
                                    new UploadSession(this, m_UseSinglePort ? m_Socket : null, endPoint, requestedOptions, filename, OnUDPReceive);
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

        private void OnUDPStop(UDPSocket sender, Exception reason)
        {
            Stop(reason);
        }

        internal void TransferStart(ITFTPSession session)
        {
            lock (m_Sessions)
            {
                m_Sessions.Add(session.RemoteEndPoint, session);
            }
        }

        internal void TransferComplete(ITFTPSession session, Exception reason)
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

        #region Dispose pattern

        ~TFTPServer()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            Stop();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion

        #region ITFTPServer Members

        public event Action<ITFTPServer,Exception> OnStop = (x,y) => { };
        public event Action<ITFTPServer> OnTransfer = x => { };

        public IPEndPoint EndPoint
        {
            get
            {
                return m_ServerEndPoint;
            }
            set
            {
                m_ServerEndPoint = value;
            }
        }

        public bool SinglePort
        {
            get
            {
                return m_UseSinglePort;
            }
            set
            {
                m_UseSinglePort = value;
            }
        }

        public short Ttl
        {
            get
            {
                return m_Ttl;
            }
            set
            {
                m_Ttl = value;
            }
        }

        public bool DontFragment
        {
            get
            {
                return m_DontFragment;
            }
            set
            {
                m_DontFragment = value;
            }
        }

        public int ResponseTimeout
        {
            get
            {
                return m_ResponseTimeout;
            }
            set
            {
                m_ResponseTimeout = value;
            }
        }

        public int Retries
        {
            get
            {
                return m_MaxRetries;
            }
            set
            {
                m_MaxRetries = value;
            }
        }

        public string RootPath
        {
            get
            {
                return m_RootPath;
            }
            set
            {
                m_RootPath = Path.GetFullPath(value);
            }
        }

        public bool AutoCreateDirectories
        {
            get
            {
                return m_AutoCreateDirectories;
            }
            set
            {
                m_AutoCreateDirectories = value;
            }
        }

        public bool AllowRead
        {
            get
            {
                return m_AllowRead;
            }
            set
            {
                m_AllowRead = value;
            }
        }

        public bool AllowWrite
        {
            get
            {
                return m_AllowWrite;
            }
            set
            {
                m_AllowWrite = value;
            }
        }

        public bool Active
        {
            get
            {
                lock (m_Sync)
                {
                    return m_Active;
                }
            }
        }

        public void Start()
        {
            lock (m_Sync)
            {
                if (!m_Active)
                {
                    try
                    {
                        m_Active = true;
                        m_Socket = new UDPSocket(m_ServerEndPoint, MaxBlockSize, m_DontFragment, m_Ttl, OnUDPReceive, OnUDPStop);
                    }
                    catch
                    {
                        m_Active = false;
                        throw;
                    }
                }
            }
        }

        public void Stop()
        {
            Stop(null);
        }

        #endregion

        #region Static helpers
        internal static Dictionary<string, string> ReadOptions(Stream s)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            while (s.Position < s.Length)
            {
                string key = ReadZString(s).ToLower();
                string val = ReadZString(s).ToLower();
                options.Add(key, val);
            }
            return options;
        }

        internal static void Send(UDPSocket socket, IPEndPoint endPoint, MemoryStream ms)
        {
            socket.Send(endPoint, new ArraySegment<byte>(ms.ToArray()));
        }

        internal static void SendError(UDPSocket socket, IPEndPoint endPoint, ushort code, string message)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Error);
            WriteUInt16(ms, code);
            WriteZString(ms, message.Substring(0, Math.Min(message.Length, 256)));
            Send(socket, endPoint, ms);
        }

        internal static void SendError(UDPSocket socket, IPEndPoint endPoint, ErrorCode code, string message)
        {
            SendError(socket, endPoint, (ushort)code, message);
        }

        internal static void SendAck(UDPSocket socket, IPEndPoint endPoint, ushort blockno)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Ack);
            WriteUInt16(ms, blockno);
            Send(socket, endPoint, ms);
        }

        internal static void SendData(UDPSocket socket, IPEndPoint endPoint, ushort blockno, byte[] data, int dataSize)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Data);
            WriteUInt16(ms, blockno);
            ms.Write(data, 0, dataSize);
            Send(socket, endPoint, ms);
        }

        internal static void SendOptionsAck(UDPSocket socket, IPEndPoint endPoint, Dictionary<string, string> options)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.OptionsAck);
            foreach (var s in options)
            {
                WriteZString(ms, s.Key);
                WriteZString(ms, s.Value);
            }
            Send(socket, endPoint, ms);
        }

        internal static string ReadZString(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            int c = s.ReadByte();
            while (c != 0)
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
            switch (ReadZString(s).ToLower())
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
            BinaryReader br = new BinaryReader(s);
            return (ushort)IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
        }

        internal static void WriteUInt16(Stream s, ushort v)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((ushort)IPAddress.HostToNetworkOrder((short)v));
        }
        #endregion
    }
}
