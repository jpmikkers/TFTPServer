using Avalonia.Controls;
using System.ComponentModel;

namespace AvaTFTPServer;

public interface IViewResolver
{
    Window LocateView(INotifyPropertyChanged vm);
}