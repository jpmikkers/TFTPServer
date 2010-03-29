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
        private class UploadSession : SessionBase
        {
            protected override void SendResponse()
            {
                //Console.WriteLine("Sending ack {0}", m_BlockNumber);

                try
                {
                    if (m_FirstBlock)
                    {
                        if (m_AcceptedOptions.Count > 0)
                        {
                            // send options ack, client will respond with block number 1
                            TFTPServer.SendOptionsAck(m_Socket, m_RemoteEndPoint, m_AcceptedOptions);
                        }
                        else
                        {
                            // send ack for block 0, client will respond with block number 1
                            TFTPServer.SendAck(m_Socket, m_RemoteEndPoint, 0);
                        }
                    }
                    else
                    {
                        // send ack for current block, client will respond with next block
                        TFTPServer.SendAck(m_Socket, m_RemoteEndPoint, m_BlockNumber);
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine("TFTPServer.Upload : Error in SendResponse() : {0}", e);
                }

                if (!m_LastBlock)
                {
                    m_Timer.Change(m_ResponseTimeout, Timeout.Infinite);
                }
                else
                {
                    m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }

            public UploadSession(TFTPServer parent, UDPSocket socket, IPEndPoint remoteEndPoint, Dictionary<string, string> requestedOptions, string filename, UDPSocket.OnReceiveDelegate onReceive)
                : base(parent,socket,remoteEndPoint,requestedOptions,filename, onReceive, 1000)
            {
                lock (m_Lock)
                {
                    try
                    {
                        m_Stream = m_Parent.GetWriteStream(m_Filename, m_RequestedOptions.ContainsKey("tsize") ? Int64.Parse(m_RequestedOptions["tsize"]) : -1);
                    }
                    catch(Exception e)
                    {
                        SendError(m_Socket, m_RemoteEndPoint, ErrorCode.FileNotFound, e.Message);
                        Dispose();
                        return;
                    }

                    // handle tsize option
                    if (m_RequestedOptions.ContainsKey("tsize"))
                    {
                        // rfc2349: in Write Request packets, the size of the file, in octets, is specified in the 
                        // request and echoed back in the OACK
                        m_AcceptedOptions.Add("tsize", m_RequestedOptions["tsize"]);
                    }

                    m_Parent.TransferStart(this);
                    SendResponse();
                }
            }

            private static void TransferBytes(Stream from, Stream to)
            {
                int bytesTodo;
                byte[] buffer = new byte[MaxBlockSize];

                do
                {
                    bytesTodo = from.Read(buffer, 0, buffer.Length);
                    to.Write(buffer, 0, bytesTodo);
                } while (bytesTodo == MaxBlockSize);
            }

            public override void ProcessData(ushort blockNr, Stream ms)
            {
                bool isComplete = false;

                lock (m_Lock)
                {
                    if (blockNr == ((ushort)(blockNr+1)))
                    {
                        m_FirstBlock = false;
                        m_BlockRetry = 0;
                        m_BlockNumber = blockNr;
                        m_LastBlock = (ms.Length - ms.Position) < m_CurrentBlockSize;
                        isComplete = m_LastBlock;
                        TransferBytes(ms, m_Stream);
                        SendResponse();
                    }
                }

                if (isComplete)
                {
                    //Console.WriteLine("transfer complete");
                    m_Parent.TransferComplete(this, null);
                }
            }
        }
    }
}
