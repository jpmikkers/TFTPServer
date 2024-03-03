using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public class TFTPAppDialogsImpl : ITFTPAppDialogs
{
    private static Window LocateView(INotifyPropertyChanged vm)
    {
        if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            return lifetime.Windows.Where(x => x.DataContext == vm).First();
        }
        throw new InvalidOperationException("Only support desktop apps");
    }

    public Task<UIConfigDialog.ChangeConfigResult> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath)
    {
        return UIConfigDialog.ShowDialog(LocateView(vm), settings, configPath);
    }

    public Task<ConfigDialog.ChangeConfigResult> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings)
    {
        return ConfigDialog.ShowDialog(LocateView(vm), settings);
    }

    public Task ShowErrorDialog(INotifyPropertyChanged vm, string title, string header, string details)
    {
        return ErrorDialog.ShowErrorDialog(LocateView(vm),title, header, details);
    }
}
