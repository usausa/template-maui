namespace Template.MobileApp.Models.App;

using System.Globalization;

//--------------------------------------------------------------------------------
// Error
//--------------------------------------------------------------------------------

public enum CalcErrorType
{
    Empty,
    InvalidNumber,
    UnknownName,
    UnknownCharacter,
    UnbalancedParenthesis,
    Incomplete,
    Invalid,
    DivideByZero,
    NegativeSquareRoot,
    FactorialRange,
    UnknownOperator,
    UnknownFunction,
    NotComputable   // NaN / Infinity
}

// 表示する文言は画面で作る。Text は原因の数値・名前・文字・演算子・関数
public sealed record CalcError(CalcErrorType Type, string? Text = null) : Error($"Calc error. type=[{Type}], text=[{Text}]");

//--------------------------------------------------------------------------------
// Engine
//--------------------------------------------------------------------------------

// 科学電卓の式評価エンジン (純モデル・UI/MAUI 非依存)。
// トークナイザ → 操車場アルゴリズム (中置→RPN) → RPN 評価器 の 3 段構成。
// 対応: 四則演算 / % (百分率) / 括弧 / 単項マイナス / 三角関数 (DEG) / log / ln / exp / √ / 累乗 / 階乗 / π / e / 暗黙の乗算
public static class CalcEngine
{
    public const int MaxFactorial = 170;

    private enum TokenType
    {
        Number,
        Operator,
        Function,
        LeftParen,
        RightParen
    }

    private sealed record Token(TokenType Type, string Text, double Value = 0d);

    private sealed record OperatorInfo(int Precedence, bool RightAssociative, int ArgCount);

    private static readonly Dictionary<string, OperatorInfo> Operators = new()
    {
        ["+"] = new(2, false, 2),
        ["-"] = new(2, false, 2),
        ["*"] = new(3, false, 2),
        ["/"] = new(3, false, 2),
        ["^"] = new(5, true, 2),
        ["neg"] = new(4, true, 1),
        ["%"] = new(6, false, 1),
        ["!"] = new(6, false, 1)
    };

    private static readonly HashSet<string> Functions =
    [
        "sin", "cos", "tan", "asin", "acos", "atan", "log", "ln", "exp", "sqrt"
    ];

    // 失敗は理由 (CalcError) で返す
    public static Result<double> Evaluate(string expression) =>
        Tokenize(expression)
            .Ensure(static x => x.Count > 0, new CalcError(CalcErrorType.Empty))
            .Bind(ToRpn)
            .Bind(EvaluateRpn)
            .Ensure(Double.IsFinite, new CalcError(CalcErrorType.NotComputable));

    //--------------------------------------------------------------------------------
    // Tokenizer
    //--------------------------------------------------------------------------------

    private static Result<List<Token>> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        var i = 0;
        while (i < expression.Length)
        {
            var c = expression[i];
            if (c == ' ')
            {
                i++;
                continue;
            }

            if (Char.IsAsciiDigit(c) || (c == '.'))
            {
                var start = i;
                while ((i < expression.Length) && (Char.IsAsciiDigit(expression[i]) || (expression[i] == '.')))
                {
                    i++;
                }

                var text = expression[start..i];
                if (!Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    return new CalcError(CalcErrorType.InvalidNumber, text);
                }

                AddWithImplicitMultiply(tokens, new Token(TokenType.Number, text, value));
                continue;
            }

            // 記号 (電卓ボタンの表示文字も受け付ける)
            switch (c)
            {
                case '+' or '-' or '*' or '/' or '^' or '%' or '!':
                    tokens.Add(MakeOperator(tokens, c.ToString()));
                    i++;
                    continue;
                case '×':
                    tokens.Add(MakeOperator(tokens, "*"));
                    i++;
                    continue;
                case '÷':
                    tokens.Add(MakeOperator(tokens, "/"));
                    i++;
                    continue;
                case '−':
                    tokens.Add(MakeOperator(tokens, "-"));
                    i++;
                    continue;
                case '(':
                    AddWithImplicitMultiply(tokens, new Token(TokenType.LeftParen, "("));
                    i++;
                    continue;
                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    i++;
                    continue;
                case '√':
                    AddWithImplicitMultiply(tokens, new Token(TokenType.Function, "sqrt"));
                    i++;
                    continue;
                case 'π':
                    AddWithImplicitMultiply(tokens, new Token(TokenType.Number, "π", Math.PI));
                    i++;
                    continue;
            }

            if (Char.IsAsciiLetter(c))
            {
                var start = i;
                while ((i < expression.Length) && Char.IsAsciiLetter(expression[i]))
                {
                    i++;
                }

                var name = expression[start..i];
                if (Functions.Contains(name))
                {
                    AddWithImplicitMultiply(tokens, new Token(TokenType.Function, name));
                }
                else if (name == "e")
                {
                    AddWithImplicitMultiply(tokens, new Token(TokenType.Number, "e", Math.E));
                }
                else
                {
                    return new CalcError(CalcErrorType.UnknownName, name);
                }

                continue;
            }

            return new CalcError(CalcErrorType.UnknownCharacter, c.ToString());
        }

