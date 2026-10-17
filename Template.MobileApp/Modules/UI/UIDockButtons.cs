namespace Template.MobileApp.Modules.UI;

using System.Diagnostics;

using Template.MobileApp.Components;

public sealed class CpuDeckButtonInfo : DeckButtonInfo
{
    private readonly DeviceInformation deviceInformation;

    private readonly int processorCount = Environment.ProcessorCount;

    private ProcessStatistics previous;

    public CpuDeckButtonInfo(DeckButton button, byte[] image, DeviceInformation deviceInformation)
        : base(button, image)
    {
        this.deviceInformation = deviceInformation;
        previous = deviceInformation.ReadProcessStatistics();
    }

    public override void Update()
    {
        var statistics = deviceInformation.ReadProcessStatistics();
        var elapsed = Stopwatch.GetElapsedTime(previous.Timestamp, statistics.Timestamp).TotalSeconds;
        if (elapsed > 0)
        {
            var usage = (statistics.CpuTime - previous.CpuTime).TotalSeconds / elapsed * 100 / processorCount;
            previous = statistics;
            Text = String.Join(Environment.NewLine, "CPU", $"{usage:F1}%");
        }
    }
}

public sealed class MemoryDeckButtonInfo : DeckButtonInfo
{
    private const double MegaByte = 1024 * 1024;

    private readonly DeviceInformation deviceInformation;

    public MemoryDeckButtonInfo(DeckButton button, byte[] image, DeviceInformation deviceInformation)
        : base(button, image)
    {
        this.deviceInformation = deviceInformation;
    }

    public override void Update()
    {
        var statistics = deviceInformation.ReadProcessStatistics();
        Text = String.Join(Environment.NewLine, "MEM", $"{statistics.WorkingSet / MegaByte:F0} MB");
    }
}

public sealed class TimerDeckButtonInfo : DeckButtonInfo
{
    private readonly IDialog dialog;

    private int remainSeconds;

    public TimerDeckButtonInfo(DeckButton button, byte[] image, IDialog dialog)
        : base(button, image)
    {
        this.dialog = dialog;
        UpdateLabel();
    }

    public void Toggle()
    {
        remainSeconds = remainSeconds > 0 ? 0 : DeckSample.TimerSeconds;
        UpdateLabel();
    }

    public override void Update()
    {
        if (remainSeconds > 0)
        {
            remainSeconds--;
            UpdateLabel();
            if (remainSeconds == 0)
            {
                _ = NotifyTimeUpAsync();
            }
        }
    }

    private void UpdateLabel() =>
        Label = TimeSpan.FromSeconds(remainSeconds > 0 ? remainSeconds : DeckSample.TimerSeconds).ToString(@"mm\:ss", CultureInfo.InvariantCulture);

    private async Task NotifyTimeUpAsync() => await dialog.InformationAsync("Time's up");
}

public sealed class MuteDeckButtonInfo : DeckButtonInfo
{
    private readonly byte[] muteImage;

    private readonly byte[] mutedImage;

    private bool muted;

    public MuteDeckButtonInfo(DeckButton button, byte[] image, byte[] mutedImage)
        : base(button, image)
    {
        muteImage = image;
        this.mutedImage = mutedImage;
    }

    public void Toggle()
    {
        muted = !muted;
        ImageBytes = muted ? mutedImage : muteImage;
        Label = muted ? "Muted" : "Mute";
    }
}
