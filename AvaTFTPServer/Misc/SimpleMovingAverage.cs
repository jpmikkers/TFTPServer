using System.Collections.Generic;

namespace AvaTFTPServer.Misc;

public class SimpleMovingAverage(int windowSize)
{
    private readonly Queue<double> _history = new(windowSize);
    private double _sum;

    public double Add(double v)
    {
        if(_history.Count == windowSize)
        {
            _sum -= _history.Dequeue();
        }
        _sum += v;
        _history.Enqueue(v);
        return _sum / _history.Count;
    }
}
