namespace Template.MobileApp.Services;

public sealed class ParameterBuilder
{
    private readonly StringBuilder parameterString = new();

    public void Add(string key, string value)
    {
        parameterString.Append(parameterString.Length == 0 ? '?' : '&');
        parameterString.Append(key);
        parameterString.Append('=');
        parameterString.Append(value);
    }

    public void AddIfNotEmpty(string key, string value)
    {
        if (!String.IsNullOrEmpty(value))
        {
            Add(key, value);
        }
    }

    public void AddIfHasValue<T>(string key, T? value)
        where T : struct
    {
        if (value.HasValue)
        {
            Add(key, value.Value.ToString()!);
        }
    }

    public void AddValue<T>(string key, T value)
        where T : struct
    {
        Add(key, value.ToString()!);
    }

    public void AddValues(string key, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            Add(key, value);
        }
    }

    public StringBuilder ToParameter()
    {
        return parameterString;
    }
}
