namespace Template.MobileApp.Models.Sample;

using System.Security.Cryptography;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum MoneyShopKind
{
    Cafe,
    Restaurant,
    Grocery,
    Convenience,
    Fashion,
    Train,
    Taxi,
    Electricity,
    Water,
    Mobile,
    Cinema,
    Book,
    Pharmacy,
    Charge
}

public enum MoneyCategory
{
    Food,
    Shopping,
    Transport,
    Utility,
    Entertainment,
    Health,
    Charge
}

public enum MoneyNoticeKind
{
    Charge,
    Point,
    Campaign,
    Statement,
    Security
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed record MoneyTransaction(DateTime Date, string Shop, MoneyShopKind Kind, int Amount)
{
    public MoneyCategory Category => MoneyBook.CategoryOf(Kind);
}

// 日ごとの取引 (一覧の日付の区切りにそのまま使う)
public sealed class MoneyDay : ReadOnlyCollection<MoneyTransaction>
{
    public DateTime Date { get; }

    public MoneyDay(DateTime date, IList<MoneyTransaction> items)
        : base(items)
    {
        Date = date;
    }
}

public sealed record MoneyMonth(DateTime Month, int Total);

public sealed record MoneyCategoryTotal(MoneyCategory Category, int Amount, double Ratio);

public sealed record MoneyBalance(DateTime Date, int Amount);

public sealed partial class MoneyNotice : ObservableObject
{
    public required DateTime Date { get; init; }

    public required MoneyNoticeKind Kind { get; init; }

    public required string Title { get; init; }

    public required string Body { get; init; }

