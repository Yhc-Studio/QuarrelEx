# QuarrelExStage v1

`*.qexstage.json` is a lightweight, per-stage interchange format shared by the Desktop and Web editors.

```json
{
  "Schema": "QuarrelExStage",
  "Version": 1,
  "SourceStage": 12,
  "Map": [[13 integers per row, 13 rows]],
  "Terrain": [
    { "Id": 13, "Attr": 0, "Tiles": [0, 0, 0, 0] }
  ]
}
```

- `SourceStage` is informational. Import always targets the stage currently selected in the editor.
- `Map` must be exactly 13×13.
- `Terrain` contains the TSA/Attr definitions actually referenced by `Map`.
- Each terrain definition has one ID, palette attribute `0..3`, and four 8×8 CHR tile indices in TL/TR/BL/BR order.
- IDs `$0E/$0F` remain internal/reserved and are never accepted as editable map cells.
- Demo packages remain limited to `$00-$0D`.
- The importer validates the whole package on a temporary ROM before committing it.

## Important: terrain definitions are global

The 13×13 map is stage data, but the TSA/Attr terrain table is shared by the ROM. Importing a stage package updates the terrain IDs carried by that package. Other stages that use the same IDs therefore see the same terrain-definition changes.

The full `QuarrelExConfig v3` (`*.qexcfg.json`) remains unchanged and continues to be used for whole-project configuration exchange.
