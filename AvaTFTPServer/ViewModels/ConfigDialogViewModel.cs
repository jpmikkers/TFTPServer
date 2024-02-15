using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;

namespace AvaTFTPServer.ViewModels;

public partial class ConfigDialogViewModel : ViewModelBase
{
    public interface IViewMethods
    {
        void Close(DialogResult result);
        Task<string?> ShowFolderPicker(string title);
    };

    private IViewMethods _viewMethods;

    public ConfigDialogViewModel(IViewMethods viewMethods)
    {
        _viewMethods = viewMethods;
    }

    [ObservableProperty]
    private string _endPoint = "0.0.0.0:69";

    [ObservableProperty]
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
    private short _timeToLive = -1;

    [ObservableProperty]
    private bool _dontFragment = true;

    [ObservableProperty]
    private int _responseTimeout = 2;

    [ObservableProperty]
    private int _retries = 5;

    [ObservableProperty]
    private int _windowSize = 1;

    public enum DialogResult
    {
        Canceled,
        Ok
    }

    [RelayCommand]
    private void Apply()
    {
        _viewMethods.Close(DialogResult.Ok);
    }

    [RelayCommand]
    private void Cancel()
    {
        _viewMethods.Close(DialogResult.Canceled);
    }

    [RelayCommand]
    private async Task SelectFolder()
    {
        RootPath = await _viewMethods.ShowFolderPicker("Select root folder") ?? RootPath;
    }
}
