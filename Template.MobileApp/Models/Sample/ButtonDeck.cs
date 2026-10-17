namespace Template.MobileApp.Models.Sample;

using System.Diagnostics;

using Template.MobileApp.Components;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum DeckButtonType
{
    Text,
    Image
}

public enum DeckAction
{
    Execute,
    OpenFolder,
    CloseFolder,
    ToggleMute,
    ToggleTimer
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class DeckButton
{
    public required int Row { get; init; }

    public required int Column { get; init; }

    public required DeckButtonType Type { get; init; }

    public required string Label { get; init; }

    public string Text { get; init; } = string.Empty;

    public string Image { get; init; } = string.Empty;

    public required Color Color1 { get; init; }

    public required Color Color2 { get; init; }

    public required DeckAction Action { get; init; }

    public required string Parameter { get; init; }
}

public sealed record DeckFolder(
    string Name,
    IReadOnlyList<DeckButton> Buttons);

#pragma warning disable CA1819
public partial class DeckButtonInfo : ObservableObject
{
    public int Row { get; }

    public int Column { get; }

    public DeckButtonType ButtonType { get; }

    [ObservableProperty]
    public partial string Label { get; set; }

    [ObservableProperty]
    public partial string Text { get; set; }

    [ObservableProperty]
    public partial Color TextColor { get; set; } = Colors.White;

    [ObservableProperty]
    public partial Color BackColor1 { get; set; }

    [ObservableProperty]
    public partial Color BackColor2 { get; set; }

    [ObservableProperty]
    public partial byte[] ImageBytes { get; set; }

    public ICommand Command { get; set; } = default!;

    public string Parameter { get; }

    public DeckButtonInfo(DeckButton button, byte[] image)
    {
        Row = button.Row;
        Column = button.Column;
        ButtonType = button.Type;
        Label = button.Label;
        Text = button.Text;
        BackColor1 = button.Color1;
        BackColor2 = button.Color2;
        ImageBytes = image;
        Parameter = button.Parameter;
    }

