namespace QuarrelEx.Config;

public sealed class EditorConfig
{
    public int NumberOfLevels { get; private set; } = 70;
    public int MapRows { get; private set; } = 13;
    public int MapColumns { get; private set; } = 13;
    public int StorageStrideNibbles { get; private set; } = 14;
    public int StageSize { get; private set; } = 0x5B;

    public int StageMapStart { get; private set; } = 0x308A;
    public int EnemyType1To35 { get; private set; } = 0x24FC;
    public int EnemyCount1To35 { get; private set; } = 0x2588;
    public int EnemyType36To70 { get; private set; } = 0x3D60;
    public int EnemyCount36To70 { get; private set; } = 0x3DEC;
    public int TerrainAttributes { get; private set; } = 0x3F00;
    public int TerrainBlocks { get; private set; } = 0x3F18;
    public int OriginalTerrainAttributes { get; private set; } = 0x1ACB;
    public int OriginalTerrainBlocks { get; private set; } = 0x1ADB;
    public int Terrain64Attributes { get; private set; } = 0x3410;
    public int Terrain64Blocks { get; private set; } = 0x3450;
    public int LevelPalette { get; private set; } = 0x1585;
    public int ExV2ConfigStart { get; private set; } = 0x2F85;

    // Original Quarrel-compatible data fields (file offsets include iNES header).
    public int StartingLives { get; private set; } = 0x02DE;
    public int InitialTankStatus { get; private set; } = 0x02CE;
    public int Enemy1X { get; private set; } = 0x2484;
    public int Enemy2X { get; private set; } = 0x2485;
    public int Enemy3X { get; private set; } = 0x2486;
    public int Enemy1Y { get; private set; } = 0x2487;
    public int Enemy2Y { get; private set; } = 0x2488;
    public int Enemy3Y { get; private set; } = 0x2489;
    public int Player1X { get; private set; } = 0x248A;
    public int Player2X { get; private set; } = 0x248B;
    public int Player1Y { get; private set; } = 0x248C;
    public int Player2Y { get; private set; } = 0x248D;
    public int FlagTsa { get; private set; } = 0x137D;
    public int FortTsa { get; private set; } = 0x1399;
    public int PaletteSpr { get; private set; } = 0x1565;
    public int PaletteFrame2 { get; private set; } = 0x1575;
    public int PaletteFrame1 { get; private set; } = 0x1595;
    public int TitleScrPalette { get; private set; } = 0x15A5;
    public int LevelSelPalette { get; private set; } = 0x15B5;
    public int PaletteMisc1 { get; private set; } = 0x15D5;
    public int PaletteMisc2 { get; private set; } = 0x15F5;

    public int ExV2MapStart { get; private set; } = 0x0010;
    public int ExV2MapStageStride { get; private set; } = 0x00B6;
    public int ExV2MapStageCount { get; private set; } = 70;

    public int ExpandedShift { get; private set; } = 0x4000;
    public int OverlayStart { get; private set; } = 0x0010;
    public int OverlayPageSize { get; private set; } = 0x0100;
    public int OverlayStageCount { get; private set; } = 35;
    public int HelperFileOffset { get; private set; } = 0x2310;
    public int StageDrawPatchExpanded { get; private set; } = 0x7064;

    public static EditorConfig LoadDefault()
    {
        var cfg = new EditorConfig();
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "BattleCityEx.ini");
        if (!File.Exists(path)) return cfg;

