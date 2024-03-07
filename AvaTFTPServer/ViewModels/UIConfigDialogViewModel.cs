using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvaTFTPServer.ViewModels.ConfigDialogViewModel;

namespace AvaTFTPServer.ViewModels;

public partial class UIConfigDialogViewModel(IViewModelCloser viewModelCloser) : ObservableValidator
{
    public enum DialogResult
    {
        Ok,
        Cancel
    }

    [ObservableProperty]
    private bool _autoScrollLog = true;

    [ObservableProperty]
    private string _configPath = string.Empty;

    [RelayCommand]
    private void Apply()
    {
        viewModelCloser.Close(this, DialogResult.Ok);
    }

    [RelayCommand]
    private void Cancel()
    {
        viewModelCloser.Close(this, DialogResult.Cancel);
    }
}
