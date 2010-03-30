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

namespace CodePlex.JPMikkers.TFTP
{
    public interface ITFTPServer : IDisposable
    {
        event Action<ITFTPServer,Exception> OnStop;
        event Action<ITFTPServer> OnTransfer;

        IPEndPoint EndPoint { get; set; }
        bool SinglePort { get; set; }
        short Ttl { get; set; }
        bool DontFragment { get; set; }

        int ResponseTimeout { get; set; }
        int Retries { get; set; }

        string RootPath { get; set; }
        bool AutoCreateDirectories { get; set; }
        bool AllowRead { get; set; }
        bool AllowWrite { get; set; }

        bool Active { get; }

        void Start();
        void Stop();
    }
}
