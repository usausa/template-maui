namespace Template.MobileApp.Modules.Navigation.Stack;

public sealed class Stack2ViewModel : AppViewModelBase
{
    protected override Task OnNotifyBackAsync() => Navigator.PopAsync(1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction4() => Navigator.PushAsync(ViewId.NavigationStack3);
}
