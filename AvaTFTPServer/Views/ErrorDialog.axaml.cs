using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaTFTPServer.ViewModels;
using System.Threading.Tasks;

namespace AvaTFTPServer;

public partial class ErrorDialog : Window
{
    private class ViewMethodsImp(ErrorDialog parent) : ErrorDialogViewModel.IViewMethods
    {
        public void Close()
        {
            parent.Close();
        }
    }

    public ErrorDialog()
    {
        InitializeComponent();
    }

    public static async Task ShowErrorDialog(Window owner, string title, string header, string details)
    {
        var vm = new ErrorDialogViewModel()
        {
            Title = title,
            Header = header,
            Details = details
        };

        var dialog = new ErrorDialog()
        {
            DataContext = vm
        };

        vm.ViewMethods = new ViewMethodsImp(dialog);

        await dialog.ShowDialog(owner);
    }
}
