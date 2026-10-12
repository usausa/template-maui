namespace Template.MobileApp.Modules.UI;

using System.Diagnostics;

using Template.MobileApp.Components;

public sealed class UIDockViewModel : AppViewModelBase
{
    private const double MegaByte = 1024 * 1024;

    private readonly IDialog dialog;

    private readonly IScreen screen;

    private readonly IFileSystem fileSystem;

    private readonly DeviceInformation deviceInformation;

    private readonly IDispatcherTimer timer;

    private readonly int processorCount = Environment.ProcessorCount;

    private readonly List<DeckButtonInfo> rootButtons = [];

    private readonly Dictionary<string, List<DeckButtonInfo>> folderButtons = [];

    private bool folderOpened;

    private ProcessStatistics previous;

    private DeckButtonInfo? cpuButton;

    private DeckButtonInfo? memButton;

    private DeckButtonInfo? timerButton;

    private DeckButtonInfo? muteButton;

    private byte[] muteImage = [];

    private byte[] mutedImage = [];

    private bool muted;

    private int remainSeconds;

    public ObservableCollection<DeckButtonInfo> Buttons { get; } = [];

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIDockViewModel(
        IDialog dialog,
        IScreen screen,
        IFileSystem fileSystem,
        IDispatcher dispatcher,
        DeviceInformation deviceInformation)
    {
        this.dialog = dialog;
        this.screen = screen;
        this.fileSystem = fileSystem;
        this.deviceInformation = deviceInformation;

        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => OnTimerTick()));
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await Navigator.PostActionAsync(InitializeAsync);
        }

        screen.SetFullscreen(true);
        previous = deviceInformation.ReadProcessStatistics();
        timer.Start();
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        screen.SetFullscreen(false);
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync()
    {
        if (folderOpened)
        {
            CloseFolder();
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.UIMenu1);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private Task InitializeAsync()
    {
        return BusyState.UsingAsync(async () =>
        {
            foreach (var button in DeckSample.LoadButtons())
            {
                rootButtons.Add(await CreateButtonAsync(button));
            }

            foreach (var folder in DeckSample.LoadFolders())
            {
                var buttons = new List<DeckButtonInfo>();
                foreach (var button in folder.Buttons)
                {
                    buttons.Add(await CreateButtonAsync(button));
                }

                folderButtons[folder.Name] = buttons;
            }

            muteButton = rootButtons.First(static x => x.Parameter == DeckSample.Mute);
            muteImage = muteButton.ImageBytes;
            mutedImage = await LoadImageAsync(DeckSample.MutedImage);
            timerButton = rootButtons.First(static x => x.Parameter == DeckSample.Timer);
            timerButton.Label = FormatTimer(DeckSample.TimerSeconds);
            cpuButton = rootButtons.First(static x => x.Parameter == DeckSample.Cpu);
            memButton = rootButtons.First(static x => x.Parameter == DeckSample.Memory);

            ShowButtons(rootButtons);
        });
    }

    private async ValueTask<DeckButtonInfo> CreateButtonAsync(DeckButton button) =>
        new()
        {
            Row = button.Row,
            Column = button.Column,
            ButtonType = button.Type,
            Label = button.Label,
            Text = button.Text,
            BackColor1 = button.Color1,
            BackColor2 = button.Color2,
            ImageBytes = await LoadImageAsync(button.Image),
            Command = CommandOf(button.Action),
            Parameter = button.Parameter
        };

    private async ValueTask<byte[]> LoadImageAsync(string image)
    {
        if (!String.IsNullOrEmpty(image))
        {
            await using var stream = await fileSystem.OpenAppPackageFileAsync(Path.Combine("DeckButtons", image));
            return await stream.ReadAllBytesAsync();
        }

        return [];
    }

    private async Task ExecuteAsync(string parameter)
    {
        if (parameter == "Exit")
        {
            await Navigator.ForwardAsync(ViewId.UIMenu1);
        }
        else
        {
            await dialog.InformationAsync(parameter);
        }
    }

    private void OpenFolder(string name)
    {
        if (folderButtons.TryGetValue(name, out var buttons))
        {
            folderOpened = true;
            ShowButtons(buttons);
        }
    }

    private void CloseFolder()
    {
        folderOpened = false;
        ShowButtons(rootButtons);
    }

    private void ToggleMute()
    {
        muted = !muted;
        if (muteButton is not null)
        {
            muteButton.ImageBytes = muted ? mutedImage : muteImage;
            muteButton.Label = muted ? "Muted" : "Mute";
        }
    }

    private async Task NotifyTimeUpAsync() => await dialog.InformationAsync("Time's up");

    private void ToggleTimer()
    {
        remainSeconds = remainSeconds > 0 ? 0 : DeckSample.TimerSeconds;
        timerButton?.Label = FormatTimer(remainSeconds > 0 ? remainSeconds : DeckSample.TimerSeconds);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void OnTimerTick()
    {
        UpdateUsage();

        if ((remainSeconds > 0) && (timerButton is not null))
        {
            remainSeconds--;
            timerButton.Label = FormatTimer(remainSeconds > 0 ? remainSeconds : DeckSample.TimerSeconds);
            if (remainSeconds == 0)
            {
                _ = NotifyTimeUpAsync();
            }
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private ICommand CommandOf(DeckAction action) =>
        action switch
        {
            DeckAction.OpenFolder => MakeDelegateCommand<string>(OpenFolder),
            DeckAction.CloseFolder => MakeDelegateCommand(CloseFolder),
            DeckAction.ToggleMute => MakeDelegateCommand(ToggleMute),
            DeckAction.ToggleTimer => MakeDelegateCommand(ToggleTimer),
            _ => MakeAsyncCommand<string>(ExecuteAsync)
        };

    private void ShowButtons(List<DeckButtonInfo> buttons)
    {
        Buttons.Clear();
        foreach (var button in buttons)
        {
            Buttons.Add(button);
        }
    }

    // 診断のパネル(DiagnosticSampler)と同じ取り方。CPU は前回からの CPU 時間の増分、メモリは WorkingSet
    private void UpdateUsage()
    {
        var statistics = deviceInformation.ReadProcessStatistics();
        var elapsed = Stopwatch.GetElapsedTime(previous.Timestamp, statistics.Timestamp).TotalSeconds;
        if ((elapsed > 0) && (cpuButton is not null) && (memButton is not null))
        {
            var cpuUsage = (statistics.CpuTime - previous.CpuTime).TotalSeconds / elapsed * 100 / processorCount;
            previous = statistics;
            cpuButton.Text = String.Join(Environment.NewLine, "CPU", $"{cpuUsage:F1}%");
            memButton.Text = String.Join(Environment.NewLine, "MEM", $"{statistics.WorkingSet / MegaByte:F0} MB");
        }
    }

    private static string FormatTimer(int seconds) => TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss", CultureInfo.InvariantCulture);
}
