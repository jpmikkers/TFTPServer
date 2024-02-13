using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace CodePlex.JPMikkers.TFTP;

#if NEVER
/// <summary>
/// Drop-in replacement for <see cref="System.Threading.Timer"/> that, unlike the original, doesn't prevent garbage collection of the callback target.
/// </summary>
public class WeakTimer
{
    private readonly Timer _timer;
    private readonly WeakReference _weakTarget;
    private readonly Action<object, object> _invoker;

    public WeakTimer(TimerCallback tc, object state, int dueTime, int period)
    {
        if(tc.Method.IsStatic)
        {
            _timer = new Timer(tc, state, dueTime, period);
        }
        else
        {
            _weakTarget = new WeakReference(tc.Target);
            _invoker = GenerateInvoker(tc.Method);
            _timer = new Timer(MyCallback, state, dueTime, period);
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
        object handler = _weakTarget.Target;

        // check whether target has been disposed.
        if(handler != null)
        {
            // target is still alive, call it
            _invoker(handler, state);
        }
        else
        {
            // target was collected, stop trying to call it
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public void Change(int dueTime, int period)
    {
        _timer.Change(dueTime, period);
    }
}
#endif