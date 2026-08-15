# TSA / Attribute guide

A 16x16 terrain block is composed from four 8x8 CHR tiles:

```text
TL | TR
---+---
BL | BR
```

- `ID` is the 16x16 terrain ID.
- `Attr` selects background palette 0-3.
- `TL/TR/BL/BR` are 8x8 CHR tile IDs `$00-$FF`.
- Terrain IDs `$0E` and `$0F` are reserved/hidden from normal editing.

The Desktop and Web editors provide a visual CHR selector rather than requiring manual hex entry.

Current BCEX terrain capacity:

- 16KB: up to `$1F` (32 table entries; `$0E/$0F` reserved).
- 32KB: up to `$3F` (64 table entries; `$20-$3F` are useful custom slots).
