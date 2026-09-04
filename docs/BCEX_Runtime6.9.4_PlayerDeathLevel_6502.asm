; QuarrelEx BCEX Runtime 6.9.4 / QXR1 v6
; Independent player Death Level
;
; QXR1 v6 byte:
;   $B56B = first raw tank-state value that survives a hit
;   Lv0 -> $20, Lv1 -> $40, Lv2 -> $60, Lv3 -> $63, Lv4 -> $64
;
; Hook/helper replacement: CPU $FFA6-$FFC5

FFA6: BD 01 01    LDA $0101,X
FFA9: CD 6B B5    CMP $B56B
FFAC: 90 18       BCC $FFC6      ; below cutoff -> original death path
FFAE: C9 63       CMP #$63       ; special Lv4 raw state
FFB0: F0 06       BEQ $FFB8
FFB2: 38          SEC
FFB3: E9 20       SBC #$20       ; normal one-level downgrade
FFB5: 4C BA FF    JMP $FFBA
FFB8: A9 60       LDA #$60       ; Lv4 -> Lv3
FFBA: 9D 01 01    STA $0101,X
FFBD: 95 A8       STA $A8,X
FFBF: 4C 86 FF    JMP $FF86      ; survive hit
FFC2: EA EA EA EA NOP / padding

FFC6: A9 73       LDA #$73       ; existing original death path begins here
