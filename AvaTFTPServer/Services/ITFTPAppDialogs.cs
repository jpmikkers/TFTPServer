using Avalonia.Controls;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public interface ITFTPAppDialogs
{
    public Task<UISettings?> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath);
    public Task<ServerSettings?> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings);
    public Task ShowErrorDialog(INotifyPropertyChanged vm, string title, string header, string details);
    public Task<string?> ShowFolderPicker(INotifyPropertyChanged vm, string title);
    public Task<IPEndPoint?> ShowIPEndPointPicker(INotifyPropertyChanged vm);
}
