# BCEX runtime format

## ExFeatureFlags ($EF7A)

| Bit | Mask | Feature |
|---:|---:|---|
| 0 | `$01` | Hold B auto-fire |
| 1 | `$02` | Pistol + Lv4 system |
| 2 | `$04` | Downgrade one level when hit |
| 3 | `$08` | Faster player movement (r6.2+) |
| 4 | `$10` | Randomize enemy spawn order |
| 5 | `$20` | Lv4 bullets can remove forest |
| 6 | `$40` | Enemy can pick up power-ups (32KB) |
| 7 | `$80` | Disable friendly fire |

## EnemyItemFlags ($EF7B)

| Bit | Mask | Enemy power-up effect |
|---:|---:|---|
| 0 | `$01` | Helmet |
| 1 | `$02` | Clock |
| 2 | `$04` | Shovel |
| 3 | `$08` | Star |
| 4 | `$10` | Grenade |
| 5 | `$20` | Tank |
| 6 | `$40` | Pistol |
| 7 | `$80` | Lock initial player state after an actual death |

## LayoutFlags

| Bit | Meaning |
|---:|---|
| 0 | 70 independent maps |
| 1 | 1-255 custom enemy total |
| 2 | Extended terrain through `$1F` |
| 3 | 64 terrain entries through `$3F` |
| 4 | Locked initial state runtime support |
| 5 | Bonus tank always replaces the current power-up |
| 6 | Faster player movement runtime support |

Current 16KB runtime uses LayoutFlags `$76`; current 32KB runtime uses `$7F`.

## Notes

- 16KB maps retain legacy storage constraints; the 32KB format is the preferred long-term format for custom terrain and independent Stage 36-70 maps.
- 32KB runtime r6.4.1 fixes enemy Star steel-breaking behavior, enemy Grenade explosion SFX, and the erroneous branch into `$B54B`.
- Config v3 is unchanged by r6.3/r6.4.1.
