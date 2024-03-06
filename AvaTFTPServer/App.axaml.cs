using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.Logging;
using AvaTFTPServer.ViewModels;
using AvaTFTPServer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvaTFTPServer
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public static void RunAvaloniaAppWithHosting(string[] args, Func<AppBuilder> avaloniaAppBuilder)
        {
            var appBuilder = Host.CreateApplicationBuilder(args);
            //appBuilder.Logging.ClearProviders();
            appBuilder.Logging.AddDebug();

            //appBuilder.Logging.AddProvider(new GuiLoggerProvider());
            appBuilder.Logging.Services.AddSingleton<ILoggerProvider,GuiLoggerProvider>();
            appBuilder.Logging.Services.AddSingleton<GuiLogger>();

            appBuilder.Services.AddSingleton<MainWindowViewModel>();
            appBuilder.Services.AddSingleton<MainWindow>();
            appBuilder.Services.AddTransient<ITFTPAppDialogs,TFTPAppDialogsImpl>();
            appBuilder.Services.AddSingleton<IViewResolver, ViewResolver>();

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
                BindingPlugins.DataValidators.RemoveAll(x => x is DataAnnotationsValidationPlugin);
                desktop.MainWindow = AppHost!.Services.GetRequiredService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}