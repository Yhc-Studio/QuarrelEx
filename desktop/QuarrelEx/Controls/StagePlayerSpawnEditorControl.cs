using System.Drawing.Drawing2D;
using QuarrelEx.Core;
using QuarrelEx.Rendering;

namespace QuarrelEx.Controls;

/// <summary>
/// Runtime 6.9.3 / QXR1 v5 per-stage player spawn editor.
/// P1/P2 may independently use Original/global or a packed 16px-grid position.
/// </summary>
public sealed class StagePlayerSpawnEditorControl : Control
{
    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private Func<int>? _stageProvider;
    private bool _selectedTwoPlayer;
    private int _dragPlayer = -1;
    private bool _editStarted;

    public event EventHandler? BeforeEdit;
    public event EventHandler? LivePositionChanged;
    public event EventHandler? DataChanged;
    public event EventHandler? SelectionChanged;

    public bool SelectedTwoPlayer
    {
        get => _selectedTwoPlayer;
        set
        {
            _selectedTwoPlayer = value;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public StagePlayerSpawnEditorControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(34, 38, 42);
        MinimumSize = new Size(360, 360);
        Size = new Size(500, 500);
        Cursor = Cursors.Cross;
        TabStop = true;
    }

    public void Bind(BattleCityRom? rom, NesRenderer? renderer, Func<int>? stageProvider)
    {
        _rom = rom;
        _renderer = renderer;
        _stageProvider = stageProvider;
        Enabled = IsAvailable;
        Invalidate();
    }

    private int Stage => _stageProvider?.Invoke() ?? 1;
    private bool IsAvailable => _rom?.SupportsFinalRulesV5 == true && Stage is >= 1 and <= 70;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.SmoothingMode = SmoothingMode.None;

        var view = MapViewport();
        using (var bg = new SolidBrush(Color.FromArgb(18, 21, 24))) e.Graphics.FillRectangle(bg, view);

        if (!IsAvailable || _rom is null || _renderer is null)
        {
            var text = _rom?.SupportsFinalRulesV5 == true
                ? "Demo 不使用逐关玩家出生点"
                : "需要 QXR1 v5 / Runtime 6.9";
            TextRenderer.DrawText(e.Graphics, text, Font, Rectangle.Round(view), Color.Gainsboro,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        DrawStage(e.Graphics, view);
        DrawGrid(e.Graphics, view);
        DrawMarker(e.Graphics, view, false);
        DrawMarker(e.Graphics, view, true);

        var footer = new Rectangle(ClientRectangle.Left + 4, ClientRectangle.Bottom - 21, Math.Max(0, ClientRectangle.Width - 8), 18);
        TextRenderer.DrawText(e.Graphics,
            "P1 / P2：实色=Custom，半透明虚线=Original/global；拖拽会自动切换为 Custom。",
            Font, footer, Color.Gainsboro, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void DrawStage(Graphics g, RectangleF view)
    {
        if (_rom is null || _renderer is null) return;
        var cellW = view.Width / 13f;
        var cellH = view.Height / 13f;
        for (var row = 0; row < 13; row++)
        for (var col = 0; col < 13; col++)
        {
            var id = _rom.GetCell(Stage, row, col);
            using var block = new Bitmap(_renderer.GetBlockBitmap(id, 1));
            g.DrawImage(block, new RectangleF(view.Left + col * cellW, view.Top + row * cellH, cellW, cellH));
        }
    }

    private static void DrawGrid(Graphics g, RectangleF view)
    {
        using var pen = new Pen(Color.FromArgb(35, 255, 255, 255), 1f);
        for (var i = 0; i <= 13; i++)
        {
            var x = view.Left + i * view.Width / 13f;
            var y = view.Top + i * view.Height / 13f;
            g.DrawLine(pen, x, view.Top, x, view.Bottom);
            g.DrawLine(pen, view.Left, y, view.Right, y);
        }
    }

    private void DrawMarker(Graphics g, RectangleF view, bool twoPlayer)
    {
        if (_rom is null) return;
        var p = _rom.GetStagePlayerSpawn(Stage, twoPlayer);
        var colF = (p.X - BattleCityRom.CustomEnemySpawnMin) / 16f;
        var rowF = (p.Y - BattleCityRom.CustomEnemySpawnMin) / 16f;
        var cellW = view.Width / 13f;
        var cellH = view.Height / 13f;
        var cx = view.Left + (colF + .5f) * cellW;
        var cy = view.Top + (rowF + .5f) * cellH;
        var rect = new RectangleF(cx - cellW / 2f + 1, cy - cellH / 2f + 1, cellW - 2, cellH - 2);
        var cell = _rom.GetStagePlayerSpawnCell(Stage, twoPlayer);
        var warn = cell.TerrainId != 0x0D;
        var baseColor = warn
            ? Color.FromArgb(245, 158, 11)
            : twoPlayer ? Color.FromArgb(34, 197, 94) : Color.FromArgb(59, 130, 246);
        using var brush = new SolidBrush(Color.FromArgb(p.IsCustom ? 190 : 95, baseColor));
        g.FillRectangle(brush, rect);
        var selected = _selectedTwoPlayer == twoPlayer;
        using var pen = new Pen(selected ? Color.White : Color.FromArgb(220, Color.White), selected ? 3f : 1.5f);
        if (!p.IsCustom) pen.DashStyle = DashStyle.Dash;
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        using var markerFont = new Font(Font, FontStyle.Bold);
        TextRenderer.DrawText(g, twoPlayer ? "P2" : "P1", markerFont, Rectangle.Round(rect), Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || !IsAvailable || _rom is null) return;
        Focus();
        var q = ClientToRomCoord(e.Location);
        var hit = NearestPlayer(q.X, q.Y);
        if (hit < 0) hit = _selectedTwoPlayer ? 1 : 0;
        _selectedTwoPlayer = hit == 1;
        _dragPlayer = hit;
        _editStarted = true;
        Capture = true;
        BeforeEdit?.Invoke(this, EventArgs.Empty);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        MovePlayer(q.X, q.Y);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragPlayer < 0 || !IsAvailable || _rom is null || (e.Button & MouseButtons.Left) == 0) return;
        var q = ClientToRomCoord(e.Location);
        MovePlayer(q.X, q.Y);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_dragPlayer < 0) return;
        _dragPlayer = -1;
        Capture = false;
        if (_editStarted)
        {
            _editStarted = false;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsAvailable || _rom is null || !new[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down }.Contains(e.KeyCode)) return;
        e.Handled = true;
        BeforeEdit?.Invoke(this, EventArgs.Empty);
        var p = _rom.GetStagePlayerSpawn(Stage, _selectedTwoPlayer);
        var x = p.X;
        var y = p.Y;
        if (e.KeyCode == Keys.Left) x = (byte)Math.Max(BattleCityRom.CustomEnemySpawnMin, x - 16);
        if (e.KeyCode == Keys.Right) x = (byte)Math.Min(BattleCityRom.CustomEnemySpawnMax, x + 16);
        if (e.KeyCode == Keys.Up) y = (byte)Math.Max(BattleCityRom.CustomEnemySpawnMin, y - 16);
        if (e.KeyCode == Keys.Down) y = (byte)Math.Min(BattleCityRom.CustomEnemySpawnMax, y + 16);
        _rom.SetStagePlayerSpawn(Stage, _selectedTwoPlayer, x, y);
        LivePositionChanged?.Invoke(this, EventArgs.Empty);
        DataChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    private void MovePlayer(int x, int y)
    {
        if (_rom is null || _dragPlayer < 0) return;
        x = BattleCityRom.CustomEnemySpawnMin + (int)Math.Round((x - BattleCityRom.CustomEnemySpawnMin) / 16.0) * 16;
        y = BattleCityRom.CustomEnemySpawnMin + (int)Math.Round((y - BattleCityRom.CustomEnemySpawnMin) / 16.0) * 16;
        _rom.SetStagePlayerSpawn(Stage, _dragPlayer == 1, x, y);
        LivePositionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    private Point ClientToRomCoord(Point point)
    {
        var v = MapViewport();
        if (v.Width <= 0 || v.Height <= 0) return new Point(BattleCityRom.CustomEnemySpawnMin, BattleCityRom.CustomEnemySpawnMin);
        var localX = Math.Clamp((point.X - v.Left) * 208.0 / v.Width, 0, 208);
        var localY = Math.Clamp((point.Y - v.Top) * 208.0 / v.Height, 0, 208);
        return new Point(
            Math.Clamp((int)Math.Round(localX + 0x10), BattleCityRom.CustomEnemySpawnMin, BattleCityRom.CustomEnemySpawnMax),
            Math.Clamp((int)Math.Round(localY + 0x10), BattleCityRom.CustomEnemySpawnMin, BattleCityRom.CustomEnemySpawnMax));
    }

    private int NearestPlayer(int x, int y)
    {
        if (_rom is null) return -1;
        var best = -1;
        var dist = int.MaxValue;
        for (var i = 0; i < 2; i++)
        {
            var p = _rom.GetStagePlayerSpawn(Stage, i == 1);
            var dx = x - p.X;
            var dy = y - p.Y;
            var d = dx * dx + dy * dy;
            if (d < dist) { dist = d; best = i; }
        }
        return dist <= 20 * 20 ? best : -1;
    }

    private RectangleF MapViewport()
    {
        var usable = new RectangleF(8, 6, Math.Max(32, ClientSize.Width - 16), Math.Max(32, ClientSize.Height - 34));
        var size = Math.Min(usable.Width, usable.Height);
        return new RectangleF(usable.Left + (usable.Width - size) / 2f, usable.Top, size, size);
    }
}
