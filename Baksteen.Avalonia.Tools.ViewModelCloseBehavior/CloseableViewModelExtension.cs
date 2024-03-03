using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Baksteen.Avalonia.Tools.CloseableViewModel;

public static class CloseableViewModelExtension
{
    private static Window? LocateView(object vm)
    {
        if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            return lifetime.Windows.Where(x => x.DataContext == vm).FirstOrDefault();
        }
        return null;
    }

    public static void Close(this ICloseableViewModel vm)
    {
        LocateView(vm)?.Close();
    }

    public static void Close<TResult>(this ICloseableViewModel<TResult> vm, TResult? result)
    {
        LocateView(vm)?.Close(result);
    }
}
