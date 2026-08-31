using System.Drawing.Drawing2D;
using QuarrelEx.Core;
using QuarrelEx.Rendering;
using QuarrelEx.Localization;

namespace QuarrelEx.Controls;

/// <summary>
/// Visual 256x240 spawn-point editor.  The stage map is used as the backdrop and
/// the five spawn objects are drawn with Battle City's real tank CHR graphics.
/// Drag a tank to edit its ROM X/Y bytes; numeric editors in GameSettingsControl
/// remain synchronized with this view.
/// </summary>
public sealed class SpawnPositionEditorControl : Control
{
    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private Func<int>? _stageProvider;
    private SpawnKind? _dragKind;
    private SpawnKind _selectedKind = SpawnKind.Player1;
    private bool _editStarted;

    public event EventHandler? BeforeEdit;
    public event EventHandler? LivePositionChanged;
    public event EventHandler? DataChanged;
    public event EventHandler? SelectionChanged;

    public SpawnKind SelectedKind
    {
        get => _selectedKind;
        set { _selectedKind = value; Invalidate(); SelectionChanged?.Invoke(this, EventArgs.Empty); }
    }

    public SpawnPositionEditorControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(34, 38, 42);
        MinimumSize = new Size(360, 338);
        Size = new Size(480, 450);
        Cursor = Cursors.Cross;
        TabStop = true;
    }

    public void Bind(BattleCityRom? rom, NesRenderer? renderer, Func<int>? stageProvider)
    {
        _rom = rom;
        _renderer = renderer;
        _stageProvider = stageProvider;
        Enabled = rom is not null;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.SmoothingMode = SmoothingMode.None;

        var r = LogicalViewport();
        using (var bg = new SolidBrush(Color.FromArgb(22, 26, 29))) e.Graphics.FillRectangle(bg, r);
        using (var border = new Pen(Color.FromArgb(105, 115, 125))) e.Graphics.DrawRectangle(border, r.X, r.Y, r.Width - 1, r.Height - 1);

        if (_rom is null || _renderer is null)
        {
            TextRenderer.DrawText(e.Graphics, I18n.T("spawn.open_note"), Font, Rectangle.Round(r), Color.Gainsboro,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        DrawStage(e.Graphics, r);
        DrawGrid(e.Graphics, r);
        foreach (var kind in DrawOrder) DrawSpawn(e.Graphics, r, kind);

        var footer = new Rectangle(ClientRectangle.Left + 4, ClientRectangle.Bottom - 22, Math.Max(0, ClientRectangle.Width - 8), 18);
        TextRenderer.DrawText(e.Graphics, I18n.T("spawn.drag_note"),
            Font, footer, Color.Gainsboro, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static readonly SpawnKind[] DrawOrder =
    [
        SpawnKind.Enemy1, SpawnKind.Enemy2, SpawnKind.Enemy3, SpawnKind.Player1, SpawnKind.Player2
    ];

    private void DrawStage(Graphics g, RectangleF view)
    {
        if (_rom is null || _renderer is null || _stageProvider is null) return;
        var stage = Math.Clamp(_stageProvider(), 1, _rom.MaxEditableStage);
        var sx = view.Width / 256f;
        var sy = view.Height / 240f;
        for (var row = 0; row < 13; row++)
        for (var col = 0; col < 13; col++)
        {
            var id = _rom.GetCell(stage, row, col);
            using var block = new Bitmap(_renderer.GetBlockBitmap(id, 1));
            // Battle City's battlefield occupies X=16..223, Y=16..223.
            var dest = new RectangleF(view.Left + (16 + col * 16) * sx,
                                      view.Top + (16 + row * 16) * sy,
                                      16 * sx, 16 * sy);
            g.DrawImage(block, dest);
        }
    }

    private static void DrawGrid(Graphics g, RectangleF view)
    {
        var sx = view.Width / 256f;
        var sy = view.Height / 240f;
        using var p = new Pen(Color.FromArgb(35, 255, 255, 255), 1f);
        for (var i = 0; i <= 13; i++)
        {
            var x = view.Left + (16 + i * 16) * sx;
            var y = view.Top + (16 + i * 16) * sy;
            g.DrawLine(p, x, view.Top + 16 * sy, x, view.Top + 224 * sy);
            g.DrawLine(p, view.Left + 16 * sx, y, view.Left + 224 * sx, y);
        }
    }

    private void DrawSpawn(Graphics g, RectangleF view, SpawnKind kind)
    {
        if (_rom is null || _renderer is null) return;
        var p = _rom.GetSpawn(kind);
        var sx = view.Width / 256f;
        var sy = view.Height / 240f;
        var cx = view.Left + p.X * sx;
        var cy = view.Top + Math.Min((int)p.Y, 239) * sy;
        var w = Math.Max(18f, 16f * sx);
        var h = Math.Max(18f, 16f * sy);
        var dest = new RectangleF(cx - w / 2, cy - h / 2, w, h);
        using var tank = new Bitmap(_renderer.GetSpawnTankBitmap(kind, 2));
        g.DrawImage(tank, dest);

        var selected = kind == _selectedKind;
        using var pen = new Pen(selected ? Color.Yellow : Color.FromArgb(220, Color.White), selected ? 2f : 1f);
        g.DrawRectangle(pen, dest.X, dest.Y, dest.Width, dest.Height);

        var label = SpawnLabel(kind);
        var lr = Rectangle.Round(new RectangleF(dest.X - 18, dest.Bottom + 1, dest.Width + 36, 17));
        using var labelFont = new Font(Font.FontFamily, Math.Max(7f, Font.Size - 1f), FontStyle.Bold);
        TextRenderer.DrawText(g, label, labelFont, lr,
            selected ? Color.Yellow : Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPadding);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || _rom is null) return;
        Focus();
        var logical = ClientToLogical(e.Location);
        var hit = HitTest(logical.X, logical.Y);
        if (hit is null) return;
        _selectedKind = hit.Value;
        _dragKind = hit;
        _editStarted = true;
        Capture = true;
        BeforeEdit?.Invoke(this, EventArgs.Empty);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_rom is null || _dragKind is null || (e.Button & MouseButtons.Left) == 0) return;
        var p = ClientToLogical(e.Location);
        var x = (byte)Math.Clamp(p.X, 0, 255);
        var y = (byte)Math.Clamp(p.Y, 0, 239);
        _rom.SetSpawn(_dragKind.Value, x, y);
        LivePositionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_dragKind is null) return;
        _dragKind = null;
        Capture = false;
        if (_editStarted)
        {
            _editStarted = false;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private Point ClientToLogical(Point point)
    {
        var r = LogicalViewport();
        if (r.Width <= 0 || r.Height <= 0) return Point.Empty;
        return new Point(
            (int)Math.Round((point.X - r.Left) * 256.0 / r.Width),
            (int)Math.Round((point.Y - r.Top) * 240.0 / r.Height));
    }

    private SpawnKind? HitTest(int x, int y)
    {
        if (_rom is null) return null;
        SpawnKind? best = null;
        var bestDist = int.MaxValue;
        foreach (var kind in DrawOrder)
        {
            var p = _rom.GetSpawn(kind);
            var dx = x - p.X;
            var dy = y - p.Y;
            var d = dx * dx + dy * dy;
            if (Math.Abs(dx) <= 12 && Math.Abs(dy) <= 12 && d < bestDist) { best = kind; bestDist = d; }
        }
        return best;
    }

    private RectangleF LogicalViewport()
    {
        const float logicalW = 256f, logicalH = 240f;
        var usableW = Math.Max(32, ClientSize.Width - 12);
        var usableH = Math.Max(32, ClientSize.Height - 30);
        var scale = Math.Min(usableW / logicalW, usableH / logicalH);
        var w = logicalW * scale;
        var h = logicalH * scale;
        return new RectangleF((ClientSize.Width - w) / 2f, 4f + Math.Max(0, (usableH - h) / 2f), w, h);
    }

    private static string SpawnLabel(SpawnKind kind) => kind switch
    {
        SpawnKind.Player1 => "P1",
        SpawnKind.Player2 => "P2",
        SpawnKind.Enemy1 => "E1",
        SpawnKind.Enemy2 => "E2",
        SpawnKind.Enemy3 => "E3",
        _ => "?"
    };
}
