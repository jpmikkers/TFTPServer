using Avalonia.Threading;
using Baksteen.Net.TFTP.Server;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace AvaTFTPServer.ViewModels;

public partial class TransferSession : ObservableObject
{
    public enum State
    {
        Created,
        Busy,
        Stopped,
        Completed,
    }

    private readonly SessionAdapter _link;
    private long _cachedTransferred;
    private long _prevTransferred;
    private readonly SimpleMovingAverage _averager;
    private readonly Stopwatch _stopwatch = new Stopwatch();

    private class SessionAdapter(Dispatcher dispatcher, TransferSession parent) : ITFTPSessionInfo
    {
        public long Id { get; set; }

        public void Start(TFTPSessionStartInfo args) => dispatcher.Post(() => parent.Start(args));

        public void UpdateStart(TFTPSessionUpdateInfo args) => dispatcher.Post(() => parent.UpdateStart(args));

        public void Progress(long transferred) => parent._cachedTransferred = transferred;

        public void Complete() => dispatcher.Post(() => parent.Complete());

        public void Stop(Exception e) => dispatcher.Post(() => parent.Stop(e));
    }

    [ObservableProperty]
    private State _sessionState = State.Created;

    [ObservableProperty]
    private DateTime _startTime = DateTime.Now;

    [ObservableProperty]
    private bool _isUpload;

    [ObservableProperty]
    private string _filename = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage), nameof(FileLengthKnown))]
    private long _fileLength = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
    private long _transferred;

    [ObservableProperty]
    private double _speed;

    [ObservableProperty]
    private string _speedAsString = "0 B/s";

    [ObservableProperty]
    private int _windowSize;

    [ObservableProperty]
    private IPEndPoint _localEndPoint = new IPEndPoint(IPAddress.Loopback, 0);

    [ObservableProperty]
    private IPEndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 0);

    [ObservableProperty]
    private Exception? _exception;

    public bool IsFinalState
    {
        get => SessionState is State.Busy or State.Completed or State.Stopped;
    }

    public double ProgressPercentage
    {
        get => FileLength > 0 ? 100.0 * Transferred / FileLength : 0.0;
    }

    public bool FileLengthKnown
    {
        get => FileLength >= 0;
    }

    public int Id { get; set; }

    public ITFTPSessionInfo SessionInfo { get => _link; }

    public TransferSession(Dispatcher dispatcher, TimeSpan updateInterval, TimeSpan averagingInterval)
    {
         _link = new SessionAdapter(dispatcher, this);
        _averager = new SimpleMovingAverage((int)Math.Max(1.0, Math.Ceiling(averagingInterval / updateInterval)));
    }

    private void Start(TFTPSessionStartInfo args)
    {
        FileLength = args.FileLength;
        Filename = args.Filename;
        IsUpload = args.IsUpload;
        LocalEndPoint = args.LocalEndPoint;
        RemoteEndPoint = args.RemoteEndPoint;
        WindowSize = args.WindowSize;
        SessionState = State.Busy;
        StartTime = args.StartTime;
        Speed = 0;
        Transferred = 0;
        _cachedTransferred = 0;
        _stopwatch.Start();
    }

    private void UpdateStart(TFTPSessionUpdateInfo args)
    {
        FileLength = args.FileLength;
    }

    private void Stop(Exception e)
    {
        SessionState = State.Stopped;
        Transferred = _cachedTransferred;
        FileLength = Transferred;
        Exception = e;
        _stopwatch.Stop();
        Speed = Transferred / _stopwatch.Elapsed.TotalSeconds;
    }

    private void Complete()
    {
        SessionState = State.Completed;
        Transferred = _cachedTransferred;
        FileLength = Transferred;
        _stopwatch.Stop();
        Speed = Transferred / _stopwatch.Elapsed.TotalSeconds;
    }

    public void Update()
    {
        if(SessionState != State.Busy) return;

        long newTransferred = _cachedTransferred;
        var bytesDone = newTransferred - _prevTransferred;

        Speed = _averager.Add(bytesDone);
        SpeedAsString = ConvertSpeed(Speed);

        if(Transferred != newTransferred)
        {
            Transferred = newTransferred;
        }

        _prevTransferred = newTransferred;
    }

    private static string ConvertSpeed(double bytesPerSecond)
    {
        const double kilobyte = 1024.0;
        int[] precisions = [0, 1, 2];
        string[] prefixes = ["", "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "Zi", "Yi"];
        int t = (Math.Abs(bytesPerSecond) < kilobyte) ? 0 : Math.Min(prefixes.Length - 1, (int)Math.Log(Math.Abs(bytesPerSecond), kilobyte));
        int precision = precisions[Math.Min(precisions.Length - 1, t)];
        return string.Format(string.Format("{{0:F{0}}} {{1}}B/s", precision), bytesPerSecond / Math.Pow(kilobyte, t), prefixes[t]);
    }
}
