using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.AvaloniaTools;
using AvaTFTPServer.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public partial class AboutDialog : Window
{
    public AboutDialog() : this(null!)
    {
        Assertions.OnlyUseEmptyConstructorInDesignMode(nameof(AboutDialog));
    }

    public AboutDialog(AboutDialogViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
