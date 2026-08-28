using System.Drawing.Drawing2D;
using QuarrelEx.Core;
using QuarrelEx.Rendering;

namespace QuarrelEx.Controls;

public sealed class StageCanvas : Control
{
    public const int LogicalCellSize = 32;
    private const int GridCells = 13;
    private (int Row, int Col)? _lastPaintCell;
    private (int Row, int Col)? _selectionAnchor;
    private (int Row, int Col)? _moveAnchor;
    private Rectangle? _moveSource;
    private Rectangle? _movePreview;
    private int _cellSize = LogicalCellSize;
    private bool _selectionMode;

    public BattleCityRom? Rom { get; set; }
    public NesRenderer? Renderer { get; set; }
    public int Stage { get; set; } = 1;
    public Rectangle? SelectionCells { get; private set; }

    public bool SelectionMode
    {
        get => _selectionMode;
        set
        {
            if (_selectionMode == value) return;
            _selectionMode = value;
            Cursor = value ? Cursors.Default : Cursors.Cross;
            _lastPaintCell = null;
            _selectionAnchor = null;
            _moveAnchor = null;
            _moveSource = null;
            _movePreview = null;
            Invalidate();
        }
    }

    public int CellSize => _cellSize;

    public event EventHandler<CellPaintEventArgs>? CellPaintRequested;
    public event EventHandler<CellPickEventArgs>? CellPickRequested;
    public event EventHandler<MapSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<MapSelectionMoveEventArgs>? SelectionMoveRequested;

