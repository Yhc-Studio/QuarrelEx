#!/usr/bin/env python3
"""Embed canonical root locale catalogs into Web/Standalone/Mobile entry points."""
from pathlib import Path
import json, re
ROOT=Path(__file__).resolve().parents[1]
LOCALES=ROOT/'locales'
FILES=[ROOT/'web'/'QuarrelEx.html',ROOT/'web'/'QuarrelEx_Standalone.html',ROOT/'web'/'QuarrelEx_Mobile.html']
langs={name:json.loads((LOCALES/f'{name}.json').read_text(encoding='utf-8')) for name in ('zh-CN','en-US','ja-JP')}
source=json.loads((LOCALES/'source-keys.json').read_text(encoding='utf-8'))
start='<!-- QX_I18N_BEGIN -->'; end='<!-- QX_I18N_END -->'
template=r'''<!-- QX_I18N_BEGIN -->
<script>
'use strict';
(() => {
  const QX_I18N_CATALOG=__CATALOG__;
  const QX_I18N_SOURCE_KEYS=__SOURCE__;
  const QX_LANG_STORAGE='quarrelex.uiLanguage';
  const textBindings=new WeakMap(), attrBindings=new WeakMap();
  let applying=false;
  function normalizeLanguage(code){code=String(code||'');if(code.toLowerCase().startsWith('zh'))return 'zh-CN';if(code.toLowerCase().startsWith('ja'))return 'ja-JP';return 'en-US';}
  function readStoredLanguage(){try{return localStorage.getItem(QX_LANG_STORAGE)||'';}catch{return '';}}
  function writeStoredLanguage(code){try{localStorage.setItem(QX_LANG_STORAGE,code);}catch{}}
  let currentLanguage=normalizeLanguage(readStoredLanguage()||navigator.language);
  function qxT(key){return QX_I18N_CATALOG[currentLanguage]?.[key]??QX_I18N_CATALOG['en-US']?.[key]??QX_I18N_CATALOG['zh-CN']?.[key]??key;}
  function qxFormat(key,args=[]){return qxT(key).replace(/\{(\d+)\}/g,(m,n)=>args[Number(n)]??m);}
  function escapeRegex(s){return s.replace(/[.*+?^${}()|[\]\\]/g,'\\$&');}

  const exactKeys=new Map(Object.entries(QX_I18N_SOURCE_KEYS));
  for(const catalog of Object.values(QX_I18N_CATALOG)){
    const counts=new Map();
    for(const text of Object.values(catalog)) if(text) counts.set(text,(counts.get(text)||0)+1);
    for(const [key,text] of Object.entries(catalog)) if(text&&counts.get(text)===1&&!exactKeys.has(text)) exactKeys.set(text,key);
  }

  function buildSourcePattern(key,text){
    const argIds=[],seenArgs=new Set();let rx='',last=0,m;const re=/\{(\d+)\}/g;
    while((m=re.exec(text))){
      rx+=escapeRegex(text.slice(last,m.index));const id=Number(m[1]);
      if(seenArgs.has(id))rx+='\\k<a'+id+'>';
      else{seenArgs.add(id);argIds.push(id);rx+='(?<a'+id+'>.*?)';}
      last=m.index+m[0].length;
    }
    if(!argIds.length)return null;
    rx+=escapeRegex(text.slice(last));
    return {key,argIds,argCount:Math.max(...argIds)+1,regex:new RegExp('^'+rx+'$','s'),length:text.length};
  }
  const patternSeen=new Set(),sourcePatterns=[];
  for(const catalog of Object.values(QX_I18N_CATALOG)){
    for(const [key,text] of Object.entries(catalog)){
      if(!text||!text.includes('{'))continue;
      const sig=key+'\u0000'+text;if(patternSeen.has(sig))continue;patternSeen.add(sig);
      const p=buildSourcePattern(key,text);if(p)sourcePatterns.push(p);
    }
  }
  sourcePatterns.sort((a,b)=>b.length-a.length);

  function identifySource(text){
    const exact=exactKeys.get(text);if(exact)return {key:exact,args:[],last:''};
    for(const p of sourcePatterns){
      const m=p.regex.exec(text);if(!m)continue;
      const args=new Array(p.argCount).fill('');
      p.argIds.forEach(argId=>{args[argId]=m.groups?.['a'+argId]??'';});
      return {key:p.key,args,last:''};
    }
    return null;
  }
  function renderBinding(b){return qxFormat(b.key,b.args);}
  function translateTextNode(node){
    const raw=node.nodeValue||'',trimmed=raw.trim();if(!trimmed)return;
    let b=textBindings.get(node);
    if(b){
      if(trimmed!==b.last){const rebound=identifySource(trimmed);if(!rebound){textBindings.delete(node);return;}b=rebound;textBindings.set(node,b);}
    }else{b=identifySource(trimmed);if(!b)return;textBindings.set(node,b);}
    const replacement=renderBinding(b);b.last=replacement;
    if(replacement===trimmed)return;
    const at=raw.indexOf(trimmed);node.nodeValue=raw.slice(0,at)+replacement+raw.slice(at+trimmed.length);
  }
  function translateAttrs(el){
    for(const attr of ['title','placeholder','aria-label']){
      if(!el.hasAttribute?.(attr))continue;const raw=(el.getAttribute(attr)||'').trim();if(!raw)continue;
      let map=attrBindings.get(el);if(!map){map={};attrBindings.set(el,map);}let b=map[attr];
      if(b){if(raw!==b.last){const rebound=identifySource(raw);if(!rebound){delete map[attr];continue;}b=rebound;map[attr]=b;}}
      else{b=identifySource(raw);if(!b)continue;map[attr]=b;}
      const replacement=renderBinding(b);b.last=replacement;if(replacement!==raw)el.setAttribute(attr,replacement);
    }
  }
  function translateRoot(root=document){
    if(applying)return;applying=true;
    try{
      if(root.nodeType===Node.TEXT_NODE)translateTextNode(root);
      else{
        if(root.nodeType===Node.ELEMENT_NODE)translateAttrs(root);
        const walker=document.createTreeWalker(root,NodeFilter.SHOW_ELEMENT|NodeFilter.SHOW_TEXT);let n;
        while((n=walker.nextNode())){if(n.nodeType===Node.TEXT_NODE){if(!['SCRIPT','STYLE'].includes(n.parentElement?.tagName))translateTextNode(n);}else translateAttrs(n);}
      }
      document.documentElement.lang=currentLanguage;
      document.querySelectorAll('[data-language-select]').forEach(s=>{s.value=currentLanguage;});
    }finally{applying=false;}
  }
  function setLanguage(code){
    currentLanguage=normalizeLanguage(code);writeStoredLanguage(currentLanguage);
    translateRoot(document);
    document.dispatchEvent(new CustomEvent('quarrelex:languagechange',{detail:{language:currentLanguage}}));
    return currentLanguage;
  }
  function localizeString(text){const raw=String(text??'');const b=identifySource(raw.trim());return b?qxFormat(b.key,b.args):text;}
  function localizeMultiline(text){return String(text??'').split(/\r?\n/).map(localizeString).join('\n');}
  const nativeAlert=window.alert.bind(window),nativeConfirm=window.confirm.bind(window);
  window.alert=message=>nativeAlert(localizeMultiline(message));
  window.confirm=message=>nativeConfirm(localizeMultiline(message));
  window.QuarrelExI18n={t:qxT,format:(key,...args)=>qxFormat(key,args),setLanguage,get language(){return currentLanguage;},localizeString,localizeMultiline,apply:translateRoot};
  document.addEventListener('change',e=>{if(e.target?.matches?.('[data-language-select]'))setLanguage(e.target.value);});
  const observer=new MutationObserver(records=>{if(applying)return;for(const r of records){for(const n of r.addedNodes)translateRoot(n);if(r.type==='characterData')translateRoot(r.target);}});
  observer.observe(document.documentElement,{subtree:true,childList:true,characterData:true});
  translateRoot(document);
})();
</script>
<!-- QX_I18N_END -->'''
js=template.replace('__CATALOG__',json.dumps(langs,ensure_ascii=False,separators=(',',':'))).replace('__SOURCE__',json.dumps(source,ensure_ascii=False,separators=(',',':')))
for path in FILES:
    text=path.read_text(encoding='utf-8')
    if start in text:
        text=re.sub(re.escape(start)+r'.*?'+re.escape(end),lambda m:js,text,flags=re.S)
    else:
        marker="<script>\n'use strict';\n\n(() => {\n  const { BattleCityRom"
        pos=text.find(marker)
        if pos<0: raise SystemExit(f'Could not locate main app script in {path}')
        text=text[:pos]+js+'\n\n'+text[pos:]
    path.write_text(text,encoding='utf-8')
    print('synced',path.relative_to(ROOT))
