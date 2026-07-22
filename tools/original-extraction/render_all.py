#!/usr/bin/env python3
"""Full data-only render: preprocess (record u16 -> commands) + interpret (commands -> audio)."""
import os, struct, wave, sys
NOAMP=bool(os.environ.get('NOAMP')); NOSPEED=bool(os.environ.get('NOSPEED'))
from preprocess import preprocess, dialogue_tokens, PA, u16
CLIP=0x2560; AUDIO=0x2f70
clipoff=[struct.unpack('>I',PA[CLIP+4*k:CLIP+4*k+4])[0] for k in range(644)]
audio=PA[AUDIO:]
BANKBASE={0x00:0,0x02:0x10c//4,0x06:0x2b4//4}
VARSEL={0:(0,3),1:(0,2),2:(1,3),3:(0,1),4:(1,2),5:(2,3)}
# 0x61 pitch param -> amplitude scale (b3998: flags 4a9c/4a9e, sample loop adds/subs s/2 or s/4)
AMP={0:1.0,1:0.5,2:0.75,3:1.25,4:1.5}
# 0x66 speed: delay=0x10-([4cc2]*2+signed param); sample period = 372+10*delay cpu cycles
# (loop cycle count approx). Container stays 13000 Hz = calibrated pitch at default delay.
CC2=-1  # word[0x4cc2] observed 0xffff in emulator
DELAY0=0x10-(CC2*2)   # delay with no 0x66 seen (=18)
def delay_of(p):
    sp = p-256 if p>=128 else p
    return max(0, 0x10-(CC2*2+sp))
def interpret(cmds):
    out=bytearray(); i=0
    amp=1.0; delay=DELAY0
    def emit(seg):
        n=len(seg)
        if n==0: return
        m=max(1,int(round(n*(372+10*delay)/(372+10*DELAY0))))
        for k in range(m):
            x=seg[min(n-1,int(k*n/m))]
            if amp!=1.0:
                x=int((x-128)*amp)+128
                x=0 if x<0 else (255 if x>255 else x)
            out.append(x)
    while i<len(cmds):
        op=cmds[i]
        if op==0x20: out+=b'\x80'*350; i+=1
        elif op==0x2e: out+=b'\x80'*1400; i+=1
        elif op==0x23: i+=1
        elif op==0x61: amp=1.0 if NOAMP else AMP.get(cmds[i+1],1.5); i+=2
        elif op==0x66: delay=DELAY0 if NOSPEED else delay_of(cmds[i+1]); i+=2
        elif op in BANKBASE:
            P,V=cmds[i+1],cmds[i+2]; i+=3
            if V in VARSEL:
                s,e=VARSEL[V]; base=BANKBASE[op]; si=base+3*P+s; ei=base+3*P+e
                if 0<=si<644 and 0<=ei<644 and clipoff[ei]<=len(audio):
                    emit(audio[clipoff[si]:clipoff[ei]])
        elif op==0x04:
            # diphone onset: clip index = 0x45c/4 + P + 14*X (2D matrix, 14 vowel cols)
            P,X=cmds[i+1],cmds[i+2]; i+=3
            idx=0x45c//4 + P + 14*X
            if 0<=idx<643 and clipoff[idx+1]<=len(audio):
                emit(audio[clipoff[idx]:clipoff[idx+1]])
        else: i+=1
    return out
def render(i):
    return interpret(preprocess(dialogue_tokens(i)))
def wav(pcm,path,rate=13000):
    w=wave.open(path,'wb'); w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate)
    w.writeframes(b''.join(struct.pack('<h',(x-128)*256) for x in pcm)); w.close()
if __name__=='__main__':
    i=int(sys.argv[1]) if len(sys.argv)>1 else 0
    import os; out=os.path.join(os.path.dirname(os.path.abspath(__file__)),f'dialogues/rendered/dlg{i:03d}.wav')
    pcm=render(i); wav(pcm,out); print(f'dlg{i:03d}: {len(pcm)} samples -> {out}')
