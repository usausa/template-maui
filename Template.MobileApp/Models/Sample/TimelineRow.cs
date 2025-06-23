namespace Template.MobileApp.Models.Sample;

#pragma warning disable CA1819
public sealed class TimelineRow
{
    public int No { get; set; }

    public int[] LineNos { get; set; } = default!;

    public int? In { get; set; }

    public int? Out { get; set; }

    public string Id { get; set; } = default!;

    public string Text { get; set; } = default!;
}
#pragma warning restore CA1819
