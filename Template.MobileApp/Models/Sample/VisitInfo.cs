namespace Template.MobileApp.Models.Sample;

public enum VisitStatus
{
    Planned,
    Visited,
    Revisit,
    Absent
}

public enum VisitCategory
{
    Regular,
    New,
    Inspection,
    Collection
}

// 訪問先 1 件
public sealed record VisitInfo(
    string Code,
    string Name,
    string Address,
    string Staff,
    DateTime ScheduledAt,
    VisitStatus Status,
    VisitCategory Category,
    bool IsPriority,
    string Phone,
    DateTime? LastVisitedAt,
    string Note);

#pragma warning disable CA5394
public static class VisitSamples
{
    private static readonly string[] Names =
    [
        "青葉 太郎", "白樺 花子", "海風 次郎", "山吹 美咲", "紅葉 健一", "若草 さくら", "朝日 大輔", "銀河 由美", "常磐 修", "港 明日香",
        "北斗 誠", "桜井 恵", "大地 拓也", "星野 千尋", "光陽 直樹", "森川 陽子", "石橋 隆", "小野寺 舞", "長谷川 翔", "藤原 玲"
    ];

    private static readonly string[] Towns = ["中央", "旭町", "港南", "緑ヶ丘", "栄", "本町", "泉", "若葉台"];

    private static readonly string[] Staffs = ["佐藤", "鈴木", "高橋", "田中"];

    private static readonly VisitCategory[] Categories = [VisitCategory.Regular, VisitCategory.New, VisitCategory.Inspection, VisitCategory.Collection];

    private static readonly string[] Notes =
    [
        "午前中の訪問を希望", "インターホンが故障中。ノック", "駐車場は裏手", "担当者変更の連絡あり", "再訪問時は資料を持参", "犬に注意", string.Empty, string.Empty
    ];

    public static VisitInfo[] Create(int count, int seed = 20260913)
    {
        var random = new Random(seed);
        var today = DateTime.Today;
        var visits = new VisitInfo[count];
        for (var i = 0; i < count; i++)
        {
            var status = random.Next(100) switch
            {
                < 45 => VisitStatus.Planned,
                < 70 => VisitStatus.Visited,
                < 85 => VisitStatus.Revisit,
                _ => VisitStatus.Absent
            };
            var scheduled = today.AddDays(random.Next(0, 5)).AddHours(9 + random.Next(0, 8)).AddMinutes(random.Next(0, 2) * 30);
            visits[i] = new VisitInfo(
                $"{(i / 5) + 1:D3}-{(i % 5) + 1:D3}",
                Names[i % Names.Length],
                $"{Towns[random.Next(Towns.Length)]}{random.Next(1, 6)}-{random.Next(1, 30)}-{random.Next(1, 20)}",
                Staffs[random.Next(Staffs.Length)],
                scheduled,
                status,
                Categories[random.Next(Categories.Length)],
                random.Next(4) == 0,
                $"0{random.Next(3, 10)}0-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}",
                status == VisitStatus.Planned && random.Next(2) == 0 ? null : today.AddDays(-random.Next(7, 120)),
                Notes[random.Next(Notes.Length)]);
        }

        return visits;
    }
}
#pragma warning restore CA5394
