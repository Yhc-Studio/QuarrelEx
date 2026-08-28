# QuarrelEx v1.1.7

**Cumulative Update from v1.1 / v1.1 → v1.1.7 累积更新**

QuarrelEx v1.1.7 is a cumulative feature, compatibility, and runtime-stability update built on top of v1.1.

Compared with v1.1, this release substantially expands the 32KB BCEX runtime and the Desktop/Web editors with per-stage gameplay settings, independent spawn configuration, GAME OVER behavior controls, enemy counter display options, Mid City2 compatibility, Demo behavior fixes, and multiple runtime corrections.

> This project does not include or distribute any copyrighted Battle City ROM image.  
> IPS patches must be applied to a legally obtained compatible ROM.

---

## Versions

| Component | Version |
|---|---|
| QuarrelEx | **1.1.7** |
| Desktop Editor | **1.1.7** |
| Web Editor | **1.6.7** |
| Config Format | **QuarrelExConfig v3** |
| 32KB BCEX Runtime | **Runtime 6.9.3 / QXR1 v5** |
| 16KB BCEX Runtime | **Unchanged from v1.1** |

---

# 中文

## 概要

QuarrelEx v1.1.7 是从 v1.1 继续迭代而来的累积更新。

v1.1 主要加入了 **Demo 地图编辑**以及统一的 **Title + Game Over Screen Editor**；从 v1.1 到 v1.1.7，更新重点进一步转向：

- 32KB BCEX Runtime 的关卡级游戏规则扩展
- Stage 1～70 独立出生点配置
- 1P / 2P 独立敌人出生方式
- 每关敌人生成节奏与老巢开关
- GAME OVER / 奖励命 / 双人模式规则
- 400 分装甲车一击模式
- 敌人数量数字显示
- Demo 原版行为恢复
- 过关、闪光坦克、道具与 Skip 流程修复
- Mid City2 / Mid City2 PS 兼容
- Desktop / Web 编辑器同步完善

---

## 编辑器新增：单关卡配置导入 / 导出

Desktop 与 Web 现在都可以针对**当前选择的单个关卡**导入或导出：

`*.qexstage.json`

该轻量关卡配置使用：

`Schema = QuarrelExStage / Version = 1`

内容仅包括：

- 当前关卡的 13×13 地图
- 这张地图实际引用到的 Terrain TSA / Attr 地形定义

它不会修改或携带敌人配置、命数、游戏规则、调色板、Demo、Title / Game Over 等其他全局数据。导入时始终写入**当前选择的目标关卡**，因此也可以用来在不同关卡之间复制完整地图。

> 注意：地图数据是逐关保存的，但 TSA / Attr 地形定义在 ROM 中属于全局共享表。导入关卡包时，包内使用到的地形 ID 定义会同步更新，因此其他使用相同地形 ID 的关卡外观也可能变化。导入器会在写入前明确提示，并先在临时 ROM 上完成完整校验。

---

## 编辑器新增：地图选择 / 移动工具

地图编辑不再只支持逐块绘制。地形列表顶部新增 **“选择 / 移动工具”**，进入该模式后不选择任何绘制地形。

支持：

- 单击选择一个地图块
- 左键拖动框选矩形区域
- 拖动已选区域进行移动
- `Ctrl + 拖动` 复制并放置选区
- `Ctrl+C` 复制
- `Ctrl+X` 剪切
- `Ctrl+V` 粘贴
- `Delete / Backspace` 清空为 `$0D`
- 方向键逐格移动选区（Desktop / Web）
- 复制内容可以切换关卡后再粘贴
- 移动、剪切、粘贴均纳入现有 Undo / Redo

地图右键吸取地形后会重新进入绘制模式，因此原来的“左键绘制 / 右键吸取”操作保持兼容。

---

## 1. Stage 1～70 玩家出生点编辑

32KB BCEX 现在可以为 **Stage 1～70** 分别设置玩家出生位置。

支持：

- P1 独立出生位置
- P2 独立出生位置
- 地图上的可视化拖拽编辑
- 坐标精确编辑
- 每关独立保存
- Desktop / Web 共用相同配置数据

这使得每一关都可以拥有独立的玩家开局位置，而不再只能依赖全局固定出生点。

---

## 2. Stage 1～70 自定义敌人出生点

新增完整的每关敌人出生位置配置。

P1 与 P2 模式可以分别选择：

- `Original`
- 自定义 `1～8` 个敌人出生点

编辑器支持：

