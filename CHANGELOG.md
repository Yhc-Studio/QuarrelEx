# Changelog

## Desktop 1.1.8 emulator-palette compile hotfix

- Fixed `CS1503` build errors in `NesRenderer.cs` by allowing `NesDisplayPalette.GetColor()` to accept NES color-index expressions as `int` and clamp them internally with `& 0x3F`.

## Runtime 6.9.4 / QXR1 v6 - Player Death Level

- Added an independent **Death Level** setting for player tanks while preserving the existing **Initial Tank Level**.
- At or below Death Level, the next hit destroys the player; above Death Level, the hit lowers the tank by one level and the player survives.
- Death Lv0 gives the classic downgrade-through-Lv0 behavior; Death Lv4 makes every level die in one hit.
- QXR1 is bumped from v5 to v6. CPU `$B56B` stores the Death Level cutoff; the player-hit helper is replaced in place at `$FFA6-$FFC5`, so the runtime remains a 32KB Mapper-0 build.
- Web 1.6.8 and Desktop 1.1.8 expose Death Level separately from Initial Tank Level. QuarrelExConfig remains v3 with optional `Gameplay.PlayerDeathLevel`.
- On Runtime 6.9.4, the legacy **Downgrade on Hit** checkbox is managed by Death Level to avoid conflicting rules; older runtimes retain the legacy checkbox behavior.
- Added the standard Runtime 6.9.4 IPS and a small Runtime 6.9.3 -> 6.9.4 incremental IPS.

### Web 1.6.8 compact Inspector / unified Spawn Settings

- Compressed TSA columns and reduced the wide-screen Inspector share so the left Map workspace receives more width while TL/TR/BL/BR remain visible.
- Merged the legacy/global spawn editor, Stage 1-70 player spawn editor, and custom enemy spawn editor into one Spawn Settings group with an Original / Custom editor-mode switch.
- Original mode shows the stock/global E1/E2/E3/P1/P2 editor; Custom mode shows the per-stage player and enemy spawn tools without changing ROM data merely by switching the UI mode.
- The Spawn editor mode automatically opens Custom when the selected stage already contains per-stage custom spawn data.

### Web 1.6.8 performance / map-workspace refinement

- Prioritized the complete 13x13 Map view and reduced the wide-screen Terrain tray to a shorter independent 170-220px scroll area.
- Added lazy Inspector rendering: hidden TSA, Palette, Game Settings/Spawn and Screen panels are invalidated and refreshed only when opened.
- Terrain selection no longer recreates all terrain preview canvases; existing buttons only update selection state and labels.
- Deferred map-dependent Settings/Spawn refresh until the paint stroke ends and only when Game Settings is visible.
- Reduced i18n MutationObserver work by excluding self-localized high-churn render roots.
- Removed the unused Bootstrap JavaScript runtime from the Web build while retaining Bootstrap 5.3.6 CSS and QuarrelEx native JS.
- Removed Inspector entry animation/backdrop-filter costs and reduced lazy-tab scheduling to one animation frame for faster response.

### Web / Mobile editor-shell refinement

- Web desktop layout now uses a fixed-height Map workspace with an independent Inspector/Properties panel on wide screens.
- The Inspector uses a vertical tool rail and independently scrollable property pane; compact widths fall back to the existing stacked layout.
- Mobile Map editing now uses a dedicated full-height workspace and an on-demand Terrain tray above the bottom navigation.
- Mobile property editors now scroll inside a stable page panel instead of scrolling the entire document.
- Improved overscroll, safe-area handling, tab keyboard navigation and viewport stability.
### Web UI framework / layout refinement
- Rebuilt the Web editor shell around Bootstrap 5 plus a QuarrelEx-specific CSS/JS design system while keeping the editor fully offline.
- Added a responsive two-row application toolbar, modern card surfaces, denser tab navigation, stable scroll areas, improved forms/tables/modals, and better focus/keyboard accessibility.
- Terrain palette now uses a bounded scroll surface on wide layouts to reduce page-length changes while map editing.
- Existing ROM editing, Config v3, i18n and drag/drop logic are unchanged.


### Web 1.6.8 layout / map-edit stability refinement

