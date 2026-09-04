# QuarrelEx

![Version](https://img.shields.io/badge/release-v1.1.8-blue)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Web-0aa0c0)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

**QuarrelEx** 是一个面向《Battle City / 坦克大战》的 ROM 与关卡编辑器，同时提供 Windows 桌面端与 Web 端。当前版本把 70 关 BCEX、TSA/CHR 可视化、游戏规则、拖拽打开、Config v3，以及 32KB Runtime 的参数统一到同一套编辑流程中。

[English](README.md) · UI 语言：**简体中文 / English / 日本語**

> **本仓库不包含游戏 ROM 本体。** IPS 需要用户自己合法取得、并且校验值匹配的基础 ROM。

## 当前版本

| 组件 | 版本 |
|---|---:|
| QuarrelEx | 1.1.8 |
| Desktop | 1.1.8 |
| Web | 1.6.8 |
| Mobile Web UI | 1.0（Core 1.6.8） |
| Config | QuarrelExConfig v3 |
| 32KB BCEX Runtime | Runtime 6.9.4 / QXR1 v6 |

## 主要功能

- 支持原版 Battle City、BCEX 16KB、BCEX 32KB。
- Stage 1~70；当前 32KB Runtime 提供 70 张独立地图。
- Demo 地图编辑。
- Enemy Type / Count；支持的 BCEX 可设置 1~255 个敌人总数。
- TSA / CHR / 调色板 / Flag-Fort 可视化编辑。
- Title + Game Over Screen Editor。
- 原版玩家/敌人出生位置编辑。
- Stage 1~70 自定义敌人出生点：1P / 2P 可分别选择 Original 或 1~8 点，支持地图叠加、16px 吸附与地形警告。
- Stage 1~70 独立 **玩家** P1/P2 出生点，支持地图拖拽。
- 每关独立 1P/2P 出敌间隔和最大同时在场数量。
- 每关独立 Base Exists。
- A+B+Start 秘籍命数。
- Final GAME OVER Skip、分数加命、2P Original / Win-Streak。
- 400 分装甲坦克 Original / One-Hit：普通装甲可变成白色 1HP，闪光/带道具装甲保持原版耐久与道具流程。
- 每关独立右上角**敌人数显示**：总数 1~50 可选 Icons / Number；51~255 强制 Number，但保留原来的偏好设置。
- 自动闪光奖励坦克固定为本关出生序号 **#4 / #11 / #18**，EnemyTotal 超过 20 时也不会后移。
- 新闪光坦克出生时不再强制清除当前已有道具。
- 修复过关后的下一关地形/场景初始化异常。
- Skip ON 使用正确的 GAME OVER 清理流程，不再在返回标题前闪一帧关卡地图。
- Demo 不继承逐关 QXR 参数，保持原版/global 玩家出生、原版敌人出生循环、原版节奏，以及老巢附近停止射击的原版行为。
- 初始坦克等级可设 Lv0~Lv4；Runtime 6.9.4 / QXR1 v6 新增独立死亡等级 Lv0~Lv4。按住 B 连发、手枪/Lv4、我方加速、随机敌人顺序、取消队友互伤、敌人拾取道具、锁定初始状态等继续保留。
- 每关可单独导入 / 导出 `.qexstage.json`：包含当前 13×13 地图和该地图实际引用的 TSA/Attr 地形定义。
- 地图新增“选择 / 移动工具”：支持单块/矩形框选、拖动移动、Ctrl+拖动复制，以及 Ctrl+C / Ctrl+X / Ctrl+V / Delete。
- 新增独立的触控优先 **Mobile Web UI**（`web/QuarrelEx_Mobile.html`）：采用底部导航和手机工具抽屉，复用 Web 1.6.8 的编辑核心，不替换桌面向 Web 页面。
- Web 桌面版进一步改为编辑器式工作台：左侧固定 Map 工作区，右侧为独立滚动的 Inspector / Properties；Mobile 则采用全高地图页、地形抽屉和页面式属性面板。
- Web / Desktop 共用 Config v3。
- Save / Save As、Undo / Redo。
- 支持拖拽打开 `.nes`；当前 ROM 未保存时提供 **保存 / 不保存 / 取消**。
- 主地图修改后，Game Settings 中所有依赖地图的可视化立即刷新。

## 仓库结构

```text
QuarrelEx/
├─ desktop/
├─ web/
├─ patches/
│  ├─ 16KB/
│  └─ 32KB/
├─ docs/
├─ examples/
├─ locales/                 # zh-CN / en-US / ja-JP 共用语言资源
├─ tools/                   # 本地化同步/校验工具
├─ .github/workflows/
├─ README.md
├─ README.zh-CN.md
├─ CHANGELOG.md
└─ LICENSE
```

## 标准 IPS 基础 ROM

```text
Battle City (J)
Size:    24592 bytes
CRC32:   F599A07E
MD5:     cd4fe2e78df0696dbe652f02c19541a1
SHA-1:   e1061c9241b06a965fb7845cb951d921aca010ef
SHA-256: a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c
```

当前 32KB 版本先运行准备脚本，再应用：

```text
patches/32KB/QuarrelEx_BCEX_32KB_Runtime6.9.4.ips
```

详细步骤和校验见 [patches/README.md](patches/README.md)。

## Desktop

工程：

```text
desktop/QuarrelEx.sln
```

## Web

```text
web/QuarrelEx.html             # 自包含的桌面向 Web 编辑器
web/QuarrelEx_Mobile.html      # 独立的 Mobile UI
```

`QuarrelEx_Mobile.html` 与桌面向 Web 界面保持独立，不合并。手机端采用“地图 / 敌人 / TSA / 设置 / 更多”底部导航，同时继续使用相同的 ROM、Config v3、单关卡 `.qexstage.json` 和 Runtime 6.9.4 逻辑。

## Config v3

Web / Desktop 使用同一个 `*.qexcfg.json`。Screen 原始字节允许 `$00-$FF`，因此兼容改版使用的 `$FF` 字符串终止符可以正常导入/导出。

规范见 [docs/QuarrelExConfig_v3_Spec.txt](docs/QuarrelExConfig_v3_Spec.txt)。

## 版权说明

仓库只提供编辑器源码与 IPS，不提供游戏 ROM。

## License

MIT，见 [LICENSE](LICENSE)。

### 模拟器显示调色板
Web 与 Desktop 端可加载标准 192-byte NES 模拟器 `.pal` 文件（64 个 RGB 颜色），使地图、TSA/CHR、出生点与调色板选择器的预览色调与模拟器保持一致。该功能只属于编辑器显示设置，不修改 ROM 数据，也不写入 QuarrelExConfig v3。
