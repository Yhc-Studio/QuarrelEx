# QuarrelEx Web 1.6.4

- `QuarrelEx.html`: regular build.
- `QuarrelEx_Standalone.html`: byte-identical single-file build.

Config format: QuarrelExConfig v3.

- Drag-and-drop `.nes` ROM opening is supported. Dirty ROMs use an in-app Save / Don't Save / Cancel prompt before replacement.

Web 1.6.4 supports the current BCEX 32KB Runtime 6.8 / QXR1 v4 while retaining Runtime 6.5 compatibility:
- Stage 1-70 custom enemy spawn editor with real-map overlay; 1P/2P choose Original or 1-8 shared S1-S8 coordinates.
- 16px snap, `$18-$D8` safe bounds, and non-empty-terrain warnings.
- Final GAME OVER Skip (default OFF), score extra-life mode/value, 2P Original/Win-Streak bonus, and Armored Tank Original/One-Hit (Runtime 6.7+ excludes flashing/item armored tanks from One-Hit).
- Runtime 6.6: title A+B+Start cheat lives, independently configurable for P1/P2 (default 10/10).
- Runtime 6.6: Stage 1-70 independent 1P/2P enemy spawn interval and maximum simultaneous enemies, with Stage 35 preset/original restore.
- Editing the current map immediately refreshes both map-backed spawn visualizers in Game Settings.
- Config v3 stays Version 3: Runtime 6.6 optional cheat-life/`EnemyPacing` fields remain, and Runtime 6.7+ adds optional `Stages[].BaseExists`.

Demo and the unified Title + Game Over Screen Editor from the previous Web release remain included.


### Runtime 6.7/6.8 / QXR1 v4

- Armored Tank One Hit now converts only normal 400-point armored tanks to the white 1-HP form; flashing/item armored tanks keep the original multi-hit durability and flashing/item behavior.
- Stage 1-70 now has optional `BaseExists` (Config v3 extension). When false, the runtime skips default/protected HQ drawing so map terrain underneath the HQ area remains active.
- Config schema remains Version 3; older v3 files remain compatible.
