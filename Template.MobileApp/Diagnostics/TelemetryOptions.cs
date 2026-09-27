namespace Template.MobileApp.Diagnostics;

public sealed class TelemetryOptions
{
    public int TraceResendCapacity { get; set; } = 120;

    public int MetricResendCapacity { get; set; } = 120;

    public int LogResendCapacity { get; set; } = 120;
}
