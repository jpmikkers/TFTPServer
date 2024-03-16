using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.AvaloniaTools;
using AvaTFTPServer.ViewModels;
using AvaTFTPServer.Views;
using Baksteen.Avalonia.Tools.GridIndexer;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaTFTPServer;


public partial class EndPointSelectionDialog : Window
{
    public record class ChangeConfigResult
    {
        public DialogResult DialogResult { get; set; }
        public UISettings Settings { get; set; } = new UISettings();
    }

    public EndPointSelectionDialog() : this(null!)
    {
        Assertions.OnlyUseEmptyConstructorInDesignMode(nameof(EndPointSelectionDialog));
    }

    public EndPointSelectionDialog(EndPointSelectionDialogViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }
}