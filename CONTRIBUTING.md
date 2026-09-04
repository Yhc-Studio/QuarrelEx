# Contributing

1. Do not commit copyrighted ROM images.
2. Keep Desktop and Web Config v3 semantics compatible.
3. When changing BCEX runtime layout/feature bits, update `docs/BCEX_Runtime.md` and editor capability detection together.
4. Test both 16KB and 32KB profiles where applicable.
5. For Desktop changes, build `desktop/QuarrelEx.sln` with .NET 8 on Windows.
6. For Web changes, test `QuarrelEx.html` and the separate `QuarrelEx_Mobile.html` entry when shared code is affected.
7. Prefer small, documented runtime hooks and preserve original behavior when a feature flag is disabled.
