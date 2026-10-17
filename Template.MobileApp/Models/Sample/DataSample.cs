namespace Template.MobileApp.Models.Sample;

public static class DataSample
{
    public static IReadOnlyList<WorkEntity> LoadWorks() =>
    [
        new() { Id = 1, Name = "Sample-1" },
        new() { Id = 2, Name = "Sample-2" },
        new() { Id = 3, Name = "Sample-3" },
        new() { Id = 4, Name = "Sample-4" }
    ];

    public static IReadOnlyList<TodoEntity> LoadTodos(DateTime now)
    {
        var today = now.Date;
        return
        [
            new() { Id = 1, Title = "請求書を送る", Note = "経理の担当へ PDF で送る", DueDate = today.AddDays(-2), IsImportant = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, Title = "家賃の振り込み", Note = string.Empty, DueDate = today.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, Title = "企画書のレビュー", Note = "3 章の図を差し替える", DueDate = today, IsImportant = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, Title = "牛乳とパンを買う", Note = string.Empty, DueDate = today, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, Title = "歯医者の予約", Note = "午前中に電話する", DueDate = today.AddDays(1), CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, Title = "週次レポートを書く", Note = string.Empty, DueDate = today.AddDays(4), CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, Title = "読みたい本をリストにする", Note = string.Empty, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, Title = "部屋の掃除", Note = string.Empty, DueDate = today.AddDays(-1), IsDone = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, Title = "ジムに行く", Note = "30 分のラン", DueDate = today, IsDone = true, CreatedAt = now, UpdatedAt = now }
        ];
    }
}