- 地图覆盖显示
- 可视化拖拽
- 16 px 对齐
- 坐标输入
- 地形位置警告
- P1 / P2 独立配置

因此，同一关在 1P 和 2P 模式下可以使用不同的敌人出生布局。

---

## 3. 每关 Enemy Pacing

Stage 1～70 现在可以独立设置敌人的生成节奏。

支持分别配置 1P / 2P：

- Enemy Spawn Interval
- Maximum Simultaneous Enemies

因此可以为不同关卡制作：

- 快节奏连续出兵
- 低密度战斗
- 高压多敌人场景
- 1P / 2P 不同难度曲线

---

## 4. 每关 HQ / Base Exists

新增每关独立的：

`Base Exists`

设置。

可以决定当前关卡是否存在老巢 / HQ。

关闭后，游戏与编辑器会按照无老巢关卡的规则处理对应地图，而不再强制依赖标准 Battle City 老巢结构。

---

## 5. 右侧敌人数量显示模式

新增每关独立的 Enemy Counter Display 设置。

当总敌人数为 **1～50** 时，可以选择：

- `Icons`
- `Number`

当总敌人数为 **51～255** 时：

- 自动强制使用 `Number`
- 原本保存的 `Icons / Number` 偏好不会被破坏
- 当敌人数重新降低到 50 以下时，可以继续恢复原先选择

这样既保留了原版图标显示，也解决了大量敌人无法在右侧完整显示的问题。

---

## 6. GAME OVER Skip

新增最终 GAME OVER 流程控制：

- `Skip OFF`：保持原版纵向 GAME OVER 流程
- `Skip ON`：跳过最终 GAME OVER 画面流程

v1.1.7 同时修复了 Skip ON 时的画面清理问题。

现在跳过 GAME OVER 后：

- 不会短暂闪出上一关地图
- 会通过正确的 GAME OVER 清理路径返回
- 不再留下错误画面或中间帧

---

## 7. Extra Life 规则

新增可配置的加命规则，用于控制分数达到条件时的奖励命行为。

相关设置已经纳入当前 32KB QXR 配置，并可以由 Desktop / Web 编辑器统一保存。

---

## 8. 2P Original / Win-Streak

双人模式新增可选规则：

- `Original`
- `Win-Streak`

Win-Streak 模式会按照连续胜利情况提供额外奖励，使 2P 对战结果能够形成连续奖励机制。

同时修复了相关结算流程中的重复奖励与状态处理问题。

---

## 9. Armored Tank：Original / One-Hit

新增 400 分装甲坦克规则：

- `Original`
- `One-Hit`

在 `One-Hit` 模式下：

- 普通 400 分装甲坦克使用白色的 1 HP 状态
- 普通子弹即可一击消灭
- 闪光 / 携带道具的 400 分坦克仍保持原有耐久与道具流程
- 不破坏原本的闪光奖励坦克逻辑

这样可以单独改变普通装甲车难度，而不会破坏奖励坦克。

---

## 10. 闪光奖励坦克生成规则修复

当每关 EnemyTotal 超过原版 20 辆限制后，自动闪光奖励坦克仍然固定出现在：

- #4
- #11
- #18

不会因为总敌人数扩展到 21～255 而错误移动闪光位置。

同时修复：

- 新闪光坦克出生时不再强制删除当前已经存在的道具
- 当前可见道具可以继续保留
- 闪光坦克与道具生成逻辑更加接近原版预期行为

---

## 11. 过关后下一关地图 / 地形初始化修复

修复了扩展 Runtime 中过关后进入下一关时可能出现的地图初始化异常。

现在正常过关后会正确准备：

- 下一关地图
- 地形数据
- 关卡状态
- 对应的 Stage 设置

不再出现短暂显示旧地图、等待旧场景或错误沿用上一关地形的情况。

---

## 12. Demo 行为恢复与隔离

Demo 地图编辑功能继续保留，但 Demo 的实际运行逻辑现在与普通 Stage QXR 配置隔离。

Demo 会保持原版 / 全局行为，包括：

- 原版玩家出生逻辑
- 原版敌人出生循环
- 原版敌人生成节奏
- 不继承普通关卡的 per-stage QXR 覆盖
- 不错误继承 Stage 30 等普通关卡设置
- 保持原版靠近 HQ 时不射击的 Demo 行为

即使启用了 Hold-B Auto Fire，Demo 中靠近 HQ 的演示坦克也不会错误持续攻击老巢。

这修复了此前 Demo 可能演变为持续攻击 HQ / 老巢的异常演示。

