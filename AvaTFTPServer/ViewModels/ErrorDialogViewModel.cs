﻿using Avalonia.Threading;
using Baksteen.Net.TFTP.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AvaTFTPServer.ViewModels
{
    public partial class ErrorDialogViewModel : ViewModelBase
    {
        public interface IViewMethods
        {
            void Close();
            //Task<ChangeConfigResult> ShowConfigDialog(ServerSettings serverSettings);
        };

        public IViewMethods? ViewMethods { get; set; }

        [ObservableProperty]
        string _title = "Error";

        [ObservableProperty]
        string _header = string.Empty;

        [ObservableProperty]
        string _details = string.Empty;

        public bool HasHeader
        {
            get => !string.IsNullOrEmpty(Header);
        }

        [RelayCommand]
        private void Okay()
        {
            ViewMethods?.Close();
        }


    }
}