namespace Template.MobileApp.Controls;

public class DeckButtonTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextTemplate { get; set; } = default!;

    public DataTemplate ImageTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return ((DeckButtonInfo)item).ButtonType == DeckButtonType.Image ? ImageTemplate : TextTemplate;
    }
}