---

## 13. A+B+Start 初始命数

扩展 A+B+Start Cheat 设置。

现在可以通过编辑器设置触发该操作后给予的命数，而不再固定为单一数值。

该设置已纳入当前 32KB QXR 配置。

---

## 14. Title + Game Over Screen Editor 改进

v1.1 引入的 Title + Game Over Screen Editor 在后续版本中继续改进。

Screen 原始数据现在可以安全处理：

`$00-$FF`

完整字节范围。

特别是：

- `$FF` 可以作为兼容 ROM 中的字符串结束符保留
- 导入 / 导出 Config 时不会错误破坏终止字节
- 提升了不同 Battle City 改版 ROM 的兼容性

---

## 15. 主地图与 Game Settings 同步

修复地图编辑器与依赖地图数据的 Game Settings 视图之间的同步问题。

现在修改主 Stage Map 后：

- 相关 Game Settings 地图视图会立即刷新
- 不需要重新打开 ROM
- 不需要关闭再打开设置窗口
- 出生点与地图地形判断能够使用最新数据

---

## 16. ROM 拖拽打开与未保存提示

Desktop 版增强 ROM 文件打开流程。

现在可以直接将：

`*.nes`

拖入 QuarrelEx 打开。

如果当前 ROM 已经被修改但尚未保存，会显示：

- `Save`
- `Don't Save`
- `Cancel`

避免拖入新 ROM 时意外丢失当前编辑内容。

---

## 17. QuarrelExConfig v3

v1.1.7 继续使用：

`QuarrelExConfig v3`

没有为了本次更新强制升级配置文件主版本。

Desktop 与 Web 继续共用：

`*.qexcfg.json`

当前 Config 支持保存包括：

- Stage 1～70 地图
- Enemy Type / Count / Total
- Feature Flags
- Enemy Item Flags
- Demo
- Title / Game Over Screens
- TSA / Attr
- Palette
- Flag / Fort TSA
- 玩家出生位置
- 自定义敌人出生位置
- Enemy Pacing
- Base Exists
- Enemy Counter Display
- GAME OVER / Extra Life / 2P / Armor 等 QXR 设置

同时继续执行严格导入校验。

存在硬错误的配置不会直接写入当前 ROM，从而降低错误 Config 破坏 ROM 数据的风险。

---

## 18. Mid City2 / Mid City2 PS 兼容

v1.1.7 新增针对以下改版的兼容 IPS：

- **Mid City2**
- **Mid City2 PS**

兼容补丁位于：

`patches/compatibility/`

应用对应兼容 IPS 后，可以直接使用当前 QuarrelEx 打开这些版本，并使用当前：

`QXR1 v5`

提供的扩展设置，同时尽可能保留各自原有的改版内容。

---

## 19. Desktop / Web 同步

本次版本继续保持 Desktop 与 Web 的主要编辑能力同步。

当前版本：

- Desktop Editor：**1.1.7**
- Web Editor：**1.6.7**

两端继续共享 QuarrelExConfig v3，因此：

- Desktop 导出的 Config 可以在 Web 导入
- Web 导出的 Config 可以在 Desktop 导入

---

## 20. 32KB BCEX Runtime 6.9.3

当前 v1.1.7 推荐的 32KB BCEX Runtime 为：

`Runtime 6.9.3 / QXR1 v5`

主要包含：

- Stage 1～70 独立地图
- Stage 1～70 独立玩家出生点
- Stage 1～70 自定义敌人出生点
- 每关 Enemy Pacing
- 每关 Base Exists
- Enemy Counter Icons / Number
- GAME OVER Skip
- Extra Life Rules
- 2P Original / Win-Streak
- Armored Tank Original / One-Hit
- A+B+Start Cheat Lives
- Demo 与普通关卡 QXR 配置隔离
- 闪光奖励坦克 #4 / #11 / #18 修正
- 可见道具保留修正
- 下一关地图 / 地形初始化修正
- Skip ON 旧地图闪屏修正

当前 32KB 标准补丁：

`patches/32KB/QuarrelEx_BCEX_32KB_Runtime6.9.3.ips`

---

## 兼容性说明

### 从 QuarrelEx v1.1 升级

编辑器可以直接升级到：

- Desktop 1.1.7
- Web 1.6.7

Config 格式仍然是 v3。

### 16KB BCEX

16KB Runtime / IPS 保持与 v1.1 相同。

v1.1.7 新增的大部分 per-stage QXR1 v5 Runtime 功能以当前 32KB BCEX Runtime 为主要目标。

