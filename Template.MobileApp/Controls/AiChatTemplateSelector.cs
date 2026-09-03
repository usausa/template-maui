namespace Template.MobileApp.Controls;

using Template.MobileApp.Models.Sample.Chat;

public sealed class AiChatTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserTemplate { get; set; }

    public DataTemplate? AssistantTemplate { get; set; }

    public DataTemplate? CodeTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var message = (AiChatMessage)item;
        if (message.Role == AiChatRole.User)
        {
            return UserTemplate!;
        }
        return message.IsCode ? CodeTemplate! : AssistantTemplate!;
    }
}
