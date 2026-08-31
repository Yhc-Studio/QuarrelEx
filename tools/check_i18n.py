#!/usr/bin/env python3
"""Static localization consistency checks for QuarrelEx Desktop/Web/Mobile."""
from pathlib import Path
import json, re, sys
ROOT=Path(__file__).resolve().parents[1]
LANGS=('zh-CN','en-US','ja-JP')
cats={x:json.loads((ROOT/'locales'/f'{x}.json').read_text(encoding='utf-8')) for x in LANGS}
base=set(cats['zh-CN'])
errors=[]
for lang in LANGS:
    keys=set(cats[lang])
    if keys!=base:
        errors.append(f'{lang}: key mismatch: missing={sorted(base-keys)}, extra={sorted(keys-base)}')
    empty=[k for k,v in cats[lang].items() if not isinstance(v,str) or not v.strip()]
    if empty: errors.append(f'{lang}: empty values: {empty}')
placeholder=re.compile(r'\{\d+\}')
for key in sorted(base):
    ref=sorted(placeholder.findall(cats['zh-CN'][key]))
    for lang in ('en-US','ja-JP'):
        got=sorted(placeholder.findall(cats[lang][key]))
        if got!=ref: errors.append(f'{key}: placeholder mismatch zh-CN={ref}, {lang}={got}')
used=set()
for p in (ROOT/'desktop'/'QuarrelEx').rglob('*.cs'):
    t=p.read_text(encoding='utf-8-sig',errors='ignore')
    used.update(re.findall(r'I18n\.T\(\s*"([^"]+)"',t))
for p in (ROOT/'web'/'QuarrelEx.html',ROOT/'web'/'QuarrelEx_Standalone.html',ROOT/'web'/'QuarrelEx_Mobile.html'):
    t=p.read_text(encoding='utf-8')
    t=re.sub(r'<!-- QX_I18N_BEGIN -->.*?<!-- QX_I18N_END -->','',t,flags=re.S)
    used.update(re.findall(r'\b(?:uiT|uiF|qxT|qxFormat)\(\s*["\']([^"\']+)["\']',t))
missing=sorted(used-base)
if missing: errors.append(f'direct source key(s) missing from catalogs: {missing}')

# Desktop must carry a compile-time catalog that matches the canonical JSON.
generated = ROOT/'desktop'/'QuarrelEx'/'Localization'/'BuiltInCatalogs.g.cs'
if not generated.exists():
    errors.append('Desktop built-in localization payload is missing: Localization/BuiltInCatalogs.g.cs')
else:
    generated_text = generated.read_text(encoding='utf-8', errors='ignore')
    import hashlib
    for name in ('source-keys','zh-CN','en-US','ja-JP'):
        raw=(ROOT/'locales'/f'{name}.json').read_bytes()
        sha=hashlib.sha256(raw).hexdigest()
        if sha not in generated_text:
            errors.append(f'Desktop built-in localization payload is stale for {name}.json; run tools/generate_desktop_i18n.py')

print(f'Locale catalogs: {len(base)} keys each')
print(f'Direct key references checked: {len(used)}')
if errors:
    print('FAIL')
    for e in errors: print(' -',e)
    sys.exit(1)
print('PASS')
