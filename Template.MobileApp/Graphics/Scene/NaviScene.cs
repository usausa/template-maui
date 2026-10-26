namespace Template.MobileApp.Graphics.Scene;

public sealed class NaviScene : SceneObject
{
    //--------------------------------------------------------------------------------
    // Resources
    //--------------------------------------------------------------------------------

    private const float MapUnit = 100f;

    private const float FollowX = 0.5f;

    private const float FollowY = 0.68f;

    private const float FollowPerSecond = 3f;

    private const float MarkerSize = 3.2f;

    private const float TurnRadius = 3.4f;

    private const float RouteWidth = 2.6f;

    private const float LabelSize = 2.4f;

    private const float PoiSize = 2.2f;

    private static readonly SKColor BackColor = new(0xFF101822);

    private static readonly SKColor RoadColor = new(0xFF26323F);

    private static readonly SKColor RoadEdgeColor = new(0xFF37454F);

    private static readonly SKColor LabelColor = new(0xFF78909C);

    private static readonly SKColor RouteColor = new(0xFF2979FF);

    private static readonly SKColor PassedColor = new(0xFF3C4A57);

    private static readonly SKColor MarkerColor = new(0xFF40C4FF);

    private static readonly SKColor TurnColor = SKColors.White;

    private static readonly SKColor PoiBackColor = new(0xE6263238);

    private static readonly SKColor PoiTextColor = new(0xFFECEFF1);

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private RouteGuidance? guidance;

    // Smoothed route position, it eases toward the value the guidance reports
    private float position;

    public void Update(RouteGuidance? value)
    {
        guidance = value;
        position = Target();
        Invalidate();
    }

    //--------------------------------------------------------------------------------
    // Loop
    //--------------------------------------------------------------------------------

    protected override void Update(float t, float dt)
    {
        position += (Target() - position) * Math.Min(1f, dt * FollowPerSecond);
    }

    protected override void OnRender(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackColor);

        if (guidance is null)
        {
            return;
        }

