using System.Globalization;
using System.Text.Json;
using QuarrelEx.Config;
using QuarrelEx.Controls;
using QuarrelEx.Core;
using QuarrelEx.Rendering;

namespace QuarrelEx;

public sealed class MainForm : Form
{
    private readonly EditorConfig _cfg = EditorConfig.LoadDefault();
    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private string? _filePath;
    private bool _dirty;
    private int _selectedTerrain = 0x0D;
    private readonly Stack<byte[]> _undo = new();
    private readonly Stack<byte[]> _redo = new();
    private byte[]? _savedBytes;
    private bool _refreshing;
    private readonly Dictionary<EditorToolKind, ToolWindowForm> _toolWindows = new();
    private readonly Dictionary<EditorToolKind, ToolStripMenuItem> _toolWindowMenuItems = new();

    private readonly TableLayoutPanel _rootLayout = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 4,
        Margin = Padding.Empty,
        Padding = Padding.Empty
    };

    private readonly ComboBox _stageCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 115 };
    private readonly StageCanvas _stageCanvas = new();
    private readonly Panel _mapViewport = new() { Dock = DockStyle.Fill, BackColor = SystemColors.ControlDark, Padding = new Padding(4) };
    private readonly FlowLayoutPanel _terrainPanel = new() { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true, Padding = new Padding(6) };
    private readonly DataGridView _enemyGrid = new();
    private readonly Label _enemySum = new() { AutoSize = true, MaximumSize = new Size(520, 0), Padding = new Padding(6) };
    private readonly TsaEditorControl _tsaEditor = new();
    private readonly PaletteEditorControl _paletteEditor = new();
    private readonly FlagTsaEditorControl _flagTsaEditor = new();
    private readonly GameSettingsControl _gameSettings = new();
    private readonly TextBox _infoBox = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 10f) };
    private readonly ToolStripStatusLabel _status = new() { Text = "打开 Battle City / Battle City Ex ROM 后开始编辑。" };
    private readonly ToolStripMenuItem _saveMenu = new("保存(&S)") { ShortcutKeys = Keys.Control | Keys.S, Enabled = false };
    private readonly ToolStripMenuItem _saveAsMenu = new("另存为(&A)...") { ShortcutKeys = Keys.Control | Keys.Shift | Keys.S, Enabled = false };
    private readonly ToolStripMenuItem _undoMenu = new("撤销(&U)") { ShortcutKeys = Keys.Control | Keys.Z, Enabled = false };
    private readonly ToolStripMenuItem _redoMenu = new("重做(&R)") { ShortcutKeys = Keys.Control | Keys.Y, Enabled = false };
    private readonly ToolStripButton _saveToolButton = new("保存") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "保存 ROM (Ctrl+S)", Enabled = false };
    private readonly ToolStripButton _undoToolButton = new("撤销") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "撤销 (Ctrl+Z)", Enabled = false };
    private readonly ToolStripButton _redoToolButton = new("重做") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "重做 (Ctrl+Y / Ctrl+Shift+Z)", Enabled = false };
    private readonly ToolStripMenuItem _convertMenu = new("转换为 32KB Ex Overlay（Legacy）") { Enabled = false };
    private readonly Label _mapNote = new() { AutoSize = true, ForeColor = Color.DimGray, Padding = new Padding(3) };

    private readonly CheckBox _autoFireCheck = new() { Text = "按住 B 键自动连发", AutoSize = true };
    private readonly CheckBox _pistolLv4Check = new() { Text = "启用手枪 / Lv4（手枪直升 Lv3）", AutoSize = true };
    private readonly CheckBox _downgradeCheck = new() { Text = "被击中时逐级降低（Lv0 才爆炸）", AutoSize = true };
    private readonly CheckBox _treeDestroyCheck = new() { Text = "Lv4 子弹可以消除树林", AutoSize = true };
    private readonly CheckBox _fastMoveCheck = new() { Text = "我方坦克加速移动（Phase 6.2）", AutoSize = true };
    private readonly CheckBox _randomEnemyCheck = new() { Text = "随机敌坦克出现顺序（保持各类型总数量）", AutoSize = true };
    private readonly CheckBox _enemyPickupCheck = new() { Text = "敌人可以拾取道具（32KB 70-map）", AutoSize = true };
    private readonly CheckBox _noFriendlyFireCheck = new() { Text = "取消队友互伤", AutoSize = true };
    private readonly CheckBox _lockInitialCheck = new() { Text = "锁定初始状态（死亡后恢复游戏设置中的初始等级）", AutoSize = true };
    private readonly Dictionary<EnemyItemEffect, CheckBox> _enemyItemChecks = new();
    private readonly Label _exOptionsInfo = new() { AutoSize = true, MaximumSize = new Size(520, 0), ForeColor = Color.DimGray, Padding = new Padding(0, 8, 0, 8) };
    private readonly Button _presetOriginalButton = new() { Text = "原版行为", AutoSize = true };
    private readonly Button _presetRecommendedButton = new() { Text = "推荐 Ex", AutoSize = true };

    private static readonly Dictionary<int, string> TerrainNames = new()
    {
        [0x00]="砖右半", [0x01]="砖下半", [0x02]="砖左半", [0x03]="砖上半", [0x04]="整砖",
        [0x05]="钢右半", [0x06]="钢下半", [0x07]="钢左半", [0x08]="钢上半", [0x09]="整钢",
        [0x0A]="整水", [0x0B]="树林", [0x0C]="冰", [0x0D]="空白", [0x0E]="未使用/扩展标记", [0x0F]="空白(内部)",
        [0x10]="右下单格水", [0x11]="左下单格水", [0x12]="上横双格水", [0x13]="下横双格水",
        [0x14]="左上单格水", [0x15]="右上单格水", [0x16]="左竖双格水", [0x17]="右竖双格水",
        [0x18]="左上单格红砖", [0x19]="右上单格红砖", [0x1A]="左下单格红砖", [0x1B]="右下单格红砖",
        [0x1C]="左上单格钢板", [0x1D]="右上单格钢板", [0x1E]="左下单格钢板", [0x1F]="右下单格钢板"
    };

    public MainForm()
    {
        Text = "Quarrel Ex - Battle City / Ex Editor";
        StartPosition = FormStartPosition.CenterScreen;
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Keep the WinForms default only if the executable icon cannot be read.
        }

        // Use DPI rather than font autoscaling. This avoids the common WinForms
        // double-scaling/clipping problem when Windows display scaling is 125%+
        // and also makes per-monitor DPI changes predictable.
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        // v0.9.2 targets 1366x768 / 1280x720 as first-class layouts.
        // The older 1280x760 client area exceeded the usable height once the
        // title bar/taskbar were included on many 768p systems.
        ClientSize = new Size(1180, 660);
        MinimumSize = new Size(820, 520);

        BuildRootLayout();
        ConfigureEnemyGrid();
        ConfigureTsaEditor();
        ConfigureExOptions();
        RefreshExOptions();

        _stageCanvas.CellPaintRequested += StageCanvas_CellPaintRequested;
        _stageCanvas.CellPickRequested += StageCanvas_CellPickRequested;
        _stageCombo.SelectedIndexChanged += (_, _) => RefreshStageView();
        FormClosing += MainForm_FormClosing;
        Shown += (_, _) => { FitToWorkingArea(); FitStageCanvasToViewport(); };
        Resize += (_, _) =>
        {
            if (WindowState != FormWindowState.Minimized) FitStageCanvasToViewport();
        };
        DpiChanged += (_, _) =>
        {
            if (!IsHandleCreated) return;
            BeginInvoke(new Action(() =>
            {
                if (_renderer is not null) BuildTerrainButtons();
                FitStageCanvasToViewport();
            }));
        };
    }

    private void BuildRootLayout()
    {
        SuspendLayout();
        try
        {
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var menu = BuildMenu();
            var toolbar = BuildToolbar();
            var content = BuildMainLayout();
            var status = BuildStatus();

            _rootLayout.Controls.Add(menu, 0, 0);
            _rootLayout.Controls.Add(toolbar, 0, 1);
            _rootLayout.Controls.Add(content, 0, 2);
            _rootLayout.Controls.Add(status, 0, 3);
            Controls.Add(_rootLayout);
            MainMenuStrip = menu;
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    private void FitToWorkingArea()
    {
        var working = Screen.FromControl(this).WorkingArea;
        var newWidth = Math.Min(Width, working.Width);
        var newHeight = Math.Min(Height, working.Height);
        if (newWidth != Width || newHeight != Height)
        {
            Size = new Size(newWidth, newHeight);
            Location = new Point(
                working.Left + Math.Max(0, (working.Width - newWidth) / 2),
                working.Top + Math.Max(0, (working.Height - newHeight) / 2));
        }
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("文件(&F)");
        var open = new ToolStripMenuItem("打开 ROM(&O)...") { ShortcutKeys = Keys.Control | Keys.O };
        open.Click += (_, _) => OpenRom();
        _saveMenu.Click += (_, _) => SaveRom(false);
        _saveAsMenu.Click += (_, _) => SaveRom(true);
        var exit = new ToolStripMenuItem("退出(&X)");
        exit.Click += (_, _) => Close();
        var importConfig = new ToolStripMenuItem("导入配置(&I)...");
        importConfig.Click += (_, _) => ImportSharedConfig();
        var exportConfig = new ToolStripMenuItem("导出配置(&C)...");
        exportConfig.Click += (_, _) => ExportSharedConfig();
        file.DropDownItems.AddRange([open, _saveMenu, _saveAsMenu, new ToolStripSeparator(), importConfig, exportConfig, new ToolStripSeparator(), exit]);

        var edit = new ToolStripMenuItem("编辑(&E)");
        _undoMenu.Click += (_, _) => Undo();
        _redoMenu.Click += (_, _) => Redo();
        edit.DropDownItems.AddRange([_undoMenu, _redoMenu]);

        var tools = new ToolStripMenuItem("工具(&T)");
        _convertMenu.Click += (_, _) => ConvertToExpanded();
        tools.DropDownItems.Add(_convertMenu);

        var window = BuildWindowMenu();

        var help = new ToolStripMenuItem("帮助(&H)");
        var helpZh = new ToolStripMenuItem("中文帮助(&C)") { ShortcutKeys = Keys.F1 };
        helpZh.Click += (_, _) => OpenHelp("zh-CN");
        var helpEn = new ToolStripMenuItem("English Help(&E)") { ShortcutKeys = Keys.Shift | Keys.F1 };
        helpEn.Click += (_, _) => OpenHelp("en-US");
        var tsaHelpZh = new ToolStripMenuItem("TSA / 属性说明（中文）(&T)");
        tsaHelpZh.Click += (_, _) => OpenTsaHelp("zh-CN");
        var tsaHelpEn = new ToolStripMenuItem("TSA / Attribute Guide (English)(&G)");
        tsaHelpEn.Click += (_, _) => OpenTsaHelp("en-US");
        var about = new ToolStripMenuItem("关于 Quarrel Ex(&A)");
        about.Click += (_, _) => ShowAbout();
        help.DropDownItems.AddRange([
            helpZh, helpEn,
            new ToolStripSeparator(),
            tsaHelpZh, tsaHelpEn,
            new ToolStripSeparator(),
            about
        ]);

        menu.Items.AddRange([file, edit, tools, window, help]);
        menu.Dock = DockStyle.Fill;
        return menu;
    }

    private ToolStrip BuildToolbar()
    {
        var tool = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            Dock = DockStyle.Top,
            ImageScalingSize = new Size(20, 20),
            Padding = new Padding(2, 1, 2, 1)
        };
        tool.Items.Add(new ToolStripLabel("关卡:"));
        tool.Items.Add(new ToolStripControlHost(_stageCombo));

        var open = new ToolStripButton("打开") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "打开 ROM (Ctrl+O)" };
        open.Click += (_, _) => OpenRom();
        _saveToolButton.Click += (_, _) => SaveRom(false);
        tool.Items.Add(new ToolStripSeparator());
        tool.Items.Add(open);
        tool.Items.Add(_saveToolButton);
        _undoToolButton.Click += (_, _) => Undo();
        _redoToolButton.Click += (_, _) => Redo();
        tool.Items.Add(_undoToolButton);
        tool.Items.Add(_redoToolButton);
        tool.Items.Add(new ToolStripSeparator());

        // Quarrel-style editor launchers. The original application icon is kept
        // for the executable/window icon; these compact glyphs identify each
        // separate editor window without consuming much 720p/768p space.
        AddToolButton(tool, EditorToolKind.Enemy, "敌人编辑器", Keys.F2);
        AddToolButton(tool, EditorToolKind.Tsa, "TSA / 属性编辑器", Keys.F3);
        AddToolButton(tool, EditorToolKind.Palette, "调色板编辑器", Keys.F4);
        AddToolButton(tool, EditorToolKind.FlagTsa, "Flag TSA Editor", Keys.F5);
        AddToolButton(tool, EditorToolKind.GameSettings, "游戏设置", Keys.F6);
        AddToolButton(tool, EditorToolKind.ExOptions, "Ex 选项", Keys.F7);
        AddToolButton(tool, EditorToolKind.RomInfo, "ROM 信息", Keys.F8);
        return tool;
    }

    private void AddToolButton(ToolStrip strip, EditorToolKind kind, string text, Keys shortcut)
    {
        var button = new ToolStripButton
        {
            Image = EditorToolIcons.Create(kind),
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            ToolTipText = $"{text} ({shortcut})",
            Tag = kind
        };
        button.Click += (_, _) => ShowToolWindow(kind);
        strip.Items.Add(button);
    }

    private Control BuildMainLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(4),
            Margin = Padding.Empty,
            AutoScroll = true
        };
        // v0.9.3 returns to the original Quarrel workflow: the main form is
        // dedicated to map + terrain, while data editors are independent
        // modeless windows launched from the toolbar or Window menu.
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

        var mapBox = new GroupBox { Text = "地图 13×13", Dock = DockStyle.Fill, MinimumSize = new Size(360, 0), Margin = new Padding(2) };
        var mapLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        mapLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mapLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _mapViewport.Controls.Add(_stageCanvas);
        _mapViewport.Resize += (_, _) => FitStageCanvasToViewport();
        mapLayout.Controls.Add(_mapViewport, 0, 0);
        mapLayout.Controls.Add(_mapNote, 0, 1);
        mapBox.Controls.Add(mapLayout);

        var terrainBox = new GroupBox
        {
            Text = "地形块（左键选择，地图右键吸取）",
            Dock = DockStyle.Fill,
            MinimumSize = new Size(230, 0),
            Margin = new Padding(2)
        };
        terrainBox.Controls.Add(_terrainPanel);

        layout.Controls.Add(mapBox, 0, 0);
        layout.Controls.Add(terrainBox, 1, 0);
        return layout;
    }

    private ToolStripMenuItem BuildWindowMenu()
    {
        var menu = new ToolStripMenuItem("窗口(&W)");
        AddToolWindowMenuItem(menu, EditorToolKind.Enemy, "敌人编辑器(&E)", Keys.F2);
        AddToolWindowMenuItem(menu, EditorToolKind.Tsa, "TSA / 属性编辑器(&T)", Keys.F3);
        AddToolWindowMenuItem(menu, EditorToolKind.Palette, "调色板编辑器(&P)", Keys.F4);
        AddToolWindowMenuItem(menu, EditorToolKind.FlagTsa, "Flag TSA Editor(&F)", Keys.F5);
        AddToolWindowMenuItem(menu, EditorToolKind.GameSettings, "游戏设置(&G)", Keys.F6);
        AddToolWindowMenuItem(menu, EditorToolKind.ExOptions, "Ex 选项(&X)", Keys.F7);
        AddToolWindowMenuItem(menu, EditorToolKind.RomInfo, "ROM 信息(&I)", Keys.F8);
        menu.DropDownItems.Add(new ToolStripSeparator());
        var showAll = new ToolStripMenuItem("显示全部工具窗口(&A)");
        showAll.Click += (_, _) => ShowAllToolWindows();
        var hideAll = new ToolStripMenuItem("隐藏全部工具窗口(&H)");
        hideAll.Click += (_, _) => HideAllToolWindows();
        menu.DropDownItems.Add(showAll);
        menu.DropDownItems.Add(hideAll);
        return menu;
    }

    private void AddToolWindowMenuItem(ToolStripMenuItem menu, EditorToolKind kind, string text, Keys shortcut)
    {
        var item = new ToolStripMenuItem(text)
        {
            ShortcutKeys = shortcut,
            ShowShortcutKeys = true,
            CheckOnClick = false,
            Image = EditorToolIcons.Create(kind),
            Tag = kind
        };
        item.Click += (_, _) => ShowToolWindow(kind);
        _toolWindowMenuItems[kind] = item;
        menu.DropDownItems.Add(item);
    }

    private void ShowToolWindow(EditorToolKind kind)
    {
        if (!_toolWindows.TryGetValue(kind, out var window) || window.IsDisposed)
        {
            window = CreateToolWindow(kind);
            _toolWindows[kind] = window;
            window.VisibleChanged += (_, _) => UpdateToolWindowMenuChecks();
            window.FormClosed += (_, _) => { _toolWindows.Remove(kind); UpdateToolWindowMenuChecks(); };

            // Ex Options is created lazily as a modeless tool window.  Before
            // this point the seven EnemyItemEffect check boxes do not exist,
            // therefore the ROM-load RefreshExOptions() call cannot initialize
            // their Enabled state.  Refresh once immediately after creation so
            // parent/child state is correct the very first time the window opens.
            if (kind == EditorToolKind.ExOptions)
                RefreshExOptions();
        }

        if (!window.Visible)
        {
            PositionToolWindow(window);
            window.Show(this);
        }
        else
        {
            if (window.WindowState == FormWindowState.Minimized) window.WindowState = FormWindowState.Normal;
            window.BringToFront();
            window.Activate();
        }
        UpdateToolWindowMenuChecks();
    }

    private ToolWindowForm CreateToolWindow(EditorToolKind kind)
    {
        var (title, size) = kind switch
        {
            EditorToolKind.Enemy => ("Quarrel Ex - 敌人编辑器", new Size(560, 410)),
            EditorToolKind.Tsa => ("Quarrel Ex - TSA / 属性编辑器", new Size(820, 560)),
            EditorToolKind.Palette => ("Quarrel Ex - 调色板编辑器", new Size(520, 430)),
            EditorToolKind.FlagTsa => ("Quarrel Ex - Flag TSA Editor", new Size(600, 500)),
            EditorToolKind.GameSettings => ("Quarrel Ex - 游戏设置", new Size(620, 690)),
            EditorToolKind.ExOptions => ("Quarrel Ex - Ex 选项", new Size(580, 600)),
            _ => ("Quarrel Ex - ROM 信息", new Size(600, 500))
        };
        return new ToolWindowForm(kind, title, BuildToolWindowContent(kind), size, Icon);
    }

    private Control BuildToolWindowContent(EditorToolKind kind)
    {
        switch (kind)
        {
            case EditorToolKind.Enemy:
            {
                var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(6) };
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.Controls.Add(_enemyGrid, 0, 0);
                root.Controls.Add(_enemySum, 0, 1);
                root.Controls.Add(new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(530, 0),
                    ForeColor = Color.DimGray,
                    Padding = new Padding(6, 2, 6, 6),
                    Text = "Type 为原始字节：常用 $80/$A0/$C0/$E0。bit2（$04）是闪光/奖励标志，因此 $84/$A4/$C4/$E4 可强制该类型带闪光。原版流程还会自动给第4、11、18辆出生敌人加 $04。"
                }, 0, 2);
                return root;
            }
            case EditorToolKind.Tsa:
            {
                var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                var header = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(6, 3, 6, 3), Margin = Padding.Empty };
                var hint = new Label { AutoSize = true, MaximumSize = new Size(520, 38), AutoEllipsis = true, Text = "Attr=0~3；点击 TL/TR/BL/BR 选择 8×8 CHR Tile。底部横向滚动条可查看全部列。", ForeColor = Color.DimGray, Padding = new Padding(0, 5, 8, 0) };
                var zh = new Button { Text = "? TSA说明", AutoSize = true, Margin = new Padding(2) };
                zh.Click += (_, _) => OpenTsaHelp("zh-CN");
                var en = new Button { Text = "English", AutoSize = true, Margin = new Padding(2) };
                en.Click += (_, _) => OpenTsaHelp("en-US");
                header.Controls.Add(hint); header.Controls.Add(zh); header.Controls.Add(en);
                root.Controls.Add(header, 0, 0); root.Controls.Add(_tsaEditor, 0, 1);
                return root;
            }
            case EditorToolKind.Palette:
                return _paletteEditor;
            case EditorToolKind.FlagTsa:
                return _flagTsaEditor;
            case EditorToolKind.GameSettings:
                return _gameSettings;
            case EditorToolKind.ExOptions:
                return BuildExOptionsPanel();
            default:
                return _infoBox;
        }
    }

    private void PositionToolWindow(Form window)
    {
        var working = Screen.FromControl(this).WorkingArea;
        var width = Math.Min(window.Width, working.Width);
        var height = Math.Min(window.Height, working.Height);
        window.Size = new Size(width, height);

        // Prefer the free area to the right of the main window, otherwise center
        // over the working area. This keeps the modeless windows reachable on 768p.
        var rightX = Right + 8;
        var x = rightX + width <= working.Right ? rightX : working.Left + Math.Max(0, (working.Width - width) / 2);
        var y = Math.Clamp(Top + 36, working.Top, Math.Max(working.Top, working.Bottom - height));
        window.Location = new Point(x, y);
    }

    private void ShowAllToolWindows()
    {
        foreach (EditorToolKind kind in Enum.GetValues<EditorToolKind>()) ShowToolWindow(kind);
    }

    private void HideAllToolWindows()
    {
        foreach (var window in _toolWindows.Values) if (!window.IsDisposed) window.Hide();
        UpdateToolWindowMenuChecks();
    }

    private void UpdateToolWindowMenuChecks()
    {
        foreach (var pair in _toolWindowMenuItems)
            pair.Value.Checked = _toolWindows.TryGetValue(pair.Key, out var w) && !w.IsDisposed && w.Visible;
    }

    private void DisposeToolWindows()
    {
        foreach (var window in _toolWindows.Values.ToArray())
        {
            if (!window.IsDisposed) window.Dispose();
        }
        _toolWindows.Clear();
        UpdateToolWindowMenuChecks();
    }

    private void FitStageCanvasToViewport()
    {
        if (_mapViewport.ClientSize.Width <= 0 || _mapViewport.ClientSize.Height <= 0) return;
        _stageCanvas.FitToViewport(_mapViewport.ClientSize);
        _stageCanvas.Location = new Point(
            Math.Max(_mapViewport.Padding.Left, (_mapViewport.ClientSize.Width - _stageCanvas.Width) / 2),
            Math.Max(_mapViewport.Padding.Top, (_mapViewport.ClientSize.Height - _stageCanvas.Height) / 2));
    }

    private void OpenHelp(string language)
    {
        var fileName = language.Equals("en-US", StringComparison.OrdinalIgnoreCase)
            ? "Help_en-US.txt"
            : "Help_zh-CN.txt";
        var title = language.Equals("en-US", StringComparison.OrdinalIgnoreCase)
            ? "Quarrel Ex - English Help"
            : "Quarrel Ex - 中文帮助";
        OpenHelpDocument(title, fileName);
    }

    private void OpenTsaHelp(string language)
    {
        var fileName = language.Equals("en-US", StringComparison.OrdinalIgnoreCase)
            ? "TSA_Help_en-US.txt"
            : "TSA_Help_zh-CN.txt";
        var title = language.Equals("en-US", StringComparison.OrdinalIgnoreCase)
            ? "Quarrel Ex - TSA / Attribute Guide"
            : "Quarrel Ex - TSA / 属性说明";
        OpenHelpDocument(title, fileName);
    }

    private void OpenHelpDocument(string title, string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Help", fileName);
        using var help = new HelpViewerForm(title, path);
        help.ShowDialog(this);
    }

    private void ShowAbout()
    {
        MessageBox.Show(this,
            "Quarrel Ex v1.0\r\nBattle City / Battle City Ex Editor\r\n\r\n" +
            "配置文件升级为 QuarrelExConfig v3：除全局设置外，现在还包含关卡地图与每关 Enemy Type / Count / Total，并继续与 Web 版通用。\r\n" +
            "敌人、TSA、调色板、Flag TSA、游戏设置、Ex选项与ROM信息继续使用独立工具窗口。",
            "关于 Quarrel Ex", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private Control BuildExOptionsPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12)
        };

        var title = new Label
        {
            Text = "Battle City Ex v2 功能开关",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 6)
        };
        panel.Controls.Add(title);
        panel.Controls.Add(_exOptionsInfo);
        panel.Controls.Add(_autoFireCheck);
        panel.Controls.Add(_pistolLv4Check);
        panel.Controls.Add(_downgradeCheck);
        panel.Controls.Add(_treeDestroyCheck);

        var future = new Label
        {
            Text = "扩展游戏功能：",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 14, 0, 4)
        };
        panel.Controls.Add(future);
        panel.Controls.Add(_fastMoveCheck);
        panel.Controls.Add(_randomEnemyCheck);
        panel.Controls.Add(_enemyPickupCheck);
        panel.Controls.Add(_noFriendlyFireCheck);
        panel.Controls.Add(_lockInitialCheck);

        var enemyEffects = new GroupBox { Text = "敌人拾取后的效果（32KB）", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(8) };
        var enemyFlow = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        foreach (var pair in new[] {
            (EnemyItemEffect.Helmet, "头盔"), (EnemyItemEffect.Clock, "时钟"), (EnemyItemEffect.Shovel, "铲子"),
            (EnemyItemEffect.Star, "星星"), (EnemyItemEffect.Grenade, "手雷"), (EnemyItemEffect.Tank, "坦克"), (EnemyItemEffect.Pistol, "手枪") })
        {
            // Start disabled. RefreshExOptions() enables these only when the
            // parent "enemy can pick up power-ups" option is actually checked.
            // This also prevents a one-frame incorrect enabled state while the
            // lazily-created tool window is being shown.
            var cb = new CheckBox { Text = pair.Item2, AutoSize = true, Enabled = false };
            var effect = pair.Item1;
            cb.CheckedChanged += (_, _) => EnemyItemEffectChanged(cb, effect);
            _enemyItemChecks[effect] = cb; enemyFlow.Controls.Add(cb);
        }
        enemyEffects.Controls.Add(enemyFlow); panel.Controls.Add(enemyEffects);

        var presets = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 14, 0, 0) };
        presets.Controls.Add(_presetOriginalButton);
        presets.Controls.Add(_presetRecommendedButton);
        panel.Controls.Add(presets);
        return panel;
    }

    private void ConfigureExOptions()
    {
        _autoFireCheck.CheckedChanged += (_, _) => FeatureCheckChanged(_autoFireCheck, ExFeature.AutoFireB);
        _pistolLv4Check.CheckedChanged += (_, _) => FeatureCheckChanged(_pistolLv4Check, ExFeature.PistolLevel4);
        _downgradeCheck.CheckedChanged += (_, _) => FeatureCheckChanged(_downgradeCheck, ExFeature.DowngradeOnHit);
        _treeDestroyCheck.CheckedChanged += (_, _) => FeatureCheckChanged(_treeDestroyCheck, ExFeature.Level4DestroyTrees);
        _fastMoveCheck.CheckedChanged += (_, _) => FeatureCheckChanged(_fastMoveCheck, ExFeature.PlayerFastMove);
        _randomEnemyCheck.CheckedChanged += (_, _) => FeatureCheckChanged(_randomEnemyCheck, ExFeature.RandomEnemySpawn);
        _enemyPickupCheck.CheckedChanged += (_, _) => FeatureCheckChanged(_enemyPickupCheck, ExFeature.EnemyPowerUpPickup);
        _noFriendlyFireCheck.CheckedChanged += (_, _) => FeatureCheckChanged(_noFriendlyFireCheck, ExFeature.NoFriendlyFire);
        _lockInitialCheck.CheckedChanged += (_, _) => LockInitialChanged();
        _presetOriginalButton.Click += (_, _) => ApplyFeaturePreset(0x00, "已切换为原版行为预设（已实现的 Ex 功能全部关闭）。");
        _presetRecommendedButton.Click += (_, _) => ApplyFeaturePreset(0xA7, "已应用推荐 Ex 预设：B连发 + 手枪/Lv4 + 逐级掉级 + Lv4消树 + 取消队友互伤；加速移动保持关闭，可单独启用。");
    }

    private void FeatureCheckChanged(CheckBox box, ExFeature feature)
    {
        if (_refreshing || _rom is null || !_rom.HasExV2Config) return;
        try
        {
            PushUndo();
            _rom.SetFeature(feature, box.Checked);
            MarkDirty();
            RefreshExOptions();
            _infoBox.Text = _rom.Describe();
            SetStatus($"Ex FeatureFlags = ${_rom.FeatureFlags:X2}", false);
        }
        catch (Exception ex) { SetStatus(ex.Message, true); }
    }

    private void LockInitialChanged()
    {
        if (_refreshing || _rom is null || !_rom.SupportsLockInitialState) return;
        try
        {
            PushUndo();
            _rom.SetLockInitialState(_lockInitialCheck.Checked);
            MarkDirty();
            _gameSettings.RefreshValues();
            _infoBox.Text = _rom.Describe();
            SetStatus("锁定初始状态已更新。", false);
        }
        catch (Exception ex) { SetStatus(ex.Message, true); RefreshExOptions(); }
    }

    private void EnemyItemEffectChanged(CheckBox box, EnemyItemEffect effect)
    {
        if (_refreshing || _rom is null || !_rom.SupportsEnemyPowerUpPickup) return;
        try
        {
            PushUndo(); _rom.SetEnemyItemEffect(effect, box.Checked); MarkDirty();
            _infoBox.Text = _rom.Describe(); SetStatus($"EnemyItemFlags = ${_rom.EnemyItemFlags:X2}", false);
        }
        catch (Exception ex) { SetStatus(ex.Message, true); }
    }

    private void ApplyFeaturePreset(byte flags, string message)
    {
        if (_rom is null || !_rom.HasExV2Config) return;
        try
        {
            PushUndo();
            _rom.SetFeatureFlags(flags);
            MarkDirty();
            RefreshExOptions();
            _infoBox.Text = _rom.Describe();
            SetStatus(message, false);
        }
        catch (Exception ex) { SetStatus(ex.Message, true); }
    }

    private void RefreshExOptions()
    {
        var hasV2 = _rom is { HasExV2Config: true };
        var oldRefreshing = _refreshing;
        _refreshing = true;
        try
        {
            _autoFireCheck.Checked = hasV2 && _rom!.IsFeatureEnabled(ExFeature.AutoFireB);
            _pistolLv4Check.Checked = hasV2 && _rom!.IsFeatureEnabled(ExFeature.PistolLevel4);
            _downgradeCheck.Checked = hasV2 && _rom!.IsFeatureEnabled(ExFeature.DowngradeOnHit);
            _treeDestroyCheck.Checked = hasV2 && _rom!.IsFeatureEnabled(ExFeature.Level4DestroyTrees);
            _fastMoveCheck.Checked = hasV2 && _rom!.SupportsPlayerFastMove && _rom.IsFeatureEnabled(ExFeature.PlayerFastMove);
            _randomEnemyCheck.Checked = hasV2 && _rom!.IsFeatureEnabled(ExFeature.RandomEnemySpawn);
            _enemyPickupCheck.Checked = hasV2 && _rom!.IsFeatureEnabled(ExFeature.EnemyPowerUpPickup);
            _noFriendlyFireCheck.Checked = hasV2 && _rom!.IsFeatureEnabled(ExFeature.NoFriendlyFire);
            _lockInitialCheck.Checked = hasV2 && _rom!.LockInitialState;
            foreach (var pair in _enemyItemChecks) pair.Value.Checked = hasV2 && _rom!.IsEnemyItemEffectEnabled(pair.Key);

            _autoFireCheck.Enabled = hasV2;
            _pistolLv4Check.Enabled = hasV2;
            _downgradeCheck.Enabled = hasV2;
            _treeDestroyCheck.Enabled = hasV2 && _pistolLv4Check.Checked;
            _fastMoveCheck.Enabled = hasV2 && _rom!.SupportsPlayerFastMove;
            _randomEnemyCheck.Enabled = hasV2;
            _enemyPickupCheck.Enabled = hasV2 && _rom!.SupportsEnemyPowerUpPickup;
            _noFriendlyFireCheck.Enabled = hasV2 && _rom!.SupportsNoFriendlyFire;
            _lockInitialCheck.Enabled = hasV2 && _rom!.SupportsLockInitialState;
            foreach (var pair in _enemyItemChecks) pair.Value.Enabled = hasV2 && _rom!.SupportsEnemyPowerUpPickup && _enemyPickupCheck.Checked;
            _presetOriginalButton.Enabled = hasV2;
            _presetRecommendedButton.Enabled = hasV2;

            if (_rom is null)
                _exOptionsInfo.Text = "打开 ROM 后显示功能状态。";
            else if (_rom.IsOriginal)
                _exOptionsInfo.Text = "Battle City 原版 ROM：没有 BCEX v2 配置块，Ex 功能不可用。";
            else if (!hasV2)
                _exOptionsInfo.Text = "Legacy Ex ROM：未检测到 BCEX v2 配置块。请使用 Battle City Ex v2 ROM；不能只写 Flag，因为旧 ROM 中没有对应功能程序。";
            else
                _exOptionsInfo.Text = $"BCEX v2 / FeatureFlags=${_rom.FeatureFlags:X2} / LayoutFlags=${_rom.LayoutFlags:X2}。功能开关只修改配置字节，不会 NOP/JMP 改写程序本体。" +
                    (_rom!.SupportsCustomEnemyTotal ? " 当前ROM支持每关1~255总敌人数。" : string.Empty) +
                    (_rom.SupportsEnemyPowerUpPickup ? $" 32KB EnemyItemFlags=${_rom.EnemyItemFlags:X2}。" : string.Empty) +
                    (_rom.SupportsPlayerFastMove ? " 支持我方坦克加速移动。" : string.Empty);
        }
        finally { _refreshing = oldRefreshing; }
    }

    private StatusStrip BuildStatus()
    {
        var status = new StatusStrip { Dock = DockStyle.Fill };
        status.Items.Add(_status);
        return status;
    }

    private void ConfigureEnemyGrid()
    {
        _enemyGrid.Dock = DockStyle.Fill;
        _enemyGrid.AllowUserToAddRows = false;
        _enemyGrid.AllowUserToDeleteRows = false;
        _enemyGrid.RowHeadersVisible = false;
        _enemyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _enemyGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _enemyGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _enemyGrid.Columns.Add("slot", "槽位");
        _enemyGrid.Columns.Add("type", "Type (Hex)");
        _enemyGrid.Columns.Add("count", "数量");
        _enemyGrid.Columns[0].ReadOnly = true;
        _enemyGrid.CellBeginEdit += (_, _) => { if (!_refreshing) PushUndo(); };
        _enemyGrid.CellEndEdit += EnemyGrid_CellEndEdit;
    }

    private void ConfigureTsaEditor()
    {
        _tsaEditor.BeforeEdit += (_, _) => { if (!_refreshing) PushUndo(); };
        _tsaEditor.DataChanged += (_, _) => RefreshAfterDataEditorChange("TSA / 属性已更新。");
        _paletteEditor.BeforeEdit += (_, _) => { if (!_refreshing) PushUndo(); };
        _paletteEditor.DataChanged += (_, _) => RefreshAfterDataEditorChange("调色板已更新。");
        _flagTsaEditor.BeforeEdit += (_, _) => { if (!_refreshing) PushUndo(); };
        _flagTsaEditor.DataChanged += (_, _) => RefreshAfterDataEditorChange("Flag / Fort TSA 已更新。");
        _gameSettings.BeforeEdit += (_, _) => { if (!_refreshing) PushUndo(); };
        _gameSettings.DataChanged += (_, _) =>
        {
            RefreshAfterDataEditorChange("游戏设置已更新。");
            _gameSettings.RefreshValues();
            RefreshExOptions();
        };
    }

    private void RefreshAfterDataEditorChange(string message)
    {
        if (_rom is null || _renderer is null) return;
        MarkDirty();
        _renderer.InvalidateCache();
        BuildTerrainButtons();
        _stageCanvas.Invalidate();
        _gameSettings.RefreshValues();
        _infoBox.Text = _rom.Describe();
        SetStatus(message, false);
    }

    private void ClearTerrainButtonImages()
    {
        _terrainPanel.SuspendLayout();
        try
        {
            for (var i = _terrainPanel.Controls.Count - 1; i >= 0; i--)
            {
                var control = _terrainPanel.Controls[i];
                if (control is Button button)
                {
                    var image = button.Image; button.Image = null; image?.Dispose();
                }
                _terrainPanel.Controls.RemoveAt(i); control.Dispose();
            }
        }
        finally { _terrainPanel.ResumeLayout(); }
    }

    private void DetachCurrentRomUi()
    {
        _stageCanvas.Renderer = null; _stageCanvas.Rom = null; _stageCanvas.Invalidate();
        ClearTerrainButtonImages();
        _enemyGrid.Rows.Clear(); _tsaEditor.Bind(null, null); _paletteEditor.Bind(null, null); _flagTsaEditor.Bind(null, null); _gameSettings.Bind(null, null, null); _infoBox.Clear();
        _renderer?.Dispose(); _renderer = null; _rom = null;
    }

    private void OpenRom()
    {
        if (!PromptSaveIfDirty()) return;
        using var dlg = new OpenFileDialog { Filter = "NES ROM (*.nes)|*.nes|所有文件 (*.*)|*.*", Title = "打开 Battle City / Battle City Ex ROM" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var newRom = new BattleCityRom(File.ReadAllBytes(dlg.FileName), _cfg);
            var newRenderer = new NesRenderer(newRom);
            DetachCurrentRomUi();
            _rom = newRom;
            _renderer = newRenderer;
            _filePath = dlg.FileName;
            _savedBytes = _rom.GetBytesCopy();
            _dirty = false;
            _undo.Clear();
            _redo.Clear();
            UpdateHistoryMenus();
            if (!_rom.SelectableTerrainIds.Contains(_selectedTerrain)) _selectedTerrain = 0x0D;
            PopulateStages();
            EnableEditing(true);
            RefreshAll();
            var openStatus = _rom.Kind switch
            {
                BattleCityRomKind.Original16K => "已打开 Battle City 原版 ROM：Stage 1~35 + Demo，TSA 表 00~0F（编辑器屏蔽 0E/0F）。",
                BattleCityRomKind.Ex16K => _rom.HasExV2Config
                    ? $"已打开 16KB BCEX v2；FeatureFlags=${_rom.FeatureFlags:X2}。"
                    : "已打开 16KB Legacy Ex；放置 10~17 时会自动扩容为 32KB。",
                BattleCityRomKind.Ex32KOverlay => _rom.HasExV2Config
                    ? $"已打开 32KB BCEX v2 Overlay；FeatureFlags=${_rom.FeatureFlags:X2}。"
                    : "已打开 32KB Quarrel Ex Overlay ROM。",
                _ => $"已打开 BCEX v2 32KB / 70独立地图；Terrain={_rom.TerrainCount}，FeatureFlags=${_rom.FeatureFlags:X2}。"
            };
            SetStatus(openStatus, false);
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "无法打开 ROM", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportSharedConfig()
    {
        if (_rom is null)
        {
            MessageBox.Show(this, "请先打开 ROM。", "导出配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter = "QuarrelExConfig v3 (*.qexcfg.json)|*.qexcfg.json",
            DefaultExt = "qexcfg.json",
            AddExtension = true,
            FileName = string.IsNullOrWhiteSpace(_filePath)
                ? "BattleCity_QuarrelEx_v3.qexcfg.json"
                : Path.GetFileNameWithoutExtension(_filePath) + ".qexcfg.json"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var cfg = _rom.ExportSharedConfig();
            var validation = _rom.ValidateSharedConfig(cfg);
            if (!validation.IsValid)
                throw new InvalidDataException(validation.FormatErrors());

            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dlg.FileName, json);

            SetStatus("QuarrelExConfig v3 已导出。", false);
            MessageBox.Show(
                this,
                $"配置导出成功。{Environment.NewLine}{dlg.FileName}{Environment.NewLine}{Environment.NewLine}该 v3 文件可在 Web / Desktop 间通用。",
                "配置导出成功",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "导出配置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportSharedConfig()
    {
        if (_rom is null)
        {
            MessageBox.Show(this, "请先打开目标 ROM。", "导入配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new OpenFileDialog
        {
            Filter = "QuarrelExConfig v3 (*.qexcfg.json)|*.qexcfg.json",
            Title = "导入 QuarrelExConfig v3"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var jsonText = File.ReadAllText(dlg.FileName);
            using (var doc = JsonDocument.Parse(jsonText))
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("配置文件根节点必须是 JSON 对象。");
                if (!root.TryGetProperty("Schema", out var schemaNode) || schemaNode.ValueKind != JsonValueKind.String || schemaNode.GetString() != "QuarrelExConfig")
                    throw new InvalidDataException("配置文件缺少有效的 Schema=QuarrelExConfig。");
                if (!root.TryGetProperty("Version", out var versionNode) || versionNode.ValueKind != JsonValueKind.Number || !versionNode.TryGetInt32(out var version) || version != 3)
                    throw new InvalidDataException("正式版只接受明确标记为 Version=3 的 QuarrelExConfig v3 文件。");
            }

            var cfg = JsonSerializer.Deserialize<QuarrelExSharedConfig>(
                          jsonText,
                          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                      ?? throw new InvalidDataException("配置文件为空或 JSON 结构错误。");

            // Transactional import: validate and apply to a temporary ROM first.
            // The active ROM is not touched at all when a hard validation error occurs.
            var staging = new BattleCityRom(_rom.GetBytesCopy(), _cfg);
            var validation = staging.ValidateSharedConfig(cfg);
            if (!validation.IsValid)
                throw new InvalidDataException(validation.FormatErrors());

            var notes = staging.ApplySharedConfig(cfg);

            PushUndo();
            _rom.RestoreBytes(staging.GetBytesCopy());
            _renderer?.InvalidateCache();
            MarkDirty();
            RefreshAll();

            var warningText = notes.Count > 0
                ? Environment.NewLine + Environment.NewLine + "兼容性提示：" + Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", notes)
                : string.Empty;

            MessageBox.Show(
                this,
                "QuarrelExConfig v3 已检查并导入成功。" + warningText,
                "配置导入成功",
                MessageBoxButtons.OK,
                notes.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            SetStatus("QuarrelExConfig v3 已检查并导入。", false);
        }
        catch (JsonException ex)
        {
            MessageBox.Show(this, "JSON 格式错误：" + ex.Message, "导入配置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "导入配置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool SaveRom(bool forceSaveAs)
    {
        if (_rom is null) return false;

        var path = _filePath;
        if (forceSaveAs || string.IsNullOrWhiteSpace(path))
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "NES ROM (*.nes)|*.nes",
                DefaultExt = "nes",
                AddExtension = true,
                FileName = string.IsNullOrWhiteSpace(path) ? "Battle_City.nes" : Path.GetFileName(path),
                InitialDirectory = string.IsNullOrWhiteSpace(path) ? null : Path.GetDirectoryName(path)
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return false;
            path = dlg.FileName;
        }

        try
        {
            _rom.Save(path!);
            _filePath = path;
            _savedBytes = _rom.GetBytesCopy();
            _dirty = false;
            UpdateTitle();
            SetStatus(forceSaveAs ? "ROM 已另存为。" : "ROM 已保存。", false);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void PopulateStages()
    {
        _stageCombo.Items.Clear();
        if (_rom is null) return;
        for (var i = 1; i <= _rom.MaxEditableStage; i++)
        {
            _stageCombo.Items.Add(_rom.IsDemoStage(i) ? "Demo" : $"Stage {i}");
        }
        if (_stageCombo.Items.Count > 0) _stageCombo.SelectedIndex = 0;
    }

    private int CurrentStage => _stageCombo.SelectedIndex >= 0 ? _stageCombo.SelectedIndex + 1 : 1;

    private void RefreshAll()
    {
        if (_rom is null || _renderer is null) return;
        _refreshing = true;
        try
        {
            _stageCanvas.Rom = _rom;
            _stageCanvas.Renderer = _renderer;
            _stageCanvas.Stage = CurrentStage;
            _stageCanvas.Invalidate();
            BuildTerrainButtons();
            RefreshEnemyGrid();
            _tsaEditor.Bind(_rom, _renderer);
            _paletteEditor.Bind(_rom, _renderer);
            _flagTsaEditor.Bind(_rom, _renderer);
            _gameSettings.Bind(_rom, _renderer, () => CurrentStage);
            RefreshExOptions();
            _infoBox.Text = _rom.Describe();
            RefreshStageNote();
            _convertMenu.Enabled = _rom.CanConvertToOverlay;
        }
        finally { _refreshing = false; }
    }

    private void RefreshStageView()
    {
        if (_rom is null) return;
        _stageCanvas.Stage = CurrentStage;
        _stageCanvas.Invalidate();
        RefreshEnemyGrid();
        _gameSettings.RefreshValues();
        RefreshStageNote();
    }

    private void RefreshStageNote()
    {
        if (_rom is null) return;
        if (_rom.IsDemoStage(CurrentStage))
            _mapNote.Text = "原版 Demo 地图；敌人 Type/Count 与 Stage 35 共用。";
        else if (_rom.IsOriginal)
            _mapNote.Text = "Battle City 原版模式：Stage 1~35 使用独立地图。";
        else if (_rom.HasIndependentMaps)
            _mapNote.Text = $"BCEX v2 32KB：Stage 1~70 地图全部独立，地形 $00~${_rom.TerrainCount - 1:X2} 可直接保存。";
        else if (CurrentStage > 35)
            _mapNote.Text = $"Stage {CurrentStage} 的地图与 Stage {CurrentStage - 35} 共用；敌人 Type/Count 独立。";
        else
            _mapNote.Text = "Stage 1~35 使用独立地图。";
    }

    private void BuildTerrainButtons()
    {
        if (_renderer is null || _rom is null) return;
        _terrainPanel.SuspendLayout();
        try
        {
            ClearTerrainButtonImages();
            foreach (var id in _rom.SelectableTerrainIds)
            {
                var dpi = Math.Max(96, _terrainPanel.DeviceDpi);
                var previewScale = 2;
                var button = new Button
                {
                    Width = ScaleForDpiCompact(92, dpi),
                    Height = ScaleForDpiCompact(40, dpi),
                    Margin = new Padding(ScaleForDpiCompact(2, dpi)),
                    Padding = new Padding(ScaleForDpiCompact(3, dpi), 0, ScaleForDpiCompact(3, dpi), 0),
                    Text = $"{id:X2}  {GetTerrainName(id)}",
                    TextAlign = ContentAlignment.MiddleLeft,
                    ImageAlign = ContentAlignment.MiddleLeft,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    Image = new Bitmap(_renderer.GetBlockBitmap(id, previewScale)),
                    Tag = id,
                    FlatStyle = FlatStyle.Flat,
                    AutoEllipsis = true,
                    UseVisualStyleBackColor = false,
                    BackColor = id == _selectedTerrain ? Color.LightSkyBlue : SystemColors.Control
                };
                button.Click += (_, _) =>
                {
                    _selectedTerrain = (int)button.Tag;
                    BuildTerrainButtons();
                };
                _terrainPanel.Controls.Add(button);
            }
        }
        finally { _terrainPanel.ResumeLayout(); }
    }

    private static string GetTerrainName(int id)
    {
        if (TerrainNames.TryGetValue(id, out var name)) return name;
        if (id is >= 0x20 and <= 0x3F) return $"自定义 {id:X2}";
        return "地形";
    }

    private static int ScaleForDpiCompact(int logicalPixels, int dpi)
    {
        var scale = Math.Clamp(dpi / 96.0, 1.0, 1.25);
        return Math.Max(1, (int)Math.Round(logicalPixels * scale));
    }

    private void RefreshEnemyGrid()
    {
        if (_rom is null) return;
        _refreshing = true;
        try
        {
            var (types, counts) = _rom.GetEnemyData(CurrentStage);
            _enemyGrid.Rows.Clear();
            var sum = 0;
            for (var i = 0; i < 4; i++)
            {
                _enemyGrid.Rows.Add(i + 1, types[i].ToString("X2"), counts[i].ToString(CultureInfo.InvariantCulture));
                sum += counts[i];
            }
            if (_rom.SupportsCustomEnemyTotal)
            {
                var good = sum is >= 1 and <= 255;
                _enemySum.Text = $"总敌人数：{sum} / 1~255{(good ? "（有效）" : "（超出范围）")}\r\n四个 Count 的合计就是本关实际敌人总数；单个槽允许 0~255。";
                _enemySum.ForeColor = good ? Color.DarkGreen : Color.DarkRed;
            }
            else
            {
                _enemySum.Text = sum == 20
                    ? "数量合计：20（原版有效）\r\n当前 ROM 没有自定义总敌人数运行支持。"
                    : $"数量合计：{sum}（建议保持20）\r\n当前 ROM 的运行程序仍按20辆规格设计。";
                _enemySum.ForeColor = sum == 20 ? Color.DarkGreen : Color.DarkOrange;
            }
        }
        finally { _refreshing = false; }
    }

    private void EnemyGrid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (_rom is null || _refreshing || e.RowIndex < 0) return;
        try
        {
            if (e.ColumnIndex == 1)
            {
                var text = Convert.ToString(_enemyGrid.Rows[e.RowIndex].Cells[1].Value)?.Trim() ?? "00";
                if (!byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)) value = 0;
                _rom.SetEnemyType(CurrentStage, e.RowIndex, value);
            }
            else if (e.ColumnIndex == 2)
            {
                var text = Convert.ToString(_enemyGrid.Rows[e.RowIndex].Cells[2].Value)?.Trim() ?? "0";
                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) number = 0;
                number = Math.Clamp(number, 0, 255);
                var (_, currentCounts) = _rom.GetEnemyData(CurrentStage);
                var proposed = currentCounts.ToArray();
                proposed[e.RowIndex] = (byte)number;
                _rom.ValidateEnemyTotal(proposed);
                _rom.SetEnemyCount(CurrentStage, e.RowIndex, (byte)number);
            }
            MarkDirty();
            RefreshEnemyGrid();
        }
        catch (Exception ex) { SetStatus(ex.Message, true); }
    }

    private void StageCanvas_CellPaintRequested(object? sender, CellPaintEventArgs e)
    {
        if (_rom is null || _renderer is null) return;
        try
        {
            if (e.NewStroke) PushUndo();
            var converted = _rom.SetCell(CurrentStage, e.Row, e.Column, _selectedTerrain);
            if (converted)
            {
                _renderer.InvalidateCache();
                _infoBox.Text = _rom.Describe();
                _convertMenu.Enabled = false;
                SetStatus("已自动转换为 32KB Ex Overlay；请另存 ROM。", false);
            }
            MarkDirty();
            var cellSize = _stageCanvas.CellSize;
            _stageCanvas.Invalidate(new Rectangle(e.Column * cellSize, e.Row * cellSize, cellSize + 1, cellSize + 1));
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "写入地图失败", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void StageCanvas_CellPickRequested(object? sender, CellPickEventArgs e)
    {
        if (_rom is null) return;
        var picked = _rom.GetCell(CurrentStage, e.Row, e.Column);
        if (!_rom.SelectableTerrainIds.Contains(picked))
        {
            SetStatus($"地形 ${picked:X2} 为内部保留值，未吸取。", true);
            return;
        }
        _selectedTerrain = picked;
        BuildTerrainButtons();
        SetStatus($"已吸取地形 {_selectedTerrain:X2}。", false);
    }

    private void ConvertToExpanded()
    {
        if (_rom is null || _renderer is null || !_rom.CanConvertToOverlay) return;
        try
        {
            PushUndo();
            if (_rom.EnsureExpandedForExtendedTerrain())
            {
                _renderer.InvalidateCache();
                MarkDirty();
                RefreshAll();
                SetStatus("已转换为 32KB Quarrel Ex Overlay 格式；请另存 ROM。", false);
            }
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "转换失败", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static void TrimHistory(Stack<byte[]> stack, int max = 50)
    {
        if (stack.Count <= max) return;
        var keep = stack.Take(max).Reverse().ToArray();
        stack.Clear();
        foreach (var item in keep) stack.Push(item);
    }

    private void UpdateHistoryMenus()
    {
        var canUndo = _rom is not null && _undo.Count > 0;
        var canRedo = _rom is not null && _redo.Count > 0;
        _undoMenu.Enabled = canUndo;
        _redoMenu.Enabled = canRedo;
        _undoToolButton.Enabled = canUndo;
        _redoToolButton.Enabled = canRedo;
    }

    private void PushUndo()
    {
        if (_rom is null) return;
        _undo.Push(_rom.GetBytesCopy());
        TrimHistory(_undo);
        _redo.Clear();
        UpdateHistoryMenus();
    }

    private void Undo()
    {
        if (_rom is null || _renderer is null || _undo.Count == 0) return;

        _redo.Push(_rom.GetBytesCopy());
        TrimHistory(_redo);
        _rom.RestoreBytes(_undo.Pop());
        _renderer.InvalidateCache();
        UpdateHistoryMenus();
        UpdateDirtyFromSavedState();
        RefreshAll();
        SetStatus("已撤销。", false);
    }

    private void Redo()
    {
        if (_rom is null || _renderer is null || _redo.Count == 0) return;

        _undo.Push(_rom.GetBytesCopy());
        TrimHistory(_undo);
        _rom.RestoreBytes(_redo.Pop());
        _renderer.InvalidateCache();
        UpdateHistoryMenus();
        UpdateDirtyFromSavedState();
        RefreshAll();
        SetStatus("已重做。", false);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.Shift | Keys.Z))
        {
            Redo();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void EnableEditing(bool enabled)
    {
        _stageCombo.Enabled = enabled;
        _saveMenu.Enabled = enabled;
        _saveAsMenu.Enabled = enabled;
        _saveToolButton.Enabled = enabled;
        _convertMenu.Enabled = enabled && _rom is { CanConvertToOverlay: true };
        UpdateHistoryMenus();
    }

    private void MarkDirty()
    {
        _dirty = true;
        UpdateTitle();
    }

    private void UpdateDirtyFromSavedState()
    {
        if (_rom is null || _savedBytes is null)
            _dirty = _rom is not null;
        else
            _dirty = !_rom.GetBytesCopy().SequenceEqual(_savedBytes);
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        var name = string.IsNullOrWhiteSpace(_filePath) ? "未打开" : Path.GetFileName(_filePath);
        Text = $"Quarrel Ex - {name}{(_dirty ? " *" : string.Empty)}";
    }

    private void SetStatus(string text, bool error)
    {
        _status.Text = text;
        _status.ForeColor = error ? Color.DarkRed : SystemColors.ControlText;
    }

    private bool PromptSaveIfDirty()
    {
        if (!_dirty || _rom is null) return true;
        var result = MessageBox.Show(this, "当前 ROM 有未保存修改，是否先保存？", "Quarrel Ex", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (result == DialogResult.Cancel) return false;
        if (result == DialogResult.Yes) return SaveRom(false);
        return result == DialogResult.No;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!PromptSaveIfDirty()) e.Cancel = true;
        if (!e.Cancel)
        {
            DisposeToolWindows();
            DetachCurrentRomUi();
        }
    }
}
