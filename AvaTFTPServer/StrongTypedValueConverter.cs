using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public abstract class StrongTypedValueConverter<ViewModelType, ViewType> : IValueConverter
{
    protected abstract ViewType ViewModelToView(ViewModelType input, CultureInfo culture);
    protected abstract ViewModelType ViewToViewModel(ViewType input, CultureInfo culture);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is ViewModelType input // && parameter is string targetCase
            && targetType.IsAssignableTo(typeof(ViewType)))
        {
            return ViewModelToView(input, culture);
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is ViewType input // && parameter is string targetCase
            && targetType.IsAssignableTo(typeof(ViewModelType)))
        {
            return ViewToViewModel(input, culture);
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
}
