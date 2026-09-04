using System.Drawing;

namespace QuarrelEx.Rendering;

/// <summary>
/// Editor-only NES master palette used to convert NES color indices ($00-$3F)
/// to RGB preview colors.  It never changes ROM bytes.
/// </summary>
public static class NesDisplayPalette
{
    public const int ColorCount = 64;
    public const int RgbByteLength = ColorCount * 3;

    private static readonly Color[] DefaultColors = BuildDefaultPalette();
    private static Color[] _colors = DefaultColors.ToArray();

    public static bool IsCustom { get; private set; }
    public static string SourceName { get; private set; } = string.Empty;

    static NesDisplayPalette() => RestorePersisted();

    public static Color GetColor(int index) => _colors[index & 0x3F];

    public static bool TryLoadFile(string path, out string error)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            if (!TryDecode(bytes, out var colors, out error)) return false;
            Apply(colors, Path.GetFileName(path), persist: true, bytes);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static void ResetToDefault()
    {
        _colors = DefaultColors.ToArray();
        IsCustom = false;
        SourceName = string.Empty;
        try
        {
            var (pal, name) = PersistencePaths();
            if (File.Exists(pal)) File.Delete(pal);
            if (File.Exists(name)) File.Delete(name);
        }
        catch { }
    }

    public static byte[] GetRgbBytes()
    {
        var output = new byte[RgbByteLength];
        for (var i = 0; i < ColorCount; i++)
        {
            output[i * 3] = _colors[i].R;
            output[i * 3 + 1] = _colors[i].G;
            output[i * 3 + 2] = _colors[i].B;
        }
        return output;
    }

    private static bool TryDecode(ReadOnlySpan<byte> bytes, out Color[] colors, out string error)
    {
        colors = Array.Empty<Color>();
        if (bytes.Length != RgbByteLength)
        {
            error = $"NES emulator palette must be exactly {RgbByteLength} bytes (64 RGB colors); current size is {bytes.Length} bytes.";
            return false;
        }

        colors = new Color[ColorCount];
        for (var i = 0; i < ColorCount; i++)
            colors[i] = Color.FromArgb(bytes[i * 3], bytes[i * 3 + 1], bytes[i * 3 + 2]);
        error = string.Empty;
        return true;
    }

    private static void Apply(Color[] colors, string sourceName, bool persist, ReadOnlySpan<byte> originalBytes)
    {
        _colors = colors;
        IsCustom = true;
        SourceName = sourceName;
        if (!persist) return;

        try
        {
            var (pal, name) = PersistencePaths();
            Directory.CreateDirectory(Path.GetDirectoryName(pal)!);
            File.WriteAllBytes(pal, originalBytes.ToArray());
            File.WriteAllText(name, sourceName);
        }
        catch { }
    }

    private static void RestorePersisted()
    {
        try
        {
            var (pal, name) = PersistencePaths();
            if (!File.Exists(pal)) return;
            var bytes = File.ReadAllBytes(pal);
            if (!TryDecode(bytes, out var colors, out _)) return;
            var sourceName = File.Exists(name) ? File.ReadAllText(name).Trim() : "custom.pal";
            Apply(colors, string.IsNullOrWhiteSpace(sourceName) ? "custom.pal" : sourceName, persist: false, bytes);
        }
        catch
        {
            _colors = DefaultColors.ToArray();
            IsCustom = false;
            SourceName = string.Empty;
        }
    }

    private static (string PalettePath, string NamePath) PersistencePaths()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuarrelEx");
        return (Path.Combine(dir, "display-palette.pal"), Path.Combine(dir, "display-palette-name.txt"));
    }

    private static Color[] BuildDefaultPalette()
    {
        int[,] rgb =
        {
            {84,84,84},{0,30,116},{8,16,144},{48,0,136},{68,0,100},{92,0,48},{84,4,0},{60,24,0},{32,42,0},{8,58,0},{0,64,0},{0,60,0},{0,50,60},{0,0,0},{0,0,0},{0,0,0},
            {152,150,152},{8,76,196},{48,50,236},{92,30,228},{136,20,176},{160,20,100},{152,34,32},{120,60,0},{84,90,0},{40,114,0},{8,124,0},{0,118,40},{0,102,120},{0,0,0},{0,0,0},{0,0,0},
            {236,238,236},{76,154,236},{120,124,236},{176,98,236},{228,84,236},{236,88,180},{236,106,100},{212,136,32},{160,170,0},{116,196,0},{76,208,32},{56,204,108},{56,180,204},{60,60,60},{0,0,0},{0,0,0},
            {236,238,236},{168,204,236},{188,188,236},{212,178,236},{236,174,236},{236,174,212},{236,180,176},{228,196,144},{204,210,120},{180,222,120},{168,226,144},{152,226,180},{160,214,228},{160,162,160},{0,0,0},{0,0,0}
        };
        var colors = new Color[ColorCount];
        for (var i = 0; i < ColorCount; i++) colors[i] = Color.FromArgb(rgb[i, 0], rgb[i, 1], rgb[i, 2]);
        return colors;
    }
}
