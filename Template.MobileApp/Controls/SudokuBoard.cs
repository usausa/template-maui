namespace Template.MobileApp.Controls;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using Template.MobileApp.Models.App;

// 数独の盤面 (正方形)。触れたマスをコマンドで渡し、選んだマスと同じ行・列・ブロック、同じ数字、重なった数字を塗り分ける。
// 数字を入れたマスを弾ませ (答えと違えば揺らし)、そろった行・列・ブロック (完成は盤面全体) を入れたマスから広がるように光らせる
public sealed class SudokuBoard : SKCanvasView
{
    private const string AnimationName = "SudokuBoard";

    // 弾ませる時間、光を 1 マス広げる遅れ、1 マスが光る時間 (ms)
    private const double PopLength = 280;
    private const double FlashStep = 55;
    private const double FlashLength = 480;

    private static readonly SKTypeface GivenTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Bold);
    private static readonly SKTypeface InputTypeface = SKTypeface.FromFamilyName("sans-serif", SKFontStyle.Normal);

    // マスの塗り (選んだマスと同じ行・列・ブロック、同じ数字、重なった数字、選んだマス)
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

    // そろったときと、完成したときの光
    private static readonly SKColor FlashColor = SKColor.Parse("#818CF8");
    private static readonly SKColor CompleteColor = SKColor.Parse("#FBBF24");

    public static readonly BindableProperty BoardProperty = BindableProperty.Create(
        nameof(Board),
        typeof(SudokuFrame),
        typeof(SudokuBoard),
        propertyChanged: OnBoardChanged);

    public SudokuFrame? Board
    {
        get => (SudokuFrame?)GetValue(BoardProperty);
        set => SetValue(BoardProperty, value);
    }

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(SudokuBoard));

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    // 動かしている効果の進み (1 は止まっている)。選ぶだけの盤面では止めずに続ける
    private double progress = 1;

    private SudokuCell? popCell;

    private IReadOnlyList<SudokuCell> flashCells = [];

    private SudokuCell flashOrigin;

    private bool completed;

    // 触れているマス (なぞって選び直す)
    private SudokuCell? touchedCell;

    public SudokuBoard()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
    }

    private static void OnBoardChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        var board = (SudokuBoard)bindable;
        if ((newValue is SudokuFrame frame) && ((frame.Placed is not null) || (frame.Flash.Count > 0)))
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
            flashCells.Count > 0 ? (flashCells.Max(GetFlashDistance) * FlashStep) + FlashLength : 0);

    private int GetFlashDistance(SudokuCell cell) =>
        Math.Max(Math.Abs(cell.Row - flashOrigin.Row), Math.Abs(cell.Column - flashOrigin.Column));

    // 幅と高さの短い方の正方形にする
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var size = Math.Min(widthConstraint, heightConstraint);
        return new Size(size, size);
    }

    //--------------------------------------------------------------------------------
    // Touch
    //--------------------------------------------------------------------------------

    // 触れたマスを選び、なぞって別のマスに移ったら選び直す
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
        using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3 * density);
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = ThickLineColor.WithAlpha(60);
        paint.MaskFilter = blur;
        var rect = layout.Board;
        rect.Offset(0, 2 * density);
        canvas.DrawRoundRect(rect, layout.Radius, layout.Radius, paint);
    }

    // 選んだマス > 重なった数字 > 同じ数字 > 同じ行・列・ブロックの順に塗る
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

    // 入れたマスから近い順に、少しずつずらして光らせる
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
                    paint.Color = color.WithAlpha((byte)(150 * Math.Sin(Math.PI * local)));
                    canvas.DrawRect(layout.GetCell(cell), paint);
                }
            }
        }
    }

    // マスの細い線と、ブロックの太い線
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
            if (i % 3 != 0)
            {
                var offset = i * layout.CellSize;
                canvas.DrawLine(board.Left + offset, board.Top, board.Left + offset, board.Bottom, paint);
                canvas.DrawLine(board.Left, board.Top + offset, board.Right, board.Top + offset, paint);
            }
        }

        paint.Color = ThickLineColor;
        paint.StrokeWidth = 2 * density;
        for (var i = 3; i < SudokuGame.Size; i += 3)
        {
            var offset = i * layout.CellSize;
            canvas.DrawLine(board.Left + offset, board.Top, board.Left + offset, board.Bottom, paint);
            canvas.DrawLine(board.Left, board.Top + offset, board.Right, board.Top + offset, paint);
        }
    }

    // 数字 (問題は濃い色、入れた数字は明るい色、答えと違えば赤) と、空きのマスのメモ
    private void DrawNumbers(SKCanvas canvas, BoardLayout layout, SudokuFrame board, float density, double elapsed)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        using var givenFont = new SKFont(GivenTypeface, layout.CellSize * 0.6f);
        using var inputFont = new SKFont(InputTypeface, layout.CellSize * 0.6f);
        using var noteFont = new SKFont(InputTypeface, layout.CellSize * 0.26f);
        using var strongNoteFont = new SKFont(GivenTypeface, layout.CellSize * 0.26f);

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
                    var noteSize = (rect.Width - (rect.Width * 0.12f)) / 3;
                    for (var note = 1; note <= SudokuGame.Size; note++)
                    {
                        if (game.HasNote(cell, note))
                        {
                            var noteRow = (note - 1) / 3;
                            var noteColumn = (note - 1) % 3;
                            var x = rect.Left + (rect.Width * 0.06f) + (noteSize * (noteColumn + 0.5f));
                            var y = rect.Top + (rect.Width * 0.06f) + (noteSize * (noteRow + 0.5f));
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

    // 入れたマスは小さい大きさから弾ませ (答えと違えば横に揺らし)、光っているマスは少し膨らませる
    private void ApplyMotion(SKCanvas canvas, SudokuGame game, SudokuCell cell, SKRect rect, float density, double elapsed)
    {
        if (progress < 1)
        {
            var scale = 1f;
            if (cell == popCell)
            {
                var t = Math.Clamp(elapsed / PopLength, 0, 1);
                scale = 0.4f + (0.6f * EaseOutBack((float)t));
                if (game.IsWrong(cell))
                {
                    canvas.Translate((float)(Math.Sin(t * Math.PI * 6) * (1 - t) * 4 * density), 0);
                }
            }
            else
            {
                var local = GetFlashProgress(cell, elapsed);
                if (local is > 0 and < 1)
                {
                    scale = 1f + (0.2f * (float)Math.Sin(Math.PI * local));
                }
            }

            canvas.Scale(scale, scale, rect.MidX, rect.MidY);
        }
    }

    private double GetFlashProgress(SudokuCell cell, double elapsed)
    {
        var result = 0d;
        if (flashCells.Contains(cell))
        {
            result = Math.Clamp((elapsed - (GetFlashDistance(cell) * FlashStep)) / FlashLength, 0, 1);
        }

        return result;
    }

    // 盤面の外側の太い枠
    private static void DrawFrame(SKCanvas canvas, BoardLayout layout, float density)
    {
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2.5f * density;
        paint.Color = ThickLineColor;
        var rect = layout.Board;
        rect.Inflate(-paint.StrokeWidth / 2, -paint.StrokeWidth / 2);
        canvas.DrawRoundRect(rect, layout.Radius, layout.Radius, paint);
    }

    private static void DrawText(SKCanvas canvas, int value, float x, float y, SKFont font, SKPaint paint)
    {
        var baseline = y - ((font.Metrics.Ascent + font.Metrics.Descent) / 2);
        canvas.DrawText(value.ToString(CultureInfo.InvariantCulture), x, baseline, SKTextAlign.Center, font, paint);
    }

    private static bool IsPeer(SudokuCell cell, SudokuCell target) =>
        (cell.Row == target.Row) ||
        (cell.Column == target.Column) ||
        (((cell.Row / 3) == (target.Row / 3)) && ((cell.Column / 3) == (target.Column / 3)));

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + (c3 * MathF.Pow(t - 1, 3)) + (c1 * MathF.Pow(t - 1, 2));
    }

    // 盤面と各マスの位置 (影がはみ出さないように内側に寄せる)
    private readonly struct BoardLayout
    {
        public SKRect Board { get; }

        public float Radius { get; }

        public float CellSize { get; }

        public BoardLayout(float size, float density)
        {
            var inset = 8 * density;
            CellSize = (size - (inset * 2)) / SudokuGame.Size;
            Board = SKRect.Create(inset, inset, CellSize * SudokuGame.Size, CellSize * SudokuGame.Size);
            Radius = 12 * density;
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
}
