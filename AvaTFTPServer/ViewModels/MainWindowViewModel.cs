using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Baksteen.Net.TFTP.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace AvaTFTPServer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public interface IViewMethods
        {
            void Close();
            Task<ChangeConfigResult> ShowConfigDialog(ServerSettings serverSettings);
            Task ShowErrorDialog(string title, string header, string details);
            void AutoScrollLog();
            void AutoScrollTransfers();
        };

        public IViewMethods? ViewMethods { get; set; }

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
        private TFTPAppSettings _appSettings;

        public ObservableCollection<LogItem> Log { get; } = [];

        public ObservableCollection<TransferSession> TransferSessions { get; } = [];

        [ObservableProperty]
        TFTPServerState _serverState;

        public bool AutoScrollLog
        {
            get => _appSettings.AutoScrollLog;
            set
            {
                var tmp = AutoScrollLog;
                if(SetProperty(ref tmp, value))
                {
                    _appSettings.AutoScrollLog = tmp;
                    _appSettings.Save();
                    ScrollLogWhenEnabled();
                }
            }
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
                var ts = new TransferSession(Dispatcher.UIThread, parent._progressTimer.Interval, TimeSpan.FromSeconds(3.0));
                Dispatcher.UIThread.Post(() =>
                {
                    parent.TransferSessions.Add(ts);
                    parent.ViewMethods?.AutoScrollTransfers();
                });
                return ts.SessionInfo;
            }
        }

        public MainWindowViewModel() : base() 
        {
            //App.Current.

            _appSettings = TFTPAppSettings.Load();

            _chunkedDispatcher = new ChunkedDispatcher<LogItem>(Dispatcher.UIThread, items =>
            {
                foreach(var item in items) Log.Add(item);
                ScrollLogWhenEnabled();
            });

            _sessionInfoFactory = new SessionInfoFactory(this);

            _progressTimer = new(s_progressInterval, DispatcherPriority.Normal, _progressTimer_Tick);
            _progressTimer.Start();

            _cleanupTimer = new(TimeSpan.FromSeconds(10.0), DispatcherPriority.Normal, _cleanupTimer_Tick);
            _cleanupTimer.Start();

            //Log.Add(new LogItem { Color = Colors.Red, Text = "een" });
            //Log.Add(new LogItem { Color = Colors.Green, Text = "twee" });
            //Log.Add(new LogItem { Color = Colors.AliceBlue, Text = "drie" });
            //foreach(var x in Enumerable.Range(1,1000000)){ LogList.Add(new LogItem { Text = x.ToString() }); };

            //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, s_progressInterval, s_averagingInterval));
            //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, s_progressInterval, s_averagingInterval) { SessionState = TFTPLiveSessionState.Stopped, FileLength=0 });
            //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, _timer.Interval, TimeSpan.FromSeconds(3.0)));

        }

        public void ScrollLogWhenEnabled()
        {
            if(AutoScrollLog) ViewMethods?.AutoScrollLog();
        }

        private void _cleanupTimer_Tick(object? sender, EventArgs e)
        {
            for(var t = TransferSessions.Count-1; t>=0; t--)
            {
                var item = TransferSessions[t];
                if( item.IsFinalState && DateTime.Now - item.StartTime > TimeSpan.FromSeconds(30))
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
            ViewMethods?.Close();
        }

        [RelayCommand]
        private async Task DoConfigure()
        {
            if(ViewMethods == null) return;

            var originalSettings = _appSettings.ServerSettings;
            var response = await ViewMethods.ShowConfigDialog(originalSettings);

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

        [RelayCommand(CanExecute = nameof(CanStartServer))]
        private async Task StartServer()
        {
            if(CanStartServer())
            {
                try
                {
                    ServerState = TFTPServerState.Starting;

                    _server = await TFTPServer.CreateAndStart(
                        new CustomLogger("TFTPServer", _chunkedDispatcher.Post),
                        streamFactory: null,
                        udpSocketFactory: null,
                        liveSessionInfoFactory: _sessionInfoFactory,
                        serverCallback: new TFTPServerInfoImpl(this),
                        _appSettings.ServerSettings.ToServerConfig());
                }
                catch (Exception ex)
                {
                    ServerState = TFTPServerState.Error;
                    await (ViewMethods?.ShowErrorDialog("Error", "Failed to start server", ex.ToString()) ?? Task.CompletedTask);
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
}
