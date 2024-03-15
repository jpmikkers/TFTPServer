﻿using Avalonia.Controls;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public interface ITFTPAppDialogs
{
    public Task<UIConfigDialog.ChangeConfigResult> ShowUIConfigDialog(INotifyPropertyChanged vm, UISettings settings, string configPath);
    public Task<ConfigDialog.ChangeConfigResult> ShowConfigDialog(INotifyPropertyChanged vm, ServerSettings settings);
    public Task ShowErrorDialog(INotifyPropertyChanged vm, string title, string header, string details);
    public Task<string?> ShowFolderPicker(INotifyPropertyChanged vm, string title);
    public Task<IPEndPoint?> ShowIPEndPointPicker(INotifyPropertyChanged vm);
}
