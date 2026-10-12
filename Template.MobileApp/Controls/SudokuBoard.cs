namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.App;

public sealed class SudokuBoard : SKCanvasView
{
    private const string AnimationName = "SudokuBoard";

    private const double PopLength = 280;

    private const double FlashStep = 55;

    private const double FlashLength = 480;

    private const int BlockSize = 3;

    private const int NoteColumns = 3;

    private const float NumberFontRate = 0.6f;

    private const float NoteFontRate = 0.26f;

    private const float NotePaddingRate = 0.12f;

    private const float ThickLineWidth = 2f;

    private const float FrameWidth = 2.5f;

    private const float ShadowBlur = 3f;

    private const float ShadowOffset = 2f;

    private const byte ShadowAlpha = 60;

    private const double FlashAlpha = 150;

    private const float FlashScale = 0.2f;

    private const float PopStartScale = 0.4f;

    private const double ShakeFrequency = 6;

    private const double ShakeWidth = 4;

    private const float Overshoot = 1.70158f;

    private const float BoardInset = 8f;

    private const float BoardRadius = 12f;

    private static readonly SKTypeface GivenTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);

    private static readonly SKTypeface InputTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Normal);

    private static readonly SKColor CellColor = SKColors.White;

    private static readonly SKColor PeerColor = SKColor.Parse("#E8ECFF");

    private static readonly SKColor SameColor = SKColor.Parse("#D2D9FF");

    private static readonly SKColor ConflictColor = SKColor.Parse("#FFE0E6");

    private static readonly SKColor SelectedColor = SKColor.Parse("#B7C2FF");

    private static readonly SKColor ThinLineColor = SKColor.Parse("#D9DDF3");

    private static readonly SKColor ThickLineColor = SKColor.Parse("#312E81");

    private static readonly SKColor GivenColor = SKColor.Parse("#1E1B4B");

    private static readonly SKColor InputColor = SKColor.Parse("#4F46E5");

    private static readonly SKColor WrongColor = SKColor.Parse("#E11D48");

    private static readonly SKColor NoteColor = SKColor.Parse("#7C7F9E");

    private static readonly SKColor FlashColor = SKColor.Parse("#818CF8");

    private static readonly SKColor CompleteColor = SKColor.Parse("#FBBF24");

    //--------------------------------------------------------------------------------
    // Member
    //--------------------------------------------------------------------------------

    private double progress = 1;

    private SudokuCell? popCell;

    private SudokuCell[] flashCells = [];

    private SudokuCell flashOrigin;

    private bool completed;

    private SudokuCell? touchedCell;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    public static readonly BindableProperty BoardProperty = BindableProperty.Create(
        nameof(Board),
        typeof(SudokuFrame),
        typeof(SudokuBoard),
        propertyChanged: OnBoardChanged);

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(SudokuBoard));

    public SudokuFrame? Board
    {
        get => (SudokuFrame?)GetValue(BoardProperty);
        set => SetValue(BoardProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public SudokuBoard()
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
            var layout = new BoardLayout(Math.Min(info.Width, info.Height), density);
            var elapsed = progress * GetAnimationLength();

            DrawShadow(canvas, layout, density);

            canvas.Save();
            using var clip = new SKRoundRect(layout.Board, layout.Radius);
            canvas.ClipRoundRect(clip, antialias: true);
            DrawCells(canvas, layout, board);
            DrawFlash(canvas, layout, elapsed);
            DrawLines(canvas, layout, density);
            DrawNumbers(canvas, layout, board, density, elapsed);
            canvas.Restore();

            DrawFrame(canvas, layout, density);
        }
    }

    private static void DrawShadow(SKCanvas canvas, BoardLayout layout, float density)
    {
        using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, ShadowBlur * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = ThickLineColor.WithAlpha(ShadowAlpha);
        paint.MaskFilter = blur;
        var rect = layout.Board;
        rect.Offset(0, ShadowOffset * density);
        canvas.DrawRoundRect(rect, layout.Radius, layout.Radius, paint);
    }

    private static void DrawCells(SKCanvas canvas, BoardLayout layout, SudokuFrame board)
    {
        using var paint = new SKPaint();
        var game = board.Game;
        var selected = board.Selected;
        var selectedValue = selected is { } s ? game.GetValue(s) : 0;

        for (var row = 0; row < SudokuGame.Size; row++)
        {
            for (var column = 0; column < SudokuGame.Size; column++)
            {
                var cell = new SudokuCell(row, column);
                var value = game.GetValue(cell);
                paint.Color = cell == selected
                    ? SelectedColor
                    : game.HasConflict(cell)
                        ? ConflictColor
                        : (selectedValue != 0) && (value == selectedValue)
                            ? SameColor
                            : (selected is { } target) && IsPeer(cell, target)
                                ? PeerColor
                                : CellColor;
                canvas.DrawRect(layout.GetCell(cell), paint);
            }
        }
    }

    private void DrawFlash(SKCanvas canvas, BoardLayout layout, double elapsed)
    {
        if (progress < 1)
        {
            using var paint = new SKPaint();
            var color = completed ? CompleteColor : FlashColor;
            foreach (var cell in flashCells)
            {
                var local = GetFlashProgress(cell, elapsed);
                if (local is > 0 and < 1)
                {
                    paint.Color = color.WithAlpha((byte)(FlashAlpha * Math.Sin(Math.PI * local)));
                    canvas.DrawRect(layout.GetCell(cell), paint);
                }
            }
        }
    }

    private static void DrawLines(SKCanvas canvas, BoardLayout layout, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;

        var board = layout.Board;
        paint.Color = ThinLineColor;
        paint.StrokeWidth = Math.Max(1, density);
        for (var i = 1; i < SudokuGame.Size; i++)
        {
            if (i % BlockSize != 0)
            {
                var offset = i * layout.CellSize;
                canvas.DrawLine(board.Left + offset, board.Top, board.Left + offset, board.Bottom, paint);
                canvas.DrawLine(board.Left, board.Top + offset, board.Right, board.Top + offset, paint);
            }
        }

        paint.Color = ThickLineColor;
        paint.StrokeWidth = ThickLineWidth * density;
        for (var i = BlockSize; i < SudokuGame.Size; i += BlockSize)
        {
            var offset = i * layout.CellSize;
            canvas.DrawLine(board.Left + offset, board.Top, board.Left + offset, board.Bottom, paint);
            canvas.DrawLine(board.Left, board.Top + offset, board.Right, board.Top + offset, paint);
        }
    }

    private void DrawNumbers(SKCanvas canvas, BoardLayout layout, SudokuFrame board, float density, double elapsed)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var givenFont = new SKFont(GivenTypeface, layout.CellSize * NumberFontRate);
        using var inputFont = new SKFont(InputTypeface, layout.CellSize * NumberFontRate);
        using var noteFont = new SKFont(InputTypeface, layout.CellSize * NoteFontRate);
        using var strongNoteFont = new SKFont(GivenTypeface, layout.CellSize * NoteFontRate);

        var game = board.Game;
        var selectedValue = board.Selected is { } s ? game.GetValue(s) : 0;

        for (var row = 0; row < SudokuGame.Size; row++)
        {
            for (var column = 0; column < SudokuGame.Size; column++)
            {
                var cell = new SudokuCell(row, column);
                var rect = layout.GetCell(cell);

                canvas.Save();
                ApplyMotion(canvas, game, cell, rect, density, elapsed);

                var value = game.GetValue(cell);
                if (value != 0)
                {
                    var given = game.IsGiven(cell);
                    var font = given ? givenFont : inputFont;
                    paint.Color = given ? GivenColor : game.IsWrong(cell) ? WrongColor : InputColor;
                    DrawText(canvas, value, rect.MidX, rect.MidY, font, paint);
                }
                else
                {
                    var noteSize = (rect.Width - (rect.Width * NotePaddingRate)) / NoteColumns;
                    for (var note = 1; note <= SudokuGame.Size; note++)
                    {
                        if (game.HasNote(cell, note))
                        {
                            var noteRow = (note - 1) / NoteColumns;
                            var noteColumn = (note - 1) % NoteColumns;
                            var x = rect.Left + (rect.Width * NotePaddingRate / 2) + (noteSize * (noteColumn + 0.5f));
                            var y = rect.Top + (rect.Width * NotePaddingRate / 2) + (noteSize * (noteRow + 0.5f));
                            var strong = note == selectedValue;
                            paint.Color = strong ? InputColor : NoteColor;
                            DrawText(canvas, note, x, y, strong ? strongNoteFont : noteFont, paint);
                        }
                    }
                }

                canvas.Restore();
            }
        }
    }

    private void ApplyMotion(SKCanvas canvas, SudokuGame game, SudokuCell cell, SKRect rect, float density, double elapsed)
    {
        if (progress < 1)
        {
            var scale = 1f;
            if (cell == popCell)
            {
                var t = Math.Clamp(elapsed / PopLength, 0, 1);
                scale = PopStartScale + ((1 - PopStartScale) * EaseOutBack((float)t));
                if (game.IsWrong(cell))
                {
                    canvas.Translate((float)(Math.Sin(t * Math.PI * ShakeFrequency) * (1 - t) * ShakeWidth * density), 0);
                }
            }
            else
            {
                var local = GetFlashProgress(cell, elapsed);
                if (local is > 0 and < 1)
                {
                    scale = 1f + (FlashScale * (float)Math.Sin(Math.PI * local));
                }
            }

            canvas.Scale(scale, scale, rect.MidX, rect.MidY);
        }
    }

    private static void DrawFrame(SKCanvas canvas, BoardLayout layout, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = FrameWidth * density;
        paint.Color = ThickLineColor;
        var rect = layout.Board;
        rect.Inflate(-paint.StrokeWidth / 2, -paint.StrokeWidth / 2);
        canvas.DrawRoundRect(rect, layout.Radius, layout.Radius, paint);
    }

    private static void DrawText(SKCanvas canvas, int value, float x, float y, SKFont font, SKPaint paint) =>
        canvas.DrawText(value.ToString(CultureInfo.InvariantCulture), x, font.Metrics.GetCenterBaseline(y), SKTextAlign.Center, font, paint);

    private static bool IsPeer(SudokuCell cell, SudokuCell target) =>
        (cell.Row == target.Row) ||
        (cell.Column == target.Column) ||
        (((cell.Row / BlockSize) == (target.Row / BlockSize)) && ((cell.Column / BlockSize) == (target.Column / BlockSize)));

    private static float EaseOutBack(float t)
    {
        return 1 + ((Overshoot + 1) * MathF.Pow(t - 1, 3)) + (Overshoot * MathF.Pow(t - 1, 2));
    }

    private readonly struct BoardLayout
    {
        public SKRect Board { get; }

        public float Radius { get; }

        public float CellSize { get; }

        public BoardLayout(float size, float density)
        {
            var inset = BoardInset * density;
            CellSize = (size - (inset * 2)) / SudokuGame.Size;
            Board = SKRect.Create(inset, inset, CellSize * SudokuGame.Size, CellSize * SudokuGame.Size);
            Radius = BoardRadius * density;
        }

        public SKRect GetCell(SudokuCell cell) =>
            SKRect.Create(Board.Left + (cell.Column * CellSize), Board.Top + (cell.Row * CellSize), CellSize, CellSize);

        public SudokuCell? Find(SKPoint point) =>
            Board.Contains(point)
                ? new SudokuCell(
                    Math.Min(SudokuGame.Size - 1, (int)((point.Y - Board.Top) / CellSize)),
                    Math.Min(SudokuGame.Size - 1, (int)((point.X - Board.Left) / CellSize)))
                : null;
    }

    //--------------------------------------------------------------------------------
    // Animation
    //--------------------------------------------------------------------------------

    private static void OnBoardChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var board = (SudokuBoard)bindable;
        if ((newValue is SudokuFrame frame) && ((frame.Placed is not null) || (frame.Flash.Length > 0)))
        {
            board.AbortAnimation(AnimationName);
            board.popCell = frame.Placed;
            board.flashCells = frame.Flash;
            board.flashOrigin = frame.Placed ?? frame.Selected ?? new SudokuCell(SudokuGame.Size / 2, SudokuGame.Size / 2);
            board.completed = frame.Game.IsCompleted;
            board.progress = 0;
            board.Animate(
                AnimationName,
                x =>
                {
                    board.progress = x;
                    board.InvalidateSurface();
                },
                length: (uint)board.GetAnimationLength(),
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
            board.InvalidateSurface();
        }
    }

    private double GetAnimationLength() =>
        Math.Max(
            popCell is null ? 0 : PopLength,
            flashCells.Length > 0 ? (flashCells.Max(GetFlashDistance) * FlashStep) + FlashLength : 0);

    private int GetFlashDistance(SudokuCell cell) =>
        Math.Max(Math.Abs(cell.Row - flashOrigin.Row), Math.Abs(cell.Column - flashOrigin.Column));

    private double GetFlashProgress(SudokuCell cell, double elapsed)
    {
        var result = 0d;
        if (flashCells.Contains(cell))
        {
            result = Math.Clamp((elapsed - (GetFlashDistance(cell) * FlashStep)) / FlashLength, 0, 1);
        }

        return result;
    }

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
            case SKTouchAction.Moved:
                if ((FindCell(e.Location) is { } cell) && ((e.ActionType == SKTouchAction.Pressed) || (cell != touchedCell)))
                {
                    touchedCell = cell;
                    if (TapCommand?.CanExecute(cell) ?? false)
                    {
                        TapCommand.Execute(cell);
                    }
                }

                break;
            case SKTouchAction.Released:
            case SKTouchAction.Cancelled:
            case SKTouchAction.Exited:
                touchedCell = null;
                break;
        }

        e.Handled = true;
    }

    private SudokuCell? FindCell(SKPoint point)
    {
        var density = Width > 0 ? (float)(CanvasSize.Width / Width) : 1f;
        var layout = new BoardLayout(Math.Min(CanvasSize.Width, CanvasSize.Height), density);
        return layout.Find(point);
    }
}
