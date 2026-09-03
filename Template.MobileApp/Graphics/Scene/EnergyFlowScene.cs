namespace Template.MobileApp.Graphics.Scene;

internal sealed class EnergySim
{
    private readonly float[] nominal = [1800f, 300f, 450f, 750f, 600f, 500f, 520f, 7.5f, 4.5f, 2.8f];
    private readonly float[] values = new float[10];

    public float GridKw => values[0];
    public float PvKw => values[1];
    public float CgsKw => values[2];
    public float LineAKw => values[3];
    public float LineBKw => values[4];
    public float UtilKw => values[5];
    public float GasBlr => values[6];
    public float SteamBlr => values[7];
    public float SteamHeat => values[8];
    public float SteamDry => values[9];

    public EnergySim()
    {
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = nominal[i];
        }
    }

    public void Update(float t, float dt)
    {
        for (var i = 0; i < values.Length; i++)
        {
            var noise = (0.6f * MathF.Sin((t * 0.53f) + (i * 2.3f))) + (0.4f * MathF.Sin((t * 1.31f) + (i * 5.1f)));
            var target = nominal[i] * (1f + (0.07f * noise));
            values[i] += (target - values[i]) * MathF.Min(1f, dt / 1.6f);
        }
    }

    public static float Flow(float value, float nominalValue) => Math.Clamp(value / nominalValue, 0f, 1.3f);
}

public sealed class EnergyFlowScene : SceneObject
{
    private const float BaseWidth = 400f;

    private static readonly SKColor BgColor = new(0x0B, 0x11, 0x18);
    private static readonly SKColor CardBg = new(0x16, 0x1F, 0x2B);
    private static readonly SKColor CardBorder = new(0x24, 0x31, 0x42);
    private static readonly SKColor BoxTop = new(0x1C, 0x27, 0x35);
    private static readonly SKColor BoxBottom = new(0x12, 0x1A, 0x25);
    private static readonly SKColor BoxBorder = new(0x3A, 0x4B, 0x5F);
    private static readonly SKColor TextMain = new(0xE6, 0xED, 0xF3);
    private static readonly SKColor TextDim = new(0x8A, 0x9B, 0xAC);
    private static readonly SKColor Accent = new(0x4F, 0xC3, 0xF7);
    private static readonly SKColor Electric = new(0xFF, 0xD5, 0x4F);
    private static readonly SKColor Gas = new(0x64, 0xB5, 0xF6);
    private static readonly SKColor Steam = new(0xFF, 0x8A, 0x65);
    private static readonly SKColor Cond = new(0x4D, 0xD0, 0xE1);
    private static readonly SKColor Green = new(0x81, 0xC7, 0x84);
    private static readonly SKColor Casing = new(0x26, 0x32, 0x40);
    private static readonly SKColor Running = new(0x66, 0xBB, 0x6A);

    private readonly EnergySim sim = new();

    protected override void Update(float t, float dt) => sim.Update(t, dt);

    protected override void OnRender(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BgColor);

        var s = width / BaseWidth;
        var vh = height / s;

        canvas.Save();
        canvas.Scale(s);

        // ドット背景は不変のためキャッシュから再生する
        DrawCachedLayer(canvas, "grid", BaseWidth, vh, c => DrawDotGrid(c, vh));
        DrawHeader(canvas);
        DrawKpiCards(canvas);
        DrawElectricSection(canvas, Time);
        DrawSteamSection(canvas, Time);
        DrawDynamics(canvas, Time, vh);
        DrawLegend(canvas, vh);

