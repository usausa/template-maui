namespace Template.MobileApp.Modules.App;

using Template.MobileApp.Models.App;

public sealed partial class AppTodoEditViewModel : AppViewModelBase
{
    [Scope]
    [ObservableProperty]
    public partial AppTodoContext Context { get; set; } = default!;

    [ObservableProperty]
    public partial bool IsDatePickerOpen { get; set; }

    public IObserveCommand CloseCommand { get; }
    public IObserveCommand SaveCommand { get; }
    public IObserveCommand DeleteCommand { get; }

    public IObserveCommand DueCommand { get; }
    public IObserveCommand PickDateCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppTodoEditViewModel()
    {
        CloseCommand = MakeAsyncCommand(OnNotifyBackAsync);
        SaveCommand = MakeAsyncCommand(SaveAsync);
        DeleteCommand = MakeAsyncCommand(DeleteAsync);

#pragma warning disable IDE0200
        DueCommand = MakeDelegateCommand<TodoDue>(x => Context.Draft.SetDue(x));
#pragma warning restore IDE0200
        PickDateCommand = MakeDelegateCommand(() => IsDatePickerOpen = true);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.PopAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private async Task SaveAsync()
    {
        await Context.SaveAsync();
        await Navigator.PopAsync();
    }

    private async Task DeleteAsync()
    {
        await Context.DeleteEditingAsync();
        await Navigator.PopAsync();
    }
}
