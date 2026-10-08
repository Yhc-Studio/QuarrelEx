# QuarrelEx

![Release](https://img.shields.io/badge/release-v1.1.10-blue)
![Desktop](https://img.shields.io/badge/Desktop-1.1.10-512BD4)
![Web](https://img.shields.io/badge/Web-1.6.14-0aa0c0)
![Mobile](https://img.shields.io/badge/Mobile-1.1.1-0aa0c0)
![License](https://img.shields.io/badge/license-MIT-green)

**QuarrelEx** is a Battle City / BCEX ROM and stage editor for Windows and modern browsers. The repository contains the complete Desktop source, self-contained Web and Mobile editors, shared localization/configuration support, current documentation, and the maintained IPS patches.

[简体中文](README.zh-CN.md) · UI languages: **简体中文 / English / 日本語**

> No Battle City ROM image is included. IPS patches require a legally obtained compatible base ROM.

## Release components

| Component | Version |
|---|---:|
| QuarrelEx | 1.1.10 |
| Desktop | 1.1.10 |
| Web | 1.6.14 |
| Mobile Web | 1.1.1 |
| Config format | QuarrelExConfig v3 |
| BCEX 32KB | Final Runtime |

## Highlights

- Stage 1–70 editing with 70 independent maps on the maintained 32KB BCEX line.
- Enemy Type/Count, custom enemy totals, per-stage pacing, enemy/player spawn points, Base Exists, and enemy-counter display mode.
- TSA/CHR, palette, Flag/Fort, Title and Game Over screen editing.
- Shared Config v3 and per-stage `.qexstage.json` import/export across Desktop and Web.
- Initial/Death Level, A+B+Start lives, GAME OVER skip, extra-life rules, 2P rules, armor rules, shields, grenade behavior, enemy 1UP, enemy bullet profiles, native 8-slot item drop tables, and per-stage bonus-tank cadence.
- Random Enemy mode with independent Type + HP 1–8 and visible armor-stage synchronization.
- Enemy Pistol behavior with synchronized armor durability and steel-destroying bullets.
- Optional Lv4 forest destruction after the HQ/base is destroyed.
- Standard 192-byte NES `.pal` loading for editor preview colors.
- Touch-first Mobile Web UI using the same current ROM model and settings as the desktop-oriented Web editor.

## Quick start

### Desktop

Open `desktop/QuarrelEx.sln` in Visual Studio 2022 with .NET 8 and the **.NET desktop development** workload installed.

### Web

Open `web/QuarrelEx.html` directly in a modern browser. The editor is self-contained and works offline.

### Mobile Web

Open `web/QuarrelEx_Mobile.html` on a phone or tablet. It uses the same ROM/config model as Web 1.6.14 with a touch-first layout.

## IPS patches

Only the current patch for each maintained ROM line is included:

```text
patches/16KB/QuarrelEx_BCEX_16KB_v1.0.ips
patches/32KB/QuarrelEx_BCEX_32KB_Final.ips
```

Current 32KB patch input:

```text
Size:       40976 bytes
CRC32:      D2572735
SHA-256:    5e0b53e33a40166e5d31d50734c6059c96bc061301d2b77e85bd10a744e53291
```

Expected output:

```text
Size:       40976 bytes
CRC32:      E33746DA
SHA-256:    ab85c29a8ee0dd68078d5fce959147e6400105e1f55fdda975e13ce9c7c05aa8
```

See [patches/README.md](patches/README.md) before applying a patch.

## Repository layout

```text
QuarrelEx/
├─ desktop/          Windows source
├─ web/              Web + Mobile Web editors
├─ patches/          current 16KB and 32KB IPS patches
├─ docs/             specifications and guides
├─ examples/         Config v3 example
├─ locales/          zh-CN / en-US / ja-JP catalogs
├─ tools/            localization generation/validation tools
└─ .github/workflows/
```

## Documentation

- [BCEX 32KB Final Runtime](docs/BCEX_32KB_Final.md)
- [BCEX 16KB](docs/BCEX_16KB.md)
- [QuarrelExConfig v3](docs/QuarrelExConfig_v3_Spec.txt)
- [Stage package format](docs/QuarrelExStage_v1_Spec.md)
- [Enemy types](docs/Enemy_Types.md)
- [TSA guide](docs/TSA_Guide.md)
- [Screen editor](docs/Screen_Editor.md)

## Development

Localization catalogs live in `locales/`. After editing them:

```text
python tools/generate_desktop_i18n.py
python tools/check_i18n.py
```

The GitHub Actions workflow validates localization and builds the Desktop project.

## License

MIT. See [LICENSE](LICENSE). Third-party notices are in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
