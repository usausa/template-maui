namespace Template.MobileApp.Modules.UI;

using System.Security.Cryptography;

using ClamGauge;

public enum GaugeKind
{
    Pressure,
    Humidity,
    Temperature,
    Wind,
    Speed,
    Rpm
}

public sealed class GaugeColorSwatch
{
    public required Brush Brush { get; init; }

    public required ICommand Command { get; init; }

    public static GaugeColorSwatch Solid(Color color, ICommand command) => new()
    {
        Brush = new SolidColorBrush(color),
        Command = command
    };

    public static GaugeColorSwatch Gradient(Color start, Color end, ICommand command) => new()
    {
        Brush = new LinearGradientBrush([new GradientStop(start, 0), new GradientStop(end, 1)], new Point(0, 0), new Point(1, 0)),
        Command = command
    };
}

public sealed partial class UIGaugeViewModel : AppViewModelBase
{
    private static readonly (Color Start, Color End)[] PressureBands =
    [
        (Color.FromArgb("#FFB0B0"), Color.FromArgb("#CC0000")),
        (Color.FromArgb("#B0D0FF"), Color.FromArgb("#1060D0")),
        (Color.FromArgb("#B8E8B8"), Color.FromArgb("#209040")),
        (Color.FromArgb("#FFD8A0"), Color.FromArgb("#E07000")),
        (Color.FromArgb("#D8C0F0"), Color.FromArgb("#7030B0"))
    ];

    private static readonly Color[] HumidityBackgrounds =
    [
        Color.FromArgb("#1A1E30"),
        Color.FromArgb("#103838"),
        Color.FromArgb("#381A38"),
        Color.FromArgb("#16301C"),
        Color.FromArgb("#2E2E2E")
    ];

    private static readonly GaugeRange[] HumidityBands =
    [
        new() { StartValue = 0, EndValue = 40, StartColor = Color.FromArgb("#2060CC"), EndColor = Color.FromArgb("#2060CC") },
        new() { StartValue = 40, EndValue = 70, StartColor = Color.FromArgb("#2880FF"), EndColor = Color.FromArgb("#40A0FF") },
        new() { StartValue = 70, EndValue = 100, StartColor = Color.FromArgb("#40A0FF"), EndColor = Color.FromArgb("#60C0FF") }
    ];

    private static readonly (Color Start, Color End)[] TemperatureArcs =
    [
        (Color.FromArgb("#3080FF"), Color.FromArgb("#FF3010")),
        (Color.FromArgb("#E0F4FF"), Color.FromArgb("#2060E0")),
        (Color.FromArgb("#40C060"), Color.FromArgb("#E8E040")),
        (Color.FromArgb("#FFC040"), Color.FromArgb("#E02060")),
        (Color.FromArgb("#FFFFFF"), Color.FromArgb("#607090"))
    ];

    private static readonly GaugeRange[] TemperatureBands =
    [
        new() { StartValue = -10, EndValue = 10, StartColor = Color.FromArgb("#3080FF"), EndColor = Color.FromArgb("#40C060") },
        new() { StartValue = 10, EndValue = 25, StartColor = Color.FromArgb("#40C060"), EndColor = Color.FromArgb("#C8D040") },
        new() { StartValue = 25, EndValue = 35, StartColor = Color.FromArgb("#C8D040"), EndColor = Color.FromArgb("#FFA040") },
        new() { StartValue = 35, EndValue = 40, StartColor = Color.FromArgb("#FFA040"), EndColor = Color.FromArgb("#FF3010") }
    ];

    private static readonly double[] TemperatureMinorTickIntervals = [1, 0.5, 0];

    private static readonly double[] WindLabelIntervals = [90, 45, 30];

    private static readonly Easing[] SpeedEasings =
    [
        Easing.CubicOut,
        Easing.Linear,
        Easing.SinInOut,
        Easing.BounceOut,
        Easing.SpringOut
    ];

    private static readonly int[] SpeedDurations = [1000, 300, 0];

    private int temperatureMinorTickIndex;

    private int windLabelIndex;

    private int speedEasingIndex;

    private int speedDurationIndex;

    private bool rpmSymmetric;

    public IReadOnlyList<string> Kinds { get; } = ["Press", "Humid", "Temp", "Wind", "Speed", "RPM"];

    [ObservableProperty]
    public partial int KindIndex { get; set; }

    [ObservableProperty]
    public partial double Pressure { get; set; } = 1013;

