# BCEX 32KB Runtime 6.9.4

Current QuarrelEx 32KB runtime: **Runtime 6.9.4 / QXR1 v6**.

## Runtime 6.9.4 addition

QXR1 v6 separates the player's starting level from the level at which a hit becomes fatal.

- `InitialTankLevel` remains the existing Lv0-Lv4 starting/respawn base level.
- `PlayerDeathLevel` is new in Runtime 6.9.4 and is independent from InitialTankLevel.
- At a level **greater than** Death Level, a hit downgrades one level and the player survives.
- At a level **less than or equal to** Death Level, a hit destroys the player immediately.

Examples:

```text
Initial Lv3 / Death Lv2
Lv3 -> hit -> Lv2 survives
Lv2 -> hit -> destroyed

Initial Lv4 / Death Lv0
Lv4 -> Lv3 -> Lv2 -> Lv1 -> Lv0 -> destroyed
```

`Death Lv4` makes every player tank level die in one hit.

## QXR1 v6 global extension

```text
$B55F-$B562  "QXR1"
$B563        QXR1 version = $06
$B564        Final Rules flags
$B565        Extra-life mode
$B566        Extra-life value
$B567        2P bonus mode
$B568        Armored-tank mode
$B569        A+B+Start P1 lives
$B56A        A+B+Start P2 lives
$B56B        Player death cutoff (Runtime 6.9.4)
$B56C-$B56F  reserved
$B570...     custom enemy spawn records
```

Death cutoff encoding:

```text
Death Lv0 -> $20
Death Lv1 -> $40
Death Lv2 -> $60
Death Lv3 -> $63
Death Lv4 -> $64
```

The player-hit helper at CPU `$FFA6-$FFC5` compares the current raw tank state against `$B56B`. This replacement fits in the existing 32-byte helper area, so Runtime 6.9.4 remains a 32KB PRG Mapper-0 build.

## Existing feature set retained

- 70 independent maps.
- Enemy totals up to 255.
- Extended terrain/TSA editing.
- Original player/enemy spawn editing.
- Stage 1-70 custom 1-8 enemy spawn points.
- Stage 1-70 independent P1/P2 player spawn positions.
- Stage 1-70 independent P1/P2 enemy spawn interval and maximum active count.
- Stage 1-70 BaseExists.
- Stage 1-70 EnemyCounterDisplay preference; totals above 50 force Number.
- A+B+Start configurable cheat lives.
- Final GAME OVER Skip.
- Score extra-life modes.
- 2P Original / Win-Streak.
- Armored Original / One-Hit.
- Automatic flashing tanks stay at spawn #4/#11/#18 even when EnemyTotal > 20.
- A new flashing tank does not forcibly remove an existing item.
- Correct next-stage terrain/setup state.
- Skip ON uses the correct GAME OVER cleanup path and does not flash the stage map.
- Demo uses original/global player spawn, original enemy spawn cycle and original Stage-30 pacing.
- Demo preserves the original near-HQ no-fire behavior even when Hold-B auto fire is enabled.

## QXR1 v5/v6 compact stage layout

QXR1 v6 retains the v5 stage layout unchanged:

```text
$BE60-$BEA5  Stage 1-70 1P enemy interval
$BEA6-$BEEB  Stage 1-70 2P enemy interval
$BEEC-$BF31  Packed stage rules
$BF32-$BF77  Stage 1-70 P1 player spawn
$BF78-$BFBD  Stage 1-70 P2 player spawn
```

Packed stage rule:

```text
bits 7-5 = 1P enemy limit (MaxActive + 1)
bit  4   = BaseExists
bit  3   = EnemyCounterDisplay (0 Icons, 1 Number)
bits 2-0 = 2P enemy limit (MaxActive + 1)
```

Player spawn byte:

```text
$FF = Original/global spawn
high nibble = X grid index 0..12
low nibble  = Y grid index 0..12
X/Y = $18 + index * $10
```

Config remains **QuarrelExConfig v3**.
