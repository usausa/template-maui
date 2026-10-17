namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Components;

public sealed class UIDockViewModel : AppViewModelBase
{
    // Field

    private readonly IDialog dialog;

    private readonly IScreen screen;

    private readonly IFileSystem fileSystem;

    private readonly DeviceInformation deviceInformation;

    private readonly IDispatcherTimer timer;

    private readonly List<DeckButtonInfo> rootButtons = [];

    private readonly Dictionary<string, List<DeckButtonInfo>> folderButtons = [];

    private bool folderOpened;

    // Property

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
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => Tick()));
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

            ShowButtons(rootButtons);
        });
    }

    private async ValueTask<DeckButtonInfo> CreateButtonAsync(DeckButton button)
    {
        var image = await LoadImageAsync(button.Image);
        var info = button switch
        {
            { Action: DeckAction.ToggleMute } => new MuteDeckButtonInfo(button, image, await LoadImageAsync(DeckSample.MutedImage)),
            { Action: DeckAction.ToggleTimer } => new TimerDeckButtonInfo(button, image, dialog),
            { Parameter: DeckSample.Cpu } => new CpuDeckButtonInfo(button, image, deviceInformation),
            { Parameter: DeckSample.Memory } => new MemoryDeckButtonInfo(button, image, deviceInformation),
            _ => new DeckButtonInfo(button, image)
        };
        info.Command = CreateCommand(button.Action, info);
        return info;
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

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void Tick()
    {
        foreach (var button in rootButtons.Concat(folderButtons.Values.SelectMany(static x => x)))
        {
            button.Update();
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private ICommand CreateCommand(DeckAction action, DeckButtonInfo button) =>
        (action, button) switch
        {
            (DeckAction.OpenFolder, _) => MakeDelegateCommand<string>(OpenFolder),
            (DeckAction.CloseFolder, _) => MakeDelegateCommand(CloseFolder),
            (DeckAction.ToggleMute, MuteDeckButtonInfo mute) => MakeDelegateCommand(mute.Toggle),
            (DeckAction.ToggleTimer, TimerDeckButtonInfo timer) => MakeDelegateCommand(timer.Toggle),
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
}
