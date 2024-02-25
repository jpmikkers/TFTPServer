using System.Globalization;
using Microsoft.Extensions.Logging;
using Avalonia.Media;

namespace AvaTFTPServer;

public class LogLevelColorBrushConverter : StrongTypedValueConverter<LogLevel, IBrush>
{
    protected override IImmutableBrush ViewModelToView(LogLevel input, CultureInfo culture) => input switch
    {
        LogLevel.Trace => Brushes.Purple,
        LogLevel.Debug => Brushes.AliceBlue,
        LogLevel.Information => Brushes.WhiteSmoke,
        LogLevel.Warning => Brushes.Orange,
        LogLevel.Error => Brushes.Red,
        LogLevel.Critical => Brushes.Red,
        _ => Brushes.WhiteSmoke,
    };
    
    protected override LogLevel ViewToViewModel(IBrush input, CultureInfo culture) => LogLevel.None;
}
