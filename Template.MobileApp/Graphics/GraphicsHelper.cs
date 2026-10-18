namespace Template.MobileApp.Graphics;

internal static class GraphicsHelper
{
    //--------------------------------------------------------------------------------
    // Color
    //--------------------------------------------------------------------------------

    public static Color Lerp(Color color, Color to, float amount)
    {
        var t = Math.Clamp(amount, 0f, 1f);
        return new Color(
            Single.Lerp(color.Red, to.Red, t),
            Single.Lerp(color.Green, to.Green, t),
            Single.Lerp(color.Blue, to.Blue, t),
            Single.Lerp(color.Alpha, to.Alpha, t));
    }

    //--------------------------------------------------------------------------------
    // Coordinate
    //--------------------------------------------------------------------------------

    public static float NormalizeDegrees(float degrees) => ((degrees % 360f) + 360f) % 360f;

    public static PointF GetPoint(PointF center, float radius, float degrees)
    {
        var radians = Single.DegreesToRadians(degrees);
        return new PointF(center.X + (radius * MathF.Cos(radians)), center.Y + (radius * MathF.Sin(radians)));
    }

    public static float Distance(PointF a, PointF b) =>
        MathF.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));
}
