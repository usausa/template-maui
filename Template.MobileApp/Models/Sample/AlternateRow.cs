namespace Template.MobileApp.Models.Sample;

public interface IAlternateRow
{
    public bool IsEven { get; }
}

public partial class AlternateRow<T> : ObservableObject, IAlternateRow
{
    public T Value { get; }

    [ObservableProperty]
    public partial bool IsEven { get; set; }

    public AlternateRow(T value)
    {
        Value = value;
    }
}
