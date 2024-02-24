using Avalonia;
using Avalonia.Dialogs;
using System;

namespace AvaTFTPServer;

internal sealed class Program
{
    // Init code moved to App.axaml.cs so it can set up Microsoft hosting and dependency injection
    [STAThread]
    public static void Main(string[] args)
    {
        App.RunAvaloniaAppWithHosting(args, BuildAvaloniaApp);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseManagedSystemDialogs();
}
