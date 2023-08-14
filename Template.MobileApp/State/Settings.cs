namespace Template.MobileApp.State;

public class Settings
{
    private readonly IPreferences preferences;

    public Settings(IPreferences preferences)
    {
        this.preferences = preferences;
    }

    public string UserId
    {
        get => preferences.Get<string>(nameof(UserId), default!);
        set => preferences.Set(nameof(UserId), value);
    }

    public string ApiEndPoint
    {
        get => preferences.Get<string>(nameof(ApiEndPoint), default!);
        set => preferences.Set(nameof(ApiEndPoint), value);
    }
}
