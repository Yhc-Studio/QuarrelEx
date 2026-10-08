using System.Drawing.Drawing2D;

namespace QuarrelEx;

/// <summary>
/// Small Fluent-style line glyphs shared by the toolbar, menus and the Modern UI navigation.
/// The glyphs are drawn at runtime so the public source package does not need third-party icon assets.
/// </summary>
public static class EditorToolIcons
{
    public static Bitmap Create(EditorToolKind kind, int size = 20, Color? foreground = null)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        var color = foreground ?? Color.FromArgb(75, 82, 92);
        var scale = size / 20f;
        using var pen = new Pen(color, Math.Max(1.35f, 1.65f * scale))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var thin = new Pen(color, Math.Max(1.0f, 1.25f * scale))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var brush = new SolidBrush(color);

        float X(float value) => value * scale;
        RectangleF R(float x, float y, float w, float h) => new(X(x), X(y), X(w), X(h));

        switch (kind)
        {
            case EditorToolKind.Enemy:
                // Tank outline.
                DrawRoundedRectangle(g, pen, R(3.5f, 8f, 13f, 7f), X(1.5f));
                g.DrawRectangle(pen, X(8f), X(5f), X(4f), X(3f));
                g.DrawLine(pen, X(12f), X(6.5f), X(17f), X(6.5f));
                g.DrawLine(thin, X(5f), X(17f), X(15f), X(17f));
                break;

            case EditorToolKind.Tsa:
                // Four consistent tiles.
                DrawRoundedRectangle(g, pen, R(3f, 3f, 5.5f, 5.5f), X(1.1f));
                DrawRoundedRectangle(g, pen, R(11.5f, 3f, 5.5f, 5.5f), X(1.1f));
                DrawRoundedRectangle(g, pen, R(3f, 11.5f, 5.5f, 5.5f), X(1.1f));
                DrawRoundedRectangle(g, pen, R(11.5f, 11.5f, 5.5f, 5.5f), X(1.1f));
                break;

            case EditorToolKind.Palette:
                // Palette outline with three color wells.
                g.DrawEllipse(pen, R(3f, 3f, 14f, 14f));
                g.FillEllipse(brush, R(6f, 6f, 2.2f, 2.2f));
                g.FillEllipse(brush, R(10f, 5f, 2.2f, 2.2f));
                g.FillEllipse(brush, R(13f, 8.5f, 2.2f, 2.2f));
                g.DrawArc(pen, R(8.5f, 10f, 6f, 5f), 25f, 145f);
                break;

            case EditorToolKind.FlagTsa:
                g.DrawLine(pen, X(5f), X(3f), X(5f), X(17f));
                g.DrawLine(pen, X(5f), X(4f), X(15f), X(4f));
                g.DrawLine(pen, X(15f), X(4f), X(13f), X(9f));
                g.DrawLine(pen, X(13f), X(9f), X(5f), X(9f));
                g.DrawLine(pen, X(3f), X(17f), X(9f), X(17f));
                break;

            case EditorToolKind.GameSettings:
                // Sliders read more clearly than a tiny gear at 18–20 px.
                g.DrawLine(pen, X(3f), X(5f), X(17f), X(5f));
                g.DrawLine(pen, X(3f), X(10f), X(17f), X(10f));
                g.DrawLine(pen, X(3f), X(15f), X(17f), X(15f));
                g.FillEllipse(brush, R(6f, 3.2f, 3.6f, 3.6f));
                g.FillEllipse(brush, R(12f, 8.2f, 3.6f, 3.6f));
                g.FillEllipse(brush, R(8.5f, 13.2f, 3.6f, 3.6f));
                break;

            case EditorToolKind.ExOptions:
                // Spark / feature glyph.
                PointF[] diamond =
                [
                    new(X(10f), X(2.5f)), new(X(12f), X(8f)), new(X(17.5f), X(10f)),
                    new(X(12f), X(12f)), new(X(10f), X(17.5f)), new(X(8f), X(12f)),
                    new(X(2.5f), X(10f)), new(X(8f), X(8f))
                ];
                g.DrawPolygon(pen, diamond);
                break;

            case EditorToolKind.RomInfo:
                g.DrawEllipse(pen, R(3f, 3f, 14f, 14f));
                g.FillEllipse(brush, R(9f, 5.2f, 2f, 2f));
                g.DrawLine(pen, X(10f), X(9f), X(10f), X(14.5f));
                break;

            case EditorToolKind.Screen:
                DrawRoundedRectangle(g, pen, R(2.5f, 3.5f, 15f, 12f), X(1.3f));
                g.DrawLine(thin, X(7f), X(17.5f), X(13f), X(17.5f));
                g.DrawLine(thin, X(10f), X(15.7f), X(10f), X(17.5f));
                break;
        }

        return bmp;
    }

    private static void DrawRoundedRectangle(Graphics g, Pen pen, RectangleF rect, float radius)
    {
        var d = radius * 2f;
        using var path = new GraphicsPath();
        path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        g.DrawPath(pen, path);
    }
}
