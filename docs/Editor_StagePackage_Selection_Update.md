# v1.1.7 Editor Update: Stage Packages + Map Selection

This update is editor-only. Runtime 6.9.3 / QXR1 v5 and all IPS files are unchanged.

## Per-stage package

- Extension: `*.qexstage.json`
- Schema: `QuarrelExStage`
- Version: `1`
- Contains: one 13x13 map plus the TSA/Attr definitions referenced by that map.
- Import target: the stage currently selected in the editor.
- Import is transactional: validate/apply on a temporary ROM first, then commit as one Undo step.
- Terrain definitions are ROM-global; the UI warns that importing a terrain ID can affect other stages using the same ID.

## Map selection workflow

Choose **Select / Move** instead of a terrain:

- Click: select one cell.
- Drag empty area: rectangle selection.
- Drag selection: move.
- Ctrl+drag: copy.
- Ctrl+C / Ctrl+X / Ctrl+V: copy/cut/paste.
- Delete / Backspace: clear selection to terrain `$0D`.
- Arrow keys: move selection by one cell.
- Clipboard remains available after switching stages.
- Right-click map: pick terrain and return to paint mode.

## Validation performed

Web v1.6.7 was loaded with the Runtime 6.9.3 test ROM in a local headless Chromium page. The following flow passed:

1. Open ROM.
2. Export Stage 1 package.
3. Parse exported JSON and verify `Schema`, `Version`, exact 13x13 dimensions and one terrain definition for every terrain ID used by the map.
4. Enter Select / Move mode.
5. Select one cell and copy it.
6. Paste it at another cell.
7. Undo the paste.
8. Re-import the exported stage package and accept the global-terrain warning.
9. Confirm successful import with no JavaScript page/console errors.

Both Web HTML files are byte-identical and all inline JavaScript blocks pass `node --check`.

The packaging container does not have the .NET SDK installed. Desktop modified C# files passed delimiter/event-reference static checks, but the final Windows build should still be compiled by Visual Studio 2022 or the repository GitHub Actions workflow.
