using QuarrelEx.Config;

namespace QuarrelEx.Core;

public sealed class BattleCityRom
{
    private byte[] _data;
    private readonly EditorConfig _cfg;
    private BattleCityRomKind _kind;

    // Legacy 32KB overlay helper. Phase 5 accepts extension IDs $10-$1F.
    private static readonly byte[] OverlayHelper =
    [
        201,14,208,43,165,70,201,2,240,35,165,133,201,36,144,3,56,233,35,56,233,1,
        24,105,128,133,18,165,90,133,17,160,0,177,17,201,16,144,6,201,32,176,2,208,2,
        169,14,166,86,164,87,32,11,216,96
    ];

    private static readonly byte[] OriginalDrawSequence = [0xA6,0x56,0xA4,0x57,0x20,0x0B,0xD8];
    private static readonly byte[] OverlayDrawSequence = [0x20,0x00,0xA3,0xEA,0xEA,0xEA,0xEA];
    private static readonly byte[] Map70DrawSequence = [0x20,0x50,0xB2,0xEA,0xEA,0xEA,0xEA];
    private static readonly byte[] OriginalAttrReference = [0xB9,0xBB,0xDA];
    private static readonly byte[] OriginalTsaReference = [0xBD,0xCB,0xDA];
    private static readonly byte[] LegacyExAttrReference = [0xB9,0xF0,0xFE];
    private static readonly byte[] Phase5AttrReference = [0xB9,0xBB,0xDA];
    private static readonly byte[] Phase5TsaReference = [0xBD,0xF0,0xFE];
    private static readonly byte[] Terrain64AttrReference = [0xB9,0x00,0xB4];
    private static readonly byte[] Terrain64TsaReference = [0xBD,0x40,0xB4];
    private static readonly byte[] ExV2Magic = [(byte)'B',(byte)'C',(byte)'E',(byte)'X'];
    private static readonly byte[] FinalRulesMagic = [(byte)'Q',(byte)'X',(byte)'R',(byte)'1'];
    private static readonly byte[] PlayerDeathLevelHook = [0xBD,0x01,0x01,0xCD,0x6B,0xB5,0x90,0x18];
    private const int FinalRulesConfigCpu = 0xB55F;
    private const int FinalRulesFlagsCpu = 0xB564;
    private const int FinalRulesExtraLifeModeCpu = 0xB565;
    private const int FinalRulesExtraLifeValueCpu = 0xB566;
    private const int FinalRulesTwoPlayerBonusCpu = 0xB567;
    private const int FinalRulesArmoredTankCpu = 0xB568;
    private const int FinalRulesCheatP1LivesCpu = 0xB569;
    private const int FinalRulesCheatP2LivesCpu = 0xB56A;
    // QXR1 v6 / Runtime 6.9.4: first raw tank-state value that survives a hit.
    // Death Lv0/Lv1/Lv2/Lv3/Lv4 => $20/$40/$60/$63/$64.
    private const int FinalRulesPlayerDeathCutoffCpu = 0xB56B;
    private const int FinalRulesSpawnStartCpu = 0xB570;
    private const int FinalRulesSpawnRecordSize = 18;
    private const int FinalRulesNormalStartingLivesCpu = 0xBBCE;
    private const int FinalRulesPacing1PIntervalCpu = 0xBE60;
    private const int FinalRulesPacing2PIntervalCpu = 0xBEA6;
    // QXR1 v3/v4 legacy tables.
    private const int FinalRulesPacing1PMaxActiveLegacyCpu = 0xBEEC;
    private const int FinalRulesPacing2PMaxActiveLegacyCpu = 0xBF32;
    private const int FinalRulesBaseExistsLegacyCpu = 0xBF78;
    // QXR1 v5 / Runtime 6.9 packed stage rules + per-stage player spawns.
    // PackedStageRules bits: 7-5=1P enemy-limit (MaxActive+1), bit4=BaseExists,
    // bit3=EnemyCounter numeric preference, bits2-0=2P enemy-limit (MaxActive+1).
    private const int FinalRulesPackedStageRulesCpu = 0xBEEC;
    private const int FinalRulesStageP1SpawnCpu = 0xBF32;
    private const int FinalRulesStageP2SpawnCpu = 0xBF78;
    public const int CustomEnemySpawnMin = 0x18;
    public const int CustomEnemySpawnMax = 0xD8;
    public const int CustomEnemySpawnPointCount = 8;