        return tokens;
    }

    // 直前トークンが値の終端なら暗黙の乗算 (2π, 3(1+2), (1+2)(3+4) など) を挿入する
    private static void AddWithImplicitMultiply(List<Token> tokens, Token token)
    {
        if (tokens.Count > 0)
        {
            var prev = tokens[^1];
            var prevIsValueEnd = (prev.Type == TokenType.Number) ||
                                 (prev.Type == TokenType.RightParen) ||
                                 ((prev.Type == TokenType.Operator) && ((prev.Text == "%") || (prev.Text == "!")));
            if (prevIsValueEnd)
            {
                tokens.Add(new Token(TokenType.Operator, "*"));
            }
        }

        tokens.Add(token);
    }

    // '-' が単項マイナスかどうかを直前トークンで判定する
    private static Token MakeOperator(List<Token> tokens, string text)
    {
        if (text == "-")
        {
            var isUnary = tokens.Count == 0;
            if (!isUnary)
            {
                var prev = tokens[^1];
                isUnary = (prev.Type == TokenType.LeftParen) ||
                          ((prev.Type == TokenType.Operator) && (prev.Text != "%") && (prev.Text != "!"));
            }

            if (isUnary)
            {
                return new Token(TokenType.Operator, "neg");
            }
        }

        return new Token(TokenType.Operator, text);
    }

    //--------------------------------------------------------------------------------
    // Shunting-yard (中置記法 → 逆ポーランド記法)
    //--------------------------------------------------------------------------------

    private static Result<List<Token>> ToRpn(List<Token> tokens)
    {
        var output = new List<Token>(tokens.Count);
        var stack = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    output.Add(token);
                    break;
                case TokenType.Function:
                case TokenType.LeftParen:
                    stack.Push(token);
                    break;
                case TokenType.Operator:
                    var info = Operators[token.Text];
                    while (stack.TryPeek(out var top) && (top.Type == TokenType.Operator))
                    {
                        var topInfo = Operators[top.Text];
                        if ((topInfo.Precedence > info.Precedence) ||
                            ((topInfo.Precedence == info.Precedence) && !info.RightAssociative))
                        {
                            output.Add(stack.Pop());
                        }
                        else
                        {
                            break;
                        }
                    }

                    stack.Push(token);
                    break;
                case TokenType.RightParen:
                    while (stack.TryPeek(out var top) && (top.Type != TokenType.LeftParen))
                    {
                        output.Add(stack.Pop());
                    }

                    if (!stack.TryPop(out _))
                    {
                        return new CalcError(CalcErrorType.UnbalancedParenthesis);
                    }

                    if (stack.TryPeek(out var func) && (func.Type == TokenType.Function))
                    {
                        output.Add(stack.Pop());
                    }

                    break;
            }
        }

        while (stack.TryPop(out var rest))
        {
            if (rest.Type == TokenType.LeftParen)
            {
                return new CalcError(CalcErrorType.UnbalancedParenthesis);
            }

            output.Add(rest);
        }

        return output;
    }

    //--------------------------------------------------------------------------------
    // RPN evaluator
    //--------------------------------------------------------------------------------

    private static Result<double> EvaluateRpn(List<Token> rpn)
    {
        var stack = new Stack<double>();

        foreach (var token in rpn)
        {
            Result<double> result;
            switch (token.Type)
            {
                case TokenType.Number:
                    result = token.Value;
                    break;
                case TokenType.Operator:
                    var info = Operators[token.Text];
                    if (stack.Count < info.ArgCount)
                    {
                        return new CalcError(CalcErrorType.Incomplete);
                    }

                    if (info.ArgCount == 1)
                    {
                        result = ApplyUnary(token.Text, stack.Pop());
                    }
                    else
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        result = ApplyBinary(token.Text, left, right);
                    }

                    break;
                case TokenType.Function:
                    if (stack.Count < 1)
                    {
                        return new CalcError(CalcErrorType.Incomplete);
                    }

                    result = ApplyFunction(token.Text, stack.Pop());
                    break;
                default:
                    return new CalcError(CalcErrorType.Invalid);
            }

            if (!result.TryGetValue(out var value))
            {
                return result;
            }

            stack.Push(value);
        }

        if (stack.Count != 1)
        {
            return new CalcError(CalcErrorType.Incomplete);
        }

        return stack.Pop();
    }

    private static Result<double> ApplyBinary(string op, double left, double right) => op switch
    {
        "+" => left + right,
        "-" => left - right,
        "*" => left * right,
        "/" => right == 0d ? new CalcError(CalcErrorType.DivideByZero) : left / right,
        "^" => Math.Pow(left, right),
        _ => new CalcError(CalcErrorType.UnknownOperator, op)
    };

    private static Result<double> ApplyUnary(string op, double value) => op switch
    {
        "neg" => -value,
        "%" => value / 100d,
        "!" => Factorial(value),
        _ => new CalcError(CalcErrorType.UnknownOperator, op)
    };

    // 三角関数は度 (DEG) で受け取る
    private static Result<double> ApplyFunction(string name, double value) => name switch
    {
        "sin" => Math.Sin(value * Math.PI / 180d),
        "cos" => Math.Cos(value * Math.PI / 180d),
        "tan" => Math.Tan(value * Math.PI / 180d),
        "asin" => Math.Asin(value) * 180d / Math.PI,
        "acos" => Math.Acos(value) * 180d / Math.PI,
        "atan" => Math.Atan(value) * 180d / Math.PI,
        "log" => Math.Log10(value),
        "ln" => Math.Log(value),
        "exp" => Math.Exp(value),
        "sqrt" => value < 0d ? new CalcError(CalcErrorType.NegativeSquareRoot) : Math.Sqrt(value),
        _ => new CalcError(CalcErrorType.UnknownFunction, name)
    };

    private static Result<double> Factorial(double value)
    {
        if ((value < 0d) || (value > MaxFactorial) || (Math.Abs(value - Math.Round(value)) > 1e-9))
        {
            return new CalcError(CalcErrorType.FactorialRange);
        }

        var result = 1d;
        for (var i = 2; i <= (int)Math.Round(value); i++)
        {
            result *= i;
        }

        return result;
    }
}

