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
using System.Net;
using System.Threading;

namespace CodePlex.JPMikkers.TFTP
{
    internal class WindowEntry
    {
        public bool IsData;
        public int Length;
        public ArraySegment<byte> Segment;
    }

    internal class DownloadSession : TFTPSession
    {
        private long m_Position;
        private ushort m_WindowSize;

        private List<WindowEntry> m_Window = new List<WindowEntry>();

        public DownloadSession(
            TFTPServer parent, 
            UDPSocket socket,
            IPEndPoint remoteEndPoint, 
            Dictionary<string, string> requestedOptions, 
            string filename, 
            ushort windowSize,
            UDPSocket.OnReceiveDelegate onReceive)
            : base(parent,socket,remoteEndPoint,requestedOptions,filename, onReceive, 0)
        {
            m_WindowSize = windowSize;
        }

        public override void Start()
        {
            try
            {
                lock (m_Lock)
                {
                    try
                    {
                        m_Stream = m_Parent.GetReadStream(m_Filename);
                        m_Length = m_Stream.Length;
                        m_Position = 0;
                    }
                    catch (Exception e)
                    {
                        TFTPServer.SendError(m_Socket, m_RemoteEndPoint, TFTPServer.ErrorCode.FileNotFound, e.Message);
                        throw;
                    }

                    // handle tsize option
                    if (m_RequestedOptions.ContainsKey(TFTPServer.Option_TransferSize))
                    {
                        if (m_Length >= 0)
                        {
                            m_AcceptedOptions.Add(TFTPServer.Option_TransferSize, m_Length.ToString());
                        }
                    }

                    if (m_AcceptedOptions.Count > 0)
                    {
                        m_BlockNumber = 0;
                        SendOptionsAck();
                    }
                    else
                    {
                        m_BlockNumber = 1;
                        SendData();
                    }
                }
            }
            catch (Exception e)
            {
                Stop(true, e);
            }
        }

        protected override void SendResponse()
        {
            // resend blocks in the window
            for (int t = 0; t < m_Window.Count; t++)
            {
                TFTPServer.Send(m_Socket, m_RemoteEndPoint, m_Window[t].Segment);
            }
            StartTimer();
        }

        private void SendOptionsAck()
        {
            var seg = TFTPServer.GetOptionsAckPacket(m_AcceptedOptions);
            m_Window.Add(new WindowEntry() { IsData = false, Length = 0, Segment = seg });
            TFTPServer.Send(m_Socket, m_RemoteEndPoint, seg);
            m_BlockRetry = 0;
            StartTimer();
        }

        private void SendData()
        {
            // fill the window up to the window size & send all the new packets
            while (m_Window.Count < m_WindowSize && !m_LastBlock)
            {
                byte[] buffer = new byte[m_CurrentBlockSize];
                int dataSize = m_Stream.Read(buffer, 0, m_CurrentBlockSize);
                var seg = TFTPServer.GetDataPacket((ushort)(m_BlockNumber + m_Window.Count), buffer, dataSize);
                m_Window.Add(new WindowEntry() { IsData = true, Length = dataSize, Segment=seg });
                m_LastBlock = (dataSize < m_CurrentBlockSize);
                TFTPServer.Send(m_Socket, m_RemoteEndPoint, seg);
                m_BlockRetry = 0;
                StartTimer();
            }
        }

        private bool WindowContainsBlock(ushort blocknr)
        {
            // rollover safe way of checking: m_BlockNumber <= blocknr < (m_BlockNumber+m_Window.Count)
            return ((ushort)(blocknr-m_BlockNumber)) < m_Window.Count;
        }

        public override void ProcessAck(ushort blockNr)
        {
            bool isComplete = false;

            lock (m_Lock)
            {
                if (WindowContainsBlock(blockNr))
                {
                    while (WindowContainsBlock(blockNr))
                    {
                        if(m_Window[0].IsData) m_Position += m_Window[0].Length;
                        m_BlockNumber++;
                        m_Window.RemoveAt(0);
                    }

                    SendData();

                    if (m_Window.Count == 0)
                    {
                        // Everything was acked
                        isComplete = true;
                        StopTimer();
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
