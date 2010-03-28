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
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.Configuration;
using System.IO;
using System.Threading;

namespace TFTPServer
{
    public partial class TFTPServer
    {
        #region Static helpers
        private static Dictionary<string, string> ReadOptions(Stream s)
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

        private static void Send(UDPSocket socket, IPEndPoint endPoint, MemoryStream ms)
        {
            socket.Send(endPoint, new ArraySegment<byte>(ms.ToArray()));
        }

        private static void SendError(UDPSocket socket, IPEndPoint endPoint, ushort code, string message)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Error);
            WriteUInt16(ms, code);
            WriteZString(ms, message.Substring(0, Math.Min(message.Length, 256)));
            Send(socket, endPoint, ms);
        }

        private static void SendError(UDPSocket socket, IPEndPoint endPoint, ErrorCode code, string message)
        {
            SendError(socket, endPoint, (ushort)code, message);
        }

        private static void SendAck(UDPSocket socket, IPEndPoint endPoint, ushort blockno)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Ack);
            WriteUInt16(ms, blockno);
            Send(socket, endPoint, ms);
        }

        private static void SendData(UDPSocket socket, IPEndPoint endPoint, ushort blockno, byte[] data, int dataSize)
        {
            MemoryStream ms = new MemoryStream();
            WriteUInt16(ms, (ushort)Opcode.Data);
            WriteUInt16(ms, blockno);
            ms.Write(data, 0, dataSize);
            Send(socket, endPoint, ms);
        }

        private static void SendOptionsAck(UDPSocket socket, IPEndPoint endPoint, Dictionary<string, string> options)
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

        private static string ReadZString(Stream s)
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

        private static void WriteZString(Stream s, string msg)
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

        private static ushort ReadUInt16(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            return (ushort)IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
        }

        private static void WriteUInt16(Stream s, ushort v)
        {
            BinaryWriter bw = new BinaryWriter(s);
            bw.Write((ushort)IPAddress.HostToNetworkOrder((short)v));
        }
        #endregion
    }
}
