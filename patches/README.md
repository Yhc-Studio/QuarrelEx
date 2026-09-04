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

## 32KB BCEX — Runtime 6.9.4 / QXR1 v6

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
32KB/QuarrelEx_BCEX_32KB_Runtime6.9.4.ips
```

Expected result:

```text
Size:    40976 bytes
CRC32:   F8EBC1BD
MD5:     6006f188a90ff69ea0c66a880302aa77
SHA-1:   76711e777021362aa7e184f0156275e3aa7d95ab
SHA-256: 826952e117211cbe3b4df2fd51b0f74a4fc1fbeaa9360727b2f248575f0e5284
```

Runtime 6.9.4 keeps the 32KB Mapper-0 layout and upgrades QXR1 to v6. It adds an independent player Death Level while retaining the existing Initial Tank Level.

## Runtime 6.9.3 -> 6.9.4 incremental patch

For a ROM that already exactly matches the standard Runtime 6.9.3 final build:

```text
Input CRC32:   F6FF962E
Input SHA-256: ce0922dabd1984863dfc5958f26f3aef1409c4e4aaa769343c0e23d606ced399
```

apply:

```text
32KB/BCEX_Runtime6.9.3_to_6.9.4_PlayerDeathLevel.ips
```

The output is byte-identical to the standard Runtime 6.9.4 result above.
