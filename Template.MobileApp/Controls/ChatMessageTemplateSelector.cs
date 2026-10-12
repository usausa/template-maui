namespace Template.MobileApp.Controls;

public sealed class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate SendTemplate { get; set; } = default!;

    public DataTemplate ReceiveTemplate { get; set; } = default!;

    public DataTemplate SystemTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is not Selectable<ChatMessage> message)
        {
            return SystemTemplate;
        }

        return message.Item.Type switch
        {
            MessageType.Send => SendTemplate,
            MessageType.Receive => ReceiveTemplate,
            _ => SystemTemplate
        };
    }
}
