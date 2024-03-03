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
            DataContext = vm;
            InitializeComponent();
            ListBoxLog.Loaded += ListBoxLog_Loaded;

            vm.ScrollLog = AutoScrollLog;
            vm.ScrollTransfers = AutoScrollTransfers;
        }

        private void ListBoxLog_Loaded(object? sender, RoutedEventArgs e)
        {
            if(DataContext is MainWindowViewModel viewModel) { viewModel.ScrollLogWhenEnabled(); }
        }

        private void AutoScrollLog()
        {
            var count = ListBoxLog.Items.Count;
            if(count > 0) ListBoxLog.ScrollIntoView(count - 1);
        }

        private void AutoScrollTransfers()
        {
            var count = ListBoxTransfers.Items.Count;
            if(count > 0) ListBoxTransfers.ScrollIntoView(count - 1);
        }
    }
}