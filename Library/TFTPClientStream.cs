using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace CodePlex.JPMikkers.TFTP
{
    public class TFTPClientStream : Stream
    {
        private enum State
        {
            Idle,
            Initialized
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

        private long m_TransferSize;
        private string m_RemoteFilename;
        private int m_BlockSize;
        private int m_Timeout;

        private State m_State = State.Idle;
        bool m_IPv6;
        internal const int MaxBlockSize = 65464 + 4;
        private EndPoint m_LocalEndPoint;
        protected IPEndPoint m_RemoteEndPoint;
        private Socket m_Socket;
        private bool m_ReadMode;
        private byte[] m_ReceiveBuffer = new byte[MaxBlockSize];
        
        private byte[] m_WriteBuffer = new byte[MaxBlockSize];
        private byte[] m_WriteFilled;

        protected bool m_FirstBlock;
        protected bool m_LastBlock;
        protected ushort m_BlockNumber;
        protected ushort m_BlockRetry;

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = 0;
            if (!m_ReadMode) throw new IOException("Can't read from a TFTPClientStream in write mode");

            switch (m_State)
            {
                case State.Idle:
                    StartReadSession();
                    m_State = State.Initialized;
                    result=Read(buffer, offset, count);
                    break;

                case State.Initialized:
                    break;
            }

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if(m_ReadMode) throw new IOException("Can't write to a TFTPClientStream in read mode");

            switch (m_State)
            {
                case State.Idle:
                    StartWriteSession();
                    m_State = State.Initialized;
                    Write(buffer, offset, count);
                    break;

                case State.Initialized:
                    break;
            }
        }

        private void StartReadSession()
        {
        }

        private void StartWriteSession()
        {
            SendRequest(m_RemoteFilename, m_Timeout, m_BlockSize, m_TransferSize);
            EndPoint endPoint = new IPEndPoint(0, 0);
            int bytesReceived=m_Socket.ReceiveFrom(m_ReceiveBuffer, ref endPoint);
            // accept OACK, ACK, ERROR
            MemoryStream ms = new MemoryStream(m_ReceiveBuffer,0,bytesReceived, false, true);
            ushort opcode = ReadUInt16(ms);

            switch ((Opcode)opcode)
            {
                case Opcode.OptionsAck:
                    var options = ReadOptions(ms);

                    if (m_BlockSize >= 1 && options.ContainsKey(Option_BlockSize))
                    {
                        m_BlockSize = int.Parse(options[Option_BlockSize]);
                    }
                    else
                    {
                        m_BlockSize = 512;
                    }

                    if (m_Timeout >= 1 && options.ContainsKey(Option_Timeout))
                    {
                        m_Timeout = int.Parse(options[Option_Timeout]);
                    }
                    else
                    {
                        m_Timeout = 10;
                    }

                    if (m_TransferSize >= 0 && options.ContainsKey(Option_TransferSize))
                    {
                        m_TransferSize = long.Parse(options[Option_TransferSize]);
                    }
                    else
                    {
                        m_TransferSize = -1;
                    }

                    break;

                case Opcode.Ack:
                    m_Timeout = 10;
                    m_BlockSize = 512;
                    m_TransferSize = -1;
                    ushort blockNr = ReadUInt16(ms);
                    if(blockNr!=0) throw new IOException("Server did not initiate WriteSession with ACK(0)");
                    break;

                case Opcode.Error:
                    ushort code = ReadUInt16(ms);
                    string msg = ReadZString(ms);
                    throw new IOException(string.Format("TFTP Error {0} : {1}", code, msg));
            }
        }

        public TFTPClientStream(IPEndPoint localEndPoint,IPEndPoint remoteEndPoint,string remoteFilename, bool read, bool dontFragment, short ttl, long transferSize, int blockSize, int timeout)
        {
            m_FirstBlock = true;
            m_LastBlock = false;
            m_BlockNumber = 0;
            m_BlockRetry = 0;

            m_RemoteFilename = remoteFilename;
            m_TransferSize = transferSize;
            m_BlockSize = blockSize;
            m_Timeout = timeout;

            m_IPv6 = (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6);
            m_Socket = new Socket(localEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            m_Socket.SendBufferSize = 65536;
            m_Socket.ReceiveBufferSize = 65536;
            m_Socket.ReceiveTimeout = 10000;
 
            if(!m_IPv6) m_Socket.DontFragment = dontFragment;
            if (ttl >= 0)
            {
                m_Socket.Ttl = ttl;
            }
            m_Socket.Bind(localEndPoint);
            m_LocalEndPoint = m_Socket.LocalEndPoint;
            m_ReadMode = read;
        }

        private void SendRequest(string filename, int timeOut, int blockSize, long transferSize)
        {
            MemoryStream ms = new MemoryStream();

            WriteUInt16(ms, m_ReadMode ? (ushort)Opcode.ReadRequest : (ushort)Opcode.WriteRequest);
            WriteZString(ms, filename);
            WriteZString(ms, "octet");

            if (timeOut >= 1)
            {
                WriteZString(ms, Option_Timeout);
                WriteZString(ms, timeOut.ToString());
            }

            if (blockSize >= 1)
            {
                WriteZString(ms, Option_BlockSize);
                WriteZString(ms, blockSize.ToString());
            }

            if (transferSize >= 0)
            {
                WriteZString(ms, Option_TransferSize);
                WriteZString(ms, transferSize.ToString());
            }

            m_Socket.SendTo(ms.ToArray(), m_RemoteEndPoint);
        }

        internal static void WriteZString(Stream s, string msg)
        {
            TextWriter tw = new StreamWriter(s, Encoding.ASCII);
            tw.Write(msg);
            tw.Flush();
            s.WriteByte(0);
        }

        internal static void WriteUInt16(Stream s, ushort v)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((ushort)IPAddress.HostToNetworkOrder((short)v));
        }

        internal static ushort ReadUInt16(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            return (ushort)IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
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
    }
}
