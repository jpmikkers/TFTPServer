using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvaTFTPServer.ViewModels.ConfigDialogViewModel;

namespace AvaTFTPServer.ViewModels;

public partial class UIConfigDialogViewModel(IViewModelCloser viewModelCloser) : ObservableValidator
{
    public record class TimeSpanItem
    {
        public string UserText { get; init; } = string.Empty;
        public TimeSpan TimeSpan { get; init; }
    }

    public enum DialogResult
    {
        Ok,
        Cancel
    }

    public List<TimeSpanItem> TimeSpans { get; private set; } = [
        new(){ UserText = "5 seconds", TimeSpan = TimeSpan.FromSeconds(5) },
        new(){ UserText = "10 seconds", TimeSpan = TimeSpan.FromSeconds(10)},
        new(){ UserText = "30 seconds", TimeSpan = TimeSpan.FromSeconds(30)},
        new(){ UserText = "1 minute", TimeSpan = TimeSpan.FromMinutes(1)},
        new(){ UserText = "5 minutes", TimeSpan = TimeSpan.FromMinutes(5)},
        new(){ UserText = "10 minutes", TimeSpan = TimeSpan.FromMinutes(10)},
        new(){ UserText = "30 minutes", TimeSpan = TimeSpan.FromMinutes(30)},
        new(){ UserText = "1 hour", TimeSpan = TimeSpan.FromHours(1)},
        new(){ UserText = "6 hour", TimeSpan = TimeSpan.FromHours(6)},
        new(){ UserText = "12 hours", TimeSpan = TimeSpan.FromHours(12)},
        new(){ UserText = "1 day", TimeSpan = TimeSpan.FromDays(1)},
        new(){ UserText = "never", TimeSpan = TimeSpan.FromDays(500000) },
    ];

    [ObservableProperty]
    private bool _autoScrollLog = true;

    [ObservableProperty]
    private string _configPath = string.Empty;

    [ObservableProperty]
    private int _selectedTimeSpanIndex = 2;

    partial void OnSelectedTimeSpanIndexChanged(int value)
    {
        CleanupTransfersAfter = TimeSpans[value].TimeSpan;
    }


    private TimeSpan _removeTransferAfter = TimeSpan.Zero;

    public TimeSpan CleanupTransfersAfter
    {
        get => _removeTransferAfter; 
        set 
        {
            int idx = TimeSpans
                .Select((item, index) => (item, index))
                .Reverse()
                .Where(x => x.item.TimeSpan <= value)
                .Select(x => x.index)
                .FirstOrDefault();

            if(SelectedTimeSpanIndex != idx)
            {
                SelectedTimeSpanIndex = idx;
            }

            //Debug.WriteLine($"selected index {idx}");

            _removeTransferAfter = value; 
        }
    }

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
