namespace Baksteen.Net.TFTP.Server;
using System;
using System.Threading;

public class DelayedDisposer
{
    private readonly Timer _timer;

    private DelayedDisposer(IDisposable obj, int timeOut)
    {
        _timer = new Timer(x => { obj.Dispose(); _timer.Dispose(); });
        _timer.Change(timeOut, Timeout.Infinite);
    }

    public static void QueueDelayedDispose(IDisposable obj, int timeOut)
    {
        new DelayedDisposer(obj, timeOut);
    }
}
