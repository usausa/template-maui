namespace Template.MobileApp.Models.Sample;

#pragma warning disable CA1056
public sealed partial class PhotoItem : ObservableObject
{
    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string ImageUrl { get; set; } = default!;

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }
}
#pragma warning restore CA1056
