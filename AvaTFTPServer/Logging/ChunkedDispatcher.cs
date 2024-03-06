using Avalonia.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AvaTFTPServer.Logging;

public class ChunkedDispatcher<T>(IDispatcher dispatcher, Action<IEnumerable<T>> onItems)
{
    private ConcurrentQueue<T> _chunkBuffer = new();

    private void PumpItems()
    {
        var tmp = new List<T>();
        while(_chunkBuffer.TryDequeue(out var item))
        {
            tmp.Add(item);
        }
        onItems(tmp);
    }

    public void Post(T item)
    {
        bool wasEmpty = _chunkBuffer.IsEmpty;
        _chunkBuffer.Enqueue(item);

        if(wasEmpty)
        {
            dispatcher.Post(PumpItems, DispatcherPriority.Background);
        }
    }
}
