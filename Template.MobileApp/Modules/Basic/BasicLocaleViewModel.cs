namespace Template.MobileApp.Modules.Basic;

using System.Collections;
using System.Resources;

using Template.MobileApp.Resources.Strings;

// .resx の参照結果 (ニュートラル / ja / 現在カルチャ)
public sealed record ResourceEntry(string Key, string NeutralValue, string JapaneseValue, string CurrentValue);

// カルチャ別の書式差 (数値 / 通貨 / 日付 / 時刻)
public sealed record CultureFormatEntry(string Name, bool IsCurrent, string Number, string Currency, string Date, string Time);

public sealed class BasicLocaleViewModel : AppViewModelBase
{
    public string CultureName { get; } = CultureInfo.CurrentCulture.Name;

    public string UICultureName { get; } = CultureInfo.CurrentUICulture.Name;

    public IReadOnlyList<ResourceEntry> ResourceEntries { get; }

    public IReadOnlyList<CultureFormatEntry> FormatEntries { get; }

    public BasicLocaleViewModel()
    {
        ResourceEntries =
        [
            .. EnumerateResources("Names", Names.ResourceManager),
            .. EnumerateResources("Messages", Messages.ResourceManager)
        ];

        // 同じ値をカルチャ毎に整形して書式差を見せる (切替機構は作らない)
        FormatEntries = new[] { CultureInfo.CurrentCulture, new CultureInfo("en-US"), new CultureInfo("de-DE"), new CultureInfo("ja-JP") }
            .DistinctBy(static x => x.Name)
            .Select(x => CreateFormatEntry(x, x.Name == CultureInfo.CurrentCulture.Name))
            .ToList();
    }

    private static IEnumerable<ResourceEntry> EnumerateResources(string source, ResourceManager manager)
    {
        // ニュートラルのセットからキーを列挙し、ja と現在カルチャの参照結果を並べる
        var neutral = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
        if (neutral is null)
        {
            yield break;
        }

        var japanese = manager.GetResourceSet(new CultureInfo("ja"), true, false);
        foreach (var key in neutral.Cast<DictionaryEntry>().Select(static x => (string)x.Key).Order(StringComparer.Ordinal))
        {
            yield return new ResourceEntry(
                $"{source}.{key}",
                neutral.GetString(key) ?? string.Empty,
                japanese?.GetString(key) ?? string.Empty,
                manager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty);
        }
    }

    private static CultureFormatEntry CreateFormatEntry(CultureInfo culture, bool isCurrent)
    {
        var number = 1234567.891d;
        var price = 1980m;
        var now = DateTime.Now;
        return new CultureFormatEntry(
            culture.Name,
            isCurrent,
            number.ToString("N2", culture),
            price.ToString("C", culture),
            now.ToString("D", culture),
            now.ToString("t", culture));
    }

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();
}
