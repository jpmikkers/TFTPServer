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
    internal class UploadSession : TFTPSession
    {
        private long m_Position;
        private ArraySegment<byte> m_Window;

        public UploadSession(TFTPServer parent, UDPSocket socket, IPEndPoint remoteEndPoint, Dictionary<string, string> requestedOptions, string filename, UDPSocket.OnReceiveDelegate onReceive)
            : base(parent,socket,remoteEndPoint,requestedOptions,filename, onReceive, 1000)
        {
        }

        public override void Start()
        {
            var sessionLogConfiguration = new SessionLogEntry.TConfiguration()
                                             {
                                                 FileLength = -1,
                                                 Filename = m_Filename,
                                                 IsUpload = true,
                                                 LocalEndPoint = m_LocalEndPoint,
                                                 RemoteEndPoint = m_RemoteEndPoint,
                                                 StartTime = DateTime.Now,
                                                 WindowSize = 1
                                             };
            try
            {
                lock (m_Lock)
                {
                    try
                    {
                        m_Length = m_RequestedOptions.ContainsKey(TFTPServer.Option_TransferSize) ? Int64.Parse(m_RequestedOptions[TFTPServer.Option_TransferSize]) : -1;
                        sessionLogConfiguration.FileLength = m_Length;
                        m_Stream = m_Parent.GetWriteStream(m_Filename, m_Length);
                        m_Position = 0;
                    }
                    catch (Exception e)
                    {
                        TFTPServer.SendError(m_Socket, m_RemoteEndPoint, TFTPServer.ErrorCode.FileNotFound, e.Message);
                        throw;
                    }
                    finally
                    {
                        // always create a SessionLog (even if the file couldn't be opened), so Stop() will have somewhere to store its errors
                        m_SessionLog = m_Parent.SessionLog.CreateSession(sessionLogConfiguration);
                    }

                    // handle tsize option
                    if (m_RequestedOptions.ContainsKey(TFTPServer.Option_TransferSize))
                    {
                        // rfc2349: in Write Request packets, the size of the file, in octets, is specified in the 
                        // request and echoed back in the OACK
                        m_AcceptedOptions.Add(TFTPServer.Option_TransferSize, m_RequestedOptions[TFTPServer.Option_TransferSize]);
                    }

                    if (m_AcceptedOptions.Count > 0)
                    {
                        // send options ack, client will respond with block number 1
                        m_Window = TFTPServer.GetOptionsAckPacket(m_AcceptedOptions);
                    }
                    else
                    {
                        // send ack for current block, client will respond with next block
                        m_Window = TFTPServer.GetDataAckPacket(m_BlockNumber);
                    }

                    TFTPServer.Send(m_Socket, m_RemoteEndPoint, m_Window);
                    m_BlockRetry = 0;
                    StartTimer();
                }
            }
            catch (Exception e)
            {
                Stop(true, e);
            }
        }

        protected override void SendResponse()
        {
            TFTPServer.Send(m_Socket, m_RemoteEndPoint, m_Window);
            StartTimer();
        }

        public override void ProcessData(ushort blockNr, ArraySegment<byte> data)
        {
            bool isComplete = false;

            lock (m_Lock)
            {
                if (blockNr == ((ushort)(m_BlockNumber+1)))
                {
                    m_BlockNumber = blockNr;
                    m_LastBlock = data.Count < m_CurrentBlockSize;
                    isComplete = m_LastBlock;
                    m_Stream.Write(data.Array, data.Offset, data.Count);

                    m_Position += data.Count;
                    m_SessionLog.Progress(m_Position);

                    // send ack for current block, client will respond with next block
                    m_Window = TFTPServer.GetDataAckPacket(m_BlockNumber);
                    TFTPServer.Send(m_Socket, m_RemoteEndPoint, m_Window);
                    m_BlockRetry = 0;

                    if (m_LastBlock)
                    {
                        StopTimer();
                        m_SessionLog.Complete();
                    }
                    else
                    {
                        StartTimer();
                    }
                }
            }

            if (isComplete)
            {
                Stop(true, null);
            }
        }
    }
}
