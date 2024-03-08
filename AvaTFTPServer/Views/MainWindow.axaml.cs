using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using AvaTFTPServer.ViewModels;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace AvaTFTPServer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(null!)
        {
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
