namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.App;

// マインスイーパーの盤面 (正方形)。タップと長押しを見分けて行と列をコマンドで渡し、見せたマスの覆いを見せた順に縮めて消す
public sealed class MinesweeperBoard : SKCanvasView
{
    private const string AnimationName = "MinesweeperBoard";

    // 覆いが消えるまでの時間と、見せた順にずらす時間の合計 (ms)
    private const double CoverLength = 180;
    private const double SpreadLength = 360;

    // 動かしてもタップとみなす距離 (論理単位)
    private const float TouchSlop = 12;

    private static readonly TimeSpan LongPressDelay = TimeSpan.FromMilliseconds(350);

    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    // 閉じたマス (芝) と開いたマス (砂) の市松、境目の縁
    private static readonly SKColor HiddenLightColor = SKColor.Parse("#AAD751");
    private static readonly SKColor HiddenDarkColor = SKColor.Parse("#A2D149");
    private static readonly SKColor HiddenPressedColor = SKColor.Parse("#C6E48B");
    private static readonly SKColor OpenedLightColor = SKColor.Parse("#E5C29F");
    private static readonly SKColor OpenedDarkColor = SKColor.Parse("#D7B899");
    private static readonly SKColor EdgeColor = SKColor.Parse("#87AF3A");

    private static readonly SKColor FlagColor = SKColor.Parse("#F23607");
    private static readonly SKColor FlagPoleColor = SKColor.Parse("#4E342E");
    private static readonly SKColor CrossColor = SKColor.Parse("#B71C1C");
    private static readonly SKColor ExplodedColor = SKColor.Parse("#DB3236");

    private static readonly SKColor[] NumberColors =
    [
        SKColor.Parse("#1976D2"),
        SKColor.Parse("#388E3C"),
        SKColor.Parse("#D32F2F"),
        SKColor.Parse("#7B1FA2"),
        SKColor.Parse("#FF8F00"),
        SKColor.Parse("#0097A7"),
        SKColor.Parse("#424242"),
        SKColor.Parse("#9E9E9E")
    ];

    // 地雷はマスごとに色を変える
    private static readonly SKColor[] MineColors =
    [
        SKColor.Parse("#DB3236"),
        SKColor.Parse("#F4C20D"),
        SKColor.Parse("#4885ED"),
        SKColor.Parse("#48E6F1"),
        SKColor.Parse("#B648F2"),
        SKColor.Parse("#ED44B5"),
        SKColor.Parse("#F4840D"),
        SKColor.Parse("#008744")
    ];

    public static readonly BindableProperty BoardProperty = BindableProperty.Create(
        nameof(Board),
        typeof(MinesweeperFrame),
        typeof(MinesweeperBoard),
        propertyChanged: OnBoardChanged);

