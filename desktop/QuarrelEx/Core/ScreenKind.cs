namespace QuarrelEx.Core;

public enum ScreenKind
{
    Title,
    GameOver
}

public enum ScreenElementKind
{
    TileString,
    LargeGlyphString
}

public sealed record ScreenElementDefinition(
    string Key,
    string DisplayName,
    int FileOffset16K,
    int Length,
    int X,
    int Y,
    ScreenElementKind Kind,
    PaletteKind Palette);
