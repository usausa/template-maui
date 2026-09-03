namespace Template.MobileApp.Graphics.Scene;

internal enum FlightIff
{
    Hostile,
    Friendly,
    Unknown
}

internal sealed class FlightContact
{
    public FlightIff Iff { get; init; }
    public float BearingDeg { get; set; }
    public float RangeNm { get; set; }
    public float BearingDrift { get; init; }
    public float RangeRate { get; set; }
}

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

    public IReadOnlyList<FlightContact> Contacts { get; } =
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

public sealed class FlightHudScene : SceneObject
{
    private const float BaseWidth = 400f;

    private static readonly SKColor BgColor = new(0x03, 0x08, 0x10);
    private static readonly SKColor Main = new(0x46, 0xF1, 0xC8);
    private static readonly SKColor Bright = new(0xDB, 0xFF, 0xF4);
    private static readonly SKColor Amber = new(0xFF, 0xB4, 0x3E);
    private static readonly SKColor Red = new(0xFF, 0x55, 0x55);
    private static readonly SKColor Blue = new(0x58, 0xB6, 0xFF);
    private static readonly SKColor Panel = new SKColor(0x05, 0x10, 0x14).WithAlpha(180);

    private readonly FlightHudSim sim = new();

    private FlightContact? selectedContact;

    private SKShader? vignette;
    private int vignetteWidth;
    private int vignetteHeight;

    protected override void Update(float t, float dt) => sim.Update(t, dt);