- Removed the accidental `?` prefix from the Chinese TSA guide button.
- Prevented map-canvas focus from scrolling the page while editing (`preventScroll`).
- Deferred map-dependent spawn/settings DOM refreshes until the end of a paint stroke and throttled canvas repainting with `requestAnimationFrame`.
- Stabilized page/header scroll geometry and reserved scrollbar space to avoid small editing-time layout jumps.
- Refined the desktop Web layout with a wider responsive CSS Grid, stable stage toolbar/note rows, responsive terrain cards, horizontally scrollable tabs, and more consistent panel spacing.

## QuarrelEx v1.1.8

- Web Editor Workbench: widened the Inspector/Properties column for TSA editing, compacted TSA cells so TL/TR/BL/BR fit better on desktop widths, and changed Tools/Help menus to exclusive hover-to-open desktop menus with touch/click fallback.

### Desktop 1.1.8 / Web 1.6.8 / Mobile Web UI

- Added unified UI localization keys shared by Desktop, Web, and Mobile Web.
- Added Simplified Chinese (`zh-CN`), English (`en-US`), and Japanese (`ja-JP`) UI languages.
- Language selection is stored separately from ROM/Config data; QuarrelExConfig v3 remains language-neutral and fully compatible across all three UI languages.
- Added Japanese Desktop Help / TSA Guide and localized dynamic status, validation, ROM-info, spawn, Final Rules, TSA, palette, and screen-editor text.
- Web and Mobile detect the browser language on first use and remember manual selection; Desktop follows the OS UI language on first use and remembers manual selection.
- Fixed Web/Mobile runtime language switching: repeated `{0}` placeholders no longer break i18n initialization, dynamic map/terrain labels update immediately, and locale lookup can re-bind text created in any of the three languages.
- Fixed Desktop runtime language switching and lazy tool-window localization; dynamic ROM-dependent labels are refreshed immediately after a language change.
- Desktop language catalogs are compiled into the application; the external `Locales` directory is now an optional override instead of a runtime requirement.
- Fixed the Desktop main-map canvas jumping far to the right after opening a ROM. The map/note container now has an explicit 100% column and a fixed one-line note row, so long localized map notes cannot enlarge the hidden layout width used for canvas centering; the canvas remains top-left anchored with one manual bounds calculation. Terrain buttons also adapt to the available width to avoid horizontal scrolling.
- Web: moved **Clear Current Stage** and **Clear All Stages** next to **Export Stage** in the stage toolbar for faster map-editing access.

## QuarrelEx v1.1.7

### Mobile Web UI 1.0 (Core Web 1.6.7)

- Added a separate `web/QuarrelEx_Mobile.html`; the existing desktop-oriented Web HTML files are not merged or replaced.
- Added phone-first top actions and fixed bottom navigation for Map / Enemy / TSA / Settings / More.
- Added a mobile bottom tool drawer for Palette, Flag TSA, Ex options, Screen Editor, ROM Info, Config/Stage import-export and Help.
- Reuses the same Web 1.6.7 ROM core, QuarrelExConfig v3, QuarrelExStage v1, Runtime 6.9.3 support and map-selection workflow.
- Added touch-size controls, safe-area padding and single-column mobile layouts.

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

### Current 32KB Runtime 6.9.3

- Correct next-stage terrain initialization.
- Skip ON no longer flashes the old stage map.
- Automatic flashing tanks stay at #4/#11/#18.
- Existing visible items are preserved when another flashing tank spawns.
- Demo is isolated from Stage-30 QXR overrides.
- Demo keeps the original near-HQ no-fire behavior even with Hold-B auto fire enabled.

### Web terrain palette refinement
- Made Terrain cards more compact and increased the number of columns in the fixed Web workbench, while restoring a slightly taller terrain tray for easier browsing.
- Custom Terrain IDs `$20-$3F` can now be renamed in the Web editor by double-clicking or right-clicking a terrain card.
- Custom terrain names are editor metadata only: they do not modify ROM bytes. Names persist in browser storage and are exported/imported through optional `QuarrelExConfig v3` `EditorMetadata.CustomTerrainNames`.


### Editor display palette files
- Web and Desktop can load a standard 192-byte NES emulator `.pal` file (64 RGB colors) as the editor display palette.
- The loaded display palette recolors map, Terrain/TSA/CHR previews, spawn sprites and NES color pickers without modifying ROM palette indices or other ROM data.
- Added a Reset Default action. Desktop persists a private copy under LocalAppData; Web persists the 192-byte RGB data in localStorage.
- Display palette selection is an editor preference and is intentionally not stored in QuarrelExConfig v3.
