# QuarrelEx Desktop 1.1.7

C# / WinForms / .NET 8 desktop editor. Open `QuarrelEx.sln` in Visual Studio 2022.

Current editor/runtime target: **QuarrelEx 1.1.7 / Runtime 6.9.3 / QXR1 v5**.

## Editor workflow

- Drag-and-drop `.nes` ROM opening with Save / Don't Save / Cancel protection for dirty ROMs.
- Save / Save As and 50-step Undo / Redo history.
- Stage 1-70 map editing plus Demo.
- Per-stage `.qexstage.json` import/export. A stage package contains the current 13x13 map plus the TSA/Attr terrain definitions actually referenced by that map.
- The terrain list starts with **Select / Move**. In selection mode you can click one cell, drag a rectangle, drag the selection to move it, Ctrl+drag to copy, or use Ctrl+C / Ctrl+X / Ctrl+V / Delete and the arrow keys.
- Right-clicking the map picks a terrain and returns to normal paint mode.
- Full-project Desktop/Web interchange remains `QuarrelExConfig v3` (`*.qexcfg.json`).

> Terrain TSA/Attr definitions are ROM-global. Importing a `.qexstage.json` package updates the terrain IDs carried by that package and can therefore affect other stages using the same IDs.

## Runtime 6.9.3 / QXR1 v5

The editor includes the current Stage 1-70 player/enemy spawn controls, EnemyPacing, BaseExists, Enemy Counter Icons/Number, Final GAME OVER Skip, Extra Life rules, 2P Win-Streak, Armored Tank One-Hit, configurable A+B+Start lives, Demo isolation fixes and Runtime 6.9.3 corrections.

The existing Demo editor and `F9` Title + Game Over Screen Editor remain included.

The public repository intentionally does not include copyrighted Battle City ROM images or artwork extracted from the original Quarrel executable.