    private static readonly IReadOnlyList<ScreenElementDefinition> TitleScreenElements =
    [
        // Large text uses the game's 32x32 magnified-glyph routine. X/Y are pixel coordinates.
        new("Title.Battle", "BATTLE 大字", 0x12A9, 6, 0x1A, 0x2E, ScreenElementKind.LargeGlyphString, PaletteKind.Title),
        new("Title.City", "CITY 大字", 0x12B0, 4, 0x3C, 0x56, ScreenElementKind.LargeGlyphString, PaletteKind.Title),

        // D6B3 strings are ordinary 8x8 background tiles. X/Y are tile coordinates.
        new("Title.TopLeft", "顶部左侧", 0x12B5, 2, 2, 3, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.TopCenter", "顶部中央", 0x12C1, 3, 11, 3, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.TopRight", "顶部右侧", 0x12B8, 2, 21, 3, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.OnePlayer", "1 PLAYER", 0x12D6, 8, 11, 17, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.TwoPlayers", "2 PLAYERS", 0x12DF, 9, 11, 19, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.Construction", "CONSTRUCTION", 0x12FB, 12, 11, 21, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.SymbolRow", "菜单符号行", 0x129F, 9, 11, 23, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.Copyright", "版权行", 0x1308, 22, 4, 25, ScreenElementKind.TileString, PaletteKind.Title),
        new("Title.Rights", "ALL RIGHTS RESERVED", 0x1330, 19, 6, 27, ScreenElementKind.TileString, PaletteKind.Title),
    ];

    private static readonly IReadOnlyList<ScreenElementDefinition> GameOverScreenElements =
    [
        new("GameOver.Game", "GAME 大字", 0x1353, 4, 0x3C, 0x46, ScreenElementKind.LargeGlyphString, PaletteKind.Level),
        new("GameOver.Over", "OVER 大字", 0x1358, 4, 0x3C, 0x78, ScreenElementKind.LargeGlyphString, PaletteKind.Level),
    ];

    public BattleCityRom(byte[] data, EditorConfig cfg)
    {
        _data = data.ToArray();
        _cfg = cfg;
        Validate();
    }

    public BattleCityRomKind Kind => _kind;
    public int PrgBanks => _data[4];
    public int ChrBanks => _data[5];
    public int PrgSizeBytes => PrgBanks * 0x4000;
    public int ChrSizeBytes => ChrBanks * 0x2000;
    public int MainBankShift => PrgBanks == 2 ? _cfg.ExpandedShift : 0;
    public int Length => _data.Length;

    public bool IsOriginal => Kind == BattleCityRomKind.Original16K;
    public bool IsEx => !IsOriginal;
    public bool IsExpanded => PrgBanks == 2;
    public bool HasOverlay => Kind == BattleCityRomKind.Ex32KOverlay;
    public bool HasIndependentMaps => Kind == BattleCityRomKind.Ex32K70Maps;
    public bool SupportsEnemyPowerUpPickup => HasIndependentMaps;
    public bool CanConvertToOverlay => Kind == BattleCityRomKind.Ex16K;
    public int MaxNormalStage => IsOriginal ? 35 : 70;
    // Demo is a real ROM-native map stored after the original 35 map slots.
    // In Ex mode we expose it as logical stage 71 so Stage 1~70 remain untouched.
    public int DemoStageNumber => MaxNormalStage + 1;
    public int MaxEditableStage => DemoStageNumber;

    public bool HasExV2Config => !IsOriginal && SpanEquals(ExV2ConfigOffset, ExV2Magic) && _data[ExV2ConfigOffset + 4] == 0x02;
    public byte ExV2ConfigVersion => HasExV2Config ? _data[ExV2ConfigOffset + 4] : (byte)0;
    public byte FeatureFlags => HasExV2Config ? _data[ExV2ConfigOffset + 5] : (byte)0;
    public byte EnemyItemFlags => HasExV2Config ? _data[ExV2ConfigOffset + 6] : (byte)0;
    public byte LayoutFlags => HasExV2Config ? _data[ExV2ConfigOffset + 7] : (byte)0;

    public bool SupportsCustomEnemyTotal => HasExV2Config && HasLayout(ExLayout.CustomEnemyTotal);
    public bool SupportsTerrain1F => HasExV2Config && HasLayout(ExLayout.ExtendedTerrain1F);
    public bool SupportsTerrain64 => HasExV2Config && HasLayout(ExLayout.Terrain64);
    public bool SupportsNoFriendlyFire => HasExV2Config && SupportsTerrain1F; // Phase 5 capability
    public bool SupportsLockInitialState => HasExV2Config && HasLayout(ExLayout.LockInitialState); // Phase 6
    public bool SupportsBonusReplaceAlways => HasExV2Config && HasLayout(ExLayout.BonusReplaceAlways); // Phase 6.1
    public bool SupportsPlayerFastMove => HasExV2Config && HasLayout(ExLayout.PlayerFastMove); // Phase 6.2
    public bool LockInitialState => SupportsLockInitialState && (EnemyItemFlags & 0x80) != 0;

    public bool HasFinalRules
    {
        get
        {
            if (!HasIndependentMaps || !SpanEquals(FinalRulesConfigOffset, FinalRulesMagic)) return false;
            var version = _data[FinalRulesConfigOffset + 4];
            return version is >= 0x02 and <= 0x06;
        }
    }
    public byte FinalRulesVersion => HasFinalRules ? _data[FinalRulesConfigOffset + 4] : (byte)0;
    public bool SupportsFinalRulesV3 => HasFinalRules && FinalRulesVersion >= 3;
    public bool SupportsFinalRulesV4 => HasFinalRules && FinalRulesVersion >= 4;
    public bool SupportsFinalRulesV5 => HasFinalRules && FinalRulesVersion >= 5;
    public bool SupportsFinalRulesV6 => HasFinalRules && FinalRulesVersion >= 6;
    public bool SupportsPlayerDeathLevel =>
        SupportsFinalRulesV6 && SpanEquals(Cpu8000FileOffset(0xFFA6), PlayerDeathLevelHook);
    public bool SupportsEnemyCounterDisplay =>
        SupportsFinalRulesV5 &&
        SpanEquals(Cpu8000FileOffset(0xC377), new byte[] { 0x20, 0x6A, 0xC7 }) &&
        SpanEquals(Cpu8000FileOffset(0xDB68), new byte[] { 0x20, 0x90, 0xC7 });
    public bool SkipFinalGameOver
    {
        get => HasFinalRules && (_data[Cpu8000FileOffset(FinalRulesFlagsCpu)] & 0x01) != 0;
        set
        {
            EnsureFinalRules();
            var o = Cpu8000FileOffset(FinalRulesFlagsCpu);
            _data[o] = value ? (byte)(_data[o] | 0x01) : (byte)(_data[o] & 0xFE);
        }
    }
    public int ExtraLifeMode
    {
        get { if (!HasFinalRules) return 0; var v = _data[Cpu8000FileOffset(FinalRulesExtraLifeModeCpu)]; return v <= 3 ? v : 0; }
        set { EnsureFinalRules(); if (value is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(value)); _data[Cpu8000FileOffset(FinalRulesExtraLifeModeCpu)] = (byte)value; }
    }
    public int ExtraLifeValue
    {
        get => HasFinalRules ? Math.Clamp((int)_data[Cpu8000FileOffset(FinalRulesExtraLifeValueCpu)], 1, 99) : 2;
        set { EnsureFinalRules(); _data[Cpu8000FileOffset(FinalRulesExtraLifeValueCpu)] = (byte)Math.Clamp(value, 1, 99); }
    }
    public int TwoPlayerBonusMode
    {
        get { if (!HasFinalRules) return 0; var v = _data[Cpu8000FileOffset(FinalRulesTwoPlayerBonusCpu)]; return v <= 1 ? v : 0; }
        set { EnsureFinalRules(); if (value is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(value)); _data[Cpu8000FileOffset(FinalRulesTwoPlayerBonusCpu)] = (byte)value; }
    }
    public int ArmoredTankMode
    {
        get { if (!HasFinalRules) return 0; var v = _data[Cpu8000FileOffset(FinalRulesArmoredTankCpu)]; return v <= 1 ? v : 0; }
        set { EnsureFinalRules(); if (value is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(value)); _data[Cpu8000FileOffset(FinalRulesArmoredTankCpu)] = (byte)value; }
    }
    public int CheatPlayer1Lives
    {
        get => SupportsFinalRulesV3 ? Math.Clamp((int)_data[Cpu8000FileOffset(FinalRulesCheatP1LivesCpu)], 1, 99) : 10;
        set { EnsureFinalRulesV3(); _data[Cpu8000FileOffset(FinalRulesCheatP1LivesCpu)] = (byte)Math.Clamp(value, 1, 99); }
    }
    public int CheatPlayer2Lives
    {
        get => SupportsFinalRulesV3 ? Math.Clamp((int)_data[Cpu8000FileOffset(FinalRulesCheatP2LivesCpu)], 1, 99) : 10;
        set { EnsureFinalRulesV3(); _data[Cpu8000FileOffset(FinalRulesCheatP2LivesCpu)] = (byte)Math.Clamp(value, 1, 99); }
    }

    public int TerrainCount => IsOriginal ? 16 : SupportsTerrain64 ? 64 : SupportsTerrain1F ? 32 : 24;

    public IReadOnlyList<int> SelectableTerrainIds
    {
        get
        {
            // $0E/$0F are internal/reserved block IDs. Hide them from both map and TSA editors.
            var max = TerrainCount - 1;
            return Enumerable.Range(0, 14)
                .Concat(max >= 0x10 ? Enumerable.Range(0x10, max - 0x10 + 1) : Enumerable.Empty<int>())
                .ToArray();
        }
    }

    public RomProfile Profile => new(
        Kind,
        Kind switch
        {
            BattleCityRomKind.Original16K => "Battle City Original 16KB",
            BattleCityRomKind.Ex16K => HasExV2Config ? "Battle City Ex / BCEX v2 16KB" : "Battle City Ex 16KB (Legacy)",
            BattleCityRomKind.Ex32KOverlay => HasExV2Config ? "BCEX v2 32KB Overlay" : "Quarrel Ex 32KB Overlay (Legacy)",
            _ => "BCEX v2 32KB / 70 Independent Maps"
        },
        MaxNormalStage,
        TerrainCount,
        SupportsCustomEnemyTotal,
        HasIndependentMaps,
        SupportsEnemyPowerUpPickup,
        TerrainCount > 16,
        SupportsTerrain64,
        HasOverlay);

    public bool IsDemoStage(int stage) => stage == DemoStageNumber;
    public bool HasLayout(ExLayout layout) => HasExV2Config && (LayoutFlags & (byte)layout) != 0;
    public bool IsFeatureEnabled(ExFeature feature) => HasExV2Config && (FeatureFlags & (byte)feature) != 0;

    public void SetFeature(ExFeature feature, bool enabled)
    {
        if (!HasExV2Config) throw new InvalidOperationException("当前 ROM 没有 BCEX v2 配置块。不能修改 Ex 功能开关。");
        if (feature == ExFeature.PlayerFastMove && !SupportsPlayerFastMove)
            throw new InvalidOperationException("我方坦克加速移动需要 BCEX v2 Phase 6.2 或更新 ROM。旧版 bit3 属于已废弃的 Pending 奖励语义。 ");
        if (feature == ExFeature.EnemyPowerUpPickup && !SupportsEnemyPowerUpPickup)
            throw new InvalidOperationException("敌人拾取道具仅支持 BCEX v2 32KB 独立70地图格式。");
        if (feature == ExFeature.NoFriendlyFire && !SupportsNoFriendlyFire)
            throw new InvalidOperationException("取消队友互伤需要 BCEX v2 Phase 5 或更新的 ROM。旧版只有 Flag 位，没有对应运行程序。 ");

        var value = FeatureFlags;
        value = enabled ? (byte)(value | (byte)feature) : (byte)(value & ~(byte)feature);
        if (feature == ExFeature.PistolLevel4 && !enabled)
            value = (byte)(value & ~(byte)ExFeature.Level4DestroyTrees);
        _data[ExV2ConfigOffset + 5] = value;
        if (feature == ExFeature.DowngradeOnHit && SupportsPlayerDeathLevel)
            WritePlayerDeathLevelRaw(enabled ? 0 : 4);
    }

    public void SetFeatureFlags(byte flags)
    {
        if (!HasExV2Config) throw new InvalidOperationException("当前 ROM 没有 BCEX v2 配置块。");
        if ((flags & (byte)ExFeature.PistolLevel4) == 0)
            flags = (byte)(flags & ~(byte)ExFeature.Level4DestroyTrees);
        if (!SupportsEnemyPowerUpPickup)
            flags = (byte)(flags & ~(byte)ExFeature.EnemyPowerUpPickup);
        if (!SupportsNoFriendlyFire)
            flags = (byte)(flags & ~(byte)ExFeature.NoFriendlyFire);
        if (!SupportsPlayerFastMove)
            flags = (byte)(flags & ~(byte)ExFeature.PlayerFastMove);
        _data[ExV2ConfigOffset + 5] = flags;
        if (SupportsPlayerDeathLevel)
            WritePlayerDeathLevelRaw((flags & (byte)ExFeature.DowngradeOnHit) != 0 ? 0 : 4);
    }

    public bool IsEnemyItemEffectEnabled(EnemyItemEffect effect)
        => HasExV2Config && (EnemyItemFlags & (byte)effect) != 0;

    public void SetEnemyItemEffect(EnemyItemEffect effect, bool enabled)
    {
        if (!SupportsEnemyPowerUpPickup || !HasExV2Config)
            throw new InvalidOperationException("当前 ROM 不支持敌人道具效果配置。");
        var lockBit = (byte)(EnemyItemFlags & 0x80);
        var value = (byte)(EnemyItemFlags & 0x7F);
        value = enabled ? (byte)(value | (byte)effect) : (byte)(value & ~(byte)effect);
        _data[ExV2ConfigOffset + 6] = (byte)(lockBit | (value & 0x7F));
    }

    public void SetEnemyItemFlags(byte flags)
    {
        if (!SupportsEnemyPowerUpPickup || !HasExV2Config)
            throw new InvalidOperationException("当前 ROM 不支持敌人道具效果配置。");
        _data[ExV2ConfigOffset + 6] = (byte)((EnemyItemFlags & 0x80) | (flags & 0x7F));
    }

    public void SetLockInitialState(bool enabled)
    {
        if (!SupportsLockInitialState)
            throw new InvalidOperationException("锁定初始状态需要 BCEX v2 Phase 6 或更新 ROM。");
        var v = EnemyItemFlags;
        _data[ExV2ConfigOffset + 6] = enabled ? (byte)(v | 0x80) : (byte)(v & 0x7F);
    }

    public byte[] GetBytesCopy() => _data.ToArray();

    public void RestoreBytes(byte[] data)
    {
        _data = data.ToArray();
        Validate();
    }

    public void Save(string path)
    {
        ValidateAllEnemyTotals();
        File.WriteAllBytes(path, _data);
    }

    public void Validate()
    {
        if (_data.Length < 0x10 || _data[0] != (byte)'N' || _data[1] != (byte)'E' || _data[2] != (byte)'S' || _data[3] != 0x1A)
            throw new InvalidDataException("不是有效的 iNES ROM。");
        if (PrgBanks is not (1 or 2))
            throw new InvalidDataException("仅支持 16KB / 32KB PRG 的 Battle City / Battle City Ex。");
        if (ChrBanks != 1)
            throw new InvalidDataException("当前版本要求 8KB CHR-ROM。");

        var mapper = (_data[6] >> 4) | (_data[7] & 0xF0);
        if (mapper != 0) throw new InvalidDataException($"ROM Mapper={mapper}，不是 Battle City 使用的 Mapper 0。");

        var expected = 0x10 + PrgSizeBytes + ChrSizeBytes;
        if (_data.Length < expected) throw new InvalidDataException("ROM 长度小于 Header 声明的 PRG+CHR 大小。");

        _kind = DetectKind();
        EnsureRange(Offset(_cfg.StageMapStart), _cfg.StageSize * 36);
        EnsureRange(Offset(_cfg.EnemyType1To35), 35 * 4);
        EnsureRange(Offset(_cfg.EnemyCount1To35), 35 * 4);
        EnsureRange(LevelPaletteOffset, 16);

        if (IsOriginal)
        {
            EnsureRange(TerrainAttributesOffset, 16);
            EnsureRange(TerrainBlocksOffset, 16 * 4);
            return;
        }

        EnsureRange(Offset(_cfg.EnemyType36To70), 35 * 4);
        EnsureRange(Offset(_cfg.EnemyCount36To70), 35 * 4);
        EnsureRange(TerrainAttributesOffset, TerrainCount);
        EnsureRange(TerrainBlocksOffset, TerrainCount * 4);

        if (!HasExpectedExtendedWater())
            throw new InvalidDataException("检测到 Ex 程序结构，但 10~17 扩展水块 TSA 表不完整或地址不匹配。");

        if (HasOverlay)
        {
            EnsureRange(_cfg.OverlayStart, _cfg.OverlayPageSize * _cfg.OverlayStageCount);
            EnsureRange(_cfg.HelperFileOffset, OverlayHelper.Length);
        }
        else if (HasIndependentMaps)
        {
            EnsureRange(_cfg.ExV2MapStart, _cfg.ExV2MapStageStride * _cfg.ExV2MapStageCount);
            for (var stage = 1; stage <= 70; stage++)
            for (var row = 0; row < 13; row++)
            for (var col = 0; col < 13; col++)
            {
                var id = _data[IndependentMapOffset(stage, row, col)];
                if (id >= TerrainCount)
                    throw new InvalidDataException($"Stage {stage} ({row},{col}) 的地形ID ${id:X2} 超出当前地形表范围 $00~${TerrainCount - 1:X2}。");
            }
        }
    }

    private BattleCityRomKind DetectKind()
    {
        if (PrgBanks == 1)
        {
            if (LooksLikeEx(0)) return BattleCityRomKind.Ex16K;
            if (LooksLikeOriginal(0)) return BattleCityRomKind.Original16K;
            throw new InvalidDataException("未识别为受支持的 Battle City 日版原版或本项目 Battle City Ex ROM。");
        }

        if (!LooksLikeEx(_cfg.ExpandedShift))
            throw new InvalidDataException("检测到 32KB NROM，但第二个 PRG Bank 不是受支持的 Battle City Ex 主程序。");

        var configOffset = _cfg.ExV2ConfigStart + _cfg.ExpandedShift;
        var hasMagic = SpanEquals(configOffset, ExV2Magic) && _data[configOffset + 4] == 0x02;
        var independent = hasMagic && (_data[configOffset + 7] & (byte)ExLayout.Independent70Maps) != 0;
        var patch = _cfg.StageDrawPatchExpanded;
        EnsureRange(patch, 7);

        if (independent)
        {
            if (!SpanEquals(patch, Map70DrawSequence))
                throw new InvalidDataException("BCEX v2 声明为32KB独立70地图，但关卡绘制入口不是预期的 $B250 helper。");
            return BattleCityRomKind.Ex32K70Maps;
        }

        if (!SpanEquals(patch, OverlayDrawSequence))
            throw new InvalidDataException("检测到 32KB Battle City Ex，但没有找到受支持的 Overlay / 70-map 读取补丁。");
        return BattleCityRomKind.Ex32KOverlay;
    }

    private bool LooksLikeOriginal(int shift)
    {
        var attrRef = CpuToFile(0xD817, shift);
        var tsaRef = CpuToFile(0xD832, shift);
        if (!SpanEquals(attrRef, OriginalAttrReference) || !SpanEquals(tsaRef, OriginalTsaReference)) return false;
        var water = _cfg.OriginalTerrainBlocks + shift + 0x0A * 4;
        return RangeEquals(water, [0x12,0x12,0x12,0x12]);
    }

    private bool LooksLikeEx(int shift)
    {
        var attrRef = CpuToFile(0xD817, shift);
        var tsaRef = CpuToFile(0xD832, shift);
        if (attrRef < 0 || tsaRef < 0 || tsaRef + 3 > _data.Length) return false;

        if (SpanEquals(attrRef, LegacyExAttrReference) &&
            _data[tsaRef] == 0xBD && _data[tsaRef + 2] == 0xFF && _data[tsaRef + 1] is 0x04 or 0x08)
            return true;

        if (SpanEquals(attrRef, Phase5AttrReference) && SpanEquals(tsaRef, Phase5TsaReference))
            return true;

        if (SpanEquals(attrRef, Terrain64AttrReference) && SpanEquals(tsaRef, Terrain64TsaReference))
            return true;

        return false;
    }

    private bool HasExpectedExtendedWater()
    {
        var found = 0;
        for (var id = 0x10; id <= 0x17; id++)
        {
            for (var q = 0; q < 4; q++)
            {
                if (_data[TerrainBlocksOffset + id * 4 + q] != 0x12) continue;
                found++;
                break;
            }
        }
        return found >= 8;
    }

    public int GetCell(int stage, int row, int column)
    {
        ValidateStage(stage);
        ValidateCell(row, column);
        if (HasIndependentMaps && !IsDemoStage(stage)) return _data[IndependentMapOffset(stage, row, column)];

        var physicalStage = GetPhysicalMapStage(stage);
        var nibbleIndex = row * _cfg.StorageStrideNibbles + column;
        var raw = GetNibble(StageMapOffset(physicalStage), nibbleIndex);
        if (HasOverlay && !IsDemoStage(stage) && raw == 0x0E)
        {
            var ext = _data[OverlayOffset(physicalStage, nibbleIndex)];
            if (ext >= 0x10 && ext < TerrainCount) return ext;
        }
        return raw;
    }

    /// <returns>True if a 16KB Ex ROM was converted to 32KB overlay as part of the write.</returns>
    public bool SetCell(int stage, int row, int column, int terrainId)
    {
        ValidateStage(stage);
        ValidateCell(row, column);
        ValidateTerrainId(terrainId);
        if (terrainId is 0x0E or 0x0F)
            throw new InvalidOperationException("地形 ID $0E/$0F 为内部保留值，编辑器禁止写入。");
        if (IsOriginal && terrainId > 0x0F)
            throw new InvalidOperationException("原版 Battle City 只能保存地形 ID 00~0F。");
        if (IsDemoStage(stage) && terrainId > 0x0D)
            throw new InvalidOperationException("Demo 地图仍使用原版4-bit地图格式，只能保存可编辑地形 $00~$0D。");

        if (HasIndependentMaps && !IsDemoStage(stage))
        {
            _data[IndependentMapOffset(stage, row, column)] = (byte)terrainId;
            return false;
        }

        var converted = false;
        if (terrainId >= 0x10 && !IsDemoStage(stage)) converted = EnsureExpandedForExtendedTerrain();

        var physicalStage = GetPhysicalMapStage(stage);
        var nibbleIndex = row * _cfg.StorageStrideNibbles + column;
        var map = StageMapOffset(physicalStage);
        if (terrainId >= 0x10)
        {
            SetNibble(map, nibbleIndex, 0x0E);
            _data[OverlayOffset(physicalStage, nibbleIndex)] = (byte)terrainId;
        }
        else
        {
            SetNibble(map, nibbleIndex, terrainId & 0x0F);
            if (HasOverlay && !IsDemoStage(stage)) _data[OverlayOffset(physicalStage, nibbleIndex)] = 0;
        }
        return converted;
    }

    public (byte[] Types, byte[] Counts) GetEnemyData(int stage)
    {
        ValidateStage(stage);
        var type = EnemyTypeOffset(stage);
        var count = EnemyCountOffset(stage);
        return (_data[type..(type + 4)], _data[count..(count + 4)]);
    }

    public int GetEnemyTotal(int stage) => GetEnemyData(stage).Counts.Sum(x => (int)x);

    public void SetEnemyType(int stage, int slot, byte value)
    {
        ValidateStage(stage); ValidateEnemySlot(slot);
        _data[EnemyTypeOffset(stage) + slot] = value;
    }

    public void SetEnemyCount(int stage, int slot, byte value)
    {
        ValidateStage(stage); ValidateEnemySlot(slot);
        _data[EnemyCountOffset(stage) + slot] = value;
    }

    public int ValidateEnemyTotal(IEnumerable<byte> counts)
    {
        var total = counts.Sum(x => (int)x);
        if (SupportsCustomEnemyTotal && (total < 1 || total > 255))
            throw new InvalidOperationException("当前 BCEX v2 支持自定义总敌人数，但四个 Count 合计必须在 1~255。");
        return total;
    }

    public void ValidateAllEnemyTotals()
    {
        if (!SupportsCustomEnemyTotal) return;
        for (var stage = 1; stage <= 70; stage++)
        {
            var total = GetEnemyTotal(stage);
            if (total < 1 || total > 255)
                throw new InvalidOperationException($"Stage {stage} 的敌人 Count 合计={total}，必须在 1~255。");
        }
    }

    public byte StartingLives
    {
        get => _data[HasFinalRules ? Cpu8000FileOffset(FinalRulesNormalStartingLivesCpu) : Offset(_cfg.StartingLives)];
        set
        {
            if (value == 0) throw new ArgumentOutOfRangeException(nameof(value), "起始命数必须在 1~255。");
            _data[HasFinalRules ? Cpu8000FileOffset(FinalRulesNormalStartingLivesCpu) : Offset(_cfg.StartingLives)] = value;
        }
    }

    private static readonly byte[] LevelToStatus = [0x00, 0x20, 0x40, 0x60, 0x63];
    private static readonly byte[] DeathLevelToCutoff = [0x20, 0x40, 0x60, 0x63, 0x64];

    public int InitialTankLevel
    {
        get
        {
            var raw = _data[Offset(_cfg.InitialTankStatus)];
            for (var i = 0; i < LevelToStatus.Length; i++) if (LevelToStatus[i] == raw) return i;
            return raw >= 0x63 ? 4 : raw >= 0x60 ? 3 : raw >= 0x40 ? 2 : raw >= 0x20 ? 1 : 0;
        }
        set
        {
            var max = IsOriginal ? 3 : 4;
            if (value < 0 || value > max) throw new ArgumentOutOfRangeException(nameof(value), $"当前 ROM 初始等级范围为 Lv0~Lv{max}。");
            _data[Offset(_cfg.InitialTankStatus)] = LevelToStatus[value];
        }
    }

    public int PlayerDeathLevel
    {
        get
        {
            if (!SupportsPlayerDeathLevel)
                return IsFeatureEnabled(ExFeature.DowngradeOnHit) ? 0 : 4;
            var raw = _data[Cpu8000FileOffset(FinalRulesPlayerDeathCutoffCpu)];
            for (var i = 0; i < DeathLevelToCutoff.Length; i++)
                if (DeathLevelToCutoff[i] == raw) return i;
            return IsFeatureEnabled(ExFeature.DowngradeOnHit) ? 0 : 4;
        }
        set
        {
            EnsureFinalRulesV6();
            if (value is < 0 or > 4) throw new ArgumentOutOfRangeException(nameof(value), "死亡等级必须在 Lv0~Lv4。");
            WritePlayerDeathLevelRaw(value);
            if (HasExV2Config)
            {
                var o = ExV2ConfigOffset + 5;
                _data[o] = value < 4 ? (byte)(_data[o] | (byte)ExFeature.DowngradeOnHit) : (byte)(_data[o] & ~(byte)ExFeature.DowngradeOnHit);
            }
        }
    }

    private void WritePlayerDeathLevelRaw(int level)
    {
        if (!SupportsPlayerDeathLevel) return;
        _data[Cpu8000FileOffset(FinalRulesPlayerDeathCutoffCpu)] = DeathLevelToCutoff[Math.Clamp(level, 0, 4)];
    }

    public (byte X, byte Y) GetSpawn(SpawnKind kind)
    {
        var (x, y) = SpawnOffsets(kind);
        return (_data[Offset(x)], _data[Offset(y)]);
    }

    public void SetSpawn(SpawnKind kind, byte x, byte y)
    {
        var (xo, yo) = SpawnOffsets(kind);
        _data[Offset(xo)] = x;
        _data[Offset(yo)] = y;
    }

    public int GetCustomEnemySpawnCount(int stage, bool twoPlayer)
    {
        EnsureFinalRulesStage(stage);
        return _data[FinalRulesSpawnRecordOffset(stage) + (twoPlayer ? 1 : 0)];
    }

    public void SetCustomEnemySpawnCount(int stage, bool twoPlayer, int count)
    {
        EnsureFinalRulesStage(stage);
        if (count is < 0 or > 8) throw new ArgumentOutOfRangeException(nameof(count), "Custom spawn count 必须是 Original(0) 或 1~8。");
        _data[FinalRulesSpawnRecordOffset(stage) + (twoPlayer ? 1 : 0)] = (byte)count;
    }

    public (byte X, byte Y) GetCustomEnemySpawnPoint(int stage, int index)
    {
        EnsureFinalRulesStage(stage);
        if (index is < 0 or >= CustomEnemySpawnPointCount) throw new ArgumentOutOfRangeException(nameof(index));
        var o = FinalRulesSpawnRecordOffset(stage);
        // Runtime clamps custom coordinates to the safe playfield as well.
        // Clamp on read so old/unused records such as $E0 never break the editor UI.
        var x = (byte)Math.Clamp((int)_data[o + 2 + index], CustomEnemySpawnMin, CustomEnemySpawnMax);
        var y = (byte)Math.Clamp((int)_data[o + 10 + index], CustomEnemySpawnMin, CustomEnemySpawnMax);
        return (x, y);
    }

    public void SetCustomEnemySpawnPoint(int stage, int index, int x, int y)
    {
        EnsureFinalRulesStage(stage);
        if (index is < 0 or >= CustomEnemySpawnPointCount) throw new ArgumentOutOfRangeException(nameof(index));
        var o = FinalRulesSpawnRecordOffset(stage);
        _data[o + 2 + index] = (byte)Math.Clamp(x, CustomEnemySpawnMin, CustomEnemySpawnMax);
        _data[o + 10 + index] = (byte)Math.Clamp(y, CustomEnemySpawnMin, CustomEnemySpawnMax);
    }

    public (int Row, int Column, int TerrainId) GetCustomEnemySpawnCell(int stage, int index)
    {
        var p = GetCustomEnemySpawnPoint(stage, index);
        var col = Math.Clamp((int)Math.Round((p.X - CustomEnemySpawnMin) / 16.0), 0, 12);
        var row = Math.Clamp((int)Math.Round((p.Y - CustomEnemySpawnMin) / 16.0), 0, 12);
        return (row, col, GetCell(stage, row, col));
    }

    public void SetDefaultEightCustomEnemySpawns(int stage)
    {
        EnsureFinalRulesStage(stage);
        (int X, int Y)[] points =
        [
            (0x18,0x18),(0x58,0x18),(0x98,0x18),(0xD8,0x18),
            (0xD8,0x78),(0x98,0xD8),(0x58,0xD8),(0x18,0x78)
        ];
        SetCustomEnemySpawnCount(stage, false, 8);
        SetCustomEnemySpawnCount(stage, true, 8);
        for (var i = 0; i < points.Length; i++)
            SetCustomEnemySpawnPoint(stage, i, points[i].X, points[i].Y);
    }

    public int GetEnemySpawnInterval(int stage, bool twoPlayer)
    {
        EnsureFinalRulesV3Stage(stage);
        var cpu = twoPlayer ? FinalRulesPacing2PIntervalCpu : FinalRulesPacing1PIntervalCpu;
        return Math.Clamp((int)_data[Cpu8000FileOffset(cpu) + stage - 1], 1, 255);
    }

    public void SetEnemySpawnInterval(int stage, bool twoPlayer, int frames)
    {
        EnsureFinalRulesV3Stage(stage);
        if (frames is < 1 or > 255) throw new ArgumentOutOfRangeException(nameof(frames), "敌人出现间隔必须是 1~255 帧；数值越小越快。");
        var cpu = twoPlayer ? FinalRulesPacing2PIntervalCpu : FinalRulesPacing1PIntervalCpu;
        _data[Cpu8000FileOffset(cpu) + stage - 1] = (byte)frames;
    }

    public int GetMaxActiveEnemies(int stage, bool twoPlayer)
    {
        EnsureFinalRulesV3Stage(stage);
        if (SupportsFinalRulesV5)
        {
            var raw = _data[Cpu8000FileOffset(FinalRulesPackedStageRulesCpu) + stage - 1];
            var limit = twoPlayer ? raw & 0x07 : (raw >> 5) & 0x07;
            return Math.Clamp(limit - 1, 1, 6);
        }
        var cpu = twoPlayer ? FinalRulesPacing2PMaxActiveLegacyCpu : FinalRulesPacing1PMaxActiveLegacyCpu;
        return Math.Clamp((int)_data[Cpu8000FileOffset(cpu) + stage - 1], 1, 6);
    }

    public void SetMaxActiveEnemies(int stage, bool twoPlayer, int count)
    {
        EnsureFinalRulesV3Stage(stage);
        if (count is < 1 or > 6) throw new ArgumentOutOfRangeException(nameof(count), "最大同时在场敌人数必须是 1~6。");
        if (SupportsFinalRulesV5)
        {
            var o = Cpu8000FileOffset(FinalRulesPackedStageRulesCpu) + stage - 1;
            var raw = _data[o];
            var limit = count + 1;
            _data[o] = twoPlayer
                ? (byte)((raw & 0xF8) | (limit & 0x07))
                : (byte)((raw & 0x1F) | ((limit & 0x07) << 5));
            return;
        }
        var cpu = twoPlayer ? FinalRulesPacing2PMaxActiveLegacyCpu : FinalRulesPacing1PMaxActiveLegacyCpu;
        _data[Cpu8000FileOffset(cpu) + stage - 1] = (byte)count;
    }

    public void SetStage35EnemyPacingPreset(int stage)
    {
        EnsureFinalRulesV3Stage(stage);
        SetEnemySpawnInterval(stage, false, GetEnemySpawnInterval(35, false));
        SetEnemySpawnInterval(stage, true, GetEnemySpawnInterval(35, true));
        SetMaxActiveEnemies(stage, false, GetMaxActiveEnemies(35, false));
        SetMaxActiveEnemies(stage, true, GetMaxActiveEnemies(35, true));
    }

    public void SetOriginalEnemyPacingPreset(int stage)
    {
        EnsureFinalRulesV3Stage(stage);
        var originalStage = Math.Min(stage, 35);
        var onePlayer = Math.Clamp(0xBE - originalStage * 4, 1, 255);
        var twoPlayer = Math.Clamp(onePlayer - 0x14, 1, 255);
        SetEnemySpawnInterval(stage, false, onePlayer);
        SetEnemySpawnInterval(stage, true, twoPlayer);
        SetMaxActiveEnemies(stage, false, 4);
        SetMaxActiveEnemies(stage, true, 6);
    }

    public bool GetStageBaseExists(int stage)
    {
        EnsureFinalRulesV4Stage(stage);
        if (SupportsFinalRulesV5)
            return (_data[Cpu8000FileOffset(FinalRulesPackedStageRulesCpu) + stage - 1] & 0x10) != 0;
        return _data[Cpu8000FileOffset(FinalRulesBaseExistsLegacyCpu) + stage - 1] != 0;
    }

    public void SetStageBaseExists(int stage, bool exists)
    {
        EnsureFinalRulesV4Stage(stage);
        if (SupportsFinalRulesV5)
        {
            var o = Cpu8000FileOffset(FinalRulesPackedStageRulesCpu) + stage - 1;
            _data[o] = exists ? (byte)(_data[o] | 0x10) : (byte)(_data[o] & 0xEF);
            return;
        }
        _data[Cpu8000FileOffset(FinalRulesBaseExistsLegacyCpu) + stage - 1] = exists ? (byte)1 : (byte)0;
    }

    public bool GetEnemyCounterNumericPreference(int stage)
    {
        EnsureEnemyCounterDisplayStage(stage);
        return (_data[Cpu8000FileOffset(FinalRulesPackedStageRulesCpu) + stage - 1] & 0x08) != 0;
    }

    public void SetEnemyCounterNumericPreference(int stage, bool numeric)
    {
        EnsureEnemyCounterDisplayStage(stage);
        var o = Cpu8000FileOffset(FinalRulesPackedStageRulesCpu) + stage - 1;
        _data[o] = numeric ? (byte)(_data[o] | 0x08) : (byte)(_data[o] & 0xF7);
    }

    public bool IsEnemyCounterNumericForced(int stage)
    {
        EnsureEnemyCounterDisplayStage(stage);
        return GetEnemyTotal(stage) > 50;
    }

    public bool UseNumericEnemyCounter(int stage)
    {
        EnsureEnemyCounterDisplayStage(stage);
        return IsEnemyCounterNumericForced(stage) || GetEnemyCounterNumericPreference(stage);
    }

    public (bool IsCustom, byte X, byte Y) GetStagePlayerSpawn(int stage, bool twoPlayer)
    {
        EnsureFinalRulesV5Stage(stage);
        var raw = _data[StagePlayerSpawnOffset(stage, twoPlayer)];
        if (raw == 0xFF)
        {
            var p = GetSpawn(twoPlayer ? SpawnKind.Player2 : SpawnKind.Player1);
            return (false, p.X, p.Y);
        }
        var xi = Math.Min(12, (raw >> 4) & 0x0F);
        var yi = Math.Min(12, raw & 0x0F);
        return (true, (byte)(CustomEnemySpawnMin + xi * 16), (byte)(CustomEnemySpawnMin + yi * 16));
    }

    public void SetStagePlayerSpawnOriginal(int stage, bool twoPlayer)
    {
        EnsureFinalRulesV5Stage(stage);
        _data[StagePlayerSpawnOffset(stage, twoPlayer)] = 0xFF;
    }

    public void SetStagePlayerSpawn(int stage, bool twoPlayer, int x, int y)
    {
        EnsureFinalRulesV5Stage(stage);
        var xi = Math.Clamp((x - CustomEnemySpawnMin + 8) / 16, 0, 12);
        var yi = Math.Clamp((y - CustomEnemySpawnMin + 8) / 16, 0, 12);
        _data[StagePlayerSpawnOffset(stage, twoPlayer)] = (byte)((xi << 4) | yi);
    }

    public (int Row, int Column, int TerrainId) GetStagePlayerSpawnCell(int stage, bool twoPlayer)
    {
        var p = GetStagePlayerSpawn(stage, twoPlayer);
        var col = Math.Clamp((p.X - CustomEnemySpawnMin) / 16, 0, 12);
        var row = Math.Clamp((p.Y - CustomEnemySpawnMin) / 16, 0, 12);
        return (row, col, GetCell(stage, row, col));
    }

    private (int X, int Y) SpawnOffsets(SpawnKind kind) => kind switch
    {
        SpawnKind.Enemy1 => (_cfg.Enemy1X, _cfg.Enemy1Y),
        SpawnKind.Enemy2 => (_cfg.Enemy2X, _cfg.Enemy2Y),
        SpawnKind.Enemy3 => (_cfg.Enemy3X, _cfg.Enemy3Y),
        SpawnKind.Player1 => (_cfg.Player1X, _cfg.Player1Y),
        SpawnKind.Player2 => (_cfg.Player2X, _cfg.Player2Y),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public void ClearStage(int stage, byte terrainId = 0x0D)
    {
        ValidateStage(stage);
        if (!SelectableTerrainIds.Contains(terrainId)) throw new InvalidOperationException("清空地形 ID 必须是可编辑地形。");
        for (var r = 0; r < 13; r++)
        for (var c = 0; c < 13; c++)
            SetCell(stage, r, c, terrainId);
    }

    public void ClearAllStages(byte terrainId = 0x0D)
    {
        for (var stage = 1; stage <= MaxNormalStage; stage++) ClearStage(stage, terrainId);
        ClearStage(DemoStageNumber, terrainId);
    }

    public byte[] GetPalette(PaletteKind kind)
    {
        var o = Offset(PaletteOffset(kind));
        EnsureRange(o, 16);
        return _data[o..(o + 16)];
    }

    public void SetPalette(PaletteKind kind, IEnumerable<byte> values)
    {
        var a = values.ToArray();
        if (a.Length != 16) throw new InvalidOperationException("每组 NES Palette 必须正好 16 bytes。");
        var o = Offset(PaletteOffset(kind));
        for (var i = 0; i < 16; i++) _data[o + i] = (byte)(a[i] & 0x3F);
    }

    public byte GetPaletteByte(PaletteKind kind, int index)
    {
        if (index is < 0 or >= 16) throw new ArgumentOutOfRangeException(nameof(index));
        return _data[Offset(PaletteOffset(kind)) + index];
    }

    public void SetPaletteByte(PaletteKind kind, int index, byte value)
    {
        if (index is < 0 or >= 16) throw new ArgumentOutOfRangeException(nameof(index));
        _data[Offset(PaletteOffset(kind)) + index] = (byte)(value & 0x3F);
    }

    private int PaletteOffset(PaletteKind kind) => kind switch
    {
        PaletteKind.Sprite => _cfg.PaletteSpr,
        PaletteKind.Frame2 => _cfg.PaletteFrame2,
        PaletteKind.Level => _cfg.LevelPalette,
        PaletteKind.Frame1 => _cfg.PaletteFrame1,
        PaletteKind.Title => _cfg.TitleScrPalette,
        PaletteKind.LevelSelect => _cfg.LevelSelPalette,
        PaletteKind.Misc1 => _cfg.PaletteMisc1,
        PaletteKind.Misc2 => _cfg.PaletteMisc2,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public byte GetFlagTsaTile(bool fort, int row, int column)
    {
        if (row is < 0 or >= 4 || column is < 0 or >= 6) throw new ArgumentOutOfRangeException();
        var o = Offset(fort ? _cfg.FortTsa : _cfg.FlagTsa) + row * 7 + column;
        return _data[o];
    }

    public void SetFlagTsaTile(bool fort, int row, int column, byte tile)
    {
        if (row is < 0 or >= 4 || column is < 0 or >= 6) throw new ArgumentOutOfRangeException();
        var o = Offset(fort ? _cfg.FortTsa : _cfg.FlagTsa) + row * 7 + column;
        _data[o] = tile;
        // Keep original row terminator intact.
        _data[Offset(fort ? _cfg.FortTsa : _cfg.FlagTsa) + row * 7 + 6] = 0xFF;
    }

    public int[] GetFlagTsa(bool fort)
    {
        var result = new int[24];
        for (var r = 0; r < 4; r++) for (var c = 0; c < 6; c++) result[r * 6 + c] = GetFlagTsaTile(fort, r, c);
        return result;
    }

    public void SetFlagTsa(bool fort, IEnumerable<int> tiles)
    {
        var a = tiles.ToArray();
        if (a.Length != 24) throw new InvalidOperationException("Flag/Fort TSA 必须包含 24 个 CHR Tile（6×4）。");
        for (var r = 0; r < 4; r++) for (var c = 0; c < 6; c++) SetFlagTsaTile(fort, r, c, (byte)a[r * 6 + c]);
    }


    public IReadOnlyList<ScreenElementDefinition> GetScreenElements(ScreenKind kind)
        => kind == ScreenKind.Title ? TitleScreenElements : GameOverScreenElements;

    public byte GetScreenElementTile(ScreenElementDefinition element, int index)
    {
        if (index < 0 || index >= element.Length) throw new ArgumentOutOfRangeException(nameof(index));
        return _data[Offset(element.FileOffset16K) + index];
    }

    public int[] GetScreenElementTiles(ScreenElementDefinition element)
        => Enumerable.Range(0, element.Length).Select(i => (int)GetScreenElementTile(element, i)).ToArray();

    public void SetScreenElementTile(ScreenElementDefinition element, int index, byte tile)
    {
        if (index < 0 || index >= element.Length) throw new ArgumentOutOfRangeException(nameof(index));
        _data[Offset(element.FileOffset16K) + index] = tile;
    }

    public ScreenElementDefinition? FindScreenElement(ScreenKind kind, string key)
        => GetScreenElements(kind).FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.Ordinal));

    public IReadOnlyList<int> SelectableTerrainIdsForStage(int stage)
        => IsDemoStage(stage) ? Enumerable.Range(0, 14).ToArray() : SelectableTerrainIds;

    public QuarrelExStagePackage ExportStagePackage(int stage)
    {
        ValidateStage(stage);
        var allowed = SelectableTerrainIdsForStage(stage).ToHashSet();
        var used = new SortedSet<int>();
        var map = Enumerable.Range(0, 13).Select(_ => new int[13]).ToArray();
        for (var row = 0; row < 13; row++)
        for (var col = 0; col < 13; col++)
        {
            var id = GetCell(stage, row, col);
            if (!allowed.Contains(id))
                throw new InvalidDataException($"Stage {(IsDemoStage(stage) ? "Demo" : stage)} ({row + 1},{col + 1}) 含内部/不可导出的地形 ${id:X2}。");
            map[row][col] = id;
            used.Add(id);
        }

        var package = new QuarrelExStagePackage { SourceStage = stage, Map = map };
        foreach (var id in used)
        {
            package.Terrain.Add(new TerrainDefinitionConfig
            {
                Id = id,
                Attr = GetTerrainAttribute(id) & 3,
                Tiles = GetTerrainTiles(id).Select(x => (int)x).ToArray()
            });
        }
        return package;
    }

    public ConfigValidationResult ValidateStagePackage(QuarrelExStagePackage? package, int targetStage)
    {
        var result = new ConfigValidationResult();
        void Error(string text) => result.Errors.Add(text);
        void Warn(string text) => result.Warnings.Add(text);

        try { ValidateStage(targetStage); }
        catch (Exception ex) { Error(ex.Message); return result; }
        if (package is null) { Error("关卡配置为空。"); return result; }
        if (!string.Equals(package.Schema, "QuarrelExStage", StringComparison.Ordinal))
            Error("关卡配置缺少有效的 Schema=QuarrelExStage。");
        if (package.Version != 1) Error($"不支持的关卡配置版本 Version={package.Version}；当前只支持 Version=1。");

        if (package.Map is null || package.Map.Length != 13)
            Error("Map 必须正好包含 13 行。");
        else
        {
            var allowed = SelectableTerrainIdsForStage(targetStage).ToHashSet();
            for (var row = 0; row < 13; row++)
            {
                if (package.Map[row] is null || package.Map[row].Length != 13)
                {
                    Error($"Map 第 {row + 1} 行必须正好包含 13 个地形 ID。");
                    continue;
                }
                for (var col = 0; col < 13; col++)
                {
                    var id = package.Map[row][col];
                    if (!allowed.Contains(id))
                        Error($"Map ({row + 1},{col + 1}) 的地形 ${id:X2} 不能用于目标关卡。");
                }
            }
        }

        var seen = new HashSet<int>();
        var targetAllowed = SelectableTerrainIdsForStage(targetStage).ToHashSet();
        if (package.Terrain is null) Error("Terrain 必须是数组。");
        foreach (var td in package.Terrain ?? [])
        {
            if (!seen.Add(td.Id)) { Error($"Terrain ${td.Id:X2} 重复定义。"); continue; }
            if (!targetAllowed.Contains(td.Id)) Error($"Terrain ${td.Id:X2} 不能用于目标关卡。");
            if (td.Attr is < 0 or > 3) Error($"Terrain ${td.Id:X2} 的 Attr={td.Attr}，只能是 0~3。");
            if (td.Tiles is null || td.Tiles.Length != 4) Error($"Terrain ${td.Id:X2} 必须包含 4 个 TSA Tile。");
            else if (td.Tiles.Any(x => x is < 0 or > 255)) Error($"Terrain ${td.Id:X2} 的 TSA Tile 必须是 0~255。");
        }

        if (package.Map is { Length: 13 } && package.Map.All(r => r is { Length: 13 }))
        {
            var used = package.Map.SelectMany(r => r).Distinct().ToArray();
            foreach (var id in used)
                if (!seen.Contains(id)) Error($"Map 使用了 Terrain ${id:X2}，但关卡配置没有携带该地形的 TSA/Attr 定义。");
        }

        if (package.SourceStage > 0 && package.SourceStage != targetStage)
            Warn($"来源为 Stage {package.SourceStage}；将导入到当前 Stage {(IsDemoStage(targetStage) ? "Demo" : targetStage.ToString())}。");
        if ((package.Terrain?.Count ?? 0) > 0)
            Warn("Terrain 的 TSA/Attr 表属于 ROM 全局数据；导入会更新配置中携带的地形 ID，其他使用相同 ID 的关卡外观也会同步变化。");
        return result;
    }

    public List<string> ApplyStagePackage(QuarrelExStagePackage package, int targetStage)
    {
        var validation = ValidateStagePackage(package, targetStage);
        if (!validation.IsValid) throw new InvalidDataException(validation.FormatErrors());

        // Apply map first: placing an extended terrain ID can promote a legacy 16KB Ex ROM
        // to the 32KB overlay layout, which also relocates the editable TSA/Attr tables.
        for (var row = 0; row < 13; row++)
        for (var col = 0; col < 13; col++)
            SetCell(targetStage, row, col, package.Map[row][col]);
        foreach (var td in package.Terrain)
        {
            SetTerrainAttribute(td.Id, (byte)td.Attr);
            for (var q = 0; q < 4; q++) SetTerrainTile(td.Id, q, (byte)td.Tiles[q]);
        }

        return validation.Warnings.ToList();
    }

    public QuarrelExSharedConfig ExportSharedConfig()
    {
        var cfg = new QuarrelExSharedConfig();
        cfg.Gameplay.StartingLives = StartingLives;
        cfg.Gameplay.InitialTankLevel = InitialTankLevel;
        cfg.Gameplay.PlayerDeathLevel = SupportsPlayerDeathLevel ? PlayerDeathLevel : null;
        cfg.Gameplay.LockInitialState = LockInitialState;
        cfg.Gameplay.FeatureFlags = HasExV2Config ? FeatureFlags : null;
        cfg.Gameplay.PlayerFastMove = SupportsPlayerFastMove ? IsFeatureEnabled(ExFeature.PlayerFastMove) : false;
        cfg.Gameplay.EnemyItemFlags = SupportsEnemyPowerUpPickup ? (EnemyItemFlags & 0x7F) : null;
        if (HasFinalRules)
        {
            cfg.Gameplay.FinalRules = new FinalRulesConfig
            {
                SkipFinalGameOver = SkipFinalGameOver,
                ExtraLifeMode = ExtraLifeMode,
                ExtraLifeValue = ExtraLifeValue,
                TwoPlayerBonusMode = TwoPlayerBonusMode,
                ArmoredTankMode = ArmoredTankMode,
                CheatPlayer1Lives = SupportsFinalRulesV3 ? CheatPlayer1Lives : null,
                CheatPlayer2Lives = SupportsFinalRulesV3 ? CheatPlayer2Lives : null
            };
        }
        foreach (SpawnKind k in Enum.GetValues<SpawnKind>())
        {
            var p = GetSpawn(k);
            cfg.Gameplay.Spawns[k.ToString()] = new SpawnPointConfig { X = p.X, Y = p.Y };
        }
        foreach (PaletteKind k in Enum.GetValues<PaletteKind>()) cfg.Palettes[k.ToString()] = GetPalette(k).Select(x => (int)x).ToArray();
        foreach (var id in SelectableTerrainIds)
        {
            var t = GetTerrainTiles(id);
            cfg.Terrain.Add(new TerrainDefinitionConfig { Id = id, Attr = GetTerrainAttribute(id) & 3, Tiles = t.Select(x => (int)x).ToArray() });
        }
        cfg.FlagTsa.Flag = GetFlagTsa(false);
        cfg.FlagTsa.Fort = GetFlagTsa(true);

        // Config v3 carries complete stage data. Original ROM exports Stage 1~35;
        // every Ex format exports logical Stage 1~70 (shared-map formats naturally
        // contain duplicated 36~70 map data, while their enemy tables stay independent).
        var stageMax = IsOriginal ? 35 : 70;
        for (var stage = 1; stage <= stageMax; stage++)
        {
            var (types, counts) = GetEnemyData(stage);
            var sc = new StageConfig
            {
                Stage = stage,
                EnemyTypes = types.Select(x => (int)x).ToArray(),
                EnemyCounts = counts.Select(x => (int)x).ToArray(),
                EnemyTotal = counts.Sum(x => (int)x),
                Map = Enumerable.Range(0, 13)
                    .Select(r => Enumerable.Range(0, 13).Select(c => GetCell(stage, r, c)).ToArray())
                    .ToArray()
            };
            if (HasFinalRules)
            {
                sc.EnemySpawn = new EnemySpawnConfig
                {
                    Player1Count = GetCustomEnemySpawnCount(stage, false),
                    Player2Count = GetCustomEnemySpawnCount(stage, true),
                    Points = Enumerable.Range(0, CustomEnemySpawnPointCount)
                        .Select(i =>
                        {
                            var p = GetCustomEnemySpawnPoint(stage, i);
                            return new SpawnPointConfig { X = p.X, Y = p.Y };
                        }).ToList()
                };
            }
            if (SupportsFinalRulesV3)
            {
                sc.EnemyPacing = new EnemyPacingConfig
                {
                    Player1IntervalFrames = GetEnemySpawnInterval(stage, false),
                    Player2IntervalFrames = GetEnemySpawnInterval(stage, true),
                    Player1MaxActive = GetMaxActiveEnemies(stage, false),
                    Player2MaxActive = GetMaxActiveEnemies(stage, true)
                };
            }
            if (SupportsFinalRulesV4)
                sc.BaseExists = GetStageBaseExists(stage);
            if (SupportsFinalRulesV5)
            {
                if (SupportsEnemyCounterDisplay) sc.EnemyCounterDisplay = GetEnemyCounterNumericPreference(stage) ? "Number" : "Icons";
                var p1 = GetStagePlayerSpawn(stage, false);
                var p2 = GetStagePlayerSpawn(stage, true);
                sc.PlayerSpawn = new StagePlayerSpawnConfig
                {
                    Player1 = p1.IsCustom ? new SpawnPointConfig { X = p1.X, Y = p1.Y } : null,
                    Player2 = p2.IsCustom ? new SpawnPointConfig { X = p2.X, Y = p2.Y } : null
                };
            }
            cfg.Stages.Add(sc);
        }

        cfg.Demo = new DemoConfig
        {
            Map = Enumerable.Range(0, 13)
                .Select(r => Enumerable.Range(0, 13).Select(c => GetCell(DemoStageNumber, r, c)).ToArray())
                .ToArray()
        };
        cfg.Screens = new ScreensConfig
        {
            Title = ExportScreenLayout(ScreenKind.Title),
            GameOver = ExportScreenLayout(ScreenKind.GameOver)
        };
        return cfg;
    }

    private ScreenLayoutConfig ExportScreenLayout(ScreenKind kind)
    {
        var layout = new ScreenLayoutConfig();
        foreach (var element in GetScreenElements(kind))
            layout.Elements[element.Key] = GetScreenElementTiles(element);
        return layout;
    }

    public ConfigValidationResult ValidateSharedConfig(QuarrelExSharedConfig? cfg)
    {
        var result = new ConfigValidationResult();
        void Error(string text) => result.Errors.Add(text);
        void Warn(string text)
        {
            if (!result.Warnings.Contains(text)) result.Warnings.Add(text);
        }

        if (cfg is null)
        {
            Error("配置文件为空。");
            return result;
        }
        if (!string.Equals(cfg.Schema, "QuarrelExConfig", StringComparison.Ordinal))
            Error("Schema 必须为 \"QuarrelExConfig\"。");
        if (cfg.Version != 3)
            Error($"只支持 QuarrelExConfig v3；当前文件 Version={cfg.Version}。");

        if (cfg.Gameplay is null)
        {
            Error("缺少 Gameplay。");
        }
        else
        {
            var g = cfg.Gameplay;
            if (g.StartingLives is < 1 or > 255)
                Error($"Gameplay.StartingLives={g.StartingLives}，必须在 1~255。");
            if (g.InitialTankLevel is < 0 or > 4)
                Error($"Gameplay.InitialTankLevel={g.InitialTankLevel}，必须在 0~4。");
            else if (IsOriginal && g.InitialTankLevel == 4)
                Warn("目标为原版 ROM，不支持 Lv4；InitialTankLevel=4 将按 Lv3 导入。");
            if (g.PlayerDeathLevel is < 0 or > 4)
                Error($"Gameplay.PlayerDeathLevel={g.PlayerDeathLevel}，必须在 0~4 或为 null。");
            else if (g.PlayerDeathLevel.HasValue && !SupportsPlayerDeathLevel)
                Warn("目标 ROM 不支持独立死亡等级；PlayerDeathLevel 将被忽略。需要 Runtime 6.9.4 / QXR1 v6。");

            if (g.FeatureFlags is < 0 or > 255)
                Error($"Gameplay.FeatureFlags={g.FeatureFlags}，必须在 0~255 或为 null。");
            if (g.EnemyItemFlags is < 0 or > 0x7F)
                Error($"Gameplay.EnemyItemFlags={g.EnemyItemFlags}，必须在 0~127 或为 null。");

            if (g.FeatureFlags.HasValue && g.PlayerFastMove.HasValue)
            {
                var bit = (g.FeatureFlags.Value & (byte)ExFeature.PlayerFastMove) != 0;
                if (bit != g.PlayerFastMove.Value)
                    Error("Gameplay.PlayerFastMove 与 FeatureFlags bit3 不一致。");
            }

            if (!HasExV2Config && (g.FeatureFlags.HasValue || g.EnemyItemFlags.HasValue || g.LockInitialState || g.PlayerFastMove == true))
                Warn("目标 ROM 没有 BCEX v2 配置块；Ex 功能相关字段将被忽略。");
            else if (HasExV2Config && g.FeatureFlags.HasValue)
            {
                var flags = (byte)g.FeatureFlags.Value;
                if ((flags & (byte)ExFeature.PlayerFastMove) != 0 && !SupportsPlayerFastMove)
                    Warn("目标 ROM 不支持“我方坦克加速移动”；该位将被清除。");
                if ((flags & (byte)ExFeature.EnemyPowerUpPickup) != 0 && !SupportsEnemyPowerUpPickup)
                    Warn("目标 ROM 不支持“敌人拾取道具”；该位将被清除。");
                if ((flags & (byte)ExFeature.NoFriendlyFire) != 0 && !SupportsNoFriendlyFire)
                    Warn("目标 ROM 不支持“取消队友互伤”；该位将被清除。");
                if ((flags & (byte)ExFeature.Level4DestroyTrees) != 0 && (flags & (byte)ExFeature.PistolLevel4) == 0)
                    Warn("配置开启了 Lv4 消树林但未开启手枪/Lv4；导入时 Lv4 消树林位会自动清除。");
            }

            if (g.EnemyItemFlags.HasValue && !SupportsEnemyPowerUpPickup)
                Warn("目标 ROM 不支持敌人道具效果；EnemyItemFlags 将被忽略。");
            if (g.LockInitialState && !SupportsLockInitialState)
                Warn("目标 ROM 不支持锁定初始状态；该选项将被忽略。");

            if (g.FinalRules is not null)
            {
                if (!HasFinalRules)
                {
                    Warn("目标 ROM 不支持 QXR1 v2~v6 Final Rules；Gameplay.FinalRules 将被忽略。");
                }
                else
                {
                    if (g.FinalRules.ExtraLifeMode is < 0 or > 3) Error("Gameplay.FinalRules.ExtraLifeMode 必须是 0~3。");
                    if (g.FinalRules.ExtraLifeValue is < 1 or > 99) Error("Gameplay.FinalRules.ExtraLifeValue 必须是 1~99。");
                    if (g.FinalRules.TwoPlayerBonusMode is < 0 or > 1) Error("Gameplay.FinalRules.TwoPlayerBonusMode 必须是 0 或 1。");
                    if (g.FinalRules.ArmoredTankMode is < 0 or > 1) Error("Gameplay.FinalRules.ArmoredTankMode 必须是 0 或 1。");
                    if ((g.FinalRules.CheatPlayer1Lives.HasValue || g.FinalRules.CheatPlayer2Lives.HasValue) && !SupportsFinalRulesV3)
                        Warn("配置包含 A+B+Start 秘籍命数，但目标 ROM 不是 QXR1 v3 / Runtime 6.6；这些值将被忽略。");
                    if (g.FinalRules.CheatPlayer1Lives.HasValue && g.FinalRules.CheatPlayer1Lives.Value is < 1 or > 99) Error("Gameplay.FinalRules.CheatPlayer1Lives 必须是 1~99。");
                    if (g.FinalRules.CheatPlayer2Lives.HasValue && g.FinalRules.CheatPlayer2Lives.Value is < 1 or > 99) Error("Gameplay.FinalRules.CheatPlayer2Lives 必须是 1~99。");
                }
            }

            if (g.Spawns is null)
            {
                Error("Gameplay.Spawns 缺失。");
            }
            else
            {
                foreach (SpawnKind k in Enum.GetValues<SpawnKind>())
                {
                    if (!g.Spawns.TryGetValue(k.ToString(), out var sp) || sp is null)
                    {
                        Error($"Gameplay.Spawns 缺少 {k}。");
                        continue;
                    }
                    if (sp.X is < 0 or > 255 || sp.Y is < 0 or > 255)
                        Error($"Gameplay.Spawns.{k} 的 X/Y 必须在 0~255；当前为 ({sp.X},{sp.Y})。");
                }
            }
        }

        if (cfg.Palettes is null)
        {
            Error("缺少 Palettes。");
        }
        else
        {
            foreach (PaletteKind k in Enum.GetValues<PaletteKind>())
            {
                if (!cfg.Palettes.TryGetValue(k.ToString(), out var pal) || pal is null)
                {
                    Error($"Palettes 缺少 {k}。");
                    continue;
                }
                if (pal.Length != 16)
                {
                    Error($"Palettes.{k} 必须正好包含 16 个颜色值。");
                    continue;
                }
                for (var i = 0; i < pal.Length; i++)
                    if (pal[i] is < 0 or > 0x3F)
                        Error($"Palettes.{k}[{i}]={pal[i]}，NES 调色板编号必须在 0~63。");
            }
        }

        if (cfg.Terrain is null || cfg.Terrain.Count == 0)
        {
            Error("Terrain 缺失或为空。");
        }
        else
        {
            var seenTerrain = new HashSet<int>();
            foreach (var td in cfg.Terrain)
            {
                if (td is null)
                {
                    Error("Terrain 中存在 null 项。");
                    continue;
                }
                if (!seenTerrain.Add(td.Id))
                    Error($"Terrain ID ${td.Id:X2} 重复。");
                if (td.Id is < 0 or > 0x3F)
                    Error($"Terrain ID {td.Id} 超出 Config v3 支持范围 $00~$3F。");
                if (td.Id is 0x0E or 0x0F)
                    Warn($"Terrain ${td.Id:X2} 为内部保留 ID；导入时会忽略。");
                if (td.Id >= TerrainCount)
                    Warn($"Terrain ${td.Id:X2} 超出目标 ROM 的 TSA 范围 $00~${TerrainCount - 1:X2}；该定义将被忽略。");
                if (td.Attr is < 0 or > 3)
                    Error($"Terrain ${td.Id:X2} 的 Attr={td.Attr}，只能是 0~3。");
                if (td.Tiles is null || td.Tiles.Length != 4)
                {
                    Error($"Terrain ${td.Id:X2} 必须包含 TL/TR/BL/BR 共 4 个 CHR Tile。");
                }
                else
                {
                    for (var q = 0; q < 4; q++)
                        if (td.Tiles[q] is < 0 or > 255)
                            Error($"Terrain ${td.Id:X2} Tiles[{q}]={td.Tiles[q]}，CHR Tile 必须在 0~255。");
                }
            }
        }

        if (cfg.FlagTsa is null)
        {
            Error("缺少 FlagTsa。");
        }
        else
        {
            void ValidateFlagArray(string name, int[]? values)
            {
                if (values is null || values.Length != 24)
                {
                    Error($"FlagTsa.{name} 必须正好包含 24 个 CHR Tile（6×4）。");
                    return;
                }
                for (var i = 0; i < values.Length; i++)
                    if (values[i] is < 0 or > 255)
                        Error($"FlagTsa.{name}[{i}]={values[i]}，CHR Tile 必须在 0~255。");
            }
            ValidateFlagArray("Flag", cfg.FlagTsa.Flag);
            ValidateFlagArray("Fort", cfg.FlagTsa.Fort);
        }

        if (cfg.Stages is null || cfg.Stages.Count is not (35 or 70))
        {
            Error($"Stages 必须包含完整的 35 关（原版）或 70 关（Ex）；当前数量={cfg.Stages?.Count ?? 0}。");
        }
        else
        {
            var configStageMax = cfg.Stages.Count == 35 ? 35 : 70;
            var stageByNumber = new Dictionary<int, StageConfig>();
            foreach (var sc in cfg.Stages)
            {
                if (sc is null)
                {
                    Error("Stages 中存在 null 项。");
                    continue;
                }
                if (sc.Stage < 1 || sc.Stage > configStageMax)
                {
                    Error($"Stage 编号 {sc.Stage} 超出当前配置应有的 1~{configStageMax}。");
                    continue;
                }
                if (!stageByNumber.TryAdd(sc.Stage, sc))
                    Error($"Stage {sc.Stage} 重复。");

                if (sc.EnemyTypes is null || sc.EnemyTypes.Length != 4)
                    Error($"Stage {sc.Stage} 的 EnemyTypes 必须正好包含 4 项。");
                else
                    for (var i = 0; i < 4; i++)
                        if (sc.EnemyTypes[i] is < 0 or > 255)
                            Error($"Stage {sc.Stage} EnemyTypes[{i}]={sc.EnemyTypes[i]}，必须在 0~255。");

                if (sc.EnemyCounts is null || sc.EnemyCounts.Length != 4)
                {
                    Error($"Stage {sc.Stage} 的 EnemyCounts 必须正好包含 4 项。");
                }
                else
                {
                    var invalidCount = false;
                    for (var i = 0; i < 4; i++)
                        if (sc.EnemyCounts[i] is < 0 or > 255)
                        {
                            Error($"Stage {sc.Stage} EnemyCounts[{i}]={sc.EnemyCounts[i]}，必须在 0~255。");
                            invalidCount = true;
                        }
                    if (!invalidCount)
                    {
                        var sum = sc.EnemyCounts.Sum();
                        if (sum is < 1 or > 255)
                            Error($"Stage {sc.Stage} 四个 EnemyCounts 合计={sum}，必须在 1~255。");
                        if (sc.EnemyTotal != sum)
                            Error($"Stage {sc.Stage} 的 EnemyTotal={sc.EnemyTotal}，但 EnemyCounts 合计={sum}。");
                        if (!SupportsCustomEnemyTotal && sum != 20)
                            Warn($"Stage {sc.Stage} 敌人总数={sum}，但目标 ROM 只支持固定 20 辆；该关 EnemyCounts 将保持目标 ROM 原值。");
                    }
                }

                if (sc.EnemySpawn is not null)
                {
                    if (!HasFinalRules)
                    {
                        Warn($"Stage {sc.Stage} 包含 EnemySpawn，但目标 ROM 不支持 QXR1 v2~v6 Final Rules；这些出生点将被忽略。");
                    }
                    else
                    {
                        if (sc.EnemySpawn.Player1Count is < 0 or > 8 || sc.EnemySpawn.Player2Count is < 0 or > 8)
                            Error($"Stage {sc.Stage}: EnemySpawn 的 1P/2P Count 必须是 0~8。");
                        if (sc.EnemySpawn.Points is null || sc.EnemySpawn.Points.Count != CustomEnemySpawnPointCount)
                        {
                            Error($"Stage {sc.Stage}: EnemySpawn.Points 必须正好有 8 个坐标。");
                        }
                        else
                        {
                            for (var i = 0; i < sc.EnemySpawn.Points.Count; i++)
                            {
                                var point = sc.EnemySpawn.Points[i];
                                if (point.X is < CustomEnemySpawnMin or > CustomEnemySpawnMax || point.Y is < CustomEnemySpawnMin or > CustomEnemySpawnMax)
                                    Error($"Stage {sc.Stage}: EnemySpawn.Points[{i}] 必须在 $18~$D8；当前 ({point.X},{point.Y})。");
                            }
                        }
                    }
                }

                if (sc.EnemyPacing is not null)
                {
                    if (!SupportsFinalRulesV3)
                    {
                        Warn($"Stage {sc.Stage} 包含 EnemyPacing，但目标 ROM 不是 QXR1 v3+ / Runtime 6.6+；该项将被忽略。");
                    }
                    else
                    {
                        var ep = sc.EnemyPacing;
                        if (ep.Player1IntervalFrames is < 1 or > 255 || ep.Player2IntervalFrames is < 1 or > 255)
                            Error($"Stage {sc.Stage}: EnemyPacing 的 1P/2P IntervalFrames 必须是 1~255。");
                        if (ep.Player1MaxActive is < 1 or > 6 || ep.Player2MaxActive is < 1 or > 6)
                            Error($"Stage {sc.Stage}: EnemyPacing 的 1P/2P MaxActive 必须是 1~6。");
                    }
                }

                if (sc.BaseExists.HasValue && !SupportsFinalRulesV4)
                    Warn($"Stage {sc.Stage} 包含 BaseExists，但目标 ROM 不是 QXR1 v4+；该项将被忽略。");

                if (sc.EnemyCounterDisplay is not null)
                {
                    if (sc.EnemyCounterDisplay != "Icons" && sc.EnemyCounterDisplay != "Number")
                        Error($"Stage {sc.Stage}: EnemyCounterDisplay 必须是 Icons 或 Number。");
                    else if (!SupportsEnemyCounterDisplay)
                        Warn($"Stage {sc.Stage} 包含 EnemyCounterDisplay，但目标 ROM 没有 Runtime 6.9.3 敌人数显示 Hook；该项将被忽略。");
                }

                if (sc.PlayerSpawn is not null)
                {
                    if (!SupportsFinalRulesV5)
                    {
                        Warn($"Stage {sc.Stage} 包含 PlayerSpawn，但目标 ROM 不是 QXR1 v5 / Runtime 6.9；该项将被忽略。");
                    }
                    else
                    {
                        foreach (var (name, point) in new[] { ("Player1", sc.PlayerSpawn.Player1), ("Player2", sc.PlayerSpawn.Player2) })
                        {
                            if (point is null) continue; // null = Original / global spawn.
                            if (point.X is < CustomEnemySpawnMin or > CustomEnemySpawnMax || point.Y is < CustomEnemySpawnMin or > CustomEnemySpawnMax)
                                Error($"Stage {sc.Stage}: PlayerSpawn.{name} 必须在 $18~$D8。");
                            else if (((point.X - CustomEnemySpawnMin) & 0x0F) != 0 || ((point.Y - CustomEnemySpawnMin) & 0x0F) != 0)
                                Error($"Stage {sc.Stage}: PlayerSpawn.{name} 必须使用 16px 网格坐标（$18,$28...$D8）。");
                        }
                    }
                }

                if (sc.Map is null || sc.Map.Length != 13 || sc.Map.Any(row => row is null || row.Length != 13))
                {
                    Error($"Stage {sc.Stage} 的 Map 必须是完整 13×13。");
                }
                else
                {
                    var warnedStageTerrain = false;
                    for (var r = 0; r < 13; r++)
                    for (var c = 0; c < 13; c++)
                    {
                        var id = sc.Map[r][c];
                        if (id is < 0 or > 0x3F)
                        {
                            Error($"Stage {sc.Stage} Map[{r},{c}]={id}，地形 ID 必须在 $00~$3F。");
                            continue;
                        }
                        if (id is 0x0E or 0x0F)
                        {
                            if (!warnedStageTerrain)
                            {
                                Warn($"Stage {sc.Stage} 地图含内部保留地形 $0E/$0F；这些格子导入时保持目标 ROM 原值。");
                                warnedStageTerrain = true;
                            }
                        }
                        else if (id >= TerrainCount)
                        {
                            if (!warnedStageTerrain)
                            {
                                Warn($"Stage {sc.Stage} 含目标 ROM 无法表示的地形 ID；这些格子导入时保持目标 ROM 原值。");
                                warnedStageTerrain = true;
                            }
                        }
                    }
                }
            }

            for (var stage = 1; stage <= configStageMax; stage++)
                if (!stageByNumber.ContainsKey(stage))
                    Error($"Stages 缺少 Stage {stage}。");

            var targetMax = IsOriginal ? 35 : 70;
            if (configStageMax > targetMax)
                Warn($"配置包含 Stage 36~{configStageMax}，但目标 ROM 只支持 Stage 1~{targetMax}；超出的关卡将忽略。");
            else if (configStageMax < targetMax)
                Warn($"配置只包含 Stage 1~{configStageMax}；目标 ROM 的 Stage {configStageMax + 1}~{targetMax} 将保持原值。");

            if (!HasIndependentMaps && configStageMax == 70)
            {
                for (var stage = 36; stage <= 70; stage++)
                {
                    if (!stageByNumber.TryGetValue(stage, out var second) ||
                        !stageByNumber.TryGetValue(stage - 35, out var first) ||
                        first.Map is null || second.Map is null ||
                        first.Map.Length != 13 || second.Map.Length != 13 ||
                        first.Map.Any(row => row is null || row.Length != 13) ||
                        second.Map.Any(row => row is null || row.Length != 13))
                        continue;
                    var same = Enumerable.Range(0, 13).All(r => first.Map[r].SequenceEqual(second.Map[r]));
                    if (!same)
                    {
                        Warn("目标 ROM 的 Stage36~70 地图复用 Stage1~35；配置中的独立二周目地图无法写入，但 Enemy Type/Count 仍可导入。");
                        break;
                    }
                }
            }
        }


        if (cfg.Demo is null)
        {
            Warn("Config v3 未包含 Demo.Map；目标 ROM 的 Demo 地图将保持原值。");
        }
        else if (cfg.Demo.Map is null || cfg.Demo.Map.Length != 13 || cfg.Demo.Map.Any(row => row is null || row.Length != 13))
        {
            Error("Demo.Map 必须是完整 13×13。");
        }
        else
        {
            for (var r = 0; r < 13; r++)
            for (var c = 0; c < 13; c++)
            {
                var id = cfg.Demo.Map[r][c];
                if (id is < 0 or > 0x0F)
                    Error($"Demo.Map[{r},{c}]={id}，Demo 使用原版4-bit地图格式，必须在 $00~$0F。");
                else if (id is 0x0E or 0x0F)
                    Warn("Demo.Map 含内部保留地形 $0E/$0F；这些格子导入时保持目标 ROM 原值。");
            }
        }

        if (cfg.Screens is null)
        {
            Warn("Config v3 未包含 Screens；Title / Game Over 将保持目标 ROM 原值。");
        }
        else
        {
            ValidateScreenLayout(ScreenKind.Title, cfg.Screens.Title, "Screens.Title");
            ValidateScreenLayout(ScreenKind.GameOver, cfg.Screens.GameOver, "Screens.GameOver");
        }

        void ValidateScreenLayout(ScreenKind kind, ScreenLayoutConfig? layout, string name)
        {
            if (layout is null)
            {
                Warn($"{name} 缺失；对应画面保持目标 ROM 原值。");
                return;
            }
            if (layout.Elements is null)
            {
                Error($"{name}.Elements 缺失。");
                return;
            }
            foreach (var def in GetScreenElements(kind))
            {
                if (!layout.Elements.TryGetValue(def.Key, out var values) || values is null)
                {
                    Warn($"{name}.Elements 缺少 {def.Key}；该元素保持目标 ROM 原值。");
                    continue;
                }
                if (values.Length != def.Length)
                {
                    Error($"{name}.Elements.{def.Key} 必须正好包含 {def.Length} 个值。");
                    continue;
                }
                for (var i = 0; i < values.Length; i++)
                    if (values[i] is < 0 or > 0xFF)
                        Error($"{name}.Elements.{def.Key}[{i}]={values[i]}，必须在 $00~$FF；$FF 可作为字符串终止符。");
            }
        }

        return result;
    }

    public List<string> ApplySharedConfig(QuarrelExSharedConfig cfg)
    {
        var validation = ValidateSharedConfig(cfg);
        if (!validation.IsValid)
            throw new InvalidDataException(validation.FormatErrors());

        var notes = validation.Warnings.ToList();
        var g = cfg.Gameplay;

        StartingLives = (byte)g.StartingLives;
        InitialTankLevel = Math.Min(g.InitialTankLevel, IsOriginal ? 3 : 4);

        if (HasExV2Config && g.FeatureFlags.HasValue)
            SetFeatureFlags((byte)g.FeatureFlags.Value);
        if (SupportsPlayerDeathLevel && g.PlayerDeathLevel.HasValue)
            PlayerDeathLevel = g.PlayerDeathLevel.Value;
        // If an older v3 config has no PlayerDeathLevel but does contain FeatureFlags,
        // SetFeatureFlags() above already maps legacy DowngradeOnHit ON/OFF to Death Lv0/Lv4.
        // If both fields are absent, preserve the target ROM's existing death threshold.
        if (SupportsEnemyPowerUpPickup && g.EnemyItemFlags.HasValue)
            SetEnemyItemFlags((byte)g.EnemyItemFlags.Value);
        if (SupportsLockInitialState)
            SetLockInitialState(g.LockInitialState);

        if (HasFinalRules && g.FinalRules is not null)
        {
            SkipFinalGameOver = g.FinalRules.SkipFinalGameOver;
            ExtraLifeMode = g.FinalRules.ExtraLifeMode;
            ExtraLifeValue = g.FinalRules.ExtraLifeValue;
            TwoPlayerBonusMode = g.FinalRules.TwoPlayerBonusMode;
            ArmoredTankMode = g.FinalRules.ArmoredTankMode;
            if (SupportsFinalRulesV3 && g.FinalRules.CheatPlayer1Lives.HasValue) CheatPlayer1Lives = g.FinalRules.CheatPlayer1Lives.Value;
            if (SupportsFinalRulesV3 && g.FinalRules.CheatPlayer2Lives.HasValue) CheatPlayer2Lives = g.FinalRules.CheatPlayer2Lives.Value;
        }

        foreach (SpawnKind k in Enum.GetValues<SpawnKind>())
        {
            var sp = g.Spawns[k.ToString()];
            SetSpawn(k, (byte)sp.X, (byte)sp.Y);
        }

        foreach (PaletteKind k in Enum.GetValues<PaletteKind>())
        {
            var pal = cfg.Palettes[k.ToString()];
            SetPalette(k, pal.Select(x => (byte)x));
        }

        foreach (var td in cfg.Terrain)
        {
            if (td.Id is 0x0E or 0x0F || td.Id < 0 || td.Id >= TerrainCount) continue;
            SetTerrainAttribute(td.Id, (byte)td.Attr);
            for (var q = 0; q < 4; q++) SetTerrainTile(td.Id, q, (byte)td.Tiles[q]);
        }

        SetFlagTsa(false, cfg.FlagTsa.Flag);
        SetFlagTsa(true, cfg.FlagTsa.Fort);

        var targetMax = IsOriginal ? 35 : 70;
        foreach (var sc in cfg.Stages.OrderBy(x => x.Stage))
        {
            if (sc.Stage < 1 || sc.Stage > targetMax) continue;

            for (var slot = 0; slot < 4; slot++)
                SetEnemyType(sc.Stage, slot, (byte)sc.EnemyTypes[slot]);

            var sum = sc.EnemyCounts.Sum();
            if (SupportsCustomEnemyTotal || sum == 20)
                for (var slot = 0; slot < 4; slot++)
                    SetEnemyCount(sc.Stage, slot, (byte)sc.EnemyCounts[slot]);

            if (HasFinalRules && sc.EnemySpawn is not null)
            {
                SetCustomEnemySpawnCount(sc.Stage, false, sc.EnemySpawn.Player1Count);
                SetCustomEnemySpawnCount(sc.Stage, true, sc.EnemySpawn.Player2Count);
                for (var i = 0; i < CustomEnemySpawnPointCount; i++)
                    SetCustomEnemySpawnPoint(sc.Stage, i, sc.EnemySpawn.Points[i].X, sc.EnemySpawn.Points[i].Y);
            }
            if (SupportsFinalRulesV3 && sc.EnemyPacing is not null)
            {
                SetEnemySpawnInterval(sc.Stage, false, sc.EnemyPacing.Player1IntervalFrames);
                SetEnemySpawnInterval(sc.Stage, true, sc.EnemyPacing.Player2IntervalFrames);
                SetMaxActiveEnemies(sc.Stage, false, sc.EnemyPacing.Player1MaxActive);
                SetMaxActiveEnemies(sc.Stage, true, sc.EnemyPacing.Player2MaxActive);
            }
            if (SupportsFinalRulesV4 && sc.BaseExists.HasValue)
                SetStageBaseExists(sc.Stage, sc.BaseExists.Value);
            if (SupportsEnemyCounterDisplay && sc.EnemyCounterDisplay is not null)
                SetEnemyCounterNumericPreference(sc.Stage, sc.EnemyCounterDisplay == "Number");
            if (SupportsFinalRulesV5 && sc.PlayerSpawn is not null)
            {
                if (sc.PlayerSpawn.Player1 is null) SetStagePlayerSpawnOriginal(sc.Stage, false);
                else SetStagePlayerSpawn(sc.Stage, false, sc.PlayerSpawn.Player1.X, sc.PlayerSpawn.Player1.Y);
                if (sc.PlayerSpawn.Player2 is null) SetStagePlayerSpawnOriginal(sc.Stage, true);
                else SetStagePlayerSpawn(sc.Stage, true, sc.PlayerSpawn.Player2.X, sc.PlayerSpawn.Player2.Y);
            }

            // Shared-map Ex formats physically store only 1~35.
            if (sc.Stage > 35 && !HasIndependentMaps) continue;

            for (var r = 0; r < 13; r++)
            for (var c = 0; c < 13; c++)
            {
                var id = sc.Map[r][c];
                if (id is 0x0E or 0x0F || id < 0 || id >= TerrainCount) continue;
                SetCell(sc.Stage, r, c, id);
            }
        }


        if (cfg.Demo?.Map is { Length: 13 } demoMap)
        {
            for (var r = 0; r < 13; r++)
            for (var c = 0; c < 13; c++)
            {
                var id = demoMap[r][c];
                if (id is 0x0E or 0x0F || id < 0 || id > 0x0D) continue;
                SetCell(DemoStageNumber, r, c, id);
            }
        }

        ApplyScreenLayout(ScreenKind.Title, cfg.Screens?.Title);
        ApplyScreenLayout(ScreenKind.GameOver, cfg.Screens?.GameOver);

        return notes;
    }

    private void ApplyScreenLayout(ScreenKind kind, ScreenLayoutConfig? layout)
    {
        if (layout?.Elements is null) return;
        foreach (var def in GetScreenElements(kind))
        {
            if (!layout.Elements.TryGetValue(def.Key, out var values) || values is null || values.Length != def.Length) continue;
            for (var i = 0; i < def.Length; i++) SetScreenElementTile(def, i, (byte)values[i]);
        }
    }

    public byte GetTerrainAttribute(int id)
    {
        ValidateTerrainId(id);
        return _data[TerrainAttributesOffset + id];
    }

    public void SetTerrainAttribute(int id, byte value)
    {
        ValidateTerrainId(id);
        if (value > 3) throw new ArgumentOutOfRangeException(nameof(value), "Attr 只能选择 0~3。");
        _data[TerrainAttributesOffset + id] = (byte)(value & 3);
    }

    public byte[] GetTerrainTiles(int id)
    {
        ValidateTerrainId(id);
        var o = TerrainBlocksOffset + id * 4;
        return _data[o..(o + 4)];
    }

    public void SetTerrainTile(int id, int quadrant, byte value)
    {
        ValidateTerrainId(id);
        if (quadrant is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(quadrant));
        _data[TerrainBlocksOffset + id * 4 + quadrant] = value;
    }

    public byte GetLevelPaletteByte(int index) => GetPaletteByte(PaletteKind.Level, index);

    public int SpriteChrOffset => 0x10 + PrgSizeBytes;
    public int BackgroundChrOffset => SpriteChrOffset + 0x1000;

    public byte GetChrByte(int offset)
    {
        EnsureRange(offset, 1);
        return _data[offset];
    }

    public bool EnsureExpandedForExtendedTerrain()
    {
        if (HasOverlay) return false;
        if (IsOriginal) throw new InvalidOperationException("原版 Battle City 不会自动转换为 Ex。");
        if (Kind != BattleCityRomKind.Ex16K) throw new InvalidOperationException("当前 ROM 不能自动转换为 32KB Legacy Overlay。");
        if (_data.Length < 0x6010) throw new InvalidDataException("16KB Ex ROM 长度不足，无法扩容。");

        var old = _data;
        var expanded = new byte[0xA010];
        Buffer.BlockCopy(old, 0, expanded, 0, 0x10);
        expanded[4] = 2;
        Buffer.BlockCopy(old, 0x10, expanded, 0x4010, 0x4000);
        Buffer.BlockCopy(old, 0x4010, expanded, 0x8010, 0x2000);
        Buffer.BlockCopy(OverlayHelper, 0, expanded, _cfg.HelperFileOffset, OverlayHelper.Length);

        var patch = _cfg.StageDrawPatchExpanded;
        if (!expanded.AsSpan(patch, OriginalDrawSequence.Length).SequenceEqual(OriginalDrawSequence))
            throw new InvalidDataException("CPU $F054 绘制代码与预期不一致，已停止自动扩容。");
        Buffer.BlockCopy(OverlayDrawSequence, 0, expanded, patch, OverlayDrawSequence.Length);

        _data = expanded;
        Validate();
        return true;
    }

    public string Describe()
    {
        var p = Profile;
        var lines = new List<string>
        {
            $"ROM 类型: {p.DisplayName}",
            $"PRG: {PrgSizeBytes / 1024} KiB",
            $"CHR: {ChrSizeBytes / 1024} KiB",
            $"关卡: {(IsOriginal ? "1~35 + Demo" : "1~70 + Demo")}",
            $"地形定义数: {TerrainCount} (${TerrainCount - 1:X2} 最大ID)",
            $"Terrain Attr: ${TerrainAttributesOffset:X4}",
            $"Terrain TSA: ${TerrainBlocksOffset:X4}",
            $"1~35 Type: ${Offset(_cfg.EnemyType1To35):X4}",
            $"1~35 Count: ${Offset(_cfg.EnemyCount1To35):X4}",
        };

        if (!IsOriginal)
        {
            lines.Add($"36~70 Type: ${Offset(_cfg.EnemyType36To70):X4}");
            lines.Add($"36~70 Count: ${Offset(_cfg.EnemyCount36To70):X4}");
            lines.Add(HasIndependentMaps
                ? $"地图格式: 70张独立地图，1 byte/cell，表从 ${_cfg.ExV2MapStart:X4} 开始"
                : $"地图格式: Stage36~70复用1~35{(HasOverlay ? " + Extended Overlay" : string.Empty)}");

            if (HasExV2Config)
            {
                lines.Add($"BCEX Version: {ExV2ConfigVersion}");
                lines.Add($"FeatureFlags: ${FeatureFlags:X2}");
                lines.Add($"EnemyItemFlags: ${EnemyItemFlags:X2}");
                lines.Add($"LayoutFlags: ${LayoutFlags:X2}");
                lines.Add($"敌人总数: {(SupportsCustomEnemyTotal ? "1~255（四个Count合计）" : "原版20辆规格")}");
                lines.Add($"取消队友互伤: {(IsFeatureEnabled(ExFeature.NoFriendlyFire) ? "ON" : "OFF")}");
                if (SupportsPlayerFastMove) lines.Add($"我方坦克加速移动: {(IsFeatureEnabled(ExFeature.PlayerFastMove) ? "ON" : "OFF")}（Phase 6.2）");
                if (SupportsLockInitialState) lines.Add($"锁定初始状态: {(LockInitialState ? "ON" : "OFF")} / Initial Lv{InitialTankLevel}");
                if (SupportsBonusReplaceAlways) lines.Add("闪光坦克奖励: 每次命中立即替换当前道具（Phase 6.1 固定）");
            }
            else
            {
                lines.Add("BCEX v2: 未检测到（Legacy Ex；Feature Flags 不可编辑）");
            }

            if (HasFinalRules)
            {
                var runtime = FinalRulesVersion >= 6 ? "6.9.4" : FinalRulesVersion >= 5 ? "6.9.3" : FinalRulesVersion >= 4 ? "6.7/6.8" : FinalRulesVersion >= 3 ? "6.6" : "6.5";
                lines.Add($"Final Rules: QXR1 v{FinalRulesVersion} / Runtime {runtime}");
                lines.Add($"Final GAME OVER Skip: {(SkipFinalGameOver ? "ON" : "OFF")}");
                lines.Add($"Extra Life: mode={ExtraLifeMode}, value={ExtraLifeValue}×10000");
                lines.Add($"2P Bonus: {(TwoPlayerBonusMode == 0 ? "Original" : "Win Streak")}");
                lines.Add($"Armored Tank: {(ArmoredTankMode == 0 ? "Original" : FinalRulesVersion >= 4 ? "One Hit（普通装甲=白色1HP；闪光奖励装甲保持原版）" : "One Hit")}");
                if (SupportsPlayerDeathLevel)
                    lines.Add($"玩家等级: Initial Lv{InitialTankLevel} / Death Lv{PlayerDeathLevel}（Runtime 6.9.4）");
                lines.Add("自定义敌人出生点: Stage 1~70 / 1P、2P 各 Original 或 1~8 点");
                if (SupportsFinalRulesV3)
                {
                    lines.Add($"A+B+Start 秘籍命数: 1P={CheatPlayer1Lives}, 2P={CheatPlayer2Lives}");
                    lines.Add("敌人出现节奏: Stage 1~70 独立 1P/2P Interval + Max Active（Runtime 6.6+）");
                }
                if (SupportsFinalRulesV4)
                    lines.Add("老巢存在: Stage 1~70 独立开关；关闭后地图底层地形不被 HQ 覆盖");
                if (SupportsFinalRulesV5)
                    lines.Add("玩家出生点: Stage 1~70 / 1P、2P 各 Original 或独立16px网格位置（Runtime 6.9.3）");
                    lines.Add("敌人数显示: Stage 1~70 Icons / Number；总数 > 50 时运行时强制 Number（Runtime 6.9.3）");
            }

            if (SupportsTerrain64) lines.Add("地形: $00~$3F，共64项；$20~$3F为预留自定义槽。");
            else if (SupportsTerrain1F) lines.Add("地形: $00~$1F，共32项；$18~$1F为单格砖/钢。");
            else lines.Add("地形: Legacy $00~$17（正常选择跳过$0E/$0F）。");
        }
        else
        {
            lines.Add("原版地形表: $00~$0F；编辑器隐藏内部 $0E/$0F；Ex功能不可用。");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private int Offset(int base16KFileOffset) => base16KFileOffset + MainBankShift;
    private int ExV2ConfigOffset => Offset(_cfg.ExV2ConfigStart);
    private static int Cpu8000FileOffset(int cpu) => 0x10 + (cpu - 0x8000);
    private int FinalRulesConfigOffset => Cpu8000FileOffset(FinalRulesConfigCpu);
    private int FinalRulesSpawnRecordOffset(int stage) => Cpu8000FileOffset(FinalRulesSpawnStartCpu + (stage - 1) * FinalRulesSpawnRecordSize);
    private int StagePlayerSpawnOffset(int stage, bool twoPlayer)
        => Cpu8000FileOffset((twoPlayer ? FinalRulesStageP2SpawnCpu : FinalRulesStageP1SpawnCpu) + stage - 1);
    private void EnsureFinalRules()
    {
        if (!HasFinalRules) throw new InvalidOperationException("当前 ROM 不支持 QXR1 Final Rules（需要 BCEX 32KB Runtime 6.5~6.9.4）。");
    }
    private void EnsureFinalRulesStage(int stage)
    {
        EnsureFinalRules();
        if (stage is < 1 or > 70) throw new ArgumentOutOfRangeException(nameof(stage), "Final Rules 出生点只支持 Stage 1~70，不包含 Demo。");
    }
    private void EnsureFinalRulesV3()
    {
        if (!SupportsFinalRulesV3) throw new InvalidOperationException("当前 ROM 不支持 QXR1 v3+ 扩展（需要 BCEX 32KB Runtime 6.6+）。");
    }
    private void EnsureFinalRulesV3Stage(int stage)
    {
        EnsureFinalRulesV3();
        if (stage is < 1 or > 70) throw new ArgumentOutOfRangeException(nameof(stage), "Runtime 6.6+ 敌人节奏只支持 Stage 1~70，不包含 Demo。");
    }
    private void EnsureFinalRulesV4()
    {
        if (!SupportsFinalRulesV4) throw new InvalidOperationException("当前 ROM 不支持 QXR1 v4+ 扩展。");
    }
    private void EnsureFinalRulesV5()
    {
        if (!SupportsFinalRulesV5) throw new InvalidOperationException("当前 ROM 不支持 QXR1 v5 扩展（需要 BCEX 32KB Runtime 6.9.3）。");
    }
    private void EnsureFinalRulesV5Stage(int stage)
    {
        EnsureFinalRulesV5();
        if (stage is < 1 or > 70) throw new ArgumentOutOfRangeException(nameof(stage), "Runtime 6.9 玩家出生点只支持 Stage 1~70，不包含 Demo。");
    }
    private void EnsureFinalRulesV6()
    {
        if (!SupportsPlayerDeathLevel) throw new InvalidOperationException("独立死亡等级需要 QXR1 v6 / Runtime 6.9.4。");
    }
    private void EnsureEnemyCounterDisplayStage(int stage)
    {
        if (!SupportsEnemyCounterDisplay) throw new InvalidOperationException("当前 ROM 不支持逐关敌人数 Icons/Number 显示（需要 Runtime 6.9.3 Hook）。");
        if (stage is < 1 or > 70) throw new ArgumentOutOfRangeException(nameof(stage), "敌人数显示只支持 Stage 1~70，不包含 Demo。");
    }
    private void EnsureFinalRulesV4Stage(int stage)
    {
        EnsureFinalRulesV4();
        if (stage is < 1 or > 70) throw new ArgumentOutOfRangeException(nameof(stage), "Runtime 6.7 老巢开关只支持 Stage 1~70，不包含 Demo。");
    }
    private int LevelPaletteOffset => Offset(_cfg.LevelPalette);

    private int TerrainAttributesOffset
    {
        get
        {
            if (IsOriginal) return _cfg.OriginalTerrainAttributes + MainBankShift;
            if (SupportsTerrain64) return _cfg.Terrain64Attributes;
            if (SupportsTerrain1F) return _cfg.OriginalTerrainAttributes + MainBankShift;
            return Offset(_cfg.TerrainAttributes);
        }
    }

    private int TerrainBlocksOffset
    {
        get
        {
            if (IsOriginal) return _cfg.OriginalTerrainBlocks + MainBankShift;
            if (SupportsTerrain64) return _cfg.Terrain64Blocks;
            if (SupportsTerrain1F) return Offset(_cfg.TerrainAttributes); // Phase 5 16KB: TSA starts at file $3F00.
            return Offset(_cfg.TerrainBlocks);
        }
    }

    private int GetPhysicalMapStage(int stage) => IsDemoStage(stage) ? 36 : IsOriginal ? stage : stage <= 35 ? stage : stage - 35;
    private int StageMapOffset(int physicalStage) => Offset(_cfg.StageMapStart + (physicalStage - 1) * _cfg.StageSize);

    private int EnemyTypeOffset(int stage)
    {
        if (IsDemoStage(stage)) return Offset(_cfg.EnemyType1To35 + 34 * 4);
        if (IsOriginal) return Offset(_cfg.EnemyType1To35 + (Math.Min(stage, 35) - 1) * 4);
        return stage <= 35
            ? Offset(_cfg.EnemyType1To35 + (stage - 1) * 4)
            : Offset(_cfg.EnemyType36To70 + (stage - 36) * 4);
    }

    private int EnemyCountOffset(int stage)
    {
        if (IsDemoStage(stage)) return Offset(_cfg.EnemyCount1To35 + 34 * 4);
        if (IsOriginal) return Offset(_cfg.EnemyCount1To35 + (Math.Min(stage, 35) - 1) * 4);
        return stage <= 35
            ? Offset(_cfg.EnemyCount1To35 + (stage - 1) * 4)
            : Offset(_cfg.EnemyCount36To70 + (stage - 36) * 4);
    }

    private int OverlayOffset(int physicalStage, int nibbleIndex)
        => _cfg.OverlayStart + (physicalStage - 1) * _cfg.OverlayPageSize + nibbleIndex;

    private int IndependentMapOffset(int stage, int row, int column)
        => _cfg.ExV2MapStart + (stage - 1) * _cfg.ExV2MapStageStride + row * _cfg.StorageStrideNibbles + column;

    private byte GetNibble(int baseOffset, int index)
    {
        var b = _data[baseOffset + (index >> 1)];
        return (byte)((index & 1) != 0 ? b & 0x0F : b >> 4);
    }

    private void SetNibble(int baseOffset, int index, int value)
    {
        var p = baseOffset + (index >> 1);
        var b = _data[p];
        _data[p] = (byte)((index & 1) != 0
            ? (b & 0xF0) | (value & 0x0F)
            : ((value & 0x0F) << 4) | (b & 0x0F));
    }

    private int CpuToFile(int cpu, int shift) => 0x10 + (cpu - 0xC000) + shift;

    private bool SpanEquals(int offset, ReadOnlySpan<byte> expected)
        => offset >= 0 && offset + expected.Length <= _data.Length && _data.AsSpan(offset, expected.Length).SequenceEqual(expected);

    private bool RangeEquals(int offset, ReadOnlySpan<byte> expected) => SpanEquals(offset, expected);

    private void EnsureRange(int offset, int length)
    {
        if (offset < 0 || length < 0 || offset + length > _data.Length)
            throw new InvalidDataException($"ROM 地址越界: ${offset:X4} + ${length:X4}");
    }

    private void ValidateStage(int stage)
    {
        if (stage < 1 || stage > MaxEditableStage) throw new ArgumentOutOfRangeException(nameof(stage), "关卡编号超出范围。");
    }

    private static void ValidateCell(int row, int column)
    {
        if (row is < 0 or >= 13 || column is < 0 or >= 13)
            throw new ArgumentOutOfRangeException(nameof(row), "地图坐标超出13×13范围。");
    }

    private static void ValidateEnemySlot(int slot)
    {
        if (slot is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(slot));
    }

    private void ValidateTerrainId(int id)
    {
        if (id < 0 || id >= TerrainCount) throw new ArgumentOutOfRangeException(nameof(id), "地形ID超出范围。");
    }
}