        try
        {
            var ini = IniFile.Load(path);
            cfg.NumberOfLevels = ini.GetDecimal("General", "NumberOfLevels", cfg.NumberOfLevels);
            cfg.MapRows = ini.GetDecimal("General", "MapRows", cfg.MapRows);
            cfg.MapColumns = ini.GetDecimal("General", "MapColumns", cfg.MapColumns);
            cfg.StorageStrideNibbles = ini.GetDecimal("General", "StorageStrideNibbles", cfg.StorageStrideNibbles);
            cfg.StageSize = ini.GetHex("General", "StageSize", cfg.StageSize);

            cfg.StageMapStart = ini.GetHex("Offsets16K", "StageMapStart", cfg.StageMapStart);
            cfg.EnemyType1To35 = ini.GetHex("Offsets16K", "EnemyType1To35", cfg.EnemyType1To35);
            cfg.EnemyCount1To35 = ini.GetHex("Offsets16K", "EnemyCount1To35", cfg.EnemyCount1To35);
            cfg.EnemyType36To70 = ini.GetHex("Offsets16K", "EnemyType36To70", cfg.EnemyType36To70);
            cfg.EnemyCount36To70 = ini.GetHex("Offsets16K", "EnemyCount36To70", cfg.EnemyCount36To70);
            cfg.TerrainAttributes = ini.GetHex("Offsets16K", "TerrainAttributes", cfg.TerrainAttributes);
            cfg.TerrainBlocks = ini.GetHex("Offsets16K", "TerrainBlocks", cfg.TerrainBlocks);
            cfg.OriginalTerrainAttributes = ini.GetHex("Original", "TerrainAttributes", cfg.OriginalTerrainAttributes);
            cfg.OriginalTerrainBlocks = ini.GetHex("Original", "TerrainBlocks", cfg.OriginalTerrainBlocks);
            cfg.Terrain64Attributes = ini.GetHex("Terrain64", "Attributes", cfg.Terrain64Attributes);
            cfg.Terrain64Blocks = ini.GetHex("Terrain64", "Blocks", cfg.Terrain64Blocks);
            cfg.LevelPalette = ini.GetHex("Offsets16K", "LevelPalette", cfg.LevelPalette);
            cfg.ExV2ConfigStart = ini.GetHex("ExV2", "ConfigStart", cfg.ExV2ConfigStart);

            cfg.StartingLives = ini.GetHex("GameSettings", "StartingLives", cfg.StartingLives);
            cfg.InitialTankStatus = ini.GetHex("GameSettings", "InitialTankStatus", cfg.InitialTankStatus);
            cfg.Enemy1X = ini.GetHex("Spawns", "Enemy1X", cfg.Enemy1X);
            cfg.Enemy2X = ini.GetHex("Spawns", "Enemy2X", cfg.Enemy2X);
            cfg.Enemy3X = ini.GetHex("Spawns", "Enemy3X", cfg.Enemy3X);
            cfg.Enemy1Y = ini.GetHex("Spawns", "Enemy1Y", cfg.Enemy1Y);
            cfg.Enemy2Y = ini.GetHex("Spawns", "Enemy2Y", cfg.Enemy2Y);
            cfg.Enemy3Y = ini.GetHex("Spawns", "Enemy3Y", cfg.Enemy3Y);
            cfg.Player1X = ini.GetHex("Spawns", "Player1X", cfg.Player1X);
            cfg.Player2X = ini.GetHex("Spawns", "Player2X", cfg.Player2X);
            cfg.Player1Y = ini.GetHex("Spawns", "Player1Y", cfg.Player1Y);
            cfg.Player2Y = ini.GetHex("Spawns", "Player2Y", cfg.Player2Y);
            cfg.FlagTsa = ini.GetHex("FlagTSA", "FlagTSA", cfg.FlagTsa);
            cfg.FortTsa = ini.GetHex("FlagTSA", "FortTSA", cfg.FortTsa);
            cfg.PaletteSpr = ini.GetHex("Palette", "PaletteSpr", cfg.PaletteSpr);
            cfg.PaletteFrame2 = ini.GetHex("Palette", "PaletteFrame2", cfg.PaletteFrame2);
            cfg.PaletteFrame1 = ini.GetHex("Palette", "PaletteFrame1", cfg.PaletteFrame1);
            cfg.TitleScrPalette = ini.GetHex("Palette", "TitleScrPalette", cfg.TitleScrPalette);
            cfg.LevelSelPalette = ini.GetHex("Palette", "LevelSelPalette", cfg.LevelSelPalette);
            cfg.PaletteMisc1 = ini.GetHex("Palette", "PaletteMisc1", cfg.PaletteMisc1);
            cfg.PaletteMisc2 = ini.GetHex("Palette", "PaletteMisc2", cfg.PaletteMisc2);
            cfg.ExV2MapStart = ini.GetHex("ExV2Maps", "MapStart", cfg.ExV2MapStart);
            cfg.ExV2MapStageStride = ini.GetHex("ExV2Maps", "StageStride", cfg.ExV2MapStageStride);
            cfg.ExV2MapStageCount = ini.GetDecimal("ExV2Maps", "StageCount", cfg.ExV2MapStageCount);

            cfg.ExpandedShift = ini.GetHex("Expanded", "Shift", cfg.ExpandedShift);
            cfg.OverlayStart = ini.GetHex("Expanded", "OverlayStart", cfg.OverlayStart);
            cfg.OverlayPageSize = ini.GetHex("Expanded", "OverlayPageSize", cfg.OverlayPageSize);
            cfg.OverlayStageCount = ini.GetDecimal("Expanded", "OverlayStageCount", cfg.OverlayStageCount);
            cfg.HelperFileOffset = ini.GetHex("Expanded", "HelperFileOffset", cfg.HelperFileOffset);
            cfg.StageDrawPatchExpanded = ini.GetHex("Expanded", "StageDrawPatchExpanded", cfg.StageDrawPatchExpanded);
        }
        catch
        {
            // Compiled defaults keep the editor usable if the INI is malformed.
        }
        return cfg;
    }
}
