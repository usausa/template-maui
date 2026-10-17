namespace Template.MobileApp.Graphics.Scene;

public sealed class EnergyFlowScene : SceneObject
{
    private const float BaseWidth = 400f;
    private const float TrendMaxKw = 2600f;
    private const int MaxHits = 24;

    private static readonly SKColor Bg0 = new(0x05, 0x09, 0x12);
    private static readonly SKColor Bg1 = new(0x0B, 0x15, 0x27);
    private static readonly SKColor GlowColor = new(0x25, 0x63, 0xEB);
    private static readonly SKColor DotColor = new(0x18, 0x25, 0x3A);
    private static readonly SKColor PanelTop = new(0x11, 0x1D, 0x31);
    private static readonly SKColor PanelBottom = new(0x0B, 0x14, 0x24);
    private static readonly SKColor PanelLine = new(0x1F, 0x2F, 0x48);
    private static readonly SKColor TextMain = new(0xEA, 0xF0, 0xF8);
    private static readonly SKColor TextSub = new(0x9F, 0xB0, 0xC8);
    private static readonly SKColor TextDim = new(0x5B, 0x6E, 0x8C);
    private static readonly SKColor Accent = new(0x38, 0xBD, 0xF8);
    private static readonly SKColor GridColor = new(0x5B, 0x8C, 0xFF);
    private static readonly SKColor SolarColor = new(0xFF, 0xC5, 0x31);
    private static readonly SKColor CgsColor = new(0xFF, 0x7A, 0x45);
    private static readonly SKColor BatteryColor = new(0x2E, 0xE6, 0xA6);
    private static readonly SKColor BusColor = new(0xC7, 0xD2, 0xFE);
    private static readonly SKColor LineAColor = new(0xA7, 0x8B, 0xFA);
    private static readonly SKColor LineBColor = new(0xF4, 0x72, 0xB6);
    private static readonly SKColor HvacColor = new(0x22, 0xD3, 0xEE);
    private static readonly SKColor UtilityColor = new(0x94, 0xA3, 0xB8);
    private static readonly SKColor GasColor = new(0x60, 0xA5, 0xFA);
    private static readonly SKColor BoilerColor = new(0xF4, 0x3F, 0x5E);
    private static readonly SKColor ElectricColor = new(0xFD, 0xE0, 0x47);
    private static readonly SKColor SteamColor = new(0xFF, 0xB4, 0xA2);
    private static readonly SKColor LossColor = new(0x64, 0x74, 0x8B);
    private static readonly SKColor GoodColor = new(0x34, 0xD3, 0x99);
    private static readonly SKColor WarnColor = new(0xFB, 0xBF, 0x24);
    private static readonly SKColor AlarmColor = new(0xF8, 0x71, 0x71);
    private static readonly SKColor InfoColor = new(0x60, 0xA5, 0xFA);

    private static readonly SKColor[] LayerColors = [GridColor, CgsColor, SolarColor, BatteryColor];

    private enum HitKind
    {
        Node,
        Mode,
        Tile,
        Area
    }

    private readonly record struct HitTarget(SKRect Rect, HitKind Kind, EnergyNode Node, EnergyFlowMode Mode);

    private readonly record struct SceneLayout(SKRect Header, SKRect Demand, SKRect Flow, SKRect Trend, SKRect Equipment, SKRect Events);

    private sealed class FlowNode
    {
        public required EnergyNode Id { get; init; }

        public required int Column { get; init; }

        public required string Label { get; init; }

        public required SKColor Color { get; init; }

        public required EnergyIcon Icon { get; init; }

        public float Value { get; set; }

        public float X { get; set; }

        public float Width { get; set; }

        public float Top { get; set; }

        public float Height { get; set; }

        public float SlotTop { get; set; }

        public float SlotHeight { get; set; }

        public float InOffset { get; set; }

        public float OutOffset { get; set; }
    }

    private sealed class FlowLink
    {
        public required FlowNode From { get; init; }

        public required FlowNode To { get; init; }

        public float Value { get; set; }

        public float Thickness { get; set; }

        public float Y0 { get; set; }

        public float Y1 { get; set; }

        public float Band { get; set; }

        public bool Related { get; set; }
    }

    private sealed class FlowGraph
    {
        public required EnergyFlowMode Mode { get; init; }

        public required FlowNode[] Nodes { get; init; }

        public required FlowLink[] Links { get; init; }

        public float Scale { get; set; }

        public float ScaleTime { get; set; }
    }

    private readonly EnergySim sim = new();
    private readonly FlowGraph powerGraph = CreatePowerGraph();
    private readonly FlowGraph heatGraph = CreateHeatGraph();
    private readonly SKPathBuilder pathBuilder = new();
    private readonly float[] stackBase = new float[EnergySim.Slots];

    private readonly Lock touchSync = new();
    private readonly HitTarget[] hits = new HitTarget[MaxHits];
    private readonly HitTarget[] frameHits = new HitTarget[MaxHits];
    private int hitCount;
    private int frameHitCount;

    private EnergyFlowMode mode;
    private EnergyNode? selected;
    private float modeChangedAt = -10f;

    private float fanAngle;
    private float wheelAngle;

    protected override void Update(float t, float dt)
    {
        sim.Update(t, dt);
        fanAngle = (fanAngle + (dt * 420f * sim.FanRatio)) % 360f;
        wheelAngle = (wheelAngle + (dt * 520f * (sim.Cgs / 410f))) % 360f;
    }

    protected override bool OnTouch(SKPoint location, int width, int height)
    {
        var s = width / BaseWidth;
        var point = new SKPoint(location.X / s, location.Y / s);
        lock (touchSync)
        {
            for (var i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                if (!hit.Rect.Contains(point))
                {
                    continue;
                }

                switch (hit.Kind)
                {
                    case HitKind.Mode:
                        if (mode != hit.Mode)
                        {
                            mode = hit.Mode;
                            modeChangedAt = Time;
                            selected = null;
                        }

                        return true;
                    case HitKind.Node:
                        selected = selected == hit.Node ? null : hit.Node;
                        return true;
                    case HitKind.Tile:
                        if (mode != hit.Mode)
                        {
                            mode = hit.Mode;
                            modeChangedAt = Time;
                        }

                        selected = hit.Node;
                        return true;
                    default:
                        if (selected is null)
                        {
                            return false;
                        }

                        selected = null;
                        return true;
                }
            }
        }

        return false;
    }

