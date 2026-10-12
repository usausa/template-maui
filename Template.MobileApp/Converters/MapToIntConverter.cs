namespace Template.MobileApp.Converters;

using Smart.Maui.Data;

// 一致しない値は DefaultValue
public sealed class MapToIntEntry : MapEntry<int>;

public sealed class MapToIntConverter : MapToObjectConverter<int>;
