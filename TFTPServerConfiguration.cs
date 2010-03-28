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
using System.Net;

namespace TFTPServer
{
    [Serializable()]
    public class TFTPServerConfiguration
    {
        public SerializableIPEndPoint EndPoint { get; set; }
        public bool SinglePort { get; set; }
        public int Ttl { get; set; }
        public bool DontFragment { get; set; }
        public string RootPath { get; set; }
        public bool AutoCreateDirectories { get; set; }
        public bool AllowRead { get; set; }
        public bool AllowWrite { get; set; }
        public int Timeout { get; set; }
        public int Retries { get; set; }

        public TFTPServerConfiguration()
        {
            EndPoint = new IPEndPoint(IPAddress.Loopback, 69);
            SinglePort = false;
            Ttl = -1;
            DontFragment = false;
            RootPath = ".";
            AutoCreateDirectories = true;
            AllowRead = true;
            AllowWrite = true;
            Timeout = 2000;
            Retries = 5;
        }

        public TFTPServerConfiguration Clone()
        {
            TFTPServerConfiguration result = new TFTPServerConfiguration();
            result.EndPoint = EndPoint.Clone();
            result.SinglePort = SinglePort;
            result.Ttl = Ttl;
            result.DontFragment = DontFragment;
            result.RootPath = RootPath;
            result.AutoCreateDirectories = AutoCreateDirectories;
            result.AllowRead = AllowRead;
            result.AllowWrite = AllowWrite;
            result.Timeout = Timeout;
            result.Retries = Retries;
            return result;
        }
    }

    [Serializable()]
    public class SerializableIPEndPoint
    {
        private IPEndPoint m_EndPoint;

        public string Address
        {
            get
            {
                return m_EndPoint.Address.ToString();
            }
            set
            {
                m_EndPoint = new IPEndPoint(IPAddress.Parse(value),m_EndPoint.Port);
            }
        }

        public int Port
        {
            get
            {
                return m_EndPoint.Port;
            }
            set
            {
                m_EndPoint = new IPEndPoint(m_EndPoint.Address,value);
            }
        }

        public SerializableIPEndPoint()
        {
            m_EndPoint = new IPEndPoint(IPAddress.Loopback, 0);
        }

        public SerializableIPEndPoint(IPEndPoint p)
        {
            m_EndPoint = (IPEndPoint)p.Create(p.Serialize());
        }

        static public implicit operator SerializableIPEndPoint(IPEndPoint c)
        {
            return new SerializableIPEndPoint(c);
        }

        static public implicit operator IPEndPoint(SerializableIPEndPoint c)
        {
            return (IPEndPoint)c.m_EndPoint.Create(c.m_EndPoint.Serialize());
        }

        public SerializableIPEndPoint Clone()
        {
            return new SerializableIPEndPoint(m_EndPoint);
        }
    }
}