    public StageCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
        Cursor = Cursors.Cross;
        TabStop = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        SetCellSize(LogicalCellSize);
        DpiChangedAfterParent += (_, _) => Invalidate();
    }

    public void FitToViewport(Size viewportSize)
    {
        var usableWidth = Math.Max(GridCells, viewportSize.Width - 8);
        var usableHeight = Math.Max(GridCells, viewportSize.Height - 8);
        var fitCell = Math.Max(1, Math.Min(usableWidth / GridCells, usableHeight / GridCells));
        var preferred = Math.Max(16, (int)Math.Round(LogicalCellSize * Math.Max(96, DeviceDpi) / 96.0));
        SetCellSize(Math.Min(preferred, fitCell));
    }

    public void ClearSelection(bool notify = true)
    {
        SelectionCells = null;
        _selectionAnchor = null;
        _moveAnchor = null;
        _moveSource = null;
        _movePreview = null;
        Invalidate();
        if (notify) SelectionChanged?.Invoke(this, new MapSelectionChangedEventArgs(null));
    }

    public void SetSelection(Rectangle? cells, bool notify = true)
    {
        SelectionCells = cells is null ? null : ClampSelection(cells.Value);
        _movePreview = null;
        Invalidate();
        if (notify) SelectionChanged?.Invoke(this, new MapSelectionChangedEventArgs(SelectionCells));
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
        using (var pen = new Pen(Color.FromArgb(60, Color.White)))
        {
            for (var i = 0; i <= GridCells; i++)
            {
                e.Graphics.DrawLine(pen, i * cellSize, 0, i * cellSize, GridCells * cellSize);
                e.Graphics.DrawLine(pen, 0, i * cellSize, GridCells * cellSize, i * cellSize);
            }
        }

        if (SelectionMode && SelectionCells is Rectangle selection)
            DrawSelection(e.Graphics, selection, false);
        if (SelectionMode && _movePreview is Rectangle preview && (!SelectionCells.HasValue || preview != SelectionCells.Value))
            DrawSelection(e.Graphics, preview, true);
    }

    private void DrawSelection(Graphics graphics, Rectangle cells, bool preview)
    {
        var rect = new Rectangle(
            cells.X * CellSize + 1,
            cells.Y * CellSize + 1,
            cells.Width * CellSize - 2,
            cells.Height * CellSize - 2);
        using var fill = new SolidBrush(Color.FromArgb(preview ? 45 : 58, Color.DodgerBlue));
        using var outline = new Pen(preview ? Color.Orange : Color.DeepSkyBlue, Math.Max(2f, DeviceDpi / 96f * 2f));
        if (preview) outline.DashStyle = DashStyle.Dash;
        graphics.FillRectangle(fill, rect);
        graphics.DrawRectangle(outline, rect);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        var cell = ToCell(e.Location);
        if (cell is null) return;

        if (e.Button == MouseButtons.Right)
        {
            CellPickRequested?.Invoke(this, new CellPickEventArgs(cell.Value.Row, cell.Value.Col));
            return;
        }
        if (e.Button != MouseButtons.Left) return;

        if (!SelectionMode)
        {
            _lastPaintCell = cell;
            CellPaintRequested?.Invoke(this, new CellPaintEventArgs(cell.Value.Row, cell.Value.Col, true));
            return;
        }

        if (SelectionCells is Rectangle existing && existing.Contains(cell.Value.Col, cell.Value.Row))
        {
            _moveAnchor = cell;
            _moveSource = existing;
            _movePreview = existing;
        }
        else
        {
            _selectionAnchor = cell;
            SetSelection(new Rectangle(cell.Value.Col, cell.Value.Row, 1, 1));
        }
        Capture = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if ((e.Button & MouseButtons.Left) == 0) return;
        var cell = ToCell(e.Location, clamp: SelectionMode);
        if (cell is null) return;

        if (!SelectionMode)
        {
            if (cell == _lastPaintCell) return;
            _lastPaintCell = cell;
            CellPaintRequested?.Invoke(this, new CellPaintEventArgs(cell.Value.Row, cell.Value.Col, false));
            return;
        }

        if (_moveAnchor is not null && _moveSource is Rectangle source)
        {
            var dx = cell.Value.Col - _moveAnchor.Value.Col;
            var dy = cell.Value.Row - _moveAnchor.Value.Row;
            var x = Math.Clamp(source.X + dx, 0, GridCells - source.Width);
            var y = Math.Clamp(source.Y + dy, 0, GridCells - source.Height);
            _movePreview = new Rectangle(x, y, source.Width, source.Height);
            Invalidate();
            return;
        }

        if (_selectionAnchor is not null)
        {
            SetSelection(RectFromCells(_selectionAnchor.Value, cell.Value));
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _lastPaintCell = null;
        if (!SelectionMode || e.Button != MouseButtons.Left) return;

        if (_moveSource is Rectangle source && _movePreview is Rectangle target && target.Location != source.Location)
        {
            var copy = (ModifierKeys & Keys.Control) != 0;
            SelectionMoveRequested?.Invoke(this, new MapSelectionMoveEventArgs(source, target.Location, copy));
            SetSelection(target);
        }
        else if (_selectionAnchor is not null && SelectionCells is Rectangle selection)
        {
            SelectionChanged?.Invoke(this, new MapSelectionChangedEventArgs(selection));
        }

        _selectionAnchor = null;
        _moveAnchor = null;
        _moveSource = null;
        _movePreview = null;
        Capture = false;
        Invalidate();
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (Capture) return;
        _selectionAnchor = null;
        _moveAnchor = null;
        _moveSource = null;
        _movePreview = null;
    }

    private (int Row, int Col)? ToCell(Point p, bool clamp = false)
    {
        var c = p.X / CellSize;
        var r = p.Y / CellSize;
        if (clamp)
            return (Math.Clamp(r, 0, GridCells - 1), Math.Clamp(c, 0, GridCells - 1));
        return r is >= 0 and < GridCells && c is >= 0 and < GridCells ? (r, c) : null;
    }

    private static Rectangle RectFromCells((int Row, int Col) a, (int Row, int Col) b)
    {
        var left = Math.Min(a.Col, b.Col);
        var top = Math.Min(a.Row, b.Row);
        return new Rectangle(left, top, Math.Abs(a.Col - b.Col) + 1, Math.Abs(a.Row - b.Row) + 1);
    }

    private static Rectangle ClampSelection(Rectangle rect)
    {
        var x = Math.Clamp(rect.X, 0, GridCells - 1);
        var y = Math.Clamp(rect.Y, 0, GridCells - 1);
        var w = Math.Clamp(rect.Width, 1, GridCells - x);
        var h = Math.Clamp(rect.Height, 1, GridCells - y);
        return new Rectangle(x, y, w, h);
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

public sealed class MapSelectionChangedEventArgs(Rectangle? selection) : EventArgs
{
    public Rectangle? Selection { get; } = selection;
}

public sealed class MapSelectionMoveEventArgs(Rectangle source, Point target, bool copy) : EventArgs
{
    public Rectangle Source { get; } = source;
    public Point Target { get; } = target;
    public bool Copy { get; } = copy;
}