### 32KB BCEX

如果需要使用 v1.1.7 的完整 Runtime 新功能和修复，建议使用当前：

`Runtime 6.9.3 / QXR1 v5`

补丁。

---

## 从 v1.1 到 v1.1.7 的重点变化

简要来说，v1.1.7 在 v1.1 的 Demo / Screen Editor 基础上，进一步完成了：

- 单关卡 `.qexstage.json` 地图 + 地形导入 / 导出
- 地图选择、移动、复制、剪切、粘贴与跨关卡复制

1. Stage 1～70 独立 P1 / P2 玩家出生点
2. Stage 1～70、1P / 2P 独立的 1～8 个敌人出生点
3. 每关 Enemy Pacing
4. 每关 Base Exists
5. Enemy Counter Icons / Number
6. EnemyTotal > 50 自动数字显示
7. GAME OVER Skip
8. Extra Life Rules
9. 2P Original / Win-Streak
10. Armored Tank Original / One-Hit
11. A+B+Start Cheat Lives
12. Demo 原版行为恢复与 QXR 隔离
13. 闪光坦克 #4 / #11 / #18 修复
14. 新闪光坦克不再删除已有道具
15. 下一关地图 / 地形初始化修复
16. Skip ON 旧关卡地图闪屏修复
17. Screen `$00-$FF` 原始字节兼容
18. ROM 拖拽打开与未保存提示
19. 主地图与 Game Settings 实时同步
20. Mid City2 / Mid City2 PS 兼容 IPS
21. Desktop 1.1.7 / Web 1.6.7 同步
22. 32KB Runtime 更新至 6.9.3 / QXR1 v5

---

# English

## Overview

QuarrelEx v1.1.7 is a cumulative update built on top of v1.1.

While v1.1 primarily introduced **Demo Map Editing** and the unified **Title + Game Over Screen Editor**, the releases leading to v1.1.7 focus on expanded per-stage gameplay configuration, spawn editing, GAME OVER behavior, enemy-counter display modes, compatibility patches, Demo restoration, and 32KB runtime fixes.

---

## Editor Addition: Per-Stage Import / Export

Desktop and Web can now import or export the **currently selected stage** as:

`*.qexstage.json`

The lightweight stage package uses:

`Schema = QuarrelExStage / Version = 1`

and contains only:

- The current 13×13 map
- The Terrain TSA / Attr definitions actually referenced by that map

It does not carry enemy settings, lives, gameplay rules, palettes, Demo data, or Title / Game Over data. Import always targets the currently selected stage, so the format can also be used to move complete maps between stages.

> Note: map data is per-stage, but the TSA / Attr terrain-definition table is ROM-global. Importing a stage package updates the terrain IDs carried by the package, so other stages using the same IDs can also change appearance. The importer warns before committing and validates the package on a temporary ROM first.

---

## Editor Addition: Map Select / Move Tool

The map editor now provides a **Select / Move** mode in addition to terrain painting.

Supported operations include:

- Click to select one cell
- Drag to select a rectangular region
- Drag an existing selection to move it
- `Ctrl + drag` to copy and place it
- `Ctrl+C` Copy
- `Ctrl+X` Cut
- `Ctrl+V` Paste
- `Delete / Backspace` clear to `$0D`
- Arrow keys to move the selection one cell at a time
- Copy a selection, switch stages, then paste it
- Move / Cut / Paste operations participate in the existing Undo / Redo history

Right-click terrain pickup returns the map to paint mode, preserving the existing left-paint / right-pick workflow.

---

## 1. Stage 1-70 Player Spawn Editing

The current 32KB BCEX runtime supports independent player spawn positions for **Stage 1-70**.

Supported settings include:

- Independent P1 spawn position
- Independent P2 spawn position
- Visual drag editing
- Precise coordinate editing
- Per-stage storage
- Shared Desktop/Web configuration data

Each stage can therefore define its own player starting positions instead of relying only on global defaults.

---

## 2. Stage 1-70 Custom Enemy Spawn Points

Enemy spawn configuration has been expanded to support per-stage custom spawn layouts.

P1 and P2 can independently use:

- `Original`
- `1-8` custom enemy spawn points

The editor provides:

- Map overlays
- Visual dragging
- 16 px snapping
- Coordinate input
- Terrain warnings
- Independent P1/P2 configuration

A single stage can therefore use different enemy spawn layouts in 1P and 2P modes.

---

## 3. Per-Stage Enemy Pacing

