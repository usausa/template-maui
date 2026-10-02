namespace Template.MobileApp.Modules.App;

using Template.MobileApp.Models.App;

public sealed partial class AppCalcViewModel : AppViewModelBase
{
    private readonly CalcInput input = new();

    [ObservableProperty]
    public partial string Expression { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Result { get; set; } = "0";

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public IObserveCommand InputCommand { get; }

    public IObserveCommand ClearCommand { get; }

    public IObserveCommand BackspaceCommand { get; }

    public IObserveCommand EvaluateCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public AppCalcViewModel()
    {
        InputCommand = MakeDelegateCommand<string>(Input);
        ClearCommand = MakeDelegateCommand(Clear);
        BackspaceCommand = MakeDelegateCommand(Backspace);
        EvaluateCommand = MakeDelegateCommand(Evaluate);
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void Input(string token)
    {
        input.Input(token);
        Expression = input.Expression;
        ErrorMessage = string.Empty;
    }

    private void Clear()
    {
        input.Clear();
        Expression = input.Expression;
        Result = "0";
        ErrorMessage = string.Empty;
    }

    private void Backspace()
    {
        input.Backspace();
        Expression = input.Expression;
        ErrorMessage = string.Empty;
    }

    // 式が空のときは何もしない
    private void Evaluate()
    {
        if (!input.IsEmpty)
        {
            var result = input.Evaluate();
            if (result.TryGetValue(out var value))
            {
                Result = CalcInput.Format(value);
                ErrorMessage = string.Empty;
            }
            else if (result.Error is CalcError error)
            {
                ErrorMessage = FormatError(error);
            }
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static string FormatError(CalcError error) =>
        error.Type switch
        {
            CalcErrorType.Empty => "式が空です",
            CalcErrorType.InvalidNumber => $"数値が不正です: {error.Text}",
            CalcErrorType.UnknownName => $"未知の名前です: {error.Text}",
            CalcErrorType.UnknownCharacter => $"未知の文字です: {error.Text}",
            CalcErrorType.UnbalancedParenthesis => "括弧が対応していません",
            CalcErrorType.Incomplete => "式が不完全です",
            CalcErrorType.DivideByZero => "0 では割れません",
            CalcErrorType.NegativeSquareRoot => "負数の平方根は計算できません",
            CalcErrorType.FactorialRange => $"階乗は 0〜{CalcEngine.MaxFactorial} の整数のみです",
            CalcErrorType.UnknownOperator => $"未知の演算子です: {error.Text}",
            CalcErrorType.UnknownFunction => $"未知の関数です: {error.Text}",
            CalcErrorType.NotComputable => "計算できません",
            _ => "式が不正です"
        };
}
