namespace Template.MobileApp.Models.Sample;

public partial class SuicaLogData : ObservableObject
{
    [ObservableProperty]
    public partial byte Terminal { get; set; }

    [ObservableProperty]
    public partial byte Process { get; set; }

    [ObservableProperty]
    public partial DateTime DateTime { get; set; }

    [ObservableProperty]
    public partial int Balance { get; set; }

    [ObservableProperty]
    public partial int TransactionId { get; set; }
}