    public MinesweeperFrame? Board
    {
        get => (MinesweeperFrame?)GetValue(BoardProperty);
        set => SetValue(BoardProperty, value);
    }

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(MinesweeperBoard));

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty LongPressCommandProperty = BindableProperty.Create(
        nameof(LongPressCommand),
        typeof(ICommand),
        typeof(MinesweeperBoard));

    public ICommand? LongPressCommand
    {
        get => (ICommand?)GetValue(LongPressCommandProperty);
        set => SetValue(LongPressCommandProperty, value);
    }

    // 覆いを消すアニメーションの進み (1 は止まっている)
    private double progress = 1;

    // 押しているマス (動かすか、長押しになると外す)
    private MinesweeperCell? pressedCell;

    private SKPoint pressedPoint;

    // 押すたびに変え、前の押下の長押しの判定を捨てる
    private int pressVersion;

    public MinesweeperBoard()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
    }

    private static void OnBoardChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var board = (MinesweeperBoard)bindable;
        board.AbortAnimation(AnimationName);
        if (newValue is MinesweeperFrame { Revealed.Count: > 0 } frame)
        {
            board.progress = 0;
            board.Animate(
                AnimationName,
                x =>
                {
                    board.progress = x;
                    board.InvalidateSurface();
                },
                length: (uint)GetAnimationLength(frame.Revealed.Count),
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

    private static double GetAnimationLength(int count) => CoverLength + (count > 1 ? SpreadLength : 0);

    // 幅と高さの短い方の正方形にする
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var size = Math.Min(widthConstraint, heightConstraint);
        return new Size(size, size);
    }

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    // 離したらタップ、押したまま止めたら長押し。動かしたらどちらにもしない
    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        var density = Width > 0 ? (float)(CanvasSize.Width / Width) : 1f;
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                pressedCell = FindCell(e.Location, density);
                pressedPoint = e.Location;
                pressVersion++;
                if (pressedCell is not null)
                {
                    var version = pressVersion;
                    Dispatcher.DispatchDelayed(LongPressDelay, () => OnLongPress(version));
                }

                InvalidateSurface();
                break;
            case SKTouchAction.Moved:
                if ((pressedCell is not null) && (SKPoint.Distance(e.Location, pressedPoint) > TouchSlop * density))
                {
                    pressedCell = null;
                    InvalidateSurface();
                }

                break;
            case SKTouchAction.Released:
                if (pressedCell is { } cell)
                {
                    pressedCell = null;
                    Execute(TapCommand, cell);
                    InvalidateSurface();
                }

                break;
            case SKTouchAction.Cancelled:
            case SKTouchAction.Exited:
                pressedCell = null;
                InvalidateSurface();
                break;
        }

        e.Handled = true;
    }

    private void OnLongPress(int version)
    {
        if ((version == pressVersion) && (pressedCell is { } cell))
        {
            pressedCell = null;
            Execute(LongPressCommand, cell);
            InvalidateSurface();
        }
    }

    private static void Execute(ICommand? command, MinesweeperCell cell)
    {
        if (command?.CanExecute(cell) ?? false)
        {
            command.Execute(cell);
        }
    }

    private MinesweeperCell? FindCell(SKPoint point, float density)
    {
        MinesweeperCell? cell = null;
        if (Board is { } board)
        {
            var layout = new BoardLayout(Math.Min(CanvasSize.Width, CanvasSize.Height), density, board.Game);
            cell = layout.Find(point);
        }

        return cell;
    }

    //--------------------------------------------------------------------------------
    // Paint
    //--------------------------------------------------------------------------------

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        if ((Board is { } board) && (info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
            // SKCanvas は物理ピクセルなので、論理単位の長さに密度を掛ける
            var density = (float)(info.Width / Width);
            var game = board.Game;
            var layout = new BoardLayout(Math.Min(info.Width, info.Height), density, game);

            DrawShadow(canvas, layout, density);

            canvas.Save();
            using var clip = new SKRoundRect(layout.Board, layout.Radius);
            canvas.ClipRoundRect(clip, antialias: true);
            DrawCells(canvas, layout, game, density);
            DrawEdges(canvas, layout, game, density);
            DrawCovers(canvas, layout, board);
            canvas.Restore();
        }
    }

    private static void DrawShadow(SKCanvas canvas, BoardLayout layout, float density)
    {
        using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3 * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = SKColors.Black.WithAlpha(60);
        paint.MaskFilter = blur;
        var rect = layout.Board;
        rect.Offset(0, 2 * density);
        canvas.DrawRoundRect(rect, layout.Radius, layout.Radius, paint);
    }

    // 開いた見た目 (開いたマスと、負けたときの旗の無い地雷) は砂、それ以外は芝 (旗、負けたときの間違った旗は ×)
    private void DrawCells(SKCanvas canvas, BoardLayout layout, MinesweeperGame game, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var font = new SKFont(BoldTypeface, layout.CellSize * 0.62f);

        for (var row = 0; row < game.Rows; row++)
        {
            for (var column = 0; column < game.Columns; column++)
            {
                var cell = new MinesweeperCell(row, column);
                var rect = layout.GetCell(cell);
                var even = ((row + column) % 2) == 0;
                if (IsOpenLook(game, cell))
                {
                    if (game.IsMine(cell))
                    {
                        var exploded = game.Exploded == cell;
                        var color = exploded ? ExplodedColor : MineColors[((row * 7) + (column * 3)) % MineColors.Length];
                        paint.Color = color;
                        canvas.DrawRect(rect, paint);
                        paint.Color = Darken(color, 0.45f);
                        canvas.DrawCircle(rect.MidX, rect.MidY, rect.Width * (exploded ? 0.3f : 0.22f), paint);
                    }
                    else
                    {
                        paint.Color = even ? OpenedLightColor : OpenedDarkColor;
                        canvas.DrawRect(rect, paint);
                        var number = game.GetNumber(cell);
                        if (number > 0)
                        {
                            paint.Color = NumberColors[number - 1];
                            var baseline = rect.MidY - ((font.Metrics.Ascent + font.Metrics.Descent) / 2);
                            canvas.DrawText(number.ToString(CultureInfo.InvariantCulture), rect.MidX, baseline, SKTextAlign.Center, font, paint);
                        }
                    }
                }
                else
                {
                    var state = game.GetState(cell);
                    paint.Color = (pressedCell == cell) && (state == MinesweeperCellState.Hidden)
                        ? HiddenPressedColor
                        : even ? HiddenLightColor : HiddenDarkColor;
                    canvas.DrawRect(rect, paint);
                    if (state == MinesweeperCellState.Flagged)
                    {
                        DrawFlag(canvas, rect, paint);
                        if ((game.State == MinesweeperState.Lost) && !game.IsMine(cell))
                        {
                            DrawCross(canvas, rect, density);
                        }
                    }
                }
            }
        }
    }

    // 開いたマスの、閉じたマスとの境目に縁を付ける
    private static void DrawEdges(SKCanvas canvas, BoardLayout layout, MinesweeperGame game, float density)
    {
        using var paint = new SKPaint();
        paint.Color = EdgeColor;
        var width = Math.Max(2 * density, layout.CellSize * 0.08f);

        for (var row = 0; row < game.Rows; row++)
        {
            for (var column = 0; column < game.Columns; column++)
            {
                if (IsOpenLook(game, new MinesweeperCell(row, column)))
                {
                    var rect = layout.GetCell(new MinesweeperCell(row, column));
                    if ((row > 0) && !IsOpenLook(game, new MinesweeperCell(row - 1, column)))
                    {
                        canvas.DrawRect(SKRect.Create(rect.Left, rect.Top, rect.Width, width), paint);
                    }

                    if ((row < game.Rows - 1) && !IsOpenLook(game, new MinesweeperCell(row + 1, column)))
                    {
                        canvas.DrawRect(SKRect.Create(rect.Left, rect.Bottom - width, rect.Width, width), paint);
                    }

                    if ((column > 0) && !IsOpenLook(game, new MinesweeperCell(row, column - 1)))
                    {
                        canvas.DrawRect(SKRect.Create(rect.Left, rect.Top, width, rect.Height), paint);
                    }

                    if ((column < game.Columns - 1) && !IsOpenLook(game, new MinesweeperCell(row, column + 1)))
                    {
                        canvas.DrawRect(SKRect.Create(rect.Right - width, rect.Top, width, rect.Height), paint);
                    }
                }
            }
        }
    }

    // 見せたマスの芝の覆いを、見せた順に少しずつずらして縮めて消す
    private void DrawCovers(SKCanvas canvas, BoardLayout layout, MinesweeperFrame board)
    {
        if (progress < 1)
        {
            using var paint = new SKPaint();
            paint.IsAntialias = true;

            var count = board.Revealed.Count;
            var elapsed = progress * GetAnimationLength(count);
            for (var i = 0; i < count; i++)
            {
                var delay = count > 1 ? SpreadLength * i / (count - 1) : 0;
                var local = Math.Clamp((elapsed - delay) / CoverLength, 0, 1);
                if (local < 1)
                {
                    var cell = board.Revealed[i];
                    var rect = layout.GetCell(cell);
                    var size = rect.Width * (1f - (float)(local * local));
                    var even = ((cell.Row + cell.Column) % 2) == 0;
                    paint.Color = (even ? HiddenLightColor : HiddenDarkColor).WithAlpha((byte)(255 * (1 - local)));
                    var cover = SKRect.Create(rect.MidX - (size / 2), rect.MidY - (size / 2), size, size);
                    canvas.DrawRoundRect(cover, size * 0.15f, size * 0.15f, paint);
                }
            }
        }
    }

    // 竿と台と赤い旗
    private static void DrawFlag(SKCanvas canvas, SKRect rect, SKPaint paint)
    {
        var w = rect.Width;
        paint.Color = FlagPoleColor;
        canvas.DrawRect(SKRect.Create(rect.Left + (w * 0.36f), rect.Top + (w * 0.2f), w * 0.07f, w * 0.58f), paint);
        canvas.DrawRoundRect(SKRect.Create(rect.Left + (w * 0.24f), rect.Top + (w * 0.74f), w * 0.4f, w * 0.08f), w * 0.03f, w * 0.03f, paint);

        using var builder = new SKPathBuilder();
        builder.MoveTo(rect.Left + (w * 0.43f), rect.Top + (w * 0.2f));
        builder.LineTo(rect.Left + (w * 0.8f), rect.Top + (w * 0.36f));
        builder.LineTo(rect.Left + (w * 0.43f), rect.Top + (w * 0.52f));
        builder.Close();
        using var path = builder.Detach();
        paint.Color = FlagColor;
        canvas.DrawPath(path, paint);
    }

    private static void DrawCross(SKCanvas canvas, SKRect rect, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.StrokeWidth = Math.Max(2 * density, rect.Width * 0.08f);
        paint.Color = CrossColor;
        var margin = rect.Width * 0.22f;
        canvas.DrawLine(rect.Left + margin, rect.Top + margin, rect.Right - margin, rect.Bottom - margin, paint);
        canvas.DrawLine(rect.Right - margin, rect.Top + margin, rect.Left + margin, rect.Bottom - margin, paint);
    }

    private static bool IsOpenLook(MinesweeperGame game, MinesweeperCell cell) =>
        (game.GetState(cell) == MinesweeperCellState.Opened) ||
        ((game.State == MinesweeperState.Lost) && game.IsMine(cell) && (game.GetState(cell) != MinesweeperCellState.Flagged));

    private static SKColor Darken(SKColor color, float amount) => new(
        (byte)(color.Red * (1 - amount)),
        (byte)(color.Green * (1 - amount)),
        (byte)(color.Blue * (1 - amount)),
        color.Alpha);

    // 盤面と各マスの位置 (影がはみ出さないように内側に寄せる)
    private readonly struct BoardLayout
    {
        private readonly int rows;

        private readonly int columns;

        public SKRect Board { get; }

        public float Radius { get; }

        public float CellSize { get; }

        public BoardLayout(float size, float density, MinesweeperGame game)
        {
            rows = game.Rows;
            columns = game.Columns;
            var inset = 8 * density;
            var available = size - (inset * 2);
            CellSize = available / Math.Max(rows, columns);
            Board = SKRect.Create(
                inset + ((available - (CellSize * columns)) / 2),
                inset + ((available - (CellSize * rows)) / 2),
                CellSize * columns,
                CellSize * rows);
            Radius = 12 * density;
        }

        public SKRect GetCell(MinesweeperCell cell) =>
            SKRect.Create(Board.Left + (cell.Column * CellSize), Board.Top + (cell.Row * CellSize), CellSize, CellSize);

        public MinesweeperCell? Find(SKPoint point) =>
            Board.Contains(point)
                ? new MinesweeperCell(
                    Math.Min(rows - 1, (int)((point.Y - Board.Top) / CellSize)),
                    Math.Min(columns - 1, (int)((point.X - Board.Left) / CellSize)))
                : null;
    }
}
