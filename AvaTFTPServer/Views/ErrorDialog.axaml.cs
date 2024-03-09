using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.AvaloniaTools;
using AvaTFTPServer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public partial class ErrorDialog : Window
{
    public ErrorDialog() : this(null!)
    {
        Assertions.OnlyUseEmptyConstructorInDesignMode(nameof(ErrorDialog));
    }

    public ErrorDialog(ErrorDialogViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }

    public static async Task ShowErrorDialog(Window owner, string title, string header, string details)
    {
        var dialog= App.AppHost!.Services.GetRequiredService<ErrorDialog>();
        var vm = (ErrorDialogViewModel)dialog.DataContext!;

        vm.Title = title;
        vm.Header = header;
        vm.Details = details;

        await dialog.ShowDialog(owner);
    }
}
