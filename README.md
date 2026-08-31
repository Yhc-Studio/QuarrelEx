# QuarrelEx

![Version](https://img.shields.io/badge/release-v1.1.8-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Web-0aa0c0)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

**QuarrelEx** is a Battle City / Battle City Ex ROM and level editor for Windows and the Web. It combines the classic Quarrel-style workflow with 70-stage BCEX editing, visual TSA/CHR tools, gameplay rules, drag-and-drop ROM opening, shared Config v3 files, and a current 32KB runtime designed to expose those options directly in the editor.

[简体中文](README.zh-CN.md) · UI languages: **简体中文 / English / 日本語**

> **No copyrighted Battle City ROM image is included.** IPS files require a legally obtained copy of the supported source ROM.

## Current release

| Component | Version |
|---|---:|
| QuarrelEx | 1.1.8 |
| Desktop | 1.1.8 |
| Web | 1.6.8 |
| Mobile Web UI | 1.0 (Core 1.6.8) |
| Config | QuarrelExConfig v3 |
| 32KB BCEX runtime | Runtime 6.9.3 / QXR1 v5 |

## Highlights

- Original Battle City, BCEX 16KB and BCEX 32KB editing.
- Stage 1-70 with 70 independent maps on the current 32KB runtime.
- Demo map editing.
- Enemy Type/Count editing; supported BCEX builds allow 1-255 total enemies.
- Visual TSA / CHR / palette / Flag-Fort editing.
- Title + Game Over Screen Editor.
- Original player/enemy spawn editing.
- Stage 1-70 custom enemy spawn points: 1P/2P independently choose Original or 1-8 points, with map overlay, 16px snap and terrain warnings.
- Stage 1-70 independent P1/P2 **player** spawn positions, including visual drag editing.
- Per-stage 1P/2P enemy spawn interval and maximum simultaneous enemies.
- Per-stage HQ/Base Exists.
- A+B+Start configurable cheat lives.
- Final GAME OVER Skip, score extra-life rules, 2P Original/Win-Streak.
- Armored Tank Original/One-Hit. Normal 400-point armored tanks can use the white 1-HP form, while flashing/item armored tanks keep their original durability/item path.
- Per-stage right-side **Enemy counter display**: Icons or Number for totals 1-50; totals 51-255 force Number while preserving the stored preference.
- Automatic flashing bonus tanks stay at spawn **#4 / #11 / #18** even when EnemyTotal exceeds 20.
- A newly spawning flashing tank no longer forcibly deletes an existing item.
- Correct next-stage terrain/setup after stage clear.
- Skip ON returns through the correct GAME OVER cleanup path without a one-frame stage-map flash.
- Demo is isolated from per-stage QXR overrides and keeps original/global player spawn, original enemy spawn cycle, original pacing and the original near-HQ no-fire behavior.
- Hold-B auto fire, Pistol/Lv4, downgrade on hit, faster movement, random enemy order, no friendly fire, enemy item pickup and locked initial player state.
- Per-stage `.qexstage.json` import/export containing the current 13x13 map plus the TSA/Attr terrain definitions actually referenced by that map.
- Map **Select / Move** tool with single-cell/rectangle selection, drag-to-move, Ctrl+drag copy, and Ctrl+C / Ctrl+X / Ctrl+V / Delete.
- Separate touch-first **Mobile Web UI** (`web/QuarrelEx_Mobile.html`) with bottom navigation and a mobile tool drawer; it reuses the same Web 1.6.8 editing core without replacing the desktop-oriented Web page.
- Shared Config v3 between Desktop and Web.
- Save / Save As, Undo / Redo.
- Drag-and-drop `.nes` opening with **Save / Don't Save / Cancel** when the current ROM is dirty.
- Main stage-map edits immediately refresh all map-backed Game Settings views.

## Repository layout

```text
QuarrelEx/
├─ desktop/
├─ web/
├─ patches/
│  ├─ 16KB/
│  └─ 32KB/
├─ docs/
├─ examples/
├─ locales/                 # zh-CN / en-US / ja-JP shared UI catalogs
├─ tools/                   # localization sync/validation helpers
├─ .github/workflows/
├─ README.md
├─ README.zh-CN.md
├─ CHANGELOG.md
└─ LICENSE
```

## Standard IPS base ROM

```text
Battle City (J)
Size:    24592 bytes
CRC32:   F599A07E
MD5:     cd4fe2e78df0696dbe652f02c19541a1
SHA-1:   e1061c9241b06a965fb7845cb951d921aca010ef
SHA-256: a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c
```

For the current 32KB build, run the preparation helper and apply:

```text
patches/32KB/QuarrelEx_BCEX_32KB_Runtime6.9.3.ips
```

See [patches/README.md](patches/README.md) for exact patching steps and checksums.

## Desktop

Open:

```text
desktop/QuarrelEx.sln
```

Requirements: Windows 10/11, .NET 8 Desktop Runtime, and Visual Studio 2022 with **.NET desktop development** to build from source.

## Web

Use either:

```text
web/QuarrelEx.html
web/QuarrelEx_Standalone.html
web/QuarrelEx_Mobile.html      # separate touch-first Mobile UI
```

`QuarrelEx_Mobile.html` is intentionally kept separate from the desktop-oriented Web UI. It uses a phone-first shell (Map / Enemy / TSA / Settings / More bottom navigation) while keeping the same ROM, Config v3, Stage package and Runtime 6.9.3 logic.

## Config v3

Desktop and Web use the same `*.qexcfg.json` schema. Screen raw bytes support `$00-$FF`, including `$FF` string terminators used by compatible ROM variants.

See [docs/QuarrelExConfig_v3_Spec.txt](docs/QuarrelExConfig_v3_Spec.txt).

## Copyright

This repository contains editor source code and patch files only. It does not distribute Battle City ROM images.

## License

MIT. See [LICENSE](LICENSE).
