using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaTFTPServer;


public class TFTPAppDialogsImpl(IViewResolver viewResolver) : ITFTPAppDialogs
{
    public Task<UIConfigDialog.ChangeConfigResult> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(
            () => UIConfigDialog.ShowDialog(viewResolver.LocateView(vm),settings,configPath),
            DispatcherPriority.Background
        );
    }

    public Task<ConfigDialog.ChangeConfigResult> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(
            () => ConfigDialog.ShowDialog(viewResolver.LocateView(vm), settings),
            DispatcherPriority.Background
        );        
    }

    public Task ShowErrorDialog(INotifyPropertyChanged vm, string title, string header, string details)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(
            () => ErrorDialog.ShowErrorDialog(viewResolver.LocateView(vm), title, header, details),
            DispatcherPriority.Background
        );
    }

    public async Task<string?> ShowFolderPicker(INotifyPropertyChanged vm, string title)
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
