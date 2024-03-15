using System.Globalization;
using Microsoft.Extensions.Logging;
using Avalonia.Media;
using Baksteen.Avalonia.Tools;
using System.Net;
using System;

namespace AvaTFTPServer;

public class IPEndPointConverter : StrongTypedValueConverter<IPEndPoint, string>
{
    protected override string ViewModelToView(IPEndPoint input, CultureInfo culture) => input.ToString();

    protected override IPEndPoint ViewToViewModel(string input, CultureInfo culture) => throw new NotImplementedException();
}
