namespace Template.MobileApp.Models.Sample;

using ClamGrid;

// 列設定画面の行 (表示の有無はチェックで編集する)
public static class ColumnOptionAccessors
{
    public static GridValueAccessorCollection<GridColumnOption> Option { get; } = new()
    {
        { nameof(GridColumnOption.IsVisible), static x => x.IsVisible, static (x, value) => x.IsVisible = value },
        { nameof(GridColumnOption.Header), static x => x.Header }
    };
}
