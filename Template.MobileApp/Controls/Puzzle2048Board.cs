namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.App;

// 2048 の盤面 (正方形)。Board の直前の 1 手から、動いたタイルを滑らせ、合体したタイルを弾ませ、新しいタイルを拡大して出す
public sealed class Puzzle2048Board : SKCanvasView
{
    private const string AnimationName = "Puzzle2048Board";

    // 全体の時間と、そのうち滑らせる割合
    private const uint AnimationLength = 260;
    private const double SlideRatio = 0.45;

    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    private static readonly SKColor BoardColor = SKColor.Parse("#BBADA0");
    private static readonly SKColor SlotColor = SKColor.Parse("#CDC1B4");
    private static readonly SKColor DarkTextColor = SKColor.Parse("#776E65");

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

    // アニメーションの進み (1 は止まっている)
    private double progress = 1;

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

    // 幅と高さの短い方の正方形にする
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var size = Math.Min(widthConstraint, heightConstraint);
        return new Size(size, size);
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            // SKCanvas は物理ピクセルなので、論理単位の長さに密度を掛ける
            var density = (float)(info.Width / Width);
            var layout = new BoardLayout(Math.Min(info.Width, info.Height), density);

            DrawBoard(canvas, layout, density);
            if (Board is { } frame)
            {
                DrawTiles(canvas, layout, density, frame);
            }
        }
    }

    // 影を落とした盤面と、空きのマス
    private static void DrawBoard(SKCanvas canvas, BoardLayout layout, float density)
    {
        using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3 * density);
        using var shadow = new SKPaint();
        shadow.IsAntialias = true;
        shadow.Color = SKColors.Black.WithAlpha(50);
        shadow.MaskFilter = blur;
        var shadowRect = layout.Board;
        shadowRect.Offset(0, 2 * density);
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

    // 滑らせている間は動く前のタイル、その後は今のタイル (合体は弾み、新しいタイルは拡大)
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
                else if (frame.Move?.Merged.Any(x => x.Id == tile.Id) ?? false)
                {
                    scale = 1f + (0.18f * MathF.Sin(t * MathF.PI));
                }

                DrawTile(canvas, layout, density, layout.GetCell(tile.Row, tile.Column), tile.Value, scale);
            }
        }
    }

    // 上が明るいグラデーションのタイルと影 (128 以上は光る)、中央に値
    private static void DrawTile(SKCanvas canvas, BoardLayout layout, float density, SKRect cell, int value, float scale)
    {
        if (scale > 0.01f)
        {
            var width = cell.Width * scale;
            var rect = SKRect.Create(cell.MidX - (width / 2), cell.MidY - (width / 2), width, width);
            var radius = layout.TileRadius * scale;
            var (color, textColor) = GetColors(value);

            using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (value >= 128 ? 7 : 2) * density);
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            paint.MaskFilter = blur;

            if (value >= 128)
            {
                paint.Color = color.WithAlpha((byte)Math.Min(200, 60 + (Math.Log2(value) * 12)));
                canvas.DrawRoundRect(rect, radius, radius, paint);
            }
            else
            {
                paint.Color = SKColors.Black.WithAlpha(40);
                var shadow = rect;
                shadow.Offset(0, 2 * density);
                canvas.DrawRoundRect(shadow, radius, radius, paint);
            }

            // 塗りの透明度はグラデーションにも掛かるので、影の色から不透明に戻す
            paint.MaskFilter = null;
            paint.Color = SKColors.White;
            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(rect.Left, rect.Top),
                new SKPoint(rect.Left, rect.Bottom),
                [Blend(color, SKColors.White, 0.18f), color],
                SKShaderTileMode.Clamp);
            paint.Shader = shader;
            canvas.DrawRoundRect(rect, radius, radius, paint);
            paint.Shader = null;

            var text = value.ToString(CultureInfo.InvariantCulture);
            var ratio = text.Length switch
            {
                <= 2 => 0.48f,
                3 => 0.40f,
                4 => 0.32f,
                _ => 0.26f
            };
            using var font = new SKFont(BoldTypeface, cell.Width * ratio * scale);
            paint.Color = textColor;
            var baseline = rect.MidY - ((font.Metrics.Ascent + font.Metrics.Descent) / 2);
            canvas.DrawText(text, rect.MidX, baseline, SKTextAlign.Center, font, paint);
        }
    }

    // 値ごとの色 (2 と 4 は濃い文字、ほかは白い文字)
    private static (SKColor Color, SKColor Text) GetColors(int value) => value switch
    {
        2 => (SKColor.Parse("#EEE4DA"), DarkTextColor),
        4 => (SKColor.Parse("#EDE0C8"), DarkTextColor),
        8 => (SKColor.Parse("#F2B179"), SKColors.White),
        16 => (SKColor.Parse("#F59563"), SKColors.White),
        32 => (SKColor.Parse("#F67C5F"), SKColors.White),
        64 => (SKColor.Parse("#F65E3B"), SKColors.White),
        128 => (SKColor.Parse("#EDCF72"), SKColors.White),
        256 => (SKColor.Parse("#EDCC61"), SKColors.White),
        512 => (SKColor.Parse("#EDC850"), SKColors.White),
        1024 => (SKColor.Parse("#EDC53F"), SKColors.White),
        2048 => (SKColor.Parse("#EDC22E"), SKColors.White),
        _ => (SKColor.Parse("#3C3A32"), SKColors.White)
    };

    private static SKColor Blend(SKColor from, SKColor to, float amount) => new(
        (byte)(from.Red + ((to.Red - from.Red) * amount)),
        (byte)(from.Green + ((to.Green - from.Green) * amount)),
        (byte)(from.Blue + ((to.Blue - from.Blue) * amount)),
        from.Alpha);

    private static float Lerp(float from, float to, float t) => from + ((to - from) * t);

    private static float EaseOut(double t) => 1f - MathF.Pow(1f - (float)t, 3);

    // 少し行き過ぎて戻る
    private static float EaseOutBack(float t)
    {
        const float overshoot = 1.70158f;
        var x = t - 1;
        return 1 + (x * x * (((overshoot + 1) * x) + overshoot));
    }

    // 盤面と各マスの位置 (影がはみ出さないように内側に寄せる)
    private readonly struct BoardLayout
    {
        public SKRect Board { get; }

        public float BoardRadius { get; }

        public float TileRadius { get; }

        private readonly float gap;

        private readonly float cellSize;

        public BoardLayout(float size, float density)
        {
            var inset = 8 * density;
            Board = SKRect.Create(inset, inset, size - (inset * 2), size - (inset * 2));
            gap = Board.Width * 0.03f;
            cellSize = (Board.Width - (gap * (Puzzle2048Game.Size + 1))) / Puzzle2048Game.Size;
            BoardRadius = gap * 1.6f;
            TileRadius = cellSize * 0.1f;
        }

        public SKRect GetCell(int row, int column) => SKRect.Create(
            Board.Left + gap + (column * (cellSize + gap)),
            Board.Top + gap + (row * (cellSize + gap)),
            cellSize,
            cellSize);
    }
}