    protected override void OnRender(SKCanvas canvas, int width, int height)
    {
        EnergyFlowMode currentMode;
        EnergyNode? currentSelected;
        float changedAt;
        lock (touchSync)
        {
            currentMode = mode;
            currentSelected = selected;
            changedAt = modeChangedAt;
        }

        var s = width / BaseWidth;
        var vh = height / s;
        var layout = ComputeLayout(vh);
        frameHitCount = 0;

        DrawStaticImage(canvas, width, height, s, (Scene: this, Layout: layout, Height: vh), static (c, state) => state.Scene.DrawChrome(c, state.Layout, state.Height));

        canvas.Save();
        canvas.Scale(s);

        DrawHeader(canvas, layout.Header);
        DrawDemand(canvas, layout.Demand);
        DrawFlow(canvas, layout.Flow, currentMode, currentSelected, changedAt);
        DrawTrend(canvas, layout.Trend);
        DrawEquipment(canvas, layout.Equipment);
        DrawEvents(canvas, layout.Events);

        canvas.Restore();

        lock (touchSync)
        {
            Array.Copy(frameHits, hits, frameHitCount);
            hitCount = frameHitCount;
        }
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

    private static SceneLayout ComputeLayout(float vh)
    {
        const float left = 12f;
        const float right = 388f;
        const float gap = 6f;
        const float equipmentHeight = 92f;

        var header = new SKRect(left, 4f, right, 42f);
        var demand = new SKRect(left, 46f, right, 166f);
        var bottom = vh - 8f;
        var eventsHeight = 38f;
        var trendHeight = 114f;
        var flowHeight = bottom - demand.Bottom - (gap * 4f) - trendHeight - equipmentHeight - eventsHeight;
        if (flowHeight < 240f)
        {
            flowHeight += eventsHeight + gap;
            eventsHeight = 0f;
        }

        if (flowHeight < 220f)
        {
            trendHeight -= 22f;
        }

        var events = new SKRect(left, bottom - eventsHeight, right, bottom);
        var equipmentBottom = eventsHeight > 0f ? events.Top - gap : bottom;
        var equipment = new SKRect(left, equipmentBottom - equipmentHeight, right, equipmentBottom);
        var trend = new SKRect(left, equipment.Top - gap - trendHeight, right, equipment.Top - gap);
        var flow = new SKRect(left, demand.Bottom + gap, right, trend.Top - gap);
        return new SceneLayout(header, demand, flow, trend, equipment, events);
    }

    private static SKRect TrendPlot(SKRect trend) => new(trend.Left + 34f, trend.Top + 27f, trend.Right - 12f, trend.Bottom - 18f);

    private static SKRect TileRect(SKRect area, int index)
    {
        var width = (area.Width - 18f) / 4f;
        var x = area.Left + (index * (width + 6f));
        return new SKRect(x, area.Top, x + width, area.Bottom);
    }

    private void DrawChrome(SKCanvas canvas, SceneLayout layout, float vh)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var background = SKShader.CreateLinearGradient(new SKPoint(0f, 0f), new SKPoint(0f, vh), [Bg1, Bg0], [0f, 1f], SKShaderTileMode.Clamp);
        paint.Shader = background;
        canvas.DrawRect(0f, 0f, BaseWidth, vh, paint);
        using var glow = SKShader.CreateRadialGradient(new SKPoint(200f, layout.Flow.MidY), 250f, [GlowColor.WithAlpha(44), GlowColor.WithAlpha(0)], [0f, 1f], SKShaderTileMode.Clamp);
        paint.Shader = glow;
        canvas.DrawRect(0f, 0f, BaseWidth, vh, paint);
        paint.Shader = null;

        var dots = new List<SKPoint>();
        for (var y = 10f; y < vh; y += 20f)
        {
            for (var x = 10f; x < BaseWidth; x += 20f)
            {
                dots.Add(new SKPoint(x, y));
            }
        }

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 1.4f;
        Stroke.Color = DotColor;
        canvas.DrawPoints(SKPointMode.Points, [.. dots], Stroke);

        DrawPanel(canvas, layout.Demand);
        DrawPanel(canvas, layout.Flow);
        DrawPanel(canvas, layout.Trend);
        for (var i = 0; i < 4; i++)
        {
            DrawPanel(canvas, TileRect(layout.Equipment, i));
        }

        if (layout.Events.Height > 0f)
        {
            DrawPanel(canvas, layout.Events);
        }

        DrawTitle(canvas, layout.Demand.Left + 12f, layout.Demand.Top + 18f, "DEMAND MONITOR", WarnColor);
        DrawTitle(canvas, layout.Flow.Left + 12f, layout.Flow.Top + 20f, "ENERGY FLOW", Accent);
        DrawTitle(canvas, layout.Trend.Left + 12f, layout.Trend.Top + 18f, "LOAD 24H", GridColor);
        DrawTitle(canvas, TileRect(layout.Equipment, 0).Left + 9f, layout.Equipment.Top + 17f, "BATTERY", BatteryColor);
        DrawTitle(canvas, TileRect(layout.Equipment, 1).Left + 9f, layout.Equipment.Top + 17f, "CGS", CgsColor);
        DrawTitle(canvas, TileRect(layout.Equipment, 2).Left + 9f, layout.Equipment.Top + 17f, "BOILER", BoilerColor);
        DrawTitle(canvas, TileRect(layout.Equipment, 3).Left + 9f, layout.Equipment.Top + 17f, "COOLING", HvacColor);

        var plot = TrendPlot(layout.Trend);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 0.7f;
        Stroke.Color = PanelLine;
        for (var mw = 1; mw <= 2; mw++)
        {
            var y = plot.Bottom - (mw * 1000f / TrendMaxKw * plot.Height);
            canvas.DrawLine(plot.Left, y, plot.Right, y, Stroke);
            DrawText(canvas, $"{mw}MW", layout.Trend.Left + 8f, y + 3f, 7.5f, TextDim);
        }

        Stroke.Color = PanelLine.WithAlpha(255);
        canvas.DrawLine(plot.Left, plot.Bottom, plot.Right, plot.Bottom, Stroke);
        for (var hour = 0; hour <= 24; hour += 6)
        {
            var x = plot.Left + (hour / 24f * plot.Width);
            canvas.DrawLine(x, plot.Bottom, x, plot.Bottom + 2.5f, Stroke);
            DrawText(canvas, $"{hour}", x, layout.Trend.Bottom - 5f, 7.5f, TextDim, align: SKTextAlign.Center);
        }
    }

    private void DrawPanel(SKCanvas canvas, SKRect rect)
    {
        using var shader = SKShader.CreateLinearGradient(new SKPoint(rect.Left, rect.Top), new SKPoint(rect.Left, rect.Bottom), [PanelTop.WithAlpha(235), PanelBottom.WithAlpha(235)], [0f, 1f], SKShaderTileMode.Clamp);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Shader = shader;
        canvas.DrawRoundRect(rect, 10f, 10f, paint);

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 1f;
        Stroke.Color = PanelLine;
        canvas.DrawRoundRect(rect, 10f, 10f, Stroke);
        Stroke.Color = SKColors.White.WithAlpha(16);
        canvas.DrawLine(rect.Left + 12f, rect.Top + 0.8f, rect.Right - 12f, rect.Top + 0.8f, Stroke);
    }

    private void DrawTitle(SKCanvas canvas, float x, float y, string text, SKColor accent)
    {
        Fill.Color = accent;
        canvas.DrawRoundRect(x, y - 8.5f, 2.8f, 10.5f, 1.4f, 1.4f, Fill);
        DrawText(canvas, text, x + 8f, y, 9.5f, TextSub, bold: true);
    }

    //--------------------------------------------------------------------------------
    // Header
    //--------------------------------------------------------------------------------

    private void DrawHeader(SKCanvas canvas, SKRect header)
    {
        var y = header.Top + 18f;
        DrawText(canvas, "PLANT-1", header.Left + 2f, y, 14.5f, TextMain, bold: true);
        DrawText(canvas, "ENERGY", header.Left + 2f + MeasureText("PLANT-1 ", 14.5f, true), y, 14.5f, Accent, bold: true);

        Fill.Color = GoodColor.WithAlpha(Blink(Time, 1f) ? (byte)255 : (byte)70);
        canvas.DrawCircle(header.Left + 5f, header.Top + 30f, 2.4f, Fill);
        DrawText(canvas, "EMS LIVE  6.6kV  CITY GAS 13A", header.Left + 12f, header.Top + 33f, 8.5f, TextSub);

        DrawText(canvas, sim.Now.ToString("HH:mm:ss"), header.Right, y, 14.5f, TextMain, bold: true, align: SKTextAlign.Right);

        var (text, color, blink) = Status();
        var textWidth = MeasureText(text, 8.5f, true);
        var pill = new SKRect(header.Right - textWidth - 21f, header.Top + 23f, header.Right, header.Top + 38f);
        var on = !blink || Blink(Time, 1.6f);
        Fill.Color = color.WithAlpha(on ? (byte)52 : (byte)18);
        canvas.DrawRoundRect(pill, 7.5f, 7.5f, Fill);
        Stroke.StrokeWidth = 0.8f;
        Stroke.Color = color.WithAlpha(on ? (byte)170 : (byte)70);
        canvas.DrawRoundRect(pill, 7.5f, 7.5f, Stroke);
        Fill.Color = color;
        canvas.DrawCircle(pill.Left + 8f, pill.MidY, 2.2f, Fill);
        DrawText(canvas, text, pill.Left + 13.5f, pill.MidY + 3f, 8.5f, color, bold: true);
    }

    private (string Text, SKColor Color, bool Blink) Status()
    {
        if (sim.Predicted > EnergySim.Contract)
        {
            return ("OVER CONTRACT", AlarmColor, true);
        }

        if (sim.Predicted > EnergySim.Target)
        {
            return ("DEMAND ALERT", WarnColor, true);
        }

        return sim.BatteryOut > 15f ? ("PEAK CUT", BatteryColor, false) : ("NORMAL", GoodColor, false);
    }

    private static SKColor DemandColor(float value)
    {
        if (value > EnergySim.Contract)
        {
            return AlarmColor;
        }

        return value > EnergySim.Target ? WarnColor : Accent;
    }

    //--------------------------------------------------------------------------------
    // Demand
    //--------------------------------------------------------------------------------

