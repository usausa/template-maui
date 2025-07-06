namespace Template.MobileApp.Modules.UI;

public sealed class UIDockViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly IScreen screen;

    private readonly IFileSystem fileSystem;

    public UIDockViewModel(
        IDialog dialog,
        IScreen screen,
        IFileSystem fileSystem)
    {
        this.dialog = dialog;
        this.screen = screen;
        this.fileSystem = fileSystem;
    }

    public ObservableCollection<DeckButtonInfo> Buttons { get; } = new();

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            await Navigator.PostActionAsync(InitializeAsync);
        }

        screen.SetFullscreen(true);
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        screen.SetFullscreen(false);
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    private Task InitializeAsync()
    {
        return BusyState.UsingAsync(async () =>
        {
            // Row0 - Row3
            for (var i = 0; i < Colors.Length; i++)
            {
                Buttons.Add(new DeckButtonInfo
                {
                    Row = i / 4,
                    Column = i % 4,
                    ButtonType = DeckButtonType.Text,
                    Label = $"Color{i + 1}",
                    Text = "Color",
                    BackColor1 = Color.FromArgb(Colors[i].Color1),
                    BackColor2 = Color.FromArgb(Colors[i].Color2),
                    Command = MakeAsyncCommand<string>(ExecuteAsync),
                    Parameter = $"Color{i + 1}"
                });
            }

            // Row4
            Buttons.Add(new DeckButtonInfo
            {
                Row = 4,
                Column = 0,
                ButtonType = DeckButtonType.Image,
                Label = "Folder1",
                BackColor1 = Color.FromArgb("#ffb347"),
                BackColor2 = Color.FromArgb("#ffcc33"),
                ImageBytes = await LoadImageAsync("folder.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Folder1"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 4,
                Column = 1,
                ButtonType = DeckButtonType.Image,
                Label = "Folder2",
                BackColor1 = Color.FromArgb("#ffb347"),
                BackColor2 = Color.FromArgb("#ffcc33"),
                ImageBytes = await LoadImageAsync("folder.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Folder2"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 4,
                Column = 2,
                ButtonType = DeckButtonType.Image,
                Label = "Sound1",
                BackColor1 = Color.FromArgb("#fc00ff"),
                BackColor2 = Color.FromArgb("#00dbde"),
                ImageBytes = await LoadImageAsync("music_note.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Sound1"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 4,
                Column = 3,
                ButtonType = DeckButtonType.Image,
                Label = "Sound2",
                BackColor1 = Color.FromArgb("#fc00ff"),
                BackColor2 = Color.FromArgb("#00dbde"),
                ImageBytes = await LoadImageAsync("music_note.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Sound2"
            });

            // Row5
            Buttons.Add(new DeckButtonInfo
            {
                Row = 5,
                Column = 0,
                ButtonType = DeckButtonType.Image,
                Label = "Volume up",
                BackColor1 = Color.FromArgb("#f46b45"),
                BackColor2 = Color.FromArgb("#eea849"),
                ImageBytes = await LoadImageAsync("volume_off.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "VolumeUp"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 5,
                Column = 1,
                ButtonType = DeckButtonType.Image,
                Label = "Mute",
                BackColor1 = Color.FromArgb("#f46b45"),
                BackColor2 = Color.FromArgb("#eea849"),
                ImageBytes = await LoadImageAsync("volume_up.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Mute"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 5,
                Column = 2,
                ButtonType = DeckButtonType.Image,
                Label = "Volume down",
                BackColor1 = Color.FromArgb("#f46b45"),
                BackColor2 = Color.FromArgb("#eea849"),
                ImageBytes = await LoadImageAsync("volume_down.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "VolumeDown"
            });

            // Row6
            Buttons.Add(new DeckButtonInfo
            {
                Row = 6,
                Column = 1,
                ButtonType = DeckButtonType.Image,
                Label = "00:30",
                BackColor1 = Color.FromArgb("#1fa2ff"),
                BackColor2 = Color.FromArgb("#12d8fa"),
                ImageBytes = await LoadImageAsync("timer.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Timer"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 6,
                Column = 2,
                ButtonType = DeckButtonType.Image,
                Label = "Lock",
                BackColor1 = Color.FromArgb("#a770ef"),
                BackColor2 = Color.FromArgb("#cf8bf3"),
                ImageBytes = await LoadImageAsync("lock.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Lock"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 6,
                Column = 3,
                ButtonType = DeckButtonType.Image,
                Label = "Settings",
                BackColor1 = Color.FromArgb("#0cebeb"),
                BackColor2 = Color.FromArgb("#20e3b2"),
                ImageBytes = await LoadImageAsync("settings.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Settings"
            });

            // Row7
            Buttons.Add(new DeckButtonInfo
            {
                Row = 7,
                Column = 0,
                ButtonType = DeckButtonType.Image,
                Label = "Exit",
                BackColor1 = Color.FromArgb("#00d2ff"),
                BackColor2 = Color.FromArgb("#00d2ff"),
                ImageBytes = await LoadImageAsync("exit_to_app.png"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "Exit"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 7,
                Column = 2,
                ButtonType = DeckButtonType.Text,
                Label = "CPU",
                Text = String.Join(Environment.NewLine, "CPU", "13%"),
                BackColor1 = Color.FromArgb("#616161"),
                BackColor2 = Color.FromArgb("#424242"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "VolumeDown"
            });
            Buttons.Add(new DeckButtonInfo
            {
                Row = 7,
                Column = 3,
                ButtonType = DeckButtonType.Text,
                Label = "Memory",
                Text = String.Join(Environment.NewLine, "MEM", "74%"),
                BackColor1 = Color.FromArgb("#616161"),
                BackColor2 = Color.FromArgb("#424242"),
                Command = MakeAsyncCommand<string>(ExecuteAsync),
                Parameter = "VolumeDown"
            });
        });
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
            await Navigator.ForwardAsync(ViewId.UIMenu);
        }
        else
        {
            await dialog.InformationAsync(parameter);
        }
    }

    private static readonly (string Color1, string Color2)[] Colors =
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
}
