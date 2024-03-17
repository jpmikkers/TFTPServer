using Avalonia.Threading;
using Baksteen.Net.TFTP.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AvaTFTPServer.ViewModels
{
    public partial class AboutDialogViewModel : ViewModelBase
    {
        private readonly IViewModelCloser _viewModelCloser;

        [ObservableProperty]
        Version _version;

        [ObservableProperty]
        string _operatingSystemDescription;

        [ObservableProperty]
        string _architecture;

        public AboutDialogViewModel(IViewModelCloser viewModelCloser)
        {
            _viewModelCloser = viewModelCloser;
            Version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0,0);
            _operatingSystemDescription = RuntimeInformation.OSDescription;
            Architecture = RuntimeInformation.ProcessArchitecture.ToString();
        }

        [RelayCommand]
        private void Okay()
        {
            _viewModelCloser.Close(this);
        }
    }
}
