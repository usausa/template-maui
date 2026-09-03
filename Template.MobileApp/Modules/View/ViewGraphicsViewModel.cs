namespace Template.MobileApp.Modules.View;

using Template.MobileApp.Graphics.Drawing;

#pragma warning disable CA5394
public sealed partial class ViewGraphicsViewModel : AppViewModelBase
{
    private readonly Random random = new();

    public ShapeDrawing Drawing { get; } = new();

    public SketchDrawing Sketch { get; } = new();

    public PulseRingDrawing Pulse { get; } = new();

    public ProgressArcDrawing Countdown { get; } = new();

    [ObservableProperty]
    public partial int ShapeCount { get; set; }

    [ObservableProperty]
    public partial int SketchCount { get; set; }

    [ObservableProperty]
    public partial ImageSource? ExportedImage { get; set; }

    [ObservableProperty]
    public partial bool HasExport { get; set; }

    [ObservableProperty]
    public partial bool CountdownRunning { get; set; }

    [ObservableProperty]
    public partial bool CountdownDone { get; set; }

    public IObserveCommand AddLineCommand { get; }
    public IObserveCommand AddCircleCommand { get; }
    public IObserveCommand AddRectCommand { get; }
    public IObserveCommand ClearCommand { get; }
    public IObserveCommand UndoSketchCommand { get; }
    public IObserveCommand ClearSketchCommand { get; }
    public IObserveCommand ExportSketchCommand { get; }
    public IObserveCommand StartCountdownCommand { get; }

    public ViewGraphicsViewModel()
    {
        Drawing.Size = new SizeF(100, 100);
        Drawing.Shapes.Add(new Line { Color = Colors.Blue, Point1 = new PointF(10, 10), Point2 = new PointF(90, 90) });
        Drawing.Shapes.Add(new Line { Color = Colors.Blue, Point1 = new PointF(10, 90), Point2 = new PointF(90, 10) });
        Drawing.Shapes.Add(new Rectangle { Color = Colors.Red, Rect = new RectF(40, 40, 20, 20) });
        Drawing.Invalidate();
        ShapeCount = Drawing.Shapes.Count;

        AddLineCommand = MakeDelegateCommand(() => AddShape(new Line
        {
            Color = RandomColor(),
            Width = 2,
            Point1 = RandomPoint(),
            Point2 = RandomPoint()
        }));
        AddCircleCommand = MakeDelegateCommand(() => AddShape(new Circle
        {
            Color = RandomColor(),
            Center = RandomPoint(),
            Radius = (float)((random.NextDouble() * 15) + 5)
        }));
        AddRectCommand = MakeDelegateCommand(() => AddShape(new Rectangle
        {
            Color = RandomColor(),
            Rect = new RectF(RandomPoint(), new SizeF((float)((random.NextDouble() * 20) + 10), (float)((random.NextDouble() * 20) + 10)))
        }));
        ClearCommand = MakeDelegateCommand(() =>
        {
            Drawing.Shapes.Clear();
            Drawing.Invalidate();
            ShapeCount = 0;
        });

        // Sketch (IInteractiveDrawing + PNG エクスポート)
        Sketch.StrokesChanged += (_, _) => SketchCount = Sketch.StrokeCount;
        UndoSketchCommand = MakeDelegateCommand(Sketch.Undo);
        ClearSketchCommand = MakeDelegateCommand(Sketch.Clear);
        ExportSketchCommand = MakeDelegateCommand(() =>
        {
            using var stream = new MemoryStream();
            Sketch.ExportPng(stream, 420, 240);
            var bytes = stream.ToArray();
            ExportedImage = ImageSource.FromStream(() => new MemoryStream(bytes));
            HasExport = true;
        });

        // Countdown (完走時のみ通知される)
        StartCountdownCommand = MakeDelegateCommand(() =>
        {
            CountdownDone = false;
            if (Countdown.Start(5f, () =>
                {
                    CountdownRunning = false;
                    CountdownDone = true;
                }))
            {
                CountdownRunning = true;
            }
        });
    }

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        Pulse.Start();
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        Pulse.Stop();
        Countdown.Cancel();
        CountdownRunning = false;
        return Task.CompletedTask;
    }

    private void AddShape(IShape shape)
    {
        Drawing.Shapes.Add(shape);
        Drawing.Invalidate();
        ShapeCount = Drawing.Shapes.Count;
    }

    private PointF RandomPoint() => new((float)(random.NextDouble() * 100), (float)(random.NextDouble() * 100));

    private Color RandomColor() => Color.FromHsla(random.NextDouble(), 0.7d, 0.5d);

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.ViewMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