Stage 1-70 can now define independent enemy pacing.

Separate 1P / 2P settings are available for:

- Enemy Spawn Interval
- Maximum Simultaneous Enemies

This makes it possible to create different pacing and difficulty curves for each stage and player mode.

---

## 4. Per-Stage HQ / Base Exists

A new per-stage:

`Base Exists`

setting determines whether the current stage contains an HQ/Base.

This allows non-standard stages to operate without being forced to use the normal Battle City HQ layout.

---

## 5. Enemy Counter Display

A per-stage Enemy Counter Display option has been added.

For **1-50** total enemies, the user can select:

- `Icons`
- `Number`

For **51-255** total enemies:

- `Number` is forced automatically
- The stored Icons/Number preference is preserved
- The previous preference can be restored when the total returns to 50 or below

This keeps the original icon display available while allowing large enemy totals to remain readable.

---

## 6. GAME OVER Skip

The final GAME OVER flow is now configurable:

- `Skip OFF`: preserves the original vertical GAME OVER flow
- `Skip ON`: skips the final GAME OVER screen flow

v1.1.7 also fixes cleanup when Skip is enabled.

The game now returns through the proper GAME OVER cleanup path without briefly flashing the previous stage map.

---

## 7. Extra Life Rules

Configurable extra-life rules have been added for score-based life rewards.

These settings are included in the current 32KB QXR configuration and are shared between the Desktop and Web editors.

---

## 8. 2P Original / Win-Streak

Two-player behavior now supports:

- `Original`
- `Win-Streak`

Win-Streak mode provides rewards based on consecutive wins and adds a persistent progression element to 2P results.

Related duplicate reward and state-handling issues have also been corrected.

---

## 9. Armored Tank: Original / One-Hit

A configurable 400-point armored tank rule has been added:

- `Original`
- `One-Hit`

In `One-Hit` mode:

- Normal 400-point armored tanks use the white 1-HP form
- They can be destroyed with one normal hit
- Flashing/item armored tanks preserve their original durability and item path
- The original bonus-tank behavior remains intact

---

## 10. Flashing Bonus Tank Fixes

Automatic flashing bonus tanks remain assigned to:

- #4
- #11
- #18

even when EnemyTotal is extended beyond the original 20-enemy limit.

Also fixed:

- Spawning a new flashing tank no longer forcibly deletes an existing visible item
- Existing items remain available
- Bonus-tank behavior is closer to the original game logic

---

## 11. Next-Stage Terrain / Setup Fix

Fixed incorrect stage initialization that could occur after clearing a stage in the expanded runtime.

The next stage now correctly prepares its:

- Map
- Terrain data
- Stage state
- Per-stage settings

without briefly reusing or displaying the previous stage setup.

---

## 12. Demo Isolation and Original Behavior

Demo editing remains available, while Demo runtime behavior is now isolated from normal per-stage QXR overrides.

The Demo preserves:

- Original/global player spawn behavior
- Original enemy spawn cycle
- Original enemy pacing
- No accidental inheritance from normal stage QXR settings
- No accidental Stage 30 override
- Original near-HQ no-fire behavior

Even with Hold-B Auto Fire enabled, the Demo tank will retain the original no-fire behavior near the HQ instead of repeatedly attacking the base.

---

## 13. A+B+Start Cheat Lives

The A+B+Start cheat has been expanded with a configurable life count.

The value can now be edited instead of being fixed to a single predefined amount.

---

## 14. Title + Game Over Screen Editor Improvements

The Title + Game Over Screen Editor introduced in v1.1 has received compatibility improvements.

Raw screen data now safely supports the complete byte range:

`$00-$FF`

including preservation of `$FF` string terminators used by compatible ROM variants.

This improves Config import/export and compatibility with modified Battle City ROMs.

---

## 15. Stage Map / Game Settings Synchronization

Map-backed Game Settings views now refresh immediately after the main stage map is edited.

This ensures that:

- Spawn overlays use current terrain
- Terrain warnings use current map data
- Reopening the ROM is not required
- Reopening the settings window is not required

---

## 16. Drag-and-Drop ROM Opening

The Desktop editor now supports opening:

`*.nes`

files through drag-and-drop.

If the current ROM contains unsaved changes, QuarrelEx presents:

- `Save`
- `Don't Save`
- `Cancel`

before replacing the current ROM.

---

## 17. QuarrelExConfig v3

QuarrelEx v1.1.7 continues to use:

`QuarrelExConfig v3`

The main Config version has not been changed.

Desktop and Web continue to share:

