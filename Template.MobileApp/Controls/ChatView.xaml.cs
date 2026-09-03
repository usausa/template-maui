namespace Template.MobileApp.Controls;

using System.Collections;

// 会話リストと入力バーを内包する再利用チャットコントロール
public partial class ChatView
{
    public static readonly BindableProperty MessagesProperty = BindableProperty.Create(
        nameof(Messages),
        typeof(IEnumerable),
        typeof(ChatView));

    public IEnumerable? Messages
    {
        get => (IEnumerable?)GetValue(MessagesProperty);
        set => SetValue(MessagesProperty, value);
    }

    public static readonly BindableProperty InputTextProperty = BindableProperty.Create(
        nameof(InputText),
        typeof(string),
        typeof(ChatView),
        string.Empty,
        BindingMode.TwoWay);

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    public static readonly BindableProperty SendCommandProperty = BindableProperty.Create(
        nameof(SendCommand),
        typeof(ICommand),
        typeof(ChatView));

    public ICommand? SendCommand
    {
        get => (ICommand?)GetValue(SendCommandProperty);
        set => SetValue(SendCommandProperty, value);
    }

    public ChatView()
    {
        InitializeComponent();
    }
}
