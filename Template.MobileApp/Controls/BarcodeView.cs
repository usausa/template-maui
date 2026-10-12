namespace Template.MobileApp.Controls;

using SkiaSharp.Views.Maui;

public sealed class BarcodeView : SKCanvasView
{
    private const int StartC = 105;

    private const int Stop = 106;

    private const int QuietZone = 10;

    private const int CheckDivisor = 103;

    private static readonly string[] Patterns =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213",
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132",
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211",
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331",
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111",
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141",
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141",
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112"
    ];

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private string? encodedValue;

    private int[] widths = [];

    private int modules;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(string),
        typeof(BarcodeView),
        string.Empty,
        propertyChanged: OnVisualChanged);

    public static readonly BindableProperty BarColorProperty = BindableProperty.Create(
        nameof(BarColor),
        typeof(Color),
        typeof(BarcodeView),
        Colors.Black,
        propertyChanged: OnVisualChanged);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((BarcodeView)bindable).InvalidateSurface();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (encodedValue != Value)
        {
            encodedValue = Value;
            widths = Encode(Value);
            modules = widths.Sum() + (QuietZone * 2);
        }

        if (widths.Length == 0)
        {
            return;
        }

        var unit = Math.Max(1, e.Info.Width / modules);
        var x = (e.Info.Width - ((modules - (QuietZone * 2)) * unit)) / 2;
        using var paint = new SKPaint();
        paint.Color = BarColor.ToSKColor();
        for (var i = 0; i < widths.Length; i++)
        {
            var width = widths[i] * unit;
            if (i % 2 == 0)
            {
                canvas.DrawRect(x, 0, width, e.Info.Height, paint);
            }
            x += width;
        }
    }

    //--------------------------------------------------------------------------------
    // Encode
    //--------------------------------------------------------------------------------

    private static int[] Encode(string value)
    {
        var digits = new string(value.Where(Char.IsAsciiDigit).ToArray());
        if (digits.Length == 0)
        {
            return [];
        }
        if (digits.Length % 2 == 1)
        {
            digits = "0" + digits;
        }

        var codes = new List<int> { StartC };
        for (var i = 0; i < digits.Length; i += 2)
        {
            codes.Add(((digits[i] - '0') * 10) + (digits[i + 1] - '0'));
        }

        var sum = codes[0];
        for (var i = 1; i < codes.Count; i++)
        {
            sum += codes[i] * i;
        }
        codes.Add(sum % CheckDivisor);
        codes.Add(Stop);

        return codes.SelectMany(static x => Patterns[x].Select(static c => c - '0')).ToArray();
    }
}