`*.qexcfg.json`

Current Config data can include:

- Stage 1-70 maps
- Enemy Type / Count / Total
- Feature Flags
- Enemy Item Flags
- Demo data
- Title / Game Over Screen data
- TSA / Attr
- Palettes
- Flag / Fort TSA
- Player spawn positions
- Custom enemy spawn positions
- Enemy Pacing
- Base Exists
- Enemy Counter Display
- GAME OVER / Extra Life / 2P / Armor QXR settings

Strict validation remains enabled. Config files containing hard validation errors are rejected before modifying the current ROM.

---

## 18. Mid City2 / Mid City2 PS Compatibility

Dedicated compatibility IPS files are now included for:

- **Mid City2**
- **Mid City2 PS**

The compatibility patches are located under:

`patches/compatibility/`

After applying the appropriate compatibility IPS, these variants can be opened directly with the current QuarrelEx and can expose the current:

`QXR1 v5`

options while preserving their variant-specific content.

---

## 19. Desktop / Web Synchronization

The current release keeps the Desktop and Web editors aligned.

Current versions:

- Desktop Editor: **1.1.7**
- Web Editor: **1.6.7**

Both use QuarrelExConfig v3, allowing Config files to be exchanged between Desktop and Web.

---

## 20. 32KB BCEX Runtime 6.9.3

The recommended 32KB runtime for QuarrelEx v1.1.7 is:

`Runtime 6.9.3 / QXR1 v5`

Major capabilities and fixes include:

- Stage 1-70 independent maps
- Stage 1-70 independent player spawns
- Stage 1-70 custom enemy spawns
- Per-stage Enemy Pacing
- Per-stage Base Exists
- Enemy Counter Icons / Number
- GAME OVER Skip
- Extra Life Rules
- 2P Original / Win-Streak
- Armored Tank Original / One-Hit
- A+B+Start Cheat Lives
- Demo isolation from normal stage QXR overrides
- Correct #4 / #11 / #18 flashing bonus tanks
- Preservation of existing visible items
- Correct next-stage terrain/setup initialization
- No old-stage map flash when GAME OVER Skip is enabled

Current standard 32KB patch:

`patches/32KB/QuarrelEx_BCEX_32KB_Runtime6.9.3.ips`

---

## Compatibility Notes

### Upgrading from QuarrelEx v1.1

The editors can be upgraded directly to:

- Desktop 1.1.7
- Web 1.6.7

The Config format remains v3.

### 16KB BCEX

The 16KB Runtime / IPS remains unchanged from v1.1.

Most of the new per-stage QXR1 v5 runtime features in v1.1.7 target the current 32KB BCEX runtime.

### 32KB BCEX

To use the complete v1.1.7 runtime feature set and fixes, use the current:

`Runtime 6.9.3 / QXR1 v5`

patch.

---

## Highlights Since v1.1

In short, v1.1.7 extends the v1.1 Demo / Screen Editor release with:

- Per-stage `.qexstage.json` map + terrain import/export
- Map selection, move, copy, cut, paste and cross-stage clipboard workflow

1. Stage 1-70 independent P1/P2 player spawns
2. Stage 1-70 independent 1-8 enemy spawn points for 1P/2P
3. Per-stage Enemy Pacing
4. Per-stage Base Exists
5. Enemy Counter Icons / Number
6. Automatic Number display above 50 enemies
7. GAME OVER Skip
8. Extra Life Rules
9. 2P Original / Win-Streak
10. Armored Tank Original / One-Hit
11. A+B+Start Cheat Lives
12. Demo behavior restoration and QXR isolation
13. Correct #4 / #11 / #18 flashing tanks
14. Existing-item preservation when a new flashing tank spawns
15. Correct next-stage map / terrain initialization
16. GAME OVER Skip old-stage flash fix
17. Screen raw-byte `$00-$FF` compatibility
18. Drag-and-drop ROM opening with unsaved-change protection
19. Immediate Stage Map / Game Settings synchronization
20. Mid City2 / Mid City2 PS compatibility IPS files
21. Desktop 1.1.7 / Web 1.6.7 synchronization
22. 32KB Runtime 6.9.3 / QXR1 v5

---

## Notes

QuarrelEx contains editor source code and patch files only.

No copyrighted Battle City ROM image is distributed with this project. Users must provide their own legally obtained compatible ROM before applying IPS patches.

Battle City and related game content belong to their respective copyright holders. QuarrelEx is an independent project and is not affiliated with or endorsed by the original publisher.
