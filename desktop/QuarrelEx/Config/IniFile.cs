using System.Globalization;

namespace QuarrelEx.Config;

internal sealed class IniFile
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections =
        new(StringComparer.OrdinalIgnoreCase);

    public static IniFile Load(string path)
    {
        var ini = new IniFile();
        var section = string.Empty;

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line[1..^1].Trim();
                if (!ini._sections.ContainsKey(section))
                    ini._sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq < 0)
                continue;

            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim();
            if (!ini._sections.TryGetValue(section, out var map))
            {
                map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                ini._sections[section] = map;
            }
            map[key] = value;
        }
        return ini;
    }

    public string Get(string section, string key, string fallback)
    {
        return _sections.TryGetValue(section, out var map) && map.TryGetValue(key, out var value)
            ? value
            : fallback;
    }

    public int GetDecimal(string section, string key, int fallback)
    {
        return int.TryParse(Get(section, key, fallback.ToString(CultureInfo.InvariantCulture)), out var value)
            ? value
            : fallback;
    }

    public int GetHex(string section, string key, int fallback)
    {
        var text = Get(section, key, fallback.ToString("X"));
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) text = text[2..];
        if (text.StartsWith('$')) text = text[1..];
        return int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }
}
