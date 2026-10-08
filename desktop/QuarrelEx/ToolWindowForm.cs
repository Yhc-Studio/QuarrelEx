namespace QuarrelEx;

/// <summary>
/// Modeless editor window used by the desktop layout.  The native Windows frame
/// is retained for reliable snapping/DPI behavior, while the client area uses the
/// shared ModernTheme.  Clicking close hides the tool so editor state and scroll
/// position remain alive.
/// </summary>
public sealed class ToolWindowForm : Form
{
    public EditorToolKind ToolKind { get; }

    public ToolWindowForm(EditorToolKind kind, string title, Control content, Size clientSize, Icon? icon)
    {
        ToolKind = kind;
        Text = title;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        ClientSize = clientSize;
        DoubleBuffered = true;
        MinimumSize = new Size(360, 280);
        BackColor = ModernTheme.WindowBack;
        ForeColor = ModernTheme.Text;
        Font = ModernTheme.CreateUiFont();
        Icon = icon;

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = ModernTheme.WindowBack
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 52,
            BackColor = ModernTheme.Surface,
            Padding = new Padding(16, 8, 16, 8),
            Margin = Padding.Empty
        };
        var titleLabel = new Label
        {
            Text = title.Replace("Quarrel Ex - ", string.Empty),
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = ModernTheme.CreateUiFont(12f, FontStyle.Bold),
            ForeColor = ModernTheme.Text
        };
        header.Controls.Add(titleLabel);

        var contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ModernTheme.WindowBack,
            Padding = new Padding(10),
            Margin = Padding.Empty
        };
        content.Dock = DockStyle.Fill;
        contentHost.Controls.Add(content);

        shell.Controls.Add(header, 0, 0);
        shell.Controls.Add(contentHost, 0, 1);
        Controls.Add(shell);
        ModernTheme.Apply(this);

        FormClosing += (_, e) =>
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        };
    }
}
