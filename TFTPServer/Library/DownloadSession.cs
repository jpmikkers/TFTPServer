﻿/*

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
    internal class DownloadSession : TFTPSession
    {
        private byte[] m_DataBuffer;
        private int m_DataSize;
        private ushort m_DataNumber;

        public DownloadSession(
            TFTPServer parent, 
            UDPSocket socket,
            IPEndPoint remoteEndPoint, 
            Dictionary<string, string> requestedOptions, 
            string filename, 
            UDPSocket.OnReceiveDelegate onReceive)
            : base(parent,socket,remoteEndPoint,requestedOptions,filename, onReceive, 0)
        {
            m_DataBuffer = new byte[m_CurrentBlockSize];
            m_DataNumber = 0;
            m_DataSize = 0;
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
                        SendResponse();
                    }
                    else
                    {
                        m_BlockNumber = 1;
                        SendResponse();
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
            if (m_FirstBlock && m_BlockNumber == 0)
            {
                // send options ack -> client will respond with ack for block number 0
                TFTPServer.SendOptionsAck(m_Socket, m_RemoteEndPoint, m_AcceptedOptions);
            }
            else
            {
                if (m_DataNumber != m_BlockNumber)
                {
                    m_DataSize = m_Stream.Read(m_DataBuffer, 0, m_CurrentBlockSize);
                    m_DataNumber = m_BlockNumber;
                    m_LastBlock = (m_DataSize < m_CurrentBlockSize);
                }
                TFTPServer.SendData(m_Socket, m_RemoteEndPoint, m_BlockNumber, m_DataBuffer, m_DataSize);
            }
            StartTimer();
        }

        public override void ProcessAck(ushort blockNr)
        {
            bool isComplete = false;

            lock (m_Lock)
            {
                //Console.WriteLine("block {0} was ackd", blockNr);

                if (blockNr == m_BlockNumber)
                {
                    m_FirstBlock = false;

                    if (!m_LastBlock)
                    {
                        m_BlockRetry = 0;
                        m_BlockNumber++;
                        SendResponse();
                    }
                    else
                    {
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
