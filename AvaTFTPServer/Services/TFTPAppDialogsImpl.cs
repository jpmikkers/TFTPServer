using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
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
    private async Task<UIConfigDialog.ChangeConfigResult> DoShowUIConfigDialog(Window owner, UISettings settings, string configPath)
    {
        var dialog = serviceProvider.GetRequiredService<UIConfigDialog>();
        var vm = (UIConfigDialogViewModel)dialog.DataContext!;

        vm.CleanupTransfersAfter = settings.CleanupTransfersAfter;
        vm.AutoScrollLog = settings.AutoScrollLog;
        vm.ConfigPath = configPath;

        if(await dialog.ShowDialog<UIConfigDialogViewModel.DialogResult?>(owner) == UIConfigDialogViewModel.DialogResult.Ok)
        {
            return new()
            {
                DialogResult = UIConfigDialogViewModel.DialogResult.Ok,
                Settings = new UISettings
                {
                    AutoScrollLog = vm.AutoScrollLog,
                    CleanupTransfersAfter = vm.CleanupTransfersAfter,
                }
            };
        }
        else
        {
            return new()
            {
                DialogResult = UIConfigDialogViewModel.DialogResult.Cancel,
                Settings = settings
            };
        }
    }

    private async Task<ConfigDialog.ChangeConfigResult> DoShowConfigDialog(Window owner, ServerSettings settings)
    {
        var dialog = serviceProvider.GetRequiredService<ConfigDialog>();
        var vm = (ConfigDialogViewModel)dialog.DataContext!;

        vm.SettingsToViewModel(settings);

        var dialogResult = await dialog.ShowDialog<ConfigDialogViewModel.DialogResult?>(owner) ?? ConfigDialogViewModel.DialogResult.Canceled;

        return new()
        {
            DialogResult = ConfigDialogViewModel.DialogResult.Ok,
            ServerSettings = (dialogResult == ConfigDialogViewModel.DialogResult.Ok) ? vm.ViewModelToSettings() : settings
        };
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

    public Task<UIConfigDialog.ChangeConfigResult> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath)
    {
        // invoke with background priority, otherwise you'll get problems where the main window keeps the focus even though the dialog is visible
        return Dispatcher.UIThread.InvokeAsync(
            () => DoShowUIConfigDialog(viewResolver.LocateView(vm),settings,configPath),
            DispatcherPriority.Background
        );
    }

    public Task<ConfigDialog.ChangeConfigResult> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings)
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
}
