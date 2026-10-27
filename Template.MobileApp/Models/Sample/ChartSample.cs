namespace Template.MobileApp.Models.Sample;

using ClamChart;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class MonthlySales
{
    public static ChartAccessorCollection<MonthlySales> Accessors { get; } = new()
    {
        { "month", static x => x.Month },
        { "productA", static x => x.ProductA },
        { "productB", static x => x.ProductB },
        { "productC", static x => x.ProductC }
    };

    public required string Month { get; init; }

    public required double ProductA { get; init; }

    public required double ProductB { get; init; }

    public required double ProductC { get; init; }
}

public sealed class ClimateRecord
{
    public static ChartAccessorCollection<ClimateRecord> Accessors { get; } = new()
    {
        { "time", static x => x.Time },
        { "temperature", static x => x.Temperature },
        { "humidity", static x => x.Humidity },
        { "outside", static x => x.Outside }
    };

    public required DateTime Time { get; init; }

    public required double Temperature { get; init; }

    public required double Humidity { get; init; }

    public required double Outside { get; init; }
}

public sealed class AccessCount
{
    public static ChartAccessorCollection<AccessCount> Accessors { get; } = new()
    {
        { "day", static x => x.Day },
        { "hour", static x => x.Hour },
        { "count", static x => x.Count }
    };

    public required string Day { get; init; }

    public required string Hour { get; init; }

    public required double Count { get; init; }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class ChartSample
{
    private static readonly string[] Months = ["1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月"];

    private static readonly string[] Weekdays = ["月", "火", "水", "木", "金", "土", "日"];

    private static readonly string[] Regions = ["東京", "大阪", "名古屋", "福岡", "札幌"];

    private static readonly double[] RegionShares = [34, 24, 17, 14, 11];

    // The numbers are fixed so that the sample looks the same on every run
    public static IReadOnlyList<MonthlySales> LoadSales() =>
    [
        .. Months.Select((x, i) => new MonthlySales
        {
            Month = x,
            ProductA = 42 + (Math.Sin(i * 0.52) * 18) + (i * 1.4),
            ProductB = 30 + (Math.Cos(i * 0.68) * 12) + (i * 0.9),
            ProductC = 18 + (Math.Sin((i * 0.41) + 1.2) * 9)
        })
    ];

    public static IReadOnlyList<ChartEntry> LoadShares() =>
        [.. Regions.Select((x, i) => new ChartEntry(x, RegionShares[i]))];

    public static IReadOnlyList<ChartPoint> LoadScatter() =>
    [
        .. Enumerable.Range(0, 36).Select(static i =>
        {
            var x = 10 + (((i * 37) % 85) * 1.0);
            return new ChartPoint(x, Math.Clamp((x * 0.7) + (Math.Sin(i * 1.7) * 22), 5, 98));
        })
    ];

    public static IReadOnlyList<AccessCount> LoadAccesses() =>
    [
        .. Weekdays.SelectMany((d, r) => Enumerable.Range(0, 12).Select(c => new AccessCount
        {
            Day = d,
            Hour = $"{c * 2}時",
            Count = Math.Round(Math.Max(0, (Math.Sin((r * 0.9) + (c * 0.5)) * 40) + (((r * 7) + (c * 13)) % 29)))
        }))
    ];

    // One year of daily counts for the calendar heatmap, weekends are quiet
    public static IReadOnlyList<ChartTimePoint> LoadContributions(DateTime today)
    {
        var start = today.AddDays(-363);
        var points = new List<ChartTimePoint>(364);
        for (var i = 0; i < 364; i++)
        {
            var date = start.AddDays(i);
            var weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var wave = (Math.Sin(i * 0.11) + 1) * 4;
            var noise = ((i * 17) % 11) * 0.7;
            var value = Math.Round(Math.Max(0, (wave + noise) * (weekend ? 0.3 : 1)));
            points.Add(new ChartTimePoint(date, value));
        }

        return points;
    }

    public static IReadOnlyList<ClimateRecord> LoadClimate(DateTime now)
    {
        var start = now.Date.AddHours(now.Hour).AddHours(-47);
        var records = new List<ClimateRecord>(48);
        for (var i = 0; i < 48; i++)
        {
            var time = start.AddHours(i);
            var phase = (time.Hour - 5) / 24d * 2 * Math.PI;
            var outside = 14 + (Math.Sin(phase) * 7) + (((i * 13) % 7) * 0.2);
            records.Add(new ClimateRecord
            {
                Time = time,
                Temperature = 22 + (Math.Sin(phase) * 2.4) + (((i * 11) % 5) * 0.12),
                Humidity = 52 - (Math.Sin(phase) * 9) + (((i * 7) % 9) * 0.4),
                Outside = outside
            });
        }

        return records;
    }
}
