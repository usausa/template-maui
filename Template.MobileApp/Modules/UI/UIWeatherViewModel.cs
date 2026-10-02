namespace Template.MobileApp.Modules.UI;

using System.Security.Cryptography;

// 時間ごとの項目。折れ線の区間は前後の値と、24 時間の範囲から描く
public sealed class UIWeatherHour
{
    public required WeatherHour Hour { get; init; }

    public required bool IsCurrent { get; init; }

    public required double? Previous { get; init; }

    public required double? Next { get; init; }

    public required double Minimum { get; init; }

    public required double Maximum { get; init; }
}

// 週間の項目。バーは週全体の範囲に合わせ、今日だけ今の気温の点を出す
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
    private readonly TimeProvider timeProvider;

    [ObservableProperty]
    public partial WeatherForecast Forecast { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<UIWeatherHour> Hours { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<UIWeatherDay> Days { get; set; } = [];

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    // 注意報の説明を開いている
    [ObservableProperty]
    public partial bool IsAlertExpanded { get; set; }

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

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    // 取得に見立てて少し待ってから、ダミーのデータを作り直す
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
        WeatherForecast.Create(timeProvider.GetLocalNow().DateTime, static x => RandomNumberGenerator.GetInt32(x));

    private void UpdateItems()
    {
        var hours = Forecast.Hours;
        var hourMinimum = hours.Min(static x => x.Temperature);
        var hourMaximum = hours.Max(static x => x.Temperature);
        Hours = hours
            .Select((x, i) => new UIWeatherHour
            {
                Hour = x,
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
