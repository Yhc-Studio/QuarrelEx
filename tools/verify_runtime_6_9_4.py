#!/usr/bin/env python3
from pathlib import Path
import argparse, hashlib, zlib
HELPER=bytes.fromhex('BD 01 01 CD 6B B5 90 18 C9 63 F0 06 38 E9 20 4C BA FF A9 60 9D 01 01 95 A8 4C 86 FF EA EA EA EA')
ap=argparse.ArgumentParser(); ap.add_argument('rom'); a=ap.parse_args()
b=Path(a.rom).read_bytes()
assert len(b)==40976, len(b)
assert b[0x356F:0x3574]==b'QXR1\x06'
assert b[0x357B] in (0x20,0x40,0x60,0x63,0x64)
assert b[0x7FB6:0x7FB6+32]==HELPER
print('PASS')
print('Death cutoff',hex(b[0x357B]))
print('CRC32',f'{zlib.crc32(b)&0xffffffff:08X}')
print('SHA256',hashlib.sha256(b).hexdigest())
