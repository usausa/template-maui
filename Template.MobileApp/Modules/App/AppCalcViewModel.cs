namespace Template.MobileApp.Modules.App;

using System.Globalization;

using Template.MobileApp.Models.App;

public sealed partial class AppCalcViewModel : AppViewModelBase
{
    private const string ContinueOperators = "+−×÷^%!";

    private double lastValue;

    private bool justEvaluated;

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

    public AppCalcViewModel()
    {
        InputCommand = MakeDelegateCommand<string>(Input);
        ClearCommand = MakeDelegateCommand(Clear);
        BackspaceCommand = MakeDelegateCommand(Backspace);
        EvaluateCommand = MakeDelegateCommand(Evaluate);
    }

    private void Input(string token)
    {
        // 「=」直後は、演算子なら結果から継続、それ以外は新しい式を開始する
        if (justEvaluated)
        {
            Expression = ContinueOperators.Contains(token, StringComparison.Ordinal)
                ? FormatValue(lastValue)
                : string.Empty;
            justEvaluated = false;
        }

        Expression += token;
        ErrorMessage = string.Empty;
    }

    private void Clear()
    {
        Expression = string.Empty;
        Result = "0";
        ErrorMessage = string.Empty;
        justEvaluated = false;
    }

    private void Backspace()
    {
        if (justEvaluated)
        {
            justEvaluated = false;
        }

        if (Expression.Length > 0)
        {
            Expression = Expression[..^1];
        }

        ErrorMessage = string.Empty;
    }

    private void Evaluate()
    {
        if (Expression.Length == 0)
        {
            return;
        }

        var result = ExpressionCalculator.Evaluate(Expression);
        if (result.Success)
        {
            lastValue = result.Value;
            Result = FormatValue(result.Value);
            ErrorMessage = string.Empty;
            justEvaluated = true;
        }
        else
        {
            ErrorMessage = result.Error;
        }
    }

    // 末尾ゼロを出さない表示 (極端な値は指数表記)
    private static string FormatValue(double value)
    {
        var abs = Math.Abs(value);
        if ((abs >= 1e12) || ((abs > 0d) && (abs < 1e-9)))
        {
            return value.ToString("0.######E+0", CultureInfo.InvariantCulture);
        }

        return value.ToString("0.##########", CultureInfo.InvariantCulture);
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.AppMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
