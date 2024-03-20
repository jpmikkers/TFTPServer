#if NEVER
namespace AvaTFTPServer.ViewModels;

using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaTFTPServer;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

												<!--<TextBlock.Text>
                                                    <MultiBinding StringFormat="Progress: {0} / {1}">
                                                        <MultiBinding.Bindings>
                                                            <Binding Path="Transferred" />
                                                            <Binding Path="FileLengthAsString" />
                                                        </MultiBinding.Bindings>
                                                    </MultiBinding>
                                                </TextBlock.Text>-->


											<!--<TextBlock.Inlines>
												<InlineCollection>
													--><!--<InlineUIContainer>
														<Border
															BorderBrush="Black"
															BorderThickness="1"
															CornerRadius="4"
															Width="16"
															Height="16"
															Background="{CompiledBinding Level, Converter={StaticResource logLevelConverter}}" />
													</InlineUIContainer>--><!--
													<Run Text="{CompiledBinding Level, StringFormat='{}{0,-11}'}" />
													<Run Text="{CompiledBinding Text}"/>
												</InlineCollection>
											</TextBlock.Inlines>-->


										<!--<TextBlock>
											<TextBlock.Text>
												<MultiBinding StringFormat="{}{0:000} {1,-20}">
													<MultiBinding.Bindings>
														<Binding Path="Id.Id" />
														<Binding Path="Level" />
													</MultiBinding.Bindings>
												</MultiBinding>
											</TextBlock.Text>
										</TextBlock>-->

				<!--<TabItem Header="Blah">
					<Border BorderBrush="Black" BorderThickness="2">
						<ScrollViewer>
							<StackPanel Margin="20">
								<ItemsRepeater ItemsSource="{Binding LogList}" ScrollViewer.HorizontalScrollBarVisibility="Auto" ScrollViewer.VerticalScrollBarVisibility="Auto" >
									<ItemsRepeater.ItemTemplate>
										<DataTemplate>
											<Border Margin="0,10,0,0"
												CornerRadius="5"
												BorderBrush="Blue" BorderThickness="1"
												Padding="5">
												<SelectableTextBlock Text="{Binding Text}"/>
											</Border>
										</DataTemplate>
									</ItemsRepeater.ItemTemplate>
								</ItemsRepeater>
							</StackPanel>
						</ScrollViewer>
					</Border>
				</TabItem>-->


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