        canvas.Restore();
    }

    //--------------------------------------------------------------------------------
    // Chrome
    //--------------------------------------------------------------------------------

    private void DrawDotGrid(SKCanvas canvas, float vh)
    {
        Fill.Color = new SKColor(0x18, 0x22, 0x2E);
        for (var y = 24f; y < vh; y += 24f)
        {
            for (var x = 12f; x < 400f; x += 24f)
            {
                canvas.DrawCircle(x, y, 1f, Fill);
            }
        }
    }

    private void DrawHeader(SKCanvas canvas)
    {
        DrawText(canvas, "ENERGY FLOW // PLANT-1", 16f, 30f, 12f, TextMain, bold: true);
        DrawText(canvas, DateTime.Now.ToString("HH:mm:ss"), 384f, 30f, 10f, TextDim, align: SKTextAlign.Right);
    }

    private void DrawKpiCards(SKCanvas canvas)
    {
        var demand = sim.LineAKw + sim.LineBKw + sim.UtilKw;
        DrawKpiCard(canvas, 16f, "DEMAND", $"{demand:0} kW", Electric);
        DrawKpiCard(canvas, 142f, "PV OUT", $"{sim.PvKw:0} kW", Green);
        DrawKpiCard(canvas, 268f, "STEAM", $"{sim.SteamBlr:0.0} t/h", Steam);
    }

    private void DrawKpiCard(SKCanvas canvas, float x, string label, string value, SKColor accent)
    {
        const float y = 44f;
        const float w = 116f;
        const float h = 46f;

        Fill.Color = CardBg;
        canvas.DrawRoundRect(x, y, w, h, 6f, 6f, Fill);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = CardBorder;
        Stroke.StrokeWidth = 1f;
        canvas.DrawRoundRect(x, y, w, h, 6f, 6f, Stroke);
        Fill.Color = accent;
        canvas.DrawRect(x, y + 6f, 3f, h - 12f, Fill);

        DrawText(canvas, label, x + 12f, y + 17f, 8f, TextDim);
        DrawText(canvas, value, x + 12f, y + 36f, 14f, accent, bold: true);
    }

    //--------------------------------------------------------------------------------
    // Electric section
    //--------------------------------------------------------------------------------

    private void DrawElectricSection(SKCanvas canvas, float t)
    {
        // Vertical bus
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Casing;
        Stroke.StrokeWidth = 10f;
        canvas.DrawLine(200f, 116f, 200f, 336f, Stroke);
        Stroke.Color = Electric.WithAlpha(200);
        Stroke.StrokeWidth = 4f;
        canvas.DrawLine(200f, 116f, 200f, 336f, Stroke);
        DrawText(canvas, "BUS 6.6kV", 208f, 120f, 8f, Electric.WithAlpha(200));

        // Sources (left)
        DrawNode(canvas, 20f, 112f, "GRID 66kV", $"{sim.GridKw:0} kW", true);
        DrawNode(canvas, 20f, 196f, "SOLAR PV-1", $"{sim.PvKw:0} kW", true);
        DrawNode(canvas, 20f, 280f, "GAS CGS-1", $"{sim.CgsKw:0} kW", true);

        // Loads (right)
        DrawNode(canvas, 250f, 112f, "LINE A", $"{sim.LineAKw:0} kW", true);
        DrawNode(canvas, 250f, 196f, "LINE B", $"{sim.LineBKw:0} kW", true);
        DrawNode(canvas, 250f, 280f, "UTILITY", $"{sim.UtilKw:0} kW", true);

        // Flows
        DrawFlowLine(canvas, 150f, 136f, 200f, 136f, Electric, EnergySim.Flow(sim.GridKw, 1800f), t);
        DrawFlowLine(canvas, 150f, 220f, 200f, 220f, Green, EnergySim.Flow(sim.PvKw, 300f), t);
        DrawFlowLine(canvas, 150f, 304f, 200f, 304f, Electric, EnergySim.Flow(sim.CgsKw, 450f), t);
        DrawFlowLine(canvas, 200f, 136f, 250f, 136f, Electric, EnergySim.Flow(sim.LineAKw, 750f), t);
        DrawFlowLine(canvas, 200f, 220f, 250f, 220f, Electric, EnergySim.Flow(sim.LineBKw, 600f), t);
        DrawFlowLine(canvas, 200f, 304f, 250f, 304f, Electric, EnergySim.Flow(sim.UtilKw, 500f), t);
    }

    //--------------------------------------------------------------------------------
    // Steam / gas section
    //--------------------------------------------------------------------------------

    private void DrawSteamSection(SKCanvas canvas, float t)
    {
        // Steam header (vertical)
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = Casing;
        Stroke.StrokeWidth = 10f;
        canvas.DrawLine(200f, 396f, 200f, 546f, Stroke);
        Stroke.Color = Steam.WithAlpha(200);
        Stroke.StrokeWidth = 4f;
        canvas.DrawLine(200f, 396f, 200f, 546f, Stroke);
        DrawText(canvas, "STEAM HDR", 208f, 400f, 8f, Steam.WithAlpha(200));

        DrawNode(canvas, 20f, 372f, "CITY GAS IN", $"{sim.GasBlr:0} m3/h", false);
        DrawNode(canvas, 20f, 470f, "BOILER B-1", $"{sim.SteamBlr:0.0} t/h", true);
        DrawNode(canvas, 250f, 402f, "HEAT PROC A", $"{sim.SteamHeat:0.0} t/h", true);
        DrawNode(canvas, 250f, 492f, "DRYER D-1", $"{sim.SteamDry:0.0} t/h", true);

        // Gas: GASIN -> BLR (vertical, left column)
        DrawFlowLine(canvas, 85f, 420f, 85f, 470f, Gas, EnergySim.Flow(sim.GasBlr, 520f), t);

        // Steam: BLR -> header, header -> loads
        DrawFlowLine(canvas, 150f, 494f, 200f, 494f, Steam, EnergySim.Flow(sim.SteamBlr, 7.5f), t);
        DrawFlowLine(canvas, 200f, 426f, 250f, 426f, Steam, EnergySim.Flow(sim.SteamHeat, 4.5f), t);
        DrawFlowLine(canvas, 200f, 516f, 250f, 516f, Steam, EnergySim.Flow(sim.SteamDry, 2.8f), t);
    }

    //--------------------------------------------------------------------------------
    // Node / flow primitives
    //--------------------------------------------------------------------------------

    private void DrawNode(SKCanvas canvas, float x, float y, string title, string value, bool running)
    {
        const float w = 130f;
        const float h = 48f;

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Fill;
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(x, y),
                new SKPoint(x, y + h),
                [BoxTop, BoxBottom],
                [0f, 1f],
                SKShaderTileMode.Clamp);
            paint.Shader = shader;
            canvas.DrawRoundRect(x, y, w, h, 5f, 5f, paint);
        }

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = BoxBorder;
        Stroke.StrokeWidth = 1.2f;
        canvas.DrawRoundRect(x, y, w, h, 5f, 5f, Stroke);

        DrawText(canvas, title, x + 10f, y + 18f, 9f, TextMain, bold: true);
        DrawText(canvas, value, x + 10f, y + 36f, 10f, Accent);

        Fill.Color = running ? Running : new SKColor(0x78, 0x90, 0x9C);
        canvas.DrawCircle(x + w - 10f, y + 10f, 3f, Fill);
    }

    private void DrawFlowLine(SKCanvas canvas, float x1, float y1, float x2, float y2, SKColor color, float flow, float t)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Butt;

        // Casing
        paint.Color = Casing;
        paint.StrokeWidth = 9f;
        canvas.DrawLine(x1, y1, x2, y2, paint);

        // Base
        paint.Color = color.WithAlpha(flow > 0.03f ? (byte)110 : (byte)45);
        paint.StrokeWidth = 3.5f;
        canvas.DrawLine(x1, y1, x2, y2, paint);

        // Animated dashes
        if (flow > 0.03f)
        {
            var speed = 30f + (70f * Math.Clamp(flow, 0f, 1.3f));
            var phase = -((t * speed) % 30f);
            using var dash = SKPathEffect.CreateDash([13f, 17f], phase);
            paint.PathEffect = dash;
            paint.Color = color;
            paint.StrokeWidth = 3.5f;
            canvas.DrawLine(x1, y1, x2, y2, paint);
            paint.PathEffect = null;
        }

        // Direction arrow at end
        var dx = x2 - x1;
        var dy = y2 - y1;
        var len = MathF.Sqrt((dx * dx) + (dy * dy));
        if (len < 1f)
        {
            return;
        }

        dx /= len;
        dy /= len;
        var ax = x2 - (dx * 4f);
        var ay = y2 - (dy * 4f);
        using var arrowBuilder = new SKPathBuilder();
        arrowBuilder.MoveTo(x2, y2);
        arrowBuilder.LineTo(ax - (dy * 4f), ay + (dx * 4f));
        arrowBuilder.LineTo(ax + (dy * 4f), ay - (dx * 4f));
        arrowBuilder.Close();
        using var arrow = arrowBuilder.Detach();
        Fill.Color = color;
        canvas.DrawPath(arrow, Fill);
    }

    //--------------------------------------------------------------------------------
    // Dynamic elements
    //--------------------------------------------------------------------------------

    private void DrawDynamics(SKCanvas canvas, float t, float vh)
    {
        var y = MathF.Min(vh - 90f, 586f);

        // Cooling fan CT-1
        var fx = 70f;
        var fy = y + 34f;
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = BoxBorder;
        Stroke.StrokeWidth = 1.5f;
        canvas.DrawCircle(fx, fy, 26f, Stroke);
        Stroke.Color = Cond.WithAlpha(160);
        Stroke.StrokeWidth = 4f;
        canvas.Save();
        canvas.RotateDegrees((t * 230f) % 360f, fx, fy);
        for (var i = 0; i < 3; i++)
        {
            canvas.DrawArc(new SKRect(fx - 20f, fy - 20f, fx + 20f, fy + 20f), i * 120f, 62f, false, Stroke);
        }

        canvas.Restore();
        Fill.Color = Cond;
        canvas.DrawCircle(fx, fy, 3.5f, Fill);
        DrawText(canvas, "CT-1 FAN RUN", fx, fy + 44f, 8f, TextDim, align: SKTextAlign.Center);

        // Feed water tank with wave surface
        const float tx = 250f;
        const float tw = 130f;
        const float th = 62f;
        var ty = y + 2f;

        Stroke.Color = BoxBorder;
        Stroke.StrokeWidth = 1.5f;
        canvas.DrawRect(tx, ty, tw, th, Stroke);

        var level = 0.62f + (0.03f * MathF.Sin(t * 0.4f));
        var surfaceY = ty + (th * (1f - level));

        using (var waveBuilder = new SKPathBuilder())
        {
            waveBuilder.MoveTo(tx + 1f, surfaceY);
            for (var i = 0; i <= 16; i++)
            {
                var wx = tx + 1f + ((tw - 2f) * i / 16f);
                var wy = surfaceY + (1.6f * MathF.Sin((t * 2.2f) + (i * 1.1f)));
                waveBuilder.LineTo(wx, wy);
            }

            waveBuilder.LineTo(tx + tw - 1f, ty + th - 1f);
            waveBuilder.LineTo(tx + 1f, ty + th - 1f);
            waveBuilder.Close();
            using var wave = waveBuilder.Detach();
            Fill.Color = Cond.WithAlpha(80);
            canvas.DrawPath(wave, Fill);
        }

        DrawText(canvas, $"TANK T-1  {(int)(level * 100f)}%", tx + (tw / 2f), ty + th + 16f, 8f, TextDim, align: SKTextAlign.Center);
    }

    private void DrawLegend(SKCanvas canvas, float vh)
    {
        var y = vh - 12f;
        DrawLegendItem(canvas, 24f, y, Electric, "ELEC");
        DrawLegendItem(canvas, 114f, y, Gas, "GAS");
        DrawLegendItem(canvas, 194f, y, Steam, "STEAM");
        DrawLegendItem(canvas, 290f, y, Cond, "COND");
    }

    private void DrawLegendItem(SKCanvas canvas, float x, float y, SKColor color, string label)
    {
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.Color = color;
        Stroke.StrokeWidth = 3f;
        canvas.DrawLine(x, y - 3f, x + 18f, y - 3f, Stroke);
        DrawText(canvas, label, x + 24f, y, 8f, TextDim);
    }
}