    private void DrawDemand(SKCanvas canvas, SKRect panel)
    {
        const float max = EnergySim.Contract * 1.15f;

        var start = sim.PeriodStart;
        var remain = (int)sim.PeriodRemaining;
        DrawText(canvas, $"{start:HH:mm}-{start.AddMinutes(EnergySim.PeriodMinutes):HH:mm}", panel.Left + 122f, panel.Top + 18f, 9f, TextSub);
        DrawText(canvas, $"REMAIN {remain / 60:00}:{remain % 60:00}", panel.Right - 12f, panel.Top + 18f, 9.5f, TextMain, bold: true, align: SKTextAlign.Right);

        var cx = panel.Left + 60f;
        var cy = panel.Top + 70f;
        const float r = 36f;
        var rect = new SKRect(cx - r, cy - r, cx + r, cy + r);
        const float startAngle = 135f;
        const float sweep = 270f;
        var targetAngle = sweep * EnergySim.Target / max;
        var contractAngle = sweep * EnergySim.Contract / max;

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 7f;
        Stroke.Color = GoodColor.WithAlpha(34);
        canvas.DrawArc(rect, startAngle, targetAngle, false, Stroke);
        Stroke.Color = WarnColor.WithAlpha(46);
        canvas.DrawArc(rect, startAngle + targetAngle, contractAngle - targetAngle, false, Stroke);
        Stroke.Color = AlarmColor.WithAlpha(52);
        canvas.DrawArc(rect, startAngle + contractAngle, sweep - contractAngle, false, Stroke);

        var color = DemandColor(sim.Predicted);
        var frac = Math.Clamp(sim.Predicted / max, 0f, 1f);
        Stroke.Color = color.WithAlpha(50);
        Stroke.StrokeWidth = 13f;
        canvas.DrawArc(rect, startAngle, sweep * frac, false, Stroke);
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 7f;
        Stroke.Color = color;
        canvas.DrawArc(rect, startAngle, sweep * frac, false, Stroke);

        DrawGaugeTick(canvas, cx, cy, r, startAngle + targetAngle, TextMain);
        DrawGaugeTick(canvas, cx, cy, r, startAngle + contractAngle, AlarmColor);

        var nowAngle = DegToRad(startAngle + (sweep * Math.Clamp(sim.Grid / max, 0f, 1f)));
        Fill.Color = TextMain;
        canvas.DrawCircle(cx + ((r - 9f) * MathF.Cos(nowAngle)), cy + ((r - 9f) * MathF.Sin(nowAngle)), 2f, Fill);

        DrawText(canvas, "PREDICT", cx, cy - 12f, 7.5f, TextDim, align: SKTextAlign.Center);
        DrawText(canvas, $"{sim.Predicted:#,0}", cx, cy + 6f, 16.5f, TextMain, bold: true, align: SKTextAlign.Center);
        DrawText(canvas, "kW", cx, cy + 17f, 7.5f, TextDim, align: SKTextAlign.Center);

        var left = panel.Left + 122f;
        var right = panel.Right - 12f;
        var top = panel.Top + 28f;
        var bottom = panel.Top + 77f;
        const float gap = 1.6f;
        var barWidth = (right - left - (gap * (EnergySim.PeriodMinutes - 1))) / EnergySim.PeriodMinutes;
        for (var i = 0; i < EnergySim.PeriodMinutes; i++)
        {
            var x = left + (i * (barWidth + gap));
            var value = sim.MinuteDemand(i);
            if (float.IsNaN(value))
            {
                Fill.Color = PanelLine;
                canvas.DrawRect(x, bottom - 1.5f, barWidth, 1.5f, Fill);
                continue;
            }

            var h = Math.Clamp(value / max, 0.02f, 1f) * (bottom - top);
            var current = i == sim.CurrentMinute;
            var barColor = DemandColor(value) == Accent ? GridColor : DemandColor(value);
            Fill.Color = barColor.WithAlpha(current ? (Blink(Time, 2f) ? (byte)255 : (byte)150) : (byte)190);
            canvas.DrawRoundRect(x, bottom - h, barWidth, h, 1.2f, 1.2f, Fill);
        }

        var targetY = bottom - (EnergySim.Target / max * (bottom - top));
        var contractY = bottom - (EnergySim.Contract / max * (bottom - top));
        using var dash = SKPathEffect.CreateDash([3f, 2.5f], 0f);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 0.9f;
        Stroke.PathEffect = dash;
        Stroke.Color = WarnColor.WithAlpha(210);
        canvas.DrawLine(left, targetY, right, targetY, Stroke);
        Stroke.Color = AlarmColor.WithAlpha(170);
        canvas.DrawLine(left, contractY, right, contractY, Stroke);
        Stroke.PathEffect = null;

        var columnWidth = (right - left) / 4f;
        DrawStat(canvas, left, panel.Top + 95f, "NOW", $"{sim.Grid:#,0}", DemandColor(sim.Grid) == Accent ? TextMain : DemandColor(sim.Grid));
        DrawStat(canvas, left + columnWidth, panel.Top + 95f, "LAST", sim.LastDemand > 0f ? $"{sim.LastDemand:#,0}" : "--", TextMain);
        DrawStat(canvas, left + (columnWidth * 2f), panel.Top + 95f, "MAX TODAY", sim.TodayMaxDemand > 0f ? $"{sim.TodayMaxDemand:#,0}" : "--", TextMain);
        DrawStat(canvas, left + (columnWidth * 3f), panel.Top + 95f, "TARGET", $"{EnergySim.Target:#,0}", WarnColor);
    }

    private void DrawGaugeTick(SKCanvas canvas, float cx, float cy, float r, float angle, SKColor color)
    {
        var rad = DegToRad(angle);
        var cos = MathF.Cos(rad);
        var sin = MathF.Sin(rad);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 1.6f;
        Stroke.Color = color;
        canvas.DrawLine(cx + ((r - 3.5f) * cos), cy + ((r - 3.5f) * sin), cx + ((r + 6f) * cos), cy + ((r + 6f) * sin), Stroke);
    }

    private void DrawStat(SKCanvas canvas, float x, float y, string label, string value, SKColor color)
    {
        DrawText(canvas, label, x, y, 8f, TextDim);
        DrawText(canvas, value, x, y + 15f, 12.5f, color, bold: true);
    }

    //--------------------------------------------------------------------------------
    // Flow
    //--------------------------------------------------------------------------------

    private static FlowNode CreateNode(EnergyNode id, int column, string label, SKColor color, EnergyIcon icon) =>
        new() { Id = id, Column = column, Label = label, Color = color, Icon = icon };

    private static FlowLink CreateLink(FlowNode from, FlowNode to) => new() { From = from, To = to };

    private static FlowGraph CreatePowerGraph()
    {
        var grid = CreateNode(EnergyNode.Grid, 0, "GRID", GridColor, EnergyIcon.Tower);
        var solar = CreateNode(EnergyNode.Solar, 0, "SOLAR PV", SolarColor, EnergyIcon.Sun);
        var cgs = CreateNode(EnergyNode.Cgs, 0, "CGS", CgsColor, EnergyIcon.Gear);
        var batteryOut = CreateNode(EnergyNode.BatteryOut, 0, "BATTERY", BatteryColor, EnergyIcon.Battery);
        var bus = CreateNode(EnergyNode.Bus, 1, "6.6kV BUS", BusColor, EnergyIcon.None);
        var lineA = CreateNode(EnergyNode.LineA, 2, "LINE A", LineAColor, EnergyIcon.Factory);
        var lineB = CreateNode(EnergyNode.LineB, 2, "LINE B", LineBColor, EnergyIcon.Factory);
        var hvac = CreateNode(EnergyNode.Hvac, 2, "HVAC", HvacColor, EnergyIcon.Snow);
        var utility = CreateNode(EnergyNode.Utility, 2, "UTILITY", UtilityColor, EnergyIcon.Plug);
        var batteryIn = CreateNode(EnergyNode.BatteryIn, 2, "CHARGE", BatteryColor, EnergyIcon.Battery);
        return new FlowGraph
        {
            Mode = EnergyFlowMode.Power,
            Nodes = [grid, solar, cgs, batteryOut, bus, lineA, lineB, hvac, utility, batteryIn],
            Links =
            [
                CreateLink(grid, bus),
                CreateLink(solar, bus),
                CreateLink(cgs, bus),
                CreateLink(batteryOut, bus),
                CreateLink(bus, lineA),
                CreateLink(bus, lineB),
                CreateLink(bus, hvac),
                CreateLink(bus, utility),
                CreateLink(bus, batteryIn)
            ]
        };
    }

    private static FlowGraph CreateHeatGraph()
    {
        var gas = CreateNode(EnergyNode.Gas, 0, "CITY GAS", GasColor, EnergyIcon.Flame);
        var engine = CreateNode(EnergyNode.CgsEngine, 1, "CGS", CgsColor, EnergyIcon.Gear);
        var boiler = CreateNode(EnergyNode.Boiler, 1, "BOILER", BoilerColor, EnergyIcon.Flame);
        var electric = CreateNode(EnergyNode.Electric, 2, "POWER", ElectricColor, EnergyIcon.Bolt);
        var steam = CreateNode(EnergyNode.Steam, 2, "STEAM", SteamColor, EnergyIcon.Steam);
        var loss = CreateNode(EnergyNode.Loss, 2, "LOSS", LossColor, EnergyIcon.Loss);
        return new FlowGraph
        {
            Mode = EnergyFlowMode.Heat,
            Nodes = [gas, engine, boiler, electric, steam, loss],
            Links =
            [
                CreateLink(gas, engine),
                CreateLink(gas, boiler),
                CreateLink(engine, electric),
                CreateLink(engine, steam),
                CreateLink(engine, loss),
                CreateLink(boiler, steam),
                CreateLink(boiler, loss)
            ]
        };
    }

    private float LinkValue(EnergyNode from, EnergyNode to) => (from, to) switch
    {
        (EnergyNode.Gas, EnergyNode.CgsEngine) => sim.GasCgs,
        (EnergyNode.Gas, EnergyNode.Boiler) => sim.GasBoiler,
        (EnergyNode.CgsEngine, EnergyNode.Electric) => sim.Cgs,
        (EnergyNode.CgsEngine, EnergyNode.Steam) => sim.CgsHeat,
        (EnergyNode.CgsEngine, EnergyNode.Loss) => sim.CgsLoss,
        (EnergyNode.Boiler, EnergyNode.Steam) => sim.BoilerSteam,
        (EnergyNode.Boiler, EnergyNode.Loss) => sim.BoilerLoss,
        (_, EnergyNode.Bus) => sim.Value(from),
        _ => sim.Value(to)
    };

