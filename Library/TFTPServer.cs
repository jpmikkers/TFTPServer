using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CodePlex.JPMikkers.TFTP
{
    public class TFTPServer : ITFTPServer
    {
        #region TFTP definitions 

        public class ConfigurationAlternative
        {
            private readonly Regex _regex;
            public ushort WindowSize { get; set; }

            public bool Match(string s)
            {
                return _regex.IsMatch(s);
            }

            private ConfigurationAlternative(Regex regex)
            {
                _regex = regex;
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

        private string _name;
        private UDPSocket _socket;

        internal const int MaxBlockSize = 65464 + 4;
        internal const int DefaultBlockSize = 512;
        internal IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 69);
        internal short _Ttl = -1;
        internal bool _dontFragment = false;
        internal int _maxRetries = 5;
        internal int _responseTimeout = 2000;
        private bool _useSinglePort = false;
        private string _rootPath = ".";
        private bool _allowRead = true;
        private bool _allowWrite = true;
        private bool _autoCreateDirectories = true;
        private bool _convertPathSeparator = true;
        private ushort _windowSize = 1;
        private readonly List<ConfigurationAlternative> _configurationAlternatives = new List<ConfigurationAlternative>();

        private readonly object _sync = new object();
        private bool _active = false;

        private readonly Dictionary<IPEndPoint, ITFTPSession> _sessions;
        private readonly SessionLog _sessionLog = new SessionLog();

        public SessionLog SessionLog
        {
            get { return _sessionLog; }
        }

        private void Stop(Exception reason)
        {
            bool notify = false;

            lock(_sync)
            {
                if(_active)
                {
                    Trace($"Stopping TFTP server '{_serverEndPoint}'");
                    _active = false;
                    notify = true;
                    _socket.Dispose();
                    // get shallow copy of running sessions
                    var sessions = new List<ITFTPSession>();
                    lock(_sessions)
                    {
                        sessions.AddRange(_sessions.Values);
                    }
                    // stop all of them
                    foreach(var session in sessions)
                    {
                        session.Stop();
                    }
                    Trace("Stopped");
                }
            }

            if(notify)
            {
                var data = new TFTPStopEventArgs();
                data.Reason = reason;
                OnStatusChange(this, data);
            }
        }

        private static string StripRoot(string filename)
        {
            // strip root from filename before calling Path.Combine(). Some clients like to prepend a leading backslash, resulting in an 'Illegal filename' error.
            if(Path.IsPathRooted(filename))
            {
                filename = filename.Substring(Path.GetPathRoot(filename).Length);
            }
            return filename;
        }

        private string GetLocalFilename(string filename)
        {
            string result = Path.GetFullPath(Path.Combine(_rootPath, StripRoot(filename)));
            if(!result.StartsWith(_rootPath))
            {
                throw new ArgumentException("Illegal filename");
            }
            return result;
        }

        internal Stream GetReadStream(string filename)
        {
            if(!_allowRead)
            {
                throw new UnauthorizedAccessException("Reading not allowed");
            }
            string targetPath = GetLocalFilename(filename);
            //Console.WriteLine("Getting read stream for file '{0}'", targetPath);
            return File.OpenRead(targetPath);
        }

        internal Stream GetWriteStream(string filename, long length)
        {
            if(!_allowWrite)
            {
                throw new UnauthorizedAccessException("Writing not allowed");
            }

            string targetPath = GetLocalFilename(filename);
            //Console.WriteLine("Getting write stream for file '{0}', size {1}", targetPath, length);

            if(_autoCreateDirectories)
            {
                string dir = Path.GetDirectoryName(targetPath);
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            return File.OpenWrite(targetPath);
        }

        public TFTPServer()
        {
            _name = "TFTPServer";
            _sessions = new Dictionary<IPEndPoint, ITFTPSession>();
        }

        private ushort GetWindowSize(string filename)
        {
            if(_configurationAlternatives.Count > 0)
            {
                filename = StripRoot(filename);

                foreach(ConfigurationAlternative alternative in _configurationAlternatives)
                {
                    if(alternative.Match(filename))
                    {
                        return alternative.WindowSize;
                    }
                }
            }
            return _windowSize;
        }

        private void OnUDPReceive(UDPSocket sender, IPEndPoint endPoint, ArraySegment<byte> data)
        {
            bool notify = false;
            int packetSize = data.Count;
            MemoryStream ms = new MemoryStream(data.Array, data.Offset, data.Count, false, true);

            ITFTPSession session;
            bool found;
            ushort opCode = ReadUInt16(ms);

            lock(_sessions)
            {
                found = _sessions.TryGetValue(endPoint, out session);
            }

            // is there a session in progress for that endpoint?
            if(found)
            {
                // yes.
                switch((Opcode)opCode)
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
                        session.ProcessData(ReadUInt16(ms), new ArraySegment<byte>(data.Array, (int)(data.Offset + ms.Position), (int)(data.Count - ms.Position)));
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
                switch((Opcode)opCode)
                {
                    case Opcode.ReadRequest:
                        {
                            string filename = ReadZString(ms);
                            if(_convertPathSeparator) filename = filename.Replace('/', '\\');
                            Mode mode = ReadMode(ms);
                            var requestedOptions = ReadOptions(ms);
                            ushort windowSize = GetWindowSize(filename);
                            ITFTPSession newSession = new DownloadSession(this, _useSinglePort ? _socket : null, endPoint, requestedOptions, filename, windowSize, OnUDPReceive);

                            lock(_sessions)
                            {
                                _sessions.Add(newSession.RemoteEndPoint, newSession);
                            }

                            notify = true;
                            Trace($"Starting transfer of file '{newSession.Filename}' from local '{newSession.LocalEndPoint}' to remote '{newSession.RemoteEndPoint}', send window size {windowSize}");
                            newSession.Start();
                        }
                        break;

                    case Opcode.WriteRequest:
                        {
                            string filename = ReadZString(ms);
                            if(_convertPathSeparator) filename = filename.Replace('/', '\\');
                            Mode mode = ReadMode(ms);
                            var requestedOptions = ReadOptions(ms);
                            ITFTPSession newSession = new UploadSession(this, _useSinglePort ? _socket : null, endPoint, requestedOptions, filename, OnUDPReceive);

                            lock(_sessions)
                            {
                                _sessions.Add(newSession.RemoteEndPoint, newSession);
                            }

                            notify = true;
                            Trace($"Starting transfer of file '{newSession.Filename}' from remote '{newSession.RemoteEndPoint}' to local '{newSession.LocalEndPoint}'");
                            newSession.Start();
                        }
                        break;

                    default:
                        SendError(_socket, endPoint, (ushort)ErrorCode.UnknownTransferID, "Unknown transfer ID");
                        break;
                }
            }

            if(notify)
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
            lock(_sessions)
            {
                if(_sessions.ContainsKey(session.RemoteEndPoint))
                {
                    if(reason == null)
                    {
                        Trace($"Completed transfer {(session is UploadSession ? "from" : "to")} '{session.RemoteEndPoint}'");
                    }
                    else
                    {
                        Trace($"Aborted transfer {(session is UploadSession ? "from" : "to")} '{session.RemoteEndPoint}', reason '{reason}'");
                    }
                    _sessions.Remove(session.RemoteEndPoint);
                    session.Dispose();
                }
            }

            OnStatusChange(this, null);
        }

        internal void Trace(string msg)
        {
            var data = new TFTPTraceEventArgs();
            data.Message = msg;
            OnTrace(this, data);
        }

        #region Dispose pattern

        ~TFTPServer()
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

        protected void Dispose(bool disposing)
        {
            if(disposing)
            {
                Stop();
            }
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
                return _name;
            }
            set
            {
                _name = value;
            }
        }


        public IPEndPoint EndPoint
        {
            get
            {
                return _serverEndPoint;
            }
            set
            {
                _serverEndPoint = value;
            }
        }

        public bool SinglePort
        {
            get
            {
                return _useSinglePort;
            }
            set
            {
                _useSinglePort = value;
            }
        }

        public short Ttl
        {
            get
            {
                return _Ttl;
            }
            set
            {
                _Ttl = value;
            }
        }

        public bool DontFragment
        {
            get
            {
                return _dontFragment;
            }
            set
            {
                _dontFragment = value;
            }
        }

        public int ResponseTimeout
        {
            get
            {
                return _responseTimeout;
            }
            set
            {
                _responseTimeout = value;
            }
        }

        public int Retries
        {
            get
            {
                return _maxRetries;
            }
            set
            {
                _maxRetries = value;
            }
        }

        public string RootPath
        {
            get
            {
                return _rootPath;
            }
            set
            {
                _rootPath = Path.GetFullPath(value);
            }
        }

        public bool AutoCreateDirectories
        {
            get
            {
                return _autoCreateDirectories;
            }
            set
            {
                _autoCreateDirectories = value;
            }
        }

        public bool ConvertPathSeparator
        {
            get
            {
                return _convertPathSeparator;
            }
            set
            {
                _convertPathSeparator = value;
            }
        }

        public bool AllowRead
        {
            get
            {
                return _allowRead;
            }
            set
            {
                _allowRead = value;
            }
        }

        public bool AllowWrite
        {
            get
            {
                return _allowWrite;
            }
            set
            {
                _allowWrite = value;
            }
        }

        public ushort WindowSize
        {
            get
            {
                return _windowSize;
            }
            set
            {
                _windowSize = Clip<ushort>(value, 1, 32);
            }
        }

        public IList<ConfigurationAlternative> ConfigurationAlternatives
        {
            get
            {
                return _configurationAlternatives;
            }
        }

        public bool Active
        {
            get
            {
                lock(_sync)
                {
                    return _active;
                }
            }
        }

        public int ActiveTransfers
        {
            get
            {
                lock(_sessions)
                {
                    return _sessions.Count;
                }
            }
        }

        public void Start()
        {
            lock(_sync)
            {
                if(!_active)
                {
                    try
                    {
                        int maxWorkerThreads, maxCompletionPortThreads;

                        Trace($"Starting TFTP server '{_serverEndPoint}'");
                        _active = true;
                        _socket = new UDPSocket(_serverEndPoint, MaxBlockSize, _dontFragment, _Ttl, OnUDPReceive, OnUDPStop);
                        Trace($"TFTP Server start succeeded, serving at '{_socket.LocalEndPoint}'");
                        System.Threading.ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);
                        Trace($"Threadpool maxWorkerThreads={maxWorkerThreads} maxCompletionPortThreads={maxCompletionPortThreads}");
                        Trace($"GCSettings.IsServerGC={System.Runtime.GCSettings.IsServerGC}");
                    }
                    catch(Exception e)
                    {
                        Trace($"TFTP Server start failed, reason '{e}'");
                        _active = false;
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
            if(value.CompareTo(minValue) < 0)
                result = minValue;
            else if(value.CompareTo(maxValue) > 0)
                result = maxValue;
            else
                result = value;
            return result;
        }

        internal static Dictionary<string, string> ReadOptions(Stream s)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            while(s.Position < s.Length)
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
            foreach(var s in options)
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
            while(c != 0)
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
            switch(ReadZString(s).ToLower())
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
