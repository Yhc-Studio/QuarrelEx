# Publishing this package to GitHub

Recommended repository name: **QuarrelEx**

Recommended description:

> Modern Battle City / Battle City Ex editor for Windows and Web.

Recommended topics:

```text
nes
famicom
battle-city
rom-hacking
level-editor
nes-editor
winforms
javascript
retro-gaming
```

## First push

Create an empty public repository named `QuarrelEx` on GitHub. Do not ask GitHub to generate another README/LICENSE because this package already contains them.

From the extracted repository directory:

```bash
git init
git add .
git commit -m "QuarrelEx v1.1"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/QuarrelEx.git
git push -u origin main
```

## First Release

Tag suggestion:

```text
v1.1.0
```

Use `docs/Release_v1.1.md` as the release-note starting point.

Recommended release assets are prepared separately in the companion `QuarrelEx_v1.1_ReleaseAssets.zip`.

Before publishing, optionally add screenshots under `docs/screenshots/` and reference them from the root README.
