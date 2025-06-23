namespace Template.MobileApp.Models.Sample;

public enum DeckButtonType
{
    Text,
    Image
}

#pragma warning disable CA1819
public sealed partial class DeckButtonInfo : ObservableObject
{
    public int Row { get; set; }

    public int Column { get; set; }

    public DeckButtonType ButtonType { get; set; }

    [ObservableProperty]
    public partial string Label { get; set; } = default!;

    [ObservableProperty]
    public partial string Text { get; set; } = default!;

    [ObservableProperty]
    public partial Color TextColor { get; set; } = Colors.White;

    [ObservableProperty]
    public partial Color BackColor1 { get; set; } = Colors.Black;

    [ObservableProperty]
    public partial Color BackColor2 { get; set; } = Colors.Black;

    [ObservableProperty]
    public partial byte[] ImageBytes { get; set; } = default!;

    public ICommand Command { get; set; } = default!;

    public string Parameter { get; set; } = default!;
}
#pragma warning restore CA1819

public class DeckButtonTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextTemplate { get; set; } = default!;

    public DataTemplate ImageTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return ((DeckButtonInfo)item).ButtonType == DeckButtonType.Image ? ImageTemplate : TextTemplate;
    }
}
