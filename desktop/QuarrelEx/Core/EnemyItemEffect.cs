namespace QuarrelEx.Core;

[Flags]
public enum EnemyItemEffect : byte
{
    None = 0,
    Helmet = 1 << 0,
    Clock = 1 << 1,
    Shovel = 1 << 2,
    Star = 1 << 3,
    Grenade = 1 << 4,
    Tank = 1 << 5,
    Pistol = 1 << 6,
    All = 0x7F
}
