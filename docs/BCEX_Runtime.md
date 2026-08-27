# BCEX 32KB Runtime 6.9.3

Current QuarrelEx 32KB runtime: **Runtime 6.9.3 / QXR1 v5**.

## Current feature set

- 70 independent maps.
- Enemy totals up to 255.
- Extended terrain/TSA editing.
- Original player/enemy spawn editing.
- Stage 1-70 custom 1-8 enemy spawn points.
- Stage 1-70 independent P1/P2 player spawn positions.
- Stage 1-70 independent P1/P2 enemy spawn interval and maximum active count.
- Stage 1-70 BaseExists.
- Stage 1-70 EnemyCounterDisplay preference in PackedStageRules bit3; totals above 50 force Number.
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

## QXR1 v5 compact stage layout

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
