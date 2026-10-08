# BCEX 32KB Final Runtime

This document describes the current maintained 32KB BCEX runtime used by QuarrelEx 1.1.10, Web 1.6.14 and Mobile 1.1.1.

## ROM format

- Mapper 0 / NROM-256
- PRG: 32KB
- CHR: 8KB
- 70 independent stages
- QuarrelExConfig v3

## Random Enemy

When **Random Enemy** is enabled, every spawned enemy independently selects one of the four configured Type slots and receives HP 1–8. The four Count fields determine the stage total rather than fixed final per-Type quotas.

The visible armor stage follows the real remaining HP so all four base enemy types can visibly behave as armored:

- HP 1: no armor stage
- HP 2–3: intermediate armor stages
- HP 4–8: highest armor stage until HP drops below 4

When Random Enemy is disabled, the normal configured Type/Count and armor rules remain in effect.

## Enemy Pistol

Enemy Pistol pickup keeps visible armor and real durability synchronized. Pistol-enhanced enemies also keep steel-destroying bullets as their armor state changes.

## HQ destroyed / forest destruction

`Gameplay.KeepTreeDestroyAfterBaseDestroyed` is an optional Config v3 boolean. When enabled, Lv4 forest destruction remains available after the HQ/base is destroyed.

## Gameplay options

The maintained 32KB runtime includes:

- 70 independent maps
- Enemy total 1–255
- Stage 1–70 enemy spawn points and P1/P2 player spawn positions
- Per-stage 1P/2P pacing
- Per-stage Base Exists and enemy-counter display mode
- Per-stage automatic bonus-tank Start / Interval / Count
- Player/enemy spawn and Helmet shield durations
- Shield-aware grenade behavior
- Enemy 1UP add count
- 100/200/300/400 enemy bullet-speed profiles
- Native 8-slot item drop tables
- Initial Tank Level and independent Death Level Lv0–Lv4
- A+B+Start lives, score extra-life rules, 2P rules and GAME OVER skip
- Configurable forest destruction after HQ loss

## Compatibility identifiers

The editor recognizes the maintained gameplay-extension and final-rules signatures used by current BCEX 32KB ROMs. These identifiers are implementation details and do not change the public patch/config format.

## Current IPS

```text
patches/32KB/QuarrelEx_BCEX_32KB_Final.ips
```

Input:

```text
Size:    40976 bytes
CRC32:   D2572735
SHA-256: 5e0b53e33a40166e5d31d50734c6059c96bc061301d2b77e85bd10a744e53291
```

Output:

```text
Size:    40976 bytes
CRC32:   E33746DA
SHA-256: ab85c29a8ee0dd68078d5fce959147e6400105e1f55fdda975e13ce9c7c05aa8
```

No ROM image is distributed in this repository.
