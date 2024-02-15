using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using AvaTFTPServer.ViewModels;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace AvaTFTPServer.Views
{
    public partial class MainWindow : Window
    {
        private class ViewMethodsImpl(MainWindow parent) : MainWindowViewModel.IViewMethods
        {
            public void AutoScrollLog() => parent.AutoScrollLog();

            public void AutoScrollTransfers() => parent.AutoScrollTransfers();

            public void Close()
            {
                //if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();
                parent.Close();
            }

            public Task<ChangeConfigResult> ShowConfigDialog(ServerSettings serverSettings)
            {
                return ConfigDialog.ShowDialog(parent, serverSettings);
            }

            public Task ShowErrorDialog(string title, string header, string details)
            {
                return ErrorDialog.ShowErrorDialog(parent, title, header, details);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ListBoxLog.Loaded += ListBoxLog_Loaded;
            ListBoxTransfers.Loaded += ListBoxTransfers_Loaded;
        }

        private void ListBoxTransfers_Loaded(object? sender, RoutedEventArgs e)
        {
            AutoScrollTransfers();
        }

        private void ListBoxLog_Loaded(object? sender, RoutedEventArgs e)
        {
            AutoScrollLog();
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

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if(!Design.IsDesignMode && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ViewMethods = new ViewMethodsImpl(this);
            }
        }
    }
}