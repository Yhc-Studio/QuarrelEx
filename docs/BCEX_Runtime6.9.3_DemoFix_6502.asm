; BCEX Runtime 6.9.3 - Demo auto-fire isolation fix
; ==================================================
;
; Root cause
; ----------
; Original Demo AI generates directional input + both fire buttons:
;
;   C6C2: 13 43 23 83
;
; Original Battle City stops firing near the base at Y >= $C8 by clearing
; the low nibble ONLY from ram_btn_press:
;
;   C6B7  LDA $08,X
;   C6B9  AND #$F0
;   C6BB  STA $08,X
;
; The BCEX auto-fire feature uses held-B as an additional fire source:
;
;   EF7D  LDA feature flags
;   ...
;   EF84  LDA $06,X       ; ram_btn_hold
;   EF86  AND #$02        ; held B
;   EF88  ORA $08,X       ; OR normal press
;
; Because Demo's ram_btn_hold still contained B=$02, the later auto-fire
; helper reintroduced firing even though the original Demo AI had deliberately
; removed fire from ram_btn_press near the base.
;
; Fix
; ---
; Demo does not need both A and B.  The original shooting code accepts either.
; Remove only B from the four Demo AI input table entries, keep A + direction.
;
.org $C6C2
    .byte $11, $41, $21, $81
;
; Before:
;   13 43 23 83
;
; After:
;   11 41 21 81
;
; Effect:
; - away from the base, A remains set -> Demo still shoots normally;
; - at Y >= $C8, original C6B7-C6BB masks A from ram_btn_press;
; - held-B is no longer present, so BCEX auto-fire cannot re-enable shooting;
; - Demo therefore regains the original no-fire protection near the HQ.
;
; CPU range:      $C6C2-$C6C5
; iNES file range $46D2-$46D5
