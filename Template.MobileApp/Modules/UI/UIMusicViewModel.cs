namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public sealed partial class UIMusicLine : ObservableObject
{
    public required string Text { get; init; }

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }
}

public sealed partial class UIMusicViewModel : AppViewModelBase
{
    // Field

    private const double SaturationFloor = 0.25;

    private const double BrightnessFloor = 0.15;

    private const double BrightnessCeiling = 0.95;

    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    private readonly IDispatcherTimer timer;

    private int lineIndex = -1;

    // Property

    public MusicPlayer Player { get; } = MusicSample.LoadPlayer();

    public WaveformDrawing Waveform { get; } = new();

    [ObservableProperty]
    public partial ImageSource? Artwork { get; set; }

    [ObservableProperty]
    public partial Color Tone { get; set; } = Color.FromArgb("#1A1038");

    [ObservableProperty]
    public partial IReadOnlyList<UIMusicLine> Lines { get; set; } = [];

    public ScrollController Scroller { get; } = new();

    [ObservableProperty]
    public partial bool ShowLyrics { get; set; } = true;

    // Command

    public IObserveCommand PlayCommand { get; }

    public IObserveCommand PreviousCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand ShuffleCommand { get; }

    public IObserveCommand RepeatCommand { get; }

    public IObserveCommand FavoriteCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIMusicViewModel(IDispatcher dispatcher)
    {
        Waveform.OnSeek = OnSeek;

        PlayCommand = MakeDelegateCommand(() => Player.IsPlaying = !Player.IsPlaying);
        PreviousCommand = MakeDelegateCommand(() => Move(-1));
        NextCommand = MakeDelegateCommand(() => Move(1));
        ShuffleCommand = MakeDelegateCommand(() => Player.IsShuffle = !Player.IsShuffle);
        RepeatCommand = MakeDelegateCommand(() => Player.IsRepeat = !Player.IsRepeat);
        FavoriteCommand = MakeDelegateCommand(() => Player.IsFavorite = !Player.IsFavorite);

        timer = dispatcher.CreateTimer();
        timer.Interval = Interval;
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => Tick()));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await LoadTrackAsync();
        }

        timer.Start();
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        ShowLyrics = !ShowLyrics;
        return Task.CompletedTask;
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Move(int offset)
    {
        Player.Move(offset);
        _ = LoadTrackAsync();
    }

    private void Tick()
    {
        if (Player.IsPlaying)
        {
            var index = Player.Index;
            Player.Advance(Interval);
            if (index != Player.Index)
            {
                _ = LoadTrackAsync();
                return;
            }
        }

        Waveform.UpdateProgress(Player.Progress);
        UpdateLine();
    }

    private void OnSeek(double ratio)
    {
        Player.Seek(ratio);
        Waveform.UpdateProgress(Player.Progress);
        UpdateLine();
    }

    private async Task LoadTrackAsync()
    {
        var track = Player.Track;
        Lines = [.. track.Lyrics.Select(x => new UIMusicLine { Text = x.Text })];
        lineIndex = -1;
        Waveform.Update(track.Waveform);
        Waveform.UpdateProgress(Player.Progress);
        UpdateLine();

        var bytes = await ReadArtworkAsync(track.Artwork);
        Artwork = ImageSource.FromStream(() => new MemoryStream(bytes));
        Tone = ToneOf(bytes);
    }

    private void UpdateLine()
    {
        var index = Player.LineIndex;
        if ((index == lineIndex) || (index < 0) || (index >= Lines.Count))
        {
            return;
        }

        if ((lineIndex >= 0) && (lineIndex < Lines.Count))
        {
            Lines[lineIndex].IsCurrent = false;
        }

        lineIndex = index;
        Lines[index].IsCurrent = true;

        Scroller.ScrollTo(index);
    }

    private static async Task<byte[]> ReadArtworkAsync(string name)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(name).ConfigureAwait(true);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory).ConfigureAwait(true);
        return memory.ToArray();
    }

    private static Color ToneOf(byte[] bytes)
    {
        using var bitmap = SKBitmap.Decode(bytes);
        using var extractor = new ColorExtractor();
        var colors = extractor.Extract(bitmap, 8);
        var vivid = colors
            .Where(static x => (Saturation(x) >= SaturationFloor) && (Brightness(x) is > BrightnessFloor and < BrightnessCeiling))
            .OrderByDescending(static x => x.Count)
            .FirstOrDefault();
        var dominant = vivid.Count > 0 ? vivid : colors.OrderByDescending(static x => x.Count).First();
        return new Color(dominant.R / 255f, dominant.G / 255f, dominant.B / 255f);
    }

    private static double Brightness(ColorCount value) =>
        Math.Max(value.R, Math.Max(value.G, value.B)) / 255d;

    private static double Saturation(ColorCount value)
    {
        var max = Math.Max(value.R, Math.Max(value.G, value.B));
        var min = Math.Min(value.R, Math.Min(value.G, value.B));
        return max == 0 ? 0 : (double)(max - min) / max;
    }
}
