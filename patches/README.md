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

## 32KB BCEX — Runtime 6.9.2

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
32KB/QuarrelEx_BCEX_32KB_Runtime6.9.2.ips
```

Expected result:

```text
Size:    40976 bytes
CRC32:   246F6679
MD5:     fe96c57fc2d1f7159f0631dac448a6d3
SHA-1:   1364ef8026d4538a3bfa1117c13a577f19308723
SHA-256: 292e7e56709b6c387059a1bd49736800bd4919004de4110be630e0aa45530c95
```

## Mid City2 compatibility

### Mid City2 source

```text
Size:    40976 bytes
CRC32:   111BD2F7
SHA-256: 4a5db45617c769e788f3dedf7b5e6f438e11a75ec46a8858741aede02188501c
```

Apply:

```text
compatibility/MidCity2_to_QuarrelEx_Runtime6.9.2.ips
```

Expected:

```text
CRC32:   BE6298A5
SHA-256: d999dee312f62f0ce10fbbdf808cc956f3c06186e7df1bddde94e1853ecf9339
```

### Mid City2 PS source

```text
Size:    40976 bytes
CRC32:   1EE13F42
SHA-256: d2caa53c09a60016a0b0e4ccf49fe4cc49a8761da00a6068287bfc93f6d632f2
```

Apply:

```text
compatibility/MidCity2_PS_to_QuarrelEx_Runtime6.9.2.ips
```

Expected:

```text
CRC32:   A7D92E53
SHA-256: 1422fc41243264da2a2444fe5872c00ba2df67b525845601d563529a87da9b15
```

Always keep a backup and verify hashes before applying a patch.
