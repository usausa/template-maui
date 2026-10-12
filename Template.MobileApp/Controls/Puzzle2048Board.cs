namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.App;

public sealed class Puzzle2048Board : SKCanvasView
{
    private const string AnimationName = "Puzzle2048Board";

    private const uint AnimationLength = 260;

    private const double SlideRatio = 0.45;

    private const float Overshoot = 1.70158f;

    private const float MergeBounce = 0.18f;

    private const float MinimumScale = 0.01f;

    private const int GlowValue = 128;

    private const float GlowBlur = 7f;

    private const double GlowBaseAlpha = 60;

    private const double GlowAlphaStep = 12;

    private const double GlowMaximumAlpha = 200;

    private const float TileShadowBlur = 2f;

    private const byte TileShadowAlpha = 40;

    private const float HighlightAmount = 0.18f;

    private const int DarkTextMaximum = 4;

    private const float BoardShadowBlur = 3f;

    private const byte BoardShadowAlpha = 50;

    private const float ShadowOffset = 2f;

    private const float BoardInset = 8f;

    private const float GapRate = 0.03f;

    private const float BoardRadiusRate = 1.6f;

    private const float TileRadiusRate = 0.1f;

    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    private static readonly SKColor BoardColor = SKColor.Parse("#BBADA0");

    private static readonly SKColor SlotColor = SKColor.Parse("#CDC1B4");

    private static readonly SKColor DarkTextColor = SKColor.Parse("#776E65");

    private static readonly SKColor LargeTileColor = SKColor.Parse("#3C3A32");

    private static readonly Dictionary<int, SKColor> TileColors = new()
    {
        [2] = SKColor.Parse("#EEE4DA"),
        [4] = SKColor.Parse("#EDE0C8"),
        [8] = SKColor.Parse("#F2B179"),
        [16] = SKColor.Parse("#F59563"),
        [32] = SKColor.Parse("#F67C5F"),
        [64] = SKColor.Parse("#F65E3B"),
        [128] = SKColor.Parse("#EDCF72"),
        [256] = SKColor.Parse("#EDCC61"),
        [512] = SKColor.Parse("#EDC850"),
        [1024] = SKColor.Parse("#EDC53F"),
        [2048] = SKColor.Parse("#EDC22E")
    };

    private static readonly float[] FontRates = [0.48f, 0.48f, 0.48f, 0.40f, 0.32f, 0.26f];

    private static readonly Dictionary<int, string> ValueTexts = [];

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private double progress = 1;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty BoardProperty = BindableProperty.Create(
        nameof(Board),
        typeof(Puzzle2048Frame),
        typeof(Puzzle2048Board),
        propertyChanged: OnBoardChanged);

