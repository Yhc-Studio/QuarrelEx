using System.Text;
using QuarrelEx.Localization;

namespace QuarrelEx;

public sealed class HelpViewerForm : Form
{
    private readonly RichTextBox _text = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BorderStyle = BorderStyle.None,
        BackColor = SystemColors.Window,
        ForeColor = SystemColors.WindowText,
        WordWrap = true,
        DetectUrls = true,
        ScrollBars = RichTextBoxScrollBars.Vertical,
        Font = SystemFonts.MessageBoxFont
    };

    public HelpViewerForm(string title, string filePath)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        ClientSize = new Size(820, 680);
        MinimumSize = new Size(560, 420);

        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // The help window still works even if the shell cannot extract the icon.
        }

        var top = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        var close = new ToolStripButton(I18n.T("common.close")) { Alignment = ToolStripItemAlignment.Right };
        close.Click += (_, _) => Close();
        top.Items.Add(close);

        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14) };
        panel.Controls.Add(_text);

        Controls.Add(panel);
        Controls.Add(top);

        try
        {
            _text.Text = File.ReadAllText(filePath, Encoding.UTF8);
            _text.SelectionStart = 0;
            _text.ScrollToCaret();
        }
        catch (Exception ex)
        {
            _text.Text = $"Unable to load help file:\r\n{filePath}\r\n\r\n{ex.Message}";
        }
    }
}
