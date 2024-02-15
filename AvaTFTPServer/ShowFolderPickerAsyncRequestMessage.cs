#if NEVER
namespace AvaTFTPServer.ViewModels;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaTFTPServer;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

// see https://stackoverflow.com/questions/675545/is-it-possible-to-use-a-c-sharp-object-initializer-with-a-factory-method
public struct ObjectIniter<TObject>
    {
        public ObjectIniter(TObject obj, out TObject capture)
        {
            Obj = obj;
            capture = obj;
        }

        public TObject Obj { get; }
    }

    var vm = new ObjectIniter<ConfigDialogViewModel>(new ConfigDialogViewModel(new ViewMethodsImpl(dialog)), out var captured)
        {
            Obj =
            {
                AllowDownloads = captured.AllowDownloads,
                
            }
        };



            WeakReferenceMessenger.Default.Register<MainWindow, ShowConfigDialogAsyncRequestMessage>(this, (r, m) =>
            {
                m.Reply(ConfigDialog.ShowDialog(r, m.Settings));
            });


public class ShowFolderPickerAsyncRequestMessage : AsyncRequestMessage<string?>
{
    public string Title {  get; set; } = string.Empty;
}

private static void AutoCleanupLink<TRecipient, TMessage, TReturn>(TRecipient recipient, Func<TMessage, Task<TReturn>> func)
    where TRecipient : Window
    where TMessage : AsyncRequestMessage<TReturn>
{
    WeakReferenceMessenger.Default.Register<TRecipient, TMessage>(recipient, (_, m) => m.Reply(func(m)));

    void CleanupFunc(object? sender, EventArgs args)
    {
        WeakReferenceMessenger.Default.Unregister<TMessage>(recipient);
        recipient.Closed -= CleanupFunc;
    }

    recipient.Closed += CleanupFunc;
}

//AutoCleanupLink<ConfigDialog, ShowFolderPickerAsyncRequestMessage, string?>(this, ShowFolderPicker2);

//WeakReferenceMessenger.Default.Register<ConfigDialog, ShowFolderPickerAsyncRequestMessage>(this, (_, m) =>
//{
//    m.Reply(ShowFolderPicker(m.Title));
//});

private async Task<string?> ShowFolderPicker2(ShowFolderPickerAsyncRequestMessage msg)
{
    var results = await StorageProvider.OpenFolderPickerAsync(
    new FolderPickerOpenOptions
    {
        AllowMultiple = false,
        Title = msg.Title
    });
    return results.Any() ? results.Select(x => x.TryGetLocalPath()).FirstOrDefault() : null;
}
#endif