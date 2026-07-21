#!/usr/bin/env python3
"""Full data-only render: preprocess (record u16 -> commands) + interpret (commands -> audio)."""
import struct, wave, sys
from preprocess import preprocess, dialogue_tokens, PA, u16
CLIP=0x2560; AUDIO=0x2f70
clipoff=[struct.unpack('>I',PA[CLIP+4*k:CLIP+4*k+4])[0] for k in range(644)]
audio=PA[AUDIO:]
BANKBASE={0x00:0,0x02:0x10c//4,0x06:0x2b4//4}
VARSEL={0:(0,3),1:(0,2),2:(1,3),3:(0,1),4:(1,2),5:(2,3)}
def interpret(cmds):
    out=bytearray(); i=0
    while i<len(cmds):
        op=cmds[i]
        if op==0x20: out+=b'\x80'*350; i+=1
        elif op==0x2e: out+=b'\x80'*1400; i+=1
        elif op==0x23: i+=1
        elif op==0x61: i+=2
        elif op==0x66: i+=2
        elif op in BANKBASE:
            P,V=cmds[i+1],cmds[i+2]; i+=3
            if V in VARSEL:
                s,e=VARSEL[V]; base=BANKBASE[op]; si=base+3*P+s; ei=base+3*P+e
                if 0<=si<644 and 0<=ei<644 and clipoff[ei]<=len(audio):
                    out+=audio[clipoff[si]:clipoff[ei]]
        elif op==0x04:
            i+=3  # bank4 onset: skip (minor)
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
