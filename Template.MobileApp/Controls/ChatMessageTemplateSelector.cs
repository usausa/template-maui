namespace Template.MobileApp.Controls;

public sealed class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? SendMessage { get; set; }

    public DataTemplate? ReceiveMessage { get; set; }

    public DataTemplate? SystemMessage { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        return ((ChatMessage)item).Type switch
        {
            MessageType.Send => SendMessage,
            MessageType.Receive => ReceiveMessage,
            MessageType.System => SystemMessage,
            _ => null
        };
    }
}