    private void DrawFlow(SKCanvas canvas, SKRect panel, EnergyFlowMode currentMode, EnergyNode? currentSelected, float changedAt)
    {
        var powerChip = new SKRect(panel.Right - 126f, panel.Top + 8f, panel.Right - 70f, panel.Top + 26f);
        var heatChip = new SKRect(panel.Right - 66f, panel.Top + 8f, panel.Right - 10f, panel.Top + 26f);
        DrawModeChip(canvas, powerChip, "POWER", currentMode == EnergyFlowMode.Power);
        DrawModeChip(canvas, heatChip, "HEAT", currentMode == EnergyFlowMode.Heat);
        AddHit(new SKRect(powerChip.Left - 2f, panel.Top, powerChip.Right + 2f, powerChip.Bottom + 6f), HitKind.Mode, EnergyNode.Bus, EnergyFlowMode.Power);
        AddHit(new SKRect(heatChip.Left - 2f, panel.Top, heatChip.Right + 2f, heatChip.Bottom + 6f), HitKind.Mode, EnergyNode.Gas, EnergyFlowMode.Heat);

        var graph = currentMode == EnergyFlowMode.Power ? powerGraph : heatGraph;
        foreach (var node in graph.Nodes)
        {
            node.Value = MathF.Max(0f, sim.Value(node.Id));
        }

        foreach (var link in graph.Links)
        {
            link.Value = MathF.Max(0f, LinkValue(link.From.Id, link.To.Id));
        }

        var area = new SKRect(panel.Left + 8f, panel.Top + 38f, panel.Right - 8f, panel.Bottom - 36f);
        LayoutGraph(graph, area, Time);
        var selectedNode = currentSelected is { } id ? Find(graph, id) : null;
        ApplySelection(graph, selectedNode);

        var fade = Math.Clamp((Time - changedAt) / 0.3f, 0f, 1f);
        using var layer = new SKPaint();
        if (fade < 1f)
        {
            layer.Color = SKColors.White.WithAlpha((byte)(255f * fade));
            canvas.SaveLayer(layer);
        }

        DrawRibbons(canvas, graph, selectedNode);
        DrawParticles(canvas, graph, selectedNode is not null);
        DrawNodes(canvas, graph, area, selectedNode, currentMode);

        if (fade < 1f)
        {
            canvas.Restore();
        }

        DrawFlowFooter(canvas, panel, graph, selectedNode);
        AddHit(area, HitKind.Area, EnergyNode.Bus, currentMode);
    }

    private void DrawModeChip(SKCanvas canvas, SKRect rect, string text, bool active)
    {
        Fill.Color = active ? Accent.WithAlpha(58) : PanelBottom;
        canvas.DrawRoundRect(rect, 9f, 9f, Fill);
        Stroke.StrokeWidth = 0.9f;
        Stroke.Color = active ? Accent.WithAlpha(210) : PanelLine;
        canvas.DrawRoundRect(rect, 9f, 9f, Stroke);
        DrawText(canvas, text, rect.MidX, rect.MidY + 3f, 8.5f, active ? TextMain : TextDim, bold: true, align: SKTextAlign.Center);
    }

    private static FlowNode? Find(FlowGraph graph, EnergyNode id)
    {
        foreach (var node in graph.Nodes)
        {
            if (node.Id == id)
            {
                return node;
            }
        }

        return null;
    }

    private static float ColumnSum(FlowGraph graph, int column)
    {
        var sum = 0f;
        foreach (var node in graph.Nodes)
        {
            if (node.Column == column)
            {
                sum += node.Value;
            }
        }

        return sum;
    }

    private static float ColumnExtent(FlowGraph graph, int column, float scale, float gap, float minSlot)
    {
        var extent = 0f;
        var count = 0;
        foreach (var node in graph.Nodes)
        {
            if (node.Column == column)
            {
                extent += MathF.Max(node.Value * scale, minSlot);
                count++;
            }
        }

        return extent + (gap * MathF.Max(0, count - 1));
    }

    private static float FitScale(FlowGraph graph, float height, float gap, float minSlot)
    {
        var scale = float.MaxValue;
        for (var column = 0; column < 3; column++)
        {
            var low = 0f;
            var high = height / MathF.Max(ColumnSum(graph, column), 1f);
            for (var i = 0; i < 24; i++)
            {
                var middle = (low + high) / 2f;
                if (ColumnExtent(graph, column, middle, gap, minSlot) <= height)
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            scale = MathF.Min(scale, low);
        }

        return scale;
    }

    private static void LayoutGraph(FlowGraph graph, SKRect area, float time)
    {
        const float gap = 8f;
        const float minSlot = 28f;

        var target = FitScale(graph, area.Height * 0.97f, gap, minSlot);
        var dt = Math.Clamp(time - graph.ScaleTime, 0f, 0.5f);
        graph.ScaleTime = time;
        graph.Scale = graph.Scale <= 0f ? target : graph.Scale + ((target - graph.Scale) * MathF.Min(1f, dt / (target < graph.Scale ? 0.25f : 2f)));
        var scale = graph.Scale;

        for (var column = 0; column < 3; column++)
        {
            var y = area.Top + ((area.Height - ColumnExtent(graph, column, scale, gap, minSlot)) / 2f);
            var (x, width) = column switch
            {
                0 => (area.Left + 86f, 7f),
                1 => (area.MidX - 5f, 10f),
                _ => (area.Right - 93f, 7f)
            };
            foreach (var node in graph.Nodes)
            {
                if (node.Column != column)
                {
                    continue;
                }

                var height = node.Value * scale;
                var slot = MathF.Max(height, minSlot);
                node.X = x;
                node.Width = width;
                node.SlotTop = y;
                node.SlotHeight = slot;
                node.Height = MathF.Max(height, 1.5f);
                node.Top = y + ((slot - node.Height) / 2f);
                node.InOffset = 0f;
                node.OutOffset = 0f;
                y += slot + gap;
            }
        }

        foreach (var link in graph.Links)
        {
            link.Thickness = link.Value * scale;
            link.Y0 = link.From.Top + link.From.OutOffset;
            link.From.OutOffset += link.Thickness;
            link.Y1 = link.To.Top + link.To.InOffset;
            link.To.InOffset += link.Thickness;
        }
    }

    private static void ApplySelection(FlowGraph graph, FlowNode? node)
    {
        foreach (var link in graph.Links)
        {
            link.Related = (node is null) || (link.From == node) || (link.To == node);
            link.Band = link.Related ? link.Thickness : 0f;
        }

        if (node is null)
        {
            return;
        }

        foreach (var first in graph.Links)
        {
            if ((node.Column == 0) && (first.From == node) && (first.To.Value > 0.5f))
            {
                var share = first.Value / first.To.Value;
                foreach (var second in graph.Links)
                {
                    if (second.From == first.To)
                    {
                        second.Band = MathF.Max(second.Band, second.Thickness * share);
                    }
                }
            }
            else if ((node.Column == 2) && (first.To == node) && (first.From.Value > 0.5f))
            {
                var share = first.Value / first.From.Value;
                foreach (var second in graph.Links)
                {
                    if (second.To == first.From)
                    {
                        second.Band = MathF.Max(second.Band, second.Thickness * share);
                    }
                }
            }
        }
    }

    private SKPath CreateRibbon(float x0, float x1, float y0, float y1, float thickness)
    {
        var xm = (x0 + x1) / 2f;
        pathBuilder.MoveTo(x0, y0);
        pathBuilder.CubicTo(xm, y0, xm, y1, x1, y1);
        pathBuilder.LineTo(x1, y1 + thickness);
        pathBuilder.CubicTo(xm, y1 + thickness, xm, y0 + thickness, x0, y0 + thickness);
        pathBuilder.Close();
        return pathBuilder.Detach();
    }

    private void DrawRibbons(SKCanvas canvas, FlowGraph graph, FlowNode? selectedNode)
    {
        var hasSelection = selectedNode is not null;
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        foreach (var link in graph.Links)
        {
            if (link.Thickness < 0.4f)
            {
                continue;
            }

            var x0 = link.From.X + link.From.Width;
            var x1 = link.To.X;
            var dim = hasSelection && !link.Related;
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(x0, 0f),
                new SKPoint(x1, 0f),
                [link.From.Color.WithAlpha(dim ? (byte)26 : (byte)125), link.To.Color.WithAlpha(dim ? (byte)20 : (byte)95)],
                [0f, 1f],
                SKShaderTileMode.Clamp);
            paint.Shader = shader;
            using var path = CreateRibbon(x0, x1, link.Y0, link.Y1, link.Thickness);
            canvas.DrawPath(path, paint);
            paint.Shader = null;

            if (dim && (link.Band > 0.4f) && (selectedNode is not null))
            {
                paint.Color = selectedNode.Color.WithAlpha(150);
                using var band = CreateRibbon(x0, x1, link.Y0, link.Y1, link.Band);
                canvas.DrawPath(band, paint);
            }
        }
    }