    [ObservableProperty]
    public partial IReadOnlyList<GaugeRange> PressureRanges { get; set; } = CreatePressureBand(PressureBands[0].Start, PressureBands[0].End);

    [ObservableProperty]
    public partial double PressureLabelInterval { get; set; } = 20;

    [ObservableProperty]
    public partial bool PressureValueDecimal { get; set; }

    [ObservableProperty]
    public partial GaugeUnitPosition PressureUnitPosition { get; set; } = GaugeUnitPosition.Bottom;

    public IReadOnlyList<GaugeColorSwatch> PressureBandSwatches { get; }

    [ObservableProperty]
    public partial double Humidity { get; set; } = 50;

    [ObservableProperty]
    public partial Color HumidityBackgroundColor { get; set; } = HumidityBackgrounds[0];

    [ObservableProperty]
    public partial IReadOnlyList<GaugeRange> HumidityRanges { get; set; } = HumidityBands;

    [ObservableProperty]
    public partial bool HumidityLabelPercent { get; set; } = true;

    [ObservableProperty]
    public partial GaugeUnitPosition HumidityUnitPosition { get; set; } = GaugeUnitPosition.Right;

    public IReadOnlyList<GaugeColorSwatch> HumidityBackgroundSwatches { get; }

    [ObservableProperty]
    public partial double Temperature { get; set; } = 22;

    [ObservableProperty]
    public partial IReadOnlyList<GaugeRange> TemperatureRanges { get; set; } = TemperatureBands;

    [ObservableProperty]
    public partial Color TemperatureArcStartColor { get; set; } = TemperatureArcs[0].Start;

    [ObservableProperty]
    public partial Color TemperatureArcEndColor { get; set; } = TemperatureArcs[0].End;

    [ObservableProperty]
    public partial double TemperatureMinorTickInterval { get; set; } = TemperatureMinorTickIntervals[0];

    [ObservableProperty]
    public partial double TemperatureLabelInterval { get; set; } = 10;

    public IReadOnlyList<GaugeColorSwatch> TemperatureArcSwatches { get; }

    [ObservableProperty]
    public partial double WindDirection { get; set; } = 90;

    [ObservableProperty]
    public partial double WindLabelInterval { get; set; } = WindLabelIntervals[0];

    [ObservableProperty]
    public partial bool WindDegreeLabel { get; set; }

    [ObservableProperty]
    public partial int WindAnimationDuration { get; set; }

    [ObservableProperty]
    public partial bool WindValueVisible { get; set; }

    [ObservableProperty]
    public partial double SpeedKmh { get; set; } = 80;

    [ObservableProperty]
    public partial Easing SpeedAnimationEasing { get; set; } = SpeedEasings[0];

    [ObservableProperty]
    public partial int SpeedAnimationDuration { get; set; } = SpeedDurations[0];

    [ObservableProperty]
    public partial GaugeUnitPosition SpeedUnitPosition { get; set; } = GaugeUnitPosition.Bottom;

    [ObservableProperty]
    public partial double Rpm { get; set; } = 3.5;

    [ObservableProperty]
    public partial bool RpmValueVisible { get; set; }

    [ObservableProperty]
    public partial double RpmStartAngle { get; set; } = -150;

    [ObservableProperty]
    public partial bool RpmWideNeedle { get; set; }

