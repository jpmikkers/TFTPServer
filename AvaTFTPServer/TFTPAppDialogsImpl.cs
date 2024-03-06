using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaTFTPServer;


public class TFTPAppDialogsImpl(IViewResolver viewResolver) : ITFTPAppDialogs
{
    public Task<UIConfigDialog.ChangeConfigResult> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath)
    {
        return UIConfigDialog.ShowDialog(viewResolver.LocateView(vm), settings, configPath);
    }

    public Task<ConfigDialog.ChangeConfigResult> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings)
    {
        return ConfigDialog.ShowDialog(viewResolver.LocateView(vm), settings, this);
    }

    public Task ShowErrorDialog(INotifyPropertyChanged vm, string title, string header, string details)
    {
        return ErrorDialog.ShowErrorDialog(viewResolver.LocateView(vm),title, header, details);
    }

    public async Task<string?> PickFolder(INotifyPropertyChanged vm, string title)
    {
        try
        {
            var results = await viewResolver.LocateView(vm).StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = title
            });
            return results.Select(x => x.TryGetLocalPath()).FirstOrDefault();
        }
        catch 
        { 
        }
        return null;
    }
}