    private static float Bezier(float p0, float p1, float p2, float p3, float u)
    {
        var v = 1f - u;
        return (v * v * v * p0) + (3f * v * v * u * p1) + (3f * v * u * u * p2) + (u * u * u * p3);
    }

    private void DrawParticles(SKCanvas canvas, FlowGraph graph, bool hasSelection)
    {
        var maxValue = 1f;
        foreach (var link in graph.Links)
        {
            maxValue = MathF.Max(maxValue, link.Value);
        }

        for (var index = 0; index < graph.Links.Length; index++)
        {
            var link = graph.Links[index];
            var band = hasSelection ? link.Band : link.Thickness;
            if (band < 1f)
            {
                continue;
            }

            var count = Math.Clamp((int)(band / 6f) + 1, 1, 6);
            var speed = 0.18f + (0.42f * link.Value / maxValue);
            var x0 = link.From.X + link.From.Width;
            var x1 = link.To.X;
            var xm = (x0 + x1) / 2f;
            for (var i = 0; i < count; i++)
            {
                var u = EnergyMath.Frac(EnergyMath.Hash01((index * 37) + i) + (Time * speed));
                var lane = 0.15f + (0.7f * EnergyMath.Hash01((index * 53) + (i * 11) + 5));
                var x = Bezier(x0, xm, xm, x1, u);
                var y = Bezier(link.Y0, link.Y0, link.Y1, link.Y1, u) + (band * lane);
                var fade = EnergyMath.Smooth(0f, 0.08f, u) * (1f - EnergyMath.Smooth(0.9f, 1f, u));
                var color = link.From.Color.Lerp(link.To.Color, u);
                Fill.Color = color.WithAlpha((byte)(95f * fade));
                canvas.DrawCircle(x, y, 3f, Fill);
                Fill.Color = color.Lerp(SKColors.White, 0.6f).WithAlpha((byte)(250f * fade));
                canvas.DrawCircle(x, y, 1.35f, Fill);
            }
        }
    }

    private static bool IsNeighbor(FlowGraph graph, FlowNode node, FlowNode? selectedNode)
    {
        if ((selectedNode is null) || (node == selectedNode))
        {
            return true;
        }

        foreach (var link in graph.Links)
        {
            if (((link.From == node) && (link.To == selectedNode)) || ((link.To == node) && (link.From == selectedNode)))
            {
                return true;
            }
        }

        return false;
    }

    private void DrawNodes(SKCanvas canvas, FlowGraph graph, SKRect area, FlowNode? selectedNode, EnergyFlowMode currentMode)
    {
        foreach (var node in graph.Nodes)
        {
            var alpha = IsNeighbor(graph, node, selectedNode) ? (byte)255 : (byte)95;
            var bar = new SKRect(node.X, node.Top, node.X + node.Width, node.Top + node.Height);
            var idle = node.Value < 0.5f;
            if (!idle)
            {
                var pulse = node.Id == EnergyNode.Bus ? 0.22f + (0.14f * MathF.Sin(Time * 2.4f)) : 0.22f;
                Fill.Color = node.Color.WithAlpha((byte)(alpha * pulse));
                canvas.DrawRoundRect(new SKRect(bar.Left - 2.5f, bar.Top - 1.5f, bar.Right + 2.5f, bar.Bottom + 1.5f), 3.5f, 3.5f, Fill);
            }

            Fill.Color = idle ? TextDim.WithAlpha(140) : node.Color.WithAlpha(alpha);
            canvas.DrawRoundRect(bar, 2f, 2f, Fill);

            if (node == selectedNode)
            {
                var grow = 3f + (1.5f * MathF.Sin(Time * 5f));
                Stroke.StrokeWidth = 1.2f;
                Stroke.Color = node.Color.WithAlpha(200);
                canvas.DrawRoundRect(new SKRect(bar.Left - grow, bar.Top - grow, bar.Right + grow, bar.Bottom + grow), 4f, 4f, Stroke);
            }

            switch (node.Column)
            {
                case 0:
                    DrawSourceLabel(canvas, node, alpha);
                    AddHit(new SKRect(area.Left, node.SlotTop - 3f, node.X + node.Width + 4f, node.SlotTop + node.SlotHeight + 3f), HitKind.Node, node.Id, currentMode);
                    break;
                case 2:
                    DrawSinkLabel(canvas, node, alpha);
                    AddHit(new SKRect(node.X - 4f, node.SlotTop - 3f, area.Right, node.SlotTop + node.SlotHeight + 3f), HitKind.Node, node.Id, currentMode);
                    break;
                default:
                    DrawMiddleLabel(canvas, node, alpha);
                    AddHit(new SKRect(node.X - 24f, node.Top - 10f, node.X + node.Width + 24f, node.Top + node.Height + 10f), HitKind.Node, node.Id, currentMode);
                    break;
            }
        }
    }

    private string? SubLabel(EnergyNode id) => id switch
    {
        EnergyNode.Gas => $"{sim.GasFlow:0} m3/h",
        EnergyNode.Steam => $"{sim.SteamTph:0.0} t/h",
        EnergyNode.Loss => $"{sim.Value(EnergyNode.Loss) / MathF.Max(sim.Value(EnergyNode.Gas), 1f) * 100f:0}% OF GAS",
        EnergyNode.BatteryOut or EnergyNode.BatteryIn => $"SOC {sim.Soc * 100f:0}%",
        EnergyNode.Solar => sim.CloudFactor < 0.75f ? "CLOUDY" : null,
        _ => null
    };

    private void DrawSourceLabel(SKCanvas canvas, FlowNode node, byte alpha)
    {
        var idle = node.Value < 0.5f;
        var nameColor = (idle ? TextDim : node.Color).WithAlpha(alpha);
        var sub = SubLabel(node.Id);
        var cy = node.SlotTop + (node.SlotHeight / 2f);
        var nameY = sub is null ? cy - 4.5f : cy - 10f;
        var right = node.X - 6f;

        DrawText(canvas, node.Label, right, nameY, 9f, nameColor, bold: true, align: SKTextAlign.Right);
        DrawIcon(canvas, node.Icon, right - MeasureText(node.Label, 9f, true) - 8f, nameY - 3.2f, 10.5f, nameColor);
        var unitWidth = MeasureText(" kW", 8f);
        DrawText(canvas, " kW", right, nameY + 13.5f, 8f, TextDim.WithAlpha(alpha), align: SKTextAlign.Right);
        DrawText(canvas, $"{node.Value:#,0}", right - unitWidth, nameY + 13.5f, 12.5f, (idle ? TextDim : TextMain).WithAlpha(alpha), bold: true, align: SKTextAlign.Right);
        if ((sub is not null) && (node.SlotHeight >= 36f))
        {
            DrawText(canvas, sub, right, nameY + 24f, 8f, TextSub.WithAlpha(alpha), align: SKTextAlign.Right);
        }
    }

    private void DrawSinkLabel(SKCanvas canvas, FlowNode node, byte alpha)
    {
        var idle = node.Value < 0.5f;
        var nameColor = (idle ? TextDim : node.Color).WithAlpha(alpha);
        var sub = SubLabel(node.Id);
        var cy = node.SlotTop + (node.SlotHeight / 2f);
        var nameY = sub is null ? cy - 4.5f : cy - 10f;
        var left = node.X + node.Width + 6f;

        DrawIcon(canvas, node.Icon, left + 5f, nameY - 3.2f, 10.5f, nameColor);
        DrawText(canvas, node.Label, left + 12.5f, nameY, 9f, nameColor, bold: true);
        var value = $"{node.Value:#,0}";
        DrawText(canvas, value, left, nameY + 13.5f, 12.5f, (idle ? TextDim : TextMain).WithAlpha(alpha), bold: true);
        DrawText(canvas, " kW", left + MeasureText(value, 12.5f, true), nameY + 13.5f, 8f, TextDim.WithAlpha(alpha));
        if ((sub is not null) && (node.SlotHeight >= 36f))
        {
            DrawText(canvas, sub, left, nameY + 24f, 8f, TextSub.WithAlpha(alpha));
        }
    }

    private void DrawMiddleLabel(SKCanvas canvas, FlowNode node, byte alpha)
    {
        var cx = node.X + (node.Width / 2f);
        if (node.Id == EnergyNode.Bus)
        {
            DrawText(canvas, node.Label, cx, node.Top - 7f, 8.5f, TextSub.WithAlpha(alpha), bold: true, align: SKTextAlign.Center);
            DrawText(canvas, $"{node.Value:#,0} kW", cx, node.Top + node.Height + 14f, 10.5f, TextMain.WithAlpha(alpha), bold: true, align: SKTextAlign.Center);
            return;
        }

        var idle = node.Value < 0.5f;
        var value = idle ? "STOP" : $"{node.Value:#,0} kW";
        var width = MathF.Max(MeasureText(node.Label, 8.5f, true), MeasureText(value, 10f, true)) + 14f;
        var cy = node.Top + (node.Height / 2f);
        var pill = new SKRect(cx - (width / 2f), cy - 13.5f, cx + (width / 2f), cy + 13.5f);
        Fill.Color = PanelBottom.WithAlpha(235);
        canvas.DrawRoundRect(pill, 6f, 6f, Fill);
        Stroke.StrokeWidth = 1f;
        Stroke.Color = (idle ? TextDim : node.Color).WithAlpha((byte)(alpha * 0.75f));
        canvas.DrawRoundRect(pill, 6f, 6f, Stroke);
        DrawText(canvas, node.Label, cx, cy - 2.5f, 8.5f, (idle ? TextDim : node.Color).WithAlpha(alpha), bold: true, align: SKTextAlign.Center);
        DrawText(canvas, value, cx, cy + 9.5f, 10f, (idle ? TextDim : TextMain).WithAlpha(alpha), bold: true, align: SKTextAlign.Center);
    }

