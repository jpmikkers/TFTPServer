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

            public Task<ConfigDialog.ChangeConfigResult> ShowConfigDialog(ServerSettings serverSettings)
            {
                return ConfigDialog.ShowDialog(parent, serverSettings);
            }

            public Task ShowErrorDialog(string title, string header, string details)
            {
                return ErrorDialog.ShowErrorDialog(parent, title, header, details);
            }

            public Task<UIConfigDialog.ChangeConfigResult> ShowUIConfigDialog(UISettings settings, string configPath)
            {
                return UIConfigDialog.ShowDialog(parent, settings, configPath);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ListBoxLog.Loaded += ListBoxLog_Loaded;
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