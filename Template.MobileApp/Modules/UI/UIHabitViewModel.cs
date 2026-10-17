namespace Template.MobileApp.Modules.UI;

public sealed class UIHabitViewModel : AppViewModelBase
{
    // Property

    public DateTime Today { get; } = DateTime.Today;

    public HabitTracker Tracker { get; }

    // Command

    public IObserveCommand BackCommand { get; }
    public IObserveCommand StepCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIHabitViewModel()
    {
        Tracker = new HabitTracker(HabitSample.LoadHabits(), HabitSample.LoadWeek(Today));

        BackCommand = MakeAsyncCommand(OnNotifyBackAsync);
        StepCommand = MakeDelegateCommand<HabitItem>(Tracker.Step);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.UIMenu2);
}
