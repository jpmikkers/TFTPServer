using Avalonia.Controls;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace AvaTFTPServer.AvaloniaTools;

/// <summary>
/// Provide some helpers to show dialogs via a Background priority invoke, this avoids a problem where the parent window 
/// keeps (or steals?) the focus even though the dialog is visible
/// </summary>
public static class DialogHelpers
{
    public static Task<TResult> ShowDialog<TResult>(Window owner, Window dialog)
        => Dispatcher.UIThread.InvokeAsync(
            () => dialog.ShowDialog<TResult>(owner),
            DispatcherPriority.Background);

    public static Task ShowDialog(Window owner, Window dialog)
        => Dispatcher.UIThread.InvokeAsync(
            () => dialog.ShowDialog(owner),
            DispatcherPriority.Background);
}
