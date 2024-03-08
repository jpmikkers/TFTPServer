using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using AvaTFTPServer.ViewModels;
using System.Net;
using Baksteen.Avalonia.Tools.GridIndexer;
using Microsoft.Extensions.DependencyInjection;

namespace AvaTFTPServer;

public partial class ConfigDialog : Window
{
    public record class ChangeConfigResult
    {
        public ConfigDialogViewModel.DialogResult DialogResult { get; set; }
        public ServerSettings ServerSettings { get; set; } = new ServerSettings();
    }

    public ConfigDialog() : this(null!)
    {
    }

    public ConfigDialog(ConfigDialogViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }

    public static async Task<ChangeConfigResult> ShowDialog(Window owner, ServerSettings settings)
    {
        var dialog = App.AppHost!.Services.GetRequiredService<ConfigDialog>();
        var vm = (ConfigDialogViewModel)dialog.DataContext!;

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
