# Publishing QuarrelEx to GitHub

Recommended repository name:

```text
QuarrelEx
```

Recommended tag:

```text
v1.1.8
```

Suggested commit:

```bash
git add .
git commit -m "QuarrelEx v1.1.8"
git push
```

Recommended Release assets:

```text
QuarrelEx_BCEX_16KB_v1.0.ips
QuarrelEx_BCEX_32KB_Runtime6.9.4.ips
QuarrelEx_Web_v1.6.8_Standalone.html
QuarrelEx_Mobile_v1.0_Core1.6.8.html
QuarrelEx_Desktop_v1.1.8_Source.zip
SHA256SUMS.txt
```

The repository keeps only one desktop-oriented Web source file: `web/QuarrelEx.html`. It is already self-contained and offline-capable. For a GitHub Release, copy/rename that file to `QuarrelEx_Web_v1.6.8_Standalone.html` instead of committing a byte-identical duplicate to the repository.

Do not upload a Battle City ROM image.
