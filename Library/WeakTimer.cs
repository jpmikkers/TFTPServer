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
using System.Linq.Expressions;
using System.Threading;
using System.Reflection;

namespace CodePlex.JPMikkers.TFTP
{
    /// <summary>
    /// Drop-in replacement for <see cref="System.Threading.Timer"/> that, unlike the original, doesn't prevent garbage collection of the callback target.
    /// </summary>
    public class WeakTimer
    {
        private readonly Timer m_Timer;
        private readonly WeakReference m_WeakTarget;
        private readonly Action<object, object> m_Invoker;

        public WeakTimer(TimerCallback tc, object state, int dueTime, int period)
        {
            if (tc.Method.IsStatic)
            {
                m_Timer = new Timer(tc, state, dueTime, period);
            }
            else
            {
                m_WeakTarget = new WeakReference(tc.Target);
                m_Invoker = GenerateInvoker(tc.Method);
                m_Timer = new Timer(MyCallback, state, dueTime, period);
            }
        }

        private static Action<object, object> GenerateInvoker(MethodInfo method)
        {
            var instExpr = Expression.Parameter(typeof(object), "instance");
            var paramExpr = Expression.Parameter(typeof(object), "state");
            // ((Method.DeclaringType)instance).Method(param)
            var invokeExpr = Expression.Call(Expression.Convert(instExpr, method.DeclaringType), method, paramExpr);
            return Expression.Lambda<Action<object, object>>(invokeExpr, instExpr, paramExpr).Compile();
        }

        private void MyCallback(object state)
        {
            object handler = m_WeakTarget.Target;

            // check whether target has been disposed.
            if (handler != null)
            {
                // target is still alive, call it
                m_Invoker(handler, state);
            }
            else
            {
                // target was collected, stop trying to call it
                m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Change(int dueTime, int period)
        {
            m_Timer.Change(dueTime, period);
        }
    }
}
