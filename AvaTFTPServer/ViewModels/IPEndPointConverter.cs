using Avalonia.Data.Converters;
using Avalonia.Data;
using System.Globalization;
using System.Linq;
using System;
using System.Net;

namespace AvaTFTPServer.ViewModels;

public class IPEndPointConverter : IValueConverter
{
    public static readonly IPEndPointConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is IPEndPoint sourceNumber && targetType.IsAssignableTo(typeof(string)))
        {
            return sourceNumber.ToString();
        }
        // converter used for the wrong type
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is string sourceText && targetType.IsAssignableTo(typeof(IPEndPoint)))
        {
            //var filtered = string.Join("", sourceText.Where(c => Char.IsDigit(c)).Select(c => new string(c, 1)));
            //if(int.TryParse(filtered, out var actualValue))
            //{
            //    return actualValue;
            //}

            if(IPEndPoint.TryParse(sourceText, out IPEndPoint? endpoint) && endpoint!=null)
            {
                return endpoint;
            }
        }
        // converter used for the wrong type
        //return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        return BindingOperations.DoNothing;
    }
}
