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

    public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(
        nameof(CancelCommand),
        typeof(ICommand),
        typeof(ChatView));

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public static readonly BindableProperty IsRespondingProperty = BindableProperty.Create(
        nameof(IsResponding),
        typeof(bool),
        typeof(ChatView),
        false);

    public bool IsResponding
    {
        get => (bool)GetValue(IsRespondingProperty);
        set => SetValue(IsRespondingProperty, value);
    }

    public static readonly BindableProperty VoiceCommandProperty = BindableProperty.Create(
        nameof(VoiceCommand),
        typeof(ICommand),
        typeof(ChatView));

    public ICommand? VoiceCommand
    {
        get => (ICommand?)GetValue(VoiceCommandProperty);
        set => SetValue(VoiceCommandProperty, value);
    }

    public static readonly BindableProperty IsListeningProperty = BindableProperty.Create(
        nameof(IsListening),
        typeof(bool),
        typeof(ChatView),
        false);

    public bool IsListening
    {
        get => (bool)GetValue(IsListeningProperty);
        set => SetValue(IsListeningProperty, value);
    }

    public ChatView()
    {
        InitializeComponent();
    }
}
