namespace Template.MobileApp.Modules.UI;

public sealed partial class UIGaugeViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial double Pressure { get; set; } = 1013;

    [ObservableProperty]
    public partial double Humidity { get; set; } = 50;

    [ObservableProperty]
    public partial double Temperature { get; set; } = 22;

    [ObservableProperty]
    public partial double WindDirection { get; set; } = 90;

    [ObservableProperty]
    public partial double SpeedKmh { get; set; } = 80;

    [ObservableProperty]
    public partial double Rpm { get; set; } = 3.5;

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
