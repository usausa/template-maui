namespace Template.MobileApp.Models.Sample.Chat;

public sealed class ChatMessage
{
    public MessageType Type { get; set; }

    public DateTime DateTime { get; init; } = DateTime.Now;

    public string Author { get; set; } = default!;

    public string TextContent { get; set; } = default!;

    public string? AvatarSource { get; set; }

    public string? StampSource { get; set; }

    public bool IsRead { get; set; }

    public IReadOnlyList<MessageReaction> Reactions { get; set; } = [];
}
