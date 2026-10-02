namespace Template.MobileApp.Models.Sample;

public enum WeatherCondition
{
    Clear,
    PartlyCloudy,
    Cloudy,
    Rain,
    Thunder,
    Snow
}

// UV の段階 (0〜2 / 3〜5 / 6〜7 / 8〜10 / 11〜)
public enum WeatherUvLevel
{
    Low,
    Moderate,
    High,
    VeryHigh,
    Extreme
}

public enum WeatherPressureTrend
{
    Falling,
    Steady,
    Rising
}

public enum WeatherMoonPhase
{
    New,
    WaxingCrescent,
    FirstQuarter,
    WaxingGibbous,
    Full,
    WaningGibbous,
    LastQuarter,
    WaningCrescent
}

public enum WeatherIndexKind
{
    Laundry,
    Umbrella,
    Clothing,
    Heatstroke,
    Ultraviolet,
    Pollen
}

// Level は 1〜5
public sealed record WeatherIndex(WeatherIndexKind Kind, int Level);

public enum WeatherAlertKind
{
    HeavyRain,
    Thunder,
    StrongWind,
    Dry,
    HeavySnow
}

public enum WeatherAlertLevel
{
    Advisory,
    Warning
}

public sealed record WeatherAlert(WeatherAlertKind Kind, WeatherAlertLevel Level);

// 気温は ℃、降水確率は %
public sealed record WeatherHour(DateTime Time, WeatherCondition Condition, bool IsNight, int Temperature, int PrecipitationChance);

public sealed record WeatherDay(DateTime Date, WeatherCondition Condition, int Low, int High, int PrecipitationChance);

// 現在・今から 24 時間・今日から 7 日の予報 (見本のダミー)
public sealed class WeatherForecast
{
    private const int HourCount = 24;

    private const int DayCount = 7;

    // 最低・最高の時刻
    private const double LowHour = 5;

    private const double HighHour = 14;

    // 露点の近似 (Magnus の式) の係数
    private const double MagnusB = 17.62;

    private const double MagnusC = 243.12;

    private const double SynodicMonth = 29.530588853;

    private static readonly DateTime NewMoon = new(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);

    // 月ごとの平均気温と、晴れの日の UV の最大
    private static readonly double[] MonthlyTemperature = [5.4, 6.1, 9.4, 14.3, 18.8, 21.9, 25.7, 26.9, 23.3, 18.0, 12.5, 7.7];

    private static readonly int[] MonthlyUv = [2, 3, 5, 6, 8, 8, 9, 9, 7, 5, 3, 2];

