namespace Template.MobileApp.Controls;

using System.Collections.ObjectModel;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

public enum GaugeUnitPosition
{
    Right,
    Bottom
}

public sealed class GaugeRange
{
    public double StartValue { get; set; }
    public double EndValue { get; set; }

    public Color StartColor { get; set; } = Colors.Transparent;

    public Color? EndColor { get; set; }
}

public sealed class Gauge : SKCanvasView
{
    // ------------------------------------------------------------------ Value
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Gauge), 0.0,
            propertyChanged: OnValueChanged);
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty MinValueProperty =
        BindableProperty.Create(nameof(MinValue), typeof(double), typeof(Gauge), 0.0,
            propertyChanged: OnVisualPropertyChanged);
    public double MinValue
    {
        get => (double)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public static readonly BindableProperty MaxValueProperty =
        BindableProperty.Create(nameof(MaxValue), typeof(double), typeof(Gauge), 100.0,
            propertyChanged: OnVisualPropertyChanged);
    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    // ------------------------------------------------------------------ Geometry
    public static readonly BindableProperty StartAngleProperty =
        BindableProperty.Create(nameof(StartAngle), typeof(float), typeof(Gauge), 225f,
            propertyChanged: OnVisualPropertyChanged);
    public float StartAngle
    {
        get => (float)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public static readonly BindableProperty SweepAngleProperty =
        BindableProperty.Create(nameof(SweepAngle), typeof(float), typeof(Gauge), 270f,
            propertyChanged: OnVisualPropertyChanged);
    public float SweepAngle
    {
        get => (float)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    // ------------------------------------------------------------------ Background
    public static readonly BindableProperty GaugeBackgroundColorProperty =
        BindableProperty.Create(nameof(GaugeBackgroundColor), typeof(Color), typeof(Gauge),
            Colors.Transparent, propertyChanged: OnVisualPropertyChanged);
    public Color GaugeBackgroundColor
    {
        get => (Color)GetValue(GaugeBackgroundColorProperty);
        set => SetValue(GaugeBackgroundColorProperty, value);
    }

    public static readonly BindableProperty GaugeBackgroundColor2Property =
        BindableProperty.Create(nameof(GaugeBackgroundColor2), typeof(Color), typeof(Gauge),
            Colors.Transparent, propertyChanged: OnVisualPropertyChanged);
    public Color GaugeBackgroundColor2
    {
        get => (Color)GetValue(GaugeBackgroundColor2Property);
        set => SetValue(GaugeBackgroundColor2Property, value);
    }

    // ------------------------------------------------------------------ Bezel Outer
    public static readonly BindableProperty BezelOuterThicknessProperty =
        BindableProperty.Create(nameof(BezelOuterThickness), typeof(float), typeof(Gauge), 4f,
            propertyChanged: OnVisualPropertyChanged);
    public float BezelOuterThickness
    {
        get => (float)GetValue(BezelOuterThicknessProperty);
        set => SetValue(BezelOuterThicknessProperty, value);
    }

    public static readonly BindableProperty BezelOuterStartColorProperty =
        BindableProperty.Create(nameof(BezelOuterStartColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#E8EEF8"), propertyChanged: OnVisualPropertyChanged);
    public Color BezelOuterStartColor
    {
        get => (Color)GetValue(BezelOuterStartColorProperty);
        set => SetValue(BezelOuterStartColorProperty, value);
    }

    public static readonly BindableProperty BezelOuterEndColorProperty =
        BindableProperty.Create(nameof(BezelOuterEndColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#6878A0"), propertyChanged: OnVisualPropertyChanged);
    public Color BezelOuterEndColor
    {
        get => (Color)GetValue(BezelOuterEndColorProperty);
        set => SetValue(BezelOuterEndColorProperty, value);
    }

    // ------------------------------------------------------------------ Bezel Inner
    public static readonly BindableProperty BezelInnerThicknessProperty =
        BindableProperty.Create(nameof(BezelInnerThickness), typeof(float), typeof(Gauge), 2f,
            propertyChanged: OnVisualPropertyChanged);
    public float BezelInnerThickness
    {
        get => (float)GetValue(BezelInnerThicknessProperty);
        set => SetValue(BezelInnerThicknessProperty, value);
    }

    public static readonly BindableProperty BezelInnerStartColorProperty =
        BindableProperty.Create(nameof(BezelInnerStartColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#4A5A88"), propertyChanged: OnVisualPropertyChanged);
    public Color BezelInnerStartColor
    {
        get => (Color)GetValue(BezelInnerStartColorProperty);
        set => SetValue(BezelInnerStartColorProperty, value);
    }

    public static readonly BindableProperty BezelInnerEndColorProperty =
        BindableProperty.Create(nameof(BezelInnerEndColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#C0D0E8"), propertyChanged: OnVisualPropertyChanged);
    public Color BezelInnerEndColor
    {
        get => (Color)GetValue(BezelInnerEndColorProperty);
        set => SetValue(BezelInnerEndColorProperty, value);
    }

    // ------------------------------------------------------------------ Arc
    public static readonly BindableProperty ArcInnerExtentProperty =
        BindableProperty.Create(nameof(ArcInnerExtent), typeof(float), typeof(Gauge), 0.77f,
            propertyChanged: OnVisualPropertyChanged);
    public float ArcInnerExtent
    {
        get => (float)GetValue(ArcInnerExtentProperty);
        set => SetValue(ArcInnerExtentProperty, value);
    }

    public static readonly BindableProperty ArcOuterExtentProperty =
        BindableProperty.Create(nameof(ArcOuterExtent), typeof(float), typeof(Gauge), 0.87f,
            propertyChanged: OnVisualPropertyChanged);
    public float ArcOuterExtent
    {
        get => (float)GetValue(ArcOuterExtentProperty);
        set => SetValue(ArcOuterExtentProperty, value);
    }

    public static readonly BindableProperty ArcTrackColorProperty =
        BindableProperty.Create(nameof(ArcTrackColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#20000000"), propertyChanged: OnVisualPropertyChanged);
    public Color ArcTrackColor
    {
        get => (Color)GetValue(ArcTrackColorProperty);
        set => SetValue(ArcTrackColorProperty, value);
    }

    public static readonly BindableProperty ArcStartColorProperty =
        BindableProperty.Create(nameof(ArcStartColor), typeof(Color), typeof(Gauge),
            Colors.DodgerBlue, propertyChanged: OnVisualPropertyChanged);
    public Color ArcStartColor
    {
        get => (Color)GetValue(ArcStartColorProperty);
        set => SetValue(ArcStartColorProperty, value);
    }

    public static readonly BindableProperty ArcEndColorProperty =
        BindableProperty.Create(nameof(ArcEndColor), typeof(Color), typeof(Gauge),
            Colors.DodgerBlue, propertyChanged: OnVisualPropertyChanged);
    public Color ArcEndColor
    {
        get => (Color)GetValue(ArcEndColorProperty);
        set => SetValue(ArcEndColorProperty, value);
    }

    // ------------------------------------------------------------------ Major Tick
    public static readonly BindableProperty MajorTickIntervalProperty =
        BindableProperty.Create(nameof(MajorTickInterval), typeof(double), typeof(Gauge), 10.0,
            propertyChanged: OnVisualPropertyChanged);
    public double MajorTickInterval
    {
        get => (double)GetValue(MajorTickIntervalProperty);
        set => SetValue(MajorTickIntervalProperty, value);
    }

    public static readonly BindableProperty MajorTickInnerExtentProperty =
        BindableProperty.Create(nameof(MajorTickInnerExtent), typeof(float), typeof(Gauge), 0.68f,
            propertyChanged: OnVisualPropertyChanged);
    public float MajorTickInnerExtent
    {
        get => (float)GetValue(MajorTickInnerExtentProperty);
        set => SetValue(MajorTickInnerExtentProperty, value);
    }

    public static readonly BindableProperty MajorTickOuterExtentProperty =
        BindableProperty.Create(nameof(MajorTickOuterExtent), typeof(float), typeof(Gauge), 0.76f,
            propertyChanged: OnVisualPropertyChanged);
    public float MajorTickOuterExtent
    {
        get => (float)GetValue(MajorTickOuterExtentProperty);
        set => SetValue(MajorTickOuterExtentProperty, value);
    }

    public static readonly BindableProperty MajorTickThicknessProperty =
        BindableProperty.Create(nameof(MajorTickThickness), typeof(float), typeof(Gauge), 2f,
            propertyChanged: OnVisualPropertyChanged);
    public float MajorTickThickness
    {
        get => (float)GetValue(MajorTickThicknessProperty);
        set => SetValue(MajorTickThicknessProperty, value);
    }

    public static readonly BindableProperty MajorTickColorProperty =
        BindableProperty.Create(nameof(MajorTickColor), typeof(Color), typeof(Gauge),
            Colors.Black, propertyChanged: OnVisualPropertyChanged);
    public Color MajorTickColor
    {
        get => (Color)GetValue(MajorTickColorProperty);
        set => SetValue(MajorTickColorProperty, value);
    }

    // ------------------------------------------------------------------ Minor Tick
    public static readonly BindableProperty MinorTickIntervalProperty =
        BindableProperty.Create(nameof(MinorTickInterval), typeof(double), typeof(Gauge), 2.0,
            propertyChanged: OnVisualPropertyChanged);
    public double MinorTickInterval
    {
        get => (double)GetValue(MinorTickIntervalProperty);
        set => SetValue(MinorTickIntervalProperty, value);
    }

    public static readonly BindableProperty MinorTickInnerExtentProperty =
        BindableProperty.Create(nameof(MinorTickInnerExtent), typeof(float), typeof(Gauge), 0.72f,
            propertyChanged: OnVisualPropertyChanged);
    public float MinorTickInnerExtent
    {
        get => (float)GetValue(MinorTickInnerExtentProperty);
        set => SetValue(MinorTickInnerExtentProperty, value);
    }

    public static readonly BindableProperty MinorTickOuterExtentProperty =
        BindableProperty.Create(nameof(MinorTickOuterExtent), typeof(float), typeof(Gauge), 0.76f,
            propertyChanged: OnVisualPropertyChanged);
    public float MinorTickOuterExtent
    {
        get => (float)GetValue(MinorTickOuterExtentProperty);
        set => SetValue(MinorTickOuterExtentProperty, value);
    }

    public static readonly BindableProperty MinorTickThicknessProperty =
        BindableProperty.Create(nameof(MinorTickThickness), typeof(float), typeof(Gauge), 1f,
            propertyChanged: OnVisualPropertyChanged);
    public float MinorTickThickness
    {
        get => (float)GetValue(MinorTickThicknessProperty);
        set => SetValue(MinorTickThicknessProperty, value);
    }

    public static readonly BindableProperty MinorTickColorProperty =
        BindableProperty.Create(nameof(MinorTickColor), typeof(Color), typeof(Gauge),
            Colors.DimGray, propertyChanged: OnVisualPropertyChanged);
    public Color MinorTickColor
    {
        get => (Color)GetValue(MinorTickColorProperty);
        set => SetValue(MinorTickColorProperty, value);
    }

    // ------------------------------------------------------------------ Label
    public static readonly BindableProperty LabelIntervalProperty =
        BindableProperty.Create(nameof(LabelInterval), typeof(double), typeof(Gauge), 20.0,
            propertyChanged: OnVisualPropertyChanged);
    public double LabelInterval
    {
        get => (double)GetValue(LabelIntervalProperty);
        set => SetValue(LabelIntervalProperty, value);
    }

    public static readonly BindableProperty LabelExtentProperty =
        BindableProperty.Create(nameof(LabelExtent), typeof(float), typeof(Gauge), 0.57f,
            propertyChanged: OnVisualPropertyChanged);
    public float LabelExtent
    {
        get => (float)GetValue(LabelExtentProperty);
        set => SetValue(LabelExtentProperty, value);
    }

    public static readonly BindableProperty LabelFormatProperty =
        BindableProperty.Create(nameof(LabelFormat), typeof(string), typeof(Gauge), "{0:N0}",
            propertyChanged: OnVisualPropertyChanged);
    public string LabelFormat
    {
        get => (string)GetValue(LabelFormatProperty);
        set => SetValue(LabelFormatProperty, value);
    }

    public static readonly BindableProperty LabelConverterProperty =
        BindableProperty.Create(nameof(LabelConverter), typeof(IValueConverter), typeof(Gauge),
            propertyChanged: OnVisualPropertyChanged);
    public IValueConverter? LabelConverter
    {
        get => (IValueConverter?)GetValue(LabelConverterProperty);
        set => SetValue(LabelConverterProperty, value);
    }

    public static readonly BindableProperty LabelFontSizeProperty =
        BindableProperty.Create(nameof(LabelFontSize), typeof(float), typeof(Gauge), 12f,
            propertyChanged: OnVisualPropertyChanged);
    public float LabelFontSize
    {
        get => (float)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public static readonly BindableProperty LabelColorProperty =
        BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(Gauge),
            Colors.Black, propertyChanged: OnVisualPropertyChanged);
    public Color LabelColor
    {
        get => (Color)GetValue(LabelColorProperty);
        set => SetValue(LabelColorProperty, value);
    }

    // ------------------------------------------------------------------ Center Value / Unit
    public static readonly BindableProperty ShowValueProperty =
        BindableProperty.Create(nameof(ShowValue), typeof(bool), typeof(Gauge), true,
            propertyChanged: OnVisualPropertyChanged);
    public bool ShowValue
    {
        get => (bool)GetValue(ShowValueProperty);
        set => SetValue(ShowValueProperty, value);
    }

    public static readonly BindableProperty ValueFormatProperty =
        BindableProperty.Create(nameof(ValueFormat), typeof(string), typeof(Gauge), "{0:N1}",
            propertyChanged: OnVisualPropertyChanged);
    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    public static readonly BindableProperty ValueFontSizeProperty =
        BindableProperty.Create(nameof(ValueFontSize), typeof(float), typeof(Gauge), 28f,
            propertyChanged: OnVisualPropertyChanged);
    public float ValueFontSize
    {
        get => (float)GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    public static readonly BindableProperty ValueColorProperty =
        BindableProperty.Create(nameof(ValueColor), typeof(Color), typeof(Gauge),
            Colors.Black, propertyChanged: OnVisualPropertyChanged);
    public Color ValueColor
    {
        get => (Color)GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    public static readonly BindableProperty UnitProperty =
        BindableProperty.Create(nameof(Unit), typeof(string), typeof(Gauge),
            propertyChanged: OnVisualPropertyChanged);
    public string? Unit
    {
        get => (string?)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly BindableProperty UnitFontSizeProperty =
        BindableProperty.Create(nameof(UnitFontSize), typeof(float), typeof(Gauge), 12f,
            propertyChanged: OnVisualPropertyChanged);
    public float UnitFontSize
    {
        get => (float)GetValue(UnitFontSizeProperty);
        set => SetValue(UnitFontSizeProperty, value);
    }

    public static readonly BindableProperty UnitColorProperty =
        BindableProperty.Create(nameof(UnitColor), typeof(Color), typeof(Gauge),
            Colors.DimGray, propertyChanged: OnVisualPropertyChanged);
    public Color UnitColor
    {
        get => (Color)GetValue(UnitColorProperty);
        set => SetValue(UnitColorProperty, value);
    }

    public static readonly BindableProperty UnitPositionProperty =
        BindableProperty.Create(nameof(UnitPosition), typeof(GaugeUnitPosition), typeof(Gauge),
            GaugeUnitPosition.Bottom, propertyChanged: OnVisualPropertyChanged);
    public GaugeUnitPosition UnitPosition
    {
        get => (GaugeUnitPosition)GetValue(UnitPositionProperty);
        set => SetValue(UnitPositionProperty, value);
    }

    public static readonly BindableProperty ValueOffsetYProperty =
        BindableProperty.Create(nameof(ValueOffsetY), typeof(float), typeof(Gauge), 0.3f,
            propertyChanged: OnVisualPropertyChanged);
    public float ValueOffsetY
    {
        get => (float)GetValue(ValueOffsetYProperty);
        set => SetValue(ValueOffsetYProperty, value);
    }

    // ------------------------------------------------------------------ Needle / Pivot
    public static readonly BindableProperty NeedleEndExtentProperty =
        BindableProperty.Create(nameof(NeedleEndExtent), typeof(float), typeof(Gauge), 0.75f,
            propertyChanged: OnVisualPropertyChanged);
    public float NeedleEndExtent
    {
        get => (float)GetValue(NeedleEndExtentProperty);
        set => SetValue(NeedleEndExtentProperty, value);
    }

    public static readonly BindableProperty NeedleStartWidthProperty =
        BindableProperty.Create(nameof(NeedleStartWidth), typeof(float), typeof(Gauge), 10f,
            propertyChanged: OnVisualPropertyChanged);
    public float NeedleStartWidth
    {
        get => (float)GetValue(NeedleStartWidthProperty);
        set => SetValue(NeedleStartWidthProperty, value);
    }

    public static readonly BindableProperty NeedleEndWidthProperty =
        BindableProperty.Create(nameof(NeedleEndWidth), typeof(float), typeof(Gauge), 2f,
            propertyChanged: OnVisualPropertyChanged);
    public float NeedleEndWidth
    {
        get => (float)GetValue(NeedleEndWidthProperty);
        set => SetValue(NeedleEndWidthProperty, value);
    }

    public static readonly BindableProperty NeedleLeftColorProperty =
        BindableProperty.Create(nameof(NeedleLeftColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#CC2828"), propertyChanged: OnVisualPropertyChanged);
    public Color NeedleLeftColor
    {
        get => (Color)GetValue(NeedleLeftColorProperty);
        set => SetValue(NeedleLeftColorProperty, value);
    }

    public static readonly BindableProperty NeedleRightColorProperty =
        BindableProperty.Create(nameof(NeedleRightColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#EE3C3C"), propertyChanged: OnVisualPropertyChanged);
    public Color NeedleRightColor
    {
        get => (Color)GetValue(NeedleRightColorProperty);
        set => SetValue(NeedleRightColorProperty, value);
    }

    public static readonly BindableProperty PivotRadiusProperty =
        BindableProperty.Create(nameof(PivotRadius), typeof(float), typeof(Gauge), 8f,
            propertyChanged: OnVisualPropertyChanged);
    public float PivotRadius
    {
        get => (float)GetValue(PivotRadiusProperty);
        set => SetValue(PivotRadiusProperty, value);
    }

    public static readonly BindableProperty PivotColorProperty =
        BindableProperty.Create(nameof(PivotColor), typeof(Color), typeof(Gauge),
            Color.FromArgb("#CC2828"), propertyChanged: OnVisualPropertyChanged);
    public Color PivotColor
    {
        get => (Color)GetValue(PivotColorProperty);
        set => SetValue(PivotColorProperty, value);
    }

    // ------------------------------------------------------------------ Animation
    public static readonly BindableProperty AnimationDurationProperty =
        BindableProperty.Create(nameof(AnimationDuration), typeof(int), typeof(Gauge), 300);
    public int AnimationDuration
    {
        get => (int)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly BindableProperty AnimationEasingProperty =
        BindableProperty.Create(nameof(AnimationEasing), typeof(Easing), typeof(Gauge),
            Easing.CubicOut);
    public Easing AnimationEasing
    {
        get => (Easing)GetValue(AnimationEasingProperty);
        set => SetValue(AnimationEasingProperty, value);
    }

    // ------------------------------------------------------------------ Internal state
    private double displayValue;
    private bool displayValueInitialized;

    // ------------------------------------------------------------------ Ranges
    public static readonly BindableProperty RangesProperty =
        BindableProperty.Create(nameof(Ranges), typeof(IList<GaugeRange>), typeof(Gauge),
            defaultValueCreator: _ => new ObservableCollection<GaugeRange>(),
            propertyChanged: OnRangesChanged);

#pragma warning disable CA2227
    public IList<GaugeRange> Ranges
    {
        get => (IList<GaugeRange>)GetValue(RangesProperty);
        set => SetValue(RangesProperty, value);
    }
#pragma warning restore CA2227

    private static void OnRangesChanged(BindableObject b, object oldValue, object newValue)
    {
        var gauge = (Gauge)b;
        if (oldValue is ObservableCollection<GaugeRange> oldCol)
        {
            oldCol.CollectionChanged -= gauge.OnRangesCollectionChanged;
        }

        if (newValue is ObservableCollection<GaugeRange> newCol)
        {
            newCol.CollectionChanged += gauge.OnRangesCollectionChanged;
        }

        gauge.InvalidateSurface();
    }

    private void OnRangesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => InvalidateSurface();

    public Func<double, string>? LabelFormatter
    {
        get;
        set
        {
            field = value;
            InvalidateSurface();
        }
    }

    // ------------------------------------------------------------------ Constructor
    public Gauge()
    {
        if (Ranges is ObservableCollection<GaugeRange> col)
        {
            col.CollectionChanged += OnRangesCollectionChanged;
        }
    }

    // ------------------------------------------------------------------ Property changed callbacks
    private static void OnVisualPropertyChanged(BindableObject b, object o, object n)
        => ((Gauge)b).InvalidateSurface();

    private static void OnValueChanged(BindableObject b, object oldVal, object newVal)
    {
        var gauge = (Gauge)b;
        var to = (double)newVal;

        if (!gauge.displayValueInitialized)
        {
            gauge.displayValueInitialized = true;
            gauge.displayValue = to;
            gauge.InvalidateSurface();
            return;
        }

        if ((gauge.AnimationDuration <= 0) || (gauge.Handler is null))
        {
            gauge.displayValue = to;
            gauge.InvalidateSurface();
            return;
        }

        var from = gauge.displayValue;
        gauge.AbortAnimation("GaugeValue");
#pragma warning disable CA2000
        var anim = new Animation(v =>
        {
            gauge.displayValue = v;
            gauge.InvalidateSurface();
        }, from, to, gauge.AnimationEasing);
#pragma warning restore CA2000
        anim.Commit(gauge, "GaugeValue", length: (uint)gauge.AnimationDuration);
    }

    // ------------------------------------------------------------------ Paint
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        // サイズまたは値域が無効なら描画しない
        if ((info.Width <= 0) || (info.Height <= 0) || (MaxValue <= MinValue))
        {
            return;
        }

        // 物理ピクセルと論理ピクセルの比率(画面密度)を求める
        // SKFont のサイズは物理ピクセル単位のため、プロパティ値(論理単位)に density を乗算して渡す
        var density = (float)(info.Width / Width);

        // 中心座標と利用可能な最大半径を算出する
        var cx = info.Width / 2f;
        var cy = info.Height / 2f;
        var center = new SKPoint(cx, cy);

        // ベゼル分を差し引いた描画半径を求める
        var maxR = MathF.Min(cx, cy);
        var bezelTotal = BezelOuterThickness + BezelInnerThickness;
        var r = maxR - bezelTotal;
        if (r <= 0)
        {
            return;
        }

        // アーク中心半径・太さ・バウンディング矩形を算出する
        var arcCenterR = r * ((ArcInnerExtent + ArcOuterExtent) / 2f);
        var arcThickness = r * (ArcOuterExtent - ArcInnerExtent);
        var arcRect = new SKRect(cx - arcCenterR, cy - arcCenterR, cx + arcCenterR, cy + arcCenterR);

        // 背景円・ベゼルリングを描画する
        DrawBackground(canvas, center, r);
        DrawBezel(canvas, center, maxR);

        // アークのトラック(溝)を描画し、その上に値アークまたはレンジアークを重ねる
        DrawArcTrack(canvas, arcRect, arcThickness);
        if (Ranges.Count == 0)
        {
            // レンジ未設定: 全体をグラデーションアークで塗る
            DrawArcFill(canvas, center, arcRect, arcThickness);
        }
        else
        {
            // レンジ設定あり: 各レンジのカラーセグメントを描画する
            DrawRanges(canvas, center, arcRect, arcThickness);
        }

        // マイナー目盛り → メジャー目盛り → ラベルの順に描画する
        DrawTicks(canvas, center, r, false);
        DrawTicks(canvas, center, r, true);
        DrawLabels(canvas, center, r, density);

        // 中央の数値表示・針・ピボットを描画する
        DrawCenterValue(canvas, center, r, density);
        DrawNeedle(canvas, center, r);
        DrawPivot(canvas, center);
    }

    // ------------------------------------------------------------------ Drawing
    // ------------------------------------------------------------------ DrawBackground
    private void DrawBackground(SKCanvas canvas, SKPoint center, float r)
    {
        // グラデーション背景か単色背景かを判定する
        var useGradient = GaugeBackgroundColor2.Alpha > 0f;

        // 両色とも透明なら描画をスキップする
        if ((!useGradient) && (GaugeBackgroundColor.Alpha <= 0f))
        {
            return;
        }

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;

        // ベゼルとの隙間をなくすため半径を 1.5px 拡張する
        var drawR = r + 1.5f;
        if (useGradient)
        {
            // 左上から右下への線形グラデーションシェーダーを設定する
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(center.X - drawR, center.Y - drawR),
                new SKPoint(center.X + drawR, center.Y + drawR),
                [GaugeBackgroundColor.ToSKColor(), GaugeBackgroundColor2.ToSKColor()],
                null,
                SKShaderTileMode.Clamp);
            paint.Shader = shader;
        }
        else
        {
            // 単色で塗りつぶす
            paint.Color = GaugeBackgroundColor.ToSKColor();
        }

        canvas.DrawCircle(center, drawR, paint);
    }

    // ------------------------------------------------------------------ DrawBezel
    private void DrawBezel(SKCanvas canvas, SKPoint center, float maxR)
    {
        var cx = center.X;
        var cy = center.Y;

        // 外側ベゼルリングをグラデーション線で描画する
        if (BezelOuterThickness > 0)
        {
            // ストローク中心が枠の内側に収まるよう半径を調整する
            var outerR = maxR - (BezelOuterThickness / 2f);
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(cx - maxR, cy - maxR),
                new SKPoint(cx + maxR, cy + maxR),
                [BezelOuterStartColor.ToSKColor(), BezelOuterEndColor.ToSKColor()],
                null,
                SKShaderTileMode.Clamp);
            using var paint = new SKPaint();
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = BezelOuterThickness;
            paint.IsAntialias = true;
            paint.Shader = shader;
            canvas.DrawCircle(cx, cy, outerR, paint);
        }

        // 内側ベゼルリングをグラデーション線で描画する
        if (BezelInnerThickness > 0)
        {
            // 外側ベゼル分をオフセットしてストローク中心を計算する
            var innerR = maxR - BezelOuterThickness - (BezelInnerThickness / 2f);
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(cx - innerR, cy - innerR),
                new SKPoint(cx + innerR, cy + innerR),
                [BezelInnerStartColor.ToSKColor(), BezelInnerEndColor.ToSKColor()],
                null,
                SKShaderTileMode.Clamp);
            using var paint = new SKPaint();
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = BezelInnerThickness;
            paint.IsAntialias = true;
            paint.Shader = shader;
            canvas.DrawCircle(cx, cy, innerR, paint);
        }
    }

    // ------------------------------------------------------------------ DrawArcTrack
    private void DrawArcTrack(SKCanvas canvas, SKRect arcRect, float arcThickness)
    {
        // トラック色が透明なら描画をスキップする
        if (ArcTrackColor.Alpha <= 0f)
        {
            return;
        }

        // アーク全体を単色で塗りつぶしてトラック(溝)を表現する
        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = arcThickness;
        paint.IsAntialias = true;
        paint.StrokeCap = SKStrokeCap.Butt;
        paint.Color = ArcTrackColor.ToSKColor();
        using var builder = new SKPathBuilder();
        builder.AddArc(arcRect, ToSkiaAngle(StartAngle), SweepAngle);
        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }

    // ------------------------------------------------------------------ DrawArcFill
    private void DrawArcFill(SKCanvas canvas, SKPoint center, SKRect arcRect, float arcThickness)
    {
        if (SweepAngle <= 0)
        {
            return;
        }

        // スイープグラデーションを生成し、StartAngle 基準で回転させる
        // SKShader.CreateSweepGradient は 3 時方向を 0° とするため -90° 補正が必要
        using var baseShader = SKShader.CreateSweepGradient(
            center,
            [ArcStartColor.ToSKColor(), ArcEndColor.ToSKColor()],
            [0f, 1f],
            SKShaderTileMode.Clamp,
            0f,
            SweepAngle);
        using var shader = baseShader.WithLocalMatrix(SKMatrix.CreateRotationDegrees(StartAngle - 90f, center.X, center.Y));

        // グラデーションアークを描画する
        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = arcThickness;
        paint.IsAntialias = true;
        paint.StrokeCap = SKStrokeCap.Butt;
        paint.Shader = shader;
        using var builder = new SKPathBuilder();
        builder.AddArc(arcRect, ToSkiaAngle(StartAngle), SweepAngle);
        using var path = builder.Detach();
        canvas.DrawPath(path, paint);
    }

    // ------------------------------------------------------------------ DrawRanges
    private void DrawRanges(SKCanvas canvas, SKPoint center, SKRect arcRect, float arcThickness)
    {
        foreach (var range in Ranges)
        {
            // レンジの開始・終了値を有効値域内にクランプする
            var clampedStart = Math.Clamp(range.StartValue, MinValue, MaxValue);
            var clampedEnd = Math.Clamp(range.EndValue, MinValue, MaxValue);
            if (clampedStart >= clampedEnd)
            {
                continue;
            }

            // 値をユーザー角度(StartAngle 基準)に変換し、このレンジのスイープ角を求める
            var startUserAngle = ValueToUserAngle(clampedStart);
            var endUserAngle = ValueToUserAngle(clampedEnd);
            var sweepDeg = endUserAngle - startUserAngle;
            if (sweepDeg <= 0)
            {
                continue;
            }

            // レンジの開始角に合わせて回転させたスイープグラデーションを生成する
            // EndColor が未設定の場合は StartColor で単色グラデーションにする
            using var baseShader = SKShader.CreateSweepGradient(
                center,
                [range.StartColor.ToSKColor(), (range.EndColor ?? range.StartColor).ToSKColor()],
                [0f, 1f],
                SKShaderTileMode.Clamp,
                0f,
                sweepDeg);
            using var shader = baseShader.WithLocalMatrix(SKMatrix.CreateRotationDegrees(startUserAngle - 90f, center.X, center.Y));

            // レンジのカラーセグメントをアーク上に描画する
            using var paint = new SKPaint();
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = arcThickness;
            paint.IsAntialias = true;
            paint.StrokeCap = SKStrokeCap.Butt;
            paint.Shader = shader;
            using var builder = new SKPathBuilder();
            builder.AddArc(arcRect, ToSkiaAngle(startUserAngle), sweepDeg);
            using var path = builder.Detach();
            canvas.DrawPath(path, paint);
        }
    }

    // ------------------------------------------------------------------ DrawTicks
    private void DrawTicks(SKCanvas canvas, SKPoint center, float r, bool major)
    {
        // major フラグに応じてメジャー/マイナー目盛りのパラメーターを選択する
        var interval = major ? MajorTickInterval : MinorTickInterval;
        if (interval <= 0)
        {
            return;
        }

        var innerExtent = major ? MajorTickInnerExtent : MinorTickInnerExtent;
        var outerExtent = major ? MajorTickOuterExtent : MinorTickOuterExtent;
        var thickness = major ? MajorTickThickness : MinorTickThickness;
        var color = major ? MajorTickColor : MinorTickColor;

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = thickness;
        paint.IsAntialias = true;
        paint.Color = color.ToSKColor();
        paint.StrokeCap = SKStrokeCap.Butt;

        // 値域全体を interval で割った本数分だけ目盛りを描画する
        var tickCount = (int)Math.Round((MaxValue - MinValue) / interval);

        for (var i = 0; i <= tickCount; i++)
        {
            var value = MinValue + (i * interval);

            // 浮動小数点の誤差を許容しつつ MaxValue を超えた場合は打ち切る
            if (value > MaxValue + (interval * 0.001))
            {
                break;
            }

            // 最終目盛りが MaxValue を超えないようにクランプする
            value = Math.Min(value, MaxValue);

            // 値を Skia 角度(ラジアン)に変換し、目盛りの内外端座標を計算して線を引く
            var skiaAngle = ToSkiaAngle(ValueToUserAngle(value));
            var rad = MathF.PI * skiaAngle / 180f;
            var cos = MathF.Cos(rad);
            var sin = MathF.Sin(rad);

            canvas.DrawLine(
                center.X + (r * innerExtent * cos),
                center.Y + (r * innerExtent * sin),
                center.X + (r * outerExtent * cos),
                center.Y + (r * outerExtent * sin),
                paint);
        }
    }

    // ------------------------------------------------------------------ DrawLabels
    private void DrawLabels(SKCanvas canvas, SKPoint center, float r, float density)
    {
        if (LabelInterval <= 0)
        {
            return;
        }

        using var font = new SKFont(SKTypeface.Default, LabelFontSize * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = LabelColor.ToSKColor();

        // フォントのアセント・ディセントの中点を求め、テキストを垂直方向に中央揃えする基準にする
        var metrics = font.Metrics;
        var midMetrics = (metrics.Ascent + metrics.Descent) / 2f;

        var labelCount = (int)Math.Round((MaxValue - MinValue) / LabelInterval);

        for (var i = 0; i <= labelCount; i++)
        {
            var value = MinValue + (i * LabelInterval);

            // 浮動小数点の誤差を許容しつつ MaxValue を超えた場合は打ち切る
            if (value > MaxValue + (LabelInterval * 0.001))
            {
                break;
            }

            value = Math.Min(value, MaxValue);

            // LabelFormatter → LabelConverter → 書式文字列の優先順でラベル文字列を決定する
            var text = LabelFormatter is not null
                ? LabelFormatter(value)
                : LabelConverter is not null
                    ? LabelConverter.Convert(value, typeof(string), null, CultureInfo.CurrentCulture) as string ?? string.Empty
                    : String.Format(LabelFormat, value);

            if (String.IsNullOrEmpty(text))
            {
                continue;
            }

            // 値を角度に変換し、LabelExtent 比率の位置座標を計算する
            var skiaAngle = ToSkiaAngle(ValueToUserAngle(value));
            var rad = MathF.PI * skiaAngle / 180f;

            var x = center.X + (r * LabelExtent * MathF.Cos(rad));
            var y = center.Y + (r * LabelExtent * MathF.Sin(rad));

            // テキスト幅を測定して水平方向に中央揃えで描画する
            var textWidth = font.MeasureText(text);
            canvas.DrawText(text, x - (textWidth / 2f), y - midMetrics, SKTextAlign.Left, font, paint);
        }
    }

    // ------------------------------------------------------------------ DrawCenterValue
    private void DrawCenterValue(SKCanvas canvas, SKPoint center, float r, float density)
    {
        if (!ShowValue)
        {
            return;
        }

        // 中央値表示の垂直基準位置を ValueOffsetY で調整する
        var baseCy = center.Y + (r * ValueOffsetY);
        var valueText = String.Format(ValueFormat, displayValue);
        var hasUnit = !String.IsNullOrEmpty(Unit);

        using var valueFont = new SKFont(SKTypeface.Default, ValueFontSize * density);
        using var valuePaint = new SKPaint();
        valuePaint.IsAntialias = true;
        valuePaint.Color = ValueColor.ToSKColor();
        var vm = valueFont.Metrics;

        // 単位なし: 数値のみを水平中央に描画する
        if (!hasUnit)
        {
            var vw = valueFont.MeasureText(valueText);
            canvas.DrawText(valueText,
                center.X - (vw / 2f),
                baseCy - ((vm.Ascent + vm.Descent) / 2f),
                SKTextAlign.Left, valueFont, valuePaint);
            return;
        }

        using var unitFont = new SKFont(SKTypeface.Default, UnitFontSize * density);
        using var unitPaint = new SKPaint();
        unitPaint.IsAntialias = true;
        unitPaint.Color = UnitColor.ToSKColor();
        var um = unitFont.Metrics;

        if (UnitPosition == GaugeUnitPosition.Bottom)
        {
            // 単位下置き: 数値と単位を縦に並べ、合計高さを中央に揃える
            var valueH = vm.Descent - vm.Ascent;
            var unitH = um.Descent - um.Ascent;
            var totalH = valueH + 2f + unitH;

            var y1 = baseCy - (totalH / 2f);
            var valueBaseline = y1 - vm.Ascent;
            var unitBaseline = y1 + valueH + 2f - um.Ascent;

            var vw = valueFont.MeasureText(valueText);
            canvas.DrawText(valueText, center.X - (vw / 2f), valueBaseline, SKTextAlign.Left, valueFont, valuePaint);

            var uw = unitFont.MeasureText(Unit);
            canvas.DrawText(Unit, center.X - (uw / 2f), unitBaseline, SKTextAlign.Left, unitFont, unitPaint);
        }
        else
        {
            // 単位右置き: 数値と単位を横に並べ、合計幅を中央に揃える
            var vw = valueFont.MeasureText(valueText);
            var uw = unitFont.MeasureText(Unit);
            var totalW = vw + 4f + uw;
            var startX = center.X - (totalW / 2f);

            canvas.DrawText(valueText, startX, baseCy - ((vm.Ascent + vm.Descent) / 2f), SKTextAlign.Left, valueFont, valuePaint);
            canvas.DrawText(Unit, startX + vw + 4f, baseCy - ((um.Ascent + um.Descent) / 2f), SKTextAlign.Left, unitFont, unitPaint);
        }
    }

    // ------------------------------------------------------------------ DrawNeedle
    private void DrawNeedle(SKCanvas canvas, SKPoint center, float r)
    {
        // 現在値を値域にクランプし、ユーザー角度に変換する
        var clamped = (float)Math.Clamp(displayValue, MinValue, MaxValue);
        var userAngle = ValueToUserAngle(clamped);

        // 針の根元・先端の半径と幅を算出する
        var startR = 0f;
        var endR = r * NeedleEndExtent;
        var startHalfW = NeedleStartWidth / 2f;
        var endHalfW = NeedleEndWidth / 2f;

        // キャンバスを中心へ移動し、指示角度だけ回転させて描画後に復元する
        canvas.Save();
        canvas.Translate(center.X, center.Y);
        canvas.RotateDegrees(userAngle);

        // 左半面(影側)を台形パスで描画する
        using var leftBuilder = new SKPathBuilder();
        leftBuilder.MoveTo(-startHalfW, -startR);
        leftBuilder.LineTo(-endHalfW, -endR);
        leftBuilder.LineTo(0f, -endR);
        leftBuilder.LineTo(0f, -startR);
        leftBuilder.Close();
        using var leftPath = leftBuilder.Detach();

        using var leftPaint = new SKPaint();
        leftPaint.Style = SKPaintStyle.Fill;
        leftPaint.Color = NeedleLeftColor.ToSKColor();
        leftPaint.IsAntialias = true;
        canvas.DrawPath(leftPath, leftPaint);

        // 右半面(ハイライト側)を台形パスで描画する
        using var rightBuilder = new SKPathBuilder();
        rightBuilder.MoveTo(0f, -startR);
        rightBuilder.LineTo(0f, -endR);
        rightBuilder.LineTo(endHalfW, -endR);
        rightBuilder.LineTo(startHalfW, -startR);
        rightBuilder.Close();
        using var rightPath = rightBuilder.Detach();

        using var rightPaint = new SKPaint();
        rightPaint.Style = SKPaintStyle.Fill;
        rightPaint.Color = NeedleRightColor.ToSKColor();
        rightPaint.IsAntialias = true;
        canvas.DrawPath(rightPath, rightPaint);

        canvas.Restore();
    }

    // ------------------------------------------------------------------ DrawPivot
    private void DrawPivot(SKCanvas canvas, SKPoint center)
    {
        // ベースカラーより明るいハイライト色を計算する
        var pivotColor = PivotColor.ToSKColor();
        var highlight = new SKColor(
            (byte)Math.Min(pivotColor.Red + 80, 255),
            (byte)Math.Min(pivotColor.Green + 50, 255),
            (byte)Math.Min(pivotColor.Blue + 40, 255));

        // 左上方向にオフセットした点をハイライトの中心とし、立体感を演出する
        var hiCenter = new SKPoint(
            center.X - (PivotRadius * 0.25f),
            center.Y - (PivotRadius * 0.25f));

        // ハイライト中心から外縁に向けた放射グラデーションで球体感を出す
        using var shader = SKShader.CreateRadialGradient(
            hiCenter,
            PivotRadius * 1.1f,
            [highlight, pivotColor],
            null,
            SKShaderTileMode.Clamp);

        using var paint = new SKPaint();
        paint.Style = SKPaintStyle.Fill;
        paint.IsAntialias = true;
        paint.Shader = shader;
        canvas.DrawCircle(center, PivotRadius, paint);
    }

    // ------------------------------------------------------------------ Helpers
    private float ValueToUserAngle(double value)
    {
        // 値を [0, 1] の比率に正規化し、StartAngle を基点に SweepAngle を按分した角度を返す
        var fraction = (value - MinValue) / (MaxValue - MinValue);
        return StartAngle + (float)(fraction * SweepAngle);
    }

    // SkiaSharp の角度系(3 時方向が 0°)に合わせるため -90° 補正する
    private static float ToSkiaAngle(float userAngle) => userAngle - 90f;
}
