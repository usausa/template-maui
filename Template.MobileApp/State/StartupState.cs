namespace Template.MobileApp.State;

public sealed class StartupState
{
    private readonly TaskCompletionSource completedSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Completed => completedSource.Task;

    public void NotifyCompleted() => completedSource.TrySetResult();
}
