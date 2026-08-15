# QuarrelEx

![Version](https://img.shields.io/badge/release-v1.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Web-0aa0c0)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

**QuarrelEx** 是一个面向《Battle City / 坦克大战》的现代关卡与 ROM 编辑器，同时提供 Windows 桌面端与 Web 端。项目沿用了原 Quarrel 编辑器的工作思路，并加入 BCEX 扩展 ROM、70 关、TSA/CHR 可视化编辑、调色板、游戏规则、Config v3、撤销/重做等功能。

[English](README.md)

> **本仓库不包含《Battle City》ROM 本体。** 仓库中的 IPS 仅用于用户自己合法取得的、校验值完全匹配的基础 ROM。

## 当前正式版本

| 组件 | 版本 |
|---|---:|
| Desktop 桌面端 | 1.0 |
| Web 网页端 | 1.5.1 |
| 通用配置格式 | QuarrelExConfig v3 |
| 16KB BCEX Runtime | 内部修订 6.3 |
| 32KB BCEX Runtime | 内部修订 6.4.1 |

Phase/Runtime 修订号只用于开发追踪；对外正式发布统一称为 **QuarrelEx v1.0**。

## 主要功能

- 支持原版 Battle City，以及 BCEX 16KB / 32KB 格式。
- Stage 1~70；32KB 版本为 70 张真正独立地图。
- 每关 4 组 Enemy Type / Count；支持的 BCEX ROM 可将敌人总数设为 **1~255**。
- TSA/属性可视化编辑：Attr 只能选择 `0~3`，TL/TR/BL/BR 直接从 `$00~$FF` CHR Tile 中选择。
- 扩展地形：16KB 到 `$00~$1F`；32KB 到 `$00~$3F`，包含额外自定义槽。
- 调色板编辑器、Flag/Fort TSA Editor。
- 玩家1/2与敌人1/2/3出生点支持数值输入和鼠标拖拽。
- Ex 功能：按住 B 连发、手枪/Lv4、受击降级、我方加速、随机敌人顺序、取消队友互伤、敌人拾取道具、锁定初始状态等。
- Web/Desktop 共用 Config v3，包含游戏设置、Palette、TSA、Flag/Fort TSA、Stage 1~70地图、Enemy Type/Count/Total。
- 撤销/重做：`Ctrl+Z`、`Ctrl+Y`、`Ctrl+Shift+Z`。
- 完整的保存 / 另存为。
- Config v3 导入前完整校验，硬错误不会写入一半 ROM。

## 仓库结构

```text
QuarrelEx/
├─ desktop/                 # C# WinForms / .NET 8 完整工程
├─ web/                     # Web端与单文件版
├─ patches/
│  ├─ 16KB/
│  └─ 32KB/                 # 32KB准备脚本 + 最终IPS
├─ docs/
├─ examples/
├─ .github/workflows/
├─ README.md
├─ README.zh-CN.md
├─ CHANGELOG.md
└─ LICENSE
```

## IPS 基础 ROM

16KB IPS 直接应用到以下基础 ROM；32KB 版本先使用仓库内的准备脚本，从同一份原版 ROM 生成 32KB 工作底包，再应用 32KB IPS：

```text
名称:    Battle City (J)
大小:    24592 bytes
CRC32:   F599A07E
MD5:     cd4fe2e78df0696dbe652f02c19541a1
SHA-1:   e1061c9241b06a965fb7845cb951d921aca010ef
SHA-256: a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c
```

具体补丁步骤与结果校验值见 [patches/README.md](patches/README.md)。

## Desktop

运行环境：Windows 10/11 + .NET 8 Desktop Runtime。

源码使用 Visual Studio 2022 打开：

```text
desktop/QuarrelEx.sln
```

常用快捷键：

| 快捷键 | 功能 |
|---|---|
| `Ctrl+O` | 打开 ROM |
| `Ctrl+S` | 保存 |
| `Ctrl+Shift+S` | 另存为 |
| `Ctrl+Z` | 撤销 |
| `Ctrl+Y` / `Ctrl+Shift+Z` | 重做 |
| `F2` | 敌人编辑器 |
| `F3` | TSA/属性 |
| `F4` | 调色板 |
| `F5` | Flag TSA |
| `F6` | 游戏设置 |
| `F7` | Ex 选项 |
| `F8` | ROM 信息 |

右侧编辑功能采用独立工具窗口，可移动、缩放、最大化或放到第二显示器。

## Web

可直接使用：

```text
web/QuarrelEx.html
web/QuarrelEx_Standalone.html
```

`QuarrelEx_Standalone.html` 是单文件版本。支持 File System Access API 的浏览器可以直接保存回打开的 ROM；不支持时会退回下载保存。

## Enemy Type / 闪光标志

`$04` 是闪光/奖励标志位，并不是一个单独的 Enemy Type。

| 普通 | 强制闪光/奖励 |
|---|---|
| `$80` | `$84` |
| `$A0` | `$A4` |
| `$C0` | `$C4` |
| `$E0` | `$E4` |

详细说明见 [docs/Enemy_Types.md](docs/Enemy_Types.md)。

## Config v3

Web 和 Desktop 只接受/导出 `QuarrelExConfig Version 3`，扩展名建议使用：

```text
*.qexcfg.json
```

正式版会在导入前完整检查 Schema、版本、Palette、TSA、地图13×13、关卡连续性、EnemyTotal/Count、ROM能力等；只有全部硬错误检查通过后才正式写入当前 ROM。

规范见 [docs/QuarrelExConfig_v3_Spec.txt](docs/QuarrelExConfig_v3_Spec.txt)。

## 编译与 GitHub Actions

仓库已经包含 Windows GitHub Actions：`.github/workflows/build-desktop.yml`。Push/PR 修改桌面端源码时可以自动 Restore + Build .NET 8 WinForms 工程。

## ROM / 版权说明

仓库只提供编辑器源码与补丁，不提供游戏 ROM。

MIT License 只覆盖 QuarrelEx 自己编写的代码；Battle City 的 ROM、图像、音乐、音效及其他第三方资源仍归各自权利人所有。

为了方便公开发布，本仓库版源码**不打包从原 Quarrel EXE 提取的原始图标资源**；QuarrelEx 自己的工具栏功能图标由程序代码生成。

## License

MIT，见 [LICENSE](LICENSE)。
