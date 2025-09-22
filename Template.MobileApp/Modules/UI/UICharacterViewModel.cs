namespace Template.MobileApp.Modules.UI;

public sealed partial class UICharacterViewModel : AppViewModelBase
{
    public ObservableCollection<CharacterItem> Characters { get; } = new();

    [ObservableProperty]
    public partial string? SelectedImage { get; set; }

    public ICommand SelectCommand { get; }

    public UICharacterViewModel()
    {
        SelectCommand = MakeDelegateCommand<CharacterItem>(x => SelectedImage = x.Full);

        Characters.Add(new CharacterItem { Name = "Ruler", Color = Color.FromArgb("#81D4FA"), Face = "usa1_face.jpg", Full = "usa1_full.jpg" });
        Characters.Add(new CharacterItem { Name = "Caster", Color = Color.FromArgb("#F48FB1"), Face = "usa2_face.jpg", Full = "usa2_full.jpg" });
        Characters.Add(new CharacterItem { Name = "Saber", Color = Color.FromArgb("#80CBC4"), Face = "usa3_face.jpg", Full = "usa3_full.jpg" });
        Characters.Add(new CharacterItem { Name = "Berserker", Color = Color.FromArgb("#B0BEC5"), Face = "usa4_face.jpg", Full = "usa4_full.jpg" });
        Characters.Add(new CharacterItem { Name = "Lancer", Color = Color.FromArgb("#C5E1A5"), Face = "usa5_face.jpg", Full = "usa5_full.jpg" });
        Characters.Add(new CharacterItem { Name = "Rider", Color = Color.FromArgb("#EF9A9A"), Face = "usa6_face.jpg", Full = "usa6_full.jpg" });
        Characters.Add(new CharacterItem { Name = "Assassin", Color = Color.FromArgb("#B39DDB"), Face = "usa7_face.jpg", Full = "usa7_full.jpg" });
        Characters.Add(new CharacterItem { Name = "Alter Ego", Color = Color.FromArgb("#EEEEEE"), Face = "usa8_face.jpg", Full = "usa8_full.jpg" });
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
