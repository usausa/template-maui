namespace Template.MobileApp.Modules.UI;

// 全幅のハートカード (VariableSizeWrapPanel の ColumnSpan=2)
public sealed class UIKitDashHero
{
    public string Caption { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public sealed class UIKitDashMetric
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;

    // 入場アニメの段差 (ms)。BindableLayout ではインデックスが取れないためモデル側で持つ
    public int EnterDelay { get; init; }
}

// ハートカード (UIKitDashHero) とメトリクス (UIKitDashMetric) をタイルの種類で振り分ける
public sealed class UIKitDashTileTemplateSelector : DataTemplateSelector
{
    public DataTemplate HeroTemplate { get; set; } = default!;

    public DataTemplate MetricTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        item is UIKitDashHero ? HeroTemplate : MetricTemplate;
}

public sealed class UIKitDashViewModel : AppViewModelBase
{
    public string Greeting { get; } = "おはようございます";
    public string UserName { get; } = "うさうさうさん";

    // 先頭のハートカード + メトリクス 4 件を 1 つのタイルパネルへ流し込む (件数は可変)
    public IReadOnlyList<object> Tiles { get; } =
    [
        new UIKitDashHero { Caption = "平均心拍数", Value = "72 bpm" },
        new UIKitDashMetric { Title = "歩数", Value = "7,852", Unit = "/ 10,000", Icon = Fonts.MaterialIcons.Directions_walk, EnterDelay = 80 },
        new UIKitDashMetric { Title = "心拍数", Value = "72", Unit = "bpm", Icon = Fonts.MaterialIcons.Favorite, EnterDelay = 140 },
        new UIKitDashMetric { Title = "消費カロリー", Value = "412", Unit = "kcal", Icon = Fonts.MaterialIcons.Local_fire_department, EnterDelay = 200 },
        new UIKitDashMetric { Title = "睡眠", Value = "7.4", Unit = "時間", Icon = Fonts.MaterialIcons.Bedtime, EnterDelay = 260 }
    ];

    public IObserveCommand NotifyCommand { get; }

    public IObserveCommand SettingCommand { get; }

    public IObserveCommand OnboardCommand { get; }

    public IObserveCommand TrackingCommand { get; }

    public UIKitDashViewModel()
    {
        NotifyCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitNotify));
        SettingCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitSetting));
        OnboardCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitOnboard));
        TrackingCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.UIKitTracking));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