    [ObservableProperty]
    public partial bool IsUnread { get; set; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

// 決済アプリの取引と集計 (残高の推移・月の支出・分類・検索)
public sealed class MoneyBook
{
    private readonly DateTime now;

    // 新しい順
    private readonly List<MoneyTransaction> transactions = [];

    // 日ごとの終わりの残高 (古い順)
    private readonly List<MoneyBalance> balances = [];

    public int Balance { get; }

    public IReadOnlyList<MoneyTransaction> Transactions => transactions;

    public MoneyBook(int startBalance, IEnumerable<MoneyTransaction> transactions, DateTime now)
    {
        this.now = now;

        var list = transactions.OrderBy(static x => x.Date).ToList();
        var balance = startBalance;
        var index = 0;
        for (var date = list.Count > 0 ? list[0].Date.Date : now.Date; date <= now.Date; date = date.AddDays(1))
        {
            while ((index < list.Count) && (list[index].Date.Date == date))
            {
                balance += list[index].Amount;
                index++;
            }

            balances.Add(new MoneyBalance(date, balance));
        }

        list.Reverse();
        this.transactions.AddRange(list);
        Balance = balance;
    }

    public static MoneyCategory CategoryOf(MoneyShopKind kind) => kind switch
    {
        MoneyShopKind.Cafe or MoneyShopKind.Restaurant or MoneyShopKind.Grocery => MoneyCategory.Food,
        MoneyShopKind.Convenience or MoneyShopKind.Fashion => MoneyCategory.Shopping,
        MoneyShopKind.Train or MoneyShopKind.Taxi => MoneyCategory.Transport,
        MoneyShopKind.Electricity or MoneyShopKind.Water or MoneyShopKind.Mobile => MoneyCategory.Utility,
        MoneyShopKind.Cinema or MoneyShopKind.Book => MoneyCategory.Entertainment,
        MoneyShopKind.Pharmacy => MoneyCategory.Health,
        _ => MoneyCategory.Charge
    };

    //--------------------------------------------------------------------------------
    // Balance
    //--------------------------------------------------------------------------------

    // 今月と前月の 1 日あたりの平均残高の比 (0.024 = +2.4%)
    public double BalanceRate()
    {
        var month = new DateTime(now.Year, now.Month, 1);
        var current = balances.Where(x => x.Date >= month).Select(static x => (double)x.Amount).DefaultIfEmpty(0).Average();
        var previous = balances.Where(x => (x.Date >= month.AddMonths(-1)) && (x.Date < month)).Select(static x => (double)x.Amount).DefaultIfEmpty(0).Average();
        return previous > 0 ? (current - previous) / previous : 0;
    }

    // 最近の日ごとの終わりの残高 (古い順)
    public IReadOnlyList<MoneyBalance> BalanceHistory(int days) => balances.TakeLast(days).ToList();

    //--------------------------------------------------------------------------------
    // Expense
    //--------------------------------------------------------------------------------

    // 新しい順の月 (初日) と支出の合計
    public IReadOnlyList<MoneyMonth> Months(int count)
    {
        var month = new DateTime(now.Year, now.Month, 1);
        return Enumerable.Range(0, count).Select(x => month.AddMonths(-x)).Select(x => new MoneyMonth(x, MonthTotal(x))).ToList();
    }

    public int MonthTotal(DateTime month) => -Payments(month).Sum(static x => x.Amount);

    // 1 週間あたりの支出 (今月は今日までの日数で割る)
    public int WeeklyAverage(DateTime month)
    {
        var days = (month.Year == now.Year) && (month.Month == now.Month) ? now.Day : DateTime.DaysInMonth(month.Year, month.Month);
        return (int)Math.Round(MonthTotal(month) * 7d / days);
    }

    // 1〜7 日、8〜14 日 ... の週ごとの支出
    public IReadOnlyList<int> WeeklyTotals(DateTime month)
    {
        var weeks = (DateTime.DaysInMonth(month.Year, month.Month) + 6) / 7;
        var totals = new int[weeks];
        foreach (var transaction in Payments(month))
        {
            totals[(transaction.Date.Day - 1) / 7] -= transaction.Amount;
        }
        return totals;
    }

    // 分類ごとの支出 (多い順)
    public IReadOnlyList<MoneyCategoryTotal> CategoryTotals(DateTime month)
    {
        var total = (double)Math.Max(1, MonthTotal(month));
        return Payments(month)
            .GroupBy(static x => x.Category)
            .Select(x => new MoneyCategoryTotal(x.Key, -x.Sum(static y => y.Amount), -x.Sum(static y => y.Amount) / total))
            .OrderByDescending(static x => x.Amount)
            .ToList();
    }

    private IEnumerable<MoneyTransaction> Payments(DateTime month) =>
        transactions.Where(x => (x.Amount < 0) && (x.Date.Year == month.Year) && (x.Date.Month == month.Month));

    //--------------------------------------------------------------------------------
    // Transaction
    //--------------------------------------------------------------------------------

    public IReadOnlyList<MoneyDay> Recent(int count) => GroupByDay(transactions.Take(count));

    // 店の名前の一部で探す (空なら最近のもの)
    public IReadOnlyList<MoneyDay> Search(string keyword, int count)
    {
        var text = keyword.Trim();
        return GroupByDay(transactions.Where(x => (text.Length == 0) || x.Shop.Contains(text, StringComparison.OrdinalIgnoreCase)).Take(count));
    }

    private static List<MoneyDay> GroupByDay(IEnumerable<MoneyTransaction> source) =>
        source.GroupBy(static x => x.Date.Date)
            .Select(static x => new MoneyDay(x.Key, x.ToList()))
            .ToList();

    //--------------------------------------------------------------------------------
    // Notice
    //--------------------------------------------------------------------------------

    //--------------------------------------------------------------------------------
    // Sample
    //--------------------------------------------------------------------------------

    // 曜日と日付で決まる 1 日の支払い。金額は日ごとの値でずらす
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// 決済の見本 (半年前から今日までの取引。残高が下回るとオートチャージする)
public static class MoneySample
{
    private const int Days = 186;

    private const int StartBalance = 18000;

    private const int ChargeThreshold = 8000;

    private const int ChargeAmount = 20000;

    private static readonly string[] Lunches = ["定食 さくら", "うさぎ弁当", "麺処 ひだまり", "サンドイッチ ハル"];

    public static int LoadStartBalance() => StartBalance;

    public static IReadOnlyList<MoneyTransaction> LoadTransactions(DateTime now)
    {
        var transactions = new List<MoneyTransaction>();
        var today = now.Date;
        var balance = StartBalance;
        for (var d = Days; d >= 0; d--)
        {
            var date = today.AddDays(-d);
            foreach (var (time, shop, kind, amount) in DayPlan(date).OrderBy(static x => x.Time))
            {
                var at = date + time;
                if (at > now)
                {
                    continue;
                }

                if ((amount < 0) && (balance + amount < ChargeThreshold))
                {
                    transactions.Add(new MoneyTransaction(at.AddMinutes(-1), "オートチャージ", MoneyShopKind.Charge, ChargeAmount));
                    balance += ChargeAmount;
                }

                transactions.Add(new MoneyTransaction(at, shop, kind, amount));
                balance += amount;
            }
        }

        return transactions;
    }

    public static IReadOnlyList<MoneyNotice> LoadNotices(MoneyBook book, DateTime now)
    {
        var charge = book.Transactions.FirstOrDefault(static x => x.Kind == MoneyShopKind.Charge);
        var today = now.Date;
        var statement = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        return
        [
            new MoneyNotice
            {
                Date = charge?.Date ?? today,
                Kind = MoneyNoticeKind.Charge,
                Title = "オートチャージしました",
                Body = $"残高が {ChargeThreshold:N0} 円を下回ったため、{ChargeAmount:N0} 円をチャージしました。",
                IsUnread = true
            },
            new MoneyNotice
            {
                Date = today.AddHours(9),
                Kind = MoneyNoticeKind.Campaign,
                Title = "週末はポイント 5 倍",
                Body = "土日のお支払いでポイントが 5 倍になります。エントリーは不要です。",
                IsUnread = true
            },
            new MoneyNotice
            {
                Date = today.AddDays(-1).AddHours(10),
                Kind = MoneyNoticeKind.Security,
                Title = "本人確認のお願い",
                Body = "送金と出金を使うには本人確認が必要です。アカウントから手続きできます。",
                IsUnread = true
            },
            new MoneyNotice
            {
                Date = today.AddDays(-3).AddHours(12),
                Kind = MoneyNoticeKind.Point,
                Title = "ポイントを付与しました",
                Body = "先週のお支払いで 128 pt を付与しました。"
            },
            new MoneyNotice
            {
                Date = new DateTime(now.Year, now.Month, 1).AddHours(8),
                Kind = MoneyNoticeKind.Statement,
                Title = $"{statement.Month}月の利用明細",
                Body = $"{statement.Month}月のお支払いは {book.MonthTotal(statement):N0} 円でした。"
            },
            new MoneyNotice
            {
                Date = today.AddDays(-9).AddHours(18),
                Kind = MoneyNoticeKind.Campaign,
                Title = "カフェでのお支払いで 10% 還元",
                Body = "対象のカフェで使うと、お支払いの 10% をポイントで還元します。"
            },
            new MoneyNotice
            {
                Date = today.AddDays(-12).AddHours(11),
                Kind = MoneyNoticeKind.Security,
                Title = "新しい端末でログインしました",
                Body = "心当たりが無いときは、パスワードを変更してください。"
            },
            new MoneyNotice
            {
                Date = new DateTime(now.Year, now.Month, 1).AddMonths(-1).AddHours(8),
                Kind = MoneyNoticeKind.Statement,
                Title = $"{statement.AddMonths(-1).Month}月の利用明細",
                Body = $"{statement.AddMonths(-1).Month}月のお支払いは {book.MonthTotal(statement.AddMonths(-1)):N0} 円でした。"
            }
        ];
    }

    // 支払いのコード (4 桁ずつ区切った 20 桁)
    public static string LoadPaymentCode() =>
        String.Join(' ', Enumerable.Range(0, 5).Select(static _ => RandomNumberGenerator.GetInt32(10000).ToString("D4", CultureInfo.InvariantCulture)));

    private static IEnumerable<(TimeSpan Time, string Shop, MoneyShopKind Kind, int Amount)> DayPlan(DateTime date)
    {
        var seed = date.DayOfYear + (date.Year * 7);
        if (date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
        {
            if (date.DayOfWeek == DayOfWeek.Monday)
            {
                yield return (new TimeSpan(7, 40, 0), "うさぎ鉄道", MoneyShopKind.Train, -3000);
            }
            yield return (new TimeSpan(8, 12, 0), "カフェ モカ", MoneyShopKind.Cafe, -(420 + ((seed % 4) * 40)));
            yield return (new TimeSpan(12, 18, 0), Lunches[seed % Lunches.Length], MoneyShopKind.Restaurant, -(780 + ((seed % 5) * 80)));
            if (seed % 3 == 0)
            {
                yield return (new TimeSpan(19, 5, 0), "うさぎマート", MoneyShopKind.Convenience, -(320 + ((seed % 7) * 90)));
            }
        }
        else
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                yield return (new TimeSpan(10, 30, 0), "フードマート", MoneyShopKind.Grocery, -(3800 + ((seed % 6) * 350)));
            }
            else if (seed % 2 == 0)
            {
                yield return (new TimeSpan(14, 0, 0), "シネマ うさぎ", MoneyShopKind.Cinema, -1900);
            }
            else
            {
                yield return (new TimeSpan(15, 20, 0), "ブックス 青空", MoneyShopKind.Book, -(1200 + ((seed % 5) * 300)));
            }
            if (seed % 4 == 1)
            {
                yield return (new TimeSpan(16, 10, 0), "ファッション ルナ", MoneyShopKind.Fashion, -(3900 + ((seed % 5) * 1000)));
            }
            if ((date.DayOfWeek == DayOfWeek.Saturday) && (seed % 3 == 0))
            {
                yield return (new TimeSpan(21, 40, 0), "うさぎタクシー", MoneyShopKind.Taxi, -(1500 + ((seed % 4) * 300)));
            }
        }

        switch (date.Day)
        {
            case 10:
                yield return (new TimeSpan(18, 30, 0), "あおば薬局", MoneyShopKind.Pharmacy, -(980 + ((seed % 4) * 420)));
                break;
            case 20:
                yield return (new TimeSpan(9, 0, 0), "うさモバイル", MoneyShopKind.Mobile, -4980);
                break;
            case 25:
                yield return (new TimeSpan(9, 0, 0), "うさぎ電力", MoneyShopKind.Electricity, -(6200 + ((seed % 5) * 300)));
                break;
            case 27:
                yield return (new TimeSpan(9, 0, 0), "うさぎ水道", MoneyShopKind.Water, -3240);
                break;
        }
    }
}
