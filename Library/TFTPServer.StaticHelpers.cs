using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Baksteen.Net.TFTP.Server;

public partial class TFTPServer
{
    #region Static helpers

    internal static Dictionary<string, string> ReadOptions(Stream s)
    {
        Dictionary<string, string> options = [];
        while(s.Position < s.Length)
        {
            string key = ReadZString(s).ToLower();
            string val = ReadZString(s).ToLower();
            options.Add(key, val);
        }
        return options;
    }

    internal static async Task SendError(IUDPSocket socket, IPEndPoint endPoint, ushort code, string message, CancellationToken cancellationToken)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.Error);
        WriteUInt16(ms, code);
        WriteZString(ms, message.Substring(0, Math.Min(message.Length, 256)));
        await socket.Send(endPoint, ms.ToArray(), cancellationToken);
    }

    internal static async Task SendError(IUDPSocket socket, IPEndPoint endPoint, ErrorCode code, string message, CancellationToken cancellationToken)
    {
        await SendError(socket, endPoint, (ushort)code, message, cancellationToken);
    }

    internal static ReadOnlyMemory<byte> GetDataAckPacket(ushort blockno)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.Ack);
        WriteUInt16(ms, blockno);
        return ms.ToArray();
    }

    internal static ReadOnlyMemory<byte> GetOptionsAckPacket(Dictionary<string, string> options)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.OptionsAck);
        foreach(var s in options)
        {
            WriteZString(ms, s.Key);
            WriteZString(ms, s.Value);
        }
        return ms.ToArray();
    }

    internal static ReadOnlyMemory<byte> GetDataPacket(ushort blockno, byte[] data, int dataSize)
    {
        MemoryStream ms = new();
        WriteUInt16(ms, (ushort)Opcode.Data);
        WriteUInt16(ms, blockno);
        ms.Write(data, 0, dataSize);
        return ms.ToArray();
    }

    internal static string ReadZeroTerminatedString(Stream s, Encoding encoding)
    {
        var mem = new MemoryStream();
        int c = s.ReadByte();
        while(c > 0) 
        { 
            mem.WriteByte((byte)c);
            c = s.ReadByte();
        }
        mem.Position = 0;
        using var sr = new StreamReader(mem, encoding);
        return sr.ReadToEnd();
    }

    internal static string ReadZString(Stream s)
    {
        return ReadZeroTerminatedString(s, Encoding.ASCII);
    }

    internal static void WriteZString(Stream s, string msg)
    {
        using var sw = new StreamWriter(s, Encoding.ASCII, -1, true);
        sw.Write(msg);
        sw.Flush();
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
        BinaryReader br = new(s);
        return (ushort)IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
    }

    internal static void WriteUInt16(Stream s, ushort v)
    {
        BinaryWriter bw = new(s);
        bw.Write((ushort)IPAddress.HostToNetworkOrder((short)v));
    }
    #endregion
}
