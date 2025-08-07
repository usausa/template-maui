namespace Template.MobileApp.Modules;

public static class ScreenSize
{
    // Device

    public static double Px1 => 1 / DeviceDisplay.MainDisplayInfo.Density;
    public static double Px2 => 2 / DeviceDisplay.MainDisplayInfo.Density;
    public static double Px3 => 3 / DeviceDisplay.MainDisplayInfo.Density;
    public static double Px4 => 4 / DeviceDisplay.MainDisplayInfo.Density;

    // Maui

    public static double Height { get; } = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;

    public static double Width { get; } = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;

    // Dialog

    public static double LargeDialogWidth { get; } = Width * 0.8;
}
