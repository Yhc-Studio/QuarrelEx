namespace QuarrelEx;

/// <summary>
/// Modeless editor window used by the Quarrel-style desktop layout.
/// Clicking the window close button hides the tool so its editor state,
/// scroll position and dynamically-created previews remain alive.
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
        MinimumSize = new Size(320, 240);
        Icon = icon;

        content.Dock = DockStyle.Fill;
        Controls.Add(content);

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
