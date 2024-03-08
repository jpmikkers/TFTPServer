using System;
using System.Collections.Generic;

namespace AvaTFTPServer;

internal static class IListExtensions
{
    public static void RemoveAll<T>(this IList<T> list, Func<T, bool> predicate)
    {
        for(int t = (list.Count - 1); t >= 0; t--)
        {
            if(predicate(list[t])) list.RemoveAt(t);
        }
    }

    public static void ReplaceAll<T>(this IList<T> list, Func<T, bool> predicate, Func<T, T> factory)
    {
        for(int t = (list.Count - 1); t >= 0; t--)
        {
            var item = list[t];
            if(predicate(item)) list.RemoveAt(t);
            list.Insert(t, factory(item));
        }
    }
}
