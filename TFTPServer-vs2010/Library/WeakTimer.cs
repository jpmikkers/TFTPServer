using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Threading;
using System.Reflection;

namespace CodePlex.JPMikkers.TFTP
{
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

            if (handler != null)
            {
                m_Invoker(handler, state);
            }
            else
            {
                m_Timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Change(int dueTime, int period)
        {
            m_Timer.Change(dueTime, period);
        }
    }
}
