namespace Template.MobileApp.Models.Sample.Graph;

public sealed class GraphCommit
{
    public required string Sha { get; init; }

    public string Author { get; init; } = string.Empty;

    public required DateTimeOffset AuthorWhen { get; init; }

    public string Summary { get; init; } = string.Empty;

    public required IReadOnlyList<string> ParentShas { get; init; }
}

public sealed class GraphRefData
{
    public required string TargetSha { get; init; }

    public required string Name { get; init; }

    public required string Kind { get; init; }
}
