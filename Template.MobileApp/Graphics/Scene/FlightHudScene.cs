namespace Template.MobileApp.Graphics.Scene;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------
internal enum FlightIff
{
    Hostile,
    Friendly,
    Unknown
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------
internal sealed class FlightContact
{
    public FlightIff Iff { get; init; }
    public float BearingDeg { get; set; }
    public float RangeNm { get; set; }
    public float BearingDrift { get; init; }
    public float RangeRate { get; set; }
}

//--------------------------------------------------------------------------------
// Simulation
//--------------------------------------------------------------------------------
internal sealed class FlightHudSim
{
    public float HeadingDeg { get; private set; } = 42f;
    public float PitchDeg { get; private set; }
    public float RollDeg { get; private set; }
    public float SpeedKt { get; private set; } = 470f;
    public float Mach { get; private set; }
    public float AltitudeFt { get; private set; } = 11500f;
    public float ClimbFpm { get; private set; }
    public float GForce { get; private set; } = 1f;
    public float FuelLbs { get; private set; } = 5800f;
    public float ThrottlePct { get; private set; } = 78f;
    public float SweepDeg { get; private set; }

    public int GunAmmo => (int)gunAmmo;
    public bool GunFiring { get; private set; }
    public int Missiles { get; private set; } = 6;
    public float Fox2Timer { get; private set; }

    public FlightContact[] Contacts { get; } =
    [
        new() { Iff = FlightIff.Hostile, BearingDeg = 38f, RangeNm = 9.5f, BearingDrift = 1.1f, RangeRate = -0.14f },
        new() { Iff = FlightIff.Hostile, BearingDeg = 71f, RangeNm = 21f, BearingDrift = -0.7f, RangeRate = -0.22f },
        new() { Iff = FlightIff.Hostile, BearingDeg = 331f, RangeNm = 30f, BearingDrift = 0.5f, RangeRate = 0.18f },
        new() { Iff = FlightIff.Friendly, BearingDeg = 198f, RangeNm = 11f, BearingDrift = -1.3f, RangeRate = 0.08f },
        new() { Iff = FlightIff.Friendly, BearingDeg = 152f, RangeNm = 24f, BearingDrift = 0.9f, RangeRate = -0.1f },
        new() { Iff = FlightIff.Unknown, BearingDeg = 265f, RangeNm = 35f, BearingDrift = 0.4f, RangeRate = -0.16f }
    ];

    private float gunAmmo = 510f;
    private float gunCycle;
    private float missileTimer;
    private float rearmTimer;

    public void Update(float t, float dt)
    {
        RollDeg = (16f * MathF.Sin(t * 0.23f)) + (5f * MathF.Sin(t * 0.71f));
        PitchDeg = (5.5f * MathF.Sin(t * 0.19f)) + (2.2f * MathF.Sin(t * 0.47f));
        HeadingDeg = Wrap360(HeadingDeg + (4.5f * MathF.Sin(t * 0.09f) * dt));
        SpeedKt = 470f + (55f * MathF.Sin(t * 0.13f)) + (12f * MathF.Sin(t * 0.43f));
        Mach = SpeedKt * 0.00157f;
        ClimbFpm = PitchDeg * 210f;
        AltitudeFt = Math.Clamp(AltitudeFt + (ClimbFpm / 60f * dt), 6500f, 17500f);
        GForce = 1f + (MathF.Abs(RollDeg) * 0.06f) + (MathF.Abs(PitchDeg) * 0.12f);
        ThrottlePct = 78f + (14f * MathF.Sin(t * 0.21f));
        FuelLbs = MathF.Max(0f, FuelLbs - (1.25f * dt));
        SweepDeg = (SweepDeg + (75f * dt)) % 360f;

        // Gun burst cycle
        gunCycle += dt;
        if (gunCycle >= 9f)
        {
            gunCycle -= 9f;
        }

        GunFiring = (gunCycle >= 7.4f) && (gunAmmo > 0f);
        if (GunFiring)
        {
            gunAmmo = MathF.Max(0f, gunAmmo - (95f * dt));
        }
        else if (gunAmmo < 30f)
        {
            gunAmmo = 510f;
        }

        // Missile launch / rearm cycle
        Fox2Timer = MathF.Max(0f, Fox2Timer - dt);
        if (Missiles > 0)
        {
            missileTimer += dt;
            if (missileTimer >= 16f)
            {
                missileTimer = 0f;
                Missiles--;
                Fox2Timer = 2.2f;
            }
        }
        else
        {
            rearmTimer += dt;
            if (rearmTimer >= 9f)
            {
                rearmTimer = 0f;
                Missiles = 6;
            }
        }

        foreach (var contact in Contacts)
        {
            contact.BearingDeg = Wrap360(contact.BearingDeg + (contact.BearingDrift * dt));
            contact.RangeNm += contact.RangeRate * dt;
            if (contact.RangeNm is < 2.5f or > 38f)
            {
                contact.RangeRate = -contact.RangeRate;
                contact.RangeNm = Math.Clamp(contact.RangeNm, 2.5f, 38f);
            }
        }
    }

    public static float Wrap360(float value) => ((value % 360f) + 360f) % 360f;
}

//--------------------------------------------------------------------------------
// Scene
//--------------------------------------------------------------------------------
public sealed class FlightHudScene : SceneObject
{
    private const float BaseWidth = 400f;

