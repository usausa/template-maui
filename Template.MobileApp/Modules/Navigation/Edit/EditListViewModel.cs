namespace Template.MobileApp.Modules.Navigation.Edit;

using Template.MobileApp.Services;

public sealed partial class EditListViewModel : AppViewModelBase
{
    private readonly IDialog dialog;

    private readonly DataService dataService;

    public ObservableCollection<WorkEntity> Items { get; } = [];

    // CollectionView.SelectedItems は IList<object> のため object で受ける (選択変更で中身が更新される)
    public ObservableCollection<object> SelectedItems { get; } = [];

    [ObservableProperty(NotifyAlso = [nameof(SelectionMode)])]
    public partial bool IsSelectionMode { get; set; }

    public SelectionMode SelectionMode => IsSelectionMode ? SelectionMode.Multiple : SelectionMode.None;

    public IObserveCommand SelectCommand { get; }
    public IObserveCommand DeleteCommand { get; }
    public IObserveCommand SelectAllCommand { get; }
    public IObserveCommand BulkDeleteCommand { get; }

    public EditListViewModel(
        IDialog dialog,
        DataService dataService)
    {
        this.dialog = dialog;
        this.dataService = dataService;

        SelectCommand = MakeAsyncCommand<WorkEntity>(x =>
            Navigator.ForwardAsync(ViewId.NavigationEditDetailUpdate, new NavigationParameter().SetValue(x)));
        DeleteCommand = MakeAsyncCommand<WorkEntity>(DeleteAsync);
        SelectAllCommand = MakeDelegateCommand(SelectAll);
        BulkDeleteCommand = MakeAsyncCommand(BulkDeleteAsync, () => SelectedItems.Count > 0);

        SelectedItems.CollectionChanged += (_, _) => BulkDeleteCommand.RaiseCanExecuteChanged();
    }

    public override async Task OnNavigatedToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            Items.AddRange(await dataService.QueryWorkListAsync());
        }
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.NavigationMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction3()
    {
        IsSelectionMode = !IsSelectionMode;
        if (!IsSelectionMode)
        {
            SelectedItems.Clear();
        }
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4() => Navigator.ForwardAsync(ViewId.NavigationEditDetailNew);

    private async Task DeleteAsync(WorkEntity entity)
    {
        if (!await dialog.ConfirmAsync($"Delete {entity.Name} ?"))
        {
            return;
        }

        await dataService.DeleteWorkAsync(entity.Id);

        Items.Remove(entity);
    }

    private void SelectAll()
    {
        foreach (var item in Items)
        {
            if (!SelectedItems.Contains(item))
            {
                SelectedItems.Add(item);
            }
        }
    }

    private async Task BulkDeleteAsync()
    {
        var targets = SelectedItems.OfType<WorkEntity>().ToList();
        if (targets.Count == 0)
        {
            return;
        }

        if (!await dialog.ConfirmAsync($"Delete {targets.Count} items ?"))
        {
            return;
        }

        foreach (var entity in targets)
        {
            await dataService.DeleteWorkAsync(entity.Id);
            Items.Remove(entity);
        }

        SelectedItems.Clear();
        IsSelectionMode = false;
    }
}
