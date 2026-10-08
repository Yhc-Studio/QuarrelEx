using System.Drawing.Drawing2D;

namespace QuarrelEx;

/// <summary>
/// Lightweight Fluent-inspired styling for the WinForms desktop editor.
/// This deliberately avoids third-party UI frameworks so the editor stays
/// self-contained and continues to build with the stock .NET 8 Windows Desktop SDK.
/// </summary>
public static class ModernTheme
{
    public static readonly Color WindowBack = Color.FromArgb(246, 247, 249);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(240, 242, 245);
    public static readonly Color NavBack = Color.FromArgb(31, 35, 43);
    public static readonly Color NavHover = Color.FromArgb(47, 52, 62);
    public static readonly Color NavSelected = Color.FromArgb(63, 70, 84);
    public static readonly Color Text = Color.FromArgb(31, 35, 41);
    public static readonly Color Muted = Color.FromArgb(102, 109, 120);
    public static readonly Color Border = Color.FromArgb(218, 221, 226);
    public static readonly Color Accent = Color.FromArgb(0, 120, 212);
    public static readonly Color AccentHover = Color.FromArgb(16, 110, 190);

    private static readonly string UiFontFamily = ResolveUiFontFamily();

    public static Font CreateUiFont(float size = 9.5f, FontStyle style = FontStyle.Regular)
    {
        return new Font(UiFontFamily, size, style, GraphicsUnit.Point);
    }

    private static string ResolveUiFontFamily()
    {
        // The previous Segoe UI Variable choice has no CJK glyphs, so Windows
        // may fall back to SimSun for Chinese text.  Prefer Microsoft YaHei UI
        // explicitly so Chinese and Latin labels share the same visual metrics.
        string[] candidates = [
            "Microsoft YaHei UI",
            "Microsoft YaHei",
            "Segoe UI Variable Text",
            "Segoe UI"
        ];

        foreach (var candidate in candidates)
        {
            try
            {
                using var probe = new Font(candidate, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
                if (string.Equals(probe.FontFamily.Name, candidate, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }
            catch
            {
                // Try the next installed family.
            }
        }

        return SystemFonts.MessageBoxFont.FontFamily.Name;
    }

    public static void Apply(Form form)
    {
        form.BackColor = WindowBack;
        form.ForeColor = Text;
        form.Font = CreateUiFont();
        ApplyRecursive(form);
    }

    public static void ApplyRecursive(Control root)
    {
        foreach (Control c in root.Controls)
        {
            StyleControl(c);
            ApplyRecursive(c);
        }
    }

    public static void StyleControl(Control c)
    {
        c.Font = CreateUiFont(c.Font.SizeInPoints <= 0 ? 9.25f : c.Font.SizeInPoints, c.Font.Style);

        switch (c)
        {
            case Button b:
                if (b.Tag is EditorToolKind || b.Tag is string navTag && navTag.StartsWith("ModernNav", StringComparison.Ordinal))
                    break;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = Border;
                b.FlatAppearance.BorderSize = 1;
                b.BackColor = Surface;
                b.ForeColor = Text;
                b.Padding = new Padding(Math.Max(b.Padding.Left, 8), Math.Max(b.Padding.Top, 3), Math.Max(b.Padding.Right, 8), Math.Max(b.Padding.Bottom, 3));
                b.UseVisualStyleBackColor = false;
                break;
            case GroupBox g:
                g.ForeColor = Text;
                g.BackColor = Surface;
                break;
            case TextBoxBase tb:
                tb.BackColor = Surface;
                tb.ForeColor = Text;
                break;
            case ComboBox combo:
                combo.BackColor = Surface;
                combo.ForeColor = Text;
                combo.FlatStyle = FlatStyle.Flat;
                break;
            case NumericUpDown num:
                num.BackColor = Surface;
                num.ForeColor = Text;
                num.BorderStyle = BorderStyle.FixedSingle;
                break;
            case CheckBox cb:
                cb.ForeColor = Text;
                cb.BackColor = Color.Transparent;
                break;
            case RadioButton rb:
                rb.ForeColor = Text;
                rb.BackColor = Color.Transparent;
                break;
            case Label label:
                if (label.ForeColor == SystemColors.ControlText)
                    label.ForeColor = Text;
                break;
            case DataGridView grid:
                StyleGrid(grid);
                break;
            case TabControl tabs:
                tabs.BackColor = WindowBack;
                tabs.ForeColor = Text;
                break;
            case TabPage page:
                page.BackColor = WindowBack;
                page.ForeColor = Text;
                break;
            case MenuStrip menu:
                StyleToolStrip(menu);
                break;
            case StatusStrip status:
                StyleToolStrip(status);
                break;
            case ToolStrip strip:
                StyleToolStrip(strip);
                break;
        }
    }

    public static void StyleToolStrip(ToolStrip strip)
    {
        strip.RenderMode = ToolStripRenderMode.Professional;
        strip.Renderer = new ModernToolStripRenderer();
        strip.BackColor = Surface;
        strip.ForeColor = Text;
        strip.Font = CreateUiFont(9.5f);
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Border;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceAlt;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Text;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 239, 255);
        grid.DefaultCellStyle.SelectionForeColor = Text;
        grid.RowHeadersDefaultCellStyle.BackColor = SurfaceAlt;
        grid.RowHeadersDefaultCellStyle.ForeColor = Muted;
    }

    public static Button CreateNavigationButton(string text, Image? image = null)
    {
        return new ModernNavigationButton
        {
            Text = text,
            Glyph = image,
            Height = 42,
            BackColor = NavBack,
            ForeColor = Color.FromArgb(235, 238, 243),
            Cursor = Cursors.Hand,
            TabStop = false,
            Font = CreateUiFont(9.75f),
            UseVisualStyleBackColor = false
        };
    }

    public static Panel CreateCard(int padding = 14)
    {
        return new ModernCardPanel
        {
            BackColor = Surface,
            Padding = new Padding(padding),
            Margin = new Padding(6)
        };
    }

    private sealed class ModernToolStripRenderer : ToolStripProfessionalRenderer
    {
        public ModernToolStripRenderer() : base(new ModernColorTable()) { RoundedEdges = false; }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(Border);
            e.Graphics.DrawLine(pen, 0, e.AffectedBounds.Bottom - 1, e.AffectedBounds.Right, e.AffectedBounds.Bottom - 1);
        }
    }

    private sealed class ModernColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => Surface;
        public override Color ToolStripGradientMiddle => Surface;
        public override Color ToolStripGradientEnd => Surface;
        public override Color MenuStripGradientBegin => Surface;
        public override Color MenuStripGradientEnd => Surface;
        public override Color ImageMarginGradientBegin => Surface;
        public override Color ImageMarginGradientMiddle => Surface;
        public override Color ImageMarginGradientEnd => Surface;
        public override Color MenuItemSelected => Color.FromArgb(233, 241, 250);
        public override Color MenuItemBorder => Border;
        public override Color MenuBorder => Border;
        public override Color ButtonSelectedHighlight => Color.FromArgb(233, 241, 250);
        public override Color ButtonSelectedBorder => Border;
        public override Color ButtonPressedHighlight => Color.FromArgb(224, 235, 247);
        public override Color ButtonPressedBorder => Border;
        public override Color SeparatorDark => Border;
        public override Color SeparatorLight => Surface;
        public override Color StatusStripGradientBegin => Surface;
        public override Color StatusStripGradientEnd => Surface;
    }
}