    private static readonly SKColor BgTop = new(0x04, 0x0B, 0x14);
    private static readonly SKColor BgBottom = new(0x01, 0x04, 0x08);
    private static readonly SKColor Main = new(0x46, 0xF1, 0xC8);
    private static readonly SKColor Bright = new(0xDB, 0xFF, 0xF4);
    private static readonly SKColor Amber = new(0xFF, 0xB4, 0x3E);
    private static readonly SKColor Red = new(0xFF, 0x55, 0x55);
    private static readonly SKColor Blue = new(0x58, 0xB6, 0xFF);
    private static readonly SKColor Dim = new(0x6E, 0xA8, 0x9C);
    private static readonly SKColor PanelTop = new(0x08, 0x17, 0x1D);
    private static readonly SKColor PanelBottom = new(0x04, 0x0C, 0x12);
    private static readonly SKColor PanelEdge = new(0x17, 0x4A, 0x43);
    private static readonly SKColor SkyTop = new(0x05, 0x16, 0x34);
    private static readonly SKColor SkyHorizon = new(0x1B, 0x52, 0x8A);
    private static readonly SKColor GroundHorizon = new(0x5A, 0x44, 0x26);
    private static readonly SKColor GroundBottom = new(0x17, 0x10, 0x09);
    private static readonly SKColor TapeFill = new SKColor(0x00, 0x07, 0x0B).WithAlpha(170);

    private static readonly int[] RollMarks = [-60, -45, -30, -20, -10, 10, 20, 30, 45, 60];

    // 縦の配置。上から見出し、姿勢の表示 (PFD)、レーダーと兵装、下の状態の行
    private readonly record struct FlightLayout(SKRect Header, SKRect Pfd, SKRect Radar, SKRect Stores, SKRect Footer, float AttitudeY, float PxPerDeg, SKPoint RadarCenter, float RadarR);

    private readonly FlightHudSim sim = new();
    private readonly SKPathBuilder pathBuilder = new();

    private FlightContact? selectedContact;
    private float lastSpeed = -1f;
    private float speedTrend;

    protected override void Update(float t, float dt)
    {
        sim.Update(t, dt);

        // 6 秒後の速度の見込み (加速度を滑らかにして使う)
        if ((lastSpeed >= 0f) && (dt > 0f))
        {
            var accel = (sim.SpeedKt - lastSpeed) / dt;
            speedTrend += ((accel * 6f) - speedTrend) * MathF.Min(1f, dt * 2f);
        }

        lastSpeed = sim.SpeedKt;
    }

    // レーダー上のブリップをタップで選択する (SceneControl → OnTouch のヒットテスト例)
    protected override bool OnTouch(SKPoint location, int width, int height)
    {
        var s = width / BaseWidth;
        var vx = location.X / s;
        var vy = location.Y / s;
        var layout = ComputeLayout(height / s);
        var center = layout.RadarCenter;
        var r = layout.RadarR;

        var dx = vx - center.X;
        var dy = vy - center.Y;
        if (((dx * dx) + (dy * dy)) > ((r + 8f) * (r + 8f)))
        {
            return false;
        }

        FlightContact? nearest = null;
        var nearestDistance = 16f;
        foreach (var contact in sim.Contacts)
        {
            if (contact.RangeNm > 40f)
            {
                continue;
            }

            var p = ContactPoint(contact, center, r, 0f);
            var distance = MathF.Sqrt(((vx - p.X) * (vx - p.X)) + ((vy - p.Y) * (vy - p.Y)));
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = contact;
            }
        }

        // 同じブリップの再タップは選択解除
        selectedContact = ReferenceEquals(selectedContact, nearest) ? null : nearest;
        return true;
    }

    protected override void OnRender(SKCanvas canvas, int width, int height)
    {
        var s = width / BaseWidth;
        var vh = height / s;
        var layout = ComputeLayout(vh);

        DrawStaticImage(canvas, width, height, s, (Scene: this, Layout: layout, Height: vh), static (c, state) => state.Scene.DrawChrome(c, state.Layout, state.Height));

        canvas.Save();
        canvas.Scale(s);
        DrawHeader(canvas, layout.Header);
        DrawPfd(canvas, layout);
        DrawRadar(canvas, layout);
        DrawStores(canvas, layout.Stores);
        canvas.Restore();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            pathBuilder.Dispose();
        }

