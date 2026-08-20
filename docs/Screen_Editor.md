# Title + Game Over Screen Editor

QuarrelEx v1.1 / Web v1.6 adds a screen-source editor for the original Battle City title and Game Over screens.

## Editing model

Battle City does not store these screens as one raw 32x30 nametable. The game draws fixed strings with ROM-native drawing routines, so QuarrelEx edits those source strings rather than inventing a replacement screen format.

- Ordinary title strings use the game's 8x8 background-tile string routine. One editor cell corresponds to one CHR tile.
- `BATTLE`, `CITY`, `GAME`, and `OVER` use the game's 32x32 magnified-glyph routine. Each editor box corresponds to one source glyph and changes the whole magnified glyph.
- `$FF` is the native terminator and cannot be selected as a screen tile.
- Coordinates and element lengths are fixed by the original drawing code and are intentionally not rewritten.
- Title preview uses the Title palette; Game Over preview uses the Level palette. The preview Attr selector is visual only.

## ROM-native data (16KB file offsets)

| Key | Offset | Length | Native mode |
|---|---:|---:|---|
| Title.Battle | `$12A9` | 6 | 32x32 magnified glyph |
| Title.City | `$12B0` | 4 | 32x32 magnified glyph |
| Title.TopLeft | `$12B5` | 2 | 8x8 tile string |
| Title.TopCenter | `$12C1` | 3 | 8x8 tile string |
| Title.TopRight | `$12B8` | 2 | 8x8 tile string |
| Title.OnePlayer | `$12D6` | 8 | 8x8 tile string |
| Title.TwoPlayers | `$12DF` | 9 | 8x8 tile string |
| Title.Construction | `$12FB` | 12 | 8x8 tile string |
| Title.SymbolRow | `$129F` | 9 | 8x8 tile string |
| Title.Copyright | `$1308` | 22 | 8x8 tile string |
| Title.Rights | `$1330` | 19 | 8x8 tile string |
| GameOver.Game | `$1353` | 4 | 32x32 magnified glyph |
| GameOver.Over | `$1358` | 4 | 32x32 magnified glyph |

For 32KB ROMs these fixed-main-bank offsets are shifted by `$4000`.

## Demo map

The Demo map is the original physical map slot 36 (`$3CFB` in a 16KB ROM, `$7CFB` in the 32KB fixed main bank). It uses the same 13x13 editor as normal stages, but the native storage is still 4-bit, so only `$00-$0D` are writable. Enemy Type/Count intentionally shares Stage 35.
