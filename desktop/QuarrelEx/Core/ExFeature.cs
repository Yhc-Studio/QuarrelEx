namespace QuarrelEx.Core;

[Flags]
public enum ExFeature : byte
{
    None = 0,
    AutoFireB = 1 << 0,
    PistolLevel4 = 1 << 1,
    DowngradeOnHit = 1 << 2,
    PlayerFastMove = 1 << 3,
    RandomEnemySpawn = 1 << 4,
    Level4DestroyTrees = 1 << 5,
    EnemyPowerUpPickup = 1 << 6,
    NoFriendlyFire = 1 << 7,
}
