namespace Template.MobileApp;

using System.Reflection;

using Template.MobileApp.Behaviors;
using Template.MobileApp.Components;
using Template.MobileApp.Helpers;

public static class Extensions
{
    //--------------------------------------------------------------------------------
    // Resource
    //--------------------------------------------------------------------------------

    public static T FindResource<T>(this ResourceDictionary resource, string key) =>
        resource.TryGetValue(key, out var value) ? (T)value : default!;

    public static IEnumerable<Type> UnderNamespaceTypes(this Assembly assembly, Type baseNamespaceType)
    {
        var ns = baseNamespaceType.Namespace!;
        return assembly.ExportedTypes.Where(x => x.Namespace?.StartsWith(ns, StringComparison.Ordinal) ?? false);
    }

    //--------------------------------------------------------------------------------
    // Element
    //--------------------------------------------------------------------------------

    public static void SetDefaultFocus(this IVisualTreeElement parent)
    {
        var first = default(VisualElement);
        foreach (var visualElement in ElementHelper.EnumerateFocusable(parent))
        {
            if (Focus.GetDefault(visualElement))
            {
                visualElement.Focus();
                return;
            }

            first ??= visualElement;
        }

        first?.Focus();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidMethod
    public static async ValueTask PostForwardAsync(this INavigator navigator, object viewId, NavigationParameter? parameter = null)
    {
        if (navigator.Executing)
        {
            async void ExecutingChanged(object? sender, EventArgs args)
            {
                if (!navigator.Executing)
                {
                    navigator.ExecutingChanged -= ExecutingChanged;
                    await navigator.ForwardAsync(viewId, parameter);
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
            async void ExecutingChanged(object? sender, EventArgs args)
            {
                if (!navigator.Executing)
                {
                    navigator.ExecutingChanged -= ExecutingChanged;
                    await task();
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

    public static IObservable<ScreenStateEventArgs> StateChangedAsObservable(this IScreen screen) =>
        Observable.FromEvent<EventHandler<ScreenStateEventArgs>, ScreenStateEventArgs>(static h => (_, e) => h(e), h => screen.ScreenStateChanged += h, h => screen.ScreenStateChanged -= h);

    public static IObservable<LocationEventArgs> LocationChangedAsObservable(this ILocationService locationService) =>
        Observable.FromEvent<EventHandler<LocationEventArgs>, LocationEventArgs>(static h => (_, e) => h(e), h => locationService.LocationChanged += h, h => locationService.LocationChanged -= h);

    public static IObservable<SpeechRecognizeEventArgs> RecognizedAsObservable(this ISpeechService speechService) =>
        Observable.FromEvent<EventHandler<SpeechRecognizeEventArgs>, SpeechRecognizeEventArgs>(static h => (_, e) => h(e), h => speechService.Recognized += h, h => speechService.Recognized -= h);

    public static IObservable<NfcEventArgs> DetectedAsObservable(this INfcReader nfcReader) =>
        Observable.FromEvent<EventHandler<NfcEventArgs>, NfcEventArgs>(static h => (_, e) => h(e), h => nfcReader.Detected += h, h => nfcReader.Detected -= h);

    public static IObservable<NoiseEventArgs> MeasuredAsObservable(this INoiseMonitor noiseMonitor) =>
        Observable.FromEvent<EventHandler<NoiseEventArgs>, NoiseEventArgs>(static h => (_, e) => h(e), h => noiseMonitor.Measured += h, h => noiseMonitor.Measured -= h);
}
