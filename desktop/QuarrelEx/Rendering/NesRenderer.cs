using System.Drawing.Imaging;
using QuarrelEx.Core;

namespace QuarrelEx.Rendering;

public sealed class NesRenderer : IDisposable
{
    private readonly BattleCityRom _rom;
    private readonly Dictionary<(int Id, int Scale), Bitmap> _blockCache = new();
    private readonly Dictionary<(byte Tile, byte Attr, int Scale), Bitmap> _tileCache = new();
    private readonly Dictionary<(SpawnKind Kind, int Scale), Bitmap> _tankCache = new();
    private static readonly Color[] NesPalette = BuildPalette();

    public NesRenderer(BattleCityRom rom) => _rom = rom;

    public static Color GetNesColor(byte index) => NesPalette[index & 0x3F];

    public Bitmap GetBlockBitmap(int terrainId, int scale = 2)
    {
        var key = (terrainId, scale);
        if (_blockCache.TryGetValue(key, out var cached)) return cached;
        var bmp = RenderBlock(terrainId, scale);
        _blockCache[key] = bmp;
        return bmp;
    }

    public Bitmap GetChrTileBitmap(byte tile, byte attr, int scale = 4)
    {
        attr &= 3;
        var key = (tile, attr, scale);
        if (_tileCache.TryGetValue(key, out var cached)) return cached;
        var bmp = RenderChrTile(tile, attr, scale);
        _tileCache[key] = bmp;
        return bmp;
    }

    public Bitmap GetSpawnTankBitmap(SpawnKind kind, int scale = 2)
    {
        var key = (kind, scale);
        if (_tankCache.TryGetValue(key, out var cached)) return cached;

        // Battle City renders an upward-facing tank as two 8x16 sprites.
        // P1/P2 share the player CHR but use Sprite palettes 0/1; enemies use
        // the $80 tank tile family and an enemy Sprite palette.
        var baseTile = kind is SpawnKind.Player1 or SpawnKind.Player2 ? (byte)0x00 : (byte)0x80;
        var palette = kind switch
        {
            SpawnKind.Player1 => (byte)0,
            SpawnKind.Player2 => (byte)1,
            _ => (byte)2
        };

        var bmp = new Bitmap(16 * scale, 16 * scale, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        using var tl = RenderSpriteChrTile(baseTile, palette, scale);
        using var bl = RenderSpriteChrTile((byte)(baseTile + 1), palette, scale);
        using var tr = RenderSpriteChrTile((byte)(baseTile + 2), palette, scale);
        using var br = RenderSpriteChrTile((byte)(baseTile + 3), palette, scale);
        g.DrawImageUnscaled(tl, 0, 0);
        g.DrawImageUnscaled(bl, 0, 8 * scale);
        g.DrawImageUnscaled(tr, 8 * scale, 0);
        g.DrawImageUnscaled(br, 8 * scale, 8 * scale);
        _tankCache[key] = bmp;
        return bmp;
    }

    public void InvalidateCache()
    {
        foreach (var bmp in _blockCache.Values) bmp.Dispose();
        foreach (var bmp in _tileCache.Values) bmp.Dispose();
        foreach (var bmp in _tankCache.Values) bmp.Dispose();
        _blockCache.Clear();
        _tileCache.Clear();
        _tankCache.Clear();
    }

    private Bitmap RenderBlock(int terrainId, int scale)
    {
        var attr = (byte)(_rom.GetTerrainAttribute(terrainId) & 3);
        var tiles = _rom.GetTerrainTiles(terrainId);
        var bmp = new Bitmap(16 * scale, 16 * scale, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        for (var q = 0; q < 4; q++)
        {
            using var tile = new Bitmap(GetChrTileBitmap(tiles[q], attr, scale));
            g.DrawImageUnscaled(tile, (q & 1) * 8 * scale, (q >> 1) * 8 * scale);
        }
        return bmp;
    }

    private Bitmap RenderChrTile(byte tile, byte attr, int scale)
    {
        var pixels = DecodeTile(tile);
        var bmp = new Bitmap(8 * scale, 8 * scale, PixelFormat.Format32bppArgb);
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            var colorIndex = pixels[y * 8 + x];
            var paletteByte = _rom.GetLevelPaletteByte(attr * 4 + colorIndex) & 0x3F;
            var color = NesPalette[paletteByte];
            for (var sy = 0; sy < scale; sy++)
            for (var sx = 0; sx < scale; sx++)
                bmp.SetPixel(x * scale + sx, y * scale + sy, color);
        }
        return bmp;
    }

    private byte[] DecodeTile(byte tile) => DecodeTileFromOffset(_rom.BackgroundChrOffset, tile);


    private Bitmap RenderSpriteChrTile(byte tile, byte palette, int scale)
    {
        var pixels = DecodeTileFromOffset(_rom.SpriteChrOffset, tile);
        var bmp = new Bitmap(8 * scale, 8 * scale, PixelFormat.Format32bppArgb);
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            var colorIndex = pixels[y * 8 + x];
            var color = colorIndex == 0
                ? Color.Transparent
                : NesPalette[_rom.GetPaletteByte(PaletteKind.Sprite, palette * 4 + colorIndex) & 0x3F];
            for (var sy = 0; sy < scale; sy++)
            for (var sx = 0; sx < scale; sx++)
                bmp.SetPixel(x * scale + sx, y * scale + sy, color);
        }
        return bmp;
    }

    private byte[] DecodeTileFromOffset(int chrOffset, byte tile)
    {
        var start = chrOffset + tile * 16;
        var output = new byte[64];
        for (var y = 0; y < 8; y++)
        {
            var lo = _rom.GetChrByte(start + y);
            var hi = _rom.GetChrByte(start + y + 8);
            for (var x = 0; x < 8; x++)
            {
                var bit = 7 - x;
                output[y * 8 + x] = (byte)(((lo >> bit) & 1) | (((hi >> bit) & 1) << 1));
            }
        }
        return output;
    }

    private static Color[] BuildPalette()
    {
        int[,] rgb =
        {
            {84,84,84},{0,30,116},{8,16,144},{48,0,136},{68,0,100},{92,0,48},{84,4,0},{60,24,0},{32,42,0},{8,58,0},{0,64,0},{0,60,0},{0,50,60},{0,0,0},{0,0,0},{0,0,0},
            {152,150,152},{8,76,196},{48,50,236},{92,30,228},{136,20,176},{160,20,100},{152,34,32},{120,60,0},{84,90,0},{40,114,0},{8,124,0},{0,118,40},{0,102,120},{0,0,0},{0,0,0},{0,0,0},
            {236,238,236},{76,154,236},{120,124,236},{176,98,236},{228,84,236},{236,88,180},{236,106,100},{212,136,32},{160,170,0},{116,196,0},{76,208,32},{56,204,108},{56,180,204},{60,60,60},{0,0,0},{0,0,0},
            {236,238,236},{168,204,236},{188,188,236},{212,178,236},{236,174,236},{236,174,212},{236,180,176},{228,196,144},{204,210,120},{180,222,120},{168,226,144},{152,226,180},{160,214,228},{160,162,160},{0,0,0},{0,0,0}
        };
        var colors = new Color[64];
        for (var i = 0; i < 64; i++) colors[i] = Color.FromArgb(rgb[i,0], rgb[i,1], rgb[i,2]);
        return colors;
    }

    public void Dispose() => InvalidateCache();
}
