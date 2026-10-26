namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum StockSide
{
    Sell,
    Buy
}

public enum StockTimeframe
{
    Minute1,
    Minute5,
    Daily,
    Weekly,
    Monthly
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record StockCandle(double Open, double High, double Low, double Close, double Volume);

public sealed partial class StockQuote : ObservableObject
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required string Market { get; init; }

    public required string Session { get; init; }

    public required double Previous { get; init; }

    [ObservableProperty(NotifyAlso = [nameof(IsUp), nameof(Change), nameof(ChangeRate)])]
    public partial double Price { get; set; }

    [ObservableProperty]
    public partial double Open { get; set; }

    [ObservableProperty]
    public partial double High { get; set; }

    [ObservableProperty]
    public partial double Low { get; set; }

    [ObservableProperty]
    public partial long Volume { get; set; }

    [ObservableProperty]
    public partial DateTime Updated { get; set; }

    public bool IsUp => Price >= Previous;

    public double Change => Price - Previous;

    public double ChangeRate => Previous > 0 ? (Price - Previous) / Previous : 0;
}

public sealed partial class StockDepth : ObservableObject
{
    public required double Price { get; init; }

    public required bool IsAsk { get; init; }

    [ObservableProperty]
    public partial int Quantity { get; set; }

    [ObservableProperty]
    public partial double Rate { get; set; }
}

public sealed record StockTick(DateTime Time, double Price, int Quantity, StockSide Side);

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

#pragma warning disable CA5394
public sealed partial class StockBoard : ObservableObject
{
    private const int DepthRows = 5;

    private const int TickLimit = 40;

    private readonly Random random;

    private int step;

    public required StockQuote Quote { get; init; }

    // Price step of the board, also the unit of a price move
    public required double TickSize { get; init; }

    // Number of updates that make up one candle, the timeframe selection changes it
    public int CandleTicks { get; set; } = 6;

    // True when the last update closed a candle and started a new one
    public bool CandleAdded { get; private set; }

    public ObservableCollection<StockDepth> Asks { get; } = [];

    public ObservableCollection<StockDepth> Bids { get; } = [];

    public ObservableCollection<StockTick> Ticks { get; } = [];

    public Collection<StockCandle> Candles { get; } = [];

    [ObservableProperty]
    public partial int AskTotal { get; set; }

    [ObservableProperty]
    public partial int BidTotal { get; set; }

    public StockBoard(int seed)
    {
        random = new Random(seed);
    }

    //--------------------------------------------------------------------------------
    // Update
    //--------------------------------------------------------------------------------

    public void Update(DateTime now)
    {
        MovePrice(now);
        UpdateDepth();
        AddTick();
        UpdateCandle();
    }

    private void MovePrice(DateTime now)
    {
        var price = Quote.Price + ((random.Next(5) - 2) * TickSize);
        Quote.Price = Math.Round(price / TickSize) * TickSize;
        Quote.High = Math.Max(Quote.High, Quote.Price);
        Quote.Low = Math.Min(Quote.Low, Quote.Price);
        Quote.Volume += random.Next(4000, 24000);
        Quote.Updated = now;
    }

    private void UpdateDepth()
    {
        BuildSide(Asks, true);
        BuildSide(Bids, false);
        UpdateDepthRate();

        AskTotal = Asks.Sum(static x => x.Quantity) + random.Next(40000, 60000);
        BidTotal = Bids.Sum(static x => x.Quantity) + random.Next(40000, 60000);
    }

    // The board is rebuilt around the current price, the rows closest to it come last
    private void BuildSide(ObservableCollection<StockDepth> side, bool ask)
    {
        side.Clear();
        for (var i = 0; i < DepthRows; i++)
        {
            var distance = ask ? DepthRows - i : i + 1;
            side.Add(new StockDepth
            {
                Price = Quote.Price + (ask ? distance * TickSize : -distance * TickSize),
                IsAsk = ask,
                Quantity = random.Next(30, 280) * 100
            });
        }
    }

    private void UpdateDepthRate()
    {
        var max = Math.Max(
            Asks.Count > 0 ? Asks.Max(static x => x.Quantity) : 0,
            Bids.Count > 0 ? Bids.Max(static x => x.Quantity) : 0);
        if (max == 0)
        {
            return;
        }

        foreach (var depth in Asks.Concat(Bids))
        {
            depth.Rate = (double)depth.Quantity / max;
        }
    }

    private void AddTick()
    {
        Ticks.Insert(0, new StockTick(
            Quote.Updated,
            Quote.Price,
            random.Next(1, 40) * 100,
            random.Next(2) == 0 ? StockSide.Buy : StockSide.Sell));
        while (Ticks.Count > TickLimit)
        {
            Ticks.RemoveAt(Ticks.Count - 1);
        }
    }

    private void UpdateCandle()
    {
        CandleAdded = false;
        if (Candles.Count == 0)
        {
            Candles.Add(new StockCandle(Quote.Price, Quote.Price, Quote.Price, Quote.Price, 0));
            CandleAdded = true;
            return;
        }

        step++;
        if (step >= CandleTicks)
        {
            step = 0;
            Candles.Add(new StockCandle(Quote.Price, Quote.Price, Quote.Price, Quote.Price, random.Next(20, 60) * 1000));
            CandleAdded = true;
            return;
        }

        var last = Candles[^1];
        Candles[^1] = last with
        {
            High = Math.Max(last.High, Quote.Price),
            Low = Math.Min(last.Low, Quote.Price),
            Close = Quote.Price,
            Volume = last.Volume + (random.Next(20, 60) * 1000)
        };
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class StockSample
{
    private const double TickSize = 0.5;

    private const double Previous = 3423.5;

    private const int Seed = 20261025;

    private const int InitialCandles = 42;

    private const int PrimeCount = 8;

    public static StockBoard LoadBoard(DateTime now)
    {
        var board = new StockBoard(Seed)
        {
            TickSize = TickSize,
            Quote = new StockQuote
            {
                Code = "9999",
                Name = "うさうさ自動車",
                Market = "うさうさ取引所",
                Session = "前場",
                Previous = Previous,
                Price = Previous,
                Open = 3431.0,
                High = 3431.0,
                Low = 3431.0,
                Volume = 12_480_000,
                Updated = now
            }
        };

        BuildCandles(board);
        board.Quote.Price = board.Candles[^1].Close;
        board.Quote.High = board.Candles.Max(static x => x.High);
        board.Quote.Low = board.Candles.Min(static x => x.Low);

        for (var i = PrimeCount; i >= 0; i--)
        {
            board.Update(now.AddSeconds(-i));
        }

        return board;
    }

    private static void BuildCandles(StockBoard board)
    {
        var random = new Random(Seed);
        var price = board.Quote.Open;
        for (var i = 0; i < InitialCandles; i++)
        {
            var open = price;
            var close = open + ((random.Next(7) - 3 + (i > 20 ? 1 : 0)) * TickSize);
            var high = Math.Max(open, close) + (random.Next(3) * TickSize);
            var low = Math.Min(open, close) - (random.Next(3) * TickSize);
            board.Candles.Add(new StockCandle(open, high, low, close, random.Next(120, 340) * 1000));
            price = close;
        }
    }
}
#pragma warning restore CA5394
