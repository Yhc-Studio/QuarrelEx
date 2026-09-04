# QuarrelEx Web 1.6.8

- `QuarrelEx.html`: self-contained, offline desktop-oriented Web editor.
- `QuarrelEx_Mobile.html`: separate touch-first Mobile UI (Mobile UI 1.0, core Web 1.6.8).
- Current runtime target: **Runtime 6.9.4 / QXR1 v6**.
- Full-project format: **QuarrelExConfig v3** (`*.qexcfg.json`).


## Localization

- Shared UI keys come from the repository-level `locales/` directory.
- Supported languages: Simplified Chinese (`zh-CN`), English (`en-US`), Japanese (`ja-JP`).
- Web/Mobile choose the browser language on first use and save manual selection in `localStorage`.
- Run `python tools/sync_i18n.py` from the repository root after editing locale JSON files.
- Run `python tools/check_i18n.py` before packaging.

## Mobile UI

`QuarrelEx_Mobile.html` is intentionally a separate HTML entry rather than a responsive merge of the desktop page. It reuses the same editor core and file formats but presents a phone-first shell:

- Compact top bar for Open / Save / Undo / Redo / More.
- Fixed bottom navigation: Map / Enemy / TSA / Settings / More.
- Bottom tool drawer for Palette, Flag TSA, Ex options, Screen Editor, ROM Info, Config and Stage import/export, and Help.
- Larger touch targets and single-column forms/tables for narrow screens.
- Safe-area padding for modern phones.
- `QuarrelEx.html` is the desktop-oriented Web entry and shares the same localization catalogs with the Mobile UI.

## New stage editing workflow

- The selected stage can be exported/imported independently as `.qexstage.json` (`QuarrelExStage` v1).
- A stage package contains only the current 13x13 map and the TSA/Attr definitions actually referenced by that map.
- Choose **Select / Move** above the terrain list to enter map-selection mode.
- Click one cell or drag a rectangle; drag a selection to move it; Ctrl+drag copies it.
- With the map focused: Ctrl+C / Ctrl+X / Ctrl+V copy, cut and paste; Delete/Backspace clears to `$0D`; arrow keys move the selection.
- The clipboard can be pasted after switching stages.
- Right-clicking the map picks a terrain and returns to paint mode.

> TSA/Attr terrain definitions are ROM-global. Importing a stage package may change other stages that use the same terrain IDs; the importer warns before committing.

## Other current features

Web 1.6.8 retains Stage 1-70 independent maps on the current 32KB runtime, custom player/enemy spawns, EnemyPacing, BaseExists, Enemy Counter Icons/Number, Final Rules, configurable Initial Tank Level and independent Death Level, Demo editing/isolation, Title + Game Over Screen Editor, drag-and-drop ROM opening, Save / Save As and Undo / Redo.

## Modern Web UI

The v1.6.8 Web shell uses Bootstrap 5.3.6 CSS together with QuarrelEx-specific native JavaScript. Bootstrap CSS is embedded directly into `QuarrelEx.html`, so the editor remains fully offline and self-contained. The Bootstrap JavaScript bundle is intentionally not loaded because QuarrelEx implements its editor tabs, menus and modals directly; this reduces startup and interaction overhead. The Bootstrap license is retained under `web/vendor/bootstrap/`; the redundant external CSS copy is not committed because the stylesheet is already embedded in the Web editor.

The modern shell adds a responsive two-column workspace, card-based editor surfaces, scroll-stable terrain palette, compact tab navigation, improved form/table styling, keyboard-accessible tab switching, and reduced-motion support without changing ROM editing logic.

## Editor-style workbench

On wide desktop screens the Web editor now behaves more like a desktop creative tool: the Map workspace stays in the left work area while the right side is an independent Inspector/Properties surface with a vertical tool rail. Both sides scroll independently, so switching or editing properties does not move the whole page. At compact widths the UI falls back to the stacked card layout.

The Mobile UI keeps its separate touch-first shell. Map editing uses the main viewport, Terrain opens in an on-demand bottom tray, and Enemy/TSA/Settings/other property pages scroll inside a stable inspector page between the fixed header and bottom navigation.


## Performance-oriented rendering

The desktop Web shell keeps heavy Inspector pages lazy. TSA, Palette, spawn/settings and Screen UI are rendered only when their Inspector page becomes visible, and hidden pages are invalidated rather than rebuilt after every edit. Terrain selection updates CSS state without recreating all terrain preview canvases. The i18n observer also skips large render roots whose strings are localized directly by the renderer.

On wide screens the 13x13 map is given layout priority; the Terrain palette uses a shorter independently scrolling tray so the map remains fully visible.


## Custom terrain names

In 64-terrain BCEX ROMs, Terrain IDs `$20-$3F` can be renamed in the Web editor by double-clicking or right-clicking a Terrain card. The names are editor metadata only and do not change ROM bytes. They are cached in browser storage and may also be carried by the optional `EditorMetadata.CustomTerrainNames` field in QuarrelExConfig v3.


## Emulator display palette (.pal)
The Web editor can load a standard 192-byte NES emulator palette file (`64 colors × RGB`) from the Palette inspector. This changes only editor preview colors; ROM palette indices and ROM bytes are untouched. The selected display palette is remembered in browser localStorage and can be reset to the built-in QuarrelEx palette at any time.
