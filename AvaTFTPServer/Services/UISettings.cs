using System;

namespace AvaTFTPServer;

public record class UISettings
{
    public TimeSpan CleanupTransfersAfter { get; set; } = TimeSpan.FromMinutes(1);
    public bool AutoScrollLog { get; set; } = true;
}
