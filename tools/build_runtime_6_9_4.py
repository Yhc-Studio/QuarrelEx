#!/usr/bin/env python3
"""Build QuarrelEx Runtime 6.9.4 / QXR1 v6 from a private Runtime 6.9.3 ROM.

The source ROM is not distributed by the repository. This helper is for maintainers.
"""
from pathlib import Path
import argparse, hashlib, zlib

OLD_HELPER=bytes.fromhex('AD 7A EF 29 04 F0 19 BD 01 01 F0 14 C9 63 F0 06 38 E9 20 4C BE FF A9 60 9D 01 01 95 A8 4C 86 FF')
NEW_HELPER=bytes.fromhex('BD 01 01 CD 6B B5 90 18 C9 63 F0 06 38 E9 20 4C BA FF A9 60 9D 01 01 95 A8 4C 86 FF EA EA EA EA')
assert len(NEW_HELPER)==32

def build(src:Path,out:Path):
    b=bytearray(src.read_bytes())
    if len(b)!=40976: raise SystemExit('Expected a 40976-byte Runtime 6.9.3 ROM.')
    if b[0x356F:0x3574]!=b'QXR1\x05': raise SystemExit('QXR1 v5 signature not found at $356F.')
    if b[0x7FB6:0x7FB6+len(OLD_HELPER)]!=OLD_HELPER: raise SystemExit('Runtime 6.9.3 downgrade helper signature mismatch.')
    b[0x3573]=0x06                         # QXR1 v6
    b[0x357B]=0x20                         # default Death Lv0 (Lv1+ survive/downgrade)
    b[0x7FB6:0x7FB6+32]=NEW_HELPER
    out.write_bytes(b)
    return bytes(b)

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument('source')
    ap.add_argument('output')
    a=ap.parse_args()
    b=build(Path(a.source),Path(a.output))
    print('size',len(b))
    print('CRC32',f'{zlib.crc32(b)&0xffffffff:08X}')
    print('SHA256',hashlib.sha256(b).hexdigest())

if __name__=='__main__': main()
