using System;

namespace Baksteen.Net.TFTP.Server;

public interface ITFTPSessionInfo
{
    public long Id { get; }

    void Start(TFTPSessionStartInfo args);

    void UpdateStart(TFTPSessionUpdateInfo args);

    void Progress(long transferred);
    void Stop(Exception e);
    void Complete();
}
