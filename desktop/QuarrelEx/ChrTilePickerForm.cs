using QuarrelEx.Rendering;

namespace QuarrelEx;

public sealed class ChrTilePickerForm : Form
{
    private readonly FlowLayoutPanel _tiles = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        WrapContents = true,
        Padding = new Padding(8)
    };
    private readonly List<Image> _ownedImages = new();

    public byte SelectedTile { get; private set; }

    public ChrTilePickerForm(NesRenderer renderer, byte attr, byte current, string targetName)
    {
        Text = $"CHR Tile 选择器 - {targetName}";
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        ClientSize = new Size(760, 640);
        MinimumSize = new Size(540, 420);
        SelectedTile = current;

        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var info = new Label
        {
            AutoSize = true,
            Padding = new Padding(10),
            Text = $"Attr = {attr & 3}（背景调色板 {attr & 3}） · 当前 Tile = ${current:X2}。点击任意 8×8 CHR Tile 完成选择。"
        };
        root.Controls.Add(info, 0, 0);
        root.Controls.Add(_tiles, 0, 1);
        Controls.Add(root);

        BuildTiles(renderer, (byte)(attr & 3), current);
        FormClosed += (_, _) => DisposeOwnedImages();
    }

    private void BuildTiles(NesRenderer renderer, byte attr, byte current)
    {
        _tiles.SuspendLayout();
        try
        {
            for (var tile = 0; tile <= 0xFF; tile++)
            {
                var image = new Bitmap(renderer.GetChrTileBitmap((byte)tile, attr, 4));
                _ownedImages.Add(image);
                var value = (byte)tile;
                var b = new Button
                {
                    Width = 62,
                    Height = 72,
                    Margin = new Padding(3),
                    Image = image,
                    ImageAlign = ContentAlignment.TopCenter,
                    Text = $"${tile:X2}",
                    TextAlign = ContentAlignment.BottomCenter,
                    TextImageRelation = TextImageRelation.ImageAboveText,
                    FlatStyle = tile == current ? FlatStyle.Popup : FlatStyle.Standard,
                    BackColor = tile == current ? Color.LightSkyBlue : SystemColors.Control
                };
                b.Click += (_, _) =>
                {
                    SelectedTile = value;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                _tiles.Controls.Add(b);
            }
        }
        finally { _tiles.ResumeLayout(); }
    }

    private void DisposeOwnedImages()
    {
        foreach (var image in _ownedImages) image.Dispose();
        _ownedImages.Clear();
    }
}
