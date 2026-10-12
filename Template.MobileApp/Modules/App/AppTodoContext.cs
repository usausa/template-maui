namespace Template.MobileApp.Modules.App;

using Smart.Mapper;

using Template.MobileApp.Models.App;
using Template.MobileApp.Services;

//--------------------------------------------------------------------------------
// Input
//--------------------------------------------------------------------------------

// 編集の入力 (新規は Id が null)。期限の区分けは Today から見る
public sealed partial class TodoDraft : ObservableObject
{
    public DateTime Today { get; init; }

    public long? Id { get; set; }

    public bool IsNew => Id is null;

    [ObservableProperty(NotifyAlso = [nameof(CanSave)])]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Note { get; set; } = string.Empty;

    [ObservableProperty(NotifyAlso = [nameof(Due)])]
    public partial DateTime? DueDate { get; set; }

    public TodoDue Due => TodoDueRule.Classify(DueDate, Today);

    [ObservableProperty]
    public partial bool IsImportant { get; set; }

    [ObservableProperty]
    public partial bool IsDone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool CanSave => !String.IsNullOrWhiteSpace(Title);

    // 期限のチップ (今日 / 明日 / 期限なし)
    public void SetDue(TodoDue due) =>
        DueDate = due switch
        {
            TodoDue.Today => Today,
            TodoDue.Tomorrow => Today.AddDays(1),
            _ => null
        };

    // 前後の空白を除き、期限は日付だけにする
    public void Normalize()
    {
        Title = Title.Trim();
        Note = Note.Trim();
        DueDate = DueDate?.Date;
    }
}

//--------------------------------------------------------------------------------
// Mapper
//--------------------------------------------------------------------------------

public static partial class AppTodoContextMapper
{
    [Mapper]
    [MapIgnore(nameof(TodoItem.Due))]
    public static partial TodoItem ToItem(this TodoEntity source);

    [Mapper]
    public static partial TodoEntity ToEntity(this TodoItem source);

    [Mapper]
    public static partial void CopyTo(this TodoItem source, TodoDraft destination);

    [Mapper]
    [MapIgnore(nameof(TodoItem.Id))]
    [MapIgnore(nameof(TodoItem.CreatedAt))]
    [MapIgnore(nameof(TodoItem.UpdatedAt))]
    public static partial void CopyTo(this TodoDraft source, TodoItem destination);
}

//--------------------------------------------------------------------------------
// Context
//--------------------------------------------------------------------------------

// 一覧と編集が共有する (一覧を開いている間)
public sealed partial class AppTodoContext : ObservableObject
{
    private readonly TimeProvider timeProvider;

    private readonly DataService dataService;

    // 編集している行 (新規は null)
    private TodoItem? editing;

    public TodoList List { get; } = new();

    [ObservableProperty]
    public partial DateTime Today { get; set; }

    [ObservableProperty]
    public partial TodoDraft Draft { get; set; } = new();

    // 消した行 (元に戻せる間)
    [ObservableProperty]
    public partial TodoItem? DeletedItem { get; set; }

    public AppTodoContext(
        TimeProvider timeProvider,
        DataService dataService)
    {
        this.timeProvider = timeProvider;
        this.dataService = dataService;
    }

    public async Task LoadAsync()
    {
        Today = timeProvider.GetLocalNow().Date;
        foreach (var entity in await dataService.QueryTodoListAsync())
        {
            var item = entity.ToItem();
            item.UpdateDue(Today);
            List.Place(item);
        }

        List.UpdateSummary();
    }

    // 行を完了 (または元) のグループへ動かすのは呼び出し側 (List.Move)
    public async Task ToggleDoneAsync(TodoItem item)
    {
        item.IsDone = !item.IsDone;
        item.UpdatedAt = timeProvider.GetLocalNow().DateTime;
        await dataService.UpdateTodoAsync(item.ToEntity());
        List.UpdateSummary();
    }

    public async Task ToggleImportantAsync(TodoItem item)
    {
        item.IsImportant = !item.IsImportant;
        item.UpdatedAt = timeProvider.GetLocalNow().DateTime;
        await dataService.UpdateTodoAsync(item.ToEntity());
    }

    // すぐに DB から消し、元に戻せるように残す
    public async Task DeleteAsync(TodoItem item)
    {
        await dataService.DeleteTodoAsync(item.Id);
        List.Remove(item);
        List.UpdateSummary();
        DeletedItem = item;
    }

    // 消した行を同じ Id で入れ直す
    public async Task UndoAsync()
    {
        if (DeletedItem is { } item)
        {
            DeletedItem = null;
            await dataService.InsertTodoEnumerableAsync([item.ToEntity()]);
            List.Place(item);
            List.UpdateSummary();
        }
    }

    public void ClearDeleted() => DeletedItem = null;

    // 新規は期限を今日にする
    public void BeginEdit(TodoItem? item)
    {
        editing = item;
        var draft = new TodoDraft { Today = Today, DueDate = Today };
        item?.CopyTo(draft);
        Draft = draft;
    }

    // 編集の入力を一覧と DB に反映する
    public async Task SaveAsync()
    {
        Draft.Normalize();
        var now = timeProvider.GetLocalNow().DateTime;
        if (editing is { } item)
        {
            List.Remove(item);
            Draft.CopyTo(item);
            item.UpdatedAt = now;
            await dataService.UpdateTodoAsync(item.ToEntity());
            List.Place(item);
        }
        else
        {
            var added = new TodoItem { CreatedAt = now, UpdatedAt = now };
            Draft.CopyTo(added);
            added.Id = await dataService.InsertTodoAsync(added.ToEntity());
            List.Place(added);
        }

        List.UpdateSummary();
    }

    public Task DeleteEditingAsync() =>
        editing is not null ? DeleteAsync(editing) : Task.CompletedTask;
}
