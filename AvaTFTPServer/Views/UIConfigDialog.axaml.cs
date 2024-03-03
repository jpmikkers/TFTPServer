using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.ViewModels;
using AvaTFTPServer.Views;
using Baksteen.Avalonia.Tools.GridIndexer;
using System.Net;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public partial class UIConfigDialog : Window
{
    private class ViewMethodsImpl(UIConfigDialog parent) : UIConfigDialogViewModel.IViewMethods
    {
        public void Close(UIConfigDialogViewModel.DialogResult result) => parent.Close(result);
    }

    public record class ChangeConfigResult
    {
        public UIConfigDialogViewModel.DialogResult DialogResult { get; set; }
        public UISettings Settings { get; set; } = new UISettings();
    }


    public UIConfigDialog()
    {
        InitializeComponent();
        GridIndexer.RunGridIndexer(this);
    }

    public static async Task<ChangeConfigResult> ShowDialog(Window owner, UISettings settings, string configPath)
    {
        var dialog = new UIConfigDialog();

        var vm = new UIConfigDialogViewModel()
        {
            ViewMethods = new ViewMethodsImpl(dialog),
            AutoScrollLog = settings.AutoScrollLog,
            ConfigPath = configPath
        };

        dialog.DataContext = vm;

        if(await dialog.ShowDialog<UIConfigDialogViewModel.DialogResult?>(owner) == UIConfigDialogViewModel.DialogResult.Ok)
        {
            return new ChangeConfigResult
            {
                DialogResult = UIConfigDialogViewModel.DialogResult.Ok,
                Settings = new UISettings
                {
                    AutoScrollLog = vm.AutoScrollLog
                }
            };
        }
        else
        {
            return new ChangeConfigResult
            {
                DialogResult = UIConfigDialogViewModel.DialogResult.Cancel,
                Settings = settings
            };
        }
    }
}