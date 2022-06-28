namespace Template.MobileApp;

using System.Reflection;

public static class Extensions
{
    public static IEnumerable<Type> UnderNamespaceTypes(this Assembly assembly, Type baseNamespaceType)
    {
        var ns = baseNamespaceType.Namespace!;
        return assembly.ExportedTypes.Where(x => x.Namespace?.StartsWith(ns, StringComparison.Ordinal) ?? false);
    }
}
