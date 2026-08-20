using QuarrelEx.Core;
using QuarrelEx.Rendering;

namespace QuarrelEx.Controls;

/// <summary>
/// Native Battle City Title / Game Over screen-source editor.
/// Ordinary strings are edited as 8x8 CHR tiles. BATTLE/CITY/GAME/OVER use
/// the original game's 32x32 magnified-glyph routine and are therefore edited
/// as whole source glyph slots rather than fake independent 8x8 sub-cells.
/// </summary>
public sealed class ScreenEditorControl : UserControl
{
    private const int PreviewScale = 2;
    private readonly ComboBox _screenKind = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
    private readonly ComboBox _previewAttr = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
    private readonly Label _selection = new() { AutoSize = true, ForeColor = Color.DimGray, Padding = new Padding(6, 2, 6, 2) };
    private readonly Label _note = new()
    {
        AutoSize = true,
        MaximumSize = new Size(760, 0),
        ForeColor = Color.DimGray,
        Padding = new Padding(8, 4, 8, 8),
        Text = "1 格 = 1 个 NES 8×8 背景 CHR Tile。BATTLE / CITY / GAME / OVER 原版使用 32×32 放大字形绘制程序，因此每个蓝框代表一个完整的源字形槽；点击后选择一个 CHR Tile。这里只修改原 ROM 已有画面字符串，不改变字符串坐标、长度或 $FF 终止符。Attr 仅用于预览颜色，不写入 ROM。"
    };
    private readonly Panel _viewport = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = SystemColors.ControlDark, Padding = new Padding(6) };
    private readonly ScreenCanvas _canvas = new() { Size = new Size(256 * PreviewScale, 240 * PreviewScale), BackColor = Color.Black };

    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private ScreenElementDefinition? _selectedElement;
    private int _selectedIndex = -1;

    public event EventHandler? BeforeEdit;
    public event EventHandler? DataChanged;

    public ScreenEditorControl()
    {
        Dock = DockStyle.Fill;
        AutoScaleMode = AutoScaleMode.Dpi;

        _screenKind.Items.AddRange(["Title Screen / 标题画面", "Game Over"]);
        _screenKind.SelectedIndex = 0;
        _screenKind.SelectedIndexChanged += (_, _) =>
        {
            _selectedElement = null;
            _selectedIndex = -1;
            RefreshView();
        };
        _previewAttr.Items.AddRange(["Attr 0", "Attr 1", "Attr 2", "Attr 3"]);
        _previewAttr.SelectedIndex = 0;
        _previewAttr.SelectedIndexChanged += (_, _) => _canvas.Invalidate();

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6), WrapContents = true };
        top.Controls.Add(new Label { Text = "画面:", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
        top.Controls.Add(_screenKind);
        top.Controls.Add(new Label { Text = "预览:", AutoSize = true, Padding = new Padding(12, 5, 0, 0) });
        top.Controls.Add(_previewAttr);
        top.Controls.Add(_selection);

        _viewport.Controls.Add(_canvas);
        _canvas.Location = new Point(_viewport.Padding.Left, _viewport.Padding.Top);
        _canvas.Paint += Canvas_Paint;
        _canvas.MouseClick += Canvas_MouseClick;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(top, 0, 0);
        root.Controls.Add(_viewport, 0, 1);
        root.Controls.Add(_note, 0, 2);
        Controls.Add(root);
    }

    private ScreenKind CurrentKind => _screenKind.SelectedIndex == 1 ? ScreenKind.GameOver : ScreenKind.Title;
    private byte PreviewAttr => (byte)Math.Clamp(_previewAttr.SelectedIndex, 0, 3);

    public void Bind(BattleCityRom? rom, NesRenderer? renderer)
    {
        _rom = rom;
        _renderer = renderer;
        _selectedElement = null;
        _selectedIndex = -1;
        RefreshView();
    }

    public void RefreshView()
    {
        if (_rom is null)
        {
            _selection.Text = "未打开 ROM";
        }
        else if (_selectedElement is null || _selectedIndex < 0)
        {
            _selection.Text = "点击蓝框选择图块";
        }
        else
        {
            var tile = _rom.GetScreenElementTile(_selectedElement, _selectedIndex);
            _selection.Text = $"{_selectedElement.DisplayName} [{_selectedIndex + 1}/{_selectedElement.Length}] = ${tile:X2}";
        }
        _canvas.Invalidate();
    }

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.Clear(Color.Black);
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        if (_rom is null || _renderer is null) return;

        // Subtle 8x8 NES tile grid.
        using (var grid = new Pen(Color.FromArgb(36, 255, 255, 255)))
        {
            for (var x = 0; x <= 256; x += 8) e.Graphics.DrawLine(grid, x * PreviewScale, 0, x * PreviewScale, 240 * PreviewScale);
            for (var y = 0; y <= 240; y += 8) e.Graphics.DrawLine(grid, 0, y * PreviewScale, 256 * PreviewScale, y * PreviewScale);
        }

        foreach (var def in _rom.GetScreenElements(CurrentKind))
        {
            for (var i = 0; i < def.Length; i++)
            {
                var tile = _rom.GetScreenElementTile(def, i);
                Rectangle rect;
                if (def.Kind == ScreenElementKind.TileString)
                {
                    rect = new Rectangle((def.X + i) * 8 * PreviewScale, def.Y * 8 * PreviewScale, 8 * PreviewScale, 8 * PreviewScale);
                    var bmp = _renderer.GetChrTileBitmap(tile, def.Palette, PreviewAttr, PreviewScale);
                    e.Graphics.DrawImageUnscaled(bmp, rect.Location);
                }
                else
                {
                    rect = new Rectangle((def.X + i * 32) * PreviewScale, def.Y * PreviewScale, 32 * PreviewScale, 32 * PreviewScale);
                    var bmp = _renderer.GetChrTileBitmap(tile, def.Palette, PreviewAttr, 4 * PreviewScale);
                    e.Graphics.DrawImageUnscaled(bmp, rect.Location);
                }

                var selected = ReferenceEquals(_selectedElement, def) && _selectedIndex == i;
                using var pen = new Pen(selected ? Color.Yellow : (def.Kind == ScreenElementKind.LargeGlyphString ? Color.DeepSkyBlue : Color.SkyBlue), selected ? 2 : 1);
                e.Graphics.DrawRectangle(pen, rect.X, rect.Y, Math.Max(1, rect.Width - 1), Math.Max(1, rect.Height - 1));
            }
        }
    }

    private void Canvas_MouseClick(object? sender, MouseEventArgs e)
    {
        if (_rom is null || _renderer is null || e.Button != MouseButtons.Left) return;
        var px = e.X / PreviewScale;
        var py = e.Y / PreviewScale;

        // Test magnified glyphs first because their boxes are larger.
        var hit = HitTest(px, py, ScreenElementKind.LargeGlyphString) ?? HitTest(px, py, ScreenElementKind.TileString);
        if (hit is null)
        {
            _selectedElement = null;
            _selectedIndex = -1;
            RefreshView();
            return;
        }

        var (def, index) = hit.Value;
        _selectedElement = def;
        _selectedIndex = index;
        RefreshView();

        var current = _rom.GetScreenElementTile(def, index);
        using var picker = new ChrTilePickerForm(_renderer, PreviewAttr, current, def.DisplayName, def.Palette, allowFF: false);
        if (picker.ShowDialog(FindForm()) != DialogResult.OK || picker.SelectedTile == current) return;

        BeforeEdit?.Invoke(this, EventArgs.Empty);
        _rom.SetScreenElementTile(def, index, picker.SelectedTile);
        _renderer.InvalidateCache();
        DataChanged?.Invoke(this, EventArgs.Empty);
        RefreshView();
    }

    private (ScreenElementDefinition Def, int Index)? HitTest(int px, int py, ScreenElementKind kind)
    {
        if (_rom is null) return null;
        foreach (var def in _rom.GetScreenElements(CurrentKind).Where(x => x.Kind == kind))
        {
            for (var i = 0; i < def.Length; i++)
            {
                var rect = kind == ScreenElementKind.TileString
                    ? new Rectangle((def.X + i) * 8, def.Y * 8, 8, 8)
                    : new Rectangle(def.X + i * 32, def.Y, 32, 32);
                if (rect.Contains(px, py)) return (def, i);
            }
        }
        return null;
    }

    private sealed class ScreenCanvas : Control
    {
        public ScreenCanvas()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}
