namespace Template.MobileApp.Modules.UI;

public sealed partial class UICharacterViewModel : AppViewModelBase
{
    public ObservableCollection<CharacterItem> Characters { get; } = [.. CharacterSample.LoadCharacters()];

    [ObservableProperty]
    public partial string? SelectedImage { get; set; }

    public ICommand SelectCommand { get; }

    public ICommand FavoriteCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UICharacterViewModel()
    {
        SelectCommand = MakeDelegateCommand<CharacterItem>(x => SelectedImage = x.Full);
        FavoriteCommand = MakeDelegateCommand<CharacterItem>(x => x.IsFavorite = !x.IsFavorite);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
