# Changelog

## QuarrelEx v1.1.7

### Desktop 1.1.7 / Web 1.6.7

- Added per-stage `.qexstage.json` import/export for the 13x13 map plus referenced TSA/Attr terrain definitions.
- Added map selection editing: single-cell/rectangle select, drag move, Ctrl+drag copy, Ctrl+C/X/V, Delete and arrow-key nudge.
- Drag-and-drop ROM opening with Save / Don't Save / Cancel.
- Shared QuarrelExConfig v3 with strict validation.
- Screen raw bytes support `$00-$FF`, preserving `$FF` string terminators.
- Stage 1-70 independent P1/P2 player spawn editing.
- Stage 1-70 custom 1-8 enemy spawn editing.
- Stage 1-70 EnemyPacing and BaseExists.
- Per-stage Enemy counter display: Icons / Number; EnemyTotal > 50 forces Number.
- Final GAME OVER Skip, Extra Life, 2P Win-Streak and Armor One-Hit.
- Mid City2 / Mid City2 PS compatibility IPS files.

### Current 32KB Runtime 6.9.3

- Correct next-stage terrain initialization.
- Skip ON no longer flashes the old stage map.
- Automatic flashing tanks stay at #4/#11/#18.
- Existing visible items are preserved when another flashing tank spawns.
- Demo is isolated from Stage-30 QXR overrides.
- Demo keeps the original near-HQ no-fire behavior even with Hold-B auto fire enabled.
