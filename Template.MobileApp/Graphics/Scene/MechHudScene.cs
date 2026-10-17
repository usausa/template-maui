namespace Template.MobileApp.Graphics.Scene;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------
internal sealed class MechSquadUnit
{
    public string Label { get; init; } = string.Empty;
    public int Platoon { get; init; }
    public float BaseX { get; init; }
    public float BaseY { get; init; }
    public float Amp { get; init; }
    public float Freq { get; init; }
    public float Phase { get; init; }

    public float PosX(float t) => BaseX + (Amp * MathF.Sin((t * Freq) + Phase));

    public float PosY(float t) => BaseY + (Amp * MathF.Cos((t * Freq * 0.8f) + Phase));
}

internal sealed class MechContact
{
    public string Label { get; init; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Vx { get; set; }
    public float Vy { get; set; }
}

//--------------------------------------------------------------------------------
// Simulation
//--------------------------------------------------------------------------------
internal sealed class MechHudSim
{
    private static readonly string[] PartNames = ["HEAD", "CORE", "L-ARM", "R-ARM", "L-LEG", "R-LEG", "L-JU", "R-JU"];

    public float HeadingDeg { get; private set; } = 15f;

    public int ActiveChannel { get; private set; }
    public string EncryptKey { get; private set; } = "KEY-3F2A";
    public bool Transmitting { get; private set; }

    public float[] PartHp { get; } = [100f, 100f, 100f, 100f, 100f, 100f, 100f, 100f];

    public MechSquadUnit[] Squad { get; } =
    [
        new() { Label = "D1-2", Platoon = 1, BaseX = -180f, BaseY = 140f, Amp = 40f, Freq = 0.09f, Phase = 0.7f },
        new() { Label = "D1-3", Platoon = 1, BaseX = -120f, BaseY = -190f, Amp = 50f, Freq = 0.07f, Phase = 2.1f },
        new() { Label = "D1-4", Platoon = 1, BaseX = 160f, BaseY = 220f, Amp = 45f, Freq = 0.11f, Phase = 4.2f },
        new() { Label = "D2", Platoon = 2, BaseX = -420f, BaseY = 420f, Amp = 30f, Freq = 0.05f, Phase = 1.3f },
        new() { Label = "D2", Platoon = 2, BaseX = -360f, BaseY = 500f, Amp = 30f, Freq = 0.06f, Phase = 3.6f },
        new() { Label = "D3", Platoon = 3, BaseX = 430f, BaseY = -430f, Amp = 35f, Freq = 0.06f, Phase = 0.4f },
        new() { Label = "D3", Platoon = 3, BaseX = 500f, BaseY = -360f, Amp = 35f, Freq = 0.08f, Phase = 5.1f }
    ];

    public MechContact[] Contacts { get; } =
    [
        new() { Label = "E1", X = 320f, Y = 660f, Vx = -9f, Vy = -14f },
        new() { Label = "E2", X = -260f, Y = 780f, Vx = 12f, Vy = -10f },
        new() { Label = "E3", X = 540f, Y = 380f, Vx = -14f, Vy = 6f },
        new() { Label = "E4", X = -520f, Y = -640f, Vx = 10f, Vy = 12f },
        new() { Label = "E5", X = 140f, Y = -820f, Vx = -6f, Vy = 15f }
    ];

    public int WorstPart { get; private set; } = 1;

    private float channelTimer;
    private float keyTimer;
    private float txTimer;
    private float damageTimer;
    private int damageCount;

    public void Update(float t, float dt)
    {
        HeadingDeg = FlightHudSim.Wrap360(HeadingDeg + (6f * MathF.Sin(t * 0.11f) * dt));

        channelTimer += dt;
        if (channelTimer >= 13f)
        {
            channelTimer = 0f;
            ActiveChannel = (ActiveChannel + 1) % 3;
        }

        keyTimer += dt;
        if (keyTimer >= 20f)
        {
            keyTimer = 0f;
            EncryptKey = $"KEY-{0x1000 + ((int)(t * 977f) % 0xEFFF):X4}";
        }

        txTimer += dt;
        if (txTimer >= 7f)
        {
            txTimer = Transmitting ? 0f : 4.8f;
            Transmitting = !Transmitting;
        }

        damageTimer += dt;
        if (damageTimer >= 17f)
        {
            damageTimer = 0f;
            damageCount++;
            var part = (damageCount * 5) % PartHp.Length;
            PartHp[part] = MathF.Max(8f, PartHp[part] - (12f + ((damageCount * 7) % 18)));
        }

        var worst = 0;
        for (var i = 1; i < PartHp.Length; i++)
        {
            if (PartHp[i] < PartHp[worst])
            {
                worst = i;
            }
        }

        WorstPart = worst;

        foreach (var contact in Contacts)
        {
            contact.X += contact.Vx * dt;
            contact.Y += contact.Vy * dt;
            if (MathF.Abs(contact.X) > 640f)
            {
                contact.Vx = -contact.Vx;
            }

            if (MathF.Abs(contact.Y) > 900f)
            {
                contact.Vy = -contact.Vy;
            }
        }
    }

