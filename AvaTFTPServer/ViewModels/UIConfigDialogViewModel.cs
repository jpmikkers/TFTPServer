using Baksteen.Avalonia.Tools.CloseableViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvaTFTPServer.ViewModels.ConfigDialogViewModel;

namespace AvaTFTPServer.ViewModels;

public partial class UIConfigDialogViewModel : ObservableValidator, ICloseableViewModel<UIConfigDialogViewModel.DialogResult>
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
        this.Close(DialogResult.Ok);
    }

    [RelayCommand]
    private void Cancel()
    {
        this.Close(DialogResult.Cancel);
    }
}
