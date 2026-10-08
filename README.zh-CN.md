# QuarrelEx

![Release](https://img.shields.io/badge/release-v1.1.10-blue)
![Desktop](https://img.shields.io/badge/Desktop-1.1.10-512BD4)
![Web](https://img.shields.io/badge/Web-1.6.14-0aa0c0)
![Mobile](https://img.shields.io/badge/Mobile-1.1.1-0aa0c0)
![License](https://img.shields.io/badge/license-MIT-green)

**QuarrelEx** 是面向《Battle City / 坦克大战》及 BCEX 的 ROM / 关卡编辑器，提供 Windows Desktop、自包含 Web 与 Mobile Web 版本。仓库保留完整 Desktop 源码、当前 Web/Mobile 编辑器、共享本地化与配置支持、正式文档，以及当前维护的 IPS 补丁。

[English](README.md) · UI：**简体中文 / English / 日本語**

> 仓库不包含游戏 ROM。IPS 补丁需要用户自行合法取得并校验匹配的基础 ROM。

## 发布组件

| 组件 | 版本 |
|---|---:|
| QuarrelEx | 1.1.10 |
| Desktop | 1.1.10 |
| Web | 1.6.14 |
| Mobile Web | 1.1.1 |
| 配置格式 | QuarrelExConfig v3 |
| BCEX 32KB | Final Runtime |

## 主要功能

- Stage 1~70；当前 32KB BCEX 提供 70 张独立地图。
- Enemy Type/Count、自定义敌人总数、逐关节奏、敌人/玩家出生点、Base Exists 与敌人数显示模式。
- TSA/CHR、调色板、Flag/Fort、标题与 Game Over 画面编辑。
- Desktop / Web 共用 Config v3 与逐关 `.qexstage.json`。
- 初始/死亡等级、A+B+Start 命数、GAME OVER Skip、分数加命、2P 规则、装甲规则、保护罩、炸弹、敌方 1UP、敌弹速度、原生 8 槽道具爆率、逐关奖励坦克频率等。
- 随机敌人：每辆独立随机 Type + HP 1~8，并同步可见装甲阶段。
- 敌人吃手枪后保持真实装甲耐久，并继续使用可破坏钢板的子弹。
- 可配置“基地被毁后继续保留 Lv4 消树林能力”。
- Web / Desktop 可加载标准 192-byte NES `.pal` 作为预览调色板。
- Mobile Web 与当前 Web 使用相同 ROM / Config 模型，并采用触控优先布局。

## 快速使用

### Desktop

使用 Visual Studio 2022 打开：

```text
desktop/QuarrelEx.sln
```

需要安装 .NET 8 与 **.NET 桌面开发**工作负载。

### Web

直接用现代浏览器打开：

```text
web/QuarrelEx.html
```

文件自包含，可离线运行。

### Mobile Web

手机或平板直接打开：

```text
web/QuarrelEx_Mobile.html
```

Mobile 1.1.1 与 Web 1.6.14 使用同一套 ROM / Config 数据模型。

## IPS 补丁

每条维护线只保留一个当前正式补丁：

```text
patches/16KB/QuarrelEx_BCEX_16KB_v1.0.ips
patches/32KB/QuarrelEx_BCEX_32KB_Final.ips
```

当前 32KB 补丁输入：

```text
大小:       40976 bytes
CRC32:      D2572735
SHA-256:    5e0b53e33a40166e5d31d50734c6059c96bc061301d2b77e85bd10a744e53291
```

应用后应得到：

```text
大小:       40976 bytes
CRC32:      E33746DA
SHA-256:    ab85c29a8ee0dd68078d5fce959147e6400105e1f55fdda975e13ce9c7c05aa8
```

应用前请阅读 [patches/README.md](patches/README.md)。

## 仓库结构

```text
QuarrelEx/
├─ desktop/          Windows 完整源码
├─ web/              Web + Mobile Web 编辑器
├─ patches/          当前 16KB / 32KB IPS
├─ docs/             规范与使用说明
├─ examples/         Config v3 示例
├─ locales/          zh-CN / en-US / ja-JP 语言资源
├─ tools/            本地化生成/校验工具
└─ .github/workflows/
```

## 文档

- [BCEX 32KB Final Runtime](docs/BCEX_32KB_Final.md)
- [BCEX 16KB](docs/BCEX_16KB.md)
- [QuarrelExConfig v3](docs/QuarrelExConfig_v3_Spec.txt)
- [单关卡包格式](docs/QuarrelExStage_v1_Spec.md)
- [敌人类型](docs/Enemy_Types.md)
- [TSA 指南](docs/TSA_Guide.md)
- [Screen Editor](docs/Screen_Editor.md)

## 开发

三语资源位于 `locales/`。修改后运行：

```text
python tools/generate_desktop_i18n.py
python tools/check_i18n.py
```

GitHub Actions 会校验本地化并构建 Desktop 工程。

## License

MIT，见 [LICENSE](LICENSE)。第三方声明见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
