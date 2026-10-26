namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum DriveItemKind
{
    Folder,
    Image,
    Document,
    Sheet,
    Slide,
    Pdf,
    Video,
    Archive,
    Code
}

public enum DriveSyncState
{
    Synced,
    Pending,
    Local
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class DriveEntry : ObservableObject
{
    public required string Name { get; init; }

    public required DriveItemKind Kind { get; init; }

    public required long Size { get; init; }

    public required DateTime Updated { get; init; }

    public required DriveSyncState Sync { get; init; }

    // Number of items inside, only meaningful for a folder
    public int Count { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}

public sealed record DriveCrumb(string Name);

public sealed record DriveUsage(long Images, long Documents, long Videos, long Others, long Capacity)
{
    public long Used => Images + Documents + Videos + Others;

    public double Ratio => Capacity > 0 ? (double)Used / Capacity : 0;
}

public sealed partial class DriveTransfer : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Current { get; set; }

    [ObservableProperty]
    public partial int Total { get; set; }

    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }
}

//--------------------------------------------------------------------------------
// Service
//--------------------------------------------------------------------------------

public sealed class CloudDrive
{
    public required IReadOnlyList<DriveCrumb> Crumbs { get; init; }

    public required DriveUsage Usage { get; init; }

    public required DriveTransfer Transfer { get; init; }

    public ObservableCollection<DriveEntry> Entries { get; } = [];

    public int SelectedCount => Entries.Count(static x => x.IsSelected);

    public long SelectedSize => Entries.Where(static x => x.IsSelected).Sum(static x => x.Size);

    public void SelectAll(bool value)
    {
        foreach (var entry in Entries)
        {
            entry.IsSelected = value;
        }
    }

    public void Remove()
    {
        foreach (var entry in Entries.Where(static x => x.IsSelected).ToList())
        {
            Entries.Remove(entry);
        }
    }

    // Moves the selection into the first folder of the current place
    public string? Move()
    {
        var folder = Entries.FirstOrDefault(static x => (x.Kind == DriveItemKind.Folder) && !x.IsSelected);
        if (folder is null)
        {
            return null;
        }

        var moved = Entries.Where(x => x.IsSelected && !ReferenceEquals(x, folder)).ToList();
        foreach (var entry in moved)
        {
            Entries.Remove(entry);
        }

        return moved.Count > 0 ? $"{moved.Count} 項目を「{folder.Name}」へ移動しました" : null;
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class DriveSample
{
    private const long Gigabyte = 1024L * 1024L * 1024L;

    private static readonly (string Name, DriveItemKind Kind, long Size, int Days, DriveSyncState Sync, int Count)[] Plan =
    [
        ("レビュー資料", DriveItemKind.Folder, 0, 1, DriveSyncState.Synced, 18),
        ("旧版アーカイブ", DriveItemKind.Folder, 0, 12, DriveSyncState.Synced, 42),
        ("画面設計書_v4.2.xlsx", DriveItemKind.Sheet, 2_640_000, 0, DriveSyncState.Synced, 0),
        ("API仕様書.pdf", DriveItemKind.Pdf, 5_120_000, 0, DriveSyncState.Pending, 0),
        ("キックオフ資料.pptx", DriveItemKind.Slide, 8_830_000, 2, DriveSyncState.Synced, 0),
        ("要件定義書.docx", DriveItemKind.Document, 1_420_000, 3, DriveSyncState.Synced, 0),
        ("画面遷移図.png", DriveItemKind.Image, 3_270_000, 3, DriveSyncState.Local, 0),
        ("操作デモ.mp4", DriveItemKind.Video, 48_600_000, 5, DriveSyncState.Pending, 0),
        ("検証ログ.zip", DriveItemKind.Archive, 12_400_000, 6, DriveSyncState.Synced, 0),
        ("sample_layout.xaml", DriveItemKind.Code, 36_800, 6, DriveSyncState.Synced, 0),
        ("議事録_1021.docx", DriveItemKind.Document, 684_000, 8, DriveSyncState.Synced, 0),
        ("配色検討.png", DriveItemKind.Image, 2_110_000, 9, DriveSyncState.Synced, 0)
    ];

    public static CloudDrive LoadDrive(DateTime now)
    {
        var drive = new CloudDrive
        {
            Crumbs = [new("クラウド"), new("共有"), new("2026"), new("設計書")],
            Usage = new DriveUsage(
                (long)(18.2 * Gigabyte),
                (long)(9.6 * Gigabyte),
                (long)(28.4 * Gigabyte),
                (long)(6.2 * Gigabyte),
                100 * Gigabyte),
            Transfer = new DriveTransfer
            {
                Name = "操作デモ.mp4",
                Current = 2,
                Total = 5,
                Progress = 0.64,
                IsActive = true
            }
        };

        foreach (var (name, kind, size, days, sync, count) in Plan)
        {
            drive.Entries.Add(new DriveEntry
            {
                Name = name,
                Kind = kind,
                Size = size,
                Updated = now.AddDays(-days).AddHours(-(days * 3) % 11),
                Sync = sync,
                Count = count
            });
        }

        return drive;
    }
}
