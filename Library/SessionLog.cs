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
using System.Threading;

namespace CodePlex.JPMikkers.TFTP
{
    public class SessionLog
    {
        private static long s_IDCounter = 0;
        private readonly HashSet<Slot> _sessions = new HashSet<Slot>();
        private readonly Queue<Slot> _sessionQueue = new Queue<Slot>();
        private int _maxItems = 100;
        private readonly WeakTimer _timer;
        private readonly System.Diagnostics.Stopwatch _stopWatch;

        private double ElapsedSeconds
        {
            get
            {
                lock (_stopWatch)
                {
                    return _stopWatch.Elapsed.TotalSeconds;
                }
            }
        }

        public int MaxItems
        {
            get { return _maxItems; }
            set 
            {
                if (value < 0) throw new ArgumentException("Argument must be >= 0", "MaxItems");
                _maxItems = value;
                Purge();
            }
        }

        protected object SyncObject
        {
            get
            {
                return _sessions;
            }
        }

        public interface ISession
        {
            void Progress(long transferred);
            void Stop(Exception e);
            void Complete();
        }

        private class Slot : ISession
        {
            private long _id;
            private long _transferred;
            private SessionLogEntry.TState _state;
            private SessionLog _parent;
            private Exception _exception;
            private double _speed;
            private double _startTime;
            private double _lastSpeedCalculation;
            private long _lastTransferred;
            private SimpleMovingAverage _runningAverage = new SimpleMovingAverage(2);
            protected internal volatile bool _triggerSpeedCalculation;

            public long Id
            {
                get { return _id; }
            }

            public Exception Exception
            {
                get { return _exception; }
            }

            private readonly SessionLogEntry.TConfiguration _configuration;

            public SessionLogEntry.TConfiguration Configuration 
            {
                get { return _configuration; }
            }

            public SessionLogEntry.TState State
            {
                get
                {
                    return _state;
                }
            }
            
            public long Transferred 
            { 
                get { return Interlocked.Read(ref _transferred); }
            }

            public double Speed
            {
                get
                {
                    return _speed;
                }
            }

            private double CalculateSpeed(long transferred, double startTime, double endTime)
            {
                return Math.Max(0,transferred) / Math.Max(0.001,endTime - startTime);
            }

            public Slot(SessionLog parent, long id, SessionLogEntry.TConfiguration sessionInfo)
            {
                _id = id;
                _parent = parent;
                _configuration = sessionInfo;
                _state = SessionLogEntry.TState.Busy;
                _exception = null;
                _transferred = 0;
                _speed = 0.0;
                _startTime = _parent.ElapsedSeconds;
                _lastSpeedCalculation = _startTime;
            }

            public void Progress(long transferred)
            {
                if (_state == SessionLogEntry.TState.Busy)
                {
                    Interlocked.Exchange(ref _transferred, transferred);

                    if (_triggerSpeedCalculation)
                    {
                        _triggerSpeedCalculation = false;
                        double currentTime = _parent.ElapsedSeconds;

                        lock (_runningAverage)
                        {
                            double elapsed = (currentTime - _lastSpeedCalculation);

                            if (elapsed > 0.01)
                            {
                                _speed = _runningAverage.Add(CalculateSpeed(transferred - _lastTransferred, _lastSpeedCalculation, currentTime));
                                _lastTransferred = transferred;
                                _lastSpeedCalculation = currentTime;
                            }
                        }
                    }
                }
            }

            public void Stop(Exception e)
            {
                lock (_parent.SyncObject)
                {
                    if (_state == SessionLogEntry.TState.Busy)
                    {
                        _speed = CalculateSpeed(_transferred, _startTime, _parent.ElapsedSeconds);
                        if (e == null)
                        {
                            _state = SessionLogEntry.TState.Completed;
                        }
                        else
                        {
                            _state = SessionLogEntry.TState.Stopped;
                            _exception = e;
                        }
                    }
                }
            }

            public void Complete()
            {
                Stop(null);
            }

            public void Invalidate()
            {
                _state = SessionLogEntry.TState.Zombie;
            }
        }

        private void Purge()
        {
            lock (_sessions)
            {
                while (_sessionQueue.Count > _maxItems)
                {
                    var sessionToRemove = _sessionQueue.Dequeue();
                    sessionToRemove.Invalidate();
                    _sessions.Remove(sessionToRemove);
                }
            }
        }

        public SessionLog()
        {
            _stopWatch = new System.Diagnostics.Stopwatch();
            _stopWatch.Start();
            _timer = new WeakTimer(OnTimer, null, 1000, 1000);
        }

        private void OnTimer(object state)
        {
            lock (_sessions)
            {
                foreach (var session in _sessions)
                {
                    session._triggerSpeedCalculation = true;
                }
            }
        }

        public ISession CreateSession(SessionLogEntry.TConfiguration args)
        {
            lock (_sessions)
            {
                var result = new Slot(this, Interlocked.Increment(ref s_IDCounter), args);
                _sessionQueue.Enqueue(result);
                _sessions.Add(result);
                Purge();
                return result;
            }
        }

        public List<SessionLogEntry> GetHistory()
        {
            lock (_sessions)
            {
                return _sessions
                    .Select(
                        x => new SessionLogEntry() 
                        { 
                            Id = x.Id,
                            Configuration = x.Configuration, 
                            State = x.State, 
                            Transferred = x.Transferred, 
                            Speed = x.Speed,
                            Exception = x.Exception
                        }
                    ).ToList();
            }
        }
    }
}
