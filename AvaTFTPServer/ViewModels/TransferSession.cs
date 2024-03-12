﻿using Avalonia.Threading;
using AvaTFTPServer.Misc;
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

    private class SessionAdapter(TransferSession parent) : ITFTPSessionInfo
    {
        public long Id { get; set; }

        public void Start(TFTPSessionStartInfo args) => parent.Start(args);

        public void UpdateStart(TFTPSessionUpdateInfo args) => parent.UpdateStart(args);

        public void Progress(long transferred) => parent._cachedTransferred = transferred;

        public void Complete() => parent.Complete();

        public void Stop(Exception e) => parent.Stop(e);
    }

    [ObservableProperty]
    private State _sessionState = State.Created;

    [ObservableProperty]
    private DateTime _startTimeUtc = DateTime.UtcNow;

    public DateTime StartTimeLocal
    {
        get=> StartTimeUtc.ToLocalTime(); 
    }

    public DateTime CompletionTimeUtc
    {
        get => StartTimeUtc + _stopwatch.Elapsed;
    }

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

    public TransferSession(TimeSpan updateInterval, TimeSpan averagingInterval)
    {
         _link = new SessionAdapter(this);
        _averager = new SimpleMovingAverage((int)Math.Max(1.0, Math.Ceiling(averagingInterval / updateInterval)));
    }

    private void Start(TFTPSessionStartInfo args)
    {
        Debug.WriteLine("session start");

        FileLength = args.FileLength;
        Filename = args.Filename;
        IsUpload = args.IsUpload;
        LocalEndPoint = args.LocalEndPoint;
        RemoteEndPoint = args.RemoteEndPoint;
        WindowSize = args.WindowSize;
        SessionState = State.Busy;
        StartTimeUtc = args.StartTimeUtc;
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
        SpeedAsString = TransferSpeed.ToString(Speed);
    }

    public void Update()
    {
        if(SessionState != State.Busy) return;

        long newTransferred = _cachedTransferred;
        var bytesDone = newTransferred - _prevTransferred;

        Speed = _averager.Add(bytesDone);
        SpeedAsString = TransferSpeed.ToString(Speed);

        if(Transferred != newTransferred)
        {
            Transferred = newTransferred;
        }

        _prevTransferred = newTransferred;
    }
}
