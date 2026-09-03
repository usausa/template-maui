namespace Template.MobileApp.Graphics.Scene;

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

internal sealed class MechHudSim
{
    private static readonly string[] PartNames = ["HEAD", "CORE", "L-ARM", "R-ARM", "L-LEG", "R-LEG", "L-JU", "R-JU"];

    public float HeadingDeg { get; private set; } = 15f;

    public int ActiveChannel { get; private set; }
    public string EncryptKey { get; private set; } = "KEY-3F2A";
    public bool Transmitting { get; private set; }

    public float[] PartHp { get; } = [100f, 100f, 100f, 100f, 100f, 100f, 100f, 100f];

    public IReadOnlyList<MechSquadUnit> Squad { get; } =
    [
        new() { Label = "D1-2", Platoon = 1, BaseX = -180f, BaseY = 140f, Amp = 40f, Freq = 0.09f, Phase = 0.7f },
        new() { Label = "D1-3", Platoon = 1, BaseX = -120f, BaseY = -190f, Amp = 50f, Freq = 0.07f, Phase = 2.1f },
        new() { Label = "D1-4", Platoon = 1, BaseX = 160f, BaseY = 220f, Amp = 45f, Freq = 0.11f, Phase = 4.2f },
        new() { Label = "D2", Platoon = 2, BaseX = -420f, BaseY = 420f, Amp = 30f, Freq = 0.05f, Phase = 1.3f },
        new() { Label = "D2", Platoon = 2, BaseX = -360f, BaseY = 500f, Amp = 30f, Freq = 0.06f, Phase = 3.6f },
        new() { Label = "D3", Platoon = 3, BaseX = 430f, BaseY = -430f, Amp = 35f, Freq = 0.06f, Phase = 0.4f },
        new() { Label = "D3", Platoon = 3, BaseX = 500f, BaseY = -360f, Amp = 35f, Freq = 0.08f, Phase = 5.1f }
    ];

    public IReadOnlyList<MechContact> Contacts { get; } =
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

public sealed class MechHudScene : SceneObject
{
    private const float BaseWidth = 400f;

    private static readonly SKColor BgColor = new(0x04, 0x0A, 0x06);
    private static readonly SKColor Main = new(0x7D, 0xF3, 0x6B);
    private static readonly SKColor Bright = new(0xEA, 0xFF, 0xF0);
    private static readonly SKColor Amber = new(0xFF, 0xB4, 0x3E);
    private static readonly SKColor Red = new(0xFF, 0x50, 0x50);
    private static readonly SKColor Cyan = new(0x4F, 0xD8, 0xE8);
    private static readonly SKColor Panel = new SKColor(0x06, 0x12, 0x08).WithAlpha(180);

    private static readonly float[] TerrainLevels = [-0.55f, -0.25f, 0.06f, 0.36f, 0.65f];

    private readonly MechHudSim sim = new();

    private SKShader? vignette;
    private int vignetteWidth;
    private int vignetteHeight;

    protected override void Update(float t, float dt) => sim.Update(t, dt);

    protected override void OnRender(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BgColor);

        var s = width / BaseWidth;
        var vh = height / s;

        canvas.Save();
        canvas.Scale(s);

        DrawStatus(canvas, Time);
        DrawComm(canvas, Time);
        DrawMap(canvas, Time, vh);
        DrawText(canvas, "GND-NAV OK   IFF ON   FCS LINKED   AP OFF", 200f, vh - 10f, 9f, Main.WithAlpha(110), align: SKTextAlign.Center);

        canvas.Restore();

        DrawVignette(canvas, width, height);
    }

    //--------------------------------------------------------------------------------
    // Status
    //--------------------------------------------------------------------------------

