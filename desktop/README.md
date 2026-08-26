# QuarrelEx Desktop 1.1.4

C# / WinForms / .NET 8 desktop editor. Open `QuarrelEx.sln` in Visual Studio 2022.

- Drag-and-drop `.nes` ROM opening works on the main form and modeless tool windows. Dirty ROMs use the standard Yes / No / Cancel save prompt before replacement.

Desktop 1.1.4 supports the current BCEX 32KB Runtime 6.8 / QXR1 v4 in **Game Settings**, while remaining compatible with Runtime 6.5 Final Rules:
- Stage 1-70 custom enemy spawn editor with actual map preview, 1P/2P Original or 1-8, shared S1-S8 coordinates, 16px snap and `$18-$D8` safe bounds.
- Final GAME OVER Skip (default OFF).
- Original / Custom Once / Repeat / Disabled score extra-life modes.
- 2P Original / Win-Streak bonus.
- Armored Tank Original / One-Hit; Runtime 6.7+ One-Hit affects only normal 400-point armored tanks (white 1HP), not flashing/item armored tanks.
- Runtime 6.6: title A+B+Start cheat lives, independent P1/P2 values with default 10/10.
- Runtime 6.6: per-stage 1P/2P spawn interval and max-active enemy controls, Stage 35 preset and original-pacing restore.
- The original-spawn and S1-S8 map visualizers refresh immediately when the main stage map is edited.
- Config v3 stays Version 3; Runtime 6.6 fields remain optional and Runtime 6.7+ adds optional `Stages[].BaseExists`.

The existing Demo editing and the `F9` Title + Game Over Screen Editor remain included.

The public repository copy intentionally does not include artwork extracted from the original Quarrel executable. Windows will use a default application icon unless you add your own licensed icon.


### Runtime 6.7/6.8 / QXR1 v4

- Armored Tank One Hit now converts only normal 400-point armored tanks to the white 1-HP form; flashing/item armored tanks keep the original multi-hit durability and flashing/item behavior.
- Stage 1-70 now has optional `BaseExists` (Config v3 extension). When false, the runtime skips default/protected HQ drawing so map terrain underneath the HQ area remains active.
- Config schema remains Version 3; older v3 files remain compatible.
