# QuarrelEx Desktop

Current release: **Desktop 1.2.0 Modern UI**, compatible with the maintained **BCEX 32KB Final Runtime** and QuarrelExConfig v3.

Build requirements:

- Windows 10/11
- Visual Studio 2022
- .NET 8
- `.NET desktop development` workload

Open `QuarrelEx.sln` and build Release, or run `build_release.bat` on Windows.

Localization is compiled from the shared `../locales/` catalogs through `Localization/BuiltInCatalogs.g.cs`. After changing localization files, run:

```text
python tools/generate_desktop_i18n.py
python tools/check_i18n.py
```

Desktop and Web share the same Config v3 semantics and current BCEX gameplay settings.


## Modern UI

Desktop 1.2.0 keeps the existing WinForms/.NET 8 codebase and ROM logic, but introduces a Fluent-inspired shell without third-party UI dependencies:

- persistent 252 px left navigation with grouped Editing / Gameplay / Information sections, fixed 18 px glyph slots and unified text baselines;
- card-style map / terrain workspace and modernized command bars;
- Microsoft YaHei UI-first typography for consistent Chinese/Latin metrics, flat buttons, grids and tool windows;
- a redesigned Game Settings workspace split into five short pages instead of one extremely long scrolling surface;
- preserved per-page scroll positions and selected page while the settings window is resized or refreshed;
- modeless editor windows remain available and retain their state when hidden.
