using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public partial class ErrorDialog : Window
{
    public ErrorDialog()
    {
        InitializeComponent();
    }

    public static async Task ShowErrorDialog(Window owner, string title, string header, string details)
    {
        var vm = App.AppHost!.Services.GetRequiredService<ErrorDialogViewModel>();

        vm.Title = title;
        vm.Header = header;
        vm.Details = details;

        var dialog = new ErrorDialog()
        {
            DataContext = vm
        };

        await dialog.ShowDialog(owner);
    }
}
