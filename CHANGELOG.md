# Changelog

## QuarrelEx v1.1.6

### Desktop 1.1.6 / Web 1.6.6

- Drag-and-drop ROM opening with Save / Don't Save / Cancel.
- Shared QuarrelExConfig v3 with strict validation.
- Screen raw bytes support `$00-$FF`, preserving `$FF` string terminators.
- Stage 1-70 independent P1/P2 player spawn editing.
- Stage 1-70 custom 1-8 enemy spawn editing.
- Stage 1-70 EnemyPacing and BaseExists.
- Final GAME OVER Skip, Extra Life, 2P Win-Streak and Armor One-Hit.
- Mid City2 / Mid City2 PS compatibility IPS files.

### Current 32KB Runtime 6.9.2

- Correct next-stage terrain initialization.
- Skip ON no longer flashes the old stage map.
- Automatic flashing tanks stay at #4/#11/#18.
- Existing visible items are preserved when another flashing tank spawns.
- Demo is isolated from Stage-30 QXR overrides.
- Demo keeps the original near-HQ no-fire behavior even with Hold-B auto fire enabled.
