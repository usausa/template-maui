namespace Template.MobileApp.Models;

using Smart.Mapper;

public static partial class ObjectMapper
{
    [Mapper]
    public static partial void Copy(SwitchBotTemperature source, SwitchBotTemperature destination);

    [Mapper]
    public static partial WorkEntity ToWorkEntity(DataListResponseEntry source);
}
