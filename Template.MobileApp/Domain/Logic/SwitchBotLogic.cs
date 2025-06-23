namespace Template.MobileApp.Domain.Logic;

public static class SwitchBotLogic
{
    public static bool IsTargetCompanyId(ushort value) => value == 0x0969;

    public static bool IsScanResponse(string uuid) => uuid == "0000fd3d-0000-1000-8000-00805f9b34fb";

    public static bool IsTemperatureDevice(byte deviceType) => deviceType is 0x35 or 0x54 or 0x77;

    public static bool IsValidTemperatureData(byte[] data) => data.Length >= 11;

    public static string ExtractDeviceId(byte[] data) => Convert.ToHexString(data.AsSpan(0, 6));

    public static double ExtractTemperature(byte[] data) => (((double)(data[8] & 0x0f) / 10) + (data[9] & 0x7f)) * ((data[9] & 0x80) > 0 ? 1 : -1);

    public static int ExtractHumidity(byte[] data) => data[10] & 0x7f;

    public static int? ExtractCo2(byte[] data) => data.Length >= 16 ? (data[13] << 8) + data[14] : null;
}
