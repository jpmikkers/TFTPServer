using Avalonia.Controls;
using Avalonia.Threading;
using Baksteen.Net.TFTP.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
        private ChunkedDispatcher<string> _chunkedDispatcher;
        private DispatcherTimer _timer;
        private TFTPAppSettings _appSettings;

        public ObservableCollection<string> Log { get; } = [];

        public ObservableCollection<TransferSession> TransferSessions { get; } = [];

        [ObservableProperty]
        TFTPServerState _serverState;

        private class TFTPServerInfoImpl(MainWindowViewModel parent) : ITFTPServerInfo
        {
            public void Started() => parent.ServerState = TFTPServerState.Running;

            public void Stopped(Exception? ex) => parent.ServerState = (ex is null) ? TFTPServerState.Error : TFTPServerState.Stopped;
        }

        private class SessionInfoFactory(MainWindowViewModel parent) : ITFTPSessionInfoFactory
        {
            public ITFTPSessionInfo Create()
            {
                var ts = new TransferSession(Dispatcher.UIThread, parent._timer.Interval, TimeSpan.FromSeconds(3.0));
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
            _appSettings = TFTPAppSettings.Load();

            _chunkedDispatcher = new ChunkedDispatcher<string>(Dispatcher.UIThread, lines => { 
                foreach(var line in lines) Log.Add(line);
                ViewMethods?.AutoScrollLog();
            });

            _sessionInfoFactory = new SessionInfoFactory(this);

            _timer = new(s_progressInterval, DispatcherPriority.Normal, _timer_Tick);
            _timer.Start();

            //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, s_progressInterval, s_averagingInterval));
            //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, s_progressInterval, s_averagingInterval) { SessionState = TFTPLiveSessionState.Stopped, FileLength=0 });
            //TransferSessions.Add(new TransferSession(Dispatcher.UIThread, _timer.Interval, TimeSpan.FromSeconds(3.0)));

        }

        private void _timer_Tick(object? sender, EventArgs e)
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