    private void DrawStatus(SKCanvas canvas, float t)
    {
        DrawText(canvas, "UNIT D1-1 // CO DAGGER", 16f, 32f, 10f, Main, bold: true);
        DrawText(canvas, "MODE GND COMBAT", 16f, 46f, 10f, Amber, bold: true);
        DrawText(canvas, $"T+{(int)(t / 60f):00}:{(int)(t % 60f):00}", 16f, 60f, 10f, Main.WithAlpha(180));

        DrawText(canvas, DateTime.Now.ToString("HH:mm:ss"), 384f, 32f, 10f, Main, bold: true, align: SKTextAlign.Right);
        DrawText(canvas, "DLNK ACTIVE", 384f, 46f, 9f, Main.WithAlpha(180), align: SKTextAlign.Right);

        var worstHp = sim.PartHp[sim.WorstPart];
        if (worstHp > 70f)
        {
            DrawText(canvas, "SYS NOMINAL", 384f, 60f, 10f, Main, align: SKTextAlign.Right);
        }
        else if (worstHp > 35f)
        {
            DrawText(canvas, $"DMG {sim.WorstPartName} {(int)worstHp}%", 384f, 60f, 10f, Amber, bold: true, align: SKTextAlign.Right);
        }
        else if (Blink(t, 2f))
        {
            DrawText(canvas, $"DMG CRIT {sim.WorstPartName}", 384f, 60f, 10f, Red, bold: true, align: SKTextAlign.Right);
        }
    }

    //--------------------------------------------------------------------------------
    // Comm
    //--------------------------------------------------------------------------------

    private void DrawComm(SKCanvas canvas, float t)
    {
        const float x0 = 16f;
        const float y0 = 76f;
        const float w = 368f;
        const float h = 128f;

        DrawCutPanel(canvas, x0, y0, w, h, 10f, Panel, Main.WithAlpha(150), 1.4f);

        DrawText(canvas, "COMM // DLNK", x0 + 10f, y0 + 18f, 10f, Main, bold: true);
        DrawText(canvas, sim.EncryptKey, x0 + w - 10f, y0 + 18f, 9f, Amber, align: SKTextAlign.Right);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Main.WithAlpha(110);
        Stroke.StrokeWidth = 1f;
        canvas.DrawLine(x0 + 10f, y0 + 25f, x0 + w - 10f, y0 + 25f, Stroke);

        string[] channels = ["CH1 OPEN", "CH2 SECURE", "CH3 COMMAND"];
        for (var i = 0; i < channels.Length; i++)
        {
            var y = y0 + 42f + (i * 15f);
            var active = i == sim.ActiveChannel;
            if (active)
            {
                DrawText(canvas, ">", x0 + 12f, y, 10f, Amber, bold: true);
            }

            DrawText(canvas, channels[i], x0 + 26f, y, 10f, active ? Amber : Main.WithAlpha(150), bold: active);
        }

        // Waveform
        const int bars = 20;
        var bx0 = x0 + 150f;
        var bw = (w - 160f - ((bars - 1) * 4f)) / bars;
        var baseY = y0 + 78f;
        for (var i = 0; i < bars; i++)
        {
            var amp = MathF.Abs(MathF.Sin((t * 9.7f) + (i * 1.31f)) * MathF.Sin((t * 3.3f) + (i * 0.7f)));
            var bh = ((sim.Transmitting ? 1f : 0.25f) * amp * 22f) + 2f;
            Fill.Color = Main.WithAlpha(200);
            canvas.DrawRect(bx0 + (i * (bw + 4f)), baseY - bh, bw, bh, Fill);
        }

        if (sim.Transmitting)
        {
            DrawText(canvas, "TX D1-1 >> CO", x0 + w - 10f, y0 + 100f, 9f, Amber, bold: true, align: SKTextAlign.Right);
        }
        else
        {
            DrawText(canvas, "RX NET IDLE", x0 + w - 10f, y0 + 100f, 9f, Main.WithAlpha(150), align: SKTextAlign.Right);
        }

        DrawText(canvas, "SIG ▮▮▮▮", x0 + 10f, y0 + 100f, 9f, Main.WithAlpha(170));
        DrawText(canvas, "NET GRID-7 // RELAY OK", x0 + 10f, y0 + 116f, 8f, Main.WithAlpha(130));
    }

