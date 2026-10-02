namespace Template.MobileApp.Modules;

using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1724
public static class Parameters
{
    private const string NextViewId = nameof(NextViewId);

    private const string No = nameof(No);

    private const string Model = nameof(Model);

    public static NavigationParameter Make() => new();

    public static NavigationParameter MakeNextViewId(ViewId viewId) =>
        new NavigationParameter().SetValue(NextViewId, viewId);

    public static ViewId GetNextViewId(this INavigationParameter parameter) =>
        parameter.GetValue<ViewId>(NextViewId);

    public static NavigationParameter WithNo(this NavigationParameter parameter, string no) =>
        parameter.SetValue(No, no);

    public static string GetNo(this INavigationParameter parameter) =>
        parameter.GetValue<string>(No);

    public static NavigationParameter MakeModel<T>(T model) =>
        new NavigationParameter().SetValue(Model, model);

    public static T GetModel<T>(this INavigationParameter parameter) =>
        parameter.GetValue<T>(Model);

    public static bool TryGetModel<T>(this INavigationParameter parameter, [NotNullWhen(true)] out T? model) =>
        parameter.TryGetValue(Model, out model);
}
#pragma warning restore CA1724
