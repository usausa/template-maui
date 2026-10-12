namespace Template.MobileApp.Modules.UI;

using System.Collections.ObjectModel;

public sealed partial class UIChatViewModel : AppViewModelBase
{
    private static readonly string[] ReactionEmojis = ["👍", "❤️", "😂", "😮", "😢", "🙏"];

    private readonly IDispatcher dispatcher;

    private Selectable<ChatMessage>? reactionTarget;

    public CollectionController Controller { get; } = new();

    public ObservableCollection<Selectable<ChatMessage>> Messages { get; } = [];

    public IReadOnlyList<string> StampList { get; } = ChatSample.LoadStamps();

    public IReadOnlyList<string> ReactionList { get; } = ReactionEmojis;

    [ObservableProperty]
    public partial string InputText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsStampTrayVisible { get; set; }

    public IObserveCommand SendCommand { get; }
    public IObserveCommand SendStampCommand { get; }
    public IObserveCommand PickImageCommand { get; }
    public IObserveCommand PickStickerCommand { get; }
    public IObserveCommand ScrollToLatestCommand { get; }
    public IObserveCommand SelectCommand { get; }
    public IObserveCommand AddReactionCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIChatViewModel(IDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;

        SendCommand = MakeDelegateCommand(ExecuteSend, () => !String.IsNullOrWhiteSpace(InputText));
        SendStampCommand = MakeDelegateCommand<string>(ExecuteSendStamp);
        PickImageCommand = MakeAsyncCommand(PickImageAsync);
        PickStickerCommand = MakeDelegateCommand(() =>
        {
            var open = !IsStampTrayVisible;
            CloseTrays();
            IsStampTrayVisible = open;
        });
        ScrollToLatestCommand = MakeDelegateCommand(() => ScrollToLast());
        SelectCommand = MakeDelegateCommand<Selectable<ChatMessage>>(SelectMessage);
        AddReactionCommand = MakeDelegateCommand<string>(AddReaction);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            foreach (var message in ChatSample.LoadMessages(DateTime.Today))
            {
                Add(message);
            }
        }
        return Task.CompletedTask;
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        ScrollToLast(animate: false);
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync()
    {
        if ((reactionTarget is not null) || IsStampTrayVisible)
        {
            CloseTrays();
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.UIMenu1);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ScrollToLast(bool animate = true)
    {
        if (Messages.Count == 0)
        {
            return;
        }

        // 追加直後はレイアウト前のため次のループでスクロールする
        dispatcher.Dispatch(() => Controller.ScrollRequest(Messages.Count - 1, position: ScrollToPosition.End, animate: animate));
    }

    private void ExecuteSend()
    {
        Add(ChatTalk.Send(DateTime.Now, InputText.Trim()));
        InputText = string.Empty;
        CloseTrays();
        ScrollToLast();
    }

    private async Task PickImageAsync()
    {
        var files = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
        var file = files.FirstOrDefault();
        if (file is not null)
        {
            Add(ChatTalk.SendPhoto(DateTime.Now, file.FullPath));
            CloseTrays();
            ScrollToLast();
        }
    }

    // 同じメッセージをもう一度押すと閉じる
    private void SelectMessage(Selectable<ChatMessage> message)
    {
        var select = !message.IsSelected;
        CloseTrays();
        if (select)
        {
            message.IsSelected = true;
            reactionTarget = message;

            // 下に出す帯が隠れないよう、選んだメッセージが見える位置までスクロールする
            var index = Messages.IndexOf(message);
            dispatcher.Dispatch(() => Controller.ScrollRequest(index, position: ScrollToPosition.MakeVisible));
        }
    }

    private void AddReaction(string emoji)
    {
        reactionTarget?.Item.AddReaction(emoji);
        CloseTrays();
    }

    private void CloseTrays()
    {
        reactionTarget?.IsSelected = false;
        reactionTarget = null;
        IsStampTrayVisible = false;
    }

    private void ExecuteSendStamp(string stamp)
    {
        Add(ChatTalk.SendStamp(DateTime.Now, stamp));
        CloseTrays();
        ScrollToLast();
    }

    private void Add(ChatMessage message) => Messages.Add(new Selectable<ChatMessage>(message));
}
