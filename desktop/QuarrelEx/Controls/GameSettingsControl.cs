using QuarrelEx.Core;
using QuarrelEx.Rendering;

namespace QuarrelEx.Controls;

public sealed class GameSettingsControl : UserControl
{
    private readonly NumericUpDown _lives = Num(1,255);
    private readonly ComboBox _initial = new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=140};
    private readonly CheckBox _lock = new(){Text="锁定初始状态（死亡后恢复初始等级；吃星星仍可升级）",AutoSize=true};
    private readonly Dictionary<SpawnKind,(NumericUpDown X,NumericUpDown Y)> _spawns=new();
    private readonly Dictionary<SpawnKind,Label> _spawnLabels=new();
    private readonly SpawnPositionEditorControl _spawnEditor=new(){Width=500,Height=448,Margin=new Padding(0,6,0,6)};
    private readonly Button _clearStage=new(){Text="清空当前关卡",AutoSize=true};
    private readonly Button _clearAll=new(){Text="清空全部关卡",AutoSize=true};
    private readonly Label _note=new(){AutoSize=true,MaximumSize=new Size(560,0),ForeColor=Color.DimGray,Padding=new Padding(0,6,0,8)};
    private BattleCityRom? _rom;
    private NesRenderer? _renderer;
    private bool _refreshing;
    private Func<int>? _stageProvider;

    public event EventHandler? BeforeEdit;
    public event EventHandler? DataChanged;

    public GameSettingsControl()
    {
        Dock=DockStyle.Fill;
        for(int i=0;i<=4;i++)_initial.Items.Add($"Lv{i}");
        var root=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(12)};
        root.Controls.Add(new Label{Text="游戏设置",Font=new Font(Font,FontStyle.Bold),AutoSize=true});
        root.Controls.Add(_note);
        root.Controls.Add(Line("起始命数 (1~255):",_lives));
        root.Controls.Add(Line("初始坦克等级:",_initial));
        root.Controls.Add(_lock);

        var visualBox=new GroupBox
        {
            Text="出生位置可视化（拖拽坦克；坐标输入与画面双向同步）",
            Width=530,Height=486,Padding=new Padding(8),Margin=new Padding(0,10,0,4)
        };
        _spawnEditor.Dock=DockStyle.Fill;
        visualBox.Controls.Add(_spawnEditor);
        root.Controls.Add(visualBox);

        var spawnBox=new GroupBox{Text="精确坐标（0~255）",AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Padding=new Padding(8)};
        var grid=new TableLayoutPanel{ColumnCount=3,RowCount=6,AutoSize=true,CellBorderStyle=TableLayoutPanelCellBorderStyle.Single};
        grid.Controls.Add(new Label{Text="对象",AutoSize=true,Padding=new Padding(5)},0,0);
        grid.Controls.Add(new Label{Text="X",AutoSize=true,Padding=new Padding(5)},1,0);
        grid.Controls.Add(new Label{Text="Y",AutoSize=true,Padding=new Padding(5)},2,0);
        var rows=new[]{(SpawnKind.Enemy1,"敌人1"),(SpawnKind.Enemy2,"敌人2"),(SpawnKind.Enemy3,"敌人3"),(SpawnKind.Player1,"玩家1"),(SpawnKind.Player2,"玩家2")};
        int row=1;
        foreach(var (kind,name) in rows)
        {
            var x=Num(0,255);var y=Num(0,255);_spawns[kind]=(x,y);
            var label=new Label{Text=name,AutoSize=true,Padding=new Padding(5),Cursor=Cursors.Hand};
            label.Click+=(_,_)=>_spawnEditor.SelectedKind=kind;
            _spawnLabels[kind]=label;
            grid.Controls.Add(label,0,row);grid.Controls.Add(x,1,row);grid.Controls.Add(y,2,row);row++;
        }
        spawnBox.Controls.Add(grid);root.Controls.Add(spawnBox);
        var clearFlow=new FlowLayoutPanel{AutoSize=true,Padding=new Padding(0,10,0,0)};clearFlow.Controls.Add(_clearStage);clearFlow.Controls.Add(_clearAll);root.Controls.Add(clearFlow);
        Controls.Add(root);

        _lives.ValueChanged += (_,_)=>Apply(()=>_rom!.StartingLives=(byte)_lives.Value);
        _initial.SelectedIndexChanged += (_,_)=>Apply(()=>{if(_initial.SelectedIndex>=0)_rom!.InitialTankLevel=_initial.SelectedIndex;});
        _lock.CheckedChanged += (_,_)=>Apply(()=>_rom!.SetLockInitialState(_lock.Checked));
        foreach(var pair in _spawns)
        {
            var kind=pair.Key;
            pair.Value.X.ValueChanged+=(_,_)=>Apply(()=>{_rom!.SetSpawn(kind,(byte)pair.Value.X.Value,(byte)pair.Value.Y.Value);_spawnEditor.SelectedKind=kind;_spawnEditor.Invalidate();});
            pair.Value.Y.ValueChanged+=(_,_)=>Apply(()=>{_rom!.SetSpawn(kind,(byte)pair.Value.X.Value,(byte)pair.Value.Y.Value);_spawnEditor.SelectedKind=kind;_spawnEditor.Invalidate();});
        }

        _spawnEditor.BeforeEdit += (_,_) => BeforeEdit?.Invoke(this,EventArgs.Empty);
        _spawnEditor.LivePositionChanged += (_,_) => SyncSpawnInputsFromRom();
        _spawnEditor.DataChanged += (_,_) => DataChanged?.Invoke(this,EventArgs.Empty);
        _spawnEditor.SelectionChanged += (_,_) => RefreshSelectedSpawnLabel();

        _clearStage.Click += (_,_)=>{if(_rom is null||_stageProvider is null)return;if(MessageBox.Show(FindForm(),"确定把当前关卡的13×13地图全部清为空白地形0D？","清空关卡",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;BeforeEdit?.Invoke(this,EventArgs.Empty);_rom.ClearStage(_stageProvider());_spawnEditor.Invalidate();DataChanged?.Invoke(this,EventArgs.Empty);};
        _clearAll.Click += (_,_)=>{if(_rom is null)return;if(MessageBox.Show(FindForm(),"确定清空当前ROM的全部可编辑关卡地图？此操作可使用Ctrl+Z撤销一次。","清空全部关卡",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;BeforeEdit?.Invoke(this,EventArgs.Empty);_rom.ClearAllStages();_spawnEditor.Invalidate();DataChanged?.Invoke(this,EventArgs.Empty);};
    }

    public void Bind(BattleCityRom? rom,NesRenderer? renderer,Func<int>? stageProvider)
    {
        _rom=rom;_renderer=renderer;_stageProvider=stageProvider;
        _spawnEditor.Bind(rom,renderer,stageProvider);
        RefreshValues();
    }

    public void RefreshValues()
    {
        _refreshing=true;
        try
        {
            Enabled=_rom is not null;
            if(_rom is null){_spawnEditor.Invalidate();return;}
            _lives.Value=_rom.StartingLives;
            var max=_rom.IsOriginal?3:4;
            while(_initial.Items.Count>max+1)_initial.Items.RemoveAt(_initial.Items.Count-1);
            while(_initial.Items.Count<max+1)_initial.Items.Add($"Lv{_initial.Items.Count}");
            _initial.SelectedIndex=Math.Clamp(_rom.InitialTankLevel,0,max);
            _lock.Enabled=_rom.SupportsLockInitialState;_lock.Checked=_rom.LockInitialState;
            _note.Text=_rom.SupportsLockInitialState
                ? "锁定只决定死亡后的复活基准等级；本条命中吃星星/手枪仍按正常规则升级。出生点可直接拖拽，或在下方输入精确 X/Y。"
                : "当前ROM没有锁定初始状态运行支持；初始等级、命数和出生位置仍可编辑。";
            SyncSpawnInputsFromRomCore();
            _spawnEditor.Invalidate();
            RefreshSelectedSpawnLabel();
        }
        finally{_refreshing=false;}
    }

    private void SyncSpawnInputsFromRom()
    {
        if(_rom is null)return;
        var old=_refreshing;_refreshing=true;
        try{SyncSpawnInputsFromRomCore();RefreshSelectedSpawnLabel();}
        finally{_refreshing=old;}
    }

    private void SyncSpawnInputsFromRomCore()
    {
        if(_rom is null)return;
        foreach(var p in _spawns)
        {
            var v=_rom.GetSpawn(p.Key);
            p.Value.X.Value=v.X;p.Value.Y.Value=v.Y;
        }
    }

    private void RefreshSelectedSpawnLabel()
    {
        foreach(var p in _spawnLabels)
        {
            p.Value.Font = new Font(Font, p.Key==_spawnEditor.SelectedKind ? FontStyle.Bold : FontStyle.Regular);
            p.Value.ForeColor = p.Key==_spawnEditor.SelectedKind ? Color.DarkGoldenrod : SystemColors.ControlText;
        }
    }

    private void Apply(Action action)
    {
        if(_refreshing||_rom is null)return;
        try{BeforeEdit?.Invoke(this,EventArgs.Empty);action();DataChanged?.Invoke(this,EventArgs.Empty);}
        catch(Exception ex){MessageBox.Show(FindForm(),ex.Message,"设置失败",MessageBoxButtons.OK,MessageBoxIcon.Error);RefreshValues();}
    }

    private static NumericUpDown Num(int min,int max)=>new(){Minimum=min,Maximum=max,Width=90,Hexadecimal=false};
    private static Control Line(string text,Control control){var p=new FlowLayoutPanel{AutoSize=true};p.Controls.Add(new Label{Text=text,AutoSize=true,Padding=new Padding(0,6,8,0)});p.Controls.Add(control);return p;}
}
