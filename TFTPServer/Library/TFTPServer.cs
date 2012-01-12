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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CodePlex.JPMikkers.TFTP
{
    public class TFTPServer : ITFTPServer
    {
        #region TFTP definitions 

        public class ConfigurationAlternative
        {
            private Regex m_Regex;
            public ushort WindowSize { get; set; }

            public bool Match(string s)
            {
                return m_Regex.IsMatch(s);
            }

            private ConfigurationAlternative(Regex regex)
            {
                m_Regex = regex;
                WindowSize = 1;
            }

            public static ConfigurationAlternative CreateRegex(string regex)
            {
                return new ConfigurationAlternative(new Regex(regex, RegexOptions.IgnoreCase));
            }

            public static ConfigurationAlternative CreateWildcard(string wildcard)
            {
                return CreateRegex(WildcardToRegex.Convert(wildcard));
            }
        }

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

        internal const string Option_Multicast = "multicast";
        internal const string Option_Timeout = "timeout";
        internal const string Option_TransferSize = "tsize";
        internal const string Option_BlockSize = "blksize";

        #endregion TFTP definitions

        private string m_Name;
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
        private bool m_ConvertPathSeparator = true;
        private ushort m_WindowSize = 1;
        private List<ConfigurationAlternative> m_ConfigurationAlternatives = new List<ConfigurationAlternative>();

        private object m_Sync = new object();
        private bool m_Active = false;

        private readonly Dictionary<IPEndPoint, ITFTPSession> m_Sessions;
        private readonly SessionLog m_SessionLog = new SessionLog();

        public SessionLog SessionLog
        {
            get { return m_SessionLog; }
        }

        private void Stop(Exception reason)
        {
            bool notify = false;

            lock (m_Sync)
            {
                if (m_Active)
                {
                    Trace(string.Format("Stopping TFTP server '{0}'", m_ServerEndPoint));
                    m_Active = false;
                    notify = true;
                    m_Socket.Dispose();
                    // get shallow copy of running sessions
                    var sessions = new List<ITFTPSession>();
                    lock (m_Sessions)
                    {
                        sessions.AddRange(m_Sessions.Values);
                    }
                    // stop all of them
                    foreach (var session in sessions)
                    {
                        session.Stop();
                    }
                    Trace("Stopped");
                }
            }

            if (notify)
            {
                var data = new TFTPStopEventArgs();
                data.Reason = reason;
                OnStatusChange(this, data);
            }
        }

        private static string StripRoot(string filename)
        {
            // strip root from filename before calling Path.Combine(). Some clients like to prepend a leading backslash, resulting in an 'Illegal filename' error.
            if (Path.IsPathRooted(filename))
            {
                filename = filename.Substring(Path.GetPathRoot(filename).Length);
            }
            return filename;
        }

        private string GetLocalFilename(string filename)
        {
            string result = Path.GetFullPath(Path.Combine(m_RootPath, StripRoot(filename)));
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
            m_Name = "TFTPServer";
            m_Sessions = new Dictionary<IPEndPoint, ITFTPSession>();
        }

        private ushort GetWindowSize(string filename)
        {
            if(m_ConfigurationAlternatives.Count>0)
            {
                filename = StripRoot(filename);

                foreach (ConfigurationAlternative alternative in m_ConfigurationAlternatives)
                {
                    if (alternative.Match(filename))
                    {
                        return alternative.WindowSize;
                    }
                }
            }
            return m_WindowSize;
        }

        private void OnUDPReceive(UDPSocket sender, IPEndPoint endPoint, ArraySegment<byte> data)
        {
            bool notify = false;
            int packetSize = data.Count;
            MemoryStream ms = new MemoryStream(data.Array, data.Offset, data.Count, false, true);

            lock (m_Sessions)
            {
                ITFTPSession session;
                ushort opCode = ReadUInt16(ms);

                // is there a session in progress for that endpoint?
                if (m_Sessions.TryGetValue(endPoint, out session))
                {
                    // yes.
                    switch ((Opcode)opCode)
                    {
                        case Opcode.ReadRequest:
                            // session already exists, and we're getting a new readrequest?
                            SendError(sender, endPoint, ErrorCode.IllegalOperation, "Read session already in progress");
                            break;

                        case Opcode.WriteRequest:
                            // session already exists, and we're getting a new writerequest?
                            SendError(sender, endPoint, ErrorCode.IllegalOperation, "Write session already in progress");
                            break;

                        case Opcode.Data:
                            session.ProcessData(ReadUInt16(ms), new ArraySegment<byte>(data.Array,(int)(data.Offset+ms.Position),(int)(data.Count-ms.Position)));
                            break;

                        case Opcode.Ack:
                            session.ProcessAck(ReadUInt16(ms));
                            break;

                        case Opcode.Error:
                            ushort code = ReadUInt16(ms);
                            string msg = ReadZString(ms);
                            session.ProcessError(code, msg);
                            break;

                        case Opcode.OptionsAck:
                            break;

                        default:
                            SendError(sender, endPoint, ErrorCode.IllegalOperation, "Unknown opcode");
                            break;
                    }
                }
                else // session==null
                {
                    // no session in progress for the endpoint that sent the packet
                    switch ((Opcode)opCode)
                    {
                        case Opcode.ReadRequest:
                            {
                                string filename = ReadZString(ms);
                                if(m_ConvertPathSeparator) filename = filename.Replace('/','\\');
                                Mode mode = ReadMode(ms);
                                var requestedOptions = ReadOptions(ms);
                                ushort windowSize = GetWindowSize(filename);
                                ITFTPSession newSession = new DownloadSession(this, m_UseSinglePort ? m_Socket : null, endPoint, requestedOptions, filename, windowSize, OnUDPReceive);
                                m_Sessions.Add(newSession.RemoteEndPoint, newSession);
                                notify = true;
                                Trace(string.Format("Starting transfer of file '{0}' from local '{1}' to remote '{2}', send window size {3}", newSession.Filename, newSession.LocalEndPoint, newSession.RemoteEndPoint, windowSize));
                                newSession.Start();
                            }
                            break;

                        case Opcode.WriteRequest:
                            {
                                string filename = ReadZString(ms);
                                if (m_ConvertPathSeparator) filename = filename.Replace('/', '\\');
                                Mode mode = ReadMode(ms);
                                var requestedOptions = ReadOptions(ms);
                                ITFTPSession newSession=new UploadSession(this, m_UseSinglePort ? m_Socket : null, endPoint, requestedOptions, filename, OnUDPReceive);
                                m_Sessions.Add(newSession.RemoteEndPoint, newSession);
                                notify = true;
                                Trace(string.Format("Starting transfer of file '{0}' from remote '{1}' to local '{2}'", newSession.Filename, newSession.RemoteEndPoint, newSession.LocalEndPoint));
                                newSession.Start();
                            }
                            break;

                        default:
                            SendError(m_Socket, endPoint, (ushort)ErrorCode.UnknownTransferID, "Unknown transfer ID");
                            break;
                    }
                }
            }

            if (notify)
            {
                OnStatusChange(this, null);
            }
        }

        private void OnUDPStop(UDPSocket sender, Exception reason)
        {
            Stop(reason);
        }

        internal void TransferComplete(ITFTPSession session, Exception reason)
        {
            lock (m_Sessions)
            {
                if (m_Sessions.ContainsKey(session.RemoteEndPoint))
                {
                    if (reason == null)
                    {
                        Trace(string.Format("Completed transfer {0} '{1}'", session is UploadSession ? "from" : "to", session.RemoteEndPoint));
                    }
                    else
                    {
                        Trace(string.Format("Aborted transfer {0} '{1}', reason '{2}'", session is UploadSession ? "from" : "to", session.RemoteEndPoint, reason));
                    }
                    m_Sessions.Remove(session.RemoteEndPoint);
                    session.Dispose();
                }
            }

            OnStatusChange(this, null);
        }

        internal void Trace(string msg)
        {
            var data=new TFTPTraceEventArgs();
            data.Message = msg;
            OnTrace(this, data);
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

        public event EventHandler<TFTPTraceEventArgs> OnTrace = (sender, data) => { };
        public event EventHandler<TFTPStopEventArgs> OnStatusChange = (sender, data) => { };

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }


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

        public bool ConvertPathSeparator
        {
            get
            {
                return m_ConvertPathSeparator;
            }
            set
            {
                m_ConvertPathSeparator = value;
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

        public ushort WindowSize
        {
            get
            {
                return m_WindowSize;
            }
            set
            {
                m_WindowSize=Clip<ushort>(value,1,32);
            }
        }

        public IList<ConfigurationAlternative> ConfigurationAlternatives
        {
            get
            {
                return m_ConfigurationAlternatives;
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

        public int ActiveTransfers
        {
            get
            {
                lock (m_Sessions)
                {
                    return m_Sessions.Count;
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
                        Trace(string.Format("Starting TFTP server '{0}'",m_ServerEndPoint));
                        m_Active = true;
                        m_Socket = new UDPSocket(m_ServerEndPoint, MaxBlockSize, m_DontFragment, m_Ttl, OnUDPReceive, OnUDPStop);
                        Trace(string.Format("TFTP Server start succeeded, serving at '{0}'",m_Socket.LocalEndPoint));
                    }
                    catch(Exception e)
                    {
                        Trace(string.Format("TFTP Server start failed, reason '{0}'",e));
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

        internal static T Clip<T>(T value, T minValue, T maxValue) where T : IComparable<T>
        {
            T result;
            if (value.CompareTo(minValue) < 0)
                result = minValue;
            else if (value.CompareTo(maxValue) > 0)
                result = maxValue;
            else
                result = value;
            return result;
        }

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

        internal static void Send(UDPSocket socket, IPEndPoint endPoint, ArraySegment<byte> data)
        {
            socket.Send(endPoint, data);
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

        internal static ArraySegment<byte> GetDataAckPacket(ushort blockno)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Ack);
            WriteUInt16(ms, blockno);
            return new ArraySegment<byte>(ms.ToArray());
        }

        internal static ArraySegment<byte> GetOptionsAckPacket(Dictionary<string, string> options)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.OptionsAck);
            foreach (var s in options)
            {
                WriteZString(ms, s.Key);
                WriteZString(ms, s.Value);
            }
            return new ArraySegment<byte>(ms.ToArray());
        }

        internal static ArraySegment<byte> GetDataPacket(ushort blockno, byte[] data, int dataSize)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Data);
            WriteUInt16(ms, blockno);
            ms.Write(data, 0, dataSize);
            return new ArraySegment<byte>(ms.ToArray());
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
