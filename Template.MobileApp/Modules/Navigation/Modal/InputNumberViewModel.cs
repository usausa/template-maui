namespace Template.MobileApp.Modules.Navigation.Modal;

using Template.MobileApp.Models.Input;
using Template.MobileApp.Modules;

public class InputNumberViewModel : AppDialogViewModelBase, IPopupInitialize<NumberInputParameter>
{
    public PopupController<string?> Popup { get; } = new();

    public NotificationValue<string> Title { get; } = new();

    public NumberInputModel Input { get; } = new();

    public ICommand ClearCommand { get; }
    public ICommand PopCommand { get; }
    public ICommand PushCommand { get; }

    public ICommand CloseCommand { get; }
    public ICommand CommitCommand { get; }

    public InputNumberViewModel()
    {
        ClearCommand = MakeDelegateCommand(Input.Clear);
        PopCommand = MakeDelegateCommand(Input.Pop);
        PushCommand = MakeDelegateCommand<string>(Input.Push);

        CloseCommand = MakeDelegateCommand(() => Popup.Close());
        CommitCommand = MakeDelegateCommand(() => Popup.Close(Input.Text));
    }

    public void Initialize(NumberInputParameter parameter)
    {
        Title.Value = parameter.Title;
        Input.Text = parameter.Value;
        Input.MaxLength = parameter.MaxLength;
    }
}
