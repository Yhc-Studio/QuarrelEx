# Changelog

## QuarrelEx v1.1.8

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
- Mid City2 / Mid City2 PS compatibility IPS files.

### Current 32KB Runtime 6.9.3

- Correct next-stage terrain initialization.
- Skip ON no longer flashes the old stage map.
- Automatic flashing tanks stay at #4/#11/#18.
- Existing visible items are preserved when another flashing tank spawns.
- Demo is isolated from Stage-30 QXR overrides.
- Demo keeps the original near-HQ no-fire behavior even with Hold-B auto fire enabled.