        base.Dispose(disposing);
    }

    //--------------------------------------------------------------------------------
    // Layout / chrome
    //--------------------------------------------------------------------------------

    private static FlightLayout ComputeLayout(float vh)
    {
        var header = new SKRect(8f, 4f, 392f, 40f);
        var footer = new SKRect(8f, vh - 26f, 392f, vh - 6f);
        var lowerHeight = Math.Clamp(vh * 0.36f, 228f, 266f);
        var lowerTop = footer.Top - 6f - lowerHeight;
        var pfd = new SKRect(8f, header.Bottom + 4f, 392f, lowerTop - 6f);
        var radar = new SKRect(8f, lowerTop, 197f, footer.Top - 6f);
        var stores = new SKRect(203f, lowerTop, 392f, footer.Top - 6f);
        var radarR = MathF.Min((radar.Width - 28f) / 2f, (radar.Height - 80f) / 2f);
        var radarCenter = new SKPoint(radar.MidX, radar.Top + 34f + radarR + ((radar.Height - 82f - (radarR * 2f)) / 2f));
        return new FlightLayout(header, pfd, radar, stores, footer, pfd.Top + (pfd.Height * 0.55f), pfd.Height / 46f, radarCenter, radarR);
    }

    private void DrawChrome(SKCanvas canvas, FlightLayout layout, float vh)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var background = SKShader.CreateLinearGradient(new SKPoint(0f, 0f), new SKPoint(0f, vh), [BgTop, BgBottom], [0f, 1f], SKShaderTileMode.Clamp);
        paint.Shader = background;
        canvas.DrawRect(0f, 0f, BaseWidth, vh, paint);
        paint.Shader = null;

        // Header
        var mode = new SKRect(layout.Header.Left, layout.Header.Top + 2f, layout.Header.Left + 116f, layout.Header.Top + 20f);
        DrawChip(canvas, mode, "MODE A/A CBT", Amber, true);
        Fill.Color = Main;
        canvas.DrawCircle(layout.Header.Left + 5f, layout.Header.Top + 30f, 2.6f, Fill);
        DrawText(canvas, "SYS NOMINAL", layout.Header.Left + 12f, layout.Header.Top + 33.5f, 9.5f, Main.WithAlpha(200));

        // Radar
        var radar = layout.Radar;
        DrawPanel(canvas, radar);
        DrawText(canvas, "RDR A-A", radar.Left + 12f, radar.Top + 19f, 10.5f, Main, bold: true);
        DrawChip(canvas, new SKRect(radar.Right - 92f, radar.Top + 7f, radar.Right - 54f, radar.Top + 23f), "TWS", Main, false);
        DrawChip(canvas, new SKRect(radar.Right - 50f, radar.Top + 7f, radar.Right - 10f, radar.Top + 23f), "40NM", Main, false);
        DrawRadarScope(canvas, layout.RadarCenter, layout.RadarR);

        // Stores
        var stores = layout.Stores;
        DrawPanel(canvas, stores);
        DrawText(canvas, "STORES", stores.Left + 12f, stores.Top + 19f, 10.5f, Main, bold: true);
        DrawAircraft(canvas, stores.MidX, stores.Top + 32f);
        DrawText(canvas, "GUN 20MM", stores.Left + 12f, stores.Top + 168f, 10.5f, Main, bold: true);
        DrawText(canvas, "AAM-4 [IR]", stores.Left + 12f, stores.Top + 212f, 10.5f, Bright, bold: true);
        DrawText(canvas, "RATE HI", stores.Left + 12f, stores.Top + 232f, 9.5f, Dim);

        // Footer
        string[] items = ["TWS AUTO", "CHAFF 24", "FLARE 24", "AP OFF"];
        var footer = layout.Footer;
        var chipWidth = (footer.Width - 18f) / items.Length;
        for (var i = 0; i < items.Length; i++)
        {
            var x = footer.Left + (i * (chipWidth + 6f));
            DrawChip(canvas, new SKRect(x, footer.Top, x + chipWidth, footer.Bottom), items[i], i == 3 ? Dim : Main, false);
        }

        // 周辺を少し暗くする
        using var vignette = SKShader.CreateRadialGradient(
            new SKPoint(200f, vh * 0.45f),
            MathF.Max(BaseWidth, vh) * 0.72f,
            [SKColors.Black.WithAlpha(0), SKColors.Black.WithAlpha(0), SKColors.Black.WithAlpha(120)],
            [0f, 0.62f, 1f],
            SKShaderTileMode.Clamp);
        paint.Shader = vignette;
        canvas.DrawRect(0f, 0f, BaseWidth, vh, paint);
        paint.Shader = null;
    }

    private void DrawPanel(SKCanvas canvas, SKRect rect)
    {
        using var shader = SKShader.CreateLinearGradient(new SKPoint(0f, rect.Top), new SKPoint(0f, rect.Bottom), [PanelTop.WithAlpha(235), PanelBottom.WithAlpha(235)], [0f, 1f], SKShaderTileMode.Clamp);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Shader = shader;
        canvas.DrawRoundRect(rect, 10f, 10f, paint);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 1f;
        Stroke.Color = PanelEdge;
        canvas.DrawRoundRect(rect, 10f, 10f, Stroke);
        Stroke.Color = Main.WithAlpha(28);
        canvas.DrawLine(rect.Left + 12f, rect.Top + 0.8f, rect.Right - 12f, rect.Top + 0.8f, Stroke);
    }

    private void DrawChip(SKCanvas canvas, SKRect rect, string text, SKColor color, bool filled)
    {
        var radius = rect.Height / 2f;
        Fill.Color = color.WithAlpha(filled ? (byte)40 : (byte)16);
        canvas.DrawRoundRect(rect, radius, radius, Fill);
        Stroke.StrokeWidth = 0.9f;
        Stroke.Color = color.WithAlpha(filled ? (byte)190 : (byte)110);
        canvas.DrawRoundRect(rect, radius, radius, Stroke);
        DrawText(canvas, text, rect.MidX, rect.MidY + 3.2f, 9f, color, bold: filled, align: SKTextAlign.Center);
    }

    //--------------------------------------------------------------------------------
    // Header
    //--------------------------------------------------------------------------------

    private void DrawHeader(SKCanvas canvas, SKRect header)
    {
        var lowFuel = sim.FuelLbs < 2000f;
        DrawText(canvas, $"FUEL {(int)sim.FuelLbs} LBS", header.Right, header.Top + 14f, 10f, lowFuel ? Amber : Main, bold: true, align: SKTextAlign.Right);
        var bar = new SKRect(header.Right - 110f, header.Top + 18f, header.Right, header.Top + 21.5f);
        Fill.Color = PanelEdge;
        canvas.DrawRoundRect(bar, 1.5f, 1.5f, Fill);
        Fill.Color = lowFuel ? Amber : Main;
        canvas.DrawRoundRect(new SKRect(bar.Left, bar.Top, bar.Left + (bar.Width * Math.Clamp(sim.FuelLbs / 6000f, 0f, 1f)), bar.Bottom), 1.5f, 1.5f, Fill);
        DrawText(canvas, $"G {sim.GForce:0.0}   THR {(int)sim.ThrottlePct}%", header.Right, header.Top + 34f, 10f, Bright, align: SKTextAlign.Right);
    }

    //--------------------------------------------------------------------------------
    // Primary flight display
    //--------------------------------------------------------------------------------

    private void DrawPfd(SKCanvas canvas, FlightLayout layout)
    {
        var pfd = layout.Pfd;
        var cx = pfd.MidX;
        var cy = layout.AttitudeY;
        var k = layout.PxPerDeg;
        var tapeHalf = MathF.Min(136f, pfd.Height * 0.34f);

        canvas.Save();
        using (var round = new SKRoundRect(pfd, 12f, 12f))
        {
            canvas.ClipRoundRect(round, SKClipOperation.Intersect, true);
        }

        // 空と地面は機体の傾き (ロール) で回し、ピッチで上下に動かす
        canvas.Save();
        canvas.Translate(cx, cy);
        canvas.RotateDegrees(-sim.RollDeg);
        canvas.Translate(0f, sim.PitchDeg * k);
        DrawSkyGround(canvas);
        canvas.Restore();

        // ピッチの目盛りはロールの目盛りの内側だけに出す
        var rollR = cy - pfd.Top - 64f;
        canvas.Save();
        canvas.ClipRect(new SKRect(cx - 112f, cy - rollR + 16f, cx + 112f, cy + rollR - 16f));
        canvas.Translate(cx, cy);
        canvas.RotateDegrees(-sim.RollDeg);
        canvas.Translate(0f, sim.PitchDeg * k);
        DrawLadder(canvas, k);
        canvas.Restore();

        DrawRollScale(canvas, cx, cy, rollR);
        DrawHeadingTape(canvas, pfd);
        DrawSpeedTape(canvas, pfd, cy, tapeHalf);
        DrawAltitudeTape(canvas, pfd, cy, tapeHalf);
        DrawFlightPath(canvas, cx, cy);
        canvas.Restore();

        Stroke.StrokeWidth = 1.2f;
        Stroke.Color = PanelEdge;
        canvas.DrawRoundRect(pfd, 12f, 12f, Stroke);
    }

    private void DrawSkyGround(SKCanvas canvas)
    {
        const float big = 700f;
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var sky = SKShader.CreateLinearGradient(new SKPoint(0f, -260f), new SKPoint(0f, 0f), [SkyTop, SkyHorizon], [0f, 1f], SKShaderTileMode.Clamp);
        paint.Shader = sky;
        canvas.DrawRect(-big, -big, big * 2f, big, paint);
        using var ground = SKShader.CreateLinearGradient(new SKPoint(0f, 0f), new SKPoint(0f, 220f), [GroundHorizon, GroundBottom], [0f, 1f], SKShaderTileMode.Clamp);
        paint.Shader = ground;
        canvas.DrawRect(-big, 0f, big * 2f, big, paint);
        paint.Shader = null;

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Bright.WithAlpha(230);
        Stroke.StrokeWidth = 2f;
        canvas.DrawLine(-big, 0f, big, 0f, Stroke);
    }

    private void DrawLadder(SKCanvas canvas, float k)
    {
        using var dash = SKPathEffect.CreateDash([7f, 5f], 0f);
        for (var p = -30; p <= 30; p += 5)
        {
            if (p == 0)
            {
                continue;
            }

            var y = -p * k;
            var major = (p % 10) == 0;
            var outer = major ? 72f : 50f;
            var tick = p > 0 ? 6f : -6f;
            Stroke.Color = Bright.WithAlpha(major ? (byte)220 : (byte)160);
            Stroke.StrokeWidth = major ? 1.6f : 1.2f;
            Stroke.PathEffect = p < 0 ? dash : null;
            canvas.DrawLine(-outer, y, -24f, y, Stroke);
            canvas.DrawLine(24f, y, outer, y, Stroke);
            Stroke.PathEffect = null;
            canvas.DrawLine(-outer, y, -outer, y + tick, Stroke);
            canvas.DrawLine(outer, y, outer, y + tick, Stroke);
            if (major)
            {
                DrawText(canvas, $"{p}", -outer - 5f, y + 3.5f, 9.5f, Bright.WithAlpha(210), align: SKTextAlign.Right);
                DrawText(canvas, $"{p}", outer + 5f, y + 3.5f, 9.5f, Bright.WithAlpha(210));
            }
        }
    }

    private void DrawRollScale(SKCanvas canvas, float cx, float cy, float r)
    {
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Bright.WithAlpha(90);
        Stroke.StrokeWidth = 1.2f;
        canvas.DrawArc(new SKPoint(cx, cy), r, 210f, 120f, false, Stroke);

        foreach (var a in RollMarks)
        {
            var angle = AngleHelper.ToSkiaAngle(a);
            var len = (Math.Abs(a) % 30) == 0 ? 11f : 6f;
            Stroke.Color = Bright.WithAlpha(200);
            Stroke.StrokeWidth = (Math.Abs(a) % 30) == 0 ? 1.8f : 1.3f;
            canvas.DrawLine(AngleHelper.GetPoint(new SKPoint(cx, cy), r, angle), AngleHelper.GetPoint(new SKPoint(cx, cy), r + len, angle), Stroke);
        }

        // 固定の目印 (上) と、傾きに合わせて回る目印 (下)
        pathBuilder.MoveTo(cx, cy - r - 1f);
        pathBuilder.LineTo(cx - 6f, cy - r - 11f);
        pathBuilder.LineTo(cx + 6f, cy - r - 11f);
        pathBuilder.Close();
        using (var index = pathBuilder.Detach())
        {
            Fill.Color = Bright;
            canvas.DrawPath(index, Fill);
        }

        canvas.Save();
        canvas.RotateDegrees(-sim.RollDeg, cx, cy);
        pathBuilder.MoveTo(cx, cy - r + 2f);
        pathBuilder.LineTo(cx - 6f, cy - r + 12f);
        pathBuilder.LineTo(cx + 6f, cy - r + 12f);
        pathBuilder.Close();
        using (var pointer = pathBuilder.Detach())
        {
            Fill.Color = Main;
            canvas.DrawPath(pointer, Fill);
        }

        canvas.Restore();
    }

    private void DrawHeadingTape(SKCanvas canvas, SKRect pfd)
    {
        const float pxPerDeg = 3.2f;
        var tape = new SKRect(pfd.Left + 74f, pfd.Top + 8f, pfd.Right - 74f, pfd.Top + 30f);
        Fill.Color = TapeFill;
        canvas.DrawRoundRect(tape, 4f, 4f, Fill);

        canvas.Save();
        canvas.ClipRect(tape);
        var heading = sim.HeadingDeg;
        var start = (MathF.Floor(heading / 5f) * 5f) - 40f;
        for (var a = start; a <= start + 80f; a += 5f)
        {
            var x = tape.MidX + ((a - heading) * pxPerDeg);
            var value = (int)FlightHudSim.Wrap360(a);
            var major = (value % 10) == 0;
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(major ? (byte)220 : (byte)140);
            Stroke.StrokeWidth = major ? 1.6f : 1.1f;
            canvas.DrawLine(x, tape.Bottom - (major ? 8f : 5f), x, tape.Bottom, Stroke);
            if ((value % 30) == 0)
            {
                var text = value switch
                {
                    0 => "N",
                    90 => "E",
                    180 => "S",
                    270 => "W",
                    _ => $"{value / 10:00}"
                };
                DrawText(canvas, text, x, tape.Top + 11.5f, 10f, (value % 90) == 0 ? Amber : Bright, bold: (value % 90) == 0, align: SKTextAlign.Center);
            }
        }

        canvas.Restore();

        var box = new SKRect(tape.MidX - 30f, tape.Bottom - 2f, tape.MidX + 30f, tape.Bottom + 19f);
        Fill.Color = SKColors.Black.WithAlpha(230);
        canvas.DrawRoundRect(box, 4f, 4f, Fill);
        Stroke.StrokeWidth = 1.2f;
        Stroke.Color = Main;
        canvas.DrawRoundRect(box, 4f, 4f, Stroke);
        DrawText(canvas, $"{(int)heading:000}°", box.MidX, box.Bottom - 5f, 13f, Bright, bold: true, align: SKTextAlign.Center);
    }

    private void DrawSpeedTape(SKCanvas canvas, SKRect pfd, float cy, float half)
    {
        const float pxPerKt = 1.2f;
        var tape = new SKRect(pfd.Left + 8f, cy - half, pfd.Left + 64f, cy + half);
        Fill.Color = TapeFill;
        canvas.DrawRoundRect(tape, 5f, 5f, Fill);

        canvas.Save();
        canvas.ClipRect(tape);
        var speed = sim.SpeedKt;
        var start = (MathF.Floor(speed / 10f) * 10f) - 130f;
        for (var v = start; v <= start + 260f; v += 10f)
        {
            if (v < 0f)
            {
                continue;
            }

            var y = cy - ((v - speed) * pxPerKt);
            var major = ((int)v % 50) == 0;
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(major ? (byte)220 : (byte)130);
            Stroke.StrokeWidth = major ? 1.6f : 1.1f;
            canvas.DrawLine(tape.Right - (major ? 10f : 6f), y, tape.Right, y, Stroke);
            if (major && (MathF.Abs(y - cy) > 16f))
            {
                DrawText(canvas, $"{(int)v}", tape.Right - 14f, y + 3.5f, 10f, Bright.WithAlpha(220), align: SKTextAlign.Right);
            }
        }

        canvas.Restore();

        // 6 秒後の速度の見込み
        var trend = Math.Clamp(speedTrend * pxPerKt, -half + 14f, half - 14f);
        if (MathF.Abs(trend) > 3f)
        {
            Stroke.Color = Main;
            Stroke.StrokeWidth = 2f;
            canvas.DrawLine(tape.Right + 3f, cy, tape.Right + 3f, cy - trend, Stroke);
            var dir = trend > 0f ? 1f : -1f;
            canvas.DrawLine(tape.Right + 3f, cy - trend, tape.Right, cy - trend + (dir * 5f), Stroke);
            canvas.DrawLine(tape.Right + 3f, cy - trend, tape.Right + 6f, cy - trend + (dir * 5f), Stroke);
        }

        DrawReadout(canvas, tape.Left - 2f, tape.Right + 6f, cy, true, $"{(int)speed:000}", 16f);
        DrawText(canvas, "SPD KT", tape.MidX, tape.Top - 6f, 9.5f, Main.WithAlpha(190), align: SKTextAlign.Center);
        DrawText(canvas, $"M {sim.Mach:0.00}", tape.MidX, tape.Bottom + 18f, 11.5f, Bright, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "AOA 4.2", tape.MidX, tape.Bottom + 32f, 10f, Main.WithAlpha(200), align: SKTextAlign.Center);
    }

    private void DrawAltitudeTape(SKCanvas canvas, SKRect pfd, float cy, float half)
    {
        const float pxPerFt = 0.25f;
        var tape = new SKRect(pfd.Right - 72f, cy - half, pfd.Right - 8f, cy + half);
        Fill.Color = TapeFill;
        canvas.DrawRoundRect(tape, 5f, 5f, Fill);

        canvas.Save();
        canvas.ClipRect(tape);
        var altitude = sim.AltitudeFt;
        var start = (MathF.Floor(altitude / 100f) * 100f) - 600f;
        for (var v = start; v <= start + 1200f; v += 100f)
        {
            var y = cy - ((v - altitude) * pxPerFt);
            var major = ((int)v % 500) == 0;
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(major ? (byte)220 : (byte)130);
            Stroke.StrokeWidth = major ? 1.6f : 1.1f;
            canvas.DrawLine(tape.Left, y, tape.Left + (major ? 10f : 6f), y, Stroke);
            if (major && (MathF.Abs(y - cy) > 16f))
            {
                DrawText(canvas, $"{(int)v}", tape.Left + 14f, y + 3.5f, 10f, Bright.WithAlpha(220));
            }
        }

        canvas.Restore();

        DrawReadout(canvas, tape.Left - 6f, tape.Right + 2f, cy, false, $"{(int)altitude}", 14f);
        DrawText(canvas, "ALT FT", tape.MidX, tape.Top - 6f, 9.5f, Main.WithAlpha(190), align: SKTextAlign.Center);
        var vs = (int)sim.ClimbFpm;
        DrawText(canvas, $"VS {vs:+0;-0}", tape.MidX, tape.Bottom + 18f, 11.5f, vs < -800 ? Amber : Bright, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "BARO 29.92", tape.MidX, tape.Bottom + 32f, 10f, Main.WithAlpha(200), align: SKTextAlign.Center);
    }

    // 目盛りの帯の上の、内側を指す読み取りの枠
    private void DrawReadout(SKCanvas canvas, float left, float right, float cy, bool pointRight, string text, float size)
    {
        const float h = 13f;
        if (pointRight)
        {
            pathBuilder.MoveTo(left, cy - h);
            pathBuilder.LineTo(right - 8f, cy - h);
            pathBuilder.LineTo(right, cy);
            pathBuilder.LineTo(right - 8f, cy + h);
            pathBuilder.LineTo(left, cy + h);
        }
        else
        {
            pathBuilder.MoveTo(right, cy - h);
            pathBuilder.LineTo(left + 8f, cy - h);
            pathBuilder.LineTo(left, cy);
            pathBuilder.LineTo(left + 8f, cy + h);
            pathBuilder.LineTo(right, cy + h);
        }

        pathBuilder.Close();
        using var path = pathBuilder.Detach();
        Fill.Color = SKColors.Black.WithAlpha(235);
        canvas.DrawPath(path, Fill);
        Stroke.StrokeWidth = 1.3f;
        Stroke.Color = Main;
        canvas.DrawPath(path, Stroke);
        var center = pointRight ? (left + right - 8f) / 2f : (left + 8f + right) / 2f;
        DrawText(canvas, text, center, cy + (size * 0.36f), size, Bright, bold: true, align: SKTextAlign.Center);
    }

    private void DrawFlightPath(SKCanvas canvas, float cx, float cy)
    {
        // 機首の向きの印 (固定)
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.Color = SKColors.Black.WithAlpha(150);
        Stroke.StrokeWidth = 4f;
        canvas.DrawLine(cx - 22f, cy, cx - 8f, cy, Stroke);
        canvas.DrawLine(cx + 8f, cy, cx + 22f, cy, Stroke);
        Stroke.Color = Amber;
        Stroke.StrokeWidth = 2.2f;
        canvas.DrawLine(cx - 22f, cy, cx - 8f, cy, Stroke);
        canvas.DrawLine(cx + 8f, cy, cx + 22f, cy, Stroke);
        Fill.Color = Amber;
        canvas.DrawCircle(cx, cy, 2.4f, Fill);

        // 飛んでいく向きの印 (少し揺れる)
        var fx = cx + (6f * MathF.Sin(Time * 0.40f));
        var fy = cy + (5f * MathF.Cos(Time * 0.31f)) - 14f;
        for (var pass = 0; pass < 2; pass++)
        {
            Stroke.Color = pass == 0 ? SKColors.Black.WithAlpha(150) : Main;
            Stroke.StrokeWidth = pass == 0 ? 4.5f : 2f;
            canvas.DrawCircle(fx, fy, 8f, Stroke);
            canvas.DrawLine(fx - 20f, fy, fx - 8f, fy, Stroke);
            canvas.DrawLine(fx + 8f, fy, fx + 20f, fy, Stroke);
            canvas.DrawLine(fx, fy - 8f, fx, fy - 15f, Stroke);
        }
    }

    //--------------------------------------------------------------------------------
    // Radar
    //--------------------------------------------------------------------------------

    private void DrawRadarScope(SKCanvas canvas, SKPoint center, float r)
    {
        using var shader = SKShader.CreateRadialGradient(center, r, [Main.WithAlpha(30), Main.WithAlpha(6)], [0f, 1f], SKShaderTileMode.Clamp);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Shader = shader;
        canvas.DrawCircle(center, r, paint);

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 1f;
        Stroke.Color = Main.WithAlpha(60);
        canvas.DrawCircle(center, r / 3f, Stroke);
        canvas.DrawCircle(center, r * 2f / 3f, Stroke);
        canvas.DrawLine(center.X - r, center.Y, center.X + r, center.Y, Stroke);
        canvas.DrawLine(center.X, center.Y - r, center.X, center.Y + r, Stroke);
        Stroke.Color = Main.WithAlpha(130);
        Stroke.StrokeWidth = 1.6f;
        canvas.DrawCircle(center, r, Stroke);
        Stroke.Color = Main.WithAlpha(40);
        Stroke.StrokeWidth = 1f;
        canvas.DrawCircle(center, r + 4f, Stroke);

        for (var a = 0; a < 360; a += 10)
        {
            var len = (a % 30) == 0 ? 7f : 3.5f;
            Stroke.Color = Main.WithAlpha((a % 30) == 0 ? (byte)150 : (byte)90);
            Stroke.StrokeWidth = (a % 30) == 0 ? 1.4f : 1f;
            canvas.DrawLine(AngleHelper.GetPoint(center, r - len, a), AngleHelper.GetPoint(center, r, a), Stroke);
        }

        DrawText(canvas, "13", center.X + (r / 3f) + 2f, center.Y - 3f, 9f, Main.WithAlpha(110));
        DrawText(canvas, "27", center.X + (r * 2f / 3f) + 2f, center.Y - 3f, 9f, Main.WithAlpha(110));
    }

    private SKPoint ContactPoint(FlightContact contact, SKPoint center, float r, float ahead)
    {
        var bearing = contact.BearingDeg + (contact.BearingDrift * ahead);
        var range = Math.Clamp(contact.RangeNm + (contact.RangeRate * ahead), 0f, 40f);
        return AngleHelper.GetPoint(center, range / 40f * r, AngleHelper.ToSkiaAngle(FlightHudSim.Wrap360(bearing - sim.HeadingDeg)));
    }

    private void DrawRadar(SKCanvas canvas, FlightLayout layout)
    {
        var center = layout.RadarCenter;
        var r = layout.RadarR;

        // Sweep (heading-up)
        canvas.Save();
        pathBuilder.AddCircle(center.X, center.Y, r);
        using (var clip = pathBuilder.Detach())
        {
            canvas.ClipPath(clip, SKClipOperation.Intersect, true);
        }

        canvas.Translate(center.X, center.Y);
        canvas.RotateDegrees(sim.SweepDeg);
        pathBuilder.MoveTo(0f, 0f);
        pathBuilder.ArcTo(new SKRect(-r, -r, r, r), 250f, 110f, false);
        pathBuilder.Close();
        using (var wedge = pathBuilder.Detach())
        {
            using var shader = SKShader.CreateSweepGradient(new SKPoint(0f, 0f), [Main.WithAlpha(0), Main.WithAlpha(0), Main.WithAlpha(120)], [0f, 0.69f, 1f]);
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Shader = shader;
            canvas.DrawPath(wedge, paint);
        }

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.Color = Bright.WithAlpha(220);
        Stroke.StrokeWidth = 1.8f;
        canvas.DrawLine(0f, 0f, r, 0f, Stroke);
        canvas.Restore();

        // Contacts (向きの線は 10 秒後の位置)
        foreach (var contact in sim.Contacts)
        {
            if (contact.RangeNm > 40f)
            {
                continue;
            }

            var p = ContactPoint(contact, center, r, 0f);
            var ahead = ContactPoint(contact, center, r, 10f);
            var screenDeg = FlightHudSim.Wrap360(FlightHudSim.Wrap360(contact.BearingDeg - sim.HeadingDeg) - 90f);
            var since = FlightHudSim.Wrap360(sim.SweepDeg - screenDeg);
            var alpha = (byte)Math.Clamp(240f - (since * 0.5f), 70f, 240f);
            var color = contact.Iff switch
            {
                FlightIff.Hostile => Red,
                FlightIff.Friendly => Blue,
                _ => Amber
            };

            Stroke.StrokeCap = SKStrokeCap.Round;
            Stroke.Color = color.WithAlpha((byte)(alpha * 0.7f));
            Stroke.StrokeWidth = 1.3f;
            canvas.DrawLine(p, ahead, Stroke);

            Fill.Color = color.WithAlpha(alpha);
            switch (contact.Iff)
            {
                case FlightIff.Hostile:
                    pathBuilder.MoveTo(p.X, p.Y - 6f);
                    pathBuilder.LineTo(p.X + 5f, p.Y);
                    pathBuilder.LineTo(p.X, p.Y + 6f);
                    pathBuilder.LineTo(p.X - 5f, p.Y);
                    pathBuilder.Close();
                    using (var diamond = pathBuilder.Detach())
                    {
                        canvas.DrawPath(diamond, Fill);
                    }

                    break;
                case FlightIff.Friendly:
                    canvas.DrawCircle(p, 4.2f, Fill);
                    break;
                default:
                    canvas.DrawRect(p.X - 3.5f, p.Y - 3.5f, 7f, 7f, Fill);
                    break;
            }

            if (ReferenceEquals(contact, selectedContact))
            {
                DrawLockBox(canvas, p);
            }
        }

        // Own ship
        pathBuilder.MoveTo(center.X, center.Y - 7f);
        pathBuilder.LineTo(center.X - 5f, center.Y + 6f);
        pathBuilder.LineTo(center.X, center.Y + 3f);
        pathBuilder.LineTo(center.X + 5f, center.Y + 6f);
        pathBuilder.Close();
        using (var own = pathBuilder.Detach())
        {
            Fill.Color = Bright;
            canvas.DrawPath(own, Fill);
        }

        // North marker (heading-up)
        var north = AngleHelper.GetPoint(center, r - 14f, AngleHelper.ToSkiaAngle(FlightHudSim.Wrap360(-sim.HeadingDeg)));
        DrawText(canvas, "N", north.X, north.Y + 3.5f, 10f, Amber, bold: true, align: SKTextAlign.Center);

        DrawText(canvas, $"CONTACTS {sim.Contacts.Length}  IFF ON", center.X, center.Y + r + 19f, 9.5f, Main.WithAlpha(180), align: SKTextAlign.Center);
        if (selectedContact is { } selected)
        {
            var iff = selected.Iff switch
            {
                FlightIff.Hostile => "HOS",
                FlightIff.Friendly => "FRD",
                _ => "UNK"
            };
            DrawText(canvas, $"LOCK {iff}  {(int)selected.BearingDeg:000}°  {selected.RangeNm:0.0}NM", center.X, center.Y + r + 35f, 9.5f, Amber, bold: true, align: SKTextAlign.Center);
        }
        else
        {
            DrawText(canvas, "TAP CONTACT TO LOCK", center.X, center.Y + r + 35f, 9f, Dim.WithAlpha(170), align: SKTextAlign.Center);
        }
    }

    private void DrawLockBox(SKCanvas canvas, SKPoint p)
    {
        const float s = 9f;
        const float c = 4f;
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Amber;
        Stroke.StrokeWidth = 1.5f;
        canvas.DrawLine(p.X - s, p.Y - s, p.X - s + c, p.Y - s, Stroke);
        canvas.DrawLine(p.X - s, p.Y - s, p.X - s, p.Y - s + c, Stroke);
        canvas.DrawLine(p.X + s, p.Y - s, p.X + s - c, p.Y - s, Stroke);
        canvas.DrawLine(p.X + s, p.Y - s, p.X + s, p.Y - s + c, Stroke);
        canvas.DrawLine(p.X - s, p.Y + s, p.X - s + c, p.Y + s, Stroke);
        canvas.DrawLine(p.X - s, p.Y + s, p.X - s, p.Y + s - c, Stroke);
        canvas.DrawLine(p.X + s, p.Y + s, p.X + s - c, p.Y + s, Stroke);
        canvas.DrawLine(p.X + s, p.Y + s, p.X + s, p.Y + s - c, Stroke);
    }

    //--------------------------------------------------------------------------------
    // Stores
    //--------------------------------------------------------------------------------

    private static readonly SKPoint[] AircraftOutline =
    [
        new(0f, 0f), new(4f, 10f), new(6f, 24f), new(8f, 42f), new(10f, 50f), new(60f, 80f), new(61f, 88f),
        new(14f, 82f), new(13f, 94f), new(30f, 106f), new(30f, 112f), new(9f, 108f), new(7f, 114f)
    ];

    private static readonly float[] StationX = [22f, 36f, 50f];

    private const float AircraftScale = 1f;

    private void DrawAircraft(SKCanvas canvas, float cx, float top)
    {
        pathBuilder.MoveTo(cx, top);
        foreach (var p in AircraftOutline)
        {
            pathBuilder.LineTo(cx + (p.X * AircraftScale), top + (p.Y * AircraftScale));
        }

        for (var i = AircraftOutline.Length - 1; i >= 0; i--)
        {
            var p = AircraftOutline[i];
            pathBuilder.LineTo(cx - (p.X * AircraftScale), top + (p.Y * AircraftScale));
        }

        pathBuilder.Close();
        using var path = pathBuilder.Detach();
        Fill.Color = Main.WithAlpha(22);
        canvas.DrawPath(path, Fill);
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 1.3f;
        Stroke.Color = Main.WithAlpha(150);
        canvas.DrawPath(path, Stroke);
        Stroke.Color = Main.WithAlpha(60);
        canvas.DrawLine(cx, top + (14f * AircraftScale), cx, top + (104f * AircraftScale), Stroke);
    }

    private void DrawStores(SKCanvas canvas, SKRect stores)
    {
        var t = Time;
        DrawChip(canvas, new SKRect(stores.Right - 78f, stores.Top + 7f, stores.Right - 10f, stores.Top + 23f), "ARM ON", Amber, true);

        // 翼の下の 6 本のミサイル (左右で外側から撃つ)
        var cx = stores.MidX;
        var top = stores.Top + 32f;
        for (var i = 0; i < 6; i++)
        {
            var side = (i % 2) == 0 ? -1f : 1f;
            var station = StationX[2 - (i / 2)];
            var x = cx + (side * station * AircraftScale);
            var y = top + ((60f + (station * 0.38f)) * AircraftScale);
            var live = i >= (6 - sim.Missiles);
            DrawMissile(canvas, x, y, live);
            if (live && (i == (6 - sim.Missiles)))
            {
                Stroke.StrokeWidth = 1.2f;
                Stroke.Color = Amber.WithAlpha(Blink(t, 1.5f) ? (byte)230 : (byte)120);
                canvas.DrawRoundRect(new SKRect(x - 6f, y - 13f, x + 6f, y + 13f), 3f, 3f, Stroke);
            }
        }

        // Gun
        var gunColor = sim.GunFiring ? (Blink(t, 8f) ? Bright : Amber) : Bright;
        DrawText(canvas, $"{sim.GunAmmo:000}", stores.Right - 12f, stores.Top + 169f, 15f, gunColor, bold: true, align: SKTextAlign.Right);
        if (sim.GunFiring)
        {
            DrawText(canvas, "FIRING", stores.Right - 56f, stores.Top + 168f, 9.5f, Red, bold: true, align: SKTextAlign.Right);
        }

        const int cells = 10;
        var left = stores.Left + 12f;
        var width = stores.Width - 24f;
        var cellW = (width - ((cells - 1) * 3f)) / cells;
        var filled = sim.GunAmmo / 510f * cells;
        for (var i = 0; i < cells; i++)
        {
            var x = left + (i * (cellW + 3f));
            var rect = new SKRect(x, stores.Top + 177f, x + cellW, stores.Top + 187f);
            if ((i < MathF.Floor(filled)) || ((i < filled) && Blink(t, 6f)))
            {
                Fill.Color = Main.WithAlpha(i < MathF.Floor(filled) ? (byte)220 : (byte)150);
                canvas.DrawRoundRect(rect, 1.5f, 1.5f, Fill);
            }
            else
            {
                Stroke.StrokeWidth = 1f;
                Stroke.Color = Main.WithAlpha(60);
                canvas.DrawRoundRect(rect, 1.5f, 1.5f, Stroke);
            }
        }

        // Missiles
        DrawText(canvas, $"RDY {sim.Missiles}/6", stores.Right - 12f, stores.Top + 212f, 10.5f, sim.Missiles > 0 ? Main : Red, bold: true, align: SKTextAlign.Right);
        if ((sim.Fox2Timer > 0f) && Blink(t, 5f))
        {
            var banner = new SKRect(stores.Right - 84f, stores.Top + 220f, stores.Right - 12f, stores.Top + 238f);
            Fill.Color = Amber;
            canvas.DrawRoundRect(banner, 4f, 4f, Fill);
            DrawText(canvas, "FOX 2", banner.MidX, banner.MidY + 4f, 11f, SKColors.Black, bold: true, align: SKTextAlign.Center);
        }
        else if ((sim.Missiles == 0) && Blink(t, 2f))
        {
            DrawText(canvas, "WPN OUT", stores.Right - 12f, stores.Top + 233f, 10.5f, Red, bold: true, align: SKTextAlign.Right);
        }
    }

    private void DrawMissile(SKCanvas canvas, float x, float y, bool live)
    {
        pathBuilder.MoveTo(x, y - 10f);
        pathBuilder.LineTo(x + 2.4f, y - 5f);
        pathBuilder.LineTo(x + 2.4f, y + 6f);
        pathBuilder.LineTo(x + 5f, y + 10f);
        pathBuilder.LineTo(x - 5f, y + 10f);
        pathBuilder.LineTo(x - 2.4f, y + 6f);
        pathBuilder.LineTo(x - 2.4f, y - 5f);
        pathBuilder.Close();
        using var path = pathBuilder.Detach();
        if (live)
        {
            Fill.Color = Main.WithAlpha(230);
            canvas.DrawPath(path, Fill);
        }
        else
        {
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.StrokeWidth = 1f;
            Stroke.Color = Main.WithAlpha(60);
            canvas.DrawPath(path, Stroke);
        }
    }
}
