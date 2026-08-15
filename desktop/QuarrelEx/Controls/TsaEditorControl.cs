using QuarrelEx.Core;
using QuarrelEx.Rendering;

namespace QuarrelEx.Controls;

public sealed class TsaEditorControl : UserControl
{
    // v0.9.2 uses a plain scrolling viewport and an explicitly-sized content
    // surface. This is more predictable than FlowLayoutPanel's TopDown layout
    // on 720p/768p and high-DPI desktops: both scrollbars remain available and
    // wide TSA rows are never squeezed to a 1-pixel column.
    private readonly Panel _viewport = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        BackColor = SystemColors.Control,
        Padding = Padding.Empty
    };

    private readonly Panel _content = new()
    {
        Location = Point.Empty,
        BackColor = SystemColors.Control,
        Margin = Padding.Empty,
        Padding = Padding.Empty
    };

    private readonly List<Image> _ownedImages = new();
    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private bool _refreshing;

    public event EventHandler? BeforeEdit;
    public event EventHandler? DataChanged;

    public TsaEditorControl()
    {
        Dock = DockStyle.Fill;
        MinimumSize = new Size(0, 120);
        _viewport.Controls.Add(_content);
        Controls.Add(_viewport);
        _viewport.Resize += (_, _) => EnsureScrollExtent();
    }

    public void Bind(BattleCityRom? rom, NesRenderer? renderer)
    {
        _rom = rom;
        _renderer = renderer;
        Rebuild();
    }

    public void Rebuild()
    {
        _refreshing = true;
        _viewport.SuspendLayout();
        _content.SuspendLayout();
        try
        {
            ClearRows();
            if (_rom is null || _renderer is null) return;

            var dpi = Math.Max(96, DeviceDpi);
            var rowWidth = UiScale(566, dpi);
            var rowHeight = UiScale(64, dpi);
            var gap = UiScale(4, dpi);
            var x = gap;
            var y = gap;

            // $0E/$0F are deliberately hidden from TSA editing.
            foreach (var id in _rom.SelectableTerrainIds)
            {
                var row = BuildRow(id, rowWidth, rowHeight, dpi);
                row.Location = new Point(x, y);
                _content.Controls.Add(row);
                y += rowHeight + gap;
            }

            _content.Size = new Size(rowWidth + gap * 2, Math.Max(1, y));
            EnsureScrollExtent();
        }
        finally
        {
            _content.ResumeLayout(true);
            _viewport.ResumeLayout(true);
            _refreshing = false;
        }
    }

    private Control BuildRow(int id, int rowWidth, int rowHeight, int dpi)
    {
        var attr = (byte)(_rom!.GetTerrainAttribute(id) & 3);
        var tiles = _rom.GetTerrainTiles(id);

        var row = new TableLayoutPanel
        {
            AutoSize = false,
            Width = rowWidth,
            Height = rowHeight,
            MinimumSize = new Size(rowWidth, rowHeight),
            MaximumSize = new Size(rowWidth, rowHeight),
            ColumnCount = 7,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = new Padding(UiScale(3, dpi)),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            BackColor = SystemColors.Window
        };

        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UiScale(38, dpi))); // ID
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UiScale(48, dpi))); // preview
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UiScale(50, dpi))); // Attr
        for (var i = 0; i < 4; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UiScale(101, dpi))); // TL/TR/BL/BR

        row.Controls.Add(new Label
        {
            Text = $"{id:X2}",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 9f, FontStyle.Bold),
            Margin = Padding.Empty
        }, 0, 0);

        // Keep pixel-art previews compact. The surrounding controls are DPI-aware;
        // scaling the bitmap again made the old TSA page huge on 125/150% displays.
        var previewImage = new Bitmap(_renderer!.GetBlockBitmap(id, 2));
        _ownedImages.Add(previewImage);
        row.Controls.Add(new PictureBox
        {
            Image = previewImage,
            SizeMode = PictureBoxSizeMode.CenterImage,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        }, 1, 0);

        var attrBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            Margin = new Padding(UiScale(3, dpi), UiScale(12, dpi), UiScale(3, dpi), UiScale(12, dpi))
        };
        attrBox.Items.AddRange(["0", "1", "2", "3"]);
        attrBox.SelectedIndex = attr;
        attrBox.SelectedIndexChanged += (_, _) =>
        {
            if (_refreshing || _rom is null || _renderer is null || attrBox.SelectedIndex < 0) return;
            BeforeEdit?.Invoke(this, EventArgs.Empty);
            _rom.SetTerrainAttribute(id, (byte)attrBox.SelectedIndex);
            _renderer.InvalidateCache();
            DataChanged?.Invoke(this, EventArgs.Empty);
            if (IsHandleCreated) BeginInvoke(new Action(Rebuild)); else Rebuild();
        };
        row.Controls.Add(attrBox, 2, 0);

        for (var q = 0; q < 4; q++)
            row.Controls.Add(BuildTileButton(id, q, tiles[q], attr, dpi), 3 + q, 0);

        return row;
    }

    private Control BuildTileButton(int id, int quadrant, byte tile, byte attr, int dpi)
    {
        var image = new Bitmap(_renderer!.GetChrTileBitmap(tile, attr, 3));
        _ownedImages.Add(image);
        var names = new[] { "TL", "TR", "BL", "BR" };
        var button = new Button
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(UiScale(2, dpi)),
            Padding = new Padding(UiScale(2, dpi), 0, UiScale(2, dpi), 0),
            Image = image,
            ImageAlign = ContentAlignment.MiddleLeft,
            Text = $"{names[quadrant]} ${tile:X2}",
            TextAlign = ContentAlignment.MiddleRight,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Tag = (id, quadrant),
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 8.5f)
        };
        button.Click += (_, _) =>
        {
            if (_rom is null || _renderer is null) return;
            using var picker = new ChrTilePickerForm(
                _renderer,
                (byte)(_rom.GetTerrainAttribute(id) & 3),
                tile,
                $"Terrain ${id:X2} / {names[quadrant]}");
            if (picker.ShowDialog(FindForm()) != DialogResult.OK) return;
            BeforeEdit?.Invoke(this, EventArgs.Empty);
            _rom.SetTerrainTile(id, quadrant, picker.SelectedTile);
            _renderer.InvalidateCache();
            DataChanged?.Invoke(this, EventArgs.Empty);
            if (IsHandleCreated) BeginInvoke(new Action(Rebuild)); else Rebuild();
        };
        return button;
    }

    private void EnsureScrollExtent()
    {
        if (_content.Width <= 0 || _content.Height <= 0)
        {
            _viewport.AutoScrollMinSize = Size.Empty;
            return;
        }

        // Explicit AutoScrollMinSize guarantees a real bottom horizontal bar even
        // when the right editor pane is narrow on 1366x768 / 1280x720 screens.
        _viewport.AutoScrollMinSize = new Size(_content.Width + 2, _content.Height + 2);
    }

    private static int UiScale(int logicalPixels, int dpi)
    {
        // Cap layout growth at 125%. Fonts remain DPI aware, but oversized
        // runtime-created panels no longer consume the entire 768p workspace.
        var scale = Math.Clamp(dpi / 96.0, 1.0, 1.25);
        return Math.Max(1, (int)Math.Round(logicalPixels * scale));
    }

    private void ClearRows()
    {
        foreach (Control row in _content.Controls) DetachImagesRecursive(row);
        while (_content.Controls.Count > 0)
        {
            var c = _content.Controls[0];
            _content.Controls.RemoveAt(0);
            c.Dispose();
        }
        foreach (var image in _ownedImages) image.Dispose();
        _ownedImages.Clear();
        _content.Size = Size.Empty;
        _viewport.AutoScrollMinSize = Size.Empty;
        _viewport.AutoScrollPosition = Point.Empty;
    }

    private static void DetachImagesRecursive(Control control)
    {
        if (control is Button b) b.Image = null;
        if (control is PictureBox p) p.Image = null;
        foreach (Control child in control.Controls) DetachImagesRecursive(child);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) ClearRows();
        base.Dispose(disposing);
    }
}
