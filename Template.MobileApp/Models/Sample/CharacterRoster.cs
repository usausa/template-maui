namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class CharacterItem : ObservableObject
{
    public string Name { get; set; } = default!;

    public Color Color { get; set; } = default!;

    public string Face { get; set; } = default!;

    public string Full { get; set; } = default!;

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// キャラクターの一覧の見本 (顔と全身の絵はアプリの中の画像)
public static class CharacterSample
{
    public static IReadOnlyList<CharacterItem> LoadCharacters() =>
    [
        new() { Name = "Ruler", Color = Color.FromArgb("#81D4FA"), Face = "usa1_face.jpg", Full = "usa1_full.jpg" },
        new() { Name = "Caster", Color = Color.FromArgb("#F48FB1"), Face = "usa2_face.jpg", Full = "usa2_full.jpg" },
        new() { Name = "Saber", Color = Color.FromArgb("#80CBC4"), Face = "usa3_face.jpg", Full = "usa3_full.jpg" },
        new() { Name = "Berserker", Color = Color.FromArgb("#B0BEC5"), Face = "usa4_face.jpg", Full = "usa4_full.jpg" },
        new() { Name = "Lancer", Color = Color.FromArgb("#C5E1A5"), Face = "usa5_face.jpg", Full = "usa5_full.jpg" },
        new() { Name = "Rider", Color = Color.FromArgb("#EF9A9A"), Face = "usa6_face.jpg", Full = "usa6_full.jpg" },
        new() { Name = "Assassin", Color = Color.FromArgb("#B39DDB"), Face = "usa7_face.jpg", Full = "usa7_full.jpg" },
        new() { Name = "Alter Ego", Color = Color.FromArgb("#EEEEEE"), Face = "usa8_face.jpg", Full = "usa8_full.jpg" }
    ];
}
