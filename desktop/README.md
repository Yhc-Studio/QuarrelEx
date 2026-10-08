# QuarrelEx Desktop

Current release: **Desktop 1.1.10**, compatible with the maintained **BCEX 32KB Final Runtime** and QuarrelExConfig v3.

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
