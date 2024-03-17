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
    private static Task<TResult> ShowDialog<TResult>(Window owner, Window dialog)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(() => dialog.ShowDialog<TResult>(owner), DispatcherPriority.Background);
    }

    private static Task ShowDialog(Window owner, Window dialog)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(() => dialog.ShowDialog(owner), DispatcherPriority.Background);
    }

    public async Task<UISettings?> ShowUIConfigDialog(INotifyPropertyChanged ownerVm, UISettings settings, string configPath)
    {
        var dialog = serviceProvider.GetRequiredService<UIConfigDialog>();
        var vm = (UIConfigDialogViewModel)dialog.DataContext!;

        vm.CleanupTransfersAfter = settings.CleanupTransfersAfter;
        vm.AutoScrollLog = settings.AutoScrollLog;
        vm.ConfigPath = configPath;

        return (await ShowDialog<DialogResult?>(viewResolver.LocateView(ownerVm),dialog) == DialogResult.Ok) ?
            new UISettings
            {
                AutoScrollLog = vm.AutoScrollLog,
                CleanupTransfersAfter = vm.CleanupTransfersAfter,
            }
            : null;
    }

    public async Task<ServerSettings?> ShowConfigDialog(INotifyPropertyChanged ownerVm, ServerSettings settings)
    {
        var dialog = serviceProvider.GetRequiredService<ConfigDialog>();
        var vm = (ConfigDialogViewModel)dialog.DataContext!;
        vm.SettingsToViewModel(settings);
        return (await ShowDialog<DialogResult?>(viewResolver.LocateView(ownerVm), dialog) == DialogResult.Ok) ? vm.ViewModelToSettings() : null;
    }

    public async Task ShowErrorDialog(INotifyPropertyChanged ownerVm, string title, string header, string details)
    {
        var dialog = serviceProvider.GetRequiredService<ErrorDialog>();
        var vm = (ErrorDialogViewModel)dialog.DataContext!;

        vm.Title = title;
        vm.Header = header;
        vm.Details = details;

        await ShowDialog(viewResolver.LocateView(ownerVm), dialog);
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

    public async Task ShowAboutDialog(INotifyPropertyChanged ownerVm)
    {
        await ShowDialog(viewResolver.LocateView(ownerVm), serviceProvider.GetRequiredService<AboutDialog>());
    }
}
