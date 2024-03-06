using Avalonia.Controls;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public interface ITFTPAppDialogs
{
    public Task<UIConfigDialog.ChangeConfigResult> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath);
    public Task<ConfigDialog.ChangeConfigResult> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings);
    public Task ShowErrorDialog(INotifyPropertyChanged vm, string title, string header, string details);
    public Task<string?> PickFolder(INotifyPropertyChanged vm, string title);
}
