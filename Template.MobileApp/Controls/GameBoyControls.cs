namespace Template.MobileApp.Controls;

using GonFox.GameBoy.Core.Devices;
using GonFox.GameBoy.Core.Video;
using GonFox.GameBoy.Platform;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.App;

//--------------------------------------------------------------------------------
// Screen
//--------------------------------------------------------------------------------

public sealed class GameBoyScreen : SKCanvasView
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(8);

    private static readonly SKColor BezelColor = SKColor.Parse("#202429");

    private static readonly SKImageInfo FrameInfo = new(VideoOutput.Width, VideoOutput.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);

    private static readonly SKSamplingOptions NearestSampling = new(SKFilterMode.Nearest);

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private readonly byte[] pixels = new byte[VideoOutput.BufferSize];

    private IDispatcherTimer? timer;

    private ulong shownSequence = UInt64.MaxValue;

    private bool hasFrame;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty FramesProperty = BindableProperty.Create(
        nameof(Frames),
        typeof(FrameExchange),
        typeof(GameBoyScreen),
        propertyChanged: static (bindable, _, _) => ((GameBoyScreen)bindable).UpdateTimer());

    public FrameExchange? Frames
    {
        get => (FrameExchange?)GetValue(FramesProperty);
        set => SetValue(FramesProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public GameBoyScreen()
    {
        Loaded += (_, _) => UpdateTimer();
        Unloaded += (_, _) => UpdateTimer();
    }

    //--------------------------------------------------------------------------------
    // Layout
    //--------------------------------------------------------------------------------

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint) =>
        new(widthConstraint, Math.Min(heightConstraint, widthConstraint * VideoOutput.Height / VideoOutput.Width));

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(BezelColor);

        if (hasFrame)
        {
            using var image = SKImage.FromPixelCopy(FrameInfo, pixels);
            var (left, top, width, height) = FrameLayout.ImageRect(e.Info.Width, e.Info.Height);
            canvas.DrawImage(image, SKRect.Create(left, top, width, height), NearestSampling);
        }
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private void UpdateTimer()
    {
        if (IsLoaded && (Frames is not null))
        {
            if (timer is null)
            {
                timer = Dispatcher.CreateTimer();
                timer.Interval = PollInterval;
                timer.Tick += (_, _) => ShowNewFrame();
            }

            timer.Start();
        }
        else
        {
            timer?.Stop();
        }
    }

    private void ShowNewFrame()
    {
        if (Frames?.TryTake(shownSequence) is { } slot)
        {
            slot.Pixels.CopyTo(pixels, 0);
            shownSequence = slot.Info.Sequence;
            hasFrame = true;
            InvalidateSurface();
        }
    }
}

//--------------------------------------------------------------------------------
// Pad
//--------------------------------------------------------------------------------

public sealed class GameBoyPad : SKCanvasView
{
    private const float UnitWidthDivisor = 2.4f;

    private const float UnitHeightDivisor = 1.4f;

    private const float ArmRate = 0.55f;

    private const float ArmThicknessRate = 0.62f;

    private const float ArmCornerRate = 0.15f;

    private const float CrossReachRate = 1.35f;

    private const float CrossDeadZoneRate = 0.2f;

    private const float FaceRate = 0.26f;

    private const float FaceReachRate = 1.3f;

    private const float FaceLabelOffsetRate = 1.65f;

    private const float PillWidthRate = 0.46f;

    private const float PillHeightRate = 0.14f;

    private const float PillReachWidthRate = 0.5f;

    private const float PillReachHeightRate = 0.6f;

    private const float PillLabelOffsetRate = 0.8f;

    private const float LabelSizeRate = 0.5f;

    private const double SectorAngle = 45;

    private const string HorizontalSource = "x";

    private const string VerticalSource = "y";

    private const string ButtonSource = "b";

    private static readonly SKPoint CrossCenter = new(0.27f, 0.40f);

    private static readonly SKPoint ACenter = new(0.84f, 0.33f);

    private static readonly SKPoint BCenter = new(0.63f, 0.49f);

    private static readonly SKPoint SelectCenter = new(0.38f, 0.82f);

    private static readonly SKPoint StartCenter = new(0.62f, 0.82f);

