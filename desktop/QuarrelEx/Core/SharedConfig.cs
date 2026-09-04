namespace QuarrelEx.Core;

public sealed class QuarrelExSharedConfig
{
    public string Schema { get; set; } = "QuarrelExConfig";
    // Formal release: only Config v3 is imported/exported.
    // v3 contains global gameplay settings, palettes, TSA/Flag TSA and complete stage data.
    public int Version { get; set; } = 3;
    public GameplayConfig Gameplay { get; set; } = new();
    public Dictionary<string, int[]> Palettes { get; set; } = new();
    public List<TerrainDefinitionConfig> Terrain { get; set; } = new();
    public FlagTsaConfig FlagTsa { get; set; } = new();
    public List<StageConfig> Stages { get; set; } = new();
    // Optional v3 extensions. Older v3 files without these fields remain valid.
    public DemoConfig? Demo { get; set; }
    public ScreensConfig? Screens { get; set; }
}


public sealed class QuarrelExStagePackage
{
    public string Schema { get; set; } = "QuarrelExStage";
    public int Version { get; set; } = 1;
    // Informational only. Import always targets the stage currently selected in the editor.
    public int SourceStage { get; set; }
    public int[][] Map { get; set; } = Enumerable.Range(0, 13).Select(_ => new int[13]).ToArray();
    // Only terrain definitions referenced by Map are included. TSA/Attr tables are ROM-global,
    // so importing these definitions may also change other stages that use the same terrain IDs.
    public List<TerrainDefinitionConfig> Terrain { get; set; } = new();
}

public sealed class GameplayConfig
{
    public int StartingLives { get; set; } = 3;
    public int InitialTankLevel { get; set; }
    // Optional Runtime 6.9.4 / QXR1 v6 extension. Null preserves legacy DowngradeOnHit semantics.
    public int? PlayerDeathLevel { get; set; }
    public bool LockInitialState { get; set; }
    public bool? PlayerFastMove { get; set; }
    public int? FeatureFlags { get; set; }
    public int? EnemyItemFlags { get; set; }
    public Dictionary<string, SpawnPointConfig> Spawns { get; set; } = new();
    // Optional Config v3 extension used by BCEX 32KB runtime 6.5 Final Rules.
    // Older v3 files omit this field and keep the target ROM's current values.
    public FinalRulesConfig? FinalRules { get; set; }
}

public sealed class FinalRulesConfig
{
    public bool SkipFinalGameOver { get; set; }
    public int ExtraLifeMode { get; set; }
    public int ExtraLifeValue { get; set; } = 2;
    public int TwoPlayerBonusMode { get; set; }
    public int ArmoredTankMode { get; set; }
    // Optional Runtime 6.6 / QXR1 v3 extension. Null keeps the target ROM value.
    public int? CheatPlayer1Lives { get; set; }
    public int? CheatPlayer2Lives { get; set; }
}

public sealed class EnemySpawnConfig
{
    public int Player1Count { get; set; }
    public int Player2Count { get; set; }
    public List<SpawnPointConfig> Points { get; set; } = Enumerable.Range(0, 8)
        .Select(_ => new SpawnPointConfig()).ToList();
}

public sealed class EnemyPacingConfig
{
    public int Player1IntervalFrames { get; set; }
    public int Player2IntervalFrames { get; set; }
    public int Player1MaxActive { get; set; }
    public int Player2MaxActive { get; set; }
}

public sealed class StagePlayerSpawnConfig
{
    // null = use the original/global player spawn position.
    public SpawnPointConfig? Player1 { get; set; }
    public SpawnPointConfig? Player2 { get; set; }
}

public sealed class SpawnPointConfig
{
    public int X { get; set; }
    public int Y { get; set; }
}

public sealed class TerrainDefinitionConfig
{
    public int Id { get; set; }
    public int Attr { get; set; }
    public int[] Tiles { get; set; } = [0, 0, 0, 0];
}

public sealed class FlagTsaConfig
{
    public int[] Flag { get; set; } = new int[24];
    public int[] Fort { get; set; } = new int[24];
}

public sealed class StageConfig
{
    public int Stage { get; set; }
    // 13 rows x 13 columns. Nested rows make manual inspection/editing easier than a flat 169-byte array.
    public int[][] Map { get; set; } = Enumerable.Range(0, 13).Select(_ => new int[13]).ToArray();
    // Raw enemy type bytes. Common base values are $80/$A0/$C0/$E0;
    // bit2 ($04) is the flashing/bonus flag, so $84/$A4/$C4/$E4 are forced-flashing variants.
    public int[] EnemyTypes { get; set; } = new int[4];
    public int[] EnemyCounts { get; set; } = new int[4];
    // Redundant by design for integrity checking. Must equal sum(EnemyCounts).
    public int EnemyTotal { get; set; }
    // Optional Config v3 extension for per-stage 1P/2P custom enemy spawn points.
    public EnemySpawnConfig? EnemySpawn { get; set; }
    // Optional Runtime 6.6 / QXR1 v3 per-stage enemy appearance pacing.
    public EnemyPacingConfig? EnemyPacing { get; set; }
    // Optional Runtime 6.7 / QXR1 v4 extension. When false the runtime does not
    // draw/protect the HQ, so the 13x13 map data underneath remains active.
    public bool? BaseExists { get; set; }
    // Optional Runtime 6.9.3 / QXR1 v5 extension. Stored preference is "Icons" or "Number".
    // Runtime forces Number when EnemyTotal > 50 without overwriting this preference.
    public string? EnemyCounterDisplay { get; set; }
    // Optional Runtime 6.9.3 / QXR1 v5 extension. A null Player1/Player2 inside
    // this object means that player uses the original/global spawn position.
    // If PlayerSpawn itself is absent, older Config v3 files preserve the target ROM.
    public StagePlayerSpawnConfig? PlayerSpawn { get; set; }
}

public sealed class ConfigValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool IsValid => Errors.Count == 0;

    public string FormatErrors()
        => Errors.Count == 0
            ? string.Empty
            : "配置文件检查失败：" + Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", Errors);
}


public sealed class DemoConfig
{
    public int[][] Map { get; set; } = Enumerable.Range(0, 13).Select(_ => new int[13]).ToArray();
}

public sealed class ScreensConfig
{
    public ScreenLayoutConfig? Title { get; set; }
    public ScreenLayoutConfig? GameOver { get; set; }
}

public sealed class ScreenLayoutConfig
{
    // Keys are stable ROM-native element names, not absolute addresses.
    public Dictionary<string, int[]> Elements { get; set; } = new();
}
