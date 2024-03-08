using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Platform.Storage;
using Baksteen.Avalonia.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
}
