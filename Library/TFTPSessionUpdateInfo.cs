namespace Baksteen.Net.TFTP.Server;

public record class TFTPSessionUpdateInfo
{
    public required long FileLength { get; init; }
}