    public virtual void Update()
    {
    }
}
#pragma warning restore CA1819

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

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class DeckButtonLoader
{
    private readonly IFileSystem fileSystem;

    private readonly IDialog dialog;

    private readonly DeviceInformation deviceInformation;

    public DeckButtonLoader(IFileSystem fileSystem, IDialog dialog, DeviceInformation deviceInformation)
    {
        this.fileSystem = fileSystem;
        this.dialog = dialog;
        this.deviceInformation = deviceInformation;
    }

    public async ValueTask<DeckButtonInfo> LoadAsync(DeckButton button)
    {
        var image = await LoadImageAsync(button.Image);
        return button switch
        {
            { Action: DeckAction.ToggleMute } => new MuteDeckButtonInfo(button, image, await LoadImageAsync(DeckSample.MutedImage)),
            { Action: DeckAction.ToggleTimer } => new TimerDeckButtonInfo(button, image, dialog),
            { Parameter: DeckSample.Cpu } => new CpuDeckButtonInfo(button, image, deviceInformation),
            { Parameter: DeckSample.Memory } => new MemoryDeckButtonInfo(button, image, deviceInformation),
            _ => new DeckButtonInfo(button, image)
        };
    }

    private async ValueTask<byte[]> LoadImageAsync(string image)
    {
        if (!String.IsNullOrEmpty(image))
        {
            await using var stream = await fileSystem.OpenAppPackageFileAsync(Path.Combine("DeckButtons", image));
            return await stream.ReadAllBytesAsync();
        }

        return [];
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class DeckSample
{
    public const int TimerSeconds = 30;

    public const string Mute = "Mute";

    public const string Timer = "Timer";

    public const string Cpu = "Cpu";

    public const string Memory = "Memory";

    public const string MutedImage = "volume_off.png";

    private const string FolderColor1 = "#ffb347";

    private const string FolderColor2 = "#ffcc33";

    private static readonly (string Color1, string Color2)[] Palette =
    [
        new("#F44336", "#FF8A80"),
        new("#E91E63", "#FF80AB"),
        new("#9C27B0", "#EA80FC"),
        new("#673AB7", "#B388FF"),
        new("#3F51B5", "#8C9EFF"),
        new("#2196F3", "#82B1FF"),
        new("#03A9F4", "#80D8FF"),
        new("#00BCD4", "#84FFFF"),
        new("#009688", "#A7FFEB"),
        new("#4CAF50", "#B9F6CA"),
        new("#8BC34A", "#CCFF90"),
        new("#CDDC39", "#F4FF81"),
        new("#FFEB3B", "#FFFF8D"),
        new("#FFC107", "#FFE57F"),
        new("#FF9800", "#FFD180"),
        new("#FF5722", "#FF9E80")
    ];

    public static IReadOnlyList<DeckFolder> LoadFolders() =>
    [
        CreateFolder(
            "Media",
            ("fast_rewind.png", "Rewind"),
            ("play_arrow.png", "Play"),
            ("pause.png", "Pause"),
            ("fast_forward.png", "Forward"),
            ("stop.png", "Stop")),
        CreateFolder(
            "System",
            ("home.png", "Home"),
            ("resume.png", "Resume"),
            ("power_settings_circle.png", "Power"))
    ];

    public static IReadOnlyList<DeckButton> LoadButtons() =>
    [
        // Row0 - Row3
        .. Palette.Select(static (x, i) => TextButton(i / 4, i % 4, $"Color{i + 1}", "Color", x.Color1, x.Color2, $"Color{i + 1}")),

        // Row4
        .. LoadFolders().Select(static (x, i) => ImageButton(4, i, x.Name, "folder.png", FolderColor1, FolderColor2, x.Name, DeckAction.OpenFolder)),
        ImageButton(4, 2, "Sound1", "music_note.png", "#fc00ff", "#00dbde", "Sound1"),
        ImageButton(4, 3, "Sound2", "music_note.png", "#fc00ff", "#00dbde", "Sound2"),

        // Row5
        ImageButton(5, 0, "Volume up", "volume_up.png", "#f46b45", "#eea849", "VolumeUp"),
        ImageButton(5, 1, "Mute", "volume_mute.png", "#f46b45", "#eea849", Mute, DeckAction.ToggleMute),
        ImageButton(5, 2, "Volume down", "volume_down.png", "#f46b45", "#eea849", "VolumeDown"),

        // Row6
        ImageButton(6, 1, "Timer", "timer.png", "#1fa2ff", "#12d8fa", Timer, DeckAction.ToggleTimer),
        ImageButton(6, 2, "Lock", "lock.png", "#a770ef", "#cf8bf3", "Lock"),
        ImageButton(6, 3, "Settings", "settings.png", "#0cebeb", "#20e3b2", "Settings"),

        // Row7
        ImageButton(7, 0, "Exit", "exit_to_app.png", "#00d2ff", "#00d2ff", "Exit"),
        TextButton(7, 2, "CPU", String.Join(Environment.NewLine, "CPU", "-"), "#616161", "#424242", Cpu),
        TextButton(7, 3, "Memory", String.Join(Environment.NewLine, "MEM", "-"), "#616161", "#424242", Memory)
    ];

    private static DeckFolder CreateFolder(string name, params (string Image, string Label)[] items) =>
        new(
            name,
            [
                ImageButton(0, 0, "Back", "arrow_circle_left.png", FolderColor1, FolderColor2, "Back", DeckAction.CloseFolder),
                .. items.Select(static (x, i) => ImageButton(1 + (i / 4), i % 4, x.Label, x.Image, "#1fa2ff", "#12d8fa", x.Label))
            ]);

    private static DeckButton TextButton(int row, int column, string label, string text, string color1, string color2, string parameter) =>
        new()
        {
            Row = row,
            Column = column,
            Type = DeckButtonType.Text,
            Label = label,
            Text = text,
            Color1 = Color.FromArgb(color1),
            Color2 = Color.FromArgb(color2),
            Action = DeckAction.Execute,
            Parameter = parameter
        };

    private static DeckButton ImageButton(
        int row, int column, string label, string image, string color1, string color2, string parameter,
        DeckAction action = DeckAction.Execute) =>
        new()
        {
            Row = row,
            Column = column,
            Type = DeckButtonType.Image,
            Label = label,
            Image = image,
            Color1 = Color.FromArgb(color1),
            Color2 = Color.FromArgb(color2),
            Action = action,
            Parameter = parameter
        };
}