    // レーダー上のブリップをタップで選択する (SceneControl → OnTouch のヒットテスト例)
    protected override bool OnTouch(SKPoint location, int width, int height)
    {
        var s = width / BaseWidth;
        var vx = location.X / s;
        var vy = location.Y / s;
        var vh = height / s;
        const float r = 84f;
        const float cx = 102f;
        var cy = vh - 152f;

        var dx = vx - cx;
        var dy = vy - cy;
        if (((dx * dx) + (dy * dy)) > ((r + 8f) * (r + 8f)))
        {
            return false;
        }

        FlightContact? nearest = null;
        var nearestDistance = 14f;
        foreach (var contact in sim.Contacts)
        {
            if (contact.RangeNm > 40f)
            {
                continue;
            }

            var rel = FlightHudSim.Wrap360(contact.BearingDeg - sim.HeadingDeg);
            var screenDeg = FlightHudSim.Wrap360(rel - 90f);
            var rad = DegToRad(screenDeg);
            var rr = contact.RangeNm / 40f * r;
            var x = cx + (rr * MathF.Cos(rad));
            var y = cy + (rr * MathF.Sin(rad));
            var distance = MathF.Sqrt(((vx - x) * (vx - x)) + ((vy - y) * (vy - y)));
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
        canvas.Clear(BgColor);

        var s = width / BaseWidth;
        var vh = height / s;

        canvas.Save();
        canvas.Scale(s);

        DrawHeadingTape(canvas, Time);
        DrawStatusRows(canvas, vh);
        DrawAttitude(canvas, Time, vh);
        DrawSpeedTape(canvas, vh);
        DrawAltitudeTape(canvas, vh);
        DrawRadar(canvas, vh);
        DrawWeapons(canvas, Time, vh);
        DrawText(canvas, "TWS AUTO   CHAFF 24   FLARE 24   AP OFF", 200f, vh - 10f, 9f, Main.WithAlpha(110), align: SKTextAlign.Center);

        canvas.Restore();

        DrawVignette(canvas, width, height);
    }

    //--------------------------------------------------------------------------------
    // Heading
    //--------------------------------------------------------------------------------

    private void DrawHeadingTape(SKCanvas canvas, float t)
    {
        _ = t;
        const float pxPerDeg = 3f;
        // MODE / FUEL の 2 行(y=34/46)と重ならないようその下から始める
        const float top = 64f;

        var heading = sim.HeadingDeg;
        var start = (MathF.Floor(heading / 5f) * 5f) - 60f;

        for (var a = start; a <= start + 125f; a += 5f)
        {
            var x = 200f + ((a - heading) * pxPerDeg);
            if (x is < 48f or > 352f)
            {
                continue;
            }

            var value = (int)FlightHudSim.Wrap360(a);
            var major = (value % 10) == 0;
            var len = major ? 10f : 6f;

            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(major ? (byte)170 : (byte)120);
            Stroke.StrokeWidth = major ? 1.8f : 1.2f;
            canvas.DrawLine(x, top, x, top + len, Stroke);

            if (!major)
            {
                continue;
            }

            if ((value % 90) == 0)
            {
                var cardinal = value switch
                {
                    0 => "N",
                    90 => "E",
                    180 => "S",
                    _ => "W"
                };
                DrawText(canvas, cardinal, x, top + 22f, 10f, Amber, bold: true, align: SKTextAlign.Center);
            }
            else if ((value % 30) == 0)
            {
                DrawText(canvas, $"{value / 10:00}", x, top + 22f, 9f, Main.WithAlpha(200), align: SKTextAlign.Center);
            }
        }

        // Center marker + readout box
        DrawGlowLine(canvas, 200f, top - 6f, 200f, top - 1f, Main, 1.8f);
        DrawCutPanel(canvas, 172f, top + 30f, 56f, 20f, 5f, Panel, Main.WithAlpha(170), 1.2f);
        DrawText(canvas, $"{(int)heading:000}°", 200f, top + 45f, 12f, Bright, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "HDG", 166f, top + 45f, 9f, Main.WithAlpha(160), align: SKTextAlign.Right);
    }

    private void DrawStatusRows(SKCanvas canvas, float vh)
    {
        _ = vh;
        DrawText(canvas, "MODE A/A CBT", 16f, 34f, 9f, Amber, bold: true);
        DrawText(canvas, "SYS NOMINAL", 16f, 46f, 9f, Main.WithAlpha(180));
        DrawText(canvas, $"FUEL {(int)sim.FuelLbs} LBS", 384f, 34f, 9f, sim.FuelLbs < 2000f ? Amber : Main.WithAlpha(180), align: SKTextAlign.Right);
        DrawText(canvas, $"G {sim.GForce:0.0}  THR {(int)sim.ThrottlePct}%", 384f, 46f, 9f, Bright, align: SKTextAlign.Right);
    }

    //--------------------------------------------------------------------------------
    // Attitude
    //--------------------------------------------------------------------------------

    private void DrawAttitude(SKCanvas canvas, float t, float vh)
    {
        const float pxPerDeg = 6.4f;
        var cy = vh * 0.36f;

        canvas.Save();
        canvas.ClipRect(new SKRect(88f, cy - 95f, 312f, cy + 95f));
        canvas.Save();
        canvas.RotateDegrees(-sim.RollDeg, 200f, cy);

        for (var p = -20; p <= 20; p += 5)
        {
            var y = cy + ((sim.PitchDeg - p) * pxPerDeg);
            if (p == 0)
            {
                DrawGlowLine(canvas, 60f, y, 172f, y, Main.WithAlpha(235), 2.2f);
                DrawGlowLine(canvas, 228f, y, 340f, y, Main.WithAlpha(235), 2.2f);
                continue;
            }

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.5f;
            paint.Color = Main.WithAlpha(170);
            if (p < 0)
            {
                using var dash = SKPathEffect.CreateDash([8f, 6f], 0f);
                paint.PathEffect = dash;
            }

            canvas.DrawLine(128f, y, 176f, y, paint);
            canvas.DrawLine(224f, y, 272f, y, paint);
            DrawText(canvas, $"{p}", 122f, y + 3f, 8f, Main.WithAlpha(170), align: SKTextAlign.Right);
            DrawText(canvas, $"{p}", 278f, y + 3f, 8f, Main.WithAlpha(170));
        }

        canvas.Restore();
        canvas.Restore();

        // Roll scale (不変のためキャッシュから再生)
        const float rollR = 112f;
        DrawCachedLayer(canvas, "roll", BaseWidth, cy + rollR + 16f, c => DrawRollScale(c, cy, rollR));

        var rollRad = DegToRad(-sim.RollDeg - 90f);
        var tx = 200f + ((rollR - 4f) * MathF.Cos(rollRad));
        var ty = cy + ((rollR - 4f) * MathF.Sin(rollRad));
        using (var triBuilder = new SKPathBuilder())
        {
            triBuilder.MoveTo(tx, ty);
            triBuilder.LineTo(tx - 5f, ty - 8f);
            triBuilder.LineTo(tx + 5f, ty - 8f);
            triBuilder.Close();
            using var tri = triBuilder.Detach();
            Fill.Color = Main;
            canvas.DrawPath(tri, Fill);
        }

        // Fixed boresight cross
        Stroke.Color = Main.WithAlpha(150);
        Stroke.StrokeWidth = 1.4f;
        canvas.DrawLine(193f, cy, 207f, cy, Stroke);
        canvas.DrawLine(200f, cy - 7f, 200f, cy + 7f, Stroke);

        // Flight path marker
        var fx = 200f + (6f * MathF.Sin(t * 0.40f));
        var fy = cy + (5f * MathF.Cos(t * 0.31f));
        Stroke.Color = Bright.WithAlpha(70);
        Stroke.StrokeWidth = 6f;
        canvas.DrawCircle(fx, fy, 9f, Stroke);
        Stroke.Color = Bright;
        Stroke.StrokeWidth = 2f;
        canvas.DrawCircle(fx, fy, 9f, Stroke);
        DrawGlowLine(canvas, fx - 21f, fy, fx - 9f, fy, Bright, 2f);
        DrawGlowLine(canvas, fx + 9f, fy, fx + 21f, fy, Bright, 2f);
        DrawGlowLine(canvas, fx, fy - 9f, fx, fy - 16f, Bright, 2f);
    }

    private void DrawRollScale(SKCanvas canvas, float cy, float rollR)
    {
        for (var a = -45; a <= 45; a += 15)
        {
            var rad = DegToRad(a - 90f);
            var x1 = 200f + (rollR * MathF.Cos(rad));
            var y1 = cy + (rollR * MathF.Sin(rad));
            var len = a == 0 ? 9f : 6f;
            var x2 = 200f + ((rollR + len) * MathF.Cos(rad));
            var y2 = cy + ((rollR + len) * MathF.Sin(rad));
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(150);
            Stroke.StrokeWidth = 1.4f;
            canvas.DrawLine(x1, y1, x2, y2, Stroke);
        }
    }

    //--------------------------------------------------------------------------------
    // Tapes
    //--------------------------------------------------------------------------------

    private void DrawSpeedTape(SKCanvas canvas, float vh)
    {
        const float pxPerKt = 1.1f;
        const float x = 50f;
        var cy = vh * 0.36f;

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Main.WithAlpha(200);
        Stroke.StrokeWidth = 1.4f;
        canvas.DrawLine(x, cy - 105f, x, cy + 105f, Stroke);

        var start = (MathF.Floor(sim.SpeedKt / 10f) * 10f) - 100f;
        for (var v = start; v <= start + 200f; v += 10f)
        {
            if (v < 0f)
            {
                continue;
            }

            var y = cy - ((v - sim.SpeedKt) * pxPerKt);
            if (MathF.Abs(y - cy) > 100f)
            {
                continue;
            }

            var major = ((int)v % 50) == 0;
            Stroke.Color = Main.WithAlpha(major ? (byte)170 : (byte)120);
            Stroke.StrokeWidth = major ? 1.8f : 1.2f;
            canvas.DrawLine(x, y, x + (major ? 9f : 5f), y, Stroke);
            if (major && (MathF.Abs(y - cy) > 14f))
            {
                DrawText(canvas, $"{(int)v}", x + 13f, y + 3f, 9f, Main.WithAlpha(170));
            }
        }

        DrawCutPanel(canvas, 14f, cy - 11f, 62f, 22f, 5f, Panel, Main.WithAlpha(170), 1.2f);
        DrawText(canvas, $"{(int)sim.SpeedKt:000}", 45f, cy + 5f, 14f, Bright, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "SPD", 45f, cy - 116f, 9f, Main.WithAlpha(160), align: SKTextAlign.Center);
        DrawText(canvas, $"M {sim.Mach:0.00}", 45f, cy + 126f, 10f, Bright, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "AOA 4.2", 45f, cy + 140f, 9f, Main.WithAlpha(170), align: SKTextAlign.Center);
    }

    private void DrawAltitudeTape(SKCanvas canvas, float vh)
    {
        const float pxPerFt = 0.22f;
        const float x = 350f;
        var cy = vh * 0.36f;

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Main.WithAlpha(200);
        Stroke.StrokeWidth = 1.4f;
        canvas.DrawLine(x, cy - 105f, x, cy + 105f, Stroke);

        var start = (MathF.Floor(sim.AltitudeFt / 100f) * 100f) - 500f;
        for (var v = start; v <= start + 1000f; v += 100f)
        {
            var y = cy + ((v - sim.AltitudeFt) * pxPerFt * -1f);
            if (MathF.Abs(y - cy) > 100f)
            {
                continue;
            }

            var major = ((int)v % 500) == 0;
            Stroke.Color = Main.WithAlpha(major ? (byte)170 : (byte)120);
            Stroke.StrokeWidth = major ? 1.8f : 1.2f;
            canvas.DrawLine(x - (major ? 9f : 5f), y, x, y, Stroke);
            if (major && (MathF.Abs(y - cy) > 14f))
            {
                DrawText(canvas, $"{(int)(v / 100f)}", x - 13f, y + 3f, 9f, Main.WithAlpha(170), align: SKTextAlign.Right);
            }
        }

        DrawCutPanel(canvas, 324f, cy - 11f, 62f, 22f, 5f, Panel, Main.WithAlpha(170), 1.2f);
        DrawText(canvas, $"{(int)sim.AltitudeFt}", 355f, cy + 5f, 12f, Bright, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "ALT", 355f, cy - 116f, 9f, Main.WithAlpha(160), align: SKTextAlign.Center);
        var vs = (int)sim.ClimbFpm;
        DrawText(canvas, $"VS {vs:+0;-0}", 355f, cy + 126f, 10f, vs < -800 ? Amber : Bright, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "BARO 29.92", 355f, cy + 140f, 9f, Main.WithAlpha(170), align: SKTextAlign.Center);
    }

    //--------------------------------------------------------------------------------
    // Radar
    //--------------------------------------------------------------------------------

    private void DrawRadar(SKCanvas canvas, float vh)
    {
        const float r = 84f;
        var cx = 102f;
        var cy = vh - 152f;

        // 不変のスコープ盤面 (背景円 / リング / 十字 / 方位目盛) はキャッシュから再生
        DrawCachedLayer(canvas, "radar", BaseWidth, vh, c => DrawRadarChrome(c, cx, cy, r));

        // Sweep (heading-up)
        canvas.Save();
        using (var clipPath = CreateCirclePath(cx, cy, r))
        {
            canvas.ClipPath(clipPath);
        }
        canvas.Translate(cx, cy);
        canvas.RotateDegrees(sim.SweepDeg);
        using (var wedgeBuilder = new SKPathBuilder())
        {
            wedgeBuilder.MoveTo(0f, 0f);
            wedgeBuilder.ArcTo(new SKRect(-r, -r, r, r), 260f, 100f, false);
            wedgeBuilder.Close();
            using var wedge = wedgeBuilder.Detach();

            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Fill;
            using var shader = SKShader.CreateSweepGradient(
                new SKPoint(0f, 0f),
                [Main.WithAlpha(0), Main.WithAlpha(0), Main.WithAlpha(110)],
                [0f, 0.72f, 1f]);
            paint.Shader = shader;
            canvas.DrawPath(wedge, paint);
        }

        Stroke.Color = Main.WithAlpha(220);
        Stroke.StrokeWidth = 1.6f;
        canvas.DrawLine(0f, 0f, r, 0f, Stroke);
        canvas.Restore();

        // Blips
        foreach (var contact in sim.Contacts)
        {
            if (contact.RangeNm > 40f)
            {
                continue;
            }

            var rel = FlightHudSim.Wrap360(contact.BearingDeg - sim.HeadingDeg);
            var screenDeg = FlightHudSim.Wrap360(rel - 90f);
            var rad = DegToRad(screenDeg);
            var rr = contact.RangeNm / 40f * r;
            var x = cx + (rr * MathF.Cos(rad));
            var y = cy + (rr * MathF.Sin(rad));

            var since = FlightHudSim.Wrap360(sim.SweepDeg - screenDeg);
            var alpha = (byte)Math.Clamp(235f - (since * 0.55f), 50f, 235f);

            switch (contact.Iff)
            {
                case FlightIff.Hostile:
                    using (var triBuilder = new SKPathBuilder())
                    {
                        triBuilder.MoveTo(x, y - 5f);
                        triBuilder.LineTo(x - 4.5f, y + 3.5f);
                        triBuilder.LineTo(x + 4.5f, y + 3.5f);
                        triBuilder.Close();
                        using var tri = triBuilder.Detach();
                        Fill.Color = Red.WithAlpha(alpha);
                        canvas.DrawPath(tri, Fill);
                    }

                    break;
                case FlightIff.Friendly:
                    Fill.Color = Blue.WithAlpha(alpha);
                    canvas.DrawCircle(x, y, 3.5f, Fill);
                    break;
                default:
                    Fill.Color = Amber.WithAlpha(alpha);
                    canvas.DrawRect(x - 3f, y - 3f, 6f, 6f, Fill);
                    break;
            }

            // タップ選択中のブリップを囲う
            if (ReferenceEquals(contact, selectedContact))
            {
                Stroke.Color = Bright;
                Stroke.StrokeWidth = 1.5f;
                canvas.DrawCircle(x, y, 7.5f, Stroke);
            }
        }

        // Own ship
        using (var ownBuilder = new SKPathBuilder())
        {
            ownBuilder.MoveTo(cx, cy - 6f);
            ownBuilder.LineTo(cx - 4.5f, cy + 5f);
            ownBuilder.LineTo(cx + 4.5f, cy + 5f);
            ownBuilder.Close();
            using var own = ownBuilder.Detach();
            Fill.Color = Bright;
            canvas.DrawPath(own, Fill);
        }

        // North marker (heading-up)
        var northRad = DegToRad(FlightHudSim.Wrap360(-sim.HeadingDeg) - 90f);
        DrawText(
            canvas,
            "N",
            cx + ((r - 13f) * MathF.Cos(northRad)),
            cy + ((r - 13f) * MathF.Sin(northRad)) + 3f,
            9f,
            Amber,
            bold: true,
            align: SKTextAlign.Center);

        DrawText(canvas, "RDR A-A", cx - r, cy - r - 10f, 9f, Main, bold: true);
        DrawText(canvas, "40NM", cx + r, cy - r - 10f, 9f, Main.WithAlpha(170), align: SKTextAlign.Right);
        DrawText(canvas, $"CONTACTS {sim.Contacts.Count}  IFF ON", cx, cy + r + 14f, 8f, Main.WithAlpha(150), align: SKTextAlign.Center);

        // 選択中ターゲットの情報
        if (selectedContact is { } selected)
        {
            var iff = selected.Iff switch
            {
                FlightIff.Hostile => "HOS",
                FlightIff.Friendly => "FRD",
                _ => "UNK"
            };
            DrawText(canvas, $"TGT {iff}  BRG {(int)selected.BearingDeg:000}  RNG {selected.RangeNm:0.0}NM", cx, cy + r + 26f, 8f, Amber, bold: true, align: SKTextAlign.Center);
        }
    }

    private void DrawRadarChrome(SKCanvas canvas, float cx, float cy, float r)
    {
        Fill.Color = Panel;
        canvas.DrawCircle(cx, cy, r, Fill);

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Main.WithAlpha(90);
        Stroke.StrokeWidth = 1.8f;
        canvas.DrawCircle(cx, cy, r, Stroke);
        Stroke.Color = Main.WithAlpha(50);
        Stroke.StrokeWidth = 1f;
        canvas.DrawCircle(cx, cy, r + 4f, Stroke);

        Stroke.Color = Main.WithAlpha(70);
        for (var i = 1; i <= 2; i++)
        {
            canvas.DrawCircle(cx, cy, r * i / 3f, Stroke);
        }

        canvas.DrawLine(cx - r, cy, cx + r, cy, Stroke);
        canvas.DrawLine(cx, cy - r, cx, cy + r, Stroke);

        for (var a = 0; a < 360; a += 30)
        {
            var rad = DegToRad(a);
            Stroke.Color = Main.WithAlpha(140);
            Stroke.StrokeWidth = 1.2f;
            canvas.DrawLine(
                cx + ((r - 5f) * MathF.Cos(rad)),
                cy + ((r - 5f) * MathF.Sin(rad)),
                cx + (r * MathF.Cos(rad)),
                cy + (r * MathF.Sin(rad)),
                Stroke);
        }
    }

    private static SKPath CreateCirclePath(float cx, float cy, float r)
    {
        using var builder = new SKPathBuilder();
        builder.AddCircle(cx, cy, r);
        return builder.Detach();
    }

    //--------------------------------------------------------------------------------
    // Weapons
    //--------------------------------------------------------------------------------

    private void DrawWeapons(SKCanvas canvas, float t, float vh)
    {
        const float x0 = 206f;
        const float w = 178f;
        const float h = 172f;
        var y0 = vh - 236f;

        DrawCutPanel(canvas, x0, y0, w, h, 10f, Panel, Main.WithAlpha(170), 1.4f);

        DrawText(canvas, "WPN", x0 + 10f, y0 + 18f, 11f, Main, bold: true);
        DrawText(canvas, "MASTER ARM ON", x0 + w - 10f, y0 + 18f, 8f, Amber, bold: true, align: SKTextAlign.Right);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Main.WithAlpha(120);
        Stroke.StrokeWidth = 1f;
        canvas.DrawLine(x0 + 10f, y0 + 26f, x0 + w - 10f, y0 + 26f, Stroke);

        // Gun
        var gunColor = sim.GunFiring ? (Blink(t, 8f) ? Bright : Amber) : Main;
        DrawText(canvas, "GUN", x0 + 10f, y0 + 48f, 11f, gunColor, bold: true);
        DrawText(canvas, $"{sim.GunAmmo:000}", x0 + w - 10f, y0 + 48f, 14f, Bright, bold: true, align: SKTextAlign.Right);
        if (sim.GunFiring)
        {
            DrawText(canvas, "FIRING", x0 + w - 54f, y0 + 48f, 9f, Red, bold: true, align: SKTextAlign.Right);
        }

        // Ammo bar
        const int cells = 10;
        var cellW = (w - 20f - ((cells - 1) * 3f)) / cells;
        var filled = sim.GunAmmo / 510f * cells;
        for (var i = 0; i < cells; i++)
        {
            var cx = x0 + 10f + (i * (cellW + 3f));
            var rect = new SKRect(cx, y0 + 58f, cx + cellW, y0 + 68f);
            if (i < MathF.Floor(filled))
            {
                Fill.Color = Main.WithAlpha(220);
                canvas.DrawRect(rect, Fill);
            }
            else if ((i < filled) && Blink(t, 6f))
            {
                Fill.Color = Main.WithAlpha(160);
                canvas.DrawRect(rect, Fill);
            }
            else
            {
                Stroke.Color = Main.WithAlpha(60);
                Stroke.StrokeWidth = 1f;
                canvas.DrawRect(rect, Stroke);
            }
        }

        // Missiles
        DrawText(canvas, "AAM", x0 + 10f, y0 + 94f, 11f, Main, bold: true);
        for (var i = 0; i < 6; i++)
        {
            var mx = x0 + 52f + (i * 20f);
            DrawMissileIcon(canvas, mx, y0 + 84f, i < sim.Missiles);
        }

        DrawText(canvas, "SEL AAM-4 [IR]", x0 + 10f, y0 + 126f, 9f, Bright);
        if ((sim.Fox2Timer > 0f) && Blink(t, 5f))
        {
            DrawText(canvas, "FOX 2!", x0 + w - 10f, y0 + 126f, 11f, Amber, bold: true, align: SKTextAlign.Right);
        }

        if (sim.Missiles == 0)
        {
            if (Blink(t, 2f))
            {
                DrawText(canvas, "WPN OUT", x0 + 10f, y0 + 146f, 10f, Red, bold: true);
            }
        }
        else
        {
            DrawText(canvas, $"RDY {sim.Missiles}/6", x0 + 10f, y0 + 146f, 9f, Main.WithAlpha(170));
        }

        DrawText(canvas, "GUN 20MM  RATE HI", x0 + w - 10f, y0 + 146f, 8f, Main.WithAlpha(140), align: SKTextAlign.Right);
    }

    private void DrawMissileIcon(SKCanvas canvas, float x, float y, bool live)
    {
        using var builder = new SKPathBuilder();
        builder.MoveTo(x, y - 9f);
        builder.LineTo(x + 3f, y - 3f);
        builder.LineTo(x + 3f, y + 6f);
        builder.LineTo(x + 6f, y + 10f);
        builder.LineTo(x - 6f, y + 10f);
        builder.LineTo(x - 3f, y + 6f);
        builder.LineTo(x - 3f, y - 3f);
        builder.Close();
        using var path = builder.Detach();

        if (live)
        {
            Fill.Color = Main.WithAlpha(220);
            canvas.DrawPath(path, Fill);
        }
        else
        {
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(45);
            Stroke.StrokeWidth = 1.2f;
            canvas.DrawPath(path, Stroke);
        }
    }

    //--------------------------------------------------------------------------------
    // Overlay
    //--------------------------------------------------------------------------------

    private void DrawVignette(SKCanvas canvas, int width, int height)
    {
        if ((vignette is null) || (vignetteWidth != width) || (vignetteHeight != height))
        {
            vignette?.Dispose();
            vignette = SKShader.CreateRadialGradient(
                new SKPoint(width / 2f, height * 0.45f),
                Math.Max(width, height) * 0.72f,
                [SKColors.Black.WithAlpha(0), SKColors.Black.WithAlpha(0), SKColors.Black.WithAlpha(150)],
                [0f, 0.62f, 1f],
                SKShaderTileMode.Clamp);
            vignetteWidth = width;
            vignetteHeight = height;
        }

        using var paint = new SKPaint();
        paint.Shader = vignette;
        canvas.DrawRect(0f, 0f, width, height, paint);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            vignette?.Dispose();
            vignette = null;
        }

        base.Dispose(disposing);
    }
}
