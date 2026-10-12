namespace Template.MobileApp.Controls;

using System;
using System.Timers;

using Font = Microsoft.Maui.Graphics.Font;

public sealed class NoiseGauge : GraphicsView, IDrawable, IDisposable
{
    private const float StartAngle = 210f;

    private const float EndAngle = -30f;

    private const float RangeAngle = StartAngle - EndAngle;

    private const double FrameInterval = 1000d / 60;

    private const double SettleThreshold = 0.1;

    private const double EasingRate = 0.1;

    private const int MajorTickStep = 10;

    private const int MinorTickStep = 5;

    private static readonly Font DefaultFont = new("Arial");

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private readonly System.Timers.Timer animationTimer = new(FrameInterval);

    private double animatedValue;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(NoiseGauge),
        propertyChanged: HandleValueChanged);

    public static readonly BindableProperty MinProperty = BindableProperty.Create(
        nameof(Min),
        typeof(double),
        typeof(NoiseGauge),
        20d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max),
        typeof(double),
        typeof(NoiseGauge),
        120d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty ThresholdProperty = BindableProperty.Create(
        nameof(Threshold),
        typeof(double),
        typeof(NoiseGauge),
        70d,
        propertyChanged: Invalidate);

    public static readonly BindableProperty PeakValueProperty = BindableProperty.Create(
        nameof(PeakValue),
        typeof(double),
        typeof(NoiseGauge),
        propertyChanged: Invalidate);

    public static readonly BindableProperty BorderMarginProperty = BindableProperty.Create(
        nameof(BorderMargin),
        typeof(float),
        typeof(NoiseGauge),
        12f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty GaugeWidthProperty = BindableProperty.Create(
        nameof(GaugeWidth),
        typeof(float),
        typeof(NoiseGauge),
        4f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty TickLengthProperty = BindableProperty.Create(
        nameof(TickLength),
        typeof(float),
        typeof(NoiseGauge),
        12f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MinorTickLengthProperty = BindableProperty.Create(
        nameof(MinorTickLength),
        typeof(float),
        typeof(NoiseGauge),
        18f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MajorTickLengthProperty = BindableProperty.Create(
        nameof(MajorTickLength),
        typeof(float),
        typeof(NoiseGauge),
        24f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty TickWidthProperty = BindableProperty.Create(
        nameof(TickWidth),
        typeof(float),
        typeof(NoiseGauge),
        1f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MinorTickWidthProperty = BindableProperty.Create(
        nameof(MinorTickWidth),
        typeof(float),
        typeof(NoiseGauge),
        2f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty MajorTickWidthProperty = BindableProperty.Create(
        nameof(MajorTickWidth),
        typeof(float),
        typeof(NoiseGauge),
        4f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty TextOffsetProperty = BindableProperty.Create(
        nameof(TextOffset),
        typeof(float),
        typeof(NoiseGauge),
        44f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty NeedleWidthProperty = BindableProperty.Create(
        nameof(NeedleWidth),
        typeof(float),
        typeof(NoiseGauge),
        11f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty PeakNeedleWidthProperty = BindableProperty.Create(
        nameof(PeakNeedleWidth),
        typeof(float),
        typeof(NoiseGauge),
        3f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty CenterCircleRadiusProperty = BindableProperty.Create(
        nameof(CenterCircleRadius),
        typeof(float),
        typeof(NoiseGauge),
        10f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty NeedleLengthPercentageProperty = BindableProperty.Create(
        nameof(NeedleLengthPercentage),
        typeof(float),
        typeof(NoiseGauge),
        1f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty NeedleBackLengthPercentageProperty = BindableProperty.Create(
        nameof(NeedleBackLengthPercentage),
        typeof(float),
        typeof(NoiseGauge),
        0.15f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty FontProperty = BindableProperty.Create(
        nameof(Font),
        typeof(Font),
        typeof(NoiseGauge),
        DefaultFont,
        propertyChanged: Invalidate);

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(float),
        typeof(NoiseGauge),
        18f,
        propertyChanged: Invalidate);

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.White,
        propertyChanged: Invalidate);

    public static readonly BindableProperty GaugeColorProperty = BindableProperty.Create(
        nameof(GaugeColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.White,
        propertyChanged: Invalidate);

    public static readonly BindableProperty WarningGaugeColorProperty = BindableProperty.Create(
        nameof(WarningGaugeColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.Red,
        propertyChanged: Invalidate);

    public static readonly BindableProperty NeedleColorProperty = BindableProperty.Create(
        nameof(NeedleColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.Red,
        propertyChanged: Invalidate);

    public static readonly BindableProperty NeedleCenterColorProperty = BindableProperty.Create(
        nameof(NeedleCenterColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.Red,
        propertyChanged: Invalidate);

    public static readonly BindableProperty PeakNeedleColorProperty = BindableProperty.Create(
        nameof(PeakNeedleColor),
        typeof(Color),
        typeof(NoiseGauge),
        Colors.Gold,
        propertyChanged: Invalidate);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public double Threshold
    {
        get => (double)GetValue(ThresholdProperty);
        set => SetValue(ThresholdProperty, value);
    }

    public double PeakValue
    {
        get => (double)GetValue(PeakValueProperty);
        set => SetValue(PeakValueProperty, value);
    }

    public float BorderMargin
    {
        get => (float)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public float GaugeWidth
    {
        get => (float)GetValue(GaugeWidthProperty);
        set => SetValue(GaugeWidthProperty, value);
    }

    public float TickLength
    {
        get => (float)GetValue(TickLengthProperty);
        set => SetValue(TickLengthProperty, value);
    }

    public float MinorTickLength
    {
        get => (float)GetValue(MinorTickLengthProperty);
        set => SetValue(MinorTickLengthProperty, value);
    }

    public float MajorTickLength
    {
        get => (float)GetValue(MajorTickLengthProperty);
        set => SetValue(MajorTickLengthProperty, value);
    }

    public float TickWidth
    {
        get => (float)GetValue(TickWidthProperty);
        set => SetValue(TickWidthProperty, value);
    }

    public float MinorTickWidth
    {
        get => (float)GetValue(MinorTickWidthProperty);
        set => SetValue(MinorTickWidthProperty, value);
    }

    public float MajorTickWidth
    {
        get => (float)GetValue(MajorTickWidthProperty);
        set => SetValue(MajorTickWidthProperty, value);
    }

    public float TextOffset
    {
        get => (float)GetValue(TextOffsetProperty);
        set => SetValue(TextOffsetProperty, value);
    }

    public float NeedleWidth
    {
        get => (float)GetValue(NeedleWidthProperty);
        set => SetValue(NeedleWidthProperty, value);
    }

    public float PeakNeedleWidth
    {
        get => (float)GetValue(PeakNeedleWidthProperty);
        set => SetValue(PeakNeedleWidthProperty, value);
    }

    public float CenterCircleRadius
    {
        get => (float)GetValue(CenterCircleRadiusProperty);
        set => SetValue(CenterCircleRadiusProperty, value);
    }

    public float NeedleLengthPercentage
    {
        get => (float)GetValue(NeedleLengthPercentageProperty);
        set => SetValue(NeedleLengthPercentageProperty, value);
    }

    public float NeedleBackLengthPercentage
    {
        get => (float)GetValue(NeedleBackLengthPercentageProperty);
        set => SetValue(NeedleBackLengthPercentageProperty, value);
    }

    public Font Font
    {
        get => (Font)GetValue(FontProperty);
        set => SetValue(FontProperty, value);
    }

    public float FontSize
    {
        get => (float)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Color GaugeColor
    {
        get => (Color)GetValue(GaugeColorProperty);
        set => SetValue(GaugeColorProperty, value);
    }

    public Color WarningGaugeColor
    {
        get => (Color)GetValue(WarningGaugeColorProperty);
        set => SetValue(WarningGaugeColorProperty, value);
    }

    public Color NeedleColor
    {
        get => (Color)GetValue(NeedleColorProperty);
        set => SetValue(NeedleColorProperty, value);
    }

    public Color NeedleCenterColor
    {
        get => (Color)GetValue(NeedleCenterColorProperty);
        set => SetValue(NeedleCenterColorProperty, value);
    }

    public Color PeakNeedleColor
    {
        get => (Color)GetValue(PeakNeedleColorProperty);
        set => SetValue(PeakNeedleColorProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public NoiseGauge()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;

        animationTimer.Elapsed += TimerElapsed;
    }

    public void Dispose()
    {
        animationTimer.Dispose();
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
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

        var size = Math.Min(dirtyRect.Width, dirtyRect.Height) - (borderMargin * 2);
        var radius = size / 2;
        var rect = new RectF(dirtyRect.Center.X - radius, borderMargin, size, size);

        var cx = dirtyRect.Center.X;
        var cy = rect.Center.Y;

        canvas.Font = font;
        canvas.FontSize = fontSize;

        canvas.FillColor = BackgroundColor;
        canvas.FillRectangle(dirtyRect);

        canvas.StrokeLineCap = LineCap.Square;
        for (var i = 0; i <= (int)range; i++)
        {
            var angle = (float)((RangeAngle * i / range) - (StartAngle - 90));
            var isMajor = i % MajorTickStep == 0;
            var isMinor = i % MinorTickStep == 0;

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

        canvas.StrokeSize = gaugeWidth;
        canvas.StrokeColor = gaugeColor;
        canvas.DrawArc(rect, StartAngle, EndAngle, true, false);

        var warningStart = StartAngle - (float)(RangeAngle * (Threshold - Min) / range);
        canvas.StrokeColor = warningGaugeColor;
        canvas.DrawArc(rect, warningStart, EndAngle, true, false);

        var needleLengthBase = (radius - MajorTickLength) * NeedleLengthPercentage;
        var peak = PeakValue;
        if (peak > min)
        {
            var peakRadian = (float)((StartAngle - (RangeAngle * (Math.Clamp(peak, min, max) - min) / range)) * MathF.PI / 180);
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeColor = PeakNeedleColor;
            canvas.StrokeSize = PeakNeedleWidth;
            canvas.DrawLine(cx, cy, cx + (needleLengthBase * MathF.Cos(peakRadian)), cy - (needleLengthBase * MathF.Sin(peakRadian)));
        }

        var value = Math.Clamp(Value, min, max);
        var needleRadian = (float)((StartAngle - (RangeAngle * (value - Min) / range)) * MathF.PI / 180);

        var needleLength = needleLengthBase;
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

        var centerCircleRadius = CenterCircleRadius;
        canvas.FillColor = NeedleCenterColor;
        canvas.FillCircle(cx, cy, centerCircleRadius);
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void HandleValueChanged(double oldValue)
    {
        animatedValue = oldValue;
        animationTimer.Start();
    }

    private static void HandleValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((NoiseGauge)bindable).HandleValueChanged((double)oldValue);
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (Math.Abs(animatedValue - Value) < SettleThreshold)
        {
            animationTimer.Stop();
            animatedValue = Value;
        }
        else
        {
            var diff = Value - animatedValue;
            animatedValue += diff * EasingRate;
        }

        Dispatcher.Dispatch(Invalidate);
    }
}
