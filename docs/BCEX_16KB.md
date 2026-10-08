# BCEX 16KB

The 16KB patch keeps the original Battle City ROM size while enabling the maintained BCEX feature set that fits the 16KB line.

## Current patch

```text
patches/16KB/QuarrelEx_BCEX_16KB_v1.0.ips
```

Required input:

```text
Battle City (J)
Size:    24592 bytes
CRC32:   F599A07E
SHA-256: a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c
```

Expected output:

```text
Size:    24592 bytes
CRC32:   AECB82CE
SHA-256: 33d51720a9891b6eb4a835b8fd9c4181c3689452456aaa417914e3dbb3427939
```

The 16KB line does not provide the full 70-map 32KB feature set. QuarrelEx detects ROM capabilities and disables unsupported editor controls automatically.
