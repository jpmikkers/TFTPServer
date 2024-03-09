using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using AvaTFTPServer.ViewModels;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using AvaTFTPServer.AvaloniaTools;

namespace AvaTFTPServer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(null!)
        {
            Assertions.OnlyUseEmptyConstructorInDesignMode(nameof(MainWindow));
        }

        public MainWindow(MainWindowViewModel vm)
        {
            InitializeComponent();

            if(!Design.IsDesignMode)
            {
                DataContext = vm;
            }
        }
    }
}
