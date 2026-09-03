namespace Template.MobileApp.Graphics.Scene;

internal sealed class CarSim
{
    private enum Phase
    {
        Straight,
        Brake,
        Corner
    }

    private static readonly float[] GearTopSpeed = [0f, 70f, 108f, 146f, 184f, 222f, 260f, 300f, 340f];

    public float SpeedKmh { get; private set; } = 120f;
    public float Rpm { get; private set; } = 9000f;
    public int Gear { get; private set; } = 3;
    public float Throttle { get; private set; }
    public float BoostCharge { get; private set; } = 0.7f;
    public bool BoostActive { get; private set; }
    public float ErsKw { get; private set; }
    public float LatG { get; private set; }
    public float LonG { get; private set; }
    public float WaterTemp { get; private set; } = 88f;
    public float OilTemp { get; private set; } = 104f;
    public float OilPressure { get; private set; } = 4f;
    public float FuelPct { get; private set; } = 68f;
    public float Battery { get; private set; } = 13.4f;
    public float TurboBar { get; private set; } = 1.2f;

    public int Lap { get; private set; } = 3;
    public float LapTime { get; private set; } = 41.2f;
    public float BestLap { get; private set; } = 81.98f;

    private Phase phase = Phase.Straight;
    private float phaseTime;
    private float phaseLength = 9f;
    private float brake;
    private float cornerSign = 1f;

    public void Update(float t, float dt)
    {
        phaseTime += dt;
        if (phaseTime >= phaseLength)
        {
            phaseTime = 0f;
            switch (phase)
            {
                case Phase.Straight:
                    phase = Phase.Brake;
                    phaseLength = 1.6f;
                    break;
                case Phase.Brake:
                    phase = Phase.Corner;
                    phaseLength = 2.6f;
                    cornerSign = -cornerSign;
                    break;
                default:
                    phase = Phase.Straight;
                    phaseLength = 8f + (2f * MathF.Sin(t * 0.37f));
                    break;
            }
        }

        var latTarget = 0f;
        switch (phase)
        {
            case Phase.Straight:
                Throttle = MathF.Min(1f, Throttle + (2.5f * dt));
                brake = MathF.Max(0f, brake - (6f * dt));
                break;
            case Phase.Brake:
                Throttle = MathF.Max(0f, Throttle - (8f * dt));
                brake = MathF.Min(1f, brake + (6f * dt));
                break;
            default:
                brake = MathF.Max(0f, brake - (3f * dt));
                Throttle = 0.45f + (0.1f * MathF.Sin(t * 3f));
                latTarget = cornerSign * (2.2f + (1.4f * MathF.Sin(phaseTime / phaseLength * MathF.PI)));
                break;
        }

        // Boost
        if (BoostActive)
        {
            BoostCharge = MathF.Max(0f, BoostCharge - (0.16f * dt));
            if (BoostCharge <= 0.05f)
            {
                BoostActive = false;
            }
        }
        else
        {
            BoostCharge = MathF.Min(1f, BoostCharge + ((Throttle > 0.6f ? 0.045f : 0.085f) * dt));
            if ((BoostCharge >= 0.985f) && (Gear >= 6) && (Throttle > 0.9f))
            {
                BoostActive = true;
            }
        }

        // Speed physics
        var thrust = Throttle * (BoostActive ? 52f : 38f);
        var accel = thrust - (SpeedKmh * SpeedKmh * 0.00042f) - (brake * 62f);
        SpeedKmh = Math.Clamp(SpeedKmh + (accel * dt), 0f, 340f);

        // RPM / gear
        Rpm = Math.Clamp(SpeedKmh / GearTopSpeed[Gear] * 18000f, 3200f, 18600f);
        if ((Rpm > 17600f) && (Gear < 8))
        {
            Gear++;
        }
        else if ((Rpm < 6500f) && (Gear > 1))
        {
            Gear--;
        }

        ErsKw = BoostActive ? 290f : (Throttle * 160f) - (brake * 110f);
        LonG = Math.Clamp(accel * 0.055f, -3.5f, 3.5f);
        LatG += (latTarget - LatG) * MathF.Min(1f, dt * 4f);

        // Telemetry channels
        WaterTemp += ((92f + (Throttle * 22f)) - WaterTemp) * dt * 0.05f;
        OilTemp += ((108f + (Throttle * 42f)) - OilTemp) * dt * 0.04f;
        OilPressure = 1.2f + (Rpm / 18000f * 6.2f) + (0.15f * MathF.Sin(t * 9f));
        FuelPct = MathF.Max(4f, FuelPct - ((0.010f + (0.05f * Throttle)) * dt));
        Battery = 13.5f + (0.25f * MathF.Sin(t * 0.9f)) - (BoostActive ? 0.7f : 0f);
        TurboBar += ((Throttle * 2.7f) - TurboBar) * MathF.Min(1f, dt * 5f);

        // Lap
        LapTime += dt;
        if (LapTime >= 92f)
        {
            BestLap = MathF.Min(BestLap, LapTime);
            LapTime = 0f;
            Lap++;
            if (Lap > 12)
            {
                Lap = 1;
            }
        }
    }
}

