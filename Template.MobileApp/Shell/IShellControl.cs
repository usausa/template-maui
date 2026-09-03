namespace Template.MobileApp.Shell;

public sealed class FunctionState
{
    public NotificationValue<string> Text { get; } = new(string.Empty);

    public NotificationValue<bool> Enabled { get; } = new();
}

public interface IShellControl
{
    NotificationValue<string> Title { get; }

    NotificationValue<bool> HeaderVisible { get; }

    NotificationValue<bool> FunctionVisible { get; }

    // [0]=Function1 .. [3]=Function4
    IReadOnlyList<FunctionState> Functions { get; }
}
