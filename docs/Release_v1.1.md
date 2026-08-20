# QuarrelEx v1.1 — Demo + Screen Editor

## English

QuarrelEx v1.1 is an editor-only update that expands coverage of the original Battle City ROM data. The BCEX runtime patches are unchanged.

### New
- **Demo map editing** in Original 16KB, Ex 16KB, legacy 32KB Overlay and BCEX 32KB 70-map ROMs.
- Demo uses the normal **13×13 map editor** and is shown as `Demo`, not as Stage 71.
- The native Demo map remains physical map slot 36 and uses 4-bit terrain storage, so writable terrain IDs are `$00-$0D`.
- Demo Enemy Type / Count intentionally shares Stage 35, matching the original game data flow.
- New unified **Title + Game Over Screen Editor**.
- Ordinary title strings are edited as native **8×8 CHR tile** slots.
- `BATTLE`, `CITY`, `GAME`, and `OVER` use the game's native **32×32 magnified-glyph** routine and are edited as whole source-glyph slots.
- Native `$FF` string terminators are protected and cannot be selected as screen tiles.
- Desktop shortcut: **F9** opens the Screen Editor.
- Web adds a dedicated **Screen / 画面** tab.

### Config v3
`QuarrelExConfig` remains **Version 3**. Two optional v3 extensions were added:
- `Demo.Map`
- `Screens.Title` / `Screens.GameOver`

Older Config v3 files remain importable. If these fields are absent, the target ROM's Demo and screen data are preserved.

### Components
- Desktop editor: **v1.1**
- Web editor: **v1.6.0**
- Config: **QuarrelExConfig v3**
- 16KB BCEX runtime: **6.3** (unchanged)
- 32KB BCEX runtime: **6.4.1** (unchanged)

### Important
No copyrighted Battle City ROM image is included. The existing IPS patches remain the current runtime patches; no new IPS is required for this editor-only update.

---

# QuarrelEx v1.1 — Demo + 画面编辑器

## 中文

QuarrelEx v1.1 是一次**编辑器功能更新**，用于补充原版《Battle City》已有 ROM 数据的编辑范围；BCEX 游戏运行程序与 IPS 本次不变。

### 新增
- 原版 16KB、Ex 16KB、Legacy 32KB Overlay、BCEX 32KB 70独立地图模式均可编辑 **Demo 地图**。
- Demo 完全复用现有的 **13×13 地图编辑器**，界面显示为 `Demo`，不会显示成 Stage 71。
- Demo 仍然使用原游戏的物理地图槽 36 和 4-bit 地形格式，因此可写地形限制为 `$00-$0D`。
- Demo 的 Enemy Type / Count 与 Stage 35 共用，保持原游戏的数据关系。
- 新增统一的 **Title + Game Over Screen Editor**。
- 标题画面的普通字符串按原生 **8×8 CHR Tile** 槽编辑。
- `BATTLE`、`CITY`、`GAME`、`OVER` 使用原游戏的 **32×32 放大字形程序**，因此按完整的源字形槽编辑，而不是拆成虚假的四个独立 Tile。
- 原生字符串终止符 `$FF` 会受到保护，不能作为画面图块写入。
- Desktop 使用 **F9** 打开 Screen Editor。
- Web 新增 **画面 / Screen** 页签。

### Config v3
`QuarrelExConfig` 仍然保持 **Version 3**，新增两个可选扩展：
- `Demo.Map`
- `Screens.Title` / `Screens.GameOver`

旧版 Config v3 仍然可以导入；如果没有这些字段，则保留目标 ROM 当前的 Demo、Title 与 Game Over 数据。

### 组件版本
- Desktop：**v1.1**
- Web：**v1.6.0**
- 配置：**QuarrelExConfig v3**
- 16KB BCEX Runtime：**6.3**（不变）
- 32KB BCEX Runtime：**6.4.1**（不变）

### 注意
本项目不包含任何受版权保护的《Battle City》ROM。本次仅更新编辑器，现有 IPS 仍然是当前正式 Runtime 补丁，不需要新增 IPS。
