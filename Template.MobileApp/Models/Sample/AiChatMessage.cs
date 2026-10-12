namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum AiChatRole
{
    User,
    Assistant
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class AiChatMessage : ObservableObject
{
    public required AiChatRole Role { get; init; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsTyping { get; set; }
}
