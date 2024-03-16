using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaTFTPServer.AvaloniaTools;
using AvaTFTPServer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvaTFTPServer;


public class TFTPAppDialogsImpl(IViewResolver viewResolver, IServiceProvider serviceProvider) : ITFTPAppDialogs
{
    private async Task<UISettings?> DoShowUIConfigDialog(Window owner, UISettings settings, string configPath)
    {
        var dialog = serviceProvider.GetRequiredService<UIConfigDialog>();
        var vm = (UIConfigDialogViewModel)dialog.DataContext!;

        vm.CleanupTransfersAfter = settings.CleanupTransfersAfter;
        vm.AutoScrollLog = settings.AutoScrollLog;
        vm.ConfigPath = configPath;

        return (await dialog.ShowDialog<DialogResult?>(owner) == DialogResult.Ok) ? 
            new UISettings {
                AutoScrollLog = vm.AutoScrollLog,
                CleanupTransfersAfter = vm.CleanupTransfersAfter,
            }
            : null;
    }

    private async Task<ServerSettings?> DoShowConfigDialog(Window owner, ServerSettings settings)
    {
        var dialog = serviceProvider.GetRequiredService<ConfigDialog>();
        var vm = (ConfigDialogViewModel)dialog.DataContext!;
        vm.SettingsToViewModel(settings);
        return (await dialog.ShowDialog<DialogResult?>(owner) == DialogResult.Ok) ? vm.ViewModelToSettings() : null;
    }

    private async Task DoShowErrorDialog(Window owner, string title, string header, string details)
    {
        var dialog = serviceProvider.GetRequiredService<ErrorDialog>();
        var vm = (ErrorDialogViewModel)dialog.DataContext!;

        vm.Title = title;
        vm.Header = header;
        vm.Details = details;

        await dialog.ShowDialog(owner);
    }

    public Task<UISettings?> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(
            () => DoShowUIConfigDialog(viewResolver.LocateView(vm),settings,configPath),
            DispatcherPriority.Background
        );
    }

    public Task<ServerSettings?> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(
            () => DoShowConfigDialog(viewResolver.LocateView(vm), settings),
            DispatcherPriority.Background
        );        
    }

    public Task ShowErrorDialog(INotifyPropertyChanged vm, string title, string header, string details)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(
            () => DoShowErrorDialog(viewResolver.LocateView(vm), title, header, details),
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

    public async Task<IPEndPoint?> ShowIPEndPointPicker(INotifyPropertyChanged ownerVm)
    {
        var dialog = serviceProvider.GetRequiredService<EndPointSelectionDialog>();
        var vm = (EndPointSelectionDialogViewModel)dialog.DataContext!;

        var result = await dialog.ShowDialog<DialogResult>(viewResolver.LocateView(ownerVm));

        if(result == DialogResult.Ok)
        {
            return vm.EndPoints[vm.SelectedIPEndPointIndex].IPEndPoint;
        }
        else
        {
            return null;
        }
    }
}
