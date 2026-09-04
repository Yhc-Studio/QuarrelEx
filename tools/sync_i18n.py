#!/usr/bin/env python3
"""Refresh only the embedded locale catalogs while preserving each UI's current i18n runtime."""
from pathlib import Path
import json,re
ROOT=Path(__file__).resolve().parents[1]
LOCALES=ROOT/'locales'
FILES=[ROOT/'web'/'QuarrelEx.html',ROOT/'web'/'QuarrelEx_Mobile.html']
langs={name:json.loads((LOCALES/f'{name}.json').read_text(encoding='utf-8')) for name in ('zh-CN','en-US','ja-JP')}
source=json.loads((LOCALES/'source-keys.json').read_text(encoding='utf-8'))
cat=json.dumps(langs,ensure_ascii=False,separators=(',',':'))
src=json.dumps(source,ensure_ascii=False,separators=(',',':'))
pattern=re.compile(r'(const QX_I18N_CATALOG=).*?(;\s*\n\s*const QX_I18N_SOURCE_KEYS=).*?(;)',re.S)
for path in FILES:
    text=path.read_text(encoding='utf-8')
    text,count=pattern.subn(lambda m:m.group(1)+cat+m.group(2)+src+m.group(3),text,count=1)
    if count!=1: raise SystemExit(f'Could not locate embedded catalogs in {path}')
    path.write_text(text,encoding='utf-8')
    print('synced',path.relative_to(ROOT))
