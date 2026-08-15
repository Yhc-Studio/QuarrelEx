using QuarrelEx.Rendering;

namespace QuarrelEx;

public sealed class PaletteColorPickerForm : Form
{
    public byte SelectedColor { get; private set; }

    public PaletteColorPickerForm(byte selected)
    {
        SelectedColor = (byte)(selected & 0x3F);
        Text = "NES Palette Color";
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(500, 155);
        MinimumSize = new Size(460, 145);

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 16,
            RowCount = 4,
            Padding = new Padding(6)
        };
        for (var i = 0; i < 16; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6.25f));
        for (var i = 0; i < 4; i++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

        for (var i = 0; i < 64; i++)
        {
            var value = (byte)i;
            var color = NesRenderer.GetNesColor(value);
            var b = new Button
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(1),
                Padding = Padding.Empty,
                Text = $"{i:X2}",
                BackColor = color,
                ForeColor = color.GetBrightness() < 0.45 ? Color.White : Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 7.5f, value == SelectedColor ? FontStyle.Bold : FontStyle.Regular)
            };
            if (value == SelectedColor) b.FlatAppearance.BorderSize = 3;
            b.Click += (_, _) =>
            {
                SelectedColor = value;
                DialogResult = DialogResult.OK;
                Close();
            };
            grid.Controls.Add(b, i % 16, i / 16);
        }
        Controls.Add(grid);
    }
}
