using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using AvaTFTPServer.ViewModels;
using System.Net;
using System.Diagnostics;
using Baksteen.Avalonia.Tools.GridIndexer;
using AvaTFTPServer.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Linq;
using Avalonia.Platform.Storage;
using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AvaTFTPServer;

public partial class ConfigDialog : Window
{
    public record class ChangeConfigResult
    {
        public ConfigDialogViewModel.DialogResult DialogResult { get; set; }
        public ServerSettings ServerSettings { get; set; } = new ServerSettings();
    }

    private class ViewMethodsImpl(ConfigDialog parent) : ConfigDialogViewModel.IViewMethods
    {
        public void Close(ConfigDialogViewModel.DialogResult result) => parent.Close(result);
        public Task<string?> ShowFolderPicker(string title) => parent.ShowFolderPicker(title);
    }

    public ConfigDialog()
    {
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }

    private async Task<string?> ShowFolderPicker(string title)
    {
        var results = await StorageProvider.OpenFolderPickerAsync(
        new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = title
        });
        return results.Any() ? results.Select(x => x.TryGetLocalPath()).FirstOrDefault() : null;
    }

    public static async Task<ChangeConfigResult> ShowDialog(Window owner, ServerSettings settings)
    {
        var dialog = new ConfigDialog();

        var vm = new ConfigDialogViewModel(new ViewMethodsImpl(dialog))
        {
            EndPoint = settings.EndPoint.ToString(),
            AllowDownloads = settings.AllowDownloads,
            AllowUploads = settings.AllowUploads,
            AutoCreateDirectories = settings.AutoCreateDirectories,
            ConvertPathSeparator = settings.ConvertPathSeparator,
            DontFragment = settings.DontFragment,
            ResponseTimeout = settings.ResponseTimeout,
            Retries = settings.Retries,
            RootPath = settings.RootPath,
            SinglePort = settings.SinglePort,
            TimeToLive = settings.TimeToLive,
            WindowSize = settings.WindowSize,
        };

        dialog.DataContext = vm;

        if(await dialog.ShowDialog<ConfigDialogViewModel.DialogResult?>(owner) == ConfigDialogViewModel.DialogResult.Ok)
        {
            return new ChangeConfigResult {
                DialogResult = ConfigDialogViewModel.DialogResult.Ok,
                ServerSettings = new ServerSettings
                {
                    AllowDownloads = vm.AllowDownloads,
                    AllowUploads = vm.AllowUploads,
                    AutoCreateDirectories = vm.AutoCreateDirectories,
                    ConvertPathSeparator = vm.ConvertPathSeparator,
                    DontFragment = vm.DontFragment,
                    ResponseTimeout = vm.ResponseTimeout ?? 2,
                    Retries = vm.Retries ?? 5,
                    RootPath = vm.RootPath,
                    SinglePort = vm.SinglePort,
                    TimeToLive = vm.TimeToLive ?? -1,
                    WindowSize = vm.WindowSize ?? 1,
                    EndPoint = IPEndPoint.TryParse(vm.EndPoint, out var parsedEndPoint) ? parsedEndPoint : settings.EndPoint,
                }
            };
        }
        else
        {
            return new ChangeConfigResult
            {
                DialogResult = ConfigDialogViewModel.DialogResult.Canceled,
                ServerSettings = settings
            };
        }
    }
}
