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
        private static long IDCounter = 0;
        private readonly HashSet<Slot> m_Sessions = new HashSet<Slot>();
        private readonly Queue<Slot> m_SessionQueue = new Queue<Slot>();
        private int m_MaxItems = 100;
        private readonly WeakTimer m_Timer;
        private readonly System.Diagnostics.Stopwatch m_StopWatch;

        private double ElapsedSeconds
        {
            get
            {
                lock (m_StopWatch)
                {
                    return m_StopWatch.Elapsed.TotalSeconds;
                }
            }
        }

        public int MaxItems
        {
            get { return m_MaxItems; }
            set 
            {
                if (value < 0) throw new ArgumentException("Argument must be >= 0", "MaxItems");
                m_MaxItems = value;
                Purge();
            }
        }

        protected object SyncObject
        {
            get
            {
                return m_Sessions;
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
            private long m_Id;
            private long m_Transferred;
            private SessionLogEntry.TState m_State;
            private SessionLog m_Parent;
            private Exception m_Exception;
            private double m_Speed;
            private double m_StartTime;
            private double m_LastSpeedCalculation;
            private long m_LastTransferred;
            private SimpleMovingAverage m_RunningAverage = new SimpleMovingAverage(2);
            protected internal volatile bool m_TriggerSpeedCalculation;

            public long Id
            {
                get { return m_Id; }
            }

            public Exception Exception
            {
                get { return m_Exception; }
            }

            private readonly SessionLogEntry.TConfiguration m_Configuration;

            public SessionLogEntry.TConfiguration Configuration 
            {
                get { return m_Configuration; }
            }

            public SessionLogEntry.TState State
            {
                get
                {
                    return m_State;
                }
            }
            
            public long Transferred 
            { 
                get { return Interlocked.Read(ref m_Transferred); }
            }

            public double Speed
            {
                get
                {
                    return m_Speed;
                }
            }

            private double CalculateSpeed(long transferred, double startTime, double endTime)
            {
                return Math.Max(0,transferred) / Math.Max(0.001,endTime - startTime);
            }

            public Slot(SessionLog parent, long id, SessionLogEntry.TConfiguration sessionInfo)
            {
                m_Id = id;
                m_Parent = parent;
                m_Configuration = sessionInfo;
                m_State = SessionLogEntry.TState.Busy;
                m_Exception = null;
                m_Transferred = 0;
                m_Speed = 0.0;
                m_StartTime = m_Parent.ElapsedSeconds;
                m_LastSpeedCalculation = m_StartTime;
            }

            public void Progress(long transferred)
            {
                if (m_State == SessionLogEntry.TState.Busy)
                {
                    Interlocked.Exchange(ref m_Transferred, transferred);

                    if (m_TriggerSpeedCalculation)
                    {
                        m_TriggerSpeedCalculation = false;
                        double currentTime = m_Parent.ElapsedSeconds;

                        lock (m_RunningAverage)
                        {
                            double elapsed = (currentTime - m_LastSpeedCalculation);

                            if (elapsed > 0.01)
                            {
                                m_Speed = m_RunningAverage.Add(CalculateSpeed(transferred - m_LastTransferred, m_LastSpeedCalculation, currentTime));
                                m_LastTransferred = transferred;
                                m_LastSpeedCalculation = currentTime;
                            }
                        }
                    }
                }
            }

            public void Stop(Exception e)
            {
                lock (m_Parent.SyncObject)
                {
                    if (m_State == SessionLogEntry.TState.Busy)
                    {
                        m_Speed = CalculateSpeed(m_Transferred, m_StartTime, m_Parent.ElapsedSeconds);
                        if (e == null)
                        {
                            m_State = SessionLogEntry.TState.Completed;
                        }
                        else
                        {
                            m_State = SessionLogEntry.TState.Stopped;
                            m_Exception = e;
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
                m_State = SessionLogEntry.TState.Zombie;
            }
        }

        private void Purge()
        {
            lock (m_Sessions)
            {
                while (m_SessionQueue.Count > m_MaxItems)
                {
                    var sessionToRemove = m_SessionQueue.Dequeue();
                    sessionToRemove.Invalidate();
                    m_Sessions.Remove(sessionToRemove);
                }
            }
        }

        public SessionLog()
        {
            m_StopWatch = new System.Diagnostics.Stopwatch();
            m_StopWatch.Start();
            m_Timer = new WeakTimer(OnTimer, null, 1000, 1000);
        }

        private void OnTimer(object state)
        {
            lock (m_Sessions)
            {
                foreach (var session in m_Sessions)
                {
                    session.m_TriggerSpeedCalculation = true;
                }
            }
        }

        public ISession CreateSession(SessionLogEntry.TConfiguration args)
        {
            lock (m_Sessions)
            {
                var result = new Slot(this, Interlocked.Increment(ref IDCounter), args);
                m_SessionQueue.Enqueue(result);
                m_Sessions.Add(result);
                Purge();
                return result;
            }
        }

        public List<SessionLogEntry> GetHistory()
        {
            lock (m_Sessions)
            {
                return m_Sessions
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
