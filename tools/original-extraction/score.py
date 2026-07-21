#!/usr/bin/env python3
import glob, json, unicodedata, re, os
from difflib import SequenceMatcher
HERE=os.path.dirname(os.path.abspath(__file__))
def norm(s):
    s=unicodedata.normalize('NFD',s.lower())
    s=''.join(c for c in s if unicodedata.category(c)!='Mn')
    s=re.sub(r'[^a-z0-9 ]',' ',s)
    return s.split()
HALLU=['sous-titr','merci d','abonnez','amara.org','sous titr']
known=[norm(k) for k in json.load(open(os.path.join(HERE,'data/known_texts.json')))]
renders=[]
for f in sorted(glob.glob(os.path.join(HERE,'dialogues/rendered/*.txt'))):
    t=open(f).read().strip()
    if any(h in t.lower() for h in HALLU): t=''
    renders.append((f.split('/')[-1], norm(t)))
def sim(a,b):
    if not a or not b: return 0.0
    return SequenceMatcher(None,a,b).ratio()
# greedy best 1:1
pairs=[]
used=set()
scored=[]
for name,r in renders:
    best=0; bi=-1
    for i,k in enumerate(known):
        if i in used: continue
        s=sim(r,k)
        if s>best: best=s; bi=i
    if bi>=0: used.add(bi)
    scored.append((name,best,bi))
sims=[s for _,s,_ in scored]
mean=sum(sims)/len(sims)
print(f'{len(renders)} renders vs {len(known)} known')
print(f'MEAN similarity (greedy 1:1): {mean:.3f}  (target >= 0.55)')
print(f'>=0.55: {sum(1 for s in sims if s>=0.55)}/{len(sims)}')
print(f'>=0.4:  {sum(1 for s in sims if s>=0.4)}/{len(sims)}')
top=sorted(scored,key=lambda x:-x[1])[:8]
print('best matches:')
for n,s,i in top: print(f'  {n} {s:.2f}')
