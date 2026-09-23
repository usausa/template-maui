namespace Template.MobileApp.Modules;

using System.Diagnostics.CodeAnalysis;

using ClamGrid;

#pragma warning disable CA1724
public static class Parameters
{
    private const string NextViewId = nameof(NextViewId);

    private const string No = nameof(No);

    private const string ColumnEditSession = nameof(ColumnEditSession);

    private const string ColumnOrders = nameof(ColumnOrders);

    public static NavigationParameter Make() => new();

    public static NavigationParameter MakeNextViewId(ViewId viewId) =>
        new NavigationParameter().SetValue(NextViewId, viewId);

    public static ViewId GetNextViewId(this INavigationParameter parameter) =>
        parameter.GetValue<ViewId>(NextViewId);

    public static NavigationParameter WithNo(this NavigationParameter parameter, string no) =>
        parameter.SetValue(No, no);

    public static string GetNo(this INavigationParameter parameter) =>
        parameter.GetValue<string>(No);

    // 列設定画面へ渡す編集セッションと、戻りで受け取る列の表示 / 順序
    public static NavigationParameter MakeColumnEditSession(GridColumnEditSession session) =>
        new NavigationParameter().SetValue(ColumnEditSession, session);

    public static GridColumnEditSession GetColumnEditSession(this INavigationParameter parameter) =>
        parameter.GetValue<GridColumnEditSession>(ColumnEditSession);

    public static NavigationParameter MakeColumnOrders(GridColumnOrder[] orders) =>
        new NavigationParameter().SetValue(ColumnOrders, orders);

    public static bool TryGetColumnOrders(this INavigationParameter parameter, [NotNullWhen(true)] out GridColumnOrder[]? orders) =>
        parameter.TryGetValue(ColumnOrders, out orders);
}
#pragma warning restore CA1724
