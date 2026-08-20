# QuarrelEx

![Version](https://img.shields.io/badge/release-v1.1-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Web-0aa0c0)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

**QuarrelEx** is a modern Battle City / Battle City Ex level and ROM editor for Windows and the Web. It is inspired by the workflow of the classic Quarrel editor and adds extended BCEX ROM support, 70-stage editing, visual TSA/CHR editing, palette editing, gameplay options, shared Config v3 files, undo/redo, and more.

[简体中文](README.zh-CN.md)

> **No copyrighted Battle City ROM is included.** The IPS patches in this repository require a legally obtained copy of the supported base ROM.

## Current release

| Component | Version |
|---|---:|
| Desktop editor | 1.1 |
| Web editor | 1.6.0 |
| Shared config format | QuarrelExConfig v3 |
| 16KB BCEX runtime | internal revision 6.3 |
| 32KB BCEX runtime | internal revision 6.4.1 |

The different internal/runtime revision numbers are development identifiers. The current editor release is **QuarrelEx v1.1**; BCEX runtime revisions are unchanged from v1.0.

## Highlights

- Original Battle City support plus BCEX 16KB/32KB formats.
- Stage 1-70 editing; 32KB BCEX provides 70 independent maps. The original Demo map is also exposed in every supported ROM mode.
- Four enemy type/count entries per stage; supported BCEX ROMs allow **1-255 total enemies**.
- Visual 16x16 TSA editor with `Attr 0-3` and direct `$00-$FF` CHR tile selection.
- Extended terrain: 16KB up to `$00-$1F`; 32KB up to `$00-$3F` with custom slots.
- Palette editor and Flag/Fort TSA editor.
- Unified Title + Game Over Screen Editor: ordinary strings are edited as native 8x8 CHR tiles; the original 32x32 magnified BATTLE/CITY/GAME/OVER glyph sources are edited as whole glyph slots.
- Player/enemy spawn position editing with numeric input and drag editing.
- Gameplay options such as auto-fire, Pistol/Lv4, level-down on hit, faster player movement, random enemy order, no friendly fire, enemy power-up pickup, and locked initial player state.
- Config v3 is shared between Desktop and Web and includes gameplay, palettes, TSA, Flag/Fort TSA, stage maps, Demo map, Title/Game Over screen elements, enemy types/counts and enemy totals.
- Undo/redo (`Ctrl+Z`, `Ctrl+Y`, `Ctrl+Shift+Z`).
- Save / Save As support.
- Strict Config v3 preflight validation before import.

## Repository layout

```text
QuarrelEx/
├─ desktop/                 # C# WinForms / .NET 8 project
├─ web/                     # Browser editor + standalone HTML
├─ patches/
│  ├─ 16KB/                 # Base ROM -> current 16KB BCEX
│  └─ 32KB/                 # 32KB preparation helper + final IPS
├─ docs/                    # BCEX, TSA, enemy type, config docs
├─ examples/                # QuarrelExConfig v3 example
├─ .github/workflows/       # Desktop CI build
├─ README.md
├─ README.zh-CN.md
├─ CHANGELOG.md
└─ LICENSE
```

## Supported base ROM for IPS patches

The 16KB IPS is applied directly to this exact ROM. The 32KB package first uses the included preparation script to create a 32KB working base from the same clean ROM, then applies the 32KB IPS:

```text
Title:   Battle City (J)
Size:    24592 bytes
CRC32:   F599A07E
MD5:     cd4fe2e78df0696dbe652f02c19541a1
SHA-1:   e1061c9241b06a965fb7845cb951d921aca010ef
SHA-256: a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c
```

Patch steps and result checksums are documented in [patches/README.md](patches/README.md).

## Desktop editor

Requirements:

- Windows 10/11
- .NET 8 Desktop Runtime
- Visual Studio 2022 with **.NET desktop development** workload to build from source

Open:

```text
desktop/QuarrelEx.sln
```

Common shortcuts:

| Shortcut | Action |
|---|---|
| `Ctrl+O` | Open ROM |
| `Ctrl+S` | Save |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+Z` | Undo |
| `Ctrl+Y` / `Ctrl+Shift+Z` | Redo |
| `F2` | Enemy editor |
| `F3` | TSA editor |
| `F4` | Palette editor |
| `F5` | Flag TSA editor |
| `F6` | Game settings |
| `F7` | Ex options |
| `F8` | ROM information |
| `F9` | Title / Game Over Screen Editor |

The tool windows can be moved, resized, maximized, or placed on a second monitor.

## Web editor

Use either:

```text
web/QuarrelEx.html
web/QuarrelEx_Standalone.html
```

The standalone build is a single file. Browsers supporting the File System Access API can save directly back to an opened file; other browsers fall back to download-based saving.

## Enemy Type note

`$04` is the flashing/bonus flag bit, not a standalone enemy class. Common examples:

| Normal | Flashing/bonus |
|---|---|
| `$80` | `$84` |
| `$A0` | `$A4` |
| `$C0` | `$C4` |
| `$E0` | `$E4` |

See [docs/Enemy_Types.md](docs/Enemy_Types.md).

## Config v3

Desktop and Web use the same `*.qexcfg.json` format. Current releases only import/export **Version 3**. Demo and Screens are optional v3 extensions, so older valid v3 files remain importable and preserve those target-ROM areas when absent. The importer validates the entire file before committing changes to the ROM.

See [docs/QuarrelExConfig_v3_Spec.txt](docs/QuarrelExConfig_v3_Spec.txt).

## Building / CI

A Windows GitHub Actions workflow is included under `.github/workflows/build-desktop.yml`. It restores and builds the .NET 8 WinForms project on every push/pull request affecting the desktop source.

## ROM and copyright notice

This repository contains editor source code and patch files only. It does not distribute Battle City ROM images.

The MIT license applies to the QuarrelEx source code written for this project. Battle City, game data, graphics, audio, and other third-party material remain the property of their respective rights holders and are not covered by the MIT license.

The public source package intentionally does not bundle artwork extracted from the original Quarrel executable. Toolbar icons used by QuarrelEx are generated by the application code.

## Credits

QuarrelEx is inspired by the original Quarrel Battle City editor and by the NES reverse-engineering/emulation community.

This project is not affiliated with or endorsed by the original game publisher.

## License

MIT. See [LICENSE](LICENSE).
