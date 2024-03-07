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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AvaTFTPServer;

public partial class ConfigDialog : Window
{
    public record class ChangeConfigResult
    {
        public ConfigDialogViewModel.DialogResult DialogResult { get; set; }
        public ServerSettings ServerSettings { get; set; } = new ServerSettings();
    }

    public ConfigDialog()
    {
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }

    public static async Task<ChangeConfigResult> ShowDialog(Window owner, ServerSettings settings)
    {
        var dialog = new ConfigDialog();

        var vm = App.AppHost!.Services.GetRequiredService<ConfigDialogViewModel>();

        vm.EndPoint = settings.EndPoint.ToString();
        vm.AllowDownloads = settings.AllowDownloads;
        vm.AllowUploads = settings.AllowUploads;
        vm.AutoCreateDirectories = settings.AutoCreateDirectories;
        vm.ConvertPathSeparator = settings.ConvertPathSeparator;
        vm.DontFragment = settings.DontFragment;
        vm.ResponseTimeout = settings.ResponseTimeout;
        vm.Retries = settings.Retries;
        vm.RootPath = settings.RootPath;
        vm.SinglePort = settings.SinglePort;
        vm.TimeToLive = settings.TimeToLive;
        vm.WindowSize = settings.WindowSize;

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
