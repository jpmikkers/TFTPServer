using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using System;

namespace Baksteen.Avalonia.Tools.AutoScrollBehavior;

// see https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-attached-properties
public sealed class AutoScrollBehavior : AvaloniaObject
{
    private record class AutoScrollTuple
    {
        public required ListBox ListBox { get; init; }
        public required INotifyCollectionChanged Collection { get; init; }

        public bool ScrollPending { get; set; }
    }


    public static readonly AvaloniaProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<AutoScrollBehavior, ListBox, bool>("IsActive", false, false, BindingMode.OneWay);

    public static bool GetIsActive(AvaloniaObject obj) => (bool)(obj.GetValue(IsActiveProperty) ?? false);

    public static void SetIsActive(AvaloniaObject obj, bool value) => obj.SetValue(IsActiveProperty, value);

    private static readonly ConcurrentDictionary<ListBox, AutoScrollTuple> s_ListBoxMap = new();
    private static readonly ConcurrentDictionary<INotifyCollectionChanged, AutoScrollTuple> s_CollectionMap = new();

    static AutoScrollBehavior()
    {
        IsActiveProperty.Changed.AddClassHandler<ListBox>(HandleCommandChanged);
    }

    private static void HandleCommandChanged(ListBox listBox, AvaloniaPropertyChangedEventArgs args)
    {
        if(args.NewValue is true)
        {
            listBox.Loaded -= ListBox_Loaded;
            listBox.Loaded += ListBox_Loaded;
            listBox.Unloaded -= ListBox_Unloaded;
            listBox.Unloaded += ListBox_Unloaded;

            if(listBox.IsLoaded)
            {
                Register(listBox);
            }
        }
        else
        {
            listBox.Loaded -= ListBox_Loaded;
            listBox.Unloaded -= ListBox_Unloaded;
            Unregister(listBox);
        }
    }

    private static void ListBox_Loaded(object? sender, RoutedEventArgs e)
    {
        if(sender is ListBox listBox)
        {
            Register(listBox);
        }
    }

    private static void ListBox_Unloaded(object? sender, RoutedEventArgs e)
    {
        if(sender is ListBox listBox)
        {
            Unregister(listBox);
        }
    }

    private static void ListBox_DataContextChanged(object? sender, System.EventArgs e)
    {
        if(sender is ListBox listBox)
        {
            Unregister(listBox);
            Register(listBox);
        }
    }

    private static void Register(ListBox listBox)
    {
        if(!s_ListBoxMap.ContainsKey(listBox))
        {
            listBox.DataContextChanged -= ListBox_DataContextChanged;
            listBox.DataContextChanged += ListBox_DataContextChanged;

            if(listBox.ItemsSource is INotifyCollectionChanged collection)
            {
                var tuple = new AutoScrollTuple { Collection = collection, ListBox = listBox, ScrollPending = false };

                s_ListBoxMap[listBox] = tuple;
                s_CollectionMap[collection] = tuple;

                collection.CollectionChanged -= Collection_CollectionChanged;
                collection.CollectionChanged += Collection_CollectionChanged;

                SmartScroll(tuple);
            }
        }
    }
    private static void Unregister(ListBox listBox)
    {
        listBox.DataContextChanged -= ListBox_DataContextChanged;

        if(s_ListBoxMap.TryRemove(listBox, out var tuple))
        {
            tuple.Collection.CollectionChanged -= Collection_CollectionChanged;
            s_CollectionMap.TryRemove(tuple.Collection, out _);
        }
    }

    private static void Collection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if(sender is INotifyCollectionChanged collection)
        {
            if(s_CollectionMap.TryGetValue(collection, out var tuple))
            {
                SmartScroll(tuple);
            }
        }
    }

    private static void SmartScroll(AutoScrollTuple tuple)
    {
        if(!tuple.ScrollPending)
        {
            tuple.ScrollPending = true;
            var listBox = tuple.ListBox;

            Dispatcher.UIThread.Post(() =>
            {
                if(listBox.IsVisible && listBox.ItemCount > 0)
                {
                    listBox.ScrollIntoView(listBox.ItemCount - 1);
                    System.Diagnostics.Debug.WriteLine("performed autoscroll");
                }
                tuple.ScrollPending = false;
            }, DispatcherPriority.Background);
        }
    }
}
