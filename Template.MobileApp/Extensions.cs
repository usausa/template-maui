namespace Template.MobileApp;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Components;
using Template.MobileApp.Helpers;

#pragma warning disable CA1724
public static class Extensions
{
    //--------------------------------------------------------------------------------
    // Resource
    //--------------------------------------------------------------------------------

    public static T FindResource<T>(this ResourceDictionary resource, string key) =>
        resource.TryGetValue(key, out var value) ? (T)value : default!;

    public static IEnumerable<(string Key, T Value)> EnumValues<T>(this ResourceDictionary resource)
    {
        if (resource is { } resources)
        {
            foreach (var key in resources.Keys)
            {
                if (resources[key] is T value)
                {
                    yield return (key, value);
                }
            }

            if (resources.MergedDictionaries is not null)
            {
                foreach (var dictionary in resources.MergedDictionaries)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        if (resources[key] is T value)
                        {
                            yield return (key, value);
                        }
                    }
                }
            }
        }
    }

    //--------------------------------------------------------------------------------
    // Element
    //--------------------------------------------------------------------------------

    public static void SetDefaultFocus(this IVisualTreeElement parent)
    {
        var element = ElementHelper.FindFocusable(parent, Focus.GetDefault) ??
                      ElementHelper.FindFocusable(parent, static _ => true);
        element?.Focus();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidMethod
    public static async ValueTask PostForwardAsync(this INavigator navigator, object viewId, NavigationParameter? parameter = null)
    {
        if (navigator.Executing)
        {
            var target = navigator.CurrentTarget;
            // ReSharper disable once AsyncVoidEventHandlerMethod
            async void ExecutingChanged(object? sender, EventArgs args)
            {
                if (!navigator.Executing)
                {
                    navigator.ExecutingChanged -= ExecutingChanged;
                    if (ReferenceEquals(navigator.CurrentTarget, target))
                    {
                        await navigator.ForwardAsync(viewId, parameter);
                    }
                }
            }

            navigator.ExecutingChanged += ExecutingChanged;
        }
        else
        {
            await navigator.ForwardAsync(viewId, parameter);
        }
    }

    // ReSharper disable once AsyncVoidMethod
    public static async ValueTask PostActionAsync(this INavigator navigator, Func<Task> task)
    {
        if (navigator.Executing)
        {
            var target = navigator.CurrentTarget;
            // ReSharper disable once AsyncVoidEventHandlerMethod
            async void ExecutingChanged(object? sender, EventArgs args)
            {
                if (!navigator.Executing)
                {
                    navigator.ExecutingChanged -= ExecutingChanged;
                    if (ReferenceEquals(navigator.CurrentTarget, target))
                    {
                        await task();
                    }
                }
            }

            navigator.ExecutingChanged += ExecutingChanged;
        }
        else
        {
            await task();
        }
    }

    //--------------------------------------------------------------------------------
    // Reactive
    //--------------------------------------------------------------------------------

    public static IObservable<EventArgs> TickAsObservable(this IDispatcherTimer timer) =>
        Observable.FromEvent<EventHandler, EventArgs>(static h => (_, e) => h(e), h => timer.Tick += h, h => timer.Tick -= h);

    public static IObservable<ScreenStateEventArgs> StateChangedAsObservable(this IScreen screen) =>
        Observable.FromEvent<EventHandler<ScreenStateEventArgs>, ScreenStateEventArgs>(static h => (_, e) => h(e), h => screen.ScreenStateChanged += h, h => screen.ScreenStateChanged -= h);

    public static IObservable<LocationEventArgs> LocationChangedAsObservable(this ILocationService locationService) =>
        Observable.FromEvent<EventHandler<LocationEventArgs>, LocationEventArgs>(static h => (_, e) => h(e), h => locationService.LocationChanged += h, h => locationService.LocationChanged -= h);

    public static IObservable<SpeechRecognizeEventArgs> RecognizedAsObservable(this ISpeechService speechService) =>
        Observable.FromEvent<EventHandler<SpeechRecognizeEventArgs>, SpeechRecognizeEventArgs>(static h => (_, e) => h(e), h => speechService.Recognized += h, h => speechService.Recognized -= h);

    public static IObservable<EventArgs> StateChangedAsObservable(this IWiFiManager wifiManager) =>
        Observable.FromEvent<EventHandler<EventArgs>, EventArgs>(static h => (_, e) => h(e), h => wifiManager.StateChanged += h, h => wifiManager.StateChanged -= h);

    public static IObservable<NotificationTappedEventArgs> TappedAsObservable(this INotificationService notificationService) =>
        Observable.FromEvent<EventHandler<NotificationTappedEventArgs>, NotificationTappedEventArgs>(static h => (_, e) => h(e), h => notificationService.Tapped += h, h => notificationService.Tapped -= h);

    public static IObservable<NfcEventArgs> DetectedAsObservable(this INfcReader nfcReader) =>
        Observable.FromEvent<EventHandler<NfcEventArgs>, NfcEventArgs>(static h => (_, e) => h(e), h => nfcReader.Detected += h, h => nfcReader.Detected -= h);

    public static IObservable<NoiseEventArgs> MeasuredAsObservable(this INoiseMonitor noiseMonitor) =>
        Observable.FromEvent<EventHandler<NoiseEventArgs>, NoiseEventArgs>(static h => (_, e) => h(e), h => noiseMonitor.Measured += h, h => noiseMonitor.Measured -= h);

    public static IObservable<ActivityEventArgs> ChangedAsObservable(this IActivityRecognizer activityRecognizer) =>
        Observable.FromEvent<EventHandler<ActivityEventArgs>, ActivityEventArgs>(static h => (_, e) => h(e), h => activityRecognizer.Changed += h, h => activityRecognizer.Changed -= h);

    public static IObservable<EventArgs> BatteryChangedAsObservable(this DeviceInformation deviceInformation) =>
        Observable.FromEvent<EventHandler, EventArgs>(static h => (_, e) => h(e), h => deviceInformation.BatteryChanged += h, h => deviceInformation.BatteryChanged -= h);

    public static IObservable<EventArgs> NetworkChangedAsObservable(this DeviceInformation deviceInformation) =>
        Observable.FromEvent<EventHandler, EventArgs>(static h => (_, e) => h(e), h => deviceInformation.NetworkChanged += h, h => deviceInformation.NetworkChanged -= h);

    public static IObservable<EventArgs> WiFiChangedAsObservable(this DeviceInformation deviceInformation) =>
        Observable.FromEvent<EventHandler, EventArgs>(static h => (_, e) => h(e), h => deviceInformation.WiFiChanged += h, h => deviceInformation.WiFiChanged -= h);
}
#pragma warning restore CA1724
