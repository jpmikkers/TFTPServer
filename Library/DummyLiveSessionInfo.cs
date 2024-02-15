using System;
using System.Net;

namespace Baksteen.Net.TFTP.Server;

public class DummyLiveSessionInfo : ITFTPSessionInfo
{
    private static IPEndPoint s_endPoint = new IPEndPoint(IPAddress.Any, 0);

    public long Id => 0;

    public void Complete()
    {
    }

    public void Progress(long transferred)
    {
    }

    public void Start(TFTPSessionStartInfo args)
    {
    }

    public void Stop(Exception e)
    {
    }

    public void UpdateStart(TFTPSessionUpdateInfo args)
    {
    }
}
