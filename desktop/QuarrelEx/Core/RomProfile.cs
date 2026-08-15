namespace QuarrelEx.Core;

public sealed record RomProfile(
    BattleCityRomKind Kind,
    string DisplayName,
    int StageCount,
    int TerrainCount,
    bool SupportsCustomEnemyTotal,
    bool SupportsIndependent70Maps,
    bool SupportsEnemyPowerUpPickup,
    bool SupportsExtendedTerrain,
    bool SupportsTerrain64,
    bool HasOverlay)
{
    public bool IsOriginal => Kind == BattleCityRomKind.Original16K;
}
