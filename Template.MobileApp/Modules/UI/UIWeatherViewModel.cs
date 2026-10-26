namespace Template.MobileApp.Modules.UI;

using System.Security.Cryptography;

public sealed class UIWeatherHour
{
    public required WeatherHour Hour { get; init; }

    public required int Index { get; init; }

    public required bool IsCurrent { get; init; }

    public required double? Previous { get; init; }

    public required double? Next { get; init; }

    public required double Minimum { get; init; }

    public required double Maximum { get; init; }
}

public sealed class UIWeatherDay
{
    public required WeatherDay Day { get; init; }

    public required bool IsToday { get; init; }

    public required double Minimum { get; init; }

    public required double Maximum { get; init; }

    public required double? Current { get; init; }
}

public sealed partial class UIWeatherViewModel : AppViewModelBase
{
    // Field

    private readonly TimeProvider timeProvider;

    // Property

    [ObservableProperty]
    public partial WeatherForecast Forecast { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<UIWeatherHour> Hours { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<UIWeatherDay> Days { get; set; } = [];

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial bool IsAlertExpanded { get; set; }

    // Command

    public IObserveCommand RefreshCommand { get; }

    public IObserveCommand AlertCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIWeatherViewModel(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        RefreshCommand = MakeAsyncCommand(RefreshAsync);
        AlertCommand = MakeDelegateCommand(() => IsAlertExpanded = !IsAlertExpanded);

        Forecast = CreateForecast();
        UpdateItems();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task RefreshAsync()
    {
        await Task.Delay(800).ConfigureAwait(true);

        Forecast = CreateForecast();
        UpdateItems();
        IsAlertExpanded = false;
        IsRefreshing = false;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private WeatherForecast CreateForecast() =>
        WeatherSample.LoadForecast(timeProvider.GetLocalNow().DateTime, static x => RandomNumberGenerator.GetInt32(x));

    private void UpdateItems()
    {
        var hours = Forecast.Hours;
        var hourMinimum = hours.Min(static x => x.Temperature);
        var hourMaximum = hours.Max(static x => x.Temperature);
        Hours = hours
            .Select((x, i) => new UIWeatherHour
            {
                Hour = x,
                Index = i,
                IsCurrent = i == 0,
                Previous = i > 0 ? hours[i - 1].Temperature : null,
                Next = i < hours.Count - 1 ? hours[i + 1].Temperature : null,
                Minimum = hourMinimum,
                Maximum = hourMaximum
            })
            .ToArray();

        var days = Forecast.Days;
        var dayMinimum = days.Min(static x => x.Low);
        var dayMaximum = days.Max(static x => x.High);
        Days = days
            .Select((x, i) => new UIWeatherDay
            {
                Day = x,
                IsToday = i == 0,
                Minimum = dayMinimum,
                Maximum = dayMaximum,
                Current = i == 0 ? Forecast.Temperature : null
            })
            .ToArray();
    }
}
