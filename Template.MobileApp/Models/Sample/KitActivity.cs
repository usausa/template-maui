namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum KitPeriod
{
    Day,
    Week,
    Month
}

public enum KitOrderState
{
    Accepted,
    Packed,
    Shipped,
    Delivering,
    Delivered
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record KitSeries(
    IReadOnlyList<double> Values,
    IReadOnlyList<string> Labels);

public sealed class KitActivity
{
    private const int StateCount = 5;

    public required DateTime Today { get; init; }

    public required string OrderNumber { get; init; }

    public required DateTime OrderEta { get; init; }

    public required KitOrderState OrderState { get; init; }

    public required int OrderCount { get; init; }

    public required int Spend { get; init; }

    public required int CouponCount { get; init; }

    public required int Points { get; init; }

    public double OrderRate => ((int)OrderState + 1) / (double)StateCount;
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class KitSample
{
    private const int MonthDays = 30;

    private static readonly int[] HourSpend = [0, 0, 0, 0, 0, 0, 0, 480, 1240, 0, 0, 920, 1680, 0, 0, 0, 640, 2180, 3240, 1120, 860, 0, 0, 0];

    private static readonly int[] HourOrder = [0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 2, 0, 0, 0, 1, 2, 3, 1, 1, 0, 0, 0];

    public static KitActivity LoadActivity(DateTime now)
    {
        var slot = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute >= 30 ? 30 : 0, 0);
        return new KitActivity
        {
            Today = now.Date,
            OrderNumber = "#A1284",
            OrderEta = slot.AddMinutes(60),
            OrderState = KitOrderState.Delivering,
            OrderCount = 14,
            Spend = 42_680,
            CouponCount = 3,
            Points = 1_234
        };
    }

    public static KitSeries LoadSpendSeries(KitActivity activity, KitPeriod period)
    {
        if (period == KitPeriod.Day)
        {
            return new KitSeries(
                HourSpend.Select(static x => (double)x).ToList(),
                Enumerable.Range(0, 24).Select(static x => $"{x}時").ToList());
        }

        return DailySeries(activity.Today, period, x => SpendOf(activity, x));
    }

    public static KitSeries LoadOrderSeries(KitActivity activity, KitPeriod period)
    {
        if (period == KitPeriod.Day)
        {
            return new KitSeries(
                HourOrder.Select(static x => (double)x).ToList(),
                Enumerable.Range(0, 24).Select(static x => $"{x}時").ToList());
        }

        return DailySeries(activity.Today, period, x => OrderOf(activity, x));
    }

    private static KitSeries DailySeries(DateTime today, KitPeriod period, Func<DateTime, int> valueOf)
    {
        var count = period == KitPeriod.Week ? 7 : MonthDays;
        var format = period == KitPeriod.Week ? "ddd" : "M/d";
        var days = Enumerable.Range(0, count).Select(x => today.AddDays(x - count + 1)).ToList();
        return new KitSeries(
            days.Select(x => (double)valueOf(x)).ToList(),
            days.Select(x => x.ToString(format, CultureInfo.CurrentCulture)).ToList());
    }

    private static int SpendOf(KitActivity activity, DateTime date) =>
        date == activity.Today
            ? activity.Spend / 8
            : 3400 + (int)((1500 * Math.Sin(date.DayOfYear * 1.37)) + (900 * Math.Sin(date.DayOfYear * 0.53)));

    private static int OrderOf(KitActivity activity, DateTime date) =>
        date == activity.Today
            ? 2
            : 3 + (int)Math.Round(1.8 * Math.Sin(date.DayOfYear * 1.13));
}
