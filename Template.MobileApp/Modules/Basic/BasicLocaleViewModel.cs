namespace Template.MobileApp.Modules.Basic;

using Template.MobileApp.Resources.Strings;

public sealed record ResourceEntry(
    string Key,
    string NeutralValue,
    string JapaneseValue,
    string CurrentValue);

public sealed record CultureFormatEntry(
    string Name,
    bool IsCurrent,
    string Number,
    string Currency,
    string Date,
    string Time);

public sealed class BasicLocaleViewModel : AppViewModelBase
{
    // Property

    public string CultureName { get; } = CultureInfo.CurrentCulture.Name;

    public string UICultureName { get; } = CultureInfo.CurrentUICulture.Name;

    public IReadOnlyList<ResourceEntry> ResourceEntries { get; }

    public IReadOnlyList<CultureFormatEntry> FormatEntries { get; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public BasicLocaleViewModel()
    {
        ResourceEntries =
        [
            .. EnumerateResources("Names", Names.Keys, Names.GetTextFor),
            .. EnumerateResources("Messages", Messages.Keys, Messages.GetTextFor)
        ];

        FormatEntries = new[] { CultureInfo.CurrentCulture, new CultureInfo("en-US"), new CultureInfo("de-DE"), new CultureInfo("ja-JP") }
            .DistinctBy(static x => x.Name)
            .Select(x => CreateFormatEntry(x, x.Name == CultureInfo.CurrentCulture.Name))
            .ToList();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    protected override Task OnNotifyBackAsync() => Navigator.ForwardAsync(ViewId.BasicMenu);

    protected override Task OnNotifyFunction1() => OnNotifyBackAsync();

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private static IEnumerable<ResourceEntry> EnumerateResources(string source, IEnumerable<string> keys, Func<string, CultureInfo, string?> getText)
    {
        var japanese = new CultureInfo("ja");
        foreach (var key in keys)
        {
            yield return new ResourceEntry(
                $"{source}.{key}",
                getText(key, CultureInfo.InvariantCulture) ?? string.Empty,
                getText(key, japanese) ?? string.Empty,
                getText(key, CultureInfo.CurrentUICulture) ?? string.Empty);
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
}