    public IObserveCommand RandomCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIGaugeViewModel()
    {
        PressureBandSwatches = [.. PressureBands.Select(x => GaugeColorSwatch.Gradient(x.Start, x.End, MakeDelegateCommand(() => PressureRanges = CreatePressureBand(x.Start, x.End))))];
        HumidityBackgroundSwatches = [.. HumidityBackgrounds.Select(x => GaugeColorSwatch.Solid(x, MakeDelegateCommand(() => HumidityBackgroundColor = x)))];
        TemperatureArcSwatches = [.. TemperatureArcs.Select(x => GaugeColorSwatch.Gradient(x.Start, x.End, MakeDelegateCommand(() => SelectTemperatureArc(x.Start, x.End))))];
        RandomCommand = MakeDelegateCommand<GaugeKind>(MoveToRandomValue);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        switch ((GaugeKind)KindIndex)
        {
            case GaugeKind.Pressure:
                PressureUnitPosition = ToggleUnitPosition(PressureUnitPosition);
                break;
            case GaugeKind.Humidity:
                HumidityUnitPosition = ToggleUnitPosition(HumidityUnitPosition);
                break;
            case GaugeKind.Temperature:
                TemperatureRanges = TemperatureRanges.Count > 0 ? [] : TemperatureBands;
                break;
            case GaugeKind.Wind:
                windLabelIndex = (windLabelIndex + 1) % WindLabelIntervals.Length;
                WindLabelInterval = WindLabelIntervals[windLabelIndex];
                WindDegreeLabel = windLabelIndex == WindLabelIntervals.Length - 1;
                break;
            case GaugeKind.Speed:
                speedEasingIndex = (speedEasingIndex + 1) % SpeedEasings.Length;
                SpeedAnimationEasing = SpeedEasings[speedEasingIndex];
                break;
            case GaugeKind.Rpm:
                RpmValueVisible = !RpmValueVisible;
                break;
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        switch ((GaugeKind)KindIndex)
        {
            case GaugeKind.Pressure:
                PressureLabelInterval = PressureLabelInterval > 10 ? 10 : 20;
                break;
            case GaugeKind.Humidity:
                HumidityLabelPercent = !HumidityLabelPercent;
                break;
            case GaugeKind.Temperature:
                temperatureMinorTickIndex = (temperatureMinorTickIndex + 1) % TemperatureMinorTickIntervals.Length;
                TemperatureMinorTickInterval = TemperatureMinorTickIntervals[temperatureMinorTickIndex];
                break;
            case GaugeKind.Wind:
                WindAnimationDuration = WindAnimationDuration > 0 ? 0 : 500;
                break;
            case GaugeKind.Speed:
                speedDurationIndex = (speedDurationIndex + 1) % SpeedDurations.Length;
                SpeedAnimationDuration = SpeedDurations[speedDurationIndex];
                break;
            case GaugeKind.Rpm:
                rpmSymmetric = !rpmSymmetric;
                RpmStartAngle = rpmSymmetric ? -135 : -150;
                break;
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        switch ((GaugeKind)KindIndex)
        {
            case GaugeKind.Pressure:
                PressureValueDecimal = !PressureValueDecimal;
                break;
            case GaugeKind.Humidity:
                HumidityRanges = HumidityRanges.Count > 0 ? [] : HumidityBands;
                break;
            case GaugeKind.Temperature:
                TemperatureLabelInterval = TemperatureLabelInterval > 5 ? 5 : 10;
                break;
            case GaugeKind.Wind:
                WindValueVisible = !WindValueVisible;
                break;
            case GaugeKind.Speed:
                SpeedUnitPosition = ToggleUnitPosition(SpeedUnitPosition);
                break;
            case GaugeKind.Rpm:
                RpmWideNeedle = !RpmWideNeedle;
                break;
        }

        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void MoveToRandomValue(GaugeKind kind)
    {
        switch (kind)
        {
            case GaugeKind.Pressure:
                Pressure = NextRandomValue(Pressure, 950, 1050, 0);
                break;
            case GaugeKind.Humidity:
                Humidity = NextRandomValue(Humidity, 0, 100, 0);
                break;
            case GaugeKind.Temperature:
                Temperature = NextRandomValue(Temperature, -10, 40, 1);
                break;
            case GaugeKind.Wind:
                WindDirection = NextRandomValue(WindDirection, 0, 360, 0);
                break;
            case GaugeKind.Speed:
                SpeedKmh = NextRandomValue(SpeedKmh, 0, 200, 0);
                break;
            case GaugeKind.Rpm:
                Rpm = NextRandomValue(Rpm, 0, 8, 2);
                break;
        }
    }

    private void SelectTemperatureArc(Color start, Color end)
    {
        TemperatureArcStartColor = start;
        TemperatureArcEndColor = end;
        TemperatureRanges = [];
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static GaugeRange[] CreatePressureBand(Color start, Color end) =>
        [new() { StartValue = 950, EndValue = 1050, StartColor = start, EndColor = end }];

    private static GaugeUnitPosition ToggleUnitPosition(GaugeUnitPosition position) =>
        position == GaugeUnitPosition.Bottom ? GaugeUnitPosition.Right : GaugeUnitPosition.Bottom;

    private static double NextRandomValue(double current, double minimum, double maximum, int digits)
    {
        var span = maximum - minimum;
        double next;
        do
        {
            next = Math.Round(minimum + (span * RandomNumberGenerator.GetInt32(0, 1001) / 1000), digits);
        }
        while (Math.Abs(next - current) < span / 5);

        return next;
    }
}
