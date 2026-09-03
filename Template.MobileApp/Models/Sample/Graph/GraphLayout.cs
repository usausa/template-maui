namespace Template.MobileApp.Models.Sample.Graph;

public enum GraphRefKind
{
    Head,
    LocalBranch,
    RemoteBranch,
    Tag
}

public sealed class GraphRef
{
    public string Name { get; init; } = default!;

    public GraphRefKind Kind { get; init; }
}

public enum GraphSegmentKind
{
    Vertical,
    HalfVerticalTop,
    HalfVerticalBottom,
    Diagonal,
    DiagonalBranch
}

public sealed class GraphSegment
{
    public GraphSegmentKind Kind { get; init; }

    public int Lane { get; init; }

    public int ToLane { get; init; }

    public int ColorIndex { get; init; }
}

public sealed class GraphRow
{
    public string Sha { get; init; } = default!;

    public string ShortSha { get; init; } = default!;

    public int Row { get; init; }

    public int Lane { get; init; }

    public string Author { get; init; } = string.Empty;

    public DateTimeOffset When { get; init; }

    public string Summary { get; init; } = string.Empty;

    public IReadOnlyList<GraphRef> Refs { get; init; } = default!;

    public IReadOnlyList<GraphSegment> Segments { get; init; } = default!;

    public int LaneCount { get; init; }
}

public sealed class GraphData
{
    public IReadOnlyList<GraphRow> Rows { get; init; } = default!;

    public int LaneCount { get; init; }
}