    //--------------------------------------------------------------------------------
    // Map
    //--------------------------------------------------------------------------------

    private void DrawMap(SKCanvas canvas, float t, float vh)
    {
        const float x0 = 16f;
        const float y0 = 220f;
        const float w = 368f;
        var h = vh - y0 - 26f;

        // パネル枠 / 等高線 / グリッド / 固定ラベルは不変のためキャッシュから再生
        DrawCachedLayer(canvas, "map", BaseWidth, vh, c => DrawMapStatic(c, x0, y0, w, h));

        canvas.Save();
        using (var clip = CreateCutPanel(x0 + 2f, y0 + 2f, w - 4f, h - 4f, 10f))
        {
            canvas.ClipPath(clip);
        }

        // World -> map projection (own at center, +/-700m x)
        var scale = (w - 20f) / 1400f;
        var cx = x0 + (w / 2f);
        var cy = y0 + (h / 2f);
        float MapX(float wx) => cx + (wx * scale);
        float MapY(float wy) => cy - (wy * scale);

        // Squad
        foreach (var unit in sim.Squad)
        {
            var ux = MapX(unit.PosX(t));
            var uy = MapY(unit.PosY(t));
            if (unit.Platoon == 1)
            {
                using var triBuilder = new SKPathBuilder();
                triBuilder.MoveTo(ux, uy - 5f);
                triBuilder.LineTo(ux - 4f, uy + 4f);
                triBuilder.LineTo(ux + 4f, uy + 4f);
                triBuilder.Close();
                using var tri = triBuilder.Detach();
                Fill.Color = Cyan.WithAlpha(220);
                canvas.DrawPath(tri, Fill);
                DrawText(canvas, unit.Label, ux + 7f, uy + 3f, 8f, Cyan.WithAlpha(220));
            }
            else
            {
                Fill.Color = Cyan.WithAlpha(160);
                canvas.DrawCircle(ux, uy, 2.5f, Fill);
            }
        }

        // Hostiles
        foreach (var contact in sim.Contacts)
        {
            var hx = MapX(contact.X);
            var hy = MapY(contact.Y);
            using var triBuilder = new SKPathBuilder();
            triBuilder.MoveTo(hx, hy - 5.5f);
            triBuilder.LineTo(hx - 4.5f, hy + 4f);
            triBuilder.LineTo(hx + 4.5f, hy + 4f);
            triBuilder.Close();
            using var tri = triBuilder.Detach();
            Fill.Color = Red.WithAlpha(220);
            canvas.DrawPath(tri, Fill);
            DrawText(canvas, contact.Label, hx + 7f, hy + 3f, 8f, Red.WithAlpha(200));
        }

        // Own unit + ping ring
        var k = (t % 2.2f) / 2.2f;
        Stroke.Color = Cyan.WithAlpha((byte)((1f - k) * 110f));
        Stroke.StrokeWidth = 1.5f;
        canvas.DrawCircle(cx, cy, 6f + (k * 30f), Stroke);

        canvas.Save();
        canvas.RotateDegrees(sim.HeadingDeg, cx, cy);
        using (var ownBuilder = new SKPathBuilder())
        {
            ownBuilder.MoveTo(cx, cy - 7f);
            ownBuilder.LineTo(cx - 5f, cy + 6f);
            ownBuilder.LineTo(cx, cy + 3f);
            ownBuilder.LineTo(cx + 5f, cy + 6f);
            ownBuilder.Close();
            using var own = ownBuilder.Detach();
            Fill.Color = Bright.WithAlpha(224);
            canvas.DrawPath(own, Fill);
        }

        canvas.Restore();
        DrawText(canvas, "D1-1", cx + 9f, cy + 3f, 8f, Bright.WithAlpha(200));

        canvas.Restore();
    }

