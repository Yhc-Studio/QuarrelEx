using System.Drawing.Drawing2D;

namespace QuarrelEx;

/// <summary>
/// Compact 20px toolbar glyphs for the Quarrel-style tool windows.
/// The public source package intentionally does not bundle third-party Quarrel artwork; Windows uses the default application icon unless you provide your own.
/// These glyphs are deliberately simple pixel-like symbols so they remain legible on 720p/768p screens.
/// </summary>
public static class EditorToolIcons
{
    public static Bitmap Create(EditorToolKind kind, int size = 20)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.Clear(Color.Transparent);

        float s = size / 20f;
        RectangleF R(float x, float y, float w, float h) => new(x * s, y * s, w * s, h * s);
        using var dark = new Pen(Color.FromArgb(45, 45, 45), Math.Max(1f, s));
        using var fill = new SolidBrush(Color.FromArgb(235, 235, 235));
        using var accent = new SolidBrush(Color.FromArgb(90, 130, 90));
        using var accent2 = new SolidBrush(Color.FromArgb(145, 105, 65));
        using var blue = new SolidBrush(Color.FromArgb(80, 120, 165));
        using var yellow = new SolidBrush(Color.FromArgb(215, 180, 70));
        using var red = new SolidBrush(Color.FromArgb(175, 75, 75));

        switch (kind)
        {
            case EditorToolKind.Enemy:
                // Small tank: body, turret, tracks.
                g.FillRectangle(accent, R(4, 7, 12, 7));
                g.FillRectangle(fill, R(8, 4, 4, 4));
                g.DrawRectangle(dark, Rectangle.Round(R(4, 7, 12, 7)));
                g.DrawRectangle(dark, Rectangle.Round(R(8, 4, 4, 4)));
                g.DrawLine(dark, 12 * s, 5 * s, 17 * s, 5 * s);
                g.DrawLine(dark, 4 * s, 16 * s, 16 * s, 16 * s);
                break;
            case EditorToolKind.Tsa:
                // 2x2 composite block.
                g.FillRectangle(accent2, R(3, 3, 6, 6));
                g.FillRectangle(blue, R(11, 3, 6, 6));
                g.FillRectangle(blue, R(3, 11, 6, 6));
                g.FillRectangle(accent2, R(11, 11, 6, 6));
                g.DrawRectangle(dark, Rectangle.Round(R(3, 3, 14, 14)));
                g.DrawLine(dark, 10 * s, 3 * s, 10 * s, 17 * s);
                g.DrawLine(dark, 3 * s, 10 * s, 17 * s, 10 * s);
                break;
            case EditorToolKind.Palette:
                g.FillRectangle(red, R(3, 3, 6, 6));
                g.FillRectangle(yellow, R(11, 3, 6, 6));
                g.FillRectangle(accent, R(3, 11, 6, 6));
                g.FillRectangle(blue, R(11, 11, 6, 6));
                g.DrawRectangle(dark, Rectangle.Round(R(2, 2, 16, 16)));
                break;
            case EditorToolKind.FlagTsa:
                g.DrawLine(dark, 5 * s, 3 * s, 5 * s, 18 * s);
                g.FillRectangle(red, R(6, 4, 10, 7));
                g.DrawRectangle(dark, Rectangle.Round(R(6, 4, 10, 7)));
                g.DrawLine(dark, 3 * s, 18 * s, 9 * s, 18 * s);
                break;
            case EditorToolKind.GameSettings:
                // Gear-like circle plus small wrench stem.
                g.FillEllipse(fill, R(4, 4, 11, 11));
                g.DrawEllipse(dark, R(4, 4, 11, 11));
                g.FillEllipse(blue, R(8, 8, 3, 3));
                g.DrawLine(dark, 13 * s, 13 * s, 18 * s, 18 * s);
                g.DrawLine(dark, 14 * s, 17 * s, 17 * s, 14 * s);
                break;
            case EditorToolKind.ExOptions:
                // EX star / feature switch.
                PointF[] star = [
                    new(10*s,2*s), new(12*s,7*s), new(18*s,7*s), new(13.5f*s,10.5f*s),
                    new(15.5f*s,17*s), new(10*s,13*s), new(4.5f*s,17*s), new(6.5f*s,10.5f*s),
                    new(2*s,7*s), new(8*s,7*s)
                ];
                g.FillPolygon(yellow, star);
                g.DrawPolygon(dark, star);
                break;
            case EditorToolKind.RomInfo:
                g.FillEllipse(blue, R(3, 3, 14, 14));
                g.DrawEllipse(dark, R(3, 3, 14, 14));
                using (var white = new SolidBrush(Color.White))
                {
                    g.FillRectangle(white, R(9, 8, 2, 7));
                    g.FillRectangle(white, R(9, 5, 2, 2));
                }
                break;
        }
        return bmp;
    }
}
