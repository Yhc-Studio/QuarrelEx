# Changelog

## 1.2.0

### Desktop Modern UI

- Rebuilt the Desktop shell around a persistent left navigation and card-style map / terrain workspace.
- Refined the navigation into clearly separated Editing / Gameplay / Information groups, widened the sidebar, shortened long labels, and introduced fixed icon/text columns so every row shares the same baseline and spacing.
- Added a Fluent-inspired light theme, Microsoft YaHei UI-first typography, flat controls, modern tool-window headers and updated grid styling.
- Simplified the top command bar while retaining stage selection, ROM open/save, undo/redo and stage package actions.
- Redesigned Game Settings into five independent pages: Basic, Player Spawn, Enemy & Pacing, Rules & Runtime, and Maintenance.
- Removed the single very long settings scroll surface that could trigger expensive WinForms layout/scroll feedback.
- Preserves the selected settings page and each page's scroll position across resize and data refresh operations.
- Keeps the existing modeless tool-window workflow and all current BCEX 32KB / Config v3 features.
- No third-party UI framework is required; Desktop remains a stock .NET 8 WinForms project.

## 1.1.10

### Editors

- Web updated to 1.6.14 and Mobile Web updated to 1.1.1.
- Desktop, Web and Mobile share the current Config v3 model and maintained BCEX 32KB options.
- Mobile wide-screen layout now keeps Enemy, TSA, Settings and tool pages at full content width.
- Web/Desktop ROM detection accepts the current maintained gameplay-extension layout.
- Fixed Desktop C# build blockers in bonus-tank cadence and ROM information/export code.
- Stabilized Desktop Game Settings scrolling: mouse-wheel input over numeric/combo fields now scrolls the page instead of triggering repeated setting writes, resize keeps the current viewport, and redundant full-page refreshes were removed.

### BCEX 32KB

- Random Enemy now uses independent Type + HP 1–8 with visible armor-stage synchronization.
- Enemy Pistol pickup keeps armor durability synchronized and preserves steel-destroying bullets.
- Added configurable Lv4 forest destruction after HQ/base destruction.
- Per-stage bonus-tank cadence supports Start / Interval / Count.
- Existing shield, grenade, enemy 1UP, item-drop, bullet-speed, Max Stage and stage-rule features are retained.

### Repository

- Consolidated the 32KB distribution to one current release IPS.
- Removed historical incremental patches and per-build development notes from the source tree.
- Reorganized release-facing documentation around current supported features and formats.

## Earlier 1.1.x releases

Earlier releases established the 70-map BCEX layout, Config v3, Desktop/Web localization, TSA/CHR/palette tools, stage packages, spawn/pacing rules and screen editing.
