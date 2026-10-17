namespace Template.MobileApp.Modules.App;

using GonFox.GameBoy.Platform;

using Template.MobileApp.Models.App;
using Template.MobileApp.Services;

public sealed partial class AppGameBoyViewModel : AppViewModelBase
{
    private const string RomFile = "GameBoy/rpgdemo.gb";

    private const string RomName = "USAGI QUEST";

    // Field

    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(100);

    private readonly Session session;

    private readonly JsonStore store;

    private readonly IDispatcherTimer ticker;

    private GameBoyEmulator? emulator;

    // Property

    [ObservableProperty]
    public partial FrameExchange? Frames { get; set; }

    [ObservableProperty]
    public partial byte Buttons { get; set; }

    [ObservableProperty]
    public partial double Fps { get; set; }

    [ObservableProperty]
    public partial bool IsMuted { get; set; }

    // Command

    public IObserveCommand BackCommand { get; }
    public IObserveCommand MuteCommand { get; }

    public IObserveCommand ButtonCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppGameBoyViewModel(
        IDispatcher dispatcher,
        Session session,
        JsonStore store)
    {
        this.session = session;
        this.store = store;

        ticker = dispatcher.CreateTimer();
        ticker.Interval = TickInterval;
        Disposables.Add(ticker.TickAsObservable().Subscribe(_ => Fps = emulator?.Update() ?? 0));
        Disposables.Add(new DelegateDisposable(ticker.Stop));

        Disposables.Add(session.AsObservable(nameof(Session.IsForeground)).Subscribe(x => _ = UpdateForegroundAsync()));

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        MuteCommand = MakeDelegateCommand(ToggleMute);

        ButtonCommand = MakeDelegateCommand<GameBoyButtonChange>(CommandMode.Simple, ChangeButton);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            emulator?.Dispose();
            emulator = null;
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        IsMuted = store.Load(AppJsonContext.Default.GameBoySnapshot)?.Muted ?? false;

        var current = new GameBoyEmulator();
        emulator = current;
        await current.StartAsync(await LoadRomAsync(), RomName, IsMuted);
        if (emulator == current)
        {
            await UpdateForegroundAsync();
        }
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        StopEmulator();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        emulator?.SetMuted(IsMuted);
        store.Save(new GameBoySnapshot { Muted = IsMuted }, AppJsonContext.Default.GameBoySnapshot);
    }

    private void ChangeButton(GameBoyButtonChange change)
    {
        if (emulator is { } current)
        {
            _ = current.ChangeButtonAsync(change);
            Buttons = current.Buttons;
        }
    }

    private Task UpdateForegroundAsync()
    {
        if (emulator is { } current)
        {
            if (session.IsForeground)
            {
                Frames = current.Frames;
                ticker.Start();
                return current.ResumeAsync();
            }

            Frames = null;
            ticker.Stop();
            Buttons = 0;
            return current.SuspendAsync();
        }
        return Task.CompletedTask;
    }

    private void StopEmulator()
    {
        ticker.Stop();
        Frames = null;
        Buttons = 0;
        Fps = 0;
        emulator?.Dispose();
        emulator = null;
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static async Task<byte[]> LoadRomAsync()
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(RomFile);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }
}
