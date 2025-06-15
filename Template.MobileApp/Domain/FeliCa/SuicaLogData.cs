namespace Template.MobileApp.Domain.FeliCa;

public class SuicaLogData
{
    public byte Terminal { get; set; }

    public byte Process { get; set; }

    public DateTime DateTime { get; set; }

    public int Balance { get; set; }

    public int TransactionId { get; set; }
}
