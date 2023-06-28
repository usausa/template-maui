namespace Template.MobileApp.State;

public class Settings
{
    private readonly IPreferences preferences;

    public Settings(IPreferences preferences)
    {
        this.preferences = preferences;
    }

    public string ServerAddress
    {
        get => preferences.Get<string>(nameof(ServerAddress), default!);
        set => preferences.Set(nameof(ServerAddress), value);
    }
}