    private void DrawMapStatic(SKCanvas canvas, float x0, float y0, float w, float h)
    {
        DrawCutPanel(canvas, x0, y0, w, h, 12f, Panel, Main.WithAlpha(150), 1.6f);

        canvas.Save();
        using (var clip = CreateCutPanel(x0 + 2f, y0 + 2f, w - 4f, h - 4f, 10f))
        {
            canvas.ClipPath(clip);
        }

        // Terrain contour (記録は再生成時のみ行われるためパスの使い捨てで良い)
        using (var terrain = BuildTerrain(x0, y0, w, h))
        {
            Stroke.StrokeCap = SKStrokeCap.Butt;
            Stroke.Color = Main.WithAlpha(85);
            Stroke.StrokeWidth = 1f;
            canvas.DrawPath(terrain, Stroke);
        }

        // Grid A-F x 1-8
        const int cols = 6;
        const int rows = 8;
        Stroke.Color = Main.WithAlpha(36);
        for (var i = 1; i < cols; i++)
        {
            var x = x0 + (w * i / cols);
            canvas.DrawLine(x, y0, x, y0 + h, Stroke);
        }

        for (var i = 1; i < rows; i++)
        {
            var y = y0 + (h * i / rows);
            canvas.DrawLine(x0, y, x0 + w, y, Stroke);
        }

        for (var i = 0; i < cols; i++)
        {
            DrawText(canvas, ((char)('A' + i)).ToString(), x0 + (w * (i + 0.5f) / cols), y0 + 14f, 8f, Main.WithAlpha(120), align: SKTextAlign.Center);
        }

        for (var i = 0; i < rows; i++)
        {
            DrawText(canvas, (i + 1).ToString(), x0 + 8f, y0 + (h * (i + 0.5f) / rows) + 3f, 8f, Main.WithAlpha(120));
        }

        var scale = (w - 20f) / 1400f;
        var cx = x0 + (w / 2f);
        var cy = y0 + (h / 2f);
        float MapX(float wx) => cx + (wx * scale);
        float MapY(float wy) => cy - (wy * scale);

        // Objective
        var ox = MapX(250f);
        var oy = MapY(-350f);
        using (var diamondBuilder = new SKPathBuilder())
        {
            diamondBuilder.MoveTo(ox, oy - 6f);
            diamondBuilder.LineTo(ox + 6f, oy);
            diamondBuilder.LineTo(ox, oy + 6f);
            diamondBuilder.LineTo(ox - 6f, oy);
            diamondBuilder.Close();
            using var diamond = diamondBuilder.Detach();
            Stroke.Color = Amber.WithAlpha(200);
            Stroke.StrokeWidth = 1.6f;
            canvas.DrawPath(diamond, Stroke);
        }

        DrawText(canvas, "OBJ-A", ox + 9f, oy + 3f, 8f, Amber.WithAlpha(200));

        // Platoon fixed labels
        DrawText(canvas, "D2", MapX(-390f), MapY(460f) - 8f, 8f, Cyan.WithAlpha(160));
        DrawText(canvas, "D3", MapX(465f), MapY(-395f) - 8f, 8f, Cyan.WithAlpha(160));

        canvas.Restore();

        DrawText(canvas, "TAC MAP", x0, y0 - 8f, 9f, Main, bold: true);
        DrawText(canvas, "GRID 200M  N-UP", x0 + w, y0 - 8f, 9f, Main.WithAlpha(150), align: SKTextAlign.Right);
    }

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

    //--------------------------------------------------------------------------------
    // Overlay
    //--------------------------------------------------------------------------------

    private void DrawVignette(SKCanvas canvas, int width, int height)
    {
        if ((vignette is null) || (vignetteWidth != width) || (vignetteHeight != height))
        {
            vignette?.Dispose();
            vignette = SKShader.CreateRadialGradient(
                new SKPoint(width / 2f, height * 0.5f),
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
