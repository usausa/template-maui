namespace Template.MobileApp.Converters;

using Smart.Maui.Data;

using Template.MobileApp.Controls;

// 一致しない値は DefaultValue(Neutral)
public sealed class MapToToneEntry : MapEntry<StatusTone>;

public sealed class MapToToneConverter : MapToObjectConverter<StatusTone>;
