using System;
using System.Net;

namespace CodePlex.JPMikkers.TFTP;

public interface ITFTPLiveSessionInfoFactory
{
    ITFTPLiveSessionInfo Create();
}

public class DefaultTFTPLiveSessionInfoFactory : ITFTPLiveSessionInfoFactory
{
    public ITFTPLiveSessionInfo Create()
    {
        return new DummyLiveSessionInfo();
    }
}

public enum TFTPLiveSessionState
{
    Busy,
    Stopped,
    Completed,
    Zombie
}

public interface ITFTPLiveSessionInfo
{
    public long Id { get; }
    public TFTPLiveSessionState State { get; }

    DateTime StartTime { get; set; }
    bool IsUpload { get; set; }
    string Filename { get; set; }
    long FileLength { get; set; }
    IPEndPoint LocalEndPoint { get; set; }
    IPEndPoint RemoteEndPoint { get; set; }
    int WindowSize { get; set; }
    public long Transferred { get; set; }
    public double Speed { get; set; }
    public Exception? Exception { get; set; }

    void Progress(long transferred);
    void Stop(Exception e);
    void Complete();
}

public class DummyLiveSessionInfo : ITFTPLiveSessionInfo
{
    private static IPEndPoint s_endPoint = new IPEndPoint(IPAddress.Any, 0);

    public long Id => 0;

    public TFTPLiveSessionState State => TFTPLiveSessionState.Zombie;

    public DateTime StartTime { get; set ; }
    public bool IsUpload { get; set; }
    public string Filename { get; set; } = string.Empty;
    public long FileLength { get; set; }
    public IPEndPoint LocalEndPoint { get; set; } = s_endPoint;
    public IPEndPoint RemoteEndPoint { get; set; } = s_endPoint;
    public int WindowSize { get; set; }
    public long Transferred { get; set; }
    public double Speed { get; set; }
    public Exception? Exception { get; set; }

    public void Complete()
    {
    }

    public void Progress(long transferred)
    {
    }

    public void Stop(Exception e)
    {
    }
}
