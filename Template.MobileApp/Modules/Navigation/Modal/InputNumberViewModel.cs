namespace Template.MobileApp.Modules.Navigation.Modal;

using Template.MobileApp.Modules;

public sealed partial class InputNumberViewModel : AppDialogViewModelBase, IPopupInitialize<NumberInputParameter>
{
    [ObservableProperty]
    public partial string Title { get; set; } = default!;

    public NumberInputModel Input { get; } = new();

    public IObserveCommand ClearCommand { get; }
    public IObserveCommand PopCommand { get; }
    public IObserveCommand PushCommand { get; }

    public IObserveCommand CloseCommand { get; }
    public IObserveCommand CommitCommand { get; }

    public InputNumberViewModel(IPopupNavigator popupNavigator)
    {
        ClearCommand = MakeDelegateCommand(Input.Clear);
        PopCommand = MakeDelegateCommand(Input.Pop);
        PushCommand = MakeDelegateCommand<string>(Input.Push);

        CloseCommand = MakeAsyncCommand(async () => await popupNavigator.CloseAsync());
        CommitCommand = MakeAsyncCommand(async () => await popupNavigator.CloseAsync(Input.Text));
    }

    public void Initialize(NumberInputParameter parameter)
    {
        Title = parameter.Title;
        Input.Text = parameter.Value;
        Input.MaxLength = parameter.MaxLength;
    }
}