/// <summary>
/// Owner-drawn navigation button with a fixed icon column and text baseline.
/// Using the stock Button ImageBeforeText layout made different glyph bounds
/// look horizontally misaligned even when the bitmap sizes were identical.
/// </summary>
public sealed class ModernNavigationButton : Button
{
    private bool _hovered;
    private bool _pressed;

    public Image? Glyph { get; set; }

    public ModernNavigationButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        Margin = Padding.Empty;
        Padding = Padding.Empty;
        SetStyle(ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        if (mevent.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(ModernTheme.NavBack);

        var bounds = ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        var card = new Rectangle(2, 1, Math.Max(1, bounds.Width - 4), Math.Max(1, bounds.Height - 2));
        var fill = _pressed
            ? ModernTheme.NavSelected
            : _hovered ? ModernTheme.NavHover : ModernTheme.NavBack;

        if (fill != ModernTheme.NavBack)
        {
            using var path = RoundedRect(card, 7f);
            using var brush = new SolidBrush(fill);
            g.FillPath(brush, path);
        }

        if (Glyph is not null)
        {
            const int iconSize = 18;
            var iconX = 14;
            var iconY = (Height - iconSize) / 2;
            // Glyphs supplied by EditorToolIcons are already tinted. Draw them
            // in one fixed 18x18 slot so all labels start at exactly the same X.
            g.DrawImage(Glyph, new Rectangle(iconX, iconY, iconSize, iconSize));
        }

        var textColor = Enabled ? ForeColor : Color.FromArgb(112, 120, 132);
        var textRect = new Rectangle(46, 0, Math.Max(0, Width - 58), Height);
        TextRenderer.DrawText(
            g,
            Text,
            Font,
            textRect,
            textColor,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPadding);

        if (Focused && ShowFocusCues)
        {
            var focus = Rectangle.Inflate(card, -4, -4);
            ControlPaint.DrawFocusRectangle(g, focus, textColor, fill);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle rect, float radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2f;
        path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public sealed class ModernCardPanel : Panel
{
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(ModernTheme.Border);
        var rect = ClientRectangle;
        rect.Width -= 1;
        rect.Height -= 1;
        if (rect.Width > 0 && rect.Height > 0)
            e.Graphics.DrawRectangle(pen, rect);
    }
}