    private static readonly SKColor BodyColor = SKColor.Parse("#C9CBC4");

    private static readonly SKColor CrossColor = SKColor.Parse("#2E3138");

    private static readonly SKColor FaceColor = SKColor.Parse("#9A2452");

    private static readonly SKColor PillColor = SKColor.Parse("#8E9199");

    private static readonly SKColor PressedColor = SKColor.Parse("#F0C040");

    private static readonly SKColor LabelColor = SKColor.Parse("#2E346C");

    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private readonly Dictionary<long, (JoypadButton? Horizontal, JoypadButton? Vertical, JoypadButton? Button)> touches = [];

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty ButtonsProperty = BindableProperty.Create(
        nameof(Buttons),
        typeof(byte),
        typeof(GameBoyPad),
        propertyChanged: static (bindable, _, _) => ((GameBoyPad)bindable).InvalidateSurface());

    public static readonly BindableProperty ChangeCommandProperty = BindableProperty.Create(
        nameof(ChangeCommand),
        typeof(ICommand),
        typeof(GameBoyPad));

    public byte Buttons
    {
        get => (byte)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    public ICommand? ChangeCommand
    {
        get => (ICommand?)GetValue(ChangeCommandProperty);
        set => SetValue(ChangeCommandProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public GameBoyPad()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
    }

    //--------------------------------------------------------------------------------
    // Layout
    //--------------------------------------------------------------------------------

    private readonly record struct PadLayout(SKPoint Cross, float Arm, SKPoint A, SKPoint B, float Face, SKRect Select, SKRect Start);

    private static PadLayout Measure(float width, float height)
    {
        var unit = Math.Min(width / UnitWidthDivisor, height / UnitHeightDivisor);
        var pillWidth = unit * PillWidthRate;
        var pillHeight = unit * PillHeightRate;
        return new PadLayout(
            Place(CrossCenter, width, height),
            unit * ArmRate,
            Place(ACenter, width, height),
            Place(BCenter, width, height),
            unit * FaceRate,
            SKRect.Create((SelectCenter.X * width) - (pillWidth / 2), (SelectCenter.Y * height) - (pillHeight / 2), pillWidth, pillHeight),
            SKRect.Create((StartCenter.X * width) - (pillWidth / 2), (StartCenter.Y * height) - (pillHeight / 2), pillWidth, pillHeight));
    }

    private static SKPoint Place(SKPoint point, float width, float height) => new(point.X * width, point.Y * height);

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(BodyColor);

        var layout = Measure(e.Info.Width, e.Info.Height);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var font = new SKFont(BoldTypeface, layout.Face * LabelSizeRate);

        DrawCross(canvas, paint, layout);
        DrawFace(canvas, paint, font, layout.A, layout.Face, JoypadButton.A, "A");
        DrawFace(canvas, paint, font, layout.B, layout.Face, JoypadButton.B, "B");
        DrawPill(canvas, paint, font, layout.Select, layout.Face, JoypadButton.Select, "SELECT");
        DrawPill(canvas, paint, font, layout.Start, layout.Face, JoypadButton.Start, "START");
    }

    private void DrawCross(SKCanvas canvas, SKPaint paint, PadLayout layout)
    {
        var center = layout.Cross;
        var arm = layout.Arm;
        var thickness = arm * ArmThicknessRate;
        var corner = thickness * ArmCornerRate;
        DrawArm(SKRect.Create(center.X - (thickness / 2), center.Y - arm, thickness, arm), JoypadButton.Up);
        DrawArm(SKRect.Create(center.X - (thickness / 2), center.Y, thickness, arm), JoypadButton.Down);
        DrawArm(SKRect.Create(center.X - arm, center.Y - (thickness / 2), arm, thickness), JoypadButton.Left);
        DrawArm(SKRect.Create(center.X, center.Y - (thickness / 2), arm, thickness), JoypadButton.Right);
        paint.Color = CrossColor;
        canvas.DrawRect(SKRect.Create(center.X - (thickness / 2), center.Y - (thickness / 2), thickness, thickness), paint);

        void DrawArm(SKRect rect, JoypadButton button)
        {
            paint.Color = IsDown(button) ? PressedColor : CrossColor;
            canvas.DrawRoundRect(rect, corner, corner, paint);
        }
    }

    private void DrawFace(SKCanvas canvas, SKPaint paint, SKFont font, SKPoint center, float radius, JoypadButton button, string label)
    {
        paint.Color = IsDown(button) ? PressedColor : FaceColor;
        canvas.DrawCircle(center, radius, paint);
        paint.Color = LabelColor;
        canvas.DrawText(label, center.X, center.Y + (radius * FaceLabelOffsetRate), SKTextAlign.Center, font, paint);
    }

    private void DrawPill(SKCanvas canvas, SKPaint paint, SKFont font, SKRect rect, float face, JoypadButton button, string label)
    {
        paint.Color = IsDown(button) ? PressedColor : PillColor;
        canvas.DrawRoundRect(rect, rect.Height / 2, rect.Height / 2, paint);
        paint.Color = LabelColor;
        canvas.DrawText(label, rect.MidX, rect.Bottom + (face * PillLabelOffsetRate), SKTextAlign.Center, font, paint);
    }

    private bool IsDown(JoypadButton button) => (Buttons & (1 << (int)button)) != 0;

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        touches.TryGetValue(e.Id, out var before);
        var touching = e.ActionType is SKTouchAction.Pressed or SKTouchAction.Moved;
        var after = touching ? HitTest(Measure(CanvasSize.Width, CanvasSize.Height), e.Location) : default;
        if (touching)
        {
            touches[e.Id] = after;
        }
        else
        {
            touches.Remove(e.Id);
        }

        var pressed = Notify($"{e.Id}:{HorizontalSource}", before.Horizontal, after.Horizontal);
        pressed |= Notify($"{e.Id}:{VerticalSource}", before.Vertical, after.Vertical);
        pressed |= Notify($"{e.Id}:{ButtonSource}", before.Button, after.Button);
        if (pressed)
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }

