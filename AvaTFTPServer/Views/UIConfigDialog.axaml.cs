using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.AvaloniaTools;
using AvaTFTPServer.ViewModels;
using AvaTFTPServer.Views;
using Baksteen.Avalonia.Tools.GridIndexer;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaTFTPServer;


public partial class UIConfigDialog : Window
{
    public UIConfigDialog() : this(null!)
    {
        Assertions.OnlyUseEmptyConstructorInDesignMode(nameof(UIConfigDialog));
    }

    public UIConfigDialog(UIConfigDialogViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }
}