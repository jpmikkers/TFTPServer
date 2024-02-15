namespace Baksteen.Net.TFTP.Server;

public class DefaultTFTPSessionInfoFactory : ITFTPSessionInfoFactory
{
    public ITFTPSessionInfo Create()
    {
        return new DummyLiveSessionInfo();
    }
}
