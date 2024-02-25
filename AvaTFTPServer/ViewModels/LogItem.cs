using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;

namespace AvaTFTPServer.ViewModels;

public record class LogItem
{
    public DateTime TimestampUtc { get; set; }
    public DateTime TimestampLocal { get => TimestampUtc.ToLocalTime(); }
    public LogLevel Level { get; set; }
    public EventId Id { get; set; }
    public Color Color { get; set; } = Colors.White;
    public string Text { get; set; } = "dummy";
}
