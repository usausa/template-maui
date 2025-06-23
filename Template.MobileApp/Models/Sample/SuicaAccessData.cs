namespace Template.MobileApp.Models.Sample;

public partial class SuicaAccessData : ObservableObject
{
    [ObservableProperty]
    public partial int Balance { get; set; }

    [ObservableProperty]
    public partial int TransactionId { get; set; }
}
