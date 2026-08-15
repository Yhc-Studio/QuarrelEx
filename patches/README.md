# BCEX IPS patches

No ROM images are included. Apply exactly one patch to the supported clean base ROM.

## Required base ROM

```text
Battle City (J)
Size:    24592 bytes
CRC32:   F599A07E
MD5:     cd4fe2e78df0696dbe652f02c19541a1
SHA-1:   e1061c9241b06a965fb7845cb951d921aca010ef
SHA-256: a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c
```

## 16KB BCEX

Patch:

```text
16KB/QuarrelEx_BCEX_16KB_v1.0.ips
```

Expected patched ROM:

```text
Size:    24592 bytes
CRC32:   AECB82CE
MD5:     116af2e87dfbafa40d945564afdac0ff
SHA-1:   ea64679211533f00f08757fb90c4a236ed836ca5
SHA-256: 33d51720a9891b6eb4a835b8fd9c4181c3689452456aaa417914e3dbb3427939
```

Internal runtime revision: 6.3.

## 32KB BCEX

For the 32KB build, first create a legal 32KB working base from your own clean ROM. This avoids storing a near-complete relocated copy of the original PRG data inside the IPS patch.

### Step 1 — prepare the 32KB base

Windows:

```text
32KB\prepare_32k_base.bat "Battle City (J).nes"
```

Cross-platform Python:

```text
python 32KB/prepare_32k_base.py "Battle City (J).nes"
```

Expected prepared base:

```text
Size:    40976 bytes
CRC32:   C24D701C
SHA-256: 1c561a7a11162171d8b55551fdfd3d56ab796a27364a252eaf8971cf554d9094
```

### Step 2 — apply the IPS

Patch the prepared `_32K_base.nes` with:

```text
32KB/QuarrelEx_BCEX_32KB_v1.0.ips
```

Expected patched ROM:

```text
Size:    40976 bytes
CRC32:   A632130F
MD5:     c8c3014a5656816e84730d726c270422
SHA-1:   388716536405f1d18b85c117fce9263f764fb22e
SHA-256: 1f056fccfe6fb1df98262526080ae88980df5eaaa3e076a20e06be2e88a494e9
```

Internal runtime revision: 6.4.1.

## Applying

Use an IPS-compatible patcher such as Floating IPS (Flips) or another trusted IPS tool. Always make a backup of your source ROM first.
