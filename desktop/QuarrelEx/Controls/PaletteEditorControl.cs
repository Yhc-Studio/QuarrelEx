using QuarrelEx.Core;
using QuarrelEx.Rendering;
using QuarrelEx.Localization;

namespace QuarrelEx.Controls;

public sealed class PaletteEditorControl : UserControl
{
    private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 205 };
    private readonly TableLayoutPanel _grid = new() { ColumnCount = 4, RowCount = 4, AutoSize = false, Dock = DockStyle.None, Anchor = AnchorStyles.Top | AnchorStyles.Left, Padding = new Padding(4), Margin = Padding.Empty };
    private readonly Panel _gridHost = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(6) };
    private readonly Label _hint = new() { AutoSize = true, ForeColor = Color.DimGray, Padding = new Padding(6) };
    private readonly Label _displayPaletteLabel = new() { AutoSize = true, Padding = new Padding(0, 5, 0, 0) };
    private readonly Label _displayPaletteStatus = new() { AutoSize = true, ForeColor = Color.DimGray, Padding = new Padding(0, 5, 4, 0) };
    private readonly Button _loadDisplayPalette = new() { AutoSize = true };
    private readonly Button _resetDisplayPalette = new() { AutoSize = true };
    private readonly List<Button> _buttons = new();
    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private bool _refreshing;

    public event EventHandler? BeforeEdit;
    public event EventHandler? DataChanged;
    public event EventHandler? DisplayPaletteChanged;

    private static readonly (PaletteKind Kind, string Name)[] Sets =
    [
        (PaletteKind.Level, "关卡背景 / Level Palette"),
        (PaletteKind.Sprite, "精灵 / Sprite Palette"),
        (PaletteKind.Frame1, "Frame 1"),
        (PaletteKind.Frame2, "Frame 2"),
        (PaletteKind.Title, "标题画面 / Title"),
        (PaletteKind.LevelSelect, "选关 / Level Select"),
        (PaletteKind.Misc1, "Misc 1"),
        (PaletteKind.Misc2, "Misc 2")
    ];

    public PaletteEditorControl()
    {
        Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6), WrapContents = true };
        top.Controls.Add(new Label { Text = "调色板组:", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
        foreach (var s in Sets) _kind.Items.Add(s.Name);
        _kind.SelectedIndex = 0;
        _kind.SelectedIndexChanged += (_, _) => Rebuild();
        top.Controls.Add(_kind);

        var displayBar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6, 0, 6, 2), WrapContents = true };
        _loadDisplayPalette.Click += (_, _) => LoadDisplayPaletteFile();
        _resetDisplayPalette.Click += (_, _) => ResetDisplayPalette();
        displayBar.Controls.Add(_displayPaletteLabel);
        displayBar.Controls.Add(_displayPaletteStatus);
        displayBar.Controls.Add(_loadDisplayPalette);
        displayBar.Controls.Add(_resetDisplayPalette);

        _hint.Text = "每组 16 个 NES 色号（4×4）。点击色块从 64 色 NES Palette 中选择；色块已采用紧凑尺寸。";
        _gridHost.Controls.Add(_grid);
        root.Controls.Add(top, 0, 0);
        root.Controls.Add(displayBar, 0, 1);
        root.Controls.Add(_hint, 0, 2);
        root.Controls.Add(_gridHost, 0, 3);
        Controls.Add(root);
        RefreshDisplayPaletteUi();
    }

    public void Bind(BattleCityRom? rom, NesRenderer? renderer)
    {
        _rom = rom;
        _renderer = renderer;
        Rebuild();
    }

    private PaletteKind CurrentKind => Sets[Math.Clamp(_kind.SelectedIndex, 0, Sets.Length - 1)].Kind;

    public void RefreshDisplayPaletteUi()
    {
        _displayPaletteLabel.Text = I18n.T("palette.display.label.desktop");
        _loadDisplayPalette.Text = I18n.T("palette.display.load");
        _resetDisplayPalette.Text = I18n.T("palette.display.reset");
        _resetDisplayPalette.Enabled = NesDisplayPalette.IsCustom;
        _displayPaletteStatus.Text = NesDisplayPalette.IsCustom
            ? I18n.T("palette.display.custom", NesDisplayPalette.SourceName)
            : I18n.T("palette.display.default");
    }

    private void LoadDisplayPaletteFile()
    {
        using var dlg = new OpenFileDialog
        {
            Title = I18n.T("palette.display.load_title"),
            Filter = I18n.T("dialog.filter.palette"),
            CheckFileExists = true,
            Multiselect = false
        };
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

        try
        {
            var length = new FileInfo(dlg.FileName).Length;
            if (length != NesDisplayPalette.RgbByteLength)
            {
                MessageBox.Show(FindForm(), I18n.T("palette.display.invalid_size", length), I18n.T("palette.display.load_failed"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, I18n.T("palette.display.load_failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!NesDisplayPalette.TryLoadFile(dlg.FileName, out var error))
        {
            MessageBox.Show(FindForm(), error, I18n.T("palette.display.load_failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        RefreshDisplayPaletteUi();
        DisplayPaletteChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ResetDisplayPalette()
    {
        if (!NesDisplayPalette.IsCustom) return;
        NesDisplayPalette.ResetToDefault();
        RefreshDisplayPaletteUi();
        DisplayPaletteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Rebuild()
    {
        RefreshDisplayPaletteUi();
        _refreshing = true;
        try
        {
            _grid.SuspendLayout();
            _grid.Controls.Clear();
            _grid.ColumnStyles.Clear();
            _grid.RowStyles.Clear();
            _buttons.Clear();
            if (_rom is null)
            {
                _grid.MinimumSize = Size.Empty;
                _grid.MaximumSize = Size.Empty;
                _grid.Size = Size.Empty;
                return;
            }

            var dpi = Math.Max(96, DeviceDpi);
            var cellW = UiScale(68, dpi);
            var cellH = UiScale(30, dpi);
            for (var i = 0; i < 4; i++)
            {
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, cellW));
                _grid.RowStyles.Add(new RowStyle(SizeType.Absolute, cellH));
            }
            var fixedGridSize = new Size(cellW * 4 + _grid.Padding.Horizontal, cellH * 4 + _grid.Padding.Vertical);
            _grid.MinimumSize = fixedGridSize;
            _grid.MaximumSize = fixedGridSize;
            _grid.Size = fixedGridSize;
            _grid.Location = new Point(_gridHost.Padding.Left, _gridHost.Padding.Top);

            for (var i = 0; i < 16; i++)
            {
                var index = i;
                var value = _rom.GetPaletteByte(CurrentKind, i);
                var color = NesRenderer.GetNesColor(value);
                var b = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(UiScale(2, dpi)),
                    Padding = Padding.Empty,
                    BackColor = color,
                    ForeColor = color.GetBrightness() < 0.45 ? Color.White : Color.Black,
                    Text = $"{i:X1}  ${value:X2}",
                    Font = new Font("Consolas", 8.5f, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat
                };
                b.Click += (_, _) =>
                {
                    if (_rom is null) return;
                    using var picker = new PaletteColorPickerForm(_rom.GetPaletteByte(CurrentKind, index));
                    if (picker.ShowDialog(FindForm()) != DialogResult.OK) return;
                    BeforeEdit?.Invoke(this, EventArgs.Empty);
                    _rom.SetPaletteByte(CurrentKind, index, picker.SelectedColor);
                    _renderer?.InvalidateCache();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                    Rebuild();
                };
                _buttons.Add(b);
                _grid.Controls.Add(b, i % 4, i / 4);
            }
        }
        finally
        {
            _grid.ResumeLayout(true);
            _refreshing = false;
        }
    }

    private static int UiScale(int logicalPixels, int dpi)
    {
        var scale = Math.Clamp(dpi / 96.0, 1.0, 1.25);
        return Math.Max(1, (int)Math.Round(logicalPixels * scale));
    }
}
