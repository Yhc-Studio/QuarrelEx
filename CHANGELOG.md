# Changelog

## QuarrelEx v1.1

### Desktop 1.1 / Web 1.6.0
- Demo map is now editable in Original and every supported Ex format. It remains a separate special map after the normal stage list and is not treated as Stage 71 by the UI.
- Demo uses the original 4-bit map storage and therefore exposes only writable terrain IDs `$00-$0D`; Enemy Type/Count shares Stage 35, matching the original game.
- Added a unified Title + Game Over Screen Editor.
- Native 8x8 title strings can be edited one CHR tile at a time.
- BATTLE / CITY / GAME / OVER use Battle City's original 32x32 magnified-glyph routine and are edited as whole source glyph slots.
- Screen editing writes only the existing ROM-native string bytes; coordinates, fixed lengths and `$FF` terminators are preserved.
- Config v3 remains Version 3 and now exports optional `Demo` and `Screens` extensions. Older v3 configs without them remain importable and preserve target-ROM data.
- Desktop adds the Screen Editor as the `F9` modeless tool window.
- No BCEX runtime/IPS change is required for this editor update.

## QuarrelEx v1.0

### Desktop 1.0 / Web 1.5.1
- Formal Save / Save As workflow.
- Undo and redo with Ctrl+Z / Ctrl+Y / Ctrl+Shift+Z.
- QuarrelExConfig v3 only, with strict preflight validation and transactional import.
- Config v3 includes stage maps and per-stage Enemy Type / Count / Total data.
- Independent desktop tool windows for Enemy, TSA, Palette, Flag TSA, Game Settings, Ex Options, and ROM Info.
- Low-resolution / high-DPI desktop layout fixes.
- Compact palette cells and graphical spawn-point editor.
- Web v1.5.1 fixes the missing refreshExOptions() regression in v1.5.

### BCEX 16KB runtime (r6.3)
- 1-255 enemy totals on supported ROMs.
- Extended terrain through $1F.
- Pistol/Lv4, downgrade-on-hit, no friendly fire, faster movement option, etc.
- Power level now persists across normal stage transitions; locked initial state only resets after a real death.

### BCEX 32KB runtime (r6.4.1)
- 70 independent maps and terrain through $3F.
- Enemy power-up pickup.
- Enemy Star gives steel-breaking bullet property.
- Enemy Grenade also triggers player explosion SFX.
- Fixed the Phase 6.4 branch offset bug ($B542 BCC now targets $B54D), preventing undefined-opcode execution and related instability.
