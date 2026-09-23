namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Models.Sample;

// CollectionView による訪問先一覧。行タップで選択、右端で展開、並替パネルで複数キーのソート
public sealed partial class UIVisitViewModel : AppViewModelBase
{
    private const int VisitCount = 40;

    private const int MaxSortKeys = 3;

    private readonly IDialog dialog;

    private readonly List<VisitCard> source = [];

    // 並替パネルで選んだ順 (先頭が第 1 キー)
    private readonly List<VisitSortKey> activeKeys = [];

    public ObservableCollection<VisitCard> Cards { get; } = [];

    public IReadOnlyList<VisitSortKey> SortKeys { get; } =
    [
        new("予定", static (x, y) => x.ScheduledAt.CompareTo(y.ScheduledAt)),
        new("名前", static (x, y) => StringComparer.CurrentCulture.Compare(x.Name, y.Name)),
        new("状態", static (x, y) => x.Status.CompareTo(y.Status)),
        new("担当", static (x, y) => StringComparer.CurrentCulture.Compare(x.Staff, y.Staff)),
        new("区分", static (x, y) => x.Category.CompareTo(y.Category)),
        new("コード", static (x, y) => String.CompareOrdinal(x.Code, y.Code))
    ];

    [ObservableProperty]
    public partial bool IsSortPanelOpen { get; set; }

    [ObservableProperty]
    public partial bool IsAllExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsDescending { get; set; }

    [ObservableProperty]
    public partial int PlannedCount { get; set; }

    [ObservableProperty]
    public partial int VisitedCount { get; set; }

    [ObservableProperty]
    public partial int RevisitCount { get; set; }

    [ObservableProperty]
    public partial int AbsentCount { get; set; }

    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    [ObservableProperty]
    public partial string SortText { get; set; } = string.Empty;

    public IObserveCommand ToggleSelectCommand { get; }
    public IObserveCommand ToggleExpandCommand { get; }
    public IObserveCommand ToggleSortPanelCommand { get; }

    public IObserveCommand SelectSortKeyCommand { get; }

    public IObserveCommand ToggleDirectionCommand { get; }

    public IObserveCommand ClearSortCommand { get; }

    public IObserveCommand ToggleExpandAllCommand { get; }

