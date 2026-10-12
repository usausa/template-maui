namespace Template.MobileApp.Modules.UI;

using Template.MobileApp.Graphics.Drawing;

public enum MoneyPage
{
    Home,
    Search,
    Notifications,
    Account
}

public sealed partial class UIMoneyViewModel : AppViewModelBase
{
    private const int PaymentSeconds = 30;

    private const int BalanceDays = 30;

    private readonly IDispatcherTimer timer;

    private readonly DateTime now = DateTime.Now;

    private readonly MoneyBook book;

    [ObservableProperty]
    public partial MoneyPage Selected { get; set; }

    [ObservableProperty]
    public partial int NotificationCount { get; set; }

    [ObservableProperty]
    public partial bool HasAccountAlert { get; set; }

    [ObservableProperty]
    public partial double Balance { get; set; }

    // 1 日あたりの平均残高の前月比
    public double BalanceRate { get; }

    public bool IsBalanceUp => BalanceRate >= 0;

    public ChartDrawing BalanceChart { get; } = new()
    {
        LineColor = Color.FromArgb("#E91E63"),
        LineHighColor = Color.FromArgb("#E91E63"),
        ValueFormat = "{0:N0} 円"
    };

    public IReadOnlyList<Selectable<MoneyMonth>> Months { get; }

    public MoneyMonth CurrentMonth => Months[0].Item;

    [ObservableProperty]
    public partial MoneyMonth Month { get; set; }

    [ObservableProperty]
    public partial int WeeklyAverage { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<MoneyCategoryTotal> Categories { get; set; } = [];

    public ChartDrawing WeeklyChart { get; } = new()
    {
        ShowAxis = false,
        BarColor = Color.FromArgb("#2DD4BF"),
        BarEndColor = Color.FromArgb("#3B82F6"),
        ValueFormat = "{0:N0} 円"
    };

    public IReadOnlyList<MoneyDay> RecentDays { get; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    // 検索とお知らせの一覧は、そのタブを開いたときに作る
    [ObservableProperty]
    public partial IReadOnlyList<MoneyDay> SearchResults { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<MoneyNotice> Notices { get; set; } = [];

    [ObservableProperty]
    public partial string PaymentCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int PaymentRemain { get; set; }

    [ObservableProperty]
    public partial double PaymentProgress { get; set; }

    [ObservableProperty]
    public partial bool IsPaymentOpen { get; set; }

    [ObservableProperty]
    public partial bool IsBalanceOpen { get; set; }

    [ObservableProperty]
    public partial bool IsMonthOpen { get; set; }

    [ObservableProperty]
    public partial bool IsCategoryOpen { get; set; }

    public ICommand PageCommand { get; }

    public IObserveCommand PayCommand { get; }

    public IObserveCommand DetailCommand { get; }

    public IObserveCommand MonthListCommand { get; }

    public IObserveCommand SelectMonthCommand { get; }

    public IObserveCommand CategoryListCommand { get; }

    public IObserveCommand ReadNoticeCommand { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public UIMoneyViewModel(IDispatcher dispatcher)
    {
        book = new MoneyBook(MoneySample.LoadStartBalance(), MoneySample.LoadTransactions(now), now);

        Disposables.Add(BalanceChart);
        Disposables.Add(WeeklyChart);

        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        Disposables.Add(timer.TickAsObservable().Subscribe(_ => TickPayment()));

        Selected = MoneyPage.Home;
        NotificationCount = 128;
        HasAccountAlert = true;
        Balance = book.Balance;
        BalanceRate = book.BalanceRate();

        Months = book.Months(6).Select(static x => new Selectable<MoneyMonth>(x)).ToList();
        Month = Months[0].Item;
        RecentDays = book.Recent(5);

        PageCommand = MakeDelegateCommand<MoneyPage>(page => Selected = page);
        PayCommand = MakeDelegateCommand(OpenPayment);
        DetailCommand = MakeDelegateCommand(OpenBalance);
        MonthListCommand = MakeDelegateCommand(() => IsMonthOpen = true);
        SelectMonthCommand = MakeDelegateCommand<Selectable<MoneyMonth>>(SelectMonth);
        CategoryListCommand = MakeDelegateCommand(() => IsCategoryOpen = true);
        ReadNoticeCommand = MakeDelegateCommand<MoneyNotice>(x => x.IsUnread = false);

        SubscribeSelected(LoadPage);
        SubscribeSearchText(_ => Search());
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    public override Task OnNavigatedToAsync(INavigationContext context)
    {
        ShowMonth(Month);
        return Task.CompletedTask;
    }

    public override Task OnNavigatingFromAsync(INavigationContext context)
    {
        timer.Stop();
        return Task.CompletedTask;
    }

    protected override Task OnNotifyBackAsync()
    {
        if (IsPaymentOpen || IsBalanceOpen || IsMonthOpen || IsCategoryOpen)
        {
            IsPaymentOpen = false;
            IsBalanceOpen = false;
            IsMonthOpen = false;
            IsCategoryOpen = false;
            return Task.CompletedTask;
        }

        return Navigator.ForwardAsync(ViewId.UIMenu1);
    }

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Operation
    //--------------------------------------------------------------------------------

    private void OpenPayment()
    {
        RenewPaymentCode();
        IsPaymentOpen = true;
        timer.Start();
    }

    private void OpenBalance()
    {
        var history = book.BalanceHistory(BalanceDays);
        BalanceChart.ShowLine(
            history.Select(static x => (double)x.Amount).ToList(),
            history.Select(static x => x.Date.ToString("M/d", CultureInfo.CurrentCulture)).ToList());
        IsBalanceOpen = true;
    }

    private void SelectMonth(Selectable<MoneyMonth> month)
    {
        IsMonthOpen = false;
        ShowMonth(month.Item);
    }

    private void ShowMonth(MoneyMonth month)
    {
        foreach (var item in Months)
        {
            item.IsSelected = item.Item == month;
        }
        Month = month;
        WeeklyAverage = book.WeeklyAverage(month.Month);
        Categories = book.CategoryTotals(month.Month);

        var weeks = book.WeeklyTotals(month.Month);
        WeeklyChart.ShowBar(
            weeks.Select(static x => (double)x).ToList(),
            Enumerable.Range(1, weeks.Count).Select(static x => $"第{x}週").ToList());
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void LoadPage(MoneyPage page)
    {
        if ((page == MoneyPage.Search) && (SearchResults.Count == 0))
        {
            Search();
        }
        else if ((page == MoneyPage.Notifications) && (Notices.Count == 0))
        {
            Notices = MoneySample.LoadNotices(book, now);
        }
    }

    // 支払いのコードは残り時間が無くなると作り直す
    private void TickPayment()
    {
        if (IsPaymentOpen)
        {
            if (PaymentRemain <= 1)
            {
                RenewPaymentCode();
            }
            else
            {
                PaymentRemain--;
                PaymentProgress = (double)PaymentRemain / PaymentSeconds;
            }
        }
        else
        {
            timer.Stop();
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void Search() =>
        SearchResults = book.Search(SearchText, 50);

    private void RenewPaymentCode()
    {
        PaymentCode = MoneySample.LoadPaymentCode();
        PaymentRemain = PaymentSeconds;
        PaymentProgress = 1;
    }
}
