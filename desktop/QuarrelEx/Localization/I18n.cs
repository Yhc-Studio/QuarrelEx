using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuarrelEx.Localization;

public static class I18n
{
    public const string Chinese = "zh-CN";
    public const string English = "en-US";
    public const string Japanese = "ja-JP";

    private sealed class Binding
    {
        public string Key { get; }
        public object[] Args { get; }
        public string LastRendered { get; set; } = string.Empty;
        public Binding(string key, object[]? args = null)
        {
            Key = key;
            Args = args ?? Array.Empty<object>();
        }
    }

    private sealed record SourcePattern(string Key, Regex Regex, int ArgCount, int[] ArgIds, int Length);

    private sealed class ComboBinding
    {
        public string?[] Keys { get; }
        public ComboBinding(string?[] keys) => Keys = keys;
    }

    private static readonly ConditionalWeakTable<object, Binding> Bindings = new();
    private static readonly ConditionalWeakTable<ComboBox, ComboBinding> ComboBindings = new();
    private static readonly ConditionalWeakTable<ToolStripItem, Binding> TooltipBindings = new();

    private static readonly Dictionary<string, string> SourceKeys = LoadJson("source-keys.json");
    private static readonly Dictionary<string, Dictionary<string, string>> Catalogs = new(StringComparer.OrdinalIgnoreCase)
    {
        [Chinese] = LoadJson("zh-CN.json"),
        [English] = LoadJson("en-US.json"),
        [Japanese] = LoadJson("ja-JP.json")
    };

    private static readonly Dictionary<string, string> ExactKeys = BuildExactKeys();
    private static readonly List<SourcePattern> SourcePatterns = BuildSourcePatterns();
    private static string _currentLanguage = LoadPreferredLanguage();

    public static event EventHandler? LanguageChanged;

    public static string CurrentLanguage => _currentLanguage;

    public static IReadOnlyList<(string Code, string NativeName)> Languages { get; } =
    [
        (Chinese, "简体中文"),
        (English, "English"),
        (Japanese, "日本語")
    ];

    public static string T(string key)
    {
        if (Catalogs.TryGetValue(_currentLanguage, out var active) && active.TryGetValue(key, out var text))
            return text;
        if (Catalogs.TryGetValue(English, out var en) && en.TryGetValue(key, out text))
            return text;
        if (Catalogs.TryGetValue(Chinese, out var zh) && zh.TryGetValue(key, out text))
            return text;
        return key;
    }

    public static string T(string key, params object[] args)
    {
        var format = T(key);
        try { return string.Format(format, args); }
        catch (FormatException) { return format; }
    }

    public static string FromSource(string text)
    {
        if (TryCreateBinding(text, out var binding)) return Render(binding);
        return text;
    }

    public static string FromSourceMultiline(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        return string.Join(Environment.NewLine, normalized.Split('\n').Select(FromSource));
    }

    public static bool SetLanguage(string code)
    {
        code = NormalizeLanguage(code);
        var changed = !string.Equals(code, _currentLanguage, StringComparison.OrdinalIgnoreCase);
        _currentLanguage = code;
        SavePreferredLanguage(code);
        // Always notify.  This also lets the user re-apply the current language after
        // controls/tool windows were created dynamically.
        LanguageChanged?.Invoke(null, EventArgs.Empty);
        return changed;
    }

    public static void TranslateControlTree(Control root)
    {
        TranslateControl(root);
        foreach (Control child in root.Controls)
            TranslateControlTree(child);
    }

    public static void TranslateToolStrip(ToolStrip strip)
    {
        foreach (ToolStripItem item in strip.Items)
            TranslateToolStripItem(item);
    }

    public static void TranslateText(object owner, Action<string> setter, string currentText)
    {
        if (Bindings.TryGetValue(owner, out var existing))
        {
            if (currentText == existing.LastRendered)
            {
                var rendered = Render(existing);
                existing.LastRendered = rendered;
                setter(rendered);
                return;
            }

            if (TryCreateBinding(currentText, out var rebound))
            {
                Bindings.Remove(owner);
                Bindings.Add(owner, rebound);
                var rendered = Render(rebound);
                rebound.LastRendered = rendered;
                setter(rendered);
                return;
            }

            Bindings.Remove(owner);
            return;
        }

        if (!TryCreateBinding(currentText, out var binding)) return;
        Bindings.Add(owner, binding);
        var value = Render(binding);
        binding.LastRendered = value;
        setter(value);
    }

