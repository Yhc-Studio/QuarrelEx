using QuarrelEx.Core;
using QuarrelEx.Rendering;
using QuarrelEx.Localization;

namespace QuarrelEx.Controls;

public sealed class GameSettingsControl : UserControl
{
    private readonly NumericUpDown _lives = Num(1,255);
    private readonly ComboBox _initial = new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=140};
    private readonly CheckBox _lock = new(){Text="锁定初始状态（死亡后恢复初始等级；吃星星仍可升级）",AutoSize=true};
    private readonly Dictionary<SpawnKind,(NumericUpDown X,NumericUpDown Y)> _spawns=new();
    private readonly Dictionary<SpawnKind,Label> _spawnLabels=new();
    private readonly SpawnPositionEditorControl _spawnEditor=new(){Width=500,Height=448,Margin=new Padding(0,6,0,6)};

    // Runtime 6.9.3 / QXR1 v5 per-stage P1/P2 player spawn editor.
    private readonly StagePlayerSpawnEditorControl _stagePlayerSpawnEditor=new(){Width=500,Height=500,Margin=new Padding(0,6,0,6)};
    private readonly GroupBox _stagePlayerSpawnBox=new(){Text="Stage 1~70 玩家出生点（Runtime 6.9.3 / QXR1 v5）",AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Padding=new Padding(8),Margin=new Padding(0,12,0,4)};
    private readonly ComboBox _stagePlayerMode1=ModeCombo();
    private readonly ComboBox _stagePlayerMode2=ModeCombo();
    private readonly NumericUpDown _stagePlayerX1=GridNum();
    private readonly NumericUpDown _stagePlayerY1=GridNum();
    private readonly NumericUpDown _stagePlayerX2=GridNum();
    private readonly NumericUpDown _stagePlayerY2=GridNum();
    private readonly Label _stagePlayerSpawnNote=new(){AutoSize=true,MaximumSize=new Size(590,0),ForeColor=Color.DimGray};

    // Runtime 6.5+ / QXR1 v2+ per-stage custom enemy spawn editor.
    private readonly CustomEnemySpawnEditorControl _customSpawnEditor=new(){Width=500,Height=500,Margin=new Padding(0,6,0,6)};
    private readonly ComboBox _customCount1 = CountCombo();
    private readonly ComboBox _customCount2 = CountCombo();
    private readonly CheckBox _customSnap = new(){Text="16px 网格吸附",Checked=true,AutoSize=true};
    private readonly CheckBox _customShowUnused = new(){Text="显示未使用点",Checked=true,AutoSize=true};
    private readonly Button _customPreset8 = new(){Text="当前关设为 8 / 8",AutoSize=true};
    private readonly Button _customOriginal = new(){Text="恢复 Original",AutoSize=true};
    private readonly GroupBox _customSpawnBox = new(){Text="Stage 1~70 自定义敌人出生点（Runtime 6.5+ Final Rules）",AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Padding=new Padding(8),Margin=new Padding(0,12,0,4)};
    private readonly (NumericUpDown X, NumericUpDown Y, Label Usage, Label Terrain)[] _customPoints = new (NumericUpDown, NumericUpDown, Label, Label)[8];
    private readonly Label _customSpawnNote=new(){AutoSize=true,MaximumSize=new Size(590,0),ForeColor=Color.DimGray};

    // Runtime 6.5/6.6 global final rules.
    private readonly GroupBox _finalRulesBox = new(){Text="Final Rules 全局规则（QXR1 v2~v5）",AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Padding=new Padding(8),Margin=new Padding(0,12,0,4)};
    private readonly CheckBox _skipGameOver = new(){Text="Skip Final GAME OVER（默认 OFF；Hi-Score 保留）",AutoSize=true};
    private readonly ComboBox _bonus2P = new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=330};
    private readonly ComboBox _armorMode = new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=330};
    private readonly ComboBox _lifeMode = new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=320};
    private readonly NumericUpDown _lifeValue = Num(1,99);
    private readonly NumericUpDown _cheatLives1 = Num(1,99);
    private readonly NumericUpDown _cheatLives2 = Num(1,99);
    private readonly Label _cheatNote = new(){AutoSize=true,MaximumSize=new Size(590,0),ForeColor=Color.DimGray};

    // Runtime 6.6 / QXR1 v3 per-stage enemy appearance pacing.
    private readonly GroupBox _pacingBox = new(){Text="Stage 1~70 敌人出现节奏（Runtime 6.6）",AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Padding=new Padding(8),Margin=new Padding(0,12,0,4)};
    private readonly NumericUpDown _pacing1Interval = Num(1,255);
    private readonly NumericUpDown _pacing2Interval = Num(1,255);
    private readonly NumericUpDown _pacing1Active = Num(1,6);
    private readonly NumericUpDown _pacing2Active = Num(1,6);
    private readonly Button _pacingStage35 = new(){Text="复制 Stage 35 节奏",AutoSize=true};
    private readonly Button _pacingOriginal = new(){Text="恢复本关原版节奏",AutoSize=true};
    private readonly Label _pacingNote = new(){AutoSize=true,MaximumSize=new Size(590,0),ForeColor=Color.DimGray};

    private readonly Button _clearStage=new(){Text="清空当前关卡",AutoSize=true};
    private readonly Button _clearAll=new(){Text="清空全部关卡",AutoSize=true};
    private readonly Label _note=new(){AutoSize=true,MaximumSize=new Size(600,0),ForeColor=Color.DimGray,Padding=new Padding(0,6,0,8)};
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
        _bonus2P.Items.AddRange(["Original：本关击杀较多者 +1000", "Win Streak：连续胜场递增奖励"]);
        _armorMode.Items.AddRange(["Original：装甲坦克原版 4 发耐久", "One Hit：普通400分装甲=白色1HP；闪光奖励装甲仍为原版4HP"]);
        _lifeMode.Items.AddRange(["Original：20,000 分仅 +1 次", "Custom Once：自定义门槛仅 +1 次", "Repeat：固定分数间隔反复 +1", "Disabled：关闭分数加命"]);

        var root=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(12)};
        root.Controls.Add(new Label{Text="游戏设置",Font=new Font(Font,FontStyle.Bold),AutoSize=true});
        root.Controls.Add(_note);
        root.Controls.Add(Line("起始命数 (1~255):",_lives));
        root.Controls.Add(Line("初始坦克等级:",_initial));
        root.Controls.Add(_lock);

        var visualBox=new GroupBox
        {
            Text="原版全局出现位置（拖拽坦克；坐标输入与画面双向同步）",
            Width=560,Height=486,Padding=new Padding(8),Margin=new Padding(0,10,0,4)
        };
        _spawnEditor.Dock=DockStyle.Fill;
        visualBox.Controls.Add(_spawnEditor);
        root.Controls.Add(visualBox);

        var spawnBox=new GroupBox{Text="原版全局出现位置精确坐标（0~255）",AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Padding=new Padding(8)};
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

        BuildStagePlayerSpawnBox();
        root.Controls.Add(_stagePlayerSpawnBox);
        BuildCustomSpawnBox();
        root.Controls.Add(_customSpawnBox);
        BuildPacingBox();
        root.Controls.Add(_pacingBox);
        BuildFinalRulesBox();
        root.Controls.Add(_finalRulesBox);

        var clearFlow=new FlowLayoutPanel{AutoSize=true,Padding=new Padding(0,10,0,0)};clearFlow.Controls.Add(_clearStage);clearFlow.Controls.Add(_clearAll);root.Controls.Add(clearFlow);
        Controls.Add(root);

        _lives.ValueChanged += (_,_)=>Apply(()=>_rom!.StartingLives=(byte)_lives.Value);
        _initial.SelectedIndexChanged += (_,_)=>Apply(()=>{if(_initial.SelectedIndex>=0)_rom!.InitialTankLevel=_initial.SelectedIndex;});
        _lock.CheckedChanged += (_,_)=>Apply(()=>_rom!.SetLockInitialState(_lock.Checked));
        foreach(var pair in _spawns)
        {
            var kind=pair.Key;
            pair.Value.X.ValueChanged+=(_,_)=>Apply(()=>{_rom!.SetSpawn(kind,(byte)pair.Value.X.Value,(byte)pair.Value.Y.Value);_spawnEditor.SelectedKind=kind;_spawnEditor.Invalidate();_stagePlayerSpawnEditor.Invalidate();});
            pair.Value.Y.ValueChanged+=(_,_)=>Apply(()=>{_rom!.SetSpawn(kind,(byte)pair.Value.X.Value,(byte)pair.Value.Y.Value);_spawnEditor.SelectedKind=kind;_spawnEditor.Invalidate();_stagePlayerSpawnEditor.Invalidate();});
        }

        _spawnEditor.BeforeEdit += (_,_) => BeforeEdit?.Invoke(this,EventArgs.Empty);
        _spawnEditor.LivePositionChanged += (_,_) => { SyncSpawnInputsFromRom(); _stagePlayerSpawnEditor.Invalidate(); };
        _spawnEditor.DataChanged += (_,_) => DataChanged?.Invoke(this,EventArgs.Empty);
        _spawnEditor.SelectionChanged += (_,_) => RefreshSelectedSpawnLabel();

        WireStagePlayerSpawnEvents();
        WireCustomSpawnEvents();
        WirePacingEvents();
        WireFinalRuleEvents();

        _clearStage.Click += (_,_)=>{if(_rom is null||_stageProvider is null)return;if(MessageBox.Show(FindForm(),I18n.T("dialog.clear_stage.message"),I18n.T("dialog.clear_stage.title"),MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;BeforeEdit?.Invoke(this,EventArgs.Empty);_rom.ClearStage(_stageProvider());RefreshMapVisuals();DataChanged?.Invoke(this,EventArgs.Empty);};
        _clearAll.Click += (_,_)=>{if(_rom is null)return;if(MessageBox.Show(FindForm(),I18n.T("dialog.clear_all.message"),I18n.T("dialog.clear_all.title"),MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;BeforeEdit?.Invoke(this,EventArgs.Empty);_rom.ClearAllStages();RefreshMapVisuals();DataChanged?.Invoke(this,EventArgs.Empty);};
    }

    private void BuildStagePlayerSpawnBox()
    {
        var flow=new FlowLayoutPanel{FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true,Padding=new Padding(2)};
        _stagePlayerSpawnNote.Text="每关 P1 / P2 可独立选择 Original（使用上方全局玩家出生点）或 Custom。Custom 使用 $18~$D8 的 16px 网格。";
        flow.Controls.Add(_stagePlayerSpawnNote);
        var modes=new TableLayoutPanel{ColumnCount=4,RowCount=3,AutoSize=true,CellBorderStyle=TableLayoutPanelCellBorderStyle.Single};
        foreach(var (text,col) in new[]{("玩家",0),("模式",1),("X",2),("Y",3)})modes.Controls.Add(new Label{Text=text,AutoSize=true,Padding=new Padding(5)},col,0);
        modes.Controls.Add(new Label{Text="P1",AutoSize=true,Padding=new Padding(5)},0,1);modes.Controls.Add(_stagePlayerMode1,1,1);modes.Controls.Add(_stagePlayerX1,2,1);modes.Controls.Add(_stagePlayerY1,3,1);
        modes.Controls.Add(new Label{Text="P2",AutoSize=true,Padding=new Padding(5)},0,2);modes.Controls.Add(_stagePlayerMode2,1,2);modes.Controls.Add(_stagePlayerX2,2,2);modes.Controls.Add(_stagePlayerY2,3,2);
        flow.Controls.Add(modes);
        _stagePlayerSpawnEditor.Width=540;_stagePlayerSpawnEditor.Height=520;flow.Controls.Add(_stagePlayerSpawnEditor);
        _stagePlayerSpawnBox.Controls.Add(flow);
    }

    private void BuildCustomSpawnBox()
    {
        var flow=new FlowLayoutPanel{FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true,Padding=new Padding(2)};
        _customSpawnNote.Text="1P/2P 均可选择 Original 或 1~8。两种模式共用 S1~S8 坐标，但 Count 独立；安全中心坐标固定为 $18~$D8。";
        flow.Controls.Add(_customSpawnNote);
        var modes=new FlowLayoutPanel{AutoSize=true};
        modes.Controls.Add(Line("1P 数量:",_customCount1));
        modes.Controls.Add(Line("2P 数量:",_customCount2));
        modes.Controls.Add(_customSnap); modes.Controls.Add(_customShowUnused);
        flow.Controls.Add(modes);
        _customSpawnEditor.Width=540;_customSpawnEditor.Height=520;
        flow.Controls.Add(_customSpawnEditor);
        var buttons=new FlowLayoutPanel{AutoSize=true};buttons.Controls.Add(_customPreset8);buttons.Controls.Add(_customOriginal);flow.Controls.Add(buttons);

        var table=new TableLayoutPanel{ColumnCount=5,RowCount=9,AutoSize=true,CellBorderStyle=TableLayoutPanelCellBorderStyle.Single};
        foreach(var (text,col) in new[]{("点",0),("X",1),("Y",2),("状态",3),("地形",4)})
            table.Controls.Add(new Label{Text=text,AutoSize=true,Padding=new Padding(5)},col,0);
        for(var i=0;i<8;i++)
        {
            var x=Num(BattleCityRom.CustomEnemySpawnMin,BattleCityRom.CustomEnemySpawnMax);x.Increment=16;
            var y=Num(BattleCityRom.CustomEnemySpawnMin,BattleCityRom.CustomEnemySpawnMax);y.Increment=16;
            var usage=new Label{AutoSize=true,Padding=new Padding(5)};
            var terrain=new Label{AutoSize=true,Padding=new Padding(5)};
            _customPoints[i]=(x,y,usage,terrain);
            var index=i;
            var label=new Label{Text=$"S{i+1}",AutoSize=true,Padding=new Padding(5),Cursor=Cursors.Hand};
            label.Click+=(_,_)=>_customSpawnEditor.SelectedIndex=index;
            table.Controls.Add(label,0,i+1);table.Controls.Add(x,1,i+1);table.Controls.Add(y,2,i+1);table.Controls.Add(usage,3,i+1);table.Controls.Add(terrain,4,i+1);
        }
        flow.Controls.Add(table);
        _customSpawnBox.Controls.Add(flow);
    }

    private void BuildPacingBox()
    {
        var flow=new FlowLayoutPanel{FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true,Padding=new Padding(2)};
        _pacingNote.Text="每关分别设置 1P / 2P。出现间隔单位为帧，数值越小越快；最大同时在场敌人数范围 1~6。Stage 35 原版参考：1P=50 帧、2P=30 帧。";
        flow.Controls.Add(_pacingNote);
        var grid=new TableLayoutPanel{ColumnCount=3,RowCount=3,AutoSize=true,CellBorderStyle=TableLayoutPanelCellBorderStyle.Single};
        grid.Controls.Add(new Label{Text="模式",AutoSize=true,Padding=new Padding(5)},0,0);
        grid.Controls.Add(new Label{Text="出现间隔（帧）",AutoSize=true,Padding=new Padding(5)},1,0);
        grid.Controls.Add(new Label{Text="最大同时在场",AutoSize=true,Padding=new Padding(5)},2,0);
        grid.Controls.Add(new Label{Text="1P",AutoSize=true,Padding=new Padding(5)},0,1);
        grid.Controls.Add(_pacing1Interval,1,1);grid.Controls.Add(_pacing1Active,2,1);
        grid.Controls.Add(new Label{Text="2P",AutoSize=true,Padding=new Padding(5)},0,2);
        grid.Controls.Add(_pacing2Interval,1,2);grid.Controls.Add(_pacing2Active,2,2);
        flow.Controls.Add(grid);
        var buttons=new FlowLayoutPanel{AutoSize=true};buttons.Controls.Add(_pacingStage35);buttons.Controls.Add(_pacingOriginal);flow.Controls.Add(buttons);
        _pacingBox.Controls.Add(flow);
    }

    private void BuildFinalRulesBox()
    {
        var flow=new FlowLayoutPanel{FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true,Padding=new Padding(2)};
        flow.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(590,0),ForeColor=Color.DimGray,Text="QXR1 v2~v5 Final Rules；秘籍命数与敌人节奏仅 v3 / Runtime 6.6 可用。Skip 默认 OFF；Win Streak：1胜1000、2胜2000、3胜4000、4胜6000、5胜8000、6胜10000、7胜及以上+1命；平局中断。"});
        flow.Controls.Add(_skipGameOver);
        flow.Controls.Add(Line("2P 通关奖励:",_bonus2P));
        flow.Controls.Add(Line("400分装甲坦克耐久:",_armorMode));
        var life=new FlowLayoutPanel{AutoSize=true};life.Controls.Add(Line("加命规则:",_lifeMode));life.Controls.Add(Line("数值 ×10,000:",_lifeValue));flow.Controls.Add(life);
        _cheatNote.Text="Runtime 6.6：标题画面按住 A+B 再按 Start 启动秘籍命数。默认 1P=10、2P=10；普通起始命数不受影响。";
        flow.Controls.Add(_cheatNote);
        var cheat=new FlowLayoutPanel{AutoSize=true};cheat.Controls.Add(Line("秘籍 1P 命数:",_cheatLives1));cheat.Controls.Add(Line("秘籍 2P 命数:",_cheatLives2));flow.Controls.Add(cheat);
        _finalRulesBox.Controls.Add(flow);
    }

    private void WireStagePlayerSpawnEvents()
    {
        _stagePlayerMode1.SelectedIndexChanged+=(_,_)=>ApplyStagePlayer(()=>ApplyStagePlayerMode(false,_stagePlayerMode1.SelectedIndex));
        _stagePlayerMode2.SelectedIndexChanged+=(_,_)=>ApplyStagePlayer(()=>ApplyStagePlayerMode(true,_stagePlayerMode2.SelectedIndex));
        _stagePlayerX1.ValueChanged+=(_,_)=>ApplyStagePlayer(()=>_rom!.SetStagePlayerSpawn(CurrentStage,false,(int)_stagePlayerX1.Value,(int)_stagePlayerY1.Value));
        _stagePlayerY1.ValueChanged+=(_,_)=>ApplyStagePlayer(()=>_rom!.SetStagePlayerSpawn(CurrentStage,false,(int)_stagePlayerX1.Value,(int)_stagePlayerY1.Value));
        _stagePlayerX2.ValueChanged+=(_,_)=>ApplyStagePlayer(()=>_rom!.SetStagePlayerSpawn(CurrentStage,true,(int)_stagePlayerX2.Value,(int)_stagePlayerY2.Value));
        _stagePlayerY2.ValueChanged+=(_,_)=>ApplyStagePlayer(()=>_rom!.SetStagePlayerSpawn(CurrentStage,true,(int)_stagePlayerX2.Value,(int)_stagePlayerY2.Value));
        _stagePlayerSpawnEditor.BeforeEdit+=(_,_)=>BeforeEdit?.Invoke(this,EventArgs.Empty);
        _stagePlayerSpawnEditor.LivePositionChanged+=(_,_)=>SyncStagePlayerSpawnInputsFromRom();
        _stagePlayerSpawnEditor.DataChanged+=(_,_)=>DataChanged?.Invoke(this,EventArgs.Empty);
        _stagePlayerSpawnEditor.SelectionChanged+=(_,_)=>SyncStagePlayerSpawnInputsFromRom();
    }

    private void ApplyStagePlayerMode(bool twoPlayer,int mode)
    {
        if(_rom is null)return;
        if(mode<=0){_rom.SetStagePlayerSpawnOriginal(CurrentStage,twoPlayer);return;}
        var p=_rom.GetStagePlayerSpawn(CurrentStage,twoPlayer);
        if(!p.IsCustom)_rom.SetStagePlayerSpawn(CurrentStage,twoPlayer,p.X,p.Y);
    }

    private void WireCustomSpawnEvents()
    {
        _customCount1.SelectedIndexChanged+=(_,_)=>ApplyCustom(()=>_rom!.SetCustomEnemySpawnCount(CurrentStage,false,_customCount1.SelectedIndex));
        _customCount2.SelectedIndexChanged+=(_,_)=>ApplyCustom(()=>_rom!.SetCustomEnemySpawnCount(CurrentStage,true,_customCount2.SelectedIndex));
        _customSnap.CheckedChanged+=(_,_)=>{_customSpawnEditor.SnapToGrid=_customSnap.Checked;_customSpawnEditor.Invalidate();};
        _customShowUnused.CheckedChanged+=(_,_)=>{_customSpawnEditor.ShowUnused=_customShowUnused.Checked;_customSpawnEditor.Invalidate();};
        _customPreset8.Click+=(_,_)=>ApplyCustom(()=>_rom!.SetDefaultEightCustomEnemySpawns(CurrentStage));
        _customOriginal.Click+=(_,_)=>ApplyCustom(()=>{_rom!.SetCustomEnemySpawnCount(CurrentStage,false,0);_rom.SetCustomEnemySpawnCount(CurrentStage,true,0);});
        for(var i=0;i<8;i++)
        {
            var index=i;var tuple=_customPoints[i];
            tuple.X.ValueChanged+=(_,_)=>ApplyCustom(()=>{var p=_rom!.GetCustomEnemySpawnPoint(CurrentStage,index);_rom.SetCustomEnemySpawnPoint(CurrentStage,index,(int)tuple.X.Value,p.Y);_customSpawnEditor.SelectedIndex=index;});
            tuple.Y.ValueChanged+=(_,_)=>ApplyCustom(()=>{var p=_rom!.GetCustomEnemySpawnPoint(CurrentStage,index);_rom.SetCustomEnemySpawnPoint(CurrentStage,index,p.X,(int)tuple.Y.Value);_customSpawnEditor.SelectedIndex=index;});
        }
        _customSpawnEditor.BeforeEdit+=(_,_)=>BeforeEdit?.Invoke(this,EventArgs.Empty);
        _customSpawnEditor.LivePositionChanged+=(_,_)=>SyncCustomSpawnInputsFromRom();
        _customSpawnEditor.DataChanged+=(_,_)=>DataChanged?.Invoke(this,EventArgs.Empty);
        _customSpawnEditor.SelectionChanged+=(_,_)=>SyncCustomSpawnInputsFromRom();
    }

    private void WirePacingEvents()
    {
        _pacing1Interval.ValueChanged+=(_,_)=>ApplyPacing(()=>_rom!.SetEnemySpawnInterval(CurrentStage,false,(int)_pacing1Interval.Value));
        _pacing2Interval.ValueChanged+=(_,_)=>ApplyPacing(()=>_rom!.SetEnemySpawnInterval(CurrentStage,true,(int)_pacing2Interval.Value));
        _pacing1Active.ValueChanged+=(_,_)=>ApplyPacing(()=>_rom!.SetMaxActiveEnemies(CurrentStage,false,(int)_pacing1Active.Value));
        _pacing2Active.ValueChanged+=(_,_)=>ApplyPacing(()=>_rom!.SetMaxActiveEnemies(CurrentStage,true,(int)_pacing2Active.Value));
        _pacingStage35.Click+=(_,_)=>ApplyPacing(()=>_rom!.SetStage35EnemyPacingPreset(CurrentStage));
        _pacingOriginal.Click+=(_,_)=>ApplyPacing(()=>_rom!.SetOriginalEnemyPacingPreset(CurrentStage));
    }

    private void WireFinalRuleEvents()
    {
        _skipGameOver.CheckedChanged+=(_,_)=>ApplyFinal(()=>_rom!.SkipFinalGameOver=_skipGameOver.Checked);
        _bonus2P.SelectedIndexChanged+=(_,_)=>ApplyFinal(()=>_rom!.TwoPlayerBonusMode=Math.Max(0,_bonus2P.SelectedIndex));
        _armorMode.SelectedIndexChanged+=(_,_)=>ApplyFinal(()=>_rom!.ArmoredTankMode=Math.Max(0,_armorMode.SelectedIndex));
        _lifeMode.SelectedIndexChanged+=(_,_)=>ApplyFinal(()=>_rom!.ExtraLifeMode=Math.Max(0,_lifeMode.SelectedIndex));
        _lifeValue.ValueChanged+=(_,_)=>ApplyFinal(()=>_rom!.ExtraLifeValue=(int)_lifeValue.Value);
        _cheatLives1.ValueChanged+=(_,_)=>ApplyFinalV3(()=>_rom!.CheatPlayer1Lives=(int)_cheatLives1.Value);
        _cheatLives2.ValueChanged+=(_,_)=>ApplyFinalV3(()=>_rom!.CheatPlayer2Lives=(int)_cheatLives2.Value);
    }

    private int CurrentStage => _stageProvider?.Invoke() ?? 1;
    private bool StagePlayerAvailable => _rom?.SupportsFinalRulesV5==true && CurrentStage is >=1 and <=70;
    private bool CustomAvailable => _rom?.HasFinalRules==true && CurrentStage is >=1 and <=70;
    private bool PacingAvailable => _rom?.SupportsFinalRulesV3==true && CurrentStage is >=1 and <=70;

    public void Bind(BattleCityRom? rom,NesRenderer? renderer,Func<int>? stageProvider)
    {
        _rom=rom;_renderer=renderer;_stageProvider=stageProvider;
        _spawnEditor.Bind(rom,renderer,stageProvider);
        _stagePlayerSpawnEditor.Bind(rom,renderer,stageProvider);
        _customSpawnEditor.Bind(rom,renderer,stageProvider);
        RefreshValues();
    }

    public void RefreshValues()
    {
        _refreshing=true;
        try
        {
            Enabled=_rom is not null;
            if(_rom is null){_spawnEditor.Invalidate();_stagePlayerSpawnEditor.Invalidate();_customSpawnEditor.Invalidate();return;}
            _lives.Value=_rom.StartingLives;
            var max=_rom.IsOriginal?3:4;
            while(_initial.Items.Count>max+1)_initial.Items.RemoveAt(_initial.Items.Count-1);
            while(_initial.Items.Count<max+1)_initial.Items.Add($"Lv{_initial.Items.Count}");
            _initial.SelectedIndex=Math.Clamp(_rom.InitialTankLevel,0,max);
            _lock.Enabled=_rom.SupportsLockInitialState;_lock.Checked=_rom.LockInitialState;
            _note.Text=_rom.SupportsLockInitialState
                ? "锁定只决定死亡后的复活基准等级；本条命中吃星星/手枪仍按正常规则升级。原版全局出生点仍可直接拖拽。"
                : "当前ROM没有锁定初始状态运行支持；初始等级、命数和原版全局出生位置仍可编辑。";
            SyncSpawnInputsFromRomCore();
            _spawnEditor.Invalidate();
            RefreshSelectedSpawnLabel();
            RefreshStagePlayerSpawnCore();
            RefreshCustomSpawnCore();
            RefreshPacingCore();
            RefreshFinalRulesCore();
        }
        finally{_refreshing=false; I18n.TranslateControlTree(this);}
    }

    private void RefreshStagePlayerSpawnCore()
    {
        var supported=_rom?.SupportsFinalRulesV5==true;
        var available=supported&&CurrentStage is >=1 and <=70;
        _stagePlayerSpawnBox.Enabled=available;
        _stagePlayerSpawnEditor.Bind(_rom,_renderer,_stageProvider);
        if(!supported)
        {
            _stagePlayerSpawnNote.Text="逐关 P1/P2 玩家出生点需要 QXR1 v5 / Runtime 6.9.3。";
            return;
        }
        if(!available)
        {
            _stagePlayerSpawnNote.Text="Demo 不使用 Stage 1~70 独立玩家出生点。";
            return;
        }
        var p1=_rom!.GetStagePlayerSpawn(CurrentStage,false);
        var p2=_rom.GetStagePlayerSpawn(CurrentStage,true);
        _stagePlayerMode1.SelectedIndex=p1.IsCustom?1:0;_stagePlayerMode2.SelectedIndex=p2.IsCustom?1:0;
        _stagePlayerX1.Value=p1.X;_stagePlayerY1.Value=p1.Y;_stagePlayerX2.Value=p2.X;_stagePlayerY2.Value=p2.Y;
        _stagePlayerX1.Enabled=_stagePlayerY1.Enabled=p1.IsCustom;_stagePlayerX2.Enabled=_stagePlayerY2.Enabled=p2.IsCustom;
        var c1=_rom.GetStagePlayerSpawnCell(CurrentStage,false);var c2=_rom.GetStagePlayerSpawnCell(CurrentStage,true);
        _stagePlayerSpawnNote.Text=I18n.T("settings.player_stage.status", CurrentStage, p1.IsCustom?"Custom":"Original", p1.X.ToString("X2"), p1.Y.ToString("X2"), c1.TerrainId.ToString("X2"), p2.IsCustom?"Custom":"Original", p2.X.ToString("X2"), p2.Y.ToString("X2"), c2.TerrainId.ToString("X2"));
        _stagePlayerSpawnEditor.Invalidate();
    }

    private void RefreshCustomSpawnCore()
    {
        var supported=_rom?.HasFinalRules==true;
        var available=supported&&CurrentStage is >=1 and <=70;
        _customSpawnBox.Enabled=available;
        _customSpawnEditor.Bind(_rom,_renderer,_stageProvider);
        _customSpawnEditor.SnapToGrid=_customSnap.Checked;
        _customSpawnEditor.ShowUnused=_customShowUnused.Checked;
        if(!supported)
        {
            _customSpawnNote.Text="当前 ROM 未检测到 QXR1 Final Rules；请使用 32KB Runtime 6.5~6.9 IPS。";
            return;
        }
        if(!available)
        {
            _customSpawnNote.Text="Demo 不使用 Stage 1~70 自定义敌人出生点。";
            return;
        }
        _customSpawnNote.Text="1P/2P 均可选择 Original 或 1~8；共用 S1~S8 坐标、Count 独立。安全中心坐标 $18~$D8；橙色表示落在非 $0D 地形。";
        _customCount1.SelectedIndex=Math.Clamp(_rom!.GetCustomEnemySpawnCount(CurrentStage,false),0,8);
        _customCount2.SelectedIndex=Math.Clamp(_rom.GetCustomEnemySpawnCount(CurrentStage,true),0,8);
        for(var i=0;i<8;i++)
        {
            var p=_rom.GetCustomEnemySpawnPoint(CurrentStage,i);
            var tuple=_customPoints[i];
            tuple.X.Value=p.X;tuple.Y.Value=p.Y;
            var a=_customCount1.SelectedIndex>0&&i<_customCount1.SelectedIndex;
            var b=_customCount2.SelectedIndex>0&&i<_customCount2.SelectedIndex;
            tuple.Usage.Text=a&&b?I18n.T("settings.usage.both"):a?I18n.T("settings.usage.p1"):b?I18n.T("settings.usage.p2"):I18n.T("settings.unused");
            var cell=_rom.GetCustomEnemySpawnCell(CurrentStage,i);
            tuple.Terrain.Text=$"${cell.TerrainId:X2}"+(cell.TerrainId!=0x0D?" ⚠":"");
            tuple.Terrain.ForeColor=cell.TerrainId!=0x0D?Color.DarkOrange:SystemColors.ControlText;
        }
        _customSpawnEditor.Invalidate();
    }

    private void RefreshPacingCore()
    {
        var supported=_rom?.SupportsFinalRulesV3==true;
        var available=supported&&CurrentStage is >=1 and <=70;
        _pacingBox.Enabled=available;
        if(!supported)
        {
            _pacingNote.Text="当前 ROM 未检测到 QXR1 v3 / Runtime 6.6；敌人出现节奏表不可编辑。";
            return;
        }
        if(!available)
        {
            _pacingNote.Text="Demo 不使用 Stage 1~70 独立敌人出现节奏。";
            return;
        }
        _pacingNote.Text="每关分别设置 1P / 2P。出现间隔单位为帧，数值越小越快；最大同时在场敌人数范围 1~6。Stage 35 原版参考：1P=50 帧、2P=30 帧。";
        _pacing1Interval.Value=_rom!.GetEnemySpawnInterval(CurrentStage,false);
        _pacing2Interval.Value=_rom.GetEnemySpawnInterval(CurrentStage,true);
        _pacing1Active.Value=_rom.GetMaxActiveEnemies(CurrentStage,false);
        _pacing2Active.Value=_rom.GetMaxActiveEnemies(CurrentStage,true);
    }

    private void RefreshFinalRulesCore()
    {
        var on=_rom?.HasFinalRules==true;
        _finalRulesBox.Enabled=on;
        if(!on)return;
        _skipGameOver.Checked=_rom!.SkipFinalGameOver;
        _bonus2P.SelectedIndex=Math.Clamp(_rom.TwoPlayerBonusMode,0,1);
        _armorMode.SelectedIndex=Math.Clamp(_rom.ArmoredTankMode,0,1);
        _lifeMode.SelectedIndex=Math.Clamp(_rom.ExtraLifeMode,0,3);
        _lifeValue.Value=Math.Clamp(_rom.ExtraLifeValue,1,99);
        _lifeValue.Enabled=_rom.ExtraLifeMode is 1 or 2;
        var v3=_rom.SupportsFinalRulesV3;
        _cheatLives1.Enabled=v3;_cheatLives2.Enabled=v3;
        _cheatNote.Text=v3
            ? "标题画面按住 A+B 再按 Start：1P/2P 使用下方独立秘籍命数；普通 Start 继续使用上方普通起始命数。"
            : "A+B+Start 独立秘籍命数需要 QXR1 v3 / Runtime 6.6。";
        if(v3){_cheatLives1.Value=_rom.CheatPlayer1Lives;_cheatLives2.Value=_rom.CheatPlayer2Lives;}
    }

    public void RefreshMapVisuals()
    {
        if(_rom is null)return;
        var old=_refreshing;_refreshing=true;
        try
        {
            _spawnEditor.Invalidate();
            RefreshStagePlayerSpawnCore();
            RefreshCustomSpawnCore();
        }
        finally{_refreshing=old;}
    }

    private void SyncSpawnInputsFromRom()
    {
        if(_rom is null)return;
        var old=_refreshing;_refreshing=true;
        try{SyncSpawnInputsFromRomCore();RefreshSelectedSpawnLabel();}
        finally{_refreshing=old;}
    }

    private void SyncStagePlayerSpawnInputsFromRom()
    {
        if(_rom is null)return;
        var old=_refreshing;_refreshing=true;
        try{RefreshStagePlayerSpawnCore();}
        finally{_refreshing=old;}
    }

    private void SyncCustomSpawnInputsFromRom()
    {
        if(_rom is null)return;
        var old=_refreshing;_refreshing=true;
        try{RefreshCustomSpawnCore();}
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
        catch(Exception ex){MessageBox.Show(FindForm(),I18n.FromSource(ex.Message),I18n.T("dialog.setting_failed"),MessageBoxButtons.OK,MessageBoxIcon.Error);RefreshValues();}
    }

    private void ApplyStagePlayer(Action action)
    {
        if(_refreshing||_rom is null||!StagePlayerAvailable)return;
        try{BeforeEdit?.Invoke(this,EventArgs.Empty);action();RefreshStagePlayerSpawnCore();DataChanged?.Invoke(this,EventArgs.Empty);}
        catch(Exception ex){MessageBox.Show(FindForm(),I18n.FromSource(ex.Message),I18n.T("dialog.player_spawn_failed"),MessageBoxButtons.OK,MessageBoxIcon.Error);RefreshValues();}
    }

    private void ApplyCustom(Action action)
    {
        if(_refreshing||_rom is null||!CustomAvailable)return;
        try{BeforeEdit?.Invoke(this,EventArgs.Empty);action();RefreshCustomSpawnCore();DataChanged?.Invoke(this,EventArgs.Empty);}
        catch(Exception ex){MessageBox.Show(FindForm(),I18n.FromSource(ex.Message),I18n.T("dialog.enemy_spawn_failed"),MessageBoxButtons.OK,MessageBoxIcon.Error);RefreshValues();}
    }

    private void ApplyPacing(Action action)
    {
        if(_refreshing||_rom is null||!PacingAvailable)return;
        try{BeforeEdit?.Invoke(this,EventArgs.Empty);action();RefreshPacingCore();DataChanged?.Invoke(this,EventArgs.Empty);}
        catch(Exception ex){MessageBox.Show(FindForm(),I18n.FromSource(ex.Message),I18n.T("dialog.pacing_failed"),MessageBoxButtons.OK,MessageBoxIcon.Error);RefreshValues();}
    }

    private void ApplyFinalV3(Action action)
    {
        if(_refreshing||_rom?.SupportsFinalRulesV3!=true)return;
        try{BeforeEdit?.Invoke(this,EventArgs.Empty);action();RefreshFinalRulesCore();DataChanged?.Invoke(this,EventArgs.Empty);}
        catch(Exception ex){MessageBox.Show(FindForm(),I18n.FromSource(ex.Message),I18n.T("dialog.runtime66_failed"),MessageBoxButtons.OK,MessageBoxIcon.Error);RefreshValues();}
    }

    private void ApplyFinal(Action action)
    {
        if(_refreshing||_rom?.HasFinalRules!=true)return;
        try{BeforeEdit?.Invoke(this,EventArgs.Empty);action();RefreshFinalRulesCore();DataChanged?.Invoke(this,EventArgs.Empty);}
        catch(Exception ex){MessageBox.Show(FindForm(),I18n.FromSource(ex.Message),I18n.T("dialog.rules_failed"),MessageBoxButtons.OK,MessageBoxIcon.Error);RefreshValues();}
    }

    private static ComboBox ModeCombo()
    {
        var c=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=125};
        c.Items.AddRange(["Original / 全局","Custom / 本关"]);
        return c;
    }

    private static NumericUpDown GridNum()=>new(){Minimum=BattleCityRom.CustomEnemySpawnMin,Maximum=BattleCityRom.CustomEnemySpawnMax,Increment=16,Width=90,Hexadecimal=false};

    private static ComboBox CountCombo()
    {
        var c=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=125};
        c.Items.Add("Original (3)");
        for(var i=1;i<=8;i++)c.Items.Add(i.ToString());
        return c;
    }

    private static NumericUpDown Num(int min,int max)=>new(){Minimum=min,Maximum=max,Width=90,Hexadecimal=false};
    private static Control Line(string text,Control control){var p=new FlowLayoutPanel{AutoSize=true};p.Controls.Add(new Label{Text=text,AutoSize=true,Padding=new Padding(0,6,8,0)});p.Controls.Add(control);return p;}
}
