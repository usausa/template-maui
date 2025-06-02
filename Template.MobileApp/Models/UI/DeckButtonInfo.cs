namespace Template.MobileApp.Models.UI;

public enum DeckButtonType
{
    Text,
    Image
}

#pragma warning disable CA1819
public sealed class DeckButtonInfo
{
    public int Row { get; set; }

    public int Column { get; set; }

    public DeckButtonType ButtonType { get; set; }

    public string Label { get; set; } = default!;

    public string Image { get; set; } = default!;

    public string Text { get; set; } = default!;

    public Color TextColor { get; set; } = Colors.White;

    public Color BackColor1 { get; set; } = Colors.Black;

    public Color BackColor2 { get; set; } = Colors.Black;

    public byte[]? ImageBytes { get; set; }
}
#pragma warning restore CA1819
