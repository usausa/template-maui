namespace Template.MobileApp.Controls;

public sealed class AiChatTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserTemplate { get; set; }

    public DataTemplate? AssistantTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var message = (AiChatMessage)item;
        return message.Role == AiChatRole.User ? UserTemplate! : AssistantTemplate!;
    }
}