    private void DrawFlowFooter(SKCanvas canvas, SKRect panel, FlowGraph graph, FlowNode? selectedNode)
    {
        var y = panel.Bottom - 11f;
        if (selectedNode is not null)
        {
            var total = ColumnSum(graph, selectedNode.Column);
            var share = total > 0.5f ? selectedNode.Value / total * 100f : 0f;
            var basis = selectedNode.Column switch
            {
                0 => "INPUT",
                2 => "OUTPUT",
                _ => "FLOW"
            };
            var today = sim.TodayEnergy(selectedNode.Id) / 1000f;
            DrawText(canvas, $"{selectedNode.Label}  {selectedNode.Value:#,0} kW  {share:0}% OF {basis}  TODAY {today:0.00} MWh", panel.MidX, y, 8.5f, selectedNode.Color, bold: true, align: SKTextAlign.Center);
            return;
        }

        string text;
        if (graph.Mode == EnergyFlowMode.Power)
        {
            var supply = MathF.Max(sim.Value(EnergyNode.Bus), 1f);
            text = $"SELF-GEN {(sim.Solar + sim.Cgs) / supply * 100f:0}%   RENEWABLE {sim.Solar / supply * 100f:0}%   IMPORT {sim.Grid:#,0} kW";
        }
        else
        {
            var efficiency = (sim.Cgs + sim.CgsHeat + sim.BoilerSteam) / MathF.Max(sim.Value(EnergyNode.Gas), 1f) * 100f;
            text = $"GAS {sim.GasFlow:0} m3/h   STEAM {sim.SteamTph:0.0} t/h   EFFICIENCY {efficiency:0}%";
        }

        DrawText(canvas, text, panel.MidX, y, 8.5f, TextSub, align: SKTextAlign.Center);
    }

    //--------------------------------------------------------------------------------
    // Trend
    //--------------------------------------------------------------------------------

    private static float TrendY(SKRect plot, float kw) => plot.Bottom - (Math.Clamp(kw / TrendMaxKw, 0f, 1f) * plot.Height);

    private static float SlotX(SKRect plot, int slot) => plot.Left + ((slot + 0.5f) / EnergySim.Slots * plot.Width);

    private void DrawTrend(SKCanvas canvas, SKRect panel)
    {
        var plot = TrendPlot(panel);
        DrawText(
            canvas,
            $"TODAY {sim.TodayLoad / 1000f:0.0} MWh  SELF {sim.TodaySelfRatio * 100f:0}%  CO2 {sim.TodayCo2:0.0} t  ¥{sim.TodayCost / 1000f:#,0}k",
            panel.Right - 12f,
            panel.Top + 18f,
            8.5f,
            TextSub,
            align: SKTextAlign.Right);

        var count = sim.CurrentSlot + 1;
        var nowX = plot.Left + (sim.NowSeconds / 86400f * plot.Width);
        Array.Clear(stackBase);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 1.2f;
        for (var layer = 0; layer < EnergySim.LayerCount; layer++)
        {
            var color = LayerColors[layer];

            pathBuilder.MoveTo(plot.Left, TrendY(plot, stackBase[0] + sim.History(layer, 0)));
            for (var slot = 0; slot < count; slot++)
            {
                var x = slot == (count - 1) ? nowX : SlotX(plot, slot);
                pathBuilder.LineTo(x, TrendY(plot, stackBase[slot] + sim.History(layer, slot)));
            }

            using var edge = pathBuilder.Detach();

            pathBuilder.MoveTo(plot.Left, TrendY(plot, stackBase[0] + sim.History(layer, 0)));
            for (var slot = 0; slot < count; slot++)
            {
                var x = slot == (count - 1) ? nowX : SlotX(plot, slot);
                pathBuilder.LineTo(x, TrendY(plot, stackBase[slot] + sim.History(layer, slot)));
            }

            for (var slot = count - 1; slot >= 0; slot--)
            {
                var x = slot == (count - 1) ? nowX : SlotX(plot, slot);
                pathBuilder.LineTo(x, TrendY(plot, stackBase[slot]));
            }

            pathBuilder.LineTo(plot.Left, TrendY(plot, stackBase[0]));
            pathBuilder.Close();
            using var area = pathBuilder.Detach();

            using var shader = SKShader.CreateLinearGradient(new SKPoint(0f, plot.Top), new SKPoint(0f, plot.Bottom), [color.WithAlpha(170), color.WithAlpha(60)], [0f, 1f], SKShaderTileMode.Clamp);
            paint.Shader = shader;
            canvas.DrawPath(area, paint);
            paint.Shader = null;
            if (layer == 0)
            {
                Stroke.Color = color;
                canvas.DrawPath(edge, Stroke);
            }

            for (var slot = 0; slot < count; slot++)
            {
                stackBase[slot] += sim.History(layer, slot);
            }
        }

        pathBuilder.MoveTo(plot.Left, TrendY(plot, sim.LoadHistory(0)));
        for (var slot = 0; slot < count; slot++)
        {
            pathBuilder.LineTo(slot == (count - 1) ? nowX : SlotX(plot, slot), TrendY(plot, sim.LoadHistory(slot)));
        }

        using var load = pathBuilder.Detach();
        Stroke.Color = TextMain.WithAlpha(200);
        Stroke.StrokeWidth = 1.1f;
        canvas.DrawPath(load, Stroke);

        pathBuilder.MoveTo(nowX, TrendY(plot, sim.Load));
        for (var slot = count; slot < EnergySim.Slots; slot++)
        {
            pathBuilder.LineTo(SlotX(plot, slot), TrendY(plot, sim.Forecast(slot)));
        }

        using var forecast = pathBuilder.Detach();
        using var dash = SKPathEffect.CreateDash([3f, 3f], 0f);
        Stroke.PathEffect = dash;
        Stroke.Color = TextMain.WithAlpha(90);
        canvas.DrawPath(forecast, Stroke);

        var targetY = TrendY(plot, EnergySim.Target);
        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 0.9f;
        Stroke.Color = WarnColor.WithAlpha(190);
        canvas.DrawLine(plot.Left, targetY, plot.Right, targetY, Stroke);
        Stroke.PathEffect = null;

        Stroke.Color = TextMain.WithAlpha(110);
        Stroke.StrokeWidth = 0.8f;
        canvas.DrawLine(nowX, plot.Top, nowX, plot.Bottom, Stroke);
        Fill.Color = GridColor.WithAlpha(110);
        canvas.DrawCircle(nowX, TrendY(plot, sim.Grid), 4f, Fill);
        Fill.Color = TextMain;
        canvas.DrawCircle(nowX, TrendY(plot, sim.Grid), 1.8f, Fill);
        DrawText(canvas, "NOW", nowX + (nowX > (plot.Right - 28f) ? -3f : 3f), plot.Top + 7.5f, 7.5f, TextSub, align: nowX > (plot.Right - 28f) ? SKTextAlign.Right : SKTextAlign.Left);
    }

    //--------------------------------------------------------------------------------
    // Equipment
    //--------------------------------------------------------------------------------

    private void DrawEquipment(SKCanvas canvas, SKRect area)
    {
        var battery = TileRect(area, 0);
        DrawBatteryTile(canvas, battery);
        AddHit(battery, HitKind.Tile, sim.BatteryIn > 15f ? EnergyNode.BatteryIn : EnergyNode.BatteryOut, EnergyFlowMode.Power);

        var cgs = TileRect(area, 1);
        DrawCgsTile(canvas, cgs);
        AddHit(cgs, HitKind.Tile, EnergyNode.Cgs, EnergyFlowMode.Power);

        var boiler = TileRect(area, 2);
        DrawBoilerTile(canvas, boiler);
        AddHit(boiler, HitKind.Tile, EnergyNode.Boiler, EnergyFlowMode.Heat);

        var cooling = TileRect(area, 3);
        DrawCoolingTile(canvas, cooling);
        AddHit(cooling, HitKind.Tile, EnergyNode.Hvac, EnergyFlowMode.Power);
    }

    private void DrawTileValues(SKCanvas canvas, SKRect tile, string value, string detail, SKColor detailColor, string caption)
    {
        var x = tile.Left + 42f;
        DrawText(canvas, value, x, tile.Top + 50f, 11.5f, TextMain, bold: true);
        DrawText(canvas, detail, x, tile.Top + 64f, 8.5f, detailColor, bold: true);
        DrawText(canvas, caption, x, tile.Top + 77f, 7.5f, TextDim);
    }

