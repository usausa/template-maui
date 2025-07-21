namespace Template.MobileApp.Controls;

using System;

using Font = Microsoft.Maui.Graphics.Font;

public sealed class NoiseGauge : GraphicsView, IDrawable
{
    private const float StartAngle = 210f;
    private const float EndAngle = -30f;
    private const float RangeAngle = StartAngle - EndAngle;

    private static readonly Font DefaultFont = new("Arial");

    // Value

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(NoiseGauge),
        propertyChanged: PropertyValueChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty MinProperty = BindableProperty.Create(
        nameof(Min),
        typeof(double),
        typeof(NoiseGauge),
        20d,
        propertyChanged: PropertyValueChanged);

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max),
        typeof(double),
        typeof(NoiseGauge),
        120d,
        propertyChanged: PropertyValueChanged);

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public static readonly BindableProperty ThresholdProperty = BindableProperty.Create(
        nameof(Threshold),
        typeof(double),
        typeof(NoiseGauge),
        70d,
        propertyChanged: PropertyValueChanged);

    public double Threshold
    {
        get => (double)GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    // Size

    public static readonly BindableProperty BorderMarginProperty = BindableProperty.Create(
        nameof(BorderMargin),
        typeof(float),
        typeof(NoiseGauge),
        12f,
        propertyChanged: PropertyValueChanged);

    public float BorderMargin
    {
        get => (float)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public static readonly BindableProperty GaugeWidthProperty = BindableProperty.Create(
        nameof(GaugeWidth),
        typeof(float),
        typeof(NoiseGauge),
        4f,
        propertyChanged: PropertyValueChanged);

    public float GaugeWidth
    {
        get => (float)GetValue(GaugeWidthProperty);
        set => SetValue(GaugeWidthProperty, value);
    }

    public static readonly BindableProperty TickLengthProperty = BindableProperty.Create(
        nameof(TickLength),
        typeof(float),
        typeof(NoiseGauge),
        12f,
        propertyChanged: PropertyValueChanged);

    public float TickLength
    {
        get => (float)GetValue(TickLengthProperty);
        set => SetValue(TickLengthProperty, value);
    }

    public static readonly BindableProperty MinorTickLengthProperty = BindableProperty.Create(
        nameof(MinorTickLength),
        typeof(float),
        typeof(NoiseGauge),
        18f,
        propertyChanged: PropertyValueChanged);

    public float MinorTickLength
    {
        get => (float)GetValue(MinorTickLengthProperty);
        set => SetValue(MinorTickLengthProperty, value);
    }

    public static readonly BindableProperty MajorTickLengthProperty = BindableProperty.Create(
        nameof(MajorTickLength),
        typeof(float),
        typeof(NoiseGauge),
        24f,
        propertyChanged: PropertyValueChanged);

    public float MajorTickLength
    {
        get => (float)GetValue(MajorTickLengthProperty);
        set => SetValue(MajorTickLengthProperty, value);
    }

    public static readonly BindableProperty TickWidthProperty = BindableProperty.Create(
        nameof(TickWidth),
        typeof(float),
        typeof(NoiseGauge),
        1f,
        propertyChanged: PropertyValueChanged);

    public float TickWidth
    {
        get => (float)GetValue(TickWidthProperty);
        set => SetValue(TickWidthProperty, value);
    }

    public static readonly BindableProperty MinorTickWidthProperty = BindableProperty.Create(
        nameof(MinorTickWidth),
        typeof(float),
        typeof(NoiseGauge),
        2f,
        propertyChanged: PropertyValueChanged);

    public float MinorTickWidth
    {
        get => (float)GetValue(MinorTickWidthProperty);
        set => SetValue(MinorTickWidthProperty, value);
    }

    public static readonly BindableProperty MajorTickWidthProperty = BindableProperty.Create(
        nameof(MajorTickWidth),
        typeof(float),
        typeof(NoiseGauge),
        4f,
        propertyChanged: PropertyValueChanged);

    public float MajorTickWidth
    {
        get => (float)GetValue(MajorTickWidthProperty);
        set => SetValue(MajorTickWidthProperty, value);
    }

    public static readonly BindableProperty TextOffsetProperty = BindableProperty.Create(
        nameof(TextOffset),
        typeof(float),
        typeof(NoiseGauge),
        44f,
        propertyChanged: PropertyValueChanged);

    public float TextOffset
    {
        get => (float)GetValue(TextOffsetProperty);
        set => SetValue(TextOffsetProperty, value);
    }

    public static readonly BindableProperty NeedleWidthProperty = BindableProperty.Create(
        nameof(NeedleWidth),
        typeof(float),
        typeof(NoiseGauge),
        11f,
        propertyChanged: PropertyValueChanged);

    public float NeedleWidth
    {
        get => (float)GetValue(NeedleWidthProperty);
        set => SetValue(NeedleWidthProperty, value);
    }

    public static readonly BindableProperty CenterCircleRadiusProperty = BindableProperty.Create(
        nameof(CenterCircleRadius),
        typeof(float),
        typeof(NoiseGauge),
        10f,
        propertyChanged: PropertyValueChanged);

    public float CenterCircleRadius
    {
        get => (float)GetValue(CenterCircleRadiusProperty);
        set => SetValue(CenterCircleRadiusProperty, value);
    }

    public static readonly BindableProperty NeedleLengthPercentageProperty = BindableProperty.Create(
        nameof(NeedleLengthPercentage),
        typeof(float),
        typeof(NoiseGauge),
        1f,
        propertyChanged: PropertyValueChanged);

    public float NeedleLengthPercentage
    {
        get => (float)GetValue(NeedleLengthPercentageProperty);
        set => SetValue(NeedleLengthPercentageProperty, value);
    }

    public static readonly BindableProperty NeedleBackLengthPercentageProperty = BindableProperty.Create(
        nameof(NeedleBackLengthPercentage),
        typeof(float),
        typeof(NoiseGauge),
        0.15f,
        propertyChanged: PropertyValueChanged);

    public float NeedleBackLengthPercentage
    {
        get => (float)GetValue(NeedleBackLengthPercentageProperty);
        set => SetValue(NeedleBackLengthPercentageProperty, value);
    }

    // Font

    public static readonly BindableProperty FontProperty = BindableProperty.Create(
        nameof(Font),
        typeof(Font),
        typeof(NoiseGauge),
        DefaultFont,
        propertyChanged: PropertyValueChanged);

    public Font Font
    {
        get => (Font)GetValue(FontProperty);
        set => SetValue(FontProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(float),
        typeof(NoiseGauge),
        18f,
        propertyChanged: PropertyValueChanged);

    public float FontSize
    {
        get => (float)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    // Color

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.White,
        propertyChanged: PropertyValueChanged);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color GaugeColor { get; set; } = Colors.White;

    public static readonly BindableProperty WarningGaugeColorProperty = BindableProperty.Create(
        nameof(WarningGaugeColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.Red,
        propertyChanged: PropertyValueChanged);

    public Color WarningGaugeColor
    {
        get => (Color)GetValue(WarningGaugeColorProperty);
        set => SetValue(WarningGaugeColorProperty, value);
    }

    public static readonly BindableProperty NeedleColorProperty = BindableProperty.Create(
        nameof(NeedleColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.Red,
        propertyChanged: PropertyValueChanged);

    public Color NeedleColor
    {
        get => (Color)GetValue(NeedleColorProperty);
        set => SetValue(NeedleColorProperty, value);
    }

    public static readonly BindableProperty NeedleCenterColorProperty = BindableProperty.Create(
        nameof(NeedleCenterColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.Red,
        propertyChanged: PropertyValueChanged);

    public Color NeedleCenterColor
    {
        get => (Color)GetValue(NeedleCenterColorProperty);
        set => SetValue(NeedleCenterColorProperty, value);
    }

    public NoiseGauge()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
    }

    private static void PropertyValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((NoiseGauge)bindable).Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var max = Max;
        var min = Min;
        var range = max - min;
        var threshold = Threshold;

        var borderMargin = BorderMargin;
        var gaugeWidth = GaugeWidth;

        var majorTickWidth = MajorTickWidth;
        var minorTickWidth = MinorTickWidth;
        var tickWidth = TickWidth;
        var majorTickLength = MajorTickLength;
        var minorTickLength = MinorTickLength;
        var tickLength = TickLength;

        var textOffset = TextOffset;

        var font = Font;
        var fontSize = FontSize;

        var textColor = TextColor;
        var gaugeColor = GaugeColor;
        var warningGaugeColor = WarningGaugeColor;

        // Calculate
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - (borderMargin * 2);
        var radius = size / 2;
        var rect = new RectF(dirtyRect.Center.X - radius, borderMargin, size, size);

        var cx = dirtyRect.Center.X;
        var cy = rect.Center.Y;

        // Setup
        canvas.Font = font;
        canvas.FontSize = fontSize;

        // Background
        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        // Tick
        canvas.StrokeLineCap = LineCap.Square;
        for (var i = 0; i <= (int)range; i++)
        {
            var angle = (float)((RangeAngle * i / range) - (StartAngle - 90));
            var isMajor = i % 10 == 0;
            var isMinor = i % 5 == 0;

            var width = isMajor ? majorTickWidth : isMinor ? minorTickWidth : tickWidth;
            var length = isMajor ? majorTickLength : isMinor ? minorTickLength : tickLength;

            canvas.SaveState();

            canvas.Translate(cx, cy);
            canvas.Rotate(angle);

            canvas.StrokeColor = i + min >= threshold ? warningGaugeColor : gaugeColor;
            canvas.StrokeSize = width;
            canvas.DrawLine(0, -radius + 0.5f, 0, -radius + length);

            canvas.RestoreState();

            if (isMajor)
            {
                var offset = radius - textOffset;
                var textRadian = (float)((StartAngle - (RangeAngle * i / range)) * MathF.PI / 180);
                var textX = cx + (offset * MathF.Cos(textRadian));
                var textY = cy - (offset * MathF.Sin(textRadian));

                var text = $"{(int)(i + min)}";
                var textSize = canvas.GetStringSize(text, font, fontSize);
                canvas.FontColor = textColor;
                canvas.DrawString(text, textX, textY + (textSize.Height / 2), HorizontalAlignment.Center);
            }
        }

        // Arc
        canvas.StrokeSize = gaugeWidth;
        canvas.StrokeColor = gaugeColor;
        canvas.DrawArc(rect, StartAngle, EndAngle, true, false);

        var warningStart = StartAngle - (float)(RangeAngle * (Threshold - Min) / range);
        canvas.StrokeColor = warningGaugeColor;
        canvas.DrawArc(rect, warningStart, EndAngle, true, false);

        // Needle
        var value = Math.Clamp(Value, min, max);
        var needleRadian = (float)((StartAngle - (RangeAngle * (value - Min) / range)) * MathF.PI / 180);

        var needleLength = (radius - MajorTickLength) * NeedleLengthPercentage;
        var needleBackLength = (radius - MajorTickLength) * NeedleBackLengthPercentage;
        var baseCenterX = cx - (needleBackLength * MathF.Cos(needleRadian));
        var baseCenterY = cy + (needleBackLength * MathF.Sin(needleRadian));
        var baseAngle1 = needleRadian + (MathF.PI / 2);
        var baseAngle2 = needleRadian - (MathF.PI / 2);
        var baseRadius = NeedleWidth / 2;

        var path = new PathF();
        path.MoveTo(cx + (needleLength * MathF.Cos(needleRadian)), cy - (needleLength * MathF.Sin(needleRadian)));
        path.LineTo(baseCenterX + (baseRadius * MathF.Cos(baseAngle1)), baseCenterY - (baseRadius * MathF.Sin(baseAngle1)));
        path.LineTo(baseCenterX + (baseRadius * MathF.Cos(baseAngle2)), baseCenterY - (baseRadius * MathF.Sin(baseAngle2)));
        path.Close();

        canvas.FillColor = NeedleColor;
        canvas.FillPath(path);

        // Center
        var centerCircleRadius = CenterCircleRadius;
        canvas.FillColor = NeedleCenterColor;
        canvas.FillCircle(cx, cy, centerCircleRadius);
    }
}
