namespace Template.MobileApp.Helpers;

public sealed class SettingParser
{
    private readonly Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);

    public SettingParser(string data)
    {
        using var reader = new StringReader(data);
        while (reader.ReadLine() is { } line)
        {
            var index = line.IndexOf('=', StringComparison.Ordinal);
            if (index > 0)
            {
                values[line[..index].Trim()] = line[(index + 1)..].Trim();
            }
        }
    }

    public bool TryGetString(string key, out string value)
    {
        if (values.TryGetValue(key, out var str))
        {
            value = str;
            return true;
        }

        value = string.Empty;
        return false;
    }

    public string GetString(string key, string defaultValue = "") => values.GetValueOrDefault(key, defaultValue);

    public bool TryGetInt(string key, out int value)
    {
        if (values.TryGetValue(key, out var temp))
        {
            return Int32.TryParse(temp, out value);
        }

        value = 0;
        return false;
    }

    public int GetInt(string key) => values.TryGetValue(key, out var value) && Int32.TryParse(value, out var result) ? result : default;

    public int GetInt(string key, int defaultValue) => values.TryGetValue(key, out var value) && Int32.TryParse(value, out var result) ? result : defaultValue;
}
