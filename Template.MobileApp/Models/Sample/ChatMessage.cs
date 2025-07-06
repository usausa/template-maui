namespace Template.MobileApp.Models.Sample;

public enum MessageType
{
    Send,
    Receive,
    System
}

public sealed class ChatMessage
{
    public MessageType Type { get; set; }

    public DateTime DateTime { get; } = DateTime.Now;

    public string Author { get; set; } = default!;

    public string TextContent { get; set; } = default!;
}
