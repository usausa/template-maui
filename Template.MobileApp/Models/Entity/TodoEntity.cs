namespace Template.MobileApp.Models.Entity;

using Smart.Data.Accessor.Attributes;

[Name("Todo")]
public sealed class TodoEntity
{
    [Key]
    public long Id { get; set; }

    public string Title { get; set; } = default!;

    public string Note { get; set; } = default!;

    public DateTime? DueDate { get; set; }

    public bool IsImportant { get; set; }

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
