using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.AvaloniaTools;
using AvaTFTPServer.ViewModels;
using AvaTFTPServer.Views;
using Baksteen.Avalonia.Tools.GridIndexer;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaTFTPServer;


public partial class SelectNetworkDialog : Window
{
    public record class ChangeConfigResult
    {
        public SelectNetworkDialogViewModel.DialogResult DialogResult { get; set; }
        public UISettings Settings { get; set; } = new UISettings();
    }

    public SelectNetworkDialog() : this(null!)
    {
        Assertions.OnlyUseEmptyConstructorInDesignMode(nameof(SelectNetworkDialog));
    }

    public SelectNetworkDialog(SelectNetworkDialogViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }
}