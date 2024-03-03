#if NEVER
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Data;
using System.Collections.Concurrent;

namespace Baksteen.Avalonia.Tools.ViewModelCloseBehavior;

// see https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-attached-properties
public sealed class ViewModelCloseBehavior : AvaloniaObject
{
    public static readonly AvaloniaProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<ViewModelCloseBehavior, Interactive, bool>("IsActive", false, false, BindingMode.OneWay);

    public static bool GetIsActive(AvaloniaObject obj) => (bool)(obj.GetValue(IsActiveProperty) ?? false);

    public static void SetIsActive(AvaloniaObject obj, bool value) => obj.SetValue(IsActiveProperty, value);

    private static readonly ConcurrentDictionary<Window, ICloseableViewModel> s_registeredViewModels = new();

    static ViewModelCloseBehavior()
    {
        IsActiveProperty.Changed.AddClassHandler<Interactive>(HandleCommandChanged);
    }

    private static void HandleCommandChanged(Interactive interactive, AvaloniaPropertyChangedEventArgs args)
    {
        if(interactive is Window window)
        {
            window.Closed += Window_Closed;
            window.Loaded += Window_Loaded;
            window.DataContextChanged += Window_DataContextChanged;
        }
    }

    private static void Window_Closed(object? sender, EventArgs e)
    {
        if(sender is Window window)
        {
            window.Closed -= Window_Closed;
            window.Loaded -= Window_Loaded;
            window.DataContextChanged -= Window_DataContextChanged;
            s_registeredViewModels.TryRemove(window, out _);
        }
    }

    private static void RegisterCloseableViewModel(Window window)
    {
        if(window.DataContext is ICloseableViewModel vm)
        {
            s_registeredViewModels[window] = vm;
        }
    }

    private static void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        if(sender is Window window)
        {
            RegisterCloseableViewModel(window);
        }
    }

    private static void Window_DataContextChanged(object? sender, EventArgs e)
    {
        if(sender is Window window)
        {
            RegisterCloseableViewModel(window);
        }
    }

    internal static void Close(ICloseableViewModel vm, object? result)
    {
        var view = s_registeredViewModels.ToList().Where(t => ReferenceEquals(t.Value,vm)).Select(x => x.Key).FirstOrDefault();
        view?.Close(result);        
    }
}
#endif