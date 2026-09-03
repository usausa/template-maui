namespace Template.MobileApp.Models.Sample;

public sealed partial class CharacterItem : ObservableObject
{
    public string Name { get; set; } = default!;

    public Color Color { get; set; } = default!;

    public string Face { get; set; } = default!;

    public string Full { get; set; } = default!;

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }
}
