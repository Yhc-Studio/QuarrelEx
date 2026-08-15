#!/usr/bin/env python3
from pathlib import Path
import hashlib, sys

EXPECTED_SHA256 = "a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c"

if len(sys.argv) not in (2, 3):
    print("Usage: python prepare_32k_base.py <Battle City (J).nes> [output.nes]")
    raise SystemExit(2)

src = Path(sys.argv[1])
out = Path(sys.argv[2]) if len(sys.argv) == 3 else src.with_name(src.stem + "_32K_base.nes")
data = src.read_bytes()
sha = hashlib.sha256(data).hexdigest()
if sha.lower() != EXPECTED_SHA256:
    raise SystemExit(f"Base ROM SHA-256 mismatch:\n  expected {EXPECTED_SHA256}\n  got      {sha}")

header = bytearray(data[:16])
header[4] = 2  # 32KB PRG
prg = data[0x10:0x4010]
chr_data = data[0x4010:]
result = bytes(header) + bytes(0x4000) + prg + chr_data
out.write_bytes(result)
print(f"Created: {out}")
print(f"Size: {len(result)} bytes")