    public string WorstPartName => PartNames[WorstPart];
}

//--------------------------------------------------------------------------------
// Scene
//--------------------------------------------------------------------------------
public sealed class MechHudScene : SceneObject
{
    private const float BaseWidth = 400f;
    private const float WorldHalfWidth = 700f;

    private static readonly SKColor BgTop = new(0x05, 0x0D, 0x07);
    private static readonly SKColor BgBottom = new(0x01, 0x05, 0x02);
    private static readonly SKColor Main = new(0x7D, 0xF3, 0x6B);
    private static readonly SKColor Bright = new(0xEA, 0xFF, 0xF0);
    private static readonly SKColor Amber = new(0xFF, 0xB4, 0x3E);
    private static readonly SKColor Red = new(0xFF, 0x50, 0x50);
    private static readonly SKColor Cyan = new(0x4F, 0xD8, 0xE8);
    private static readonly SKColor Dim = new(0x7F, 0xA8, 0x86);
    private static readonly SKColor PanelTop = new(0x08, 0x16, 0x0C);
    private static readonly SKColor PanelBottom = new(0x04, 0x0B, 0x06);
    private static readonly SKColor PanelEdge = new(0x22, 0x50, 0x28);
    private static readonly SKColor TerrainLow = new(0x05, 0x10, 0x08);
    private static readonly SKColor TerrainMid = new(0x0E, 0x26, 0x12);
    private static readonly SKColor TerrainHigh = new(0x1E, 0x3A, 0x18);
    private static readonly SKColor TerrainPeak = new(0x38, 0x50, 0x24);

    private static readonly float[] TerrainLevels = [-0.55f, -0.25f, 0.06f, 0.36f, 0.65f];

    private static readonly string[] Channels = ["CH1 OPEN", "CH2 SECURE", "CH3 COMMAND"];

    private static readonly SKRect[] PartRects =
    [
        new(-7f, 0f, 7f, 12f),
        new(-15f, 15f, 15f, 45f),
        new(-31f, 17f, -19f, 47f),
        new(19f, 17f, 31f, 47f),
        new(-17f, 59f, -5f, 91f),
        new(5f, 59f, 17f, 91f)
    ];

    private static readonly SKPoint[] JointPoints = [new(-9f, 52f), new(9f, 52f)];

    private readonly record struct TacticalLayout(
        SKRect Header,
        SKRect Comm,
        SKRect Mech,
        SKRect Map,
        SKRect Footer);

    private readonly MechHudSim sim = new();
    private readonly SKPathBuilder pathBuilder = new();
    private readonly int[] threatOrder = new int[5];

    protected override void Update(float t, float dt) => sim.Update(t, dt);

