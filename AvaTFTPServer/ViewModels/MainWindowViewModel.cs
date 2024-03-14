using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Baksteen.Net.TFTP.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AvaTFTPServer.Services.Logging;
using AvaTFTPServer.AvaloniaTools;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace AvaTFTPServer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const string s_linuxHelp =
        """
        ========================================================================
        PLEASE NOTE: on Linux, user mode applications cannot open IP ports below
        port number 1024 by default (default TFTP port is 69). To solve this you
        have the following choices:

            - run this program with elevated rights using the sudo command

        or

            - give the executable the capability to open low ports in user mode 
              via this command:

              sudo setcap CAP_NET_BIND_SERVICE=eip ./AvaTFTPServer

        ========================================================================
        """;

    public enum TFTPServerState
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error
    }

    private readonly static TimeSpan s_progressInterval = TimeSpan.FromSeconds(1.0);
    private readonly static TimeSpan s_averagingInterval = TimeSpan.FromSeconds(3.0);

    private TFTPServer? _server;
    private readonly SessionInfoFactory _sessionInfoFactory;
    private ChunkedDispatcher<LogItem> _chunkedDispatcher;
    private readonly DispatcherTimer _cleanupTimer;
    private readonly DispatcherTimer _progressTimer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITFTPAppDialogs _appDialogs;
    private readonly GuiLogger _guiLogger;
    private readonly IViewModelCloser _viewModelCloser;
    private TFTPAppSettings _appSettings;

    public ObservableCollection<LogItem> Log { get; } = [];

    public ObservableCollection<TransferSession> TransferSessions { get; } = [];

    [ObservableProperty]
    TFTPServerState _serverState;

    [ObservableProperty]
    private bool _autoScrollLog;


    partial void OnAutoScrollLogChanged(bool oldValue, bool newValue)
    {
        _appSettings.UISettings.AutoScrollLog = newValue;
        _appSettings.Save();
    }

    private class TFTPServerInfoImpl(MainWindowViewModel parent) : ITFTPServerInfo
    {
        public void Started() => parent.ServerState = TFTPServerState.Running;

        public void Stopped(Exception? ex) => parent.ServerState = (ex is not null) ? TFTPServerState.Error : TFTPServerState.Stopped;
    }

    private class SessionInfoFactory(MainWindowViewModel parent) : ITFTPSessionInfoFactory
    {
        public ITFTPSessionInfo Create()
        {
            var ts = new TransferSession(parent._progressTimer.Interval, TimeSpan.FromSeconds(3.0));
            Dispatcher.UIThread.Post(() =>
            {
                parent.TransferSessions.Add(ts);
            });
            return ts.SessionInfo;
        }
    }

    public MainWindowViewModel()
    {
        _guiLogger = new GuiLogger();
        _appDialogs = new TFTPAppDialogsImpl(new ViewResolver(),null!);
        _sessionInfoFactory = new SessionInfoFactory(this);
        _appSettings = new TFTPAppSettings();
        _chunkedDispatcher = new ChunkedDispatcher<LogItem>(Dispatcher.UIThread, x => { });
        _progressTimer = new(s_progressInterval, DispatcherPriority.Normal, _progressTimer_Tick);
        _cleanupTimer = new(TimeSpan.FromSeconds(10.0), DispatcherPriority.Normal, _cleanupTimer_Tick);
        _loggerFactory = new LoggerFactory();
        _viewModelCloser = new ViewModelCloser(new ViewResolver());
    }

    public MainWindowViewModel(
        ILoggerFactory loggerFactory, 
        ITFTPAppDialogs appDialogs, 
        GuiLogger guiLogger,
        IViewModelCloser viewModelCloser,
        TFTPAppSettings appSettings) : base() 
    {
        //App.Current.
        _guiLogger = guiLogger;
        _viewModelCloser = viewModelCloser;
        _appSettings = appSettings;

        AutoScrollLog = _appSettings.UISettings.AutoScrollLog;

        _chunkedDispatcher = new ChunkedDispatcher<LogItem>(Dispatcher.UIThread, items =>
        {
            //System.Diagnostics.Debug.WriteLine($"got {items.Count()} items");
            foreach(var item in items) Log.Add(item);
        });

        _guiLogger.RegisterCallback(_chunkedDispatcher.Post);

        _sessionInfoFactory = new SessionInfoFactory(this);

        _progressTimer = new(s_progressInterval, DispatcherPriority.Normal, _progressTimer_Tick);
        _progressTimer.Start();

        _cleanupTimer = new(GetCleanupTimerInterval(), DispatcherPriority.Normal, _cleanupTimer_Tick);
        _cleanupTimer.Start();
        _loggerFactory = loggerFactory;
        _appDialogs = appDialogs;

        //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, s_progressInterval, s_averagingInterval));
        //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, s_progressInterval, s_averagingInterval) { SessionState = TFTPLiveSessionState.Stopped, FileLength=0 });
        //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, _timer.Interval, TimeSpan.FromSeconds(3.0)));
    }

    private TimeSpan GetCleanupTimerInterval()
    {
        var tmp = _appSettings.UISettings.CleanupTransfersAfter / 4;
        if(tmp < TimeSpan.FromSeconds(1)) tmp = TimeSpan.FromSeconds(1);
        if(tmp > TimeSpan.FromDays(1)) tmp = TimeSpan.FromDays(1);
        return tmp;
    }

    private void _cleanupTimer_Tick(object? sender, EventArgs e)
    {
        for(var t = TransferSessions.Count-1; t>=0; t--)
        {
            var item = TransferSessions[t];
            if( item.IsFinalState && (DateTime.UtcNow - item.CompletionTimeUtc) > _appSettings.UISettings.CleanupTransfersAfter)
            {
                TransferSessions.RemoveAt(t);
            }
        }
    }

    private void _progressTimer_Tick(object? sender, EventArgs e)
    {
        foreach(var session in TransferSessions) session.Update();
    }

    [RelayCommand]
    private void DoExit()
    {
        _guiLogger.UnregisterCallback();
        _viewModelCloser.Close(this);
    }

    [RelayCommand]
    private async Task DoConfigure()
    {
        var originalSettings = _appSettings.ServerSettings;
        var response = await _appDialogs.ShowConfigDialog(this, originalSettings);

        if(response.DialogResult == ConfigDialogViewModel.DialogResult.Ok)
        {
            if(!originalSettings.Equals(response.ServerSettings))
            {
                _appSettings.ServerSettings = response.ServerSettings;
                _appSettings.Save();

                if(_server is not null)
                {
                    // TODO show messagebox about restarting server?
                    await StopServer();
                    await StartServer();
                }
            }
        }
    }

    [RelayCommand]
    private async Task DoConfigureUI()
    {
        var originalSettings = _appSettings.UISettings;
        var response = await _appDialogs.ShowUIConfigDialog(this, originalSettings, _appSettings.ConfigPath);

        if(response.DialogResult == UIConfigDialogViewModel.DialogResult.Ok)
        {
            if(!originalSettings.Equals(response.Settings))
            {
                _appSettings.UISettings = response.Settings;
                _appSettings.Save();

                AutoScrollLog = _appSettings.UISettings.AutoScrollLog;

                _cleanupTimer.Stop();
                _cleanupTimer.Interval = GetCleanupTimerInterval();
                _cleanupTimer.Start();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartServer))]
    private async Task StartServer()
    {
        if(CanStartServer())
        {
            try
            {
                ServerState = TFTPServerState.Starting;

                _server = await TFTPServer.CreateAndStart(
                    _loggerFactory.CreateLogger<TFTPServer>(),
                    streamFactory: null,
                    udpSocketFactory: null,
                    liveSessionInfoFactory: _sessionInfoFactory,
                    serverCallback: new TFTPServerInfoImpl(this),
                    _appSettings.ServerSettings.ToServerConfig());
            }
            catch(SocketException ex) when (
                ex.SocketErrorCode == SocketError.AccessDenied && 
                _appSettings.ServerSettings.EndPoint.Port < 1024 &&
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))              
            {
                ServerState = TFTPServerState.Error;
                await _appDialogs.ShowErrorDialog(
                    this,
                    "Error",
                    "Failed to start server",
                    $"{s_linuxHelp}\r\n{ex}");
            }
            catch (Exception ex)
            {
                ServerState = TFTPServerState.Error;
                await _appDialogs.ShowErrorDialog(this, "Error", "Failed to start server", ex.ToString());
            }
        }
    }

    [MemberNotNullWhen(returnValue: false, nameof(_server))]
    private bool CanStartServer()
    {
        return _server is null;
    }

    [RelayCommand(CanExecute = nameof(CanStopServer))]
    private async Task StopServer()
    {
        if(CanStopServer())
        {
            ServerState = TFTPServerState.Stopping;
            await _server.Stop();
            _server = null;
        }
    }

    [MemberNotNullWhen(returnValue: true, nameof(_server))]
    private bool CanStopServer()
    {
        return _server is not null;
    }
}
