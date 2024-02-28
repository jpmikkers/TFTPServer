using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.ViewModels;
using AvaTFTPServer.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AvaTFTPServer
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public static void RunAvaloniaAppWithHosting(string[] args, Func<AppBuilder> avaloniaAppBuilder)
        {
            var appBuilder = Host.CreateApplicationBuilder(args);
            appBuilder.Logging.AddDebug();
            //appBuilder.Services.AddWindowsFormsBlazorWebView();
            //appBuilder.Services.AddBlazorWebViewDeveloperTools();
            //appBuilder.Services.AddSingleton<WeatherForecastService>();
            using var myApp = appBuilder.Build();
            App.AppHost = myApp;

            myApp.Start();

            try
            {
                avaloniaAppBuilder()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                Task.Run(async () => await myApp.StopAsync()).GetAwaiter().GetResult();
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}