namespace Template.MobileApp.State;

using Template.MobileApp.Diagnostics;

public sealed class ApplicationMetrics
{
    public double Value1
    {
        get => Volatile.Read(ref field);
        set => Volatile.Write(ref field, value);
    }

    public double Value2
    {
        get => Volatile.Read(ref field);
        set => Volatile.Write(ref field, value);
    }

    public double Value3
    {
        get => Volatile.Read(ref field);
        set => Volatile.Write(ref field, value);
    }

    public double Value4
    {
        get => Volatile.Read(ref field);
        set => Volatile.Write(ref field, value);
    }
}

public static class ApplicationMetricsExtensions
{
    public static void AddApplicationMetrics(this DiagnosticsInstrumentation instrumentation, ApplicationMetrics metrics) =>
        instrumentation.AddCustomMetrics(
            ("application.custom.value1", () => metrics.Value1),
            ("application.custom.value2", () => metrics.Value2),
            ("application.custom.value3", () => metrics.Value3),
            ("application.custom.value4", () => metrics.Value4));
}