public sealed class TelemetryScene : SceneObject
{
    private const float BaseWidth = 400f;

    private static readonly SKColor BgColor = new(0x04, 0x07, 0x0D);
    private static readonly SKColor Cyan = new(0x00, 0xE5, 0xFF);
    private static readonly SKColor Azure = new(0x2E, 0x9B, 0xFF);
    private static readonly SKColor Amber = new(0xFF, 0xB3, 0x20);
    private static readonly SKColor Red = new(0xFF, 0x2D, 0x40);
    private static readonly SKColor Green = new(0x2E, 0xFF, 0x9E);
    private static readonly SKColor Dim = new(0x5E, 0x8C, 0xA8);
    private static readonly SKColor Panel = new SKColor(0x0A, 0x14, 0x21).WithAlpha(215);
    private static readonly SKColor PanelLine = new(0x1C, 0x3A, 0x52);
    private static readonly SKColor White = new(0xE8, 0xF6, 0xFF);

    // Segment bits: a, b, c, d, e, f, g
    private static readonly bool[][] SegTable =
    [
        [true, true, true, true, true, true, false],
        [false, true, true, false, false, false, false],
        [true, true, false, true, true, false, true],
        [true, true, true, true, false, false, true],
        [false, true, true, false, false, true, true],
        [true, false, true, true, false, true, true],
        [true, false, true, true, true, true, true],
        [true, true, true, false, false, false, false],
        [true, true, true, true, true, true, true],
        [true, true, true, true, false, true, true]
    ];

    private readonly CarSim sim = new();

    private SKShader? glow;
    private int glowWidth;
    private int glowHeight;

    protected override void Update(float t, float dt) => sim.Update(t, dt);

    protected override void OnRender(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BgColor);

        DrawBackdrop(canvas, width, height);

        var s = width / BaseWidth;
        var vh = height / s;

        canvas.Save();
        canvas.Scale(s);

        DrawTopRow(canvas);
        DrawSpeedCluster(canvas, vh);
        DrawTachometer(canvas, vh);
        DrawBoost(canvas, Time, vh);
        DrawErs(canvas, vh);
        DrawGear(canvas, Time, vh);
        DrawGForce(canvas, vh);
        DrawMiniGauges(canvas, Time, vh);

