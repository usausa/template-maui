namespace Template.MobileApp.Modules.UI;

public sealed partial class UITransitViewModel : AppViewModelBase
{
    // Property

    [Scope]
    [ObservableProperty]
    public partial UITransitContext Context { get; set; } = default!;

    [ObservableProperty]
    public partial int SelectedIndex { get; set; }

    [ObservableProperty]
    public partial TransitRoute Route { get; set; } = default!;

    [ObservableProperty]
    public partial double PagePosition { get; set; }

    // Command

    public IObserveCommand PreviousCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand ExpandCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UITransitViewModel()
    {
        SubscribeSelectedIndex(_ => SelectRoute());

        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        PreviousCommand = MakeDelegateCommand(() => Move(-1), () => (Context is not null) && Context.CanShift(-1));
        NextCommand = MakeDelegateCommand(() => Move(1), () => (Context is not null) && Context.CanShift(1));
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        ExpandCommand = MakeDelegateCommand<TransitSegment>(static x => x.IsExpanded = !x.IsExpanded);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatingToAsync(INavigationContext context)
    {
        if (!context.Attribute.IsRestore())
        {
            SelectRoute();
        }
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2()
    {
        Context.Reverse();
        SelectRoute();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction3()
    {
        Context.OpenTimetable(Route);
        return Navigator.PushAsync(ViewId.UITransitTimetable);
    }

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Move(int offset)
    {
        Context.Move(offset);
        SelectRoute();
        PreviousCommand.RaiseCanExecuteChanged();
        NextCommand.RaiseCanExecuteChanged();
    }

    private void SelectRoute() => Route = Context.Routes[Math.Clamp(SelectedIndex, 0, Context.Routes.Count - 1)];
}