        // The map is laid out in a 0..100 square and scrolls so the vehicle keeps its place
        var scale = Math.Max(width, height) / MapUnit;
        var marker = PointAt(guidance.Route, Position());
        var offsetX = (width * FollowX) - (marker.X * scale);
        var offsetY = (height * FollowY) - (marker.Y * scale);

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale);

        DrawRoads(canvas);
        DrawRoute(canvas);

        canvas.Restore();

        // Markers and labels are drawn outside the map transform so they keep their pixel size
        DrawSpots(canvas, offsetX, offsetY, scale);
        DrawVehicle(canvas, offsetX, offsetY, scale);
    }

    //--------------------------------------------------------------------------------
    // Map
    //--------------------------------------------------------------------------------

    private void DrawRoads(SKCanvas canvas)
    {
        Stroke.StrokeCap = SKStrokeCap.Round;

        foreach (var road in guidance!.Roads)
        {
            Stroke.Color = RoadEdgeColor;
            Stroke.StrokeWidth = road.Width + 0.5f;
            canvas.DrawLine(road.X1, road.Y1, road.X2, road.Y2, Stroke);
            Stroke.Color = RoadColor;
            Stroke.StrokeWidth = road.Width;
            canvas.DrawLine(road.X1, road.Y1, road.X2, road.Y2, Stroke);
        }
    }

    private float Target() => guidance is not null ? (float)guidance.RouteProgress() : 0f;

    private float Position() => Math.Clamp(position, 0f, 1f);

    private void DrawRoute(SKCanvas canvas)
    {
        var route = guidance!.Route;
        if (route.Count < 2)
        {
            return;
        }

        var current = Position();
        var marker = PointAt(route, current);

        using var passed = new SKPathBuilder();
        using var upcoming = new SKPathBuilder();
        passed.MoveTo(route[0].X, route[0].Y);
        upcoming.MoveTo(marker.X, marker.Y);

        var split = IndexAt(route, current);
        for (var i = 1; i < route.Count; i++)
        {
            if (i <= split)
            {
                passed.LineTo(route[i].X, route[i].Y);
            }
            else
            {
                upcoming.LineTo(route[i].X, route[i].Y);
            }
        }

        passed.LineTo(marker.X, marker.Y);

        using var passedPath = passed.Detach();
        using var upcomingPath = upcoming.Detach();

        Stroke.StrokeCap = SKStrokeCap.Round;
        Stroke.StrokeJoin = SKStrokeJoin.Round;

        Stroke.Color = PassedColor;
        Stroke.StrokeWidth = RouteWidth;
        canvas.DrawPath(passedPath, Stroke);

        Stroke.Color = RouteColor.WithAlpha(70);
        Stroke.StrokeWidth = RouteWidth + 1.6f;
        canvas.DrawPath(upcomingPath, Stroke);
        Stroke.Color = RouteColor;
        Stroke.StrokeWidth = RouteWidth;
        canvas.DrawPath(upcomingPath, Stroke);

        // The next corner the guidance is counting down to
        if (split + 1 < route.Count)
        {
            var turn = route[split + 1];
            Fill.Color = TurnColor;
            canvas.DrawCircle(turn.X, turn.Y, TurnRadius / 2.2f, Fill);
        }
    }

    private void DrawVehicle(SKCanvas canvas, float offsetX, float offsetY, float scale)
    {
        var route = guidance!.Route;
        var current = Position();
        var marker = PointAt(route, current);
        var ahead = PointAt(route, Math.Min(1f, current + 0.01f));
        var angle = MathF.Atan2(ahead.Y - marker.Y, ahead.X - marker.X);
        var x = offsetX + (marker.X * scale);
        var y = offsetY + (marker.Y * scale);
        var size = MarkerSize * scale;

        Fill.Color = MarkerColor.WithAlpha(55);
        canvas.DrawCircle(x, y, size * 1.6f, Fill);
        Fill.Color = SKColors.White;
        canvas.DrawCircle(x, y, size * 1.05f, Fill);

        // An arrow head that points the way the car is heading
        var head = size * 0.85f;
        using var builder = new SKPathBuilder();
        builder.MoveTo(x + (MathF.Cos(angle) * head), y + (MathF.Sin(angle) * head));
        builder.LineTo(x + (MathF.Cos(angle + 2.3f) * head), y + (MathF.Sin(angle + 2.3f) * head));
        builder.LineTo(x + (MathF.Cos(angle) * head * 0.25f), y + (MathF.Sin(angle) * head * 0.25f));
        builder.LineTo(x + (MathF.Cos(angle - 2.3f) * head), y + (MathF.Sin(angle - 2.3f) * head));
        builder.Close();
        using var arrow = builder.Detach();

        Fill.Color = MarkerColor;
        canvas.DrawPath(arrow, Fill);
    }

    private void DrawSpots(SKCanvas canvas, float offsetX, float offsetY, float scale)
    {
        var labelSize = LabelSize * scale;
        var poiSize = PoiSize * scale;

        foreach (var road in guidance!.Roads.Where(static x => !String.IsNullOrEmpty(x.Name)))
        {
            var x = offsetX + ((road.X1 + road.X2) / 2f * scale);
            var y = offsetY + ((road.Y1 + road.Y2) / 2f * scale);
            DrawText(canvas, road.Name, x, y - (labelSize * 0.5f), labelSize, LabelColor, align: SKTextAlign.Center);
        }

        foreach (var spot in guidance.Spots)
        {
            var x = offsetX + (spot.X * scale);
            var y = offsetY + (spot.Y * scale);
            var width = MeasureText(spot.Name, poiSize) + (poiSize * 1.4f);
            var height = poiSize * 1.9f;

            Fill.Color = PoiBackColor;
            canvas.DrawRoundRect(new SKRect(x - (width / 2f), y - (height / 2f), x + (width / 2f), y + (height / 2f)), height * 0.3f, height * 0.3f, Fill);
            DrawText(canvas, spot.Name, x, y + (poiSize * 0.36f), poiSize, PoiTextColor, align: SKTextAlign.Center);
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    // The position is counted in vertices, one per guidance step, not in map length
    private static int IndexAt(IReadOnlyList<PointF> route, float position) =>
        Math.Min((int)(Math.Clamp(position, 0f, 1f) * (route.Count - 1)), route.Count - 2);

    private static PointF PointAt(IReadOnlyList<PointF> route, float position)
    {
        var target = Math.Clamp(position, 0f, 1f) * (route.Count - 1);
        var index = Math.Min((int)target, route.Count - 2);
        var rate = target - index;
        return new PointF(
            route[index].X + ((route[index + 1].X - route[index].X) * rate),
            route[index].Y + ((route[index + 1].Y - route[index].Y) * rate));
    }
}
