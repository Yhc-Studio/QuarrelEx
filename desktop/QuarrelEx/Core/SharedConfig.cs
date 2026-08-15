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
}

public sealed class GameplayConfig
{
    public int StartingLives { get; set; } = 3;
    public int InitialTankLevel { get; set; }
    public bool LockInitialState { get; set; }
    public bool? PlayerFastMove { get; set; }
    public int? FeatureFlags { get; set; }
    public int? EnemyItemFlags { get; set; }
    public Dictionary<string, SpawnPointConfig> Spawns { get; set; } = new();
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