    public Puzzle2048Frame? Board
    {
        get => (Puzzle2048Frame?)GetValue(BoardProperty);
        set => SetValue(BoardProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Layout
    //--------------------------------------------------------------------------------

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var size = Math.Min(widthConstraint, heightConstraint);
        return new Size(size, size);
    }

    //--------------------------------------------------------------------------------
    // Render
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            var density = (float)(info.Width / Width);
            var layout = new BoardLayout(Math.Min(info.Width, info.Height), density);

            DrawBoard(canvas, layout, density);
            if (Board is { } frame)
            {
                DrawTiles(canvas, layout, density, frame);
            }
        }
    }

    private static void DrawBoard(SKCanvas canvas, BoardLayout layout, float density)
    {
        using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, BoardShadowBlur * density);
        using var shadow = new SKPaint();
        shadow.IsAntialias = true;
        shadow.Color = SKColors.Black.WithAlpha(BoardShadowAlpha);
        shadow.MaskFilter = blur;
        var shadowRect = layout.Board;
        shadowRect.Offset(0, ShadowOffset * density);
        canvas.DrawRoundRect(shadowRect, layout.BoardRadius, layout.BoardRadius, shadow);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = BoardColor;
        canvas.DrawRoundRect(layout.Board, layout.BoardRadius, layout.BoardRadius, paint);

        paint.Color = SlotColor;
        for (var row = 0; row < Puzzle2048Game.Size; row++)
        {
            for (var column = 0; column < Puzzle2048Game.Size; column++)
            {
                canvas.DrawRoundRect(layout.GetCell(row, column), layout.TileRadius, layout.TileRadius, paint);
            }
        }
    }

    private void DrawTiles(SKCanvas canvas, BoardLayout layout, float density, Puzzle2048Frame frame)
    {
        if ((frame.Move is { } move) && (progress < SlideRatio))
        {
            var t = EaseOut(progress / SlideRatio);
            foreach (var slide in move.Slides)
            {
                var from = layout.GetCell(slide.FromRow, slide.FromColumn);
                var to = layout.GetCell(slide.ToRow, slide.ToColumn);
                var rect = SKRect.Create(
                    Lerp(from.Left, to.Left, t),
                    Lerp(from.Top, to.Top, t),
                    from.Width,
                    from.Height);
                DrawTile(canvas, layout, density, rect, slide.Value, 1f);
            }
        }
        else
        {
            var t = frame.Move is null ? 1 : (float)((progress - SlideRatio) / (1 - SlideRatio));
            foreach (var tile in frame.Tiles)
            {
                var scale = 1f;
                if (frame.Move?.Spawned?.Id == tile.Id)
                {
                    scale = EaseOutBack(t);
                }
                else if (IsMerged(frame.Move, tile.Id))
                {
                    scale = 1f + (MergeBounce * MathF.Sin(t * MathF.PI));
                }

                DrawTile(canvas, layout, density, layout.GetCell(tile.Row, tile.Column), tile.Value, scale);
            }
        }
    }

    private static void DrawTile(SKCanvas canvas, BoardLayout layout, float density, SKRect cell, int value, float scale)
    {
        if (scale > MinimumScale)
        {
            var width = cell.Width * scale;
            var rect = SKRect.Create(cell.MidX - (width / 2), cell.MidY - (width / 2), width, width);
            var radius = layout.TileRadius * scale;
            var (color, textColor) = GetColors(value);

            using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (value >= GlowValue ? GlowBlur : TileShadowBlur) * density);
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.MaskFilter = blur;

            if (value >= GlowValue)
            {
                paint.Color = color.WithAlpha((byte)Math.Min(GlowMaximumAlpha, GlowBaseAlpha + (Math.Log2(value) * GlowAlphaStep)));
                canvas.DrawRoundRect(rect, radius, radius, paint);
            }
            else
            {
                paint.Color = SKColors.Black.WithAlpha(TileShadowAlpha);
                var shadow = rect;
                shadow.Offset(0, ShadowOffset * density);
                canvas.DrawRoundRect(shadow, radius, radius, paint);
            }

            paint.MaskFilter = null;
            paint.Color = SKColors.White;
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(rect.Left, rect.Top),
                new SKPoint(rect.Left, rect.Bottom),
                [color.Lerp(SKColors.White, HighlightAmount), color],
                SKShaderTileMode.Clamp);
            paint.Shader = shader;
            canvas.DrawRoundRect(rect, radius, radius, paint);
            paint.Shader = null;

            var text = GetValueText(value);
            var ratio = FontRates[Math.Min(text.Length, FontRates.Length - 1)];
            using var font = new SKFont(BoldTypeface, cell.Width * ratio * scale);
            paint.Color = textColor;
            canvas.DrawText(text, rect.MidX, font.Metrics.GetCenterBaseline(rect.MidY), SKTextAlign.Center, font, paint);
        }
    }

    private static (SKColor Color, SKColor Text) GetColors(int value) =>
        (TileColors.GetValueOrDefault(value, LargeTileColor), value <= DarkTextMaximum ? DarkTextColor : SKColors.White);

    private static string GetValueText(int value)
    {
        if (!ValueTexts.TryGetValue(value, out var text))
        {
            text = value.ToString(CultureInfo.InvariantCulture);
            ValueTexts[value] = text;
        }

        return text;
    }

    private static bool IsMerged(Puzzle2048Move? move, int id)
    {
        if (move is not null)
        {
            foreach (var tile in move.Merged)
            {
                if (tile.Id == id)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static float Lerp(float from, float to, float t) => from + ((to - from) * t);

    private static float EaseOut(double t) => 1f - MathF.Pow(1f - (float)t, 3);

    private static float EaseOutBack(float t)
    {
        var x = t - 1;
        return 1 + (x * x * (((Overshoot + 1) * x) + Overshoot));
    }

    private readonly struct BoardLayout
    {
        public SKRect Board { get; }

        public float BoardRadius { get; }

        public float TileRadius { get; }

        private readonly float gap;

        private readonly float cellSize;

        public BoardLayout(float size, float density)
        {
            var inset = BoardInset * density;
            Board = SKRect.Create(inset, inset, size - (inset * 2), size - (inset * 2));
            gap = Board.Width * GapRate;
            cellSize = (Board.Width - (gap * (Puzzle2048Game.Size + 1))) / Puzzle2048Game.Size;
            BoardRadius = gap * BoardRadiusRate;
            TileRadius = cellSize * TileRadiusRate;
        }

        public SKRect GetCell(int row, int column) => SKRect.Create(
            Board.Left + gap + (column * (cellSize + gap)),
            Board.Top + gap + (row * (cellSize + gap)),
            cellSize,
            cellSize);
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private static void OnBoardChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var board = (Puzzle2048Board)bindable;
        board.AbortAnimation(AnimationName);
        if (newValue is Puzzle2048Frame { Move: not null })
        {
            board.progress = 0;
            board.Animate(
                AnimationName,
                x =>
                {
                    board.progress = x;
                    board.InvalidateSurface();
                },
                length: AnimationLength,
                finished: (_, cancelled) =>
                {
                    if (!cancelled)
                    {
                        board.progress = 1;
                        board.InvalidateSurface();
                    }
                });
        }
        else
        {
            board.progress = 1;
            board.InvalidateSurface();
        }
    }
}
