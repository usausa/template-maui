namespace Template.MobileApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum SuperCrowd
{
    Low,
    Medium,
    High
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class SuperBanner
{
    public required string Image { get; init; }

    public required string Title { get; init; }

    public required string Sub { get; init; }
}

public sealed class SuperApp
{
    public required string Glyph { get; init; }

    public required string Name { get; init; }

    public required Color Color { get; init; }
}

public sealed partial class SuperCoupon : ObservableObject
{
    public required string Title { get; init; }

    public required string Sub { get; init; }

    public required Brush Background { get; init; }

    [ObservableProperty]
    public partial bool IsAcquired { get; set; }
}

public sealed class SuperShop
{
    public required string Glyph { get; init; }

    public required string Name { get; init; }

    public required string Category { get; init; }

    public required Color Color { get; init; }

    public required int Distance { get; init; }

    public required SuperCrowd Crowd { get; init; }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

// スーパーアプリのホームの見本 (ポイント・バナー・サービス・クーポン・近くのお店)
public static class SuperSample
{
    public static int LoadPoint() => 1250;

    public static IReadOnlyList<SuperBanner> LoadBanners() =>
    [
        new() { Image = "banner01.jpg", Title = "オータムセール開催中", Sub = "全館ポイント 5 倍キャンペーン" },
        new() { Image = "banner02.jpg", Title = "新作ギフト入荷", Sub = "ラッピング無料でお届け" },
        new() { Image = "banner03.jpg", Title = "ポイントキャンペーン", Sub = "期間限定ポイント 2 倍" }
    ];

    public static IReadOnlyList<SuperApp> LoadApps() =>
    [
        new() { Glyph = Fonts.MaterialIcons.Payments, Name = "支払い", Color = Color.FromArgb("#1E88E5") },
        new() { Glyph = Fonts.MaterialIcons.Receipt_long, Name = "家計簿", Color = Color.FromArgb("#43A047") },
        new() { Glyph = Fonts.MaterialIcons.Local_shipping, Name = "配送", Color = Color.FromArgb("#FB8C00") },
        new() { Glyph = Fonts.MaterialIcons.Confirmation_number, Name = "チケット", Color = Color.FromArgb("#8E24AA") },
        new() { Glyph = Fonts.MaterialIcons.Restaurant, Name = "フード", Color = Color.FromArgb("#E53935") },
        new() { Glyph = Fonts.MaterialIcons.Directions_car, Name = "タクシー", Color = Color.FromArgb("#00ACC1") },
        new() { Glyph = Fonts.MaterialIcons.Hotel, Name = "ホテル", Color = Color.FromArgb("#5E35B1") },
        new() { Glyph = Fonts.MaterialIcons.More_horiz, Name = "その他", Color = Color.FromArgb("#757575") }
    ];

    public static IReadOnlyList<SuperCoupon> LoadCoupons() =>
    [
        new()
        {
            Title = "コーヒー 100 円引き",
            Sub = "対象店舗限定 / 7月末まで",
            Background = new LinearGradientBrush(
                [new GradientStop(Color.FromArgb("#FF8A65"), 0f), new GradientStop(Color.FromArgb("#FF5252"), 1f)],
                new Point(0, 0),
                new Point(1, 1))
        },
        new()
        {
            Title = "送料無料クーポン",
            Sub = "3,000 円以上の注文で利用可",
            Background = new LinearGradientBrush(
                [new GradientStop(Color.FromArgb("#4FC3F7"), 0f), new GradientStop(Color.FromArgb("#1E88E5"), 1f)],
                new Point(0, 0),
                new Point(1, 1))
        },
        new()
        {
            Title = "ポイント 2 倍デー",
            Sub = "毎週金曜日はポイントアップ",
            Background = new LinearGradientBrush(
                [new GradientStop(Color.FromArgb("#81C784"), 0f), new GradientStop(Color.FromArgb("#2E7D32"), 1f)],
                new Point(0, 0),
                new Point(1, 1))
        },
        new()
        {
            Title = "映画 300 円引き",
            Sub = "レイトショー限定",
            Background = new LinearGradientBrush(
                [new GradientStop(Color.FromArgb("#BA68C8"), 0f), new GradientStop(Color.FromArgb("#6A1B9A"), 1f)],
                new Point(0, 0),
                new Point(1, 1))
        }
    ];

    public static IReadOnlyList<SuperShop> LoadShops() =>
    [
        new() { Glyph = Fonts.MaterialIcons.Local_cafe, Name = "カフェ モカ", Category = "カフェ", Color = Color.FromArgb("#6D4C41"), Distance = 120, Crowd = SuperCrowd.Low },
        new() { Glyph = Fonts.MaterialIcons.Ramen_dining, Name = "麺処 ひだまり", Category = "ラーメン", Color = Color.FromArgb("#E53935"), Distance = 350, Crowd = SuperCrowd.High },
        new() { Glyph = Fonts.MaterialIcons.Local_grocery_store, Name = "フードマート", Category = "スーパー", Color = Color.FromArgb("#43A047"), Distance = 480, Crowd = SuperCrowd.Medium },
        new() { Glyph = Fonts.MaterialIcons.Bakery_dining, Name = "パン工房 ルナ", Category = "パン", Color = Color.FromArgb("#FB8C00"), Distance = 620, Crowd = SuperCrowd.Low },
        new() { Glyph = Fonts.MaterialIcons.Local_pharmacy, Name = "あおば薬局", Category = "ドラッグストア", Color = Color.FromArgb("#1E88E5"), Distance = 780, Crowd = SuperCrowd.Medium },
        new() { Glyph = Fonts.MaterialIcons.Menu_book, Name = "ブックス 青空", Category = "書店", Color = Color.FromArgb("#5E35B1"), Distance = 930, Crowd = SuperCrowd.Low }
    ];
}
