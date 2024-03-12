using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using AvaTFTPServer.ViewModels;
using System.Net;
using Baksteen.Avalonia.Tools.GridIndexer;
using Microsoft.Extensions.DependencyInjection;
using AvaTFTPServer.AvaloniaTools;
using AvaTFTPServer.Views;

namespace AvaTFTPServer;

public partial class ConfigDialog : Window
{
    public record class ChangeConfigResult
    {
        public ConfigDialogViewModel.DialogResult DialogResult { get; set; }
        public ServerSettings ServerSettings { get; set; } = new ServerSettings();
    }

    public ConfigDialog() : this(null!)
    {
        Assertions.OnlyUseEmptyConstructorInDesignMode(nameof(ConfigDialog));
    }

    public ConfigDialog(ConfigDialogViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }

}
