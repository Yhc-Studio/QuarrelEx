# QuarrelEx Web 1.6.8

- `QuarrelEx.html`: regular build.
- `QuarrelEx_Standalone.html`: byte-identical single-file desktop-oriented build.
- `QuarrelEx_Mobile.html`: separate touch-first Mobile UI (Mobile UI 1.0, core Web 1.6.8).
- Current runtime target: **Runtime 6.9.3 / QXR1 v5**.
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
- The normal `QuarrelEx.html` and `QuarrelEx_Standalone.html` remain the desktop-oriented entries and share the same localization catalogs.

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

Web 1.6.8 retains Stage 1-70 independent maps on the current 32KB runtime, custom player/enemy spawns, EnemyPacing, BaseExists, Enemy Counter Icons/Number, Final Rules, Demo editing/isolation, Title + Game Over Screen Editor, drag-and-drop ROM opening, Save / Save As and Undo / Redo.