    public IObserveCommand ReloadCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIVisitViewModel(IDialog dialog)
    {
        this.dialog = dialog;

        ToggleSelectCommand = MakeDelegateCommand<VisitCard>(x =>
        {
            x.IsSelected = !x.IsSelected;
            UpdateSelectedCount();
        });
        ToggleExpandCommand = MakeDelegateCommand<VisitCard>(x => x.IsExpanded = !x.IsExpanded);
        ToggleSortPanelCommand = MakeDelegateCommand(() => IsSortPanelOpen = !IsSortPanelOpen);
        SelectSortKeyCommand = MakeDelegateCommand<VisitSortKey>(SelectSortKey);
        ToggleDirectionCommand = MakeDelegateCommand(ToggleDirection);
        ClearSortCommand = MakeDelegateCommand(ClearSort);
        ToggleExpandAllCommand = MakeDelegateCommand(ToggleExpandAll);
        ReloadCommand = MakeDelegateCommand(Load);

        activeKeys.Add(SortKeys[0]);
        UpdateSortKeys();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Load();
        }

        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        IsSortPanelOpen = !IsSortPanelOpen;
        return Task.CompletedTask;
    }

    // 未訪問を一括選択 (すべて選択済みなら解除)
    protected override Task OnNotifyFunction3()
    {
        var planned = source.Where(static x => x.Status == VisitStatus.Planned).ToList();
        var select = planned.Any(static x => !x.IsSelected);
        foreach (var card in planned)
        {
            card.IsSelected = select;
        }

        UpdateSelectedCount();
        return Task.CompletedTask;
    }

    protected override async Task OnNotifyFunction4()
    {
        var selected = source.Where(static x => x.IsSelected).ToList();
        if (selected.Count == 0)
        {
            await dialog.InformationAsync("訪問先が選択されていません。");
            return;
        }

        var names = String.Join("\n", selected.Take(5).Select(static x => $"・{x.Code} {x.Name}"));
        var rest = selected.Count > 5 ? $"\n…他 {selected.Count - 5} 件" : string.Empty;
        await dialog.InformationAsync($"{selected.Count} 件を確定します。\n{names}{rest}");
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Load()
    {
        source.Clear();
        source.AddRange(VisitSamples.Create(VisitCount).Select(static x => new VisitCard(x)));
        PlannedCount = source.Count(static x => x.Status == VisitStatus.Planned);
        VisitedCount = source.Count(static x => x.Status == VisitStatus.Visited);
        RevisitCount = source.Count(static x => x.Status == VisitStatus.Revisit);
        AbsentCount = source.Count(static x => x.Status == VisitStatus.Absent);
        IsAllExpanded = false;
        UpdateSelectedCount();
        ApplySort();
    }

    // 第 1 キーの再選択は昇降の反転、それ以外は先頭に積む (最大 3 キー)
    private void SelectSortKey(VisitSortKey key)
    {
        if ((activeKeys.Count > 0) && ReferenceEquals(activeKeys[0], key))
        {
            key.Descending = !key.Descending;
        }
        else
        {
            activeKeys.Remove(key);
            activeKeys.Insert(0, key);
            while (activeKeys.Count > MaxSortKeys)
            {
                activeKeys.RemoveAt(activeKeys.Count - 1);
            }
        }

        UpdateSortKeys();
        ApplySort();
    }

    private void ToggleDirection()
    {
        if (activeKeys.Count == 0)
        {
            return;
        }

        activeKeys[0].Descending = !activeKeys[0].Descending;
        UpdateSortKeys();
        ApplySort();
    }

    // 並べ替えを解除する (コード順)
    private void ClearSort()
    {
        activeKeys.Clear();
        foreach (var key in SortKeys)
        {
            key.Descending = false;
        }

        UpdateSortKeys();
        ApplySort();
    }

    private void ToggleExpandAll()
    {
        IsAllExpanded = !IsAllExpanded;
        foreach (var card in source)
        {
            card.IsExpanded = IsAllExpanded;
        }
    }

    private void UpdateSortKeys()
    {
        foreach (var key in SortKeys)
        {
            var index = activeKeys.IndexOf(key);
            key.Priority = index + 1;
            key.IsActive = index >= 0;
        }

        IsDescending = (activeKeys.Count > 0) && activeKeys[0].Descending;
        SortText = activeKeys.Count > 0 ? String.Join(" › ", activeKeys.Select(static x => x.Name + (x.Descending ? " ↓" : " ↑"))) : "なし";
    }

    // 選択・展開の状態はカードが持つため、並べ替えでは順番だけを差し替える
    private void ApplySort()
    {
        var comparer = Comparer<VisitCard>.Create((x, y) =>
        {
            foreach (var key in activeKeys)
            {
                var result = key.Compare(x.Info, y.Info);
                if (result != 0)
                {
                    return key.Descending ? -result : result;
                }
            }

            return String.CompareOrdinal(x.Code, y.Code);
        });

        Cards.Clear();
        foreach (var card in source.OrderBy(static x => x, comparer))
        {
            Cards.Add(card);
        }
    }

    private void UpdateSelectedCount() => SelectedCount = source.Count(static x => x.IsSelected);
}

// 文言と色は XAML 側のコンバーター (MapToText / MapToColor) で決める
public sealed partial class VisitCard : ObservableObject
{
    public VisitInfo Info { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    public string Code => Info.Code;

    public string Name => Info.Name;

    public string Address => Info.Address;

    public string Staff => Info.Staff;

    public VisitCategory Category => Info.Category;

    public string Phone => Info.Phone;

    public string Note => Info.Note;

    public bool IsPriority => Info.IsPriority;

    public bool IsToday => Info.ScheduledAt.Date == DateTime.Today;

    public bool IsFirstVisit => Info.LastVisitedAt is null;

    public VisitStatus Status => Info.Status;

    public DateTime ScheduledAt => Info.ScheduledAt;

    public DateTime? LastVisitedAt => Info.LastVisitedAt;

    public VisitCard(VisitInfo info)
    {
        Info = info;
    }
}

public sealed partial class VisitSortKey : ObservableObject
{
    public string Name { get; }

    public Comparison<VisitInfo> Compare { get; }

    // 並替パネルに出す順位 (0 = 未使用)
    [ObservableProperty]
    public partial int Priority { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial bool Descending { get; set; }

    public VisitSortKey(string name, Comparison<VisitInfo> compare)
    {
        Name = name;
        Compare = compare;
    }
}
