namespace Template.MobileApp.Models.Sample.Calendar;

#pragma warning disable CA1720
public enum CalendarSelectionMode
{
    // 選択なし
    None,

    // 一度に 1 日だけ選択できます
    Single,

    // 複数の日付を個別に選択できます
    Multiple,

    // 開始日と終了日の範囲を選択できます
    Range
}
#pragma warning restore CA1720
