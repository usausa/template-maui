namespace Template.MobileApp.State;

public sealed partial class StartupState : ObservableObject
{
    private readonly TaskCompletionSource completedSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Completed => completedSource.Task;

    public DateTime? CompletedAt { get; private set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; private set; }

    public void NotifyCompleted()
    {
        CompletedAt = DateTime.Now;
        IsCompleted = true;
        completedSource.TrySetResult();
    }
}