    private static void TranslateControl(Control control)
    {
        TranslateText(control, value => control.Text = value, control.Text);

        if (control is ComboBox combo)
            TranslateComboBox(combo);
        if (control is DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
                TranslateText(column, value => column.HeaderText = value, column.HeaderText);
        }
        if (control is ToolStrip strip)
            TranslateToolStrip(strip);
    }

    private static void TranslateComboBox(ComboBox combo)
    {
        if (!ComboBindings.TryGetValue(combo, out var binding) || binding.Keys.Length != combo.Items.Count)
        {
            var keys = new string?[combo.Items.Count];
            for (var i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is string s && TryFindExactKey(s, out var key)) keys[i] = key;
            }
            if (ComboBindings.TryGetValue(combo, out _)) ComboBindings.Remove(combo);
            binding = new ComboBinding(keys);
            ComboBindings.Add(combo, binding);
        }

        var selected = combo.SelectedIndex;
        for (var i = 0; i < combo.Items.Count && i < binding.Keys.Length; i++)
        {
            if (binding.Keys[i] is { } key) combo.Items[i] = T(key);
        }
        if (selected >= 0 && selected < combo.Items.Count) combo.SelectedIndex = selected;
    }

    private static void TranslateToolStripItem(ToolStripItem item)
    {
        TranslateText(item, value => item.Text = value, item.Text);
        if (!string.IsNullOrWhiteSpace(item.ToolTipText))
        {
            if (!TooltipBindings.TryGetValue(item, out var tooltipBinding) && TryFindExactKey(item.ToolTipText, out var tooltipKey))
            {
                tooltipBinding = new Binding(tooltipKey);
                TooltipBindings.Add(item, tooltipBinding);
            }
            if (tooltipBinding is not null) item.ToolTipText = T(tooltipBinding.Key);
        }
        if (item is ToolStripDropDownItem dropDown)
            foreach (ToolStripItem child in dropDown.DropDownItems)
                TranslateToolStripItem(child);
    }

    private static string Render(Binding binding)
    {
        var format = T(binding.Key);
        if (binding.Args.Length == 0) return format;
        try { return string.Format(format, binding.Args); }
        catch (FormatException) { return format; }
    }

    private static bool TryFindExactKey(string source, out string key)
    {
        if (ExactKeys.TryGetValue(source, out var found))
        {
            key = found;
            return true;
        }
        key = string.Empty;
        return false;
    }

    private static bool TryCreateBinding(string source, out Binding binding)
    {
        if (TryFindExactKey(source, out var key))
        {
            binding = new Binding(key);
            return true;
        }

        foreach (var pattern in SourcePatterns)
        {
            var match = pattern.Regex.Match(source);
            if (!match.Success) continue;

            var args = Enumerable.Repeat<object>(string.Empty, pattern.ArgCount).ToArray();
            foreach (var argIndex in pattern.ArgIds)
                args[argIndex] = match.Groups[$"a{argIndex}"].Value;

            binding = new Binding(pattern.Key, args);
            return true;
        }

        binding = null!;
        return false;
    }

    private static Dictionary<string, string> BuildExactKeys()
    {
        var result = new Dictionary<string, string>(SourceKeys, StringComparer.Ordinal);

        foreach (var catalog in Catalogs.Values)
        {
            var counts = catalog.Values
                .Where(text => !string.IsNullOrEmpty(text))
                .GroupBy(text => text, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

            foreach (var (key, text) in catalog)
            {
                if (string.IsNullOrEmpty(text) || counts[text] != 1 || result.ContainsKey(text)) continue;
                result[text] = key;
            }
        }

        return result;
    }

    private static List<SourcePattern> BuildSourcePatterns()
    {
        var result = new List<SourcePattern>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var placeholder = new Regex(@"\{(\d+)\}", RegexOptions.CultureInvariant);

        foreach (var catalog in Catalogs.Values)
        {
            foreach (var (key, template) in catalog)
            {
                if (string.IsNullOrEmpty(template)) continue;
                var matches = placeholder.Matches(template);
                if (matches.Count == 0) continue;

                var signature = key + "\0" + template;
                if (!seen.Add(signature)) continue;

                var allArgIds = matches.Cast<Match>().Select(m => int.Parse(m.Groups[1].Value)).ToArray();
                var argIds = allArgIds.Distinct().ToArray();
                var argCount = allArgIds.Max() + 1;
                var seenArgs = new HashSet<int>();
                var regexText = new StringBuilder("^");
                var last = 0;
                foreach (Match match in matches)
                {
                    regexText.Append(Regex.Escape(template.Substring(last, match.Index - last)));
                    var argId = int.Parse(match.Groups[1].Value);
                    if (seenArgs.Add(argId)) regexText.Append($"(?<a{argId}>.*?)");
                    else regexText.Append($@"\k<a{argId}>");
                    last = match.Index + match.Length;
                }
                regexText.Append(Regex.Escape(template[last..]));
                regexText.Append('$');

                result.Add(new SourcePattern(
                    key,
                    new Regex(regexText.ToString(), RegexOptions.Singleline | RegexOptions.CultureInvariant),
                    argCount,
                    argIds,
                    template.Length));
            }
        }

        return result.OrderByDescending(x => x.Length).ToList();
    }

    private static Dictionary<string, string> LoadJson(string fileName)
    {
        // The generated BuiltInCatalogs.g.cs is the runtime baseline.  This is
        // deliberately independent of MSBuild EmbeddedResource naming and of a
        // loose Locales directory, so Release/Publish builds always retain the
        // three built-in languages.
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            var builtInJson = BuiltInCatalogs.ReadJson(fileName);
            if (!string.IsNullOrWhiteSpace(builtInJson))
            {
                var builtIn = JsonSerializer.Deserialize<Dictionary<string, string>>(builtInJson);
                if (builtIn is not null)
                    foreach (var (key, value) in builtIn) result[key] = value;
            }
        }
        catch
        {
            // Keep an empty baseline and allow the loose-file fallback below.
        }

        // Loose files are optional development/user overrides.  Overlay them on
        // top of the built-in catalog instead of making runtime localization
        // depend on their presence.
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Locales", fileName);
            if (File.Exists(path))
            {
                var loose = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
                if (loose is not null)
                    foreach (var (key, value) in loose) result[key] = value;
            }
        }
        catch
        {
            // A malformed optional override must never disable built-in i18n.
        }
        return result;
    }

    public static bool CatalogsReady =>
        SourceKeys.Count != 0 &&
        Catalogs.TryGetValue(Chinese, out var zh) && zh.Count != 0 &&
        Catalogs.TryGetValue(English, out var en) && en.Count != 0 &&
        Catalogs.TryGetValue(Japanese, out var ja) && ja.Count != 0;

    public static IReadOnlyDictionary<string, int> CatalogCounts { get; } =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["source-keys"] = SourceKeys.Count,
            [Chinese] = Catalogs.TryGetValue(Chinese, out var zhCount) ? zhCount.Count : 0,
            [English] = Catalogs.TryGetValue(English, out var enCount) ? enCount.Count : 0,
            [Japanese] = Catalogs.TryGetValue(Japanese, out var jaCount) ? jaCount.Count : 0
        };

    private static string LoadPreferredLanguage()
    {
        try
        {
            var path = PreferencePath();
            if (File.Exists(path)) return NormalizeLanguage(File.ReadAllText(path).Trim());
        }
        catch { }

        var ui = System.Globalization.CultureInfo.CurrentUICulture.Name;
        if (ui.StartsWith("zh", StringComparison.OrdinalIgnoreCase)) return Chinese;
        if (ui.StartsWith("ja", StringComparison.OrdinalIgnoreCase)) return Japanese;
        return English;
    }

    private static string NormalizeLanguage(string? code)
    {
        if (code?.StartsWith("zh", StringComparison.OrdinalIgnoreCase) == true) return Chinese;
        if (code?.StartsWith("ja", StringComparison.OrdinalIgnoreCase) == true) return Japanese;
        return English;
    }

    private static void SavePreferredLanguage(string code)
    {
        try
        {
            var path = PreferencePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, code);
        }
        catch { }
    }

    private static string PreferencePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QuarrelEx", "ui-language.txt");
}
