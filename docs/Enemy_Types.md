# Enemy Type notes

Enemy Type is an 8-bit value. Common base values used by the editor are `$80`, `$A0`, `$C0`, and `$E0`.

The `$04` bit is the flashing / bonus-tank flag. Therefore examples are:

| Normal | Flashing / bonus |
|---|---|
| `$80` | `$84` |
| `$A0` | `$A4` |
| `$C0` | `$C4` |
| `$E0` | `$E4` |

`$04` by itself should not be treated as a complete enemy class. It is a flag added to a normal Enemy Type.

The original game can also add the bonus flag automatically at specific spawn points, so a table value without `$04` can still produce a flashing tank during normal play.
