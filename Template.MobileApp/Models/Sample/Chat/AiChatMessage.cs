namespace Template.MobileApp.Models.Sample.Chat;

public enum AiChatRole
{
    User,
    Assistant
}

public sealed partial class AiChatMessage : ObservableObject
{
    public required AiChatRole Role { get; init; }

    public bool IsCode { get; init; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsTyping { get; set; }
}
