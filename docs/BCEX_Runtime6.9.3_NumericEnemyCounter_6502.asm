; BCEX Runtime 6.9.3 / QXR1 v5 - Numeric Enemy Counter
; =========================================================
;
; Per-stage preference is stored in PackedStageRules bit3:
;
;   CPU $BEEC + stage-1
;   bit3 = 0 : Icons preference
;   bit3 = 1 : Number preference
;
; Runtime rule:
;   EnemyTotal 1..50  -> use the stored preference
;   EnemyTotal 51..255 -> force Number
;
; Demo ($46 == $02) always keeps the original icon display.
;
; The numeric value has the same meaning as the original icon counter:
; RAM $7F = enemies still waiting to spawn.
;
; New runtime hooks
; -----------------
; CPU $C377:
;   old  JSR $C8C0
;   new  JSR $C76A       ; select Icons/Number and draw initial value
;
; CPU $DB68:
;   old  JSR $C76A       ; old >=20 icon saturation helper
;   new  JSR $C790       ; update Icons or Number
;
; Counter dispatcher / update code: CPU $C76A-$C79E
; Decimal number renderer:           CPU $C000-$C054
; Correct icon initializer:          CPU $C055-$C06A
;
; $C000-$C06F was unreachable garbage ASCII before the reset entry at $C070.
;
; Numeric display uses X=$1C, Y=$03 and tiles:
;   blank/gray = $11
;   digit 0    = $6E
;   digit 9    = $77
;
; No leading zero:
;   5   -> "  5"
;   20  -> " 20"
;   100 -> "100"
;   255 -> "255"
;
; The icon initializer also corrects totals below 20:
; it draws the original 20 markers and erases markers total..19 so a stage
; with EnemyTotal=10 begins with 10 visible icons rather than 20.
;
; --------------------------------------------------------------------
; Initial / update dispatcher at $C76A
; --------------------------------------------------------------------
.org $C76A
InitEnemyCounter:
    LDA $46
    CMP #$02
    BEQ UseIcons            ; Demo stays original

    LDA $7F
    CMP #$33                ; 51
    BCS UseNumber

    LDX $85
    DEX
    LDA $BEEC,X
    AND #$08
    BNE UseNumber

UseIcons:
    LDA #$00
    STA $81                 ; runtime mode flag: 0=icons
    JMP DrawInitialIcons

UseNumber:
    LDA #$01
    STA $81                 ; runtime mode flag: 1=number
    LDA $7F
    JMP DrawEnemyNumber

UpdateEnemyCounter:
    LDX $81
    BNE UpdateNumber

    CMP #$14                ; icon mode saturates at 20
    BCS CounterReturn
    JMP $C8B1               ; original erase-one-icon routine

CounterReturn:
    RTS

UpdateNumber:
    JMP DrawEnemyNumber

; --------------------------------------------------------------------
; Numeric decimal renderer in unused pre-reset garbage area.
; Input A = 0..255.
; --------------------------------------------------------------------
.org $C000
DrawEnemyNumber:
    LDX #$00
HundredsLoop:
    CMP #$64
    BCC HundredsDone
    SBC #$64                ; CMP left carry set
    INX
    JMP HundredsLoop

HundredsDone:
    STX $01
    LDY #$00

TensLoop:
    CMP #$0A
    BCC TensDone
    SBC #$0A
    INY
    JMP TensLoop

TensDone:
    CLC
    ADC #$6E
    STA $3B                 ; ones tile

    LDA #$11
    STA $39                 ; blank hundreds
    STA $3A                 ; blank tens

    LDA $01
    BEQ NoHundreds
    CLC
    ADC #$6E
    STA $39
    TYA
    CLC
    ADC #$6E
    STA $3A
    JMP NumberReady

NoHundreds:
    TYA
    BEQ NumberReady
    CLC
    ADC #$6E
    STA $3A

NumberReady:
    LDA #$FF
    STA $3C

    LDA #$39
    STA $11
    LDA #$00
    STA $12
    STA $60                 ; tiles are already final tile IDs

    LDX #$1C
    LDY #$03
    JSR $D6DD
    RTS

; --------------------------------------------------------------------
; Icon mode initial draw at $C055.
; Original C8C0 always draws 20.  Remove excess markers when total < 20.
; --------------------------------------------------------------------
.org $C055
DrawInitialIcons:
    JSR $C8C0
    LDA #$13                ; index 19
    STA $5A
EraseExtraIcons:
    LDA $5A
    CMP $7F
    BCC IconsReady
    JSR $C8B1
    DEC $5A
    JMP EraseExtraIcons
IconsReady:
    RTS
