# QuarrelEx IPS patches

No ROM images are included.

## Standard Battle City base ROM

```text
Battle City (J)
Size:    24592 bytes
CRC32:   F599A07E
MD5:     cd4fe2e78df0696dbe652f02c19541a1
SHA-1:   e1061c9241b06a965fb7845cb951d921aca010ef
SHA-256: a869aead5b6957fc62002ff9636e048cc34baf0100d629b07dc51aa18f220c0c
```

## 16KB BCEX

Apply directly:

```text
16KB/QuarrelEx_BCEX_16KB_v1.0.ips
```

Expected result:

```text
Size:    24592 bytes
CRC32:   AECB82CE
SHA-256: 33d51720a9891b6eb4a835b8fd9c4181c3689452456aaa417914e3dbb3427939
```

## 32KB BCEX — Runtime 6.9.3

Prepare the 32KB base first:

```text
32KB\prepare_32k_base.bat "Battle City (J).nes"
```

or:

```text
python 32KB/prepare_32k_base.py "Battle City (J).nes"
```

Expected prepared base:

```text
Size:    40976 bytes
CRC32:   C24D701C
SHA-256: 1c561a7a11162171d8b55551fdfd3d56ab796a27364a252eaf8971cf554d9094
```

Then apply:

```text
32KB/QuarrelEx_BCEX_32KB_Runtime6.9.3.ips
```

Expected result:

```text
Size:    40976 bytes
CRC32:   F6FF962E
MD5:     51b4972874660a6229997d36222914b2
SHA-1:   e341ac4a2eca87f9feaf2d8bf746fc65e5551f65
SHA-256: ce0922dabd1984863dfc5958f26f3aef1409c4e4aaa769343c0e23d606ced399
```
