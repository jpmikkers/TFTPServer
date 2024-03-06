using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace AvaTFTPServer.Logging;

public sealed class GuiLogger : ILogger, IDisposable
{
    private readonly ConcurrentQueue<LogItem> _logItems = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;   // getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

    private Action<LogItem> _onLogItem;

    public GuiLogger()
    {
        _onLogItem = x => _logItems.Enqueue(x);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if(!IsEnabled(logLevel)) return;

        var timestamp = DateTime.UtcNow;

        var sw = new StringWriter();
        //sw.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{eventId.Id,2}: {logLevel,-12}]");
        //sw.Write($" {name} - ");
        sw.Write($"{formatter(state, exception)}");

        using var sr = new StringReader(sw.ToString());

        while(sr.ReadLine() is string line)
        {
            _onLogItem(new() { TimestampUtc = timestamp, Id = eventId, Level = logLevel, Text = line });
        }

        //ColorConsoleLoggerConfiguration config = getCurrentConfig();
        //if(config.EventId == 0 || config.EventId == eventId.Id)
        //{
        //    ConsoleColor originalColor = Console.ForegroundColor;

        //    Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        //    Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

        //    Console.ForegroundColor = originalColor;
        //    Console.Write($"     {name} - ");

        //    Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        //    Console.Write($"{formatter(state, exception)}");

        //    Console.ForegroundColor = originalColor;
        //    Console.WriteLine();
        //}
    }

    public void RegisterCallback(Action<LogItem> callback)
    {
        while(_logItems.TryDequeue(out var item)) { callback(item); }
        _onLogItem = callback;
    }

    public void UnregisterCallback()
    {
        _onLogItem = x => { };
    }

    public void Dispose()
    {
        UnregisterCallback();
    }
}