    public required string Location { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required WeatherCondition Condition { get; init; }

    public required bool IsNight { get; init; }

    public required int Temperature { get; init; }

    public required int FeelsLike { get; init; }

    public required int High { get; init; }

    public required int Low { get; init; }

    // %
    public required int Humidity { get; init; }

    // m/s
    public required double WindSpeed { get; init; }

    // 風が吹いてくる向き (度。北 = 0 で時計回り)
    public required int WindDirection { get; init; }

    public required int UvIndex { get; init; }

    public WeatherUvLevel UvLevel => UvIndex switch
    {
        <= 2 => WeatherUvLevel.Low,
        <= 5 => WeatherUvLevel.Moderate,
        <= 7 => WeatherUvLevel.High,
        <= 10 => WeatherUvLevel.VeryHigh,
        _ => WeatherUvLevel.Extreme
    };

    public required int DewPoint { get; init; }

    // hPa
    public required int Pressure { get; init; }

    public required WeatherPressureTrend PressureTrend { get; init; }

    public required DateTime Sunrise { get; init; }

    public required DateTime Sunset { get; init; }

    public TimeSpan DayLength => Sunset - Sunrise;

    // 新月からの日数
    public required double MoonAge { get; init; }

    public WeatherMoonPhase MoonPhase => (WeatherMoonPhase)((int)Math.Floor((MoonAge / SynodicMonth * 8) + 0.5) % 8);

    // 光っている割合 (0〜1)
    public double MoonIllumination => (1 - Math.Cos(2 * Math.PI * MoonAge / SynodicMonth)) / 2;

    public required IReadOnlyList<WeatherIndex> Indices { get; init; }

    public required IReadOnlyList<WeatherAlert> Alerts { get; init; }

    public bool HasAlerts => Alerts.Count > 0;

    public bool HasWarning => Alerts.Any(static x => x.Level == WeatherAlertLevel.Warning);

    public required DateTime AlertIssuedAt { get; init; }

    // 先頭は今
    public required IReadOnlyList<WeatherHour> Hours { get; init; }

    // 先頭は今日
    public required IReadOnlyList<WeatherDay> Days { get; init; }

    //--------------------------------------------------------------------------------
    // Create
    //--------------------------------------------------------------------------------

    // random は 0 以上 n 未満を返す
    public static WeatherForecast Create(DateTime now, Func<int, int> random)
    {
        // 日ごとの天気と平均気温は前の日から少しずつ変える
        var conditions = new WeatherCondition[DayCount];
        var lows = new int[DayCount];
        var highs = new int[DayCount];
        var mean = MonthlyTemperature[now.Month - 1] + ((random(5) - 2) * 0.5);
        var condition = (WeatherCondition)random(4);
        for (var i = 0; i < DayCount; i++)
        {
            if (i > 0)
            {
                mean += (random(7) - 3) * 0.5;
                condition = NextCondition(condition, random);
            }

            var dayCondition = ((mean <= 3) && (condition is WeatherCondition.Rain or WeatherCondition.Thunder)) ? WeatherCondition.Snow : condition;
            var range = TemperatureRange(dayCondition) + (random(3) - 1);
            conditions[i] = dayCondition;
            lows[i] = (int)Math.Round(mean - (range / 2d));
            highs[i] = (int)Math.Round(mean + (range / 2d));
        }

        var hours = new WeatherHour[HourCount];
        var start = now.Date.AddHours(now.Hour);
        for (var i = 0; i < HourCount; i++)
        {
            var time = i == 0 ? now : start.AddHours(i);
            var day = Math.Min((time.Date - now.Date).Days, DayCount - 1);
            var hourCondition = VaryCondition(conditions[day], random);
            hours[i] = new WeatherHour(
                time,
                hourCondition,
                IsNightAt(time),
                (int)Math.Round(TemperatureAt(time, now.Date, lows, highs)),
                PrecipitationChance(hourCondition, random));
        }

        // 今の気温は今日の最低・最高に含める
        var current = hours[0];
        lows[0] = Math.Min(lows[0], current.Temperature);
        highs[0] = Math.Max(highs[0], current.Temperature);

        var days = new WeatherDay[DayCount];
        for (var i = 0; i < DayCount; i++)
        {
            days[i] = new WeatherDay(now.Date.AddDays(i), conditions[i], lows[i], highs[i], PrecipitationChance(conditions[i], random));
        }

        var humidity = BaseHumidity(current.Condition) + random(11);
        var windSpeed = Math.Round(1 + (random(50) / 10d) + (current.Condition is WeatherCondition.Rain or WeatherCondition.Thunder ? 2 : 0), 1);
        var (sunrise, sunset) = SunTimes(now.Date);

        // 今日の残りの時間の降水確率のいちばん高い値と、南中の UV
        var chance = Math.Max(days[0].PrecipitationChance, hours.Where(x => x.Time.Date == now.Date).Max(static x => x.PrecipitationChance));
        var uvMax = CalculateUvIndex(sunrise + ((sunset - sunrise) / 2), conditions[0], sunrise, sunset);

        // 次の日の天気が良くなるなら気圧は上がる
        var trend = ((int)conditions[1]).CompareTo((int)conditions[0]) switch
        {
            < 0 => WeatherPressureTrend.Rising,
            > 0 => WeatherPressureTrend.Falling,
            _ => WeatherPressureTrend.Steady
        };

        return new WeatherForecast
        {
            Location = "東京",
            UpdatedAt = now,
            Condition = current.Condition,
            IsNight = current.IsNight,
            Temperature = current.Temperature,
            FeelsLike = CalculateFeelsLike(current.Temperature, humidity, windSpeed),
            High = highs[0],
            Low = lows[0],
            Humidity = humidity,
            WindSpeed = windSpeed,
            WindDirection = random(360),
            UvIndex = CalculateUvIndex(now, current.Condition, sunrise, sunset),
            DewPoint = CalculateDewPoint(current.Temperature, humidity),
            Pressure = BasePressure(current.Condition) + random(9) - 4,
            PressureTrend = trend,
            Sunrise = sunrise,
            Sunset = sunset,
            MoonAge = CalculateMoonAge(now),
            Hours = hours,
            Days = days,
            Indices = CreateIndices(now, days[0], chance, humidity, windSpeed, uvMax),
            Alerts = CreateAlerts(days[0], chance, humidity, windSpeed),
            AlertIssuedAt = now.Date.AddHours(now.Hour)
        };
    }

    // 隣の天気に移ることがある (雷は雨の日だけ)
    private static WeatherCondition NextCondition(WeatherCondition condition, Func<int, int> random)
    {
        var roll = random(10);
        return condition switch
        {
            WeatherCondition.Clear => roll < 6 ? WeatherCondition.Clear : WeatherCondition.PartlyCloudy,
            WeatherCondition.PartlyCloudy => roll < 3 ? WeatherCondition.Clear : roll < 7 ? WeatherCondition.PartlyCloudy : WeatherCondition.Cloudy,
            WeatherCondition.Cloudy => roll < 3 ? WeatherCondition.PartlyCloudy : roll < 6 ? WeatherCondition.Cloudy : WeatherCondition.Rain,
            _ => roll < 4 ? WeatherCondition.Cloudy : roll < 9 ? WeatherCondition.Rain : WeatherCondition.Thunder
        };
    }

    // 時間ごとは、日の天気から少しだけ変える
    private static WeatherCondition VaryCondition(WeatherCondition condition, Func<int, int> random)
    {
        var roll = random(10);
        return condition switch
        {
            WeatherCondition.Clear => roll < 8 ? WeatherCondition.Clear : WeatherCondition.PartlyCloudy,
            WeatherCondition.PartlyCloudy => roll < 3 ? WeatherCondition.Clear : roll < 8 ? WeatherCondition.PartlyCloudy : WeatherCondition.Cloudy,
            WeatherCondition.Cloudy => roll < 2 ? WeatherCondition.PartlyCloudy : roll < 9 ? WeatherCondition.Cloudy : WeatherCondition.Rain,
            WeatherCondition.Rain => roll < 3 ? WeatherCondition.Cloudy : WeatherCondition.Rain,
            WeatherCondition.Thunder => roll < 4 ? WeatherCondition.Rain : WeatherCondition.Thunder,
            _ => roll < 3 ? WeatherCondition.Cloudy : WeatherCondition.Snow
        };
    }

    // 1 日の気温の幅
    private static int TemperatureRange(WeatherCondition condition) => condition switch
    {
        WeatherCondition.Clear => 9,
        WeatherCondition.PartlyCloudy => 7,
        WeatherCondition.Cloudy => 5,
        WeatherCondition.Thunder => 6,
        _ => 4
    };

    private static int PrecipitationChance(WeatherCondition condition, Func<int, int> random) => condition switch
    {
        WeatherCondition.Clear => random(2) * 10,
        WeatherCondition.PartlyCloudy => 10 + (random(2) * 10),
        WeatherCondition.Cloudy => 20 + (random(3) * 10),
        _ => 60 + (random(4) * 10)
    };

    private static int BaseHumidity(WeatherCondition condition) => condition switch
    {
        WeatherCondition.Clear => 45,
        WeatherCondition.PartlyCloudy => 50,
        WeatherCondition.Cloudy => 60,
        _ => 80
    };

    private static int BasePressure(WeatherCondition condition) => condition switch
    {
        WeatherCondition.Clear => 1020,
        WeatherCondition.PartlyCloudy => 1015,
        WeatherCondition.Cloudy => 1010,
        WeatherCondition.Rain => 1003,
        WeatherCondition.Thunder => 998,
        _ => 1005
    };

    // 最低 (5 時) と最高 (14 時) の間をなめらかにつなぐ。day は today からの日数
    private static double TemperatureAt(DateTime time, DateTime today, int[] lows, int[] highs)
    {
        var day = Math.Min((time.Date - today).Days, lows.Length - 1);
        var hour = time.TimeOfDay.TotalHours;
        if (hour < LowHour)
        {
            // 前の日の最高から今日の最低へ
            var previousHigh = highs[Math.Max(day - 1, 0)];
            return Ease(previousHigh, lows[day], (hour + 24 - HighHour) / (24 - HighHour + LowHour));
        }

        if (hour < HighHour)
        {
            return Ease(lows[day], highs[day], (hour - LowHour) / (HighHour - LowHour));
        }

        // 今日の最高から次の日の最低へ
        var nextLow = lows[Math.Min(day + 1, lows.Length - 1)];
        return Ease(highs[day], nextLow, (hour - HighHour) / (24 - HighHour + LowHour));
    }

    private static double Ease(double from, double to, double t) =>
        from + ((to - from) * (1 - Math.Cos(Math.PI * t)) / 2);

    // 東京に近い値 (昼の長さは 1 年の正弦、南中は 11:35)
    private static (DateTime Sunrise, DateTime Sunset) SunTimes(DateTime date)
    {
        var length = 12.2 + (2.4 * Math.Sin(2 * Math.PI * (date.DayOfYear - 80) / 365.25));
        var noon = date.AddHours(11 + (35 / 60d));
        return (noon.AddHours(-length / 2), noon.AddHours(length / 2));
    }

    private static bool IsNightAt(DateTime time)
    {
        var (sunrise, sunset) = SunTimes(time.Date);
        return (time < sunrise) || (time >= sunset);
    }

    // 風が強いと低く、蒸し暑いと高く感じる
    private static int CalculateFeelsLike(int temperature, int humidity, double windSpeed)
    {
        var value = temperature - (Math.Max(windSpeed - 2, 0) * 0.6);
        if ((temperature >= 25) && (humidity >= 70))
        {
            value += 2;
        }

        return (int)Math.Round(value);
    }

    private static int CalculateDewPoint(int temperature, int humidity)
    {
        var gamma = Math.Log(humidity / 100d) + (MagnusB * temperature / (MagnusC + temperature));
        return (int)Math.Round(MagnusC * gamma / (MagnusB - gamma));
    }

    private static double CalculateMoonAge(DateTime now)
    {
        var days = (now.ToUniversalTime() - NewMoon).TotalDays;
        return ((days % SynodicMonth) + SynodicMonth) % SynodicMonth;
    }

    // 洗濯・傘・服装・熱中症・紫外線・花粉 (値が大きいほど、よく乾く・必要・厚着・危険・強い・多い)
    private static WeatherIndex[] CreateIndices(DateTime now, WeatherDay today, int chance, int humidity, double windSpeed, int uvMax)
    {
        var laundry = today.Condition switch
        {
            WeatherCondition.Clear => 5,
            WeatherCondition.PartlyCloudy => 4,
            WeatherCondition.Cloudy => 3,
            _ => 1
        };
        laundry -= (humidity >= 75 ? 1 : 0) + (chance >= 50 ? 1 : 0);
        laundry += (windSpeed >= 4) && (laundry is > 1 and < 5) ? 1 : 0;

        // 暑さ指数 (WBGT) の近似
        var wbgt = (0.725 * today.High) + (0.0368 * humidity) + (0.00364 * today.High * humidity) - 3.246;

        var pollen = now.Month switch
        {
            2 or 3 or 4 => 4,
            5 or 9 or 10 => 2,
            _ => 1
        };
        pollen += today.Condition switch
        {
            WeatherCondition.Clear when windSpeed >= 4 => 1,
            WeatherCondition.Clear or WeatherCondition.PartlyCloudy => 0,
            WeatherCondition.Cloudy => -1,
            _ => -2
        };

        return
        [
            new(WeatherIndexKind.Laundry, Math.Clamp(laundry, 1, 5)),
            new(WeatherIndexKind.Umbrella, chance switch
            {
                <= 10 => 1,
                <= 30 => 2,
                <= 50 => 3,
                <= 70 => 4,
                _ => 5
            }),
            new(WeatherIndexKind.Clothing, today.High switch
            {
                >= 28 => 1,
                >= 23 => 2,
                >= 18 => 3,
                >= 12 => 4,
                _ => 5
            }),
            new(WeatherIndexKind.Heatstroke, wbgt switch
            {
                < 21 => 1,
                < 25 => 2,
                < 28 => 3,
                < 31 => 4,
                _ => 5
            }),
            new(WeatherIndexKind.Ultraviolet, uvMax switch
            {
                <= 2 => 1,
                <= 5 => 2,
                <= 7 => 3,
                <= 10 => 4,
                _ => 5
            }),
            new(WeatherIndexKind.Pollen, Math.Clamp(pollen, 1, 5))
        ];
    }

    private static List<WeatherAlert> CreateAlerts(WeatherDay today, int chance, int humidity, double windSpeed)
    {
        var alerts = new List<WeatherAlert>();
        if ((today.Condition is WeatherCondition.Rain or WeatherCondition.Thunder) && (chance >= 70))
        {
            alerts.Add(new WeatherAlert(WeatherAlertKind.HeavyRain, chance >= 90 ? WeatherAlertLevel.Warning : WeatherAlertLevel.Advisory));
        }

        if (today.Condition == WeatherCondition.Thunder)
        {
            alerts.Add(new WeatherAlert(WeatherAlertKind.Thunder, WeatherAlertLevel.Advisory));
        }

        if ((today.Condition == WeatherCondition.Snow) && (chance >= 70))
        {
            alerts.Add(new WeatherAlert(WeatherAlertKind.HeavySnow, WeatherAlertLevel.Advisory));
        }

        if (windSpeed >= 7)
        {
            alerts.Add(new WeatherAlert(WeatherAlertKind.StrongWind, WeatherAlertLevel.Advisory));
        }

        if ((today.Condition is WeatherCondition.Clear or WeatherCondition.PartlyCloudy) && (humidity < 55))
        {
            alerts.Add(new WeatherAlert(WeatherAlertKind.Dry, WeatherAlertLevel.Advisory));
        }

        return alerts;
    }

    // 昼の高さに合わせる (夜は 0)
    private static int CalculateUvIndex(DateTime now, WeatherCondition condition, DateTime sunrise, DateTime sunset)
    {
        var ratio = (now - sunrise).TotalHours / (sunset - sunrise).TotalHours;
        var factor = condition switch
        {
            WeatherCondition.Clear => 1.0,
            WeatherCondition.PartlyCloudy => 0.8,
            WeatherCondition.Cloudy => 0.5,
            _ => 0.3
        };
        return ratio is > 0 and < 1 ? (int)Math.Round(MonthlyUv[now.Month - 1] * factor * Math.Sin(Math.PI * ratio)) : 0;
    }
}
