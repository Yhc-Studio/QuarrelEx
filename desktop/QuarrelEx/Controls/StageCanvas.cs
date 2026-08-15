using System.Drawing.Drawing2D;
using QuarrelEx.Core;
using QuarrelEx.Rendering;

namespace QuarrelEx.Controls;

public sealed class StageCanvas : Control
{
    public const int LogicalCellSize = 32;
    private const int GridCells = 13;
    private (int Row, int Col)? _lastPaintCell;
    private int _cellSize = LogicalCellSize;

    public BattleCityRom? Rom { get; set; }
    public NesRenderer? Renderer { get; set; }
    public int Stage { get; set; } = 1;

    /// <summary>
    /// Current physical cell size. Unlike v0.2 this value is fitted to the
    /// available map viewport, so the complete 13x13 map remains visible on
    /// high-DPI displays instead of growing beyond the panel and being clipped.
    /// </summary>
    public int CellSize => _cellSize;

    public event EventHandler<CellPaintEventArgs>? CellPaintRequested;
    public event EventHandler<CellPickEventArgs>? CellPickRequested;

    public StageCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
        Cursor = Cursors.Cross;
        SetStyle(ControlStyles.ResizeRedraw, true);
        SetCellSize(LogicalCellSize);
        DpiChangedAfterParent += (_, _) => Invalidate();
    }

    /// <summary>
    /// Fits the complete 13x13 map into the given viewport. The preferred cell
    /// size still follows monitor DPI, but it is capped by the available width
    /// and height. This gives a crisp larger map when space is available while
    /// guaranteeing that all 169 cells remain visible when DPI is 125/150/200%.
    /// </summary>
    public void FitToViewport(Size viewportSize)
    {
        var usableWidth = Math.Max(GridCells, viewportSize.Width - 8);
        var usableHeight = Math.Max(GridCells, viewportSize.Height - 8);
        var fitCell = Math.Max(1, Math.Min(usableWidth / GridCells, usableHeight / GridCells));
        var preferred = Math.Max(16, (int)Math.Round(LogicalCellSize * Math.Max(96, DeviceDpi) / 96.0));
        SetCellSize(Math.Min(preferred, fitCell));
    }

    private void SetCellSize(int value)
    {
        _cellSize = Math.Max(1, value);
        var size = GridCells * _cellSize + 1;
        Size = new Size(size, size);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Rom is null || Renderer is null) return;

        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.CompositingMode = CompositingMode.SourceCopy;

        var cellSize = CellSize;
        for (var r = 0; r < GridCells; r++)
        for (var c = 0; c < GridCells; c++)
        {
            var id = Rom.GetCell(Stage, r, c);
            var bmp = Renderer.GetBlockBitmap(id, 1);
            var dest = new Rectangle(c * cellSize, r * cellSize, cellSize, cellSize);
            e.Graphics.DrawImage(bmp, dest, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel);
        }

        e.Graphics.CompositingMode = CompositingMode.SourceOver;
        using var pen = new Pen(Color.FromArgb(60, Color.White));
        for (var i = 0; i <= GridCells; i++)
        {
            e.Graphics.DrawLine(pen, i * cellSize, 0, i * cellSize, GridCells * cellSize);
            e.Graphics.DrawLine(pen, 0, i * cellSize, GridCells * cellSize, i * cellSize);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        var cell = ToCell(e.Location);
        if (cell is null) return;

        if (e.Button == MouseButtons.Left)
        {
            _lastPaintCell = cell;
            CellPaintRequested?.Invoke(this, new CellPaintEventArgs(cell.Value.Row, cell.Value.Col, true));
        }
        else if (e.Button == MouseButtons.Right)
        {
            CellPickRequested?.Invoke(this, new CellPickEventArgs(cell.Value.Row, cell.Value.Col));
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if ((e.Button & MouseButtons.Left) == 0) return;
        var cell = ToCell(e.Location);
        if (cell is null || cell == _lastPaintCell) return;
        _lastPaintCell = cell;
        CellPaintRequested?.Invoke(this, new CellPaintEventArgs(cell.Value.Row, cell.Value.Col, false));
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _lastPaintCell = null;
    }

    private (int Row, int Col)? ToCell(Point p)
    {
        var c = p.X / CellSize;
        var r = p.Y / CellSize;
        return r is >= 0 and < GridCells && c is >= 0 and < GridCells ? (r, c) : null;
    }
}

public sealed class CellPaintEventArgs(int row, int col, bool newStroke) : EventArgs
{
    public int Row { get; } = row;
    public int Column { get; } = col;
    public bool NewStroke { get; } = newStroke;
}

public sealed class CellPickEventArgs(int row, int col) : EventArgs
{
    public int Row { get; } = row;
    public int Column { get; } = col;
}
