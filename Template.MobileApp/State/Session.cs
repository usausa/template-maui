namespace Template.MobileApp.State;

#pragma warning disable CA1724
public sealed partial class Session : ObservableObject
{
    [ObservableProperty]
    public partial bool IsForeground { get; set; } = true;
}
#pragma warning restore CA1724
