namespace Template.MobileApp.Domain.Logic;

using System.Buffers.Binary;

public static class SuicaLogic
{
    private static readonly Dictionary<byte, string> TerminalNames = new()
    {
        { 3, "精算機" },
        { 4, "携帯型端末" },
        { 5, "車載端末" },
        { 7, "券売機" },
        { 8, "券売機" },
        { 9, "入金機" },
        { 18, "券売機" },
        { 20, "券売機等" },
        { 21, "券売機等" },
        { 22, "改札機" },
        { 23, "簡易改札機" },
        { 24, "窓口端末" },
        { 25, "窓口端末" },
        { 26, "改札端末" },
        { 27, "携帯電話" },
        { 28, "乗継精算機" },
        { 29, "連絡改札機" },
        { 31, "簡易入金機" },
        { 199, "物販端末" },
        { 200, "自販機" }
    };

    private static readonly Dictionary<byte, string> ProcessNames = new()
    {
        { 1, "運賃支払" },
        { 2, "チャージ" },
        { 3, "磁気券購入" },
        { 4, "精算" },
        { 5, "入場精算" },
        { 6, "改札窓口処理" },
        { 7, "新規発行" },
        { 8, "窓口控除" },
        { 13, "バス(PiTaPa系)" },
        { 15, "バス(IruCa系)" },
        { 17, "再発行処理" },
        { 19, "新幹線利用" },
        { 20, "入場時AC" },
        { 21, "出場時AC" },
        { 31, "バスチャージ" },
        { 35, "バス路面電車企画券購入" },
        { 70, "物販" },
        { 72, "特典チャージ" },
        { 73, "レジ入金" },
        { 74, "物販取消" },
        { 75, "入場物販" }
    };

    private static readonly HashSet<byte> ProcessOfSales = [70, 72, 73, 74, 75];

    private static readonly HashSet<byte> ProcessOfBus = [13, 15, 31, 35];

    private static readonly Dictionary<int, string> RegionNames = new()
    {
        { 0, "首都圏" },
        { 1, "中部圏" },
        { 2, "近畿圏" },
        { 3, "その他" }
    };

    public static string ConvertTerminalString(byte type) =>
        TerminalNames.TryGetValue(type, out var value) ? value : type.ToString("X");

    public static string ConvertProcessString(byte process)
    {
        var processType = ConvertProcessType(process);
        var withCache = (process & 0b10000000) != 0;

        var name = ProcessNames.TryGetValue(processType, out var value) ? value : processType.ToString("X");

        return withCache ? name + " 現金併用" : name;
    }

    public static byte ConvertProcessType(byte process) =>
        (byte)(process & 0b01111111);

    public static bool IsProcessOfSales(byte process)
    {
        var processType = ConvertProcessType(process);
        return ProcessOfSales.Contains(processType);
    }

    public static bool IsProcessOfBus(byte process)
    {
        var processType = ConvertProcessType(process);
        return ProcessOfBus.Contains(processType);
    }

    public static string ConvertRegionString(int region) =>
        RegionNames.TryGetValue(region, out var value) ? value : region.ToString("X");

    private static DateTime ExtractDate(Span<byte> bytes)
    {
        var year = 2000 + (bytes[0] >> 1);
        var month = BinaryPrimitives.ReadUInt16BigEndian(bytes[..2]) >> 5 & 0b1111;
        var day = bytes[1] & 0b11111;
        return new DateTime(year, month, day);
    }

    private static DateTime ExtractDateTime(Span<byte> bytes)
    {
        var year = 2000 + (bytes[0] >> 1);
        var month = BinaryPrimitives.ReadUInt16BigEndian(bytes[..2]) >> 5 & 0b1111;
        var day = bytes[1] & 0b11111;
        var hour = bytes[2] >> 3;
        var minute = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2, 2)) >> 5 & 0b111111;
        return new DateTime(year, month, day, hour, minute, 0);
    }

    public static int ExtractAccessBalance(Span<byte> bytes) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(11, 2));

    public static int ExtractAccessTransactionId(Span<byte> bytes) =>
        BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(14, 2));

    public static bool IsValidLog(Span<byte> bytes) =>
        bytes[1] != 0x00;

    public static byte ExtractLogTerminal(Span<byte> bytes) =>
        bytes[0];

    public static byte ExtractLogProcess(Span<byte> bytes) =>
        bytes[1];

    public static DateTime ExtractLogDateTime(Span<byte> bytes) =>
        IsProcessOfSales(ExtractLogProcess(bytes)) ? ExtractDateTime(bytes[4..]) : ExtractDate(bytes[4..]);

    public static int ExtractLogBalance(Span<byte> bytes) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(10, 2));

    public static int ExtractLogTransactionId(Span<byte> bytes) =>
        BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(13, 2));
}