    private void DrawLed(SKCanvas canvas, SKRect tile, SKColor color, bool blink)
    {
        var on = !blink || Blink(Time, 1.2f);
        Fill.Color = color.WithAlpha(on ? (byte)60 : (byte)20);
        canvas.DrawCircle(tile.Right - 10f, tile.Top + 12f, 4f, Fill);
        Fill.Color = color.WithAlpha(on ? (byte)255 : (byte)90);
        canvas.DrawCircle(tile.Right - 10f, tile.Top + 12f, 2f, Fill);
    }

    private void DrawBatteryTile(SKCanvas canvas, SKRect tile)
    {
        var cx = tile.Left + 23f;
        var cy = tile.Top + 57f;
        var body = new SKRect(cx - 10f, cy - 18f, cx + 10f, cy + 18f);
        var inner = new SKRect(body.Left + 2.5f, body.Top + 2.5f, body.Right - 2.5f, body.Bottom - 2.5f);
        var low = sim.Soc < 0.2f;

        var level = inner.Height * sim.Soc;
        using var shader = SKShader.CreateLinearGradient(new SKPoint(0f, inner.Bottom - level), new SKPoint(0f, inner.Bottom), [(low ? WarnColor : BatteryColor).WithAlpha(240), (low ? WarnColor : BatteryColor).WithAlpha(120)], [0f, 1f], SKShaderTileMode.Clamp);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Shader = shader;
        canvas.DrawRoundRect(new SKRect(inner.Left, inner.Bottom - level, inner.Right, inner.Bottom), 1.5f, 1.5f, paint);

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 1.5f;
        Stroke.Color = TextSub;
        canvas.DrawRoundRect(body, 3.5f, 3.5f, Stroke);
        Fill.Color = TextSub;
        canvas.DrawRoundRect(new SKRect(cx - 4f, body.Top - 3f, cx + 4f, body.Top), 1f, 1f, Fill);

        var chargingNow = sim.BatteryIn > 15f;
        var dischargingNow = sim.BatteryOut > 15f;
        if (chargingNow || dischargingNow)
        {
            Stroke.StrokeWidth = 1.6f;
            for (var i = 0; i < 2; i++)
            {
                var phase = EnergyMath.Frac((Time * 0.8f) + (i * 0.5f));
                var y = chargingNow ? inner.Bottom - (phase * inner.Height) : inner.Top + (phase * inner.Height);
                var dir = chargingNow ? -1f : 1f;
                Stroke.Color = SKColors.White.WithAlpha((byte)(200f * MathF.Sin(phase * MathF.PI)));
                canvas.DrawLine(cx - 5f, y - (dir * 2.5f), cx, y + (dir * 2.5f), Stroke);
                canvas.DrawLine(cx + 5f, y - (dir * 2.5f), cx, y + (dir * 2.5f), Stroke);
            }
        }

        var (detail, color, caption) = chargingNow
            ? ($"+{sim.BatteryIn:0}kW", BatteryColor, "CHARGE")
            : dischargingNow ? ($"-{sim.BatteryOut:0}kW", BatteryColor, "PEAK CUT") : ("IDLE", TextDim, "STANDBY");
        DrawTileValues(canvas, tile, $"{sim.Soc * 100f:0}%", detail, color, caption);
        DrawLed(canvas, tile, chargingNow || dischargingNow ? BatteryColor : GoodColor, chargingNow || dischargingNow);
    }

    private void DrawCgsTile(SKCanvas canvas, SKRect tile)
    {
        var cx = tile.Left + 23f;
        var cy = tile.Top + 59f;
        const float r = 13f;
        var running = sim.Cgs > 50f;
        var color = running ? CgsColor : TextDim;

        if (running)
        {
            Stroke.StrokeCap = SKStrokeCap.Round;
            Stroke.StrokeWidth = 1.2f;
            for (var i = 0; i < 3; i++)
            {
                var phase = EnergyMath.Frac((Time * 0.7f) + (i / 3f));
                var x = cx - 6f + (i * 6f);
                var y = cy - r - 3f - (phase * 7f);
                pathBuilder.MoveTo(x, y + 4f);
                pathBuilder.QuadTo(x + 2f, y + 2f, x, y);
                pathBuilder.QuadTo(x - 2f, y - 2f, x, y - 4f);
                using var heat = pathBuilder.Detach();
                Stroke.Color = CgsColor.WithAlpha((byte)(170f * MathF.Sin(phase * MathF.PI)));
                canvas.DrawPath(heat, Stroke);
            }
        }

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 2.2f;
        Stroke.Color = color.WithAlpha(running ? (byte)230 : (byte)140);
        canvas.DrawCircle(cx, cy, r, Stroke);
        Stroke.StrokeWidth = 1.5f;
        for (var i = 0; i < 5; i++)
        {
            var rad = DegToRad(wheelAngle + (i * 72f));
            canvas.DrawLine(cx + (3.5f * MathF.Cos(rad)), cy + (3.5f * MathF.Sin(rad)), cx + ((r - 2f) * MathF.Cos(rad)), cy + ((r - 2f) * MathF.Sin(rad)), Stroke);
        }

        Fill.Color = color;
        canvas.DrawCircle(cx, cy, 3.2f, Fill);

        DrawTileValues(
            canvas,
            tile,
            running ? $"{sim.Cgs:0}kW" : "STOP",
            running ? $"{sim.CgsSteamTph:0.00}t/h" : "--",
            CgsColor,
            running ? "HEAT REC" : "STANDBY");
        DrawLed(canvas, tile, running ? GoodColor : TextDim, false);
    }

    private void DrawBoilerTile(SKCanvas canvas, SKRect tile)
    {
        var cx = tile.Left + 23f;
        var baseY = tile.Top + 76f;
        var load = Math.Clamp(sim.BoilerSteam / 1500f, 0.15f, 1f);
        var flicker = 1f + (0.08f * MathF.Sin(Time * 13f)) + (0.05f * MathF.Sin((Time * 7.3f) + 1f));
        var height = (15f + (17f * load)) * flicker;
        var width = 8f + (5f * load);
        var sway = 2f * MathF.Sin(Time * 4.7f);

        Fill.Color = CgsColor.WithAlpha(40);
        canvas.DrawOval(new SKRect(cx - (width * 1.3f), baseY - (height * 0.8f), cx + (width * 1.3f), baseY + 2f), Fill);

        using var outer = CreateFlame(cx, baseY, width, height, sway);
        using var outerShader = SKShader.CreateLinearGradient(new SKPoint(0f, baseY - height), new SKPoint(0f, baseY), [WarnColor.WithAlpha(230), CgsColor, BoilerColor], [0f, 0.55f, 1f], SKShaderTileMode.Clamp);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Shader = outerShader;
        canvas.DrawPath(outer, paint);

        using var inner = CreateFlame(cx, baseY, width * 0.5f, height * 0.55f, sway * 0.6f);
        using var innerShader = SKShader.CreateLinearGradient(new SKPoint(0f, baseY - (height * 0.55f)), new SKPoint(0f, baseY), [SKColors.White, ElectricColor], [0f, 1f], SKShaderTileMode.Clamp);
        paint.Shader = innerShader;
        canvas.DrawPath(inner, paint);
        paint.Shader = null;

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 2f;
        Stroke.Color = GasColor.WithAlpha(200);
        canvas.DrawLine(cx - 11f, baseY + 2f, cx + 11f, baseY + 2f, Stroke);

        DrawTileValues(canvas, tile, $"{sim.BoilerSteamTph:0.0}t/h", $"{sim.BoilerGasFlow:0}m3/h", GasColor, "STEAM OUT");
        DrawLed(canvas, tile, GoodColor, false);
    }

    private SKPath CreateFlame(float cx, float baseY, float width, float height, float sway)
    {
        var tipX = cx + sway;
        var tipY = baseY - height;
        pathBuilder.MoveTo(tipX, tipY);
        pathBuilder.CubicTo(cx + width, baseY - (height * 0.45f), cx + (width * 0.9f), baseY, cx, baseY);
        pathBuilder.CubicTo(cx - (width * 0.9f), baseY, cx - width, baseY - (height * 0.45f), tipX, tipY);
        pathBuilder.Close();
        return pathBuilder.Detach();
    }

    private void DrawCoolingTile(SKCanvas canvas, SKRect tile)
    {
        var cx = tile.Left + 22f;
        var cy = tile.Top + 57f;
        const float r = 14f;

        Stroke.StrokeCap = SKStrokeCap.Butt;
        Stroke.StrokeWidth = 1.3f;
        Stroke.Color = TextSub.WithAlpha(150);
        canvas.DrawCircle(cx, cy, r + 2f, Stroke);

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 4.5f;
        Stroke.Color = HvacColor.WithAlpha(210);
        var blades = new SKRect(cx - r + 3f, cy - r + 3f, cx + r - 3f, cy + r - 3f);
        for (var i = 0; i < 4; i++)
        {
            canvas.DrawArc(blades, fanAngle + (i * 90f), 48f, false, Stroke);
        }

        Fill.Color = HvacColor;
        canvas.DrawCircle(cx, cy, 3f, Fill);

        DrawTileValues(canvas, tile, $"{sim.WaterTemp:0.0}°C", $"FAN {sim.FanRatio * 100f:0}%", HvacColor, "CW RETURN");
        DrawLed(canvas, tile, GoodColor, false);
    }