    protected override void OnRender(SKCanvas canvas, int width, int height)
    {
        var s = width / BaseWidth;
        var vh = height / s;
        var layout = ComputeLayout(vh);

        DrawStaticImage(canvas, width, height, s, (Scene: this, Layout: layout, Height: vh), static (c, state) => state.Scene.DrawChrome(c, state.Layout, state.Height));

        canvas.Save();
        canvas.Scale(s);
        DrawHeader(canvas, layout.Header, Time);
        DrawComm(canvas, layout.Comm, Time);
        DrawMech(canvas, layout.Mech, Time);
        DrawMap(canvas, layout.Map, Time);
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

    private static TacticalLayout ComputeLayout(float vh)
    {
        var header = new SKRect(8f, 4f, 392f, 50f);
        var comm = new SKRect(8f, 56f, 254f, 188f);
        var mech = new SKRect(260f, 56f, 392f, 188f);
        var footer = new SKRect(8f, vh - 26f, 392f, vh - 6f);
        var map = new SKRect(8f, 196f, 392f, footer.Top - 6f);
        return new TacticalLayout(header, comm, mech, map, footer);
    }

    private static float MapScale(SKRect map) => (map.Width - 20f) / (WorldHalfWidth * 2f);

    private static SKPoint ToMap(SKRect map, float x, float y)
    {
        var scale = MapScale(map);
        return new SKPoint(map.MidX + (x * scale), map.MidY - (y * scale));
    }

    private void DrawChrome(SKCanvas canvas, TacticalLayout layout, float vh)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var background = SKShader.CreateLinearGradient(new SKPoint(0f, 0f), new SKPoint(0f, vh), [BgTop, BgBottom], [0f, 1f], SKShaderTileMode.Clamp);
        paint.Shader = background;
        canvas.DrawRect(0f, 0f, BaseWidth, vh, paint);
        paint.Shader = null;

        // Header
        var header = layout.Header;
        var ex = header.Left + 20f;
        var ey = header.MidY;
        for (var i = 0; i < 6; i++)
        {
            var rad = DegToRad((i * 60f) - 90f);
            var x = ex + (19f * MathF.Cos(rad));
            var y = ey + (19f * MathF.Sin(rad));
            if (i == 0)
            {
                pathBuilder.MoveTo(x, y);
            }
            else
            {
                pathBuilder.LineTo(x, y);
            }
        }

        pathBuilder.Close();
        using (var hex = pathBuilder.Detach())
        {
            Fill.Color = Main.WithAlpha(30);
            canvas.DrawPath(hex, Fill);
            Stroke.StrokeWidth = 1.6f;
            Stroke.Color = Main;
            canvas.DrawPath(hex, Stroke);
        }

        DrawText(canvas, "D1", ex, ey + 4.5f, 12f, Main, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "UNIT D1-1 // CO DAGGER", header.Left + 48f, header.Top + 17f, 11f, Main, bold: true);
        DrawChip(canvas, new SKRect(header.Left + 48f, header.Top + 25f, header.Left + 158f, header.Top + 42f), "MODE GND COMBAT", Amber, true);

        DrawPanel(canvas, layout.Comm);
        DrawText(canvas, "COMM // DLNK", layout.Comm.Left + 12f, layout.Comm.Top + 19f, 10.5f, Main, bold: true);
        Stroke.StrokeWidth = 1f;
        Stroke.Color = Main.WithAlpha(60);
        canvas.DrawLine(layout.Comm.Left + 10f, layout.Comm.Top + 27f, layout.Comm.Right - 10f, layout.Comm.Top + 27f, Stroke);

        var scope = ScopeRect(layout.Comm);
        Fill.Color = SKColors.Black.WithAlpha(110);
        canvas.DrawRoundRect(scope, 4f, 4f, Fill);
        Stroke.Color = Main.WithAlpha(40);
        for (var i = 1; i < 4; i++)
        {
            var x = scope.Left + (scope.Width * i / 4f);
            canvas.DrawLine(x, scope.Top + 2f, x, scope.Bottom - 2f, Stroke);
        }

        canvas.DrawLine(scope.Left + 2f, scope.MidY, scope.Right - 2f, scope.MidY, Stroke);
        Stroke.Color = Main.WithAlpha(90);
        canvas.DrawRoundRect(scope, 4f, 4f, Stroke);

        DrawPanel(canvas, layout.Mech);
        DrawText(canvas, "MECH", layout.Mech.Left + 12f, layout.Mech.Top + 19f, 10.5f, Main, bold: true);
        DrawText(canvas, "INTEGRITY", layout.Mech.Left + 76f, layout.Mech.Top + 44f, 8.5f, Dim);
        DrawText(canvas, "WORST", layout.Mech.Left + 76f, layout.Mech.Top + 86f, 8.5f, Dim);

        DrawMapStatic(canvas, layout.Map);

        string[] items = ["GND-NAV OK", "IFF ON", "FCS LINKED", "AP OFF"];
        var footer = layout.Footer;
        var chipWidth = (footer.Width - 18f) / items.Length;
        for (var i = 0; i < items.Length; i++)
        {
            var x = footer.Left + (i * (chipWidth + 6f));
            DrawChip(canvas, new SKRect(x, footer.Top, x + chipWidth, footer.Bottom), items[i], i == 3 ? Dim : Main, false);
        }

        using var vignette = SKShader.CreateRadialGradient(
            new SKPoint(200f, vh * 0.5f),
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
        using var path = CreateCutPanel(rect.Left, rect.Top, rect.Width, rect.Height, 9f);
        canvas.DrawPath(path, paint);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 1.2f;
        Stroke.Color = PanelEdge;
        canvas.DrawPath(path, Stroke);
        Stroke.Color = Main.WithAlpha(150);
        Stroke.StrokeWidth = 2f;
        canvas.DrawLine(rect.Left + 12f, rect.Top, rect.Left + 40f, rect.Top, Stroke);
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

    private void DrawHeader(SKCanvas canvas, SKRect header, float t)
    {
        DrawText(canvas, $"T+{(int)(t / 60f):00}:{(int)(t % 60f):00}", header.Left + 166f, header.Top + 38f, 10f, Main.WithAlpha(190));
        DrawText(canvas, DateTime.Now.ToString("HH:mm:ss"), header.Right, header.Top + 18f, 13f, Main, bold: true, align: SKTextAlign.Right);

        var worstHp = sim.PartHp[sim.WorstPart];
        var (text, color) = worstHp > 70f
            ? ("SYS NOMINAL", Main)
            : worstHp > 35f ? ($"DMG {sim.WorstPartName} {(int)worstHp}%", Amber) : ($"DMG CRIT {sim.WorstPartName}", Red);
        var width = MeasureText(text, 9f, true) + 18f;
        var chip = new SKRect(header.Right - width, header.Top + 25f, header.Right, header.Top + 42f);
        if ((worstHp > 35f) || Blink(t, 2f))
        {
            DrawChip(canvas, chip, text, color, true);
        }

        Fill.Color = Main.WithAlpha(Blink(t, 1.2f) ? (byte)255 : (byte)80);
        canvas.DrawCircle(chip.Left - 76f, chip.MidY, 2.6f, Fill);
        DrawText(canvas, "DLNK ACTIVE", chip.Left - 70f, chip.MidY + 3.2f, 9f, Main.WithAlpha(200));
    }

    //--------------------------------------------------------------------------------
    // Comm
    //--------------------------------------------------------------------------------

    private static SKRect ScopeRect(SKRect comm) => new(comm.Left + 132f, comm.Top + 36f, comm.Right - 10f, comm.Top + 88f);

    private void DrawComm(SKCanvas canvas, SKRect panel, float t)
    {
        var keyWidth = MeasureText(sim.EncryptKey, 9f) + 16f;
        DrawChip(canvas, new SKRect(panel.Right - 10f - keyWidth, panel.Top + 8f, panel.Right - 10f, panel.Top + 23f), sim.EncryptKey, Amber, false);

        for (var i = 0; i < Channels.Length; i++)
        {
            var y = panel.Top + 46f + (i * 17f);
            var active = i == sim.ActiveChannel;
            if (active)
            {
                Fill.Color = Amber.WithAlpha(34);
                canvas.DrawRoundRect(new SKRect(panel.Left + 8f, y - 12f, panel.Left + 124f, y + 4f), 3f, 3f, Fill);
                Fill.Color = Amber;
                canvas.DrawRect(panel.Left + 8f, y - 12f, 3f, 16f, Fill);
            }

            DrawText(canvas, Channels[i], panel.Left + 18f, y, 10.5f, active ? Amber : Main.WithAlpha(150), bold: active);
        }

        var scope = ScopeRect(panel);
        var amp = sim.Transmitting ? (scope.Height * 0.38f) : (scope.Height * 0.08f);
        const int points = 48;
        for (var i = 0; i <= points; i++)
        {
            var u = i / (float)points;
            var x = scope.Left + 3f + ((scope.Width - 6f) * u);
            var wave = (MathF.Sin((t * 9.7f) + (u * 19f)) * MathF.Sin((t * 3.3f) + (u * 7f))) + (0.35f * MathF.Sin((t * 23f) + (u * 41f)));
            var y = scope.MidY - (wave * amp * 0.75f);
            if (i == 0)
            {
                pathBuilder.MoveTo(x, y);
            }
            else
            {
                pathBuilder.LineTo(x, y);
            }
        }

        using (var wavePath = pathBuilder.Detach())
        {
            Stroke.StrokeCap = SKStrokeCap.Round;
            Stroke.Color = (sim.Transmitting ? Amber : Main).WithAlpha(60);
            Stroke.StrokeWidth = 4f;
            canvas.DrawPath(wavePath, Stroke);
            Stroke.Color = sim.Transmitting ? Amber : Main;
            Stroke.StrokeWidth = 1.5f;
            canvas.DrawPath(wavePath, Stroke);
        }

        var rowY = panel.Top + 108f;
        DrawText(canvas, "SIG", panel.Left + 12f, rowY, 10f, Main.WithAlpha(180));
        Fill.Color = Main.WithAlpha(200);
        for (var i = 0; i < 4; i++)
        {
            var barH = 4f + (i * 2.6f);
            canvas.DrawRect(panel.Left + 36f + (i * 6f), rowY - barH, 4f, barH, Fill);
        }

        if (sim.Transmitting)
        {
            Fill.Color = Amber.WithAlpha(Blink(t, 3f) ? (byte)255 : (byte)90);
            canvas.DrawCircle(scope.Left + 4f, rowY - 3.5f, 3f, Fill);
            DrawText(canvas, "TX D1-1 >> CO", scope.Left + 12f, rowY, 10f, Amber, bold: true);
        }
        else
        {
            Fill.Color = Main.WithAlpha(100);
            canvas.DrawCircle(scope.Left + 4f, rowY - 3.5f, 3f, Fill);
            DrawText(canvas, "RX NET IDLE", scope.Left + 12f, rowY, 10f, Main.WithAlpha(160));
        }

        DrawText(canvas, "NET GRID-7 // RELAY OK", panel.Left + 12f, panel.Top + 124f, 9f, Dim);
    }

    //--------------------------------------------------------------------------------
    // Mech
    //--------------------------------------------------------------------------------

    private static SKColor HpColor(float hp)
    {
        if (hp >= 70f)
        {
            return Main;
        }

        return hp >= 35f ? Amber : Red;
    }

    private void DrawMech(SKCanvas canvas, SKRect panel, float t)
    {
        var cx = panel.Left + 40f;
        var top = panel.Top + 30f;
        var worst = sim.WorstPart;

        for (var i = 0; i < PartRects.Length; i++)
        {
            var r = PartRects[i];
            var rect = new SKRect(cx + r.Left, top + r.Top, cx + r.Right, top + r.Bottom);
            SetPartPaint(i, worst, t);
            canvas.DrawRoundRect(rect, 3f, 3f, Fill);
            canvas.DrawRoundRect(rect, 3f, 3f, Stroke);
        }

        for (var j = 0; j < JointPoints.Length; j++)
        {
            var p = JointPoints[j];
            var point = new SKPoint(cx + p.X, top + p.Y);
            SetPartPaint(PartRects.Length + j, worst, t);
            canvas.DrawCircle(point, 5f, Fill);
            canvas.DrawCircle(point, 5f, Stroke);
        }

        var integrity = 0f;
        foreach (var hp in sim.PartHp)
        {
            integrity += hp;
        }

        integrity /= sim.PartHp.Length;
        var x = panel.Left + 76f;
        DrawText(canvas, $"{integrity:0}%", x, panel.Top + 64f, 17f, HpColor(integrity), bold: true);
        var worstHp = sim.PartHp[worst];
        if (worstHp > 99.5f)
        {
            DrawText(canvas, "ALL OK", x, panel.Top + 102f, 10.5f, Main, bold: true);
        }
        else
        {
            DrawText(canvas, sim.WorstPartName, x, panel.Top + 102f, 10.5f, HpColor(worstHp), bold: true);
            DrawText(canvas, $"{(int)worstHp}%", x, panel.Top + 118f, 10.5f, HpColor(worstHp));
        }
    }

    private void SetPartPaint(int index, int worst, float t)
    {
        var hp = sim.PartHp[index];
        var color = HpColor(hp);
        var critical = (hp < 20f) && !Blink(t, 2.5f);
        Fill.Color = color.WithAlpha(critical ? (byte)40 : (byte)(70 + (hp * 1.2f)));
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = index == worst ? 1.8f : 1f;
        Stroke.Color = index == worst ? Bright.WithAlpha(Blink(t, 1.5f) ? (byte)255 : (byte)140) : color.WithAlpha(200);
    }

    //--------------------------------------------------------------------------------
    // Map
    //--------------------------------------------------------------------------------

    private void DrawMapStatic(SKCanvas canvas, SKRect map)
    {
        using var frame = CreateCutPanel(map.Left, map.Top, map.Width, map.Height, 12f);
        canvas.Save();
        canvas.ClipPath(frame, SKClipOperation.Intersect, true);

        const int cols = 120;
        var rows = (int)(cols * map.Height / map.Width);
        using (var bitmap = new SKBitmap(cols + 1, rows + 1))
        {
            for (var i = 0; i <= cols; i++)
            {
                for (var j = 0; j <= rows; j++)
                {
                    var u = i / (float)cols * 3f;
                    var v = j / (float)rows * 3f;
                    var h = TerrainHeight(u, v);
                    var shade = Math.Clamp(1f + ((TerrainHeight(u - 0.02f, v - 0.02f) - h) * 5f), 0.7f, 1.35f);
                    bitmap.SetPixel(i, j, Shade(TerrainColor(h), shade));
                }
            }

            using var image = SKImage.FromBitmap(bitmap);
            canvas.DrawImage(image, map, new SKSamplingOptions(SKFilterMode.Linear));
        }

        using (var terrain = BuildTerrain(map.Left, map.Top, map.Width, map.Height))
        {
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(70);
            Stroke.StrokeWidth = 0.9f;
            canvas.DrawPath(terrain, Stroke);
        }

        // Grid A-F x 1-8
        const int gridCols = 6;
        const int gridRows = 8;
        Stroke.Color = Main.WithAlpha(34);
        Stroke.StrokeWidth = 1f;
        for (var i = 1; i < gridCols; i++)
        {
            var x = map.Left + (map.Width * i / gridCols);
            canvas.DrawLine(x, map.Top, x, map.Bottom, Stroke);
        }

        for (var i = 1; i < gridRows; i++)
        {
            var y = map.Top + (map.Height * i / gridRows);
            canvas.DrawLine(map.Left, y, map.Right, y, Stroke);
        }

        for (var i = 0; i < gridCols; i++)
        {
            DrawText(canvas, ((char)('A' + i)).ToString(), map.Left + (map.Width * (i + 0.5f) / gridCols), map.Top + 15f, 9f, Main.WithAlpha(130), align: SKTextAlign.Center);
        }

        for (var i = 0; i < gridRows; i++)
        {
            DrawText(canvas, (i + 1).ToString(), map.Left + 8f, map.Top + (map.Height * (i + 0.5f) / gridRows) + 3.5f, 9f, Main.WithAlpha(130));
        }

        var scale = MapScale(map);
        var barLeft = map.Left + 14f;
        var barY = map.Bottom - 14f;
        Stroke.Color = Bright.WithAlpha(200);
        Stroke.StrokeWidth = 1.6f;
        canvas.DrawLine(barLeft, barY, barLeft + (200f * scale), barY, Stroke);
        canvas.DrawLine(barLeft, barY - 4f, barLeft, barY, Stroke);
        canvas.DrawLine(barLeft + (200f * scale), barY - 4f, barLeft + (200f * scale), barY, Stroke);
        DrawText(canvas, "200M", barLeft + (200f * scale) + 6f, barY + 3.5f, 9f, Bright.WithAlpha(200));

        var nx = map.Right - 18f;
        var ny = map.Bottom - 22f;
        pathBuilder.MoveTo(nx, ny - 9f);
        pathBuilder.LineTo(nx + 5f, ny + 5f);
        pathBuilder.LineTo(nx, ny + 2f);
        pathBuilder.LineTo(nx - 5f, ny + 5f);
        pathBuilder.Close();
        using (var north = pathBuilder.Detach())
        {
            Fill.Color = Bright.WithAlpha(210);
            canvas.DrawPath(north, Fill);
        }

        DrawText(canvas, "N", nx, ny + 16f, 9f, Bright.WithAlpha(210), bold: true, align: SKTextAlign.Center);

        var objective = ToMap(map, 250f, -350f);
        pathBuilder.MoveTo(objective.X, objective.Y - 7f);
        pathBuilder.LineTo(objective.X + 7f, objective.Y);
        pathBuilder.LineTo(objective.X, objective.Y + 7f);
        pathBuilder.LineTo(objective.X - 7f, objective.Y);
        pathBuilder.Close();
        using (var diamond = pathBuilder.Detach())
        {
            Fill.Color = Amber.WithAlpha(60);
            canvas.DrawPath(diamond, Fill);
            Stroke.Color = Amber;
            Stroke.StrokeWidth = 1.6f;
            canvas.DrawPath(diamond, Stroke);
        }

        canvas.Restore();

        Stroke.StrokeWidth = 1.6f;
        Stroke.Color = Main.WithAlpha(160);
        canvas.DrawPath(frame, Stroke);

        DrawChip(canvas, new SKRect(map.Left + 26f, map.Top + 22f, map.Left + 96f, map.Top + 38f), "TAC MAP", Main, true);
        DrawChip(canvas, new SKRect(map.Right - 124f, map.Top + 22f, map.Right - 12f, map.Top + 38f), "GRID 200M N-UP", Main, false);
    }

    private static SKColor TerrainColor(float h)
    {
        var n = Math.Clamp((h + 1.3f) / 2.6f, 0f, 1f);
        if (n < 0.4f)
        {
            return TerrainLow.Lerp(TerrainMid, n / 0.4f);
        }

        return n < 0.75f ? TerrainMid.Lerp(TerrainHigh, (n - 0.4f) / 0.35f) : TerrainHigh.Lerp(TerrainPeak, (n - 0.75f) / 0.25f);
    }

    private static SKColor Shade(SKColor color, float k) =>
        new((byte)Math.Min(255f, color.Red * k), (byte)Math.Min(255f, color.Green * k), (byte)Math.Min(255f, color.Blue * k));

    private void DrawMap(SKCanvas canvas, SKRect map, float t)
    {
        var scale = MapScale(map);
        var own = new SKPoint(map.MidX, map.MidY);

        canvas.Save();
        using (var clip = CreateCutPanel(map.Left + 1f, map.Top + 1f, map.Width - 2f, map.Height - 2f, 11f))
        {
            canvas.ClipPath(clip, SKClipOperation.Intersect, true);
        }

        var sweep = (t * 50f) % 360f;
        canvas.Save();
        canvas.Translate(own.X, own.Y);
        canvas.RotateDegrees(sweep);
        const float sweepR = 230f;
        pathBuilder.MoveTo(0f, 0f);
        pathBuilder.ArcTo(new SKRect(-sweepR, -sweepR, sweepR, sweepR), 320f, 40f, false);
        pathBuilder.Close();
        using (var wedge = pathBuilder.Detach())
        {
            using var shader = SKShader.CreateSweepGradient(new SKPoint(0f, 0f), [Cyan.WithAlpha(0), Cyan.WithAlpha(0), Cyan.WithAlpha(46)], [0f, 0.89f, 1f]);
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Shader = shader;
            canvas.DrawPath(wedge, paint);
        }

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 1.2f;
        Stroke.Color = Cyan.WithAlpha(120);
        canvas.DrawLine(0f, 0f, sweepR, 0f, Stroke);
        canvas.Restore();

        var objective = ToMap(map, 250f, -350f);
        using var dash = SKPathEffect.CreateDash([5f, 4f], -((t * 12f) % 9f));
        Stroke.PathEffect = dash;
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 1.3f;
        Stroke.Color = Amber.WithAlpha(150);
        canvas.DrawLine(own, objective, Stroke);
        Stroke.PathEffect = null;
        var pulse = Phase(t, 1.8f);
        Stroke.Color = Amber.WithAlpha((byte)((1f - pulse) * 170f));
        canvas.DrawCircle(objective, 9f + (pulse * 14f), Stroke);
        DrawText(canvas, "OBJ-A", objective.X, objective.Y - 13f, 9.5f, Amber, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, $"{Distance(250f, -350f):0}M {Bearing(250f, -350f):000}°", objective.X, objective.Y + 22f, 9f, Amber.WithAlpha(210), align: SKTextAlign.Center);

        canvas.Save();
        canvas.Translate(own.X, own.Y);
        canvas.RotateDegrees(sim.HeadingDeg - 90f);
        pathBuilder.MoveTo(0f, 0f);
        pathBuilder.ArcTo(new SKRect(-80f, -80f, 80f, 80f), -35f, 70f, false);
        pathBuilder.Close();
        using (var cone = pathBuilder.Detach())
        {
            using var shader = SKShader.CreateRadialGradient(new SKPoint(0f, 0f), 80f, [Cyan.WithAlpha(70), Cyan.WithAlpha(0)], [0f, 1f], SKShaderTileMode.Clamp);
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Shader = shader;
            canvas.DrawPath(cone, paint);
        }

        canvas.Restore();

        // Hostiles
        using var ringDash = SKPathEffect.CreateDash([3f, 4f], 0f);
        foreach (var contact in sim.Contacts)
        {
            var p = ToMap(map, contact.X, contact.Y);
            for (var k = 1; k <= 5; k++)
            {
                var trail = ToMap(map, contact.X - (contact.Vx * k * 4f), contact.Y - (contact.Vy * k * 4f));
                Fill.Color = Red.WithAlpha((byte)(120 - (k * 20)));
                canvas.DrawCircle(trail, 1.6f, Fill);
            }

            Stroke.PathEffect = ringDash;
            Stroke.StrokeWidth = 1f;
            Stroke.Color = Red.WithAlpha(70);
            canvas.DrawCircle(p, 120f * scale, Stroke);
            Stroke.PathEffect = null;

            var angle = FlightHudSim.Wrap360(MathF.Atan2(p.Y - own.Y, p.X - own.X) * 180f / MathF.PI);
            var since = FlightHudSim.Wrap360(sweep - angle);
            var alpha = (byte)Math.Clamp(255f - (since * 0.6f), 140f, 255f);
            pathBuilder.MoveTo(p.X, p.Y - 6.5f);
            pathBuilder.LineTo(p.X + 6.5f, p.Y);
            pathBuilder.LineTo(p.X, p.Y + 6.5f);
            pathBuilder.LineTo(p.X - 6.5f, p.Y);
            pathBuilder.Close();
            using (var diamond = pathBuilder.Detach())
            {
                Fill.Color = Red.WithAlpha((byte)(alpha * 0.35f));
                canvas.DrawPath(diamond, Fill);
                Stroke.StrokeWidth = 1.6f;
                Stroke.Color = Red.WithAlpha(alpha);
                canvas.DrawPath(diamond, Stroke);
            }

            DrawText(canvas, contact.Label, p.X + 9f, p.Y + 3.5f, 9.5f, Red.WithAlpha(alpha), bold: true);
        }

        // Squad
        var labeled = 0;
        foreach (var unit in sim.Squad)
        {
            var p = ToMap(map, unit.PosX(t), unit.PosY(t));
            if (unit.Platoon == 1)
            {
                var box = new SKRect(p.X - 6f, p.Y - 4.5f, p.X + 6f, p.Y + 4.5f);
                Fill.Color = Cyan.WithAlpha(60);
                canvas.DrawRect(box, Fill);
                Stroke.StrokeWidth = 1.4f;
                Stroke.Color = Cyan;
                canvas.DrawRect(box, Stroke);
                canvas.DrawLine(box.Left, box.Top, box.Right, box.Bottom, Stroke);
                canvas.DrawLine(box.Left, box.Bottom, box.Right, box.Top, Stroke);
                DrawText(canvas, unit.Label, p.X + 9f, p.Y + 3.5f, 9.5f, Cyan, bold: true);
            }
            else
            {
                Fill.Color = Cyan.WithAlpha(170);
                canvas.DrawRect(p.X - 3f, p.Y - 3f, 6f, 6f, Fill);
                if ((labeled & (1 << unit.Platoon)) == 0)
                {
                    labeled |= 1 << unit.Platoon;
                    DrawText(canvas, unit.Label, p.X + 7f, p.Y - 5f, 9.5f, Cyan.WithAlpha(190), bold: true);
                }
            }
        }

        // Own unit + ping ring
        var ring = (t % 2.2f) / 2.2f;
        Stroke.StrokeWidth = 1.5f;
        Stroke.Color = Cyan.WithAlpha((byte)((1f - ring) * 130f));
        canvas.DrawCircle(own, 7f + (ring * 34f), Stroke);
        canvas.Save();
        canvas.RotateDegrees(sim.HeadingDeg, own.X, own.Y);
        pathBuilder.MoveTo(own.X, own.Y - 9f);
        pathBuilder.LineTo(own.X - 6.5f, own.Y + 7f);
        pathBuilder.LineTo(own.X, own.Y + 3.5f);
        pathBuilder.LineTo(own.X + 6.5f, own.Y + 7f);
        pathBuilder.Close();
        using (var chevron = pathBuilder.Detach())
        {
            Fill.Color = Bright;
            canvas.DrawPath(chevron, Fill);
        }

        canvas.Restore();
        DrawText(canvas, "D1-1", own.X + 11f, own.Y + 3.5f, 9.5f, Bright, bold: true);

        canvas.Restore();

        DrawThreats(canvas, map);
    }

    private void DrawThreats(SKCanvas canvas, SKRect map)
    {
        var count = Math.Min(threatOrder.Length, sim.Contacts.Length);
        for (var i = 0; i < count; i++)
        {
            threatOrder[i] = i;
        }

        for (var i = 1; i < count; i++)
        {
            var key = threatOrder[i];
            var keyDistance = Distance(sim.Contacts[key].X, sim.Contacts[key].Y);
            var j = i - 1;
            while ((j >= 0) && (Distance(sim.Contacts[threatOrder[j]].X, sim.Contacts[threatOrder[j]].Y) > keyDistance))
            {
                threatOrder[j + 1] = threatOrder[j];
                j--;
            }

            threatOrder[j + 1] = key;
        }

        var rows = Math.Min(3, count);
        var height = 22f + (rows * 14f);
        var box = new SKRect(map.Right - 124f, map.Bottom - 40f - height, map.Right - 12f, map.Bottom - 40f);
        Fill.Color = PanelBottom.WithAlpha(185);
        canvas.DrawRoundRect(box, 5f, 5f, Fill);
        Stroke.StrokeWidth = 1f;
        Stroke.Color = Red.WithAlpha(130);
        canvas.DrawRoundRect(box, 5f, 5f, Stroke);
        DrawText(canvas, "THREATS", box.Left + 8f, box.Top + 15f, 9.5f, Red, bold: true);
        for (var i = 0; i < rows; i++)
        {
            var contact = sim.Contacts[threatOrder[i]];
            var y = box.Top + 30f + (i * 14f);
            DrawText(canvas, contact.Label, box.Left + 8f, y, 9.5f, Red.WithAlpha(230), bold: true);
            DrawText(canvas, $"{Distance(contact.X, contact.Y):0}M", box.Left + 34f, y, 9.5f, Bright.WithAlpha(220));
            DrawText(canvas, $"{Bearing(contact.X, contact.Y):000}°", box.Right - 8f, y, 9.5f, Bright.WithAlpha(220), align: SKTextAlign.Right);
        }
    }

    private static float Distance(float x, float y) => MathF.Sqrt((x * x) + (y * y));

    private static float Bearing(float x, float y) => FlightHudSim.Wrap360(MathF.Atan2(x, y) * 180f / MathF.PI);

    private static float Phase(float t, float period) => (t % period) / period;

    private static SKPath BuildTerrain(float x0, float y0, float w, float h)
    {
        const int nx = 40;
        var ny = (int)(nx * h / w);

        using var builder = new SKPathBuilder();
        var samples = new float[nx + 1][];
        for (var i = 0; i <= nx; i++)
        {
            samples[i] = new float[ny + 1];
            for (var j = 0; j <= ny; j++)
            {
                samples[i][j] = TerrainHeight(i / (float)nx * 3f, j / (float)ny * 3f);
            }
        }

        var cellW = w / nx;
        var cellH = h / ny;
        Span<SKPoint> points = stackalloc SKPoint[4];

        foreach (var level in TerrainLevels)
        {
            for (var i = 0; i < nx; i++)
            {
                for (var j = 0; j < ny; j++)
                {
                    var count = 0;

                    var v00 = samples[i][j];
                    var v10 = samples[i + 1][j];
                    var v11 = samples[i + 1][j + 1];
                    var v01 = samples[i][j + 1];

                    var px = x0 + (i * cellW);
                    var py = y0 + (j * cellH);

                    // Top edge
                    if ((v00 - level) * (v10 - level) < 0f)
                    {
                        points[count++] = new SKPoint(px + (cellW * (level - v00) / (v10 - v00)), py);
                    }

                    // Right edge
                    if ((v10 - level) * (v11 - level) < 0f)
                    {
                        points[count++] = new SKPoint(px + cellW, py + (cellH * (level - v10) / (v11 - v10)));
                    }

                    // Bottom edge
                    if ((v01 - level) * (v11 - level) < 0f)
                    {
                        points[count++] = new SKPoint(px + (cellW * (level - v01) / (v11 - v01)), py + cellH);
                    }

                    // Left edge
                    if ((v00 - level) * (v01 - level) < 0f)
                    {
                        points[count++] = new SKPoint(px, py + (cellH * (level - v00) / (v01 - v00)));
                    }

                    if (count >= 2)
                    {
                        builder.MoveTo(points[0]);
                        builder.LineTo(points[1]);
                    }

                    if (count == 4)
                    {
                        builder.MoveTo(points[2]);
                        builder.LineTo(points[3]);
                    }
                }
            }
        }

        return builder.Detach();
    }

    private static float TerrainHeight(float u, float v) =>
        (0.45f * MathF.Sin(u * 7.3f)) +
        (0.40f * MathF.Sin(v * 5.9f)) +
        (0.28f * MathF.Sin((u + v) * 4.6f)) +
        (0.20f * MathF.Sin((u * 11.7f) - (v * 3.1f))) +
        (0.13f * MathF.Sin((v * 9.3f) + (u * 2.9f)));
}