        e.Handled = true;
    }

    private bool Notify(string source, JoypadButton? before, JoypadButton? after)
    {
        if (before == after)
        {
            return false;
        }

        if (before is { } released)
        {
            Execute(new GameBoyButtonChange(source, released, false));
        }

        if (after is { } pressed)
        {
            Execute(new GameBoyButtonChange(source, pressed, true));
            return true;
        }

        return false;
    }

    private void Execute(GameBoyButtonChange change)
    {
        if (ChangeCommand?.CanExecute(change) == true)
        {
            ChangeCommand.Execute(change);
        }
    }

    private static (JoypadButton? Horizontal, JoypadButton? Vertical, JoypadButton? Button) HitTest(PadLayout layout, SKPoint point)
    {
        var dx = point.X - layout.Cross.X;
        var dy = point.Y - layout.Cross.Y;
        var distance = MathF.Sqrt((dx * dx) + (dy * dy));
        if ((distance <= layout.Arm * CrossReachRate) && (distance >= layout.Arm * CrossDeadZoneRate))
        {
            var angle = Math.Atan2(-dy, dx) * 180 / Math.PI;
            var sector = (int)Math.Round(((angle + 360) % 360) / SectorAngle) % 8;
            JoypadButton? horizontal = sector is 0 or 1 or 7 ? JoypadButton.Right : sector is 3 or 4 or 5 ? JoypadButton.Left : null;
            JoypadButton? vertical = sector is 1 or 2 or 3 ? JoypadButton.Up : sector is 5 or 6 or 7 ? JoypadButton.Down : null;
            return (horizontal, vertical, null);
        }

        var reach = layout.Face * FaceReachRate;
        if (SKPoint.Distance(point, layout.A) <= reach)
        {
            return (null, null, JoypadButton.A);
        }

        if (SKPoint.Distance(point, layout.B) <= reach)
        {
            return (null, null, JoypadButton.B);
        }

        var select = layout.Select;
        select.Inflate(layout.Face * PillReachWidthRate, layout.Face * PillReachHeightRate);
        if (select.Contains(point))
        {
            return (null, null, JoypadButton.Select);
        }

        var start = layout.Start;
        start.Inflate(layout.Face * PillReachWidthRate, layout.Face * PillReachHeightRate);
        return start.Contains(point) ? (null, null, JoypadButton.Start) : default;
    }
}
