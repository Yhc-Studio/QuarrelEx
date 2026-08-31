using QuarrelEx.Core;
using QuarrelEx.Rendering;
using QuarrelEx.Localization;

namespace QuarrelEx.Controls;

public sealed class FlagTsaEditorControl : UserControl
{
    private readonly TableLayoutPanel _flag = MakeGrid();
    private readonly TableLayoutPanel _fort = MakeGrid();
    private readonly CheckBox _baseExists = new() { AutoSize = true, Text = "当前关卡存在老巢" };
    private readonly Label _baseNote = new() { AutoSize = true, ForeColor = Color.DimGray, MaximumSize = new Size(620, 0) };
    private readonly List<Image> _images = new();
    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private Func<int>? _stageProvider;
    private bool _rebuilding;

    public event EventHandler? BeforeEdit;
    public event EventHandler? DataChanged;

    public FlagTsaEditorControl()
    {
        Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, AutoScroll = true };
        for (var i = 0; i < 4; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { AutoSize = true, Padding = new Padding(8), ForeColor = Color.DimGray,
            Text = "原 Quarrel Flag TSA Editor：每组实际是 6×4 个背景 CHR Tile；ROM 中每行后面的 FF 结束标记由编辑器自动保留。点击任意格选择 CHR Tile。" },0,0);

        var baseFlow = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false, Dock = DockStyle.Fill, Padding = new Padding(6) };
        baseFlow.Controls.Add(_baseExists);
        baseFlow.Controls.Add(_baseNote);
        root.Controls.Add(Wrap("Stage 老巢 / HQ", baseFlow),0,1);
        root.Controls.Add(Wrap("Flag / Eagle TSA", _flag),0,2);
        root.Controls.Add(Wrap("Fort / Base Wall TSA", _fort),0,3);
        Controls.Add(root);

        _baseExists.CheckedChanged += (_, _) =>
        {
            if (_rebuilding || _rom is null || !_rom.SupportsFinalRulesV4) return;
            var stage = _stageProvider?.Invoke() ?? 1;
            if (stage is < 1 or > 70 || _rom.IsDemoStage(stage)) return;
            BeforeEdit?.Invoke(this, EventArgs.Empty);
            _rom.SetStageBaseExists(stage, _baseExists.Checked);
            DataChanged?.Invoke(this, EventArgs.Empty);
            RefreshBaseControl();
        };
    }

    private static TableLayoutPanel MakeGrid()
    {
        var g = new TableLayoutPanel { ColumnCount = 6, RowCount = 4, AutoSize = true, Padding = new Padding(6), CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
        for (var c=0;c<6;c++) g.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,78));
        for (var r=0;r<4;r++) g.RowStyles.Add(new RowStyle(SizeType.Absolute,72));
        return g;
    }
    private static Control Wrap(string title, Control c) { var g=new GroupBox{Text=title,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Padding=new Padding(8)}; g.Controls.Add(c); return g; }

    public void Bind(BattleCityRom? rom, NesRenderer? renderer, Func<int>? stageProvider = null)
    {
        ClearImages(); _rom=rom; _renderer=renderer; _stageProvider=stageProvider; Rebuild();
    }

    public void Rebuild()
    {
        ClearGrid(_flag); ClearGrid(_fort); ClearImages();
        RefreshBaseControl();
        if (_rom is null || _renderer is null) return;
        Build(_flag,false); Build(_fort,true);
    }

    private void RefreshBaseControl()
    {
        _rebuilding = true;
        try
        {
            if (_rom is null)
            {
                _baseExists.Checked = true;
                _baseExists.Enabled = false;
                _baseNote.Text = I18n.T("flag.open_first");
                return;
            }
            var stage = _stageProvider?.Invoke() ?? 1;
            _baseExists.Text = stage is >= 1 and <= 70 ? I18n.T("flag.exists_stage", stage) : I18n.T("flag.hq.exists");
            if (!_rom.SupportsFinalRulesV4)
            {
                _baseExists.Checked = true;
                _baseExists.Enabled = false;
                _baseNote.Text = I18n.T("flag.requires");
                return;
            }
            if (stage is < 1 or > 70 || _rom.IsDemoStage(stage))
            {
                _baseExists.Checked = true;
                _baseExists.Enabled = false;
                _baseNote.Text = I18n.T("flag.demo");
                return;
            }
            _baseExists.Enabled = true;
            _baseExists.Checked = _rom.GetStageBaseExists(stage);
            _baseNote.Text = I18n.T(_baseExists.Checked ? "flag.on_note" : "flag.off_note");
        }
        finally { _rebuilding = false; I18n.TranslateControlTree(this); }
    }

    private void Build(TableLayoutPanel grid, bool fort)
    {
        for (var r=0;r<4;r++) for(var c=0;c<6;c++)
        {
            var rr=r; var cc=c; var tile=_rom!.GetFlagTsaTile(fort,r,c);
            var image=new Bitmap(_renderer!.GetChrTileBitmap(tile,0,4)); _images.Add(image);
            var b=new Button{Dock=DockStyle.Fill,Margin=new Padding(2),Image=image,ImageAlign=ContentAlignment.MiddleLeft,Text=$"${tile:X2}",TextAlign=ContentAlignment.MiddleRight,TextImageRelation=TextImageRelation.ImageBeforeText};
            b.Click += (_,_) =>
            {
                if(_rom is null||_renderer is null)return;
                using var picker=new ChrTilePickerForm(_renderer,0,_rom.GetFlagTsaTile(fort,rr,cc),fort?"Fort TSA":"Flag TSA");
                if(picker.ShowDialog(FindForm())!=DialogResult.OK)return;
                BeforeEdit?.Invoke(this,EventArgs.Empty); _rom.SetFlagTsaTile(fort,rr,cc,picker.SelectedTile); _renderer.InvalidateCache(); DataChanged?.Invoke(this,EventArgs.Empty); Rebuild();
            };
            grid.Controls.Add(b,c,r);
        }
    }

    private static void ClearGrid(TableLayoutPanel g)
    {
        foreach(Control c in g.Controls) if(c is Button b)b.Image=null;
        while(g.Controls.Count>0){var c=g.Controls[0];g.Controls.RemoveAt(0);c.Dispose();}
    }
    private void ClearImages(){foreach(var i in _images)i.Dispose();_images.Clear();}
    protected override void Dispose(bool disposing){if(disposing){ClearGrid(_flag);ClearGrid(_fort);ClearImages();}base.Dispose(disposing);}
}
