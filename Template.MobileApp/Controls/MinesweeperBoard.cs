namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.App;

public sealed class MinesweeperBoard : SKCanvasView
{
    private const string AnimationName = "MinesweeperBoard";

    private const double CoverLength = 180;

    private const double SpreadLength = 360;

    private const float TouchSlop = 12;

    private const float NumberFontRate = 0.62f;

    private const int MineColorRowStep = 7;

    private const int MineColorColumnStep = 3;

    private const float MineCoreRate = 0.22f;

    private const float ExplodedCoreRate = 0.3f;

    private const float MineCoreDarken = 0.45f;

    private const float EdgeMinimumWidth = 2f;

    private const float EdgeWidthRate = 0.08f;

    private const float CoverRadiusRate = 0.15f;

    private const float FlagBaseRadius = 0.03f;

    private const float CrossMinimumWidth = 2f;

    private const float CrossWidthRate = 0.08f;

    private const float CrossMarginRate = 0.22f;

    private const float ShadowBlur = 3f;

    private const float ShadowOffset = 2f;

    private const byte ShadowAlpha = 60;

    private const float BoardInset = 8f;

    private const float BoardRadius = 12f;

    private static readonly TimeSpan LongPressDelay = TimeSpan.FromMilliseconds(350);

    private static readonly SKTypeface BoldTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

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

    private static readonly (float Left, float Top, float Width, float Height) FlagPole = (0.36f, 0.2f, 0.07f, 0.58f);

    private static readonly (float Left, float Top, float Width, float Height) FlagBase = (0.24f, 0.74f, 0.4f, 0.08f);

    private static readonly SKPoint[] FlagCloth = [new(0.43f, 0.2f), new(0.8f, 0.36f), new(0.43f, 0.52f)];

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private double progress = 1;

    private MinesweeperCell? pressedCell;

    private SKPoint pressedPoint;

    private int pressVersion;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty BoardProperty = BindableProperty.Create(
        nameof(Board),
        typeof(MinesweeperFrame),
        typeof(MinesweeperBoard),
        propertyChanged: OnBoardChanged);

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(MinesweeperBoard));

    public static readonly BindableProperty LongPressCommandProperty = BindableProperty.Create(
        nameof(LongPressCommand),
        typeof(ICommand),
        typeof(MinesweeperBoard));

    public MinesweeperFrame? Board
    {
        get => (MinesweeperFrame?)GetValue(BoardProperty);
        set => SetValue(BoardProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public ICommand? LongPressCommand
    {
        get => (ICommand?)GetValue(LongPressCommandProperty);
        set => SetValue(LongPressCommandProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public MinesweeperBoard()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
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

        if ((Board is { } board) && (info.Width > 0) && (info.Height > 0) && (Width > 0))
        {
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
        using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, ShadowBlur * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = SKColors.Black.WithAlpha(ShadowAlpha);
        paint.MaskFilter = blur;
        var rect = layout.Board;
        rect.Offset(0, ShadowOffset * density);
        canvas.DrawRoundRect(rect, layout.Radius, layout.Radius, paint);
    }

    private void DrawCells(SKCanvas canvas, BoardLayout layout, MinesweeperGame game, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var font = new SKFont(BoldTypeface, layout.CellSize * NumberFontRate);

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
                        var color = exploded ? ExplodedColor : MineColors[((row * MineColorRowStep) + (column * MineColorColumnStep)) % MineColors.Length];
                        paint.Color = color;
                        canvas.DrawRect(rect, paint);
                        paint.Color = color.Lerp(SKColors.Black.WithAlpha(color.Alpha), MineCoreDarken);
                        canvas.DrawCircle(rect.MidX, rect.MidY, rect.Width * (exploded ? ExplodedCoreRate : MineCoreRate), paint);
                    }
                    else
                    {
                        paint.Color = even ? OpenedLightColor : OpenedDarkColor;
                        canvas.DrawRect(rect, paint);
                        var number = game.GetNumber(cell);
                        if (number > 0)
                        {
                            paint.Color = NumberColors[number - 1];
                            canvas.DrawText(number.ToString(CultureInfo.InvariantCulture), rect.MidX, font.Metrics.GetCenterBaseline(rect.MidY), SKTextAlign.Center, font, paint);
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

    private static void DrawEdges(SKCanvas canvas, BoardLayout layout, MinesweeperGame game, float density)
    {
        using var paint = new SKPaint();
        paint.Color = EdgeColor;
        var width = Math.Max(EdgeMinimumWidth * density, layout.CellSize * EdgeWidthRate);

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
                    paint.Color = (even ? HiddenLightColor : HiddenDarkColor).WithOpacity((float)(1 - local));
                    var cover = SKRect.Create(rect.MidX - (size / 2), rect.MidY - (size / 2), size, size);
                    canvas.DrawRoundRect(cover, size * CoverRadiusRate, size * CoverRadiusRate, paint);
                }
            }
        }
    }

    private static void DrawFlag(SKCanvas canvas, SKRect rect, SKPaint paint)
    {
        var w = rect.Width;
        paint.Color = FlagPoleColor;
        canvas.DrawRect(SKRect.Create(rect.Left + (w * FlagPole.Left), rect.Top + (w * FlagPole.Top), w * FlagPole.Width, w * FlagPole.Height), paint);
        canvas.DrawRoundRect(SKRect.Create(rect.Left + (w * FlagBase.Left), rect.Top + (w * FlagBase.Top), w * FlagBase.Width, w * FlagBase.Height), w * FlagBaseRadius, w * FlagBaseRadius, paint);

        using var builder = new SKPathBuilder();
        builder.MoveTo(rect.Left + (w * FlagCloth[0].X), rect.Top + (w * FlagCloth[0].Y));
        builder.LineTo(rect.Left + (w * FlagCloth[1].X), rect.Top + (w * FlagCloth[1].Y));
        builder.LineTo(rect.Left + (w * FlagCloth[2].X), rect.Top + (w * FlagCloth[2].Y));
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
        paint.StrokeWidth = Math.Max(CrossMinimumWidth * density, rect.Width * CrossWidthRate);
        paint.Color = CrossColor;
        var margin = rect.Width * CrossMarginRate;
        canvas.DrawLine(rect.Left + margin, rect.Top + margin, rect.Right - margin, rect.Bottom - margin, paint);
        canvas.DrawLine(rect.Right - margin, rect.Top + margin, rect.Left + margin, rect.Bottom - margin, paint);
    }

    private static bool IsOpenLook(MinesweeperGame game, MinesweeperCell cell) =>
        (game.GetState(cell) == MinesweeperCellState.Opened) ||
        ((game.State == MinesweeperState.Lost) && game.IsMine(cell) && (game.GetState(cell) != MinesweeperCellState.Flagged));

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
            var inset = BoardInset * density;
            var available = size - (inset * 2);
            CellSize = available / Math.Max(rows, columns);
            Board = SKRect.Create(
                inset + ((available - (CellSize * columns)) / 2),
                inset + ((available - (CellSize * rows)) / 2),
                CellSize * columns,
                CellSize * rows);
            Radius = BoardRadius * density;
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

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

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

    private static void Execute(ICommand? command, MinesweeperCell cell)
    {
        if (command?.CanExecute(cell) ?? false)
        {
            command.Execute(cell);
        }
    }
}