        canvas.Restore();
    }

    //--------------------------------------------------------------------------------
    // Backdrop
    //--------------------------------------------------------------------------------

    private void DrawBackdrop(SKCanvas canvas, int width, int height)
    {
        if ((glow is null) || (glowWidth != width) || (glowHeight != height))
        {
            glow?.Dispose();
            glow = SKShader.CreateRadialGradient(
                new SKPoint(width / 2f, height * 0.24f),
                width * 0.9f,
                [Azure.WithAlpha(38), Azure.WithAlpha(0)],
                [0f, 1f],
                SKShaderTileMode.Clamp);
            glowWidth = width;
            glowHeight = height;
        }

        using var paint = new SKPaint();
        paint.Shader = glow;
        canvas.DrawRect(0f, 0f, width, height, paint);
    }

    //--------------------------------------------------------------------------------
    // Top row
    //--------------------------------------------------------------------------------

    private void DrawTopRow(SKCanvas canvas)
    {
        var time = TimeSpan.FromSeconds(sim.LapTime);
        var best = TimeSpan.FromSeconds(sim.BestLap);

        DrawText(canvas, "LAP", 16f, 28f, 8f, Dim);
        DrawGlowText(canvas, $"{sim.Lap:00}/12", 16f, 46f, 14f, White, 3f, bold: true);

        DrawText(canvas, "TIME", 112f, 28f, 8f, Dim);
        DrawGlowText(canvas, $"{time.Minutes}:{time.Seconds:00}.{time.Milliseconds:000}", 112f, 46f, 14f, Cyan, 3f, bold: true);

        DrawText(canvas, "BEST", 236f, 28f, 8f, Dim);
        DrawGlowText(canvas, $"{best.Minutes}:{best.Seconds:00}.{best.Milliseconds:000}", 236f, 46f, 14f, Amber, 3f, bold: true);

        DrawText(canvas, "POS", 352f, 28f, 8f, Dim);
        DrawGlowText(canvas, "P04", 352f, 46f, 14f, White, 3f, bold: true);

        // ダブルバッファ試験 (D8) の現在モード
        DrawText(canvas, UseDoubleBuffer ? "MODE BUFFER" : "MODE DIRECT", 16f, 60f, 8f, Dim);
    }

    //--------------------------------------------------------------------------------
    // Speed cluster
    //--------------------------------------------------------------------------------

    private void DrawSpeedCluster(SKCanvas canvas, float vh)
    {
        var top = vh * 0.075f;
        var speedColor = sim.BoostActive ? Amber : Cyan;

        DrawSevenSeg(canvas, $"{(int)sim.SpeedKmh:000}", 200f, top, 56f, speedColor);
        DrawText(canvas, "km/h", 200f, top + 70f, 9f, Dim, align: SKTextAlign.Center);

        // Cell bar
        const int cells = 26;
        var barY = top + 78f;
        var cellW = (368f - ((cells - 1) * 3f)) / cells;
        var lit = (int)(sim.SpeedKmh / 340f * cells);
        for (var i = 0; i < cells; i++)
        {
            var x = 16f + (i * (cellW + 3f));
            var ratio = i / (float)cells;
            var color = ratio < 0.6f ? Green : ratio < 0.85f ? Amber : Red;
            if (i < lit)
            {
                Fill.Color = color.WithAlpha(230);
                canvas.DrawRect(x, barY, cellW, 9f, Fill);
            }
            else
            {
                Fill.Color = PanelLine.WithAlpha(120);
                canvas.DrawRect(x, barY, cellW, 9f, Fill);
            }
        }
    }

    //--------------------------------------------------------------------------------
    // Tachometer / boost
    //--------------------------------------------------------------------------------

    private void DrawTachometer(SKCanvas canvas, float vh)
    {
        var cy = vh * 0.31f;
        const float cx = 102f;
        const float r = 60f;
        const float start = 150f;
        const float sweep = 240f;
        const float maxRpm = 19000f;
        const float redlineRpm = 17000f;

        var frac = Math.Clamp(sim.Rpm / maxRpm, 0f, 1f);
        var inRed = sim.Rpm >= redlineRpm;

        // 不変の盤面 (レッドライン帯 / トラック / 目盛) はキャッシュから再生
        DrawCachedLayer(canvas, "tacho", BaseWidth, cy + r + 16f, c => DrawTachometerChrome(c, cx, cy, r, start, sweep, maxRpm, redlineRpm));

        Stroke.StrokeCap = SKStrokeCap.Butt;

        // Value arc
        if (frac > 0.005f)
        {
            Stroke.Color = inRed ? Red : sim.Rpm > 15200f ? Amber : Cyan;
            Stroke.StrokeWidth = 10f;
            canvas.DrawArc(new SKRect(cx - r, cy - r, cx + r, cy + r), start, sweep * frac, false, Stroke);
        }

        // Needle
        var needleRad = DegToRad(start + (sweep * frac));
        DrawGlowLine(
            canvas,
            cx + (10f * MathF.Cos(needleRad)),
            cy + (10f * MathF.Sin(needleRad)),
            cx + ((r - 6f) * MathF.Cos(needleRad)),
            cy + ((r - 6f) * MathF.Sin(needleRad)),
            inRed ? Red : White,
            2f);

        // Hub
        Fill.Color = Panel;
        canvas.DrawCircle(cx, cy, 8f, Fill);
        Stroke.Color = PanelLine;
        Stroke.StrokeWidth = 1.5f;
        canvas.DrawCircle(cx, cy, 8f, Stroke);
        Fill.Color = inRed ? Red : Cyan;
        canvas.DrawCircle(cx, cy, 3f, Fill);

        // Readout
        DrawGlowText(canvas, $"{(int)sim.Rpm}", cx, cy + 44f, 14f, inRed ? Red : White, 3f, bold: true, align: SKTextAlign.Center);
    }

    private void DrawTachometerChrome(SKCanvas canvas, float cx, float cy, float r, float start, float sweep, float maxRpm, float redlineRpm)
    {
        var redFrac = redlineRpm / maxRpm;

        Stroke.StrokeCap = SKStrokeCap.Butt;

        // Redline band
        Stroke.Color = Red.WithAlpha(200);
        Stroke.StrokeWidth = 3f;
        canvas.DrawArc(new SKRect(cx - r - 9f, cy - r - 9f, cx + r + 9f, cy + r + 9f), start + (sweep * redFrac), sweep * (1f - redFrac), false, Stroke);

        // Track
        Stroke.Color = PanelLine.WithAlpha(160);
        Stroke.StrokeWidth = 10f;
        canvas.DrawArc(new SKRect(cx - r, cy - r, cx + r, cy + r), start, sweep, false, Stroke);

        // Ticks (major every 2000, label inside)
        for (var rpm = 0; rpm <= (int)maxRpm; rpm += 1000)
        {
            var angle = DegToRad(start + (sweep * rpm / maxRpm));
            var major = (rpm % 2000) == 0;
            var tickColor = rpm >= redlineRpm ? Red.WithAlpha(220) : Dim.WithAlpha(major ? (byte)220 : (byte)110);
            var r0 = r + 7f;
            var r1 = r + (major ? 15f : 11f);
            Stroke.Color = tickColor;
            Stroke.StrokeWidth = major ? 1.8f : 1.1f;
            canvas.DrawLine(
                cx + (r0 * MathF.Cos(angle)),
                cy + (r0 * MathF.Sin(angle)),
                cx + (r1 * MathF.Cos(angle)),
                cy + (r1 * MathF.Sin(angle)),
                Stroke);

            if (major)
            {
                var tp = new SKPoint(cx + ((r - 16f) * MathF.Cos(angle)), cy + ((r - 16f) * MathF.Sin(angle)));
                DrawText(canvas, $"{rpm / 1000}", tp.X, tp.Y + 3f, 8f, rpm >= 18000 ? Red : Dim, align: SKTextAlign.Center);
            }
        }

        // Fixed labels
        DrawText(canvas, "×1000 r/min", cx, cy + 26f, 8f, Dim, align: SKTextAlign.Center);
        DrawText(canvas, "ENGINE RPM", cx, cy + 58f, 8f, Dim, align: SKTextAlign.Center);
    }

    private void DrawBoost(SKCanvas canvas, float t, float vh)
    {
        var cy = vh * 0.31f;
        const float cx = 298f;
        const float r = 60f;
        const float start = 150f;
        const float sweep = 240f;

        // Rotating decoration
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Azure.WithAlpha(120);
        Stroke.StrokeWidth = 2f;
        canvas.Save();
        canvas.RotateDegrees((t * 40f) % 360f, cx, cy);
        for (var i = 0; i < 3; i++)
        {
            canvas.DrawArc(new SKRect(cx - r - 8f, cy - r - 8f, cx + r + 8f, cy + r + 8f), i * 120f, 70f, false, Stroke);
        }

        canvas.Restore();

        // Segment ring (左下始まりの時計回り)
        const int segments = 22;
        var litSegments = (int)(sim.BoostCharge * segments);
        var ringColor = sim.BoostActive
            ? (Blink(t, 14f) ? Red : Amber)
            : (sim.BoostCharge < 0.25f ? Red : Cyan);
        for (var i = 0; i < segments; i++)
        {
            var segStart = start + (i * (sweep / segments)) + 1.5f;
            Stroke.Color = i < litSegments ? ringColor.WithAlpha(230) : PanelLine.WithAlpha(150);
            Stroke.StrokeWidth = 9f;
            canvas.DrawArc(new SKRect(cx - r, cy - r, cx + r, cy + r), segStart, (sweep / segments) - 3f, false, Stroke);
        }

        // Scale labels
        for (var p = 0; p <= 100; p += 50)
        {
            var rad = DegToRad(start + (sweep * p / 100f));
            DrawText(canvas, $"{p}", cx + ((r + 15f) * MathF.Cos(rad)), cy + ((r + 15f) * MathF.Sin(rad)) + 3f, 8f, Dim, align: SKTextAlign.Center);
        }

        // Pulse rings when active
        if (sim.BoostActive)
        {
            var k = (t * 1.6f) % 1f;
            Stroke.Color = Amber.WithAlpha((byte)((1f - k) * 120f));
            Stroke.StrokeWidth = 2f;
            canvas.DrawCircle(cx, cy, r + (k * 24f), Stroke);
        }

        // Center disc
        Fill.Color = Panel;
        canvas.DrawCircle(cx, cy, r - 16f, Fill);
        Stroke.Color = PanelLine;
        Stroke.StrokeWidth = 1.5f;
        canvas.DrawCircle(cx, cy, r - 16f, Stroke);

        DrawGlowText(canvas, $"{(int)(sim.BoostCharge * 100f):00}%", cx, cy + 2f, 22f, sim.BoostActive ? Amber : White, 4f, bold: true, align: SKTextAlign.Center);
        if (sim.BoostActive)
        {
            if (Blink(t, 18f))
            {
                DrawText(canvas, "■ DISCHARGE", cx, cy + 22f, 8f, Red, bold: true, align: SKTextAlign.Center);
            }
        }
        else if (sim.BoostCharge >= 0.985f)
        {
            DrawGlowText(canvas, "READY", cx, cy + 22f, 8f, Green, 3f, bold: true, align: SKTextAlign.Center);
        }
        else
        {
            DrawText(canvas, "CHARGING..", cx, cy + 22f, 8f, Azure, align: SKTextAlign.Center);
        }

        DrawText(canvas, "BOOST POT", cx, cy - 22f, 8f, Dim, align: SKTextAlign.Center);
    }

    //--------------------------------------------------------------------------------
    // ERS / Gear / G-Force
    //--------------------------------------------------------------------------------

    private void DrawErs(SKCanvas canvas, float vh)
    {
        var cy = vh * 0.52f;
        const float cx = 68f;
        const float r = 36f;

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.Color = PanelLine.WithAlpha(150);
        Stroke.StrokeWidth = 6f;
        canvas.DrawArc(new SKRect(cx - r, cy - r, cx + r, cy + r), 135f, 270f, false, Stroke);

        var norm = Math.Clamp((sim.ErsKw + 100f) / 400f, 0f, 1f);
        var color = sim.BoostActive ? Amber : sim.ErsKw < -5f ? Azure : Cyan;
        Stroke.Color = color;
        canvas.DrawArc(new SKRect(cx - r, cy - r, cx + r, cy + r), 135f, 270f * norm, false, Stroke);

        var rad = DegToRad(135f + (270f * norm));
        var px = cx + (r * MathF.Cos(rad));
        var py = cy + (r * MathF.Sin(rad));
        Fill.Color = White.WithAlpha(80);
        canvas.DrawCircle(px, py, 6f, Fill);
        Fill.Color = White;
        canvas.DrawCircle(px, py, 3f, Fill);

        DrawGlowText(canvas, $"{(int)sim.ErsKw:+0;-0}", cx, cy + 5f, 16f, color, 4f, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "kW", cx, cy + 18f, 8f, Dim, align: SKTextAlign.Center);
        DrawText(canvas, "ERS OUTPUT", cx, cy + r + 16f, 8f, Dim, align: SKTextAlign.Center);
    }

    private void DrawGear(SKCanvas canvas, float t, float vh)
    {
        var cy = vh * 0.52f;
        const float cx = 200f;
        const float w = 84f;
        const float h = 104f;
        var y0 = cy - (h / 2f);

        DrawCutPanel(canvas, cx - (w / 2f), y0, w, h, 10f, Panel, PanelLine, 1.6f);
        Fill.Color = Cyan;
        canvas.DrawRect(cx - (w / 2f) + 5f, y0 + 5f, 2f, 16f, Fill);
        canvas.DrawRect(cx + (w / 2f) - 7f, y0 + h - 21f, 2f, 16f, Fill);

        var gearColor = sim.Rpm > 17000f ? (Blink(t, 12f) ? Red : Amber) : sim.Rpm > 15200f ? Amber : White;
        DrawGlowText(canvas, sim.Gear.ToString(), cx, cy + 20f, 72f, gearColor, 6f, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "GEAR", cx, cy + 40f, 9f, Dim, align: SKTextAlign.Center);

        // Side indicators
        for (var i = 0; i < 8; i++)
        {
            var iy = cy + 32f - (i * 10.5f);
            Fill.Color = i < sim.Gear ? Cyan.WithAlpha(220) : PanelLine.WithAlpha(140);
            canvas.DrawRect(cx - (w / 2f) + 9f, iy, 5f, 7f, Fill);
            canvas.DrawRect(cx + (w / 2f) - 14f, iy, 5f, 7f, Fill);
        }
    }

    private void DrawGForce(SKCanvas canvas, float vh)
    {
        var cy = vh * 0.52f;
        const float cx = 332f;
        const float r = 36f;

        // 不変の盤面 (外円 / 十字 / 破線円) はキャッシュから再生
        DrawCachedLayer(canvas, "gforce", BaseWidth, cy + r + 16f, c => DrawGForceChrome(c, cx, cy, r));

        var total = MathF.Sqrt((sim.LatG * sim.LatG) + (sim.LonG * sim.LonG));
        var color = total < 2.2f ? Cyan : total < 3.2f ? Amber : Red;
        var gx = cx + (Math.Clamp(sim.LatG / 3.5f, -1f, 1f) * r * 0.9f);
        var gy = cy + (Math.Clamp(-sim.LonG / 3.5f, -1f, 1f) * r * 0.9f);

        Stroke.Color = color.WithAlpha(200);
        Stroke.StrokeWidth = 1.5f;
        canvas.DrawLine(cx, cy, gx, gy, Stroke);
        Fill.Color = color.WithAlpha(90);
        canvas.DrawCircle(gx, gy, 8f, Fill);
        Fill.Color = color;
        canvas.DrawCircle(gx, gy, 4f, Fill);

        DrawText(canvas, $"G-FORCE  {total:0.0} G", cx, cy + r + 16f, 8f, Dim, align: SKTextAlign.Center);
    }

    private void DrawGForceChrome(SKCanvas canvas, float cx, float cy, float r)
    {
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = PanelLine.WithAlpha(180);
        Stroke.StrokeWidth = 1.2f;
        canvas.DrawCircle(cx, cy, r, Stroke);
        canvas.DrawLine(cx - r, cy, cx + r, cy, Stroke);
        canvas.DrawLine(cx, cy - r, cx, cy + r, Stroke);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1f;
        paint.Color = PanelLine.WithAlpha(180);
        using var dash = SKPathEffect.CreateDash([4f, 4f], 0f);
        paint.PathEffect = dash;
        canvas.DrawCircle(cx, cy, r / 2f, paint);
    }

    //--------------------------------------------------------------------------------
    // Mini gauges
    //--------------------------------------------------------------------------------

    private void DrawMiniGauges(SKCanvas canvas, float t, float vh)
    {
        var top = vh * 0.615f;
        var bottom = vh - 12f;
        var cellH = (bottom - top - 8f) / 2f;
        var cy1 = top + (cellH / 2f);
        var cy2 = top + cellH + 8f + (cellH / 2f);

        DrawMiniGauge(canvas, t, 68f, cy1, cellH, "WATER", sim.WaterTemp, 50f, 130f, "°C", Azure, sim.WaterTemp > 110f);
        DrawMiniGauge(canvas, t, 200f, cy1, cellH, "OIL TEMP", sim.OilTemp, 60f, 160f, "°C", Amber, sim.OilTemp > 145f);
        DrawMiniGauge(canvas, t, 332f, cy1, cellH, "OIL PRESS", sim.OilPressure, 0f, 10f, "bar", Cyan, sim.OilPressure < 1.5f);
        DrawMiniGauge(canvas, t, 68f, cy2, cellH, "FUEL", sim.FuelPct, 0f, 100f, "%", Green, sim.FuelPct < 12f);
        DrawMiniGauge(canvas, t, 200f, cy2, cellH, "BATTERY", sim.Battery, 10f, 16f, "V", Azure, sim.Battery < 12.2f);
        DrawMiniGauge(canvas, t, 332f, cy2, cellH, "TURBO", sim.TurboBar, 0f, 3f, "bar", Amber, sim.TurboBar > 2.6f);
    }

    // 半円アークの中に数値を描く小型計器
    private void DrawMiniGauge(SKCanvas canvas, float t, float cx, float cy, float h, string label, float value, float min, float max, string unit, SKColor accent, bool warning)
    {
        const float w = 124f;
        var x = cx - (w / 2f);
        var y = cy - (h / 2f);
        var flash = warning && Blink(t, 12f);
        var color = warning ? Red : accent;

        DrawCutPanel(canvas, x, y, w, h, 10f, Panel, warning ? Red.WithAlpha((byte)(flash ? 200 : 90)) : PanelLine, warning ? 1.8f : 1.4f);

        // 角のアクセント
        Fill.Color = color.WithAlpha(160);
        canvas.DrawRect(x + 4f, y + 8f, 2f, 14f, Fill);
        canvas.DrawRect(x + w - 6f, y + h - 22f, 2f, 14f, Fill);

        DrawText(canvas, label, cx, y + 15f, 8f, Dim, align: SKTextAlign.Center);

        // Half arc
        var gy = cy + (h * 0.17f);
        var r = MathF.Min(31f, h * 0.30f);
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.Color = PanelLine.WithAlpha(140);
        Stroke.StrokeWidth = 5f;
        canvas.DrawArc(new SKRect(cx - r, gy - r, cx + r, gy + r), 180f, 180f, false, Stroke);

        var norm = Math.Clamp((value - min) / (max - min), 0f, 1f);
        if (norm > 0.005f)
        {
            Stroke.Color = flash ? Red : color;
            canvas.DrawArc(new SKRect(cx - r, gy - r, cx + r, gy + r), 180f, 180f * norm, false, Stroke);
        }

        // 針先ドット
        var rad = DegToRad(180f + (180f * norm));
        Fill.Color = White;
        canvas.DrawCircle(cx + (r * MathF.Cos(rad)), gy + (r * MathF.Sin(rad)), 2.6f, Fill);

        // 目盛
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Dim.WithAlpha(150);
        Stroke.StrokeWidth = 1.4f;
        for (var i = 0; i <= 4; i++)
        {
            var tickRad = DegToRad(180f + (45f * i));
            canvas.DrawLine(
                cx + ((r + 5f) * MathF.Cos(tickRad)),
                gy + ((r + 5f) * MathF.Sin(tickRad)),
                cx + ((r + 9f) * MathF.Cos(tickRad)),
                gy + ((r + 9f) * MathF.Sin(tickRad)),
                Stroke);
        }

        // アークの中に値と単位
        var text = max <= 20f ? $"{value:0.0}" : $"{(int)value}";
        DrawGlowText(canvas, text, cx, gy + 4f, 18f, flash ? Red : White, 4f, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, unit, cx, gy + 18f, 8f, Dim, align: SKTextAlign.Center);
    }

    //--------------------------------------------------------------------------------
    // Seven segment
    //--------------------------------------------------------------------------------

    private static void DrawSevenSeg(SKCanvas canvas, string digits, float centerX, float topY, float h, SKColor color)
    {
        var w = h * 0.58f;
        var gap = h * 0.17f;
        var th = h * 0.115f;
        var total = (digits.Length * w) + ((digits.Length - 1) * gap);

        canvas.Save();
        canvas.Translate(centerX, topY);
        canvas.Skew(-0.12f, 0f);

        var x0 = -total / 2f;

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.StrokeWidth = th;

        for (var d = 0; d < digits.Length; d++)
        {
            var digit = digits[d] - '0';
            if (digit is < 0 or > 9)
            {
                continue;
            }

            var x = x0 + (d * (w + gap));

            // Ghost pass
            paint.MaskFilter = null;
            paint.Color = color.WithAlpha(16);
            DrawSegments(canvas, paint, x, w, h, th, SegTable[8]);

            // Glow pass
            paint.MaskFilter = GetBlur(h * 0.055f);
            paint.Color = color.WithAlpha(110);
            DrawSegments(canvas, paint, x, w, h, th, SegTable[digit]);

            // Solid pass
            paint.MaskFilter = null;
            paint.Color = color;
            DrawSegments(canvas, paint, x, w, h, th, SegTable[digit]);
        }

        canvas.Restore();
    }

    private static void DrawSegments(SKCanvas canvas, SKPaint paint, float x, float w, float h, float th, bool[] segments)
    {
        var pad = th * 0.75f;
        var mid = h / 2f;

        if (segments[0])
        {
            canvas.DrawLine(x + pad, 0f, x + w - pad, 0f, paint);
        }

        if (segments[1])
        {
            canvas.DrawLine(x + w, pad, x + w, mid - pad, paint);
        }

        if (segments[2])
        {
            canvas.DrawLine(x + w, mid + pad, x + w, h - pad, paint);
        }

        if (segments[3])
        {
            canvas.DrawLine(x + pad, h, x + w - pad, h, paint);
        }

        if (segments[4])
        {
            canvas.DrawLine(x, mid + pad, x, h - pad, paint);
        }

        if (segments[5])
        {
            canvas.DrawLine(x, pad, x, mid - pad, paint);
        }

        if (segments[6])
        {
            canvas.DrawLine(x + pad, mid, x + w - pad, mid, paint);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            glow?.Dispose();
            glow = null;
        }

        base.Dispose(disposing);
    }
}