    //--------------------------------------------------------------------------------
    // Events
    //--------------------------------------------------------------------------------

    private static SKColor LevelColor(EnergyLevel level) => level switch
    {
        EnergyLevel.Good => GoodColor,
        EnergyLevel.Warn => WarnColor,
        EnergyLevel.Alarm => AlarmColor,
        _ => InfoColor
    };

    private void DrawEvents(SKCanvas canvas, SKRect panel)
    {
        if (panel.Height <= 0f)
        {
            return;
        }

        var events = sim.Events;
        for (var i = 0; i < Math.Min(2, events.Count); i++)
        {
            var item = events[i];
            var appear = EnergyMath.Smooth(0f, 0.4f, Time - item.SceneTime);
            var y = panel.Top + 16f + (i * 15f) + ((1f - appear) * 6f);
            var alpha = (byte)((i == 0 ? 255f : 150f) * appear);
            var color = LevelColor(item.Level);
            DrawText(canvas, item.Time.ToString("HH:mm:ss"), panel.Left + 10f, y, 8.5f, TextDim.WithAlpha(alpha));
            Fill.Color = color.WithAlpha(alpha);
            canvas.DrawCircle(panel.Left + 63f, y - 3f, 2.4f, Fill);
            DrawText(canvas, item.Source, panel.Left + 70f, y, 8.5f, color.WithAlpha(alpha), bold: true);
            DrawText(canvas, item.Text, panel.Left + 122f, y, 8.5f, (i == 0 ? TextMain : TextSub).WithAlpha(alpha));
        }
    }

    //--------------------------------------------------------------------------------
    // Helpers
    //--------------------------------------------------------------------------------

    private void AddHit(SKRect rect, HitKind kind, EnergyNode node, EnergyFlowMode hitMode)
    {
        if (frameHitCount < MaxHits)
        {
            frameHits[frameHitCount] = new HitTarget(rect, kind, node, hitMode);
            frameHitCount++;
        }
    }

    private void DrawIcon(SKCanvas canvas, EnergyIcon icon, float cx, float cy, float size, SKColor color)
    {
        var k = size / 12f;
        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeWidth = 1.3f * k;
        Stroke.Color = color;
        Fill.Color = color;
        switch (icon)
        {
            case EnergyIcon.Tower:
                pathBuilder.MoveTo(cx - (4f * k), cy + (6f * k));
                pathBuilder.LineTo(cx, cy - (6f * k));
                pathBuilder.LineTo(cx + (4f * k), cy + (6f * k));
                pathBuilder.MoveTo(cx - (5.5f * k), cy - (2.5f * k));
                pathBuilder.LineTo(cx + (5.5f * k), cy - (2.5f * k));
                pathBuilder.MoveTo(cx - (3.4f * k), cy + (1.8f * k));
                pathBuilder.LineTo(cx + (3.4f * k), cy + (1.8f * k));
                DrawIconPath(canvas, Stroke);
                break;
            case EnergyIcon.Sun:
                canvas.DrawCircle(cx, cy, 2.8f * k, Fill);
                for (var i = 0; i < 8; i++)
                {
                    var rad = i * MathF.PI / 4f;
                    canvas.DrawLine(cx + (4.4f * k * MathF.Cos(rad)), cy + (4.4f * k * MathF.Sin(rad)), cx + (6f * k * MathF.Cos(rad)), cy + (6f * k * MathF.Sin(rad)), Stroke);
                }

                break;
            case EnergyIcon.Gear:
                canvas.DrawCircle(cx, cy, 3.8f * k, Stroke);
                for (var i = 0; i < 6; i++)
                {
                    var rad = (i * MathF.PI / 3f) + 0.3f;
                    canvas.DrawLine(cx + (4.4f * k * MathF.Cos(rad)), cy + (4.4f * k * MathF.Sin(rad)), cx + (6f * k * MathF.Cos(rad)), cy + (6f * k * MathF.Sin(rad)), Stroke);
                }

                break;
            case EnergyIcon.Battery:
                canvas.DrawRoundRect(new SKRect(cx - (3.5f * k), cy - (4.8f * k), cx + (3.5f * k), cy + (6f * k)), 1.2f * k, 1.2f * k, Stroke);
                canvas.DrawRect(cx - (1.6f * k), cy - (6.6f * k), 3.2f * k, 1.4f * k, Fill);
                canvas.DrawRect(cx - (1.8f * k), cy + (0.5f * k), 3.6f * k, 3.6f * k, Fill);
                break;
            case EnergyIcon.Factory:
                pathBuilder.MoveTo(cx - (6f * k), cy + (5.5f * k));
                pathBuilder.LineTo(cx - (6f * k), cy - (1f * k));
                pathBuilder.LineTo(cx - (2f * k), cy - (4f * k));
                pathBuilder.LineTo(cx - (2f * k), cy - (1f * k));
                pathBuilder.LineTo(cx + (2f * k), cy - (4f * k));
                pathBuilder.LineTo(cx + (2f * k), cy - (1f * k));
                pathBuilder.LineTo(cx + (6f * k), cy - (4f * k));
                pathBuilder.LineTo(cx + (6f * k), cy + (5.5f * k));
                pathBuilder.Close();
                DrawIconPath(canvas, Stroke);
                break;
            case EnergyIcon.Snow:
                for (var i = 0; i < 3; i++)
                {
                    var rad = (i * MathF.PI / 3f) + (MathF.PI / 2f);
                    canvas.DrawLine(cx - (6f * k * MathF.Cos(rad)), cy - (6f * k * MathF.Sin(rad)), cx + (6f * k * MathF.Cos(rad)), cy + (6f * k * MathF.Sin(rad)), Stroke);
                }

                break;
            case EnergyIcon.Plug:
                canvas.DrawLine(cx - (2.5f * k), cy - (6f * k), cx - (2.5f * k), cy - (2.5f * k), Stroke);
                canvas.DrawLine(cx + (2.5f * k), cy - (6f * k), cx + (2.5f * k), cy - (2.5f * k), Stroke);
                pathBuilder.MoveTo(cx - (5f * k), cy - (2.5f * k));
                pathBuilder.LineTo(cx + (5f * k), cy - (2.5f * k));
                pathBuilder.LineTo(cx + (5f * k), cy);
                pathBuilder.QuadTo(cx + (5f * k), cy + (4f * k), cx, cy + (4f * k));
                pathBuilder.QuadTo(cx - (5f * k), cy + (4f * k), cx - (5f * k), cy);
                pathBuilder.Close();
                DrawIconPath(canvas, Stroke);
                canvas.DrawLine(cx, cy + (4f * k), cx, cy + (6.5f * k), Stroke);
                break;
            case EnergyIcon.Flame:
                pathBuilder.MoveTo(cx, cy - (6.5f * k));
                pathBuilder.CubicTo(cx + (6f * k), cy - k, cx + (4.5f * k), cy + (6f * k), cx, cy + (6f * k));
                pathBuilder.CubicTo(cx - (4.5f * k), cy + (6f * k), cx - (6f * k), cy - k, cx, cy - (6.5f * k));
                pathBuilder.Close();
                DrawIconPath(canvas, Fill);
                break;
            case EnergyIcon.Bolt:
                pathBuilder.MoveTo(cx + (1.5f * k), cy - (6.5f * k));
                pathBuilder.LineTo(cx - (4.5f * k), cy + (1f * k));
                pathBuilder.LineTo(cx - (0.5f * k), cy + (1f * k));
                pathBuilder.LineTo(cx - (1.5f * k), cy + (6.5f * k));
                pathBuilder.LineTo(cx + (4.5f * k), cy - (1f * k));
                pathBuilder.LineTo(cx + (0.5f * k), cy - (1f * k));
                pathBuilder.Close();
                DrawIconPath(canvas, Fill);
                break;
            case EnergyIcon.Steam:
                for (var i = -1; i <= 1; i++)
                {
                    var x = cx + (i * 3.6f * k);
                    pathBuilder.MoveTo(x, cy + (6f * k));
                    pathBuilder.QuadTo(x + (2.2f * k), cy + (3f * k), x, cy);
                    pathBuilder.QuadTo(x - (2.2f * k), cy - (3f * k), x, cy - (6f * k));
                }

                DrawIconPath(canvas, Stroke);
                break;
            case EnergyIcon.Loss:
                canvas.DrawLine(cx, cy - (6f * k), cx, cy + (4f * k), Stroke);
                canvas.DrawLine(cx - (3.5f * k), cy + (1f * k), cx, cy + (5f * k), Stroke);
                canvas.DrawLine(cx + (3.5f * k), cy + (1f * k), cx, cy + (5f * k), Stroke);
                break;
        }
    }

    private void DrawIconPath(SKCanvas canvas, SKPaint paint)
    {
        using var path = pathBuilder.Detach();
        canvas.DrawPath(path, paint);
    }
}
