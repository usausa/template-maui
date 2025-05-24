namespace Template.MobileApp.Modules.Key;

using System.Diagnostics;

public sealed class KeyEntryViewModel : AppViewModelBase
{
    public EntryController Input1 { get; }
    public EntryController Input2 { get; }
    public EntryController Input3 { get; }

    public KeyEntryViewModel()
    {
        Input1 = new EntryController(MakeDelegateCommand<EntryCompleteEvent>(Input1Complete));
        Input2 = new EntryController(MakeDelegateCommand<EntryCompleteEvent>(Input2Complete));
        Input3 = new EntryController(MakeDelegateCommand<EntryCompleteEvent>(Input3Complete));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.KeyMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    protected override Task OnNotifyFunction3()
    {
        Input1.Enable = !Input1.Enable;
        return Task.CompletedTask;
    }

    protected override Task OnNotifyFunction4()
    {
        Input3.Text = "123";
        return Task.CompletedTask;
    }

    private void Input1Complete(EntryCompleteEvent ice)
    {
        ice.Handled = String.IsNullOrEmpty(Input1.Text);
        Debug.WriteLine($"**** Input1 completed {Input1.Text}");
    }

    private void Input2Complete(EntryCompleteEvent ice)
    {
        ice.Handled = String.IsNullOrEmpty(Input2.Text);
        Debug.WriteLine($"**** Input2 completed {Input2.Text}");
    }

    private void Input3Complete(EntryCompleteEvent ice)
    {
        ice.Handled = String.IsNullOrEmpty(Input3.Text);
        Debug.WriteLine($"**** Input3 completed {Input3.Text}");

        if (!ice.Handled)
        {
            Input1.Text = string.Empty;
            Input2.Text = string.Empty;
            Input3.Text = string.Empty;
            Input1.FocusRequest();
        }
    }
}
