namespace Template.MobileApp.Modules.Navigation.Stack;

public sealed class Stack3ViewModel : AppViewModelBase
{
    protected override Task OnNotifyBackAsync() => Navigator.PopAsync(1);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction2() => Navigator.PopAsync(2);
}
