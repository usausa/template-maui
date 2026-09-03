namespace Template.MobileApp.Models.App;

using System.Globalization;

// 科学電卓の式評価エンジン (純モデル・UI/MAUI 非依存)。
// トークナイザ → 操車場アルゴリズム (中置→RPN) → RPN 評価器 の 3 段構成。
// 対応: 四則演算 / % (百分率) / 括弧 / 単項マイナス / 三角関数 (DEG) / log / ln / exp / √ / 累乗 / 階乗 / π / e / 暗黙の乗算
public static class ExpressionCalculator
{
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

    public static CalculationResult Evaluate(string expression)
    {
        try
        {
            var tokens = Tokenize(expression);
            if (tokens.Count == 0)
            {
                return CalculationResult.Failed("式が空です");
            }

            var rpn = ToRpn(tokens);
            var value = EvaluateRpn(rpn);
            if (Double.IsNaN(value) || Double.IsInfinity(value))
            {
                return CalculationResult.Failed("計算できません");
            }

            return CalculationResult.Succeeded(value);
        }
        catch (CalculationException ex)
        {
            return CalculationResult.Failed(ex.Message);
        }
    }

    //--------------------------------------------------------------------------------
    // Tokenizer
    //--------------------------------------------------------------------------------

    private static List<Token> Tokenize(string expression)
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
                    throw new CalculationException($"数値が不正です: {text}");
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
                    throw new CalculationException($"未知の名前です: {name}");
                }

                continue;
            }

            throw new CalculationException($"未知の文字です: {c}");
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

    private static List<Token> ToRpn(List<Token> tokens)
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
                        throw new CalculationException("括弧が対応していません");
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
                throw new CalculationException("括弧が対応していません");
            }

            output.Add(rest);
        }

        return output;
    }

    //--------------------------------------------------------------------------------
    // RPN evaluator
    //--------------------------------------------------------------------------------

    private static double EvaluateRpn(List<Token> rpn)
    {
        var stack = new Stack<double>();

        foreach (var token in rpn)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    stack.Push(token.Value);
                    break;
                case TokenType.Operator:
                    var info = Operators[token.Text];
                    if (stack.Count < info.ArgCount)
                    {
                        throw new CalculationException("式が不完全です");
                    }

                    if (info.ArgCount == 1)
                    {
                        stack.Push(ApplyUnary(token.Text, stack.Pop()));
                    }
                    else
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(ApplyBinary(token.Text, left, right));
                    }

                    break;
                case TokenType.Function:
                    if (stack.Count < 1)
                    {
                        throw new CalculationException("式が不完全です");
                    }

                    stack.Push(ApplyFunction(token.Text, stack.Pop()));
                    break;
                default:
                    throw new CalculationException("式が不正です");
            }
        }

        if (stack.Count != 1)
        {
            throw new CalculationException("式が不完全です");
        }

        return stack.Pop();
    }

    private static double ApplyBinary(string op, double left, double right) => op switch
    {
        "+" => left + right,
        "-" => left - right,
        "*" => left * right,
        "/" => right == 0d ? throw new CalculationException("0 では割れません") : left / right,
        "^" => Math.Pow(left, right),
        _ => throw new CalculationException($"未知の演算子です: {op}")
    };

    private static double ApplyUnary(string op, double value) => op switch
    {
        "neg" => -value,
        "%" => value / 100d,
        "!" => Factorial(value),
        _ => throw new CalculationException($"未知の演算子です: {op}")
    };

    // 三角関数は度 (DEG) で受け取る
    private static double ApplyFunction(string name, double value) => name switch
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
        "sqrt" => value < 0d ? throw new CalculationException("負数の平方根は計算できません") : Math.Sqrt(value),
        _ => throw new CalculationException($"未知の関数です: {name}")
    };

    private static double Factorial(double value)
    {
        if ((value < 0d) || (value > 170d) || (Math.Abs(value - Math.Round(value)) > 1e-9))
        {
            throw new CalculationException("階乗は 0〜170 の整数のみです");
        }

        var result = 1d;
        for (var i = 2; i <= (int)Math.Round(value); i++)
        {
            result *= i;
        }

        return result;
    }
}

public sealed class CalculationException : Exception
{
    public CalculationException()
    {
    }

    public CalculationException(string message)
        : base(message)
    {
    }

    public CalculationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public readonly record struct CalculationResult(bool Success, double Value, string Error)
{
    public static CalculationResult Succeeded(double value) => new(true, value, string.Empty);

    public static CalculationResult Failed(string error) => new(false, 0d, error);
}
