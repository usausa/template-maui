namespace Template.MobileApp.Diagnostics;

using Xamarin.Android.Net;

public sealed partial class TelemetryService
{
    // Skip DiagnosticsHandler to avoid double counting of metrics on Android
    private static partial HttpMessageHandler CreateExportHandler() => new AndroidMessageHandler();
}
