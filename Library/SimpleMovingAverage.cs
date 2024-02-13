using System.Collections.Generic;

namespace Baksteen.Net.TFTP.Server;

public class SimpleMovingAverage
{
    private readonly Queue<double> _history;
    private readonly int _windowSize;
    private double _sum;

    public SimpleMovingAverage(int windowSize)
    {
        _history = new Queue<double>();
        _windowSize = windowSize;
    }

    public double Add(double v)
    {
        if(_history.Count == _windowSize)
        {
            _sum -= _history.Dequeue();
        }
        _sum += v;
        _history.Enqueue(v);
        return _sum / _history.Count;
    }
}
