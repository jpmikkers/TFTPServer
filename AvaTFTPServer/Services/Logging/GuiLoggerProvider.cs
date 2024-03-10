using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AvaTFTPServer.Services.Logging;

//[UnsupportedOSPlatform("browser")]
[ProviderAlias("GuiLogger")]
public sealed class GuiLoggerProvider : ILoggerProvider
{
    //private readonly IDisposable? _onChangeToken;
    //private ColorConsoleLoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, GuiLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly GuiLogger _guiLogger;

    //public ColorConsoleLoggerProvider(
    //    IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
    //{
    //    _currentConfig = config.CurrentValue;
    //    _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    //}

    public GuiLoggerProvider(GuiLogger guiLogger)
    {
        _guiLogger = guiLogger;
        //_currentConfig = config.CurrentValue;
        //_onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => _guiLogger);

    //private ColorConsoleLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        //_onChangeToken?.Dispose();
    }
}
