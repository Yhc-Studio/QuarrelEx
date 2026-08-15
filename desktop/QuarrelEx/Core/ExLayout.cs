namespace QuarrelEx.Core;

[Flags]
public enum ExLayout : byte
{
    None = 0,
    Independent70Maps = 1 << 0,
    CustomEnemyTotal = 1 << 1,
    ExtendedTerrain1F = 1 << 2,
    Terrain64 = 1 << 3,
    LockInitialState = 1 << 4,
    BonusReplaceAlways = 1 << 5,
    PlayerFastMove = 1 << 6,
}
