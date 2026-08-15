param(
    [Parameter(Mandatory=$true)][string]$InputRom,
    [string]$OutputRom = ""
)

$ExpectedSha256 = "A869AEAD5B6957FC62002FF9636E048CC34BAF0100D629B07DC51AA18F220C0C"
$actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $InputRom).Hash.ToUpperInvariant()
if ($actual -ne $ExpectedSha256) {
    throw "Base ROM SHA-256 mismatch. Expected $ExpectedSha256, got $actual"
}

if ([string]::IsNullOrWhiteSpace($OutputRom)) {
    $dir = Split-Path -Parent $InputRom
    $name = [System.IO.Path]::GetFileNameWithoutExtension($InputRom)
    $OutputRom = Join-Path $dir ($name + "_32K_base.nes")
}

[byte[]]$src = [System.IO.File]::ReadAllBytes($InputRom)
[byte[]]$result = New-Object byte[] 40976
[Array]::Copy($src, 0, $result, 0, 16)
$result[4] = 2
# bytes 0x10..0x400F stay zero as the new expansion PRG bank
[Array]::Copy($src, 0x10, $result, 0x4010, 0x4000)
[Array]::Copy($src, 0x4010, $result, 0x8010, 0x2000)
[System.IO.File]::WriteAllBytes($OutputRom, $result)
Write-Host "Created: $OutputRom"
Write-Host "Size: $($result.Length) bytes"
