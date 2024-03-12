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
}
