using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Platform.Storage;
using Baksteen.Avalonia.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvaTFTPServer.ViewModels;

// use NotifyDataErrorInfo for mvvmct validation, see https://github.com/AvaloniaUI/Avalonia/issues/8397
// then on Apply/OK buttons you can bind IsEnabled to !HasErrors
public partial class ConfigDialogViewModel : ObservableValidator
{
    private readonly ITFTPAppDialogs _tftpAppDialogs;
    private readonly IViewModelCloser _viewModelCloser;

    public ConfigDialogViewModel(ITFTPAppDialogs tftpAppDialogs, IViewModelCloser viewModelCloser) : base()
    {
        _tftpAppDialogs = tftpAppDialogs;
        _viewModelCloser = viewModelCloser;
    }

    [ObservableProperty]
    [Required]
    [NotifyDataErrorInfo]
    private string _endPoint = "0.0.0.0:69";

    [ObservableProperty]
    [Required]
    [NotifyDataErrorInfo]
    [PathMustBeValid]
    private string _rootPath = ".";

    [ObservableProperty]
    private bool _allowDownloads = true;

    [ObservableProperty]
    private bool _allowUploads = true;

    [ObservableProperty]
    private bool _autoCreateDirectories = true;

    [ObservableProperty]
    private bool _convertPathSeparator = true;

    [ObservableProperty]
    private bool _singlePort = false;

    [ObservableProperty]
    [Required]
    [NotifyDataErrorInfo]  
    private short? _timeToLive = -1;

    [ObservableProperty]
    private bool _dontFragment = true;

    [ObservableProperty]
    [Required]
    [NotifyDataErrorInfo]
    private int? _responseTimeout = 2;

    [ObservableProperty]
    [Required]
    [NotifyDataErrorInfo]
    private int? _retries = 5;

    [ObservableProperty]
    [Required]
    [NotifyDataErrorInfo]
    private int? _windowSize = 1;

    public enum DialogResult
    {
        Canceled,
        Ok
    }

    [RelayCommand]
    private void Apply()
    {
        _viewModelCloser.Close(this, DialogResult.Ok);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _viewModelCloser.Close(this, DialogResult.Canceled);
    }

    [RelayCommand]
    private async Task SelectFolder()
    {
        RootPath = (await _tftpAppDialogs.ShowFolderPicker(this, "Select root path")) ?? RootPath;
    }

    [RelayCommand]
    private async Task SelectIPEndPoint()
    {
        var result = await _tftpAppDialogs.ShowIPEndPointPicker(this);

        if(result is not null)
        {
            EndPoint = result.ToString();
        }
    }

    public void SettingsToViewModel(ServerSettings settings)
    {
        EndPoint = settings.EndPoint.ToString();
        AllowDownloads = settings.AllowDownloads;
        AllowUploads = settings.AllowUploads;
        AutoCreateDirectories = settings.AutoCreateDirectories;
        ConvertPathSeparator = settings.ConvertPathSeparator;
        DontFragment = settings.DontFragment;
        ResponseTimeout = settings.ResponseTimeout;
        Retries = settings.Retries;
        RootPath = settings.RootPath;
        SinglePort = settings.SinglePort;
        TimeToLive = settings.TimeToLive;
        WindowSize = settings.WindowSize;
    }

    public ServerSettings ViewModelToSettings()
    {
        return new()
        {
            AllowDownloads = AllowDownloads,
            AllowUploads = AllowUploads,
            AutoCreateDirectories = AutoCreateDirectories,
            ConvertPathSeparator = ConvertPathSeparator,
            DontFragment = DontFragment,
            ResponseTimeout = ResponseTimeout ?? 2,
            Retries = Retries ?? 5,
            RootPath = RootPath,
            SinglePort = SinglePort,
            TimeToLive = TimeToLive ?? -1,
            WindowSize = WindowSize ?? 1,
            EndPoint = IPEndPoint.TryParse(EndPoint, out var parsedEndPoint) ? parsedEndPoint : new IPEndPoint(IPAddress.Loopback, 69)
        };
    }
}