//--------------------------------------------------------------------------------
// Input
//--------------------------------------------------------------------------------

// 式の入力。計算の後に演算子を入れると前の結果から続け、ほかは新しい式にする
public sealed class CalcInput
{
    private const string ContinueOperators = "+−×÷^%!";

    private double lastValue;

    private bool justEvaluated;

    public string Expression { get; private set; } = string.Empty;

    public bool IsEmpty => Expression.Length == 0;

    public void Input(string token)
    {
        if (justEvaluated)
        {
            Expression = ContinueOperators.Contains(token, StringComparison.Ordinal) ? Format(lastValue) : string.Empty;
            justEvaluated = false;
        }

        Expression += token;
    }

    public void Clear()
    {
        Expression = string.Empty;
        justEvaluated = false;
    }

    public void Backspace()
    {
        justEvaluated = false;
        if (Expression.Length > 0)
        {
            Expression = Expression[..^1];
        }
    }

    public Result<double> Evaluate()
    {
        var result = CalcEngine.Evaluate(Expression);
        if (result.TryGetValue(out var value))
        {
            lastValue = value;
            justEvaluated = true;
        }

        return result;
    }

    // 末尾ゼロを出さない (極端な値は指数表記)
    public static string Format(double value)
    {
        var abs = Math.Abs(value);
        if ((abs >= 1e12) || ((abs > 0d) && (abs < 1e-9)))
        {
            return value.ToString("0.######E+0", CultureInfo.InvariantCulture);
        }

        return value.ToString("0.##########", CultureInfo.InvariantCulture);
    }
}
