# French pronunciation dependency

Rody uses eSpeak NG pronunciation through the ephone fork, retaining whole-sentence
French rules and source alignment. No speech synthesis from this dependency is used.
Rody owns phoneme rendering, punctuation pauses and manual corrections.

## Provenance and licensing

- Native source: https://github.com/sjmik/ephone-js at
  `4f6d246c1d3acf67a4d814e20da02fa3967bc92d` (upstream version 1.52.0.1).
- Browser distribution: npm `ephone@1.0.2`, unmodified `ephone.js` renamed
  `ephone.mjs`, plus its unmodified Romance language pack `lang/roa.js`.
- Both inherit GPL-3.0-or-later. Full license is in `COPYING` and the shipped
  `StreamingAssets/RodyFrench/COPYING.txt`. Unicode data terms ship in `COPYING.Unicode.txt`. Source archive acquisition is pinned and
  checksum-verified by `build-macos.sh`. Retain these notices when distributing.
- The tiny `free.c` and `RodyFrench.jslib` are Rody integration code. Upstream
  pronunciation rules and dictionary contents have not been patched.

## Rebuild

On macOS with Xcode command-line tools, CMake, curl and tar:

```sh
tools/french-converter/build-macos.sh
```

This compiles only the native dependency, never Unity. It produces a universal
arm64/x86_64 `libRodyFrench.dylib`, copies the browser distribution, and installs
only native French/English dictionaries, French/English-US voice definitions,
`phontab` and `phonindex`. English is eSpeak's built-in pronunciation fallback
for some foreign words. No online service is contacted at runtime.

The browser uses its bundled WASM; native and browser engines are initialized
once and invoked synchronously once ready. Native support is currently macOS
(Editor and player). Windows/Linux native binaries are not supplied.

## Native ABI

Library name: `RodyFrench`.

- `int espeak_InitForTextToIpa(const char *voice, const char *path)`:
  voice `roa/fr`; path `Application.streamingAssetsPath + "/RodyFrench"`.
  Return zero means success. UTF-8, null-terminated strings.
- `char *espeak_TextToIpaWithSourceMap(const char **text, int *map, int capacity)`:
  initialize first; pass the address of a UTF-8 text pointer. Returns newly
  allocated UTF-8 IPA. Advances the text pointer while consuming the sentence.
- `void RodyFrench_Free(void *result)`: free returned IPA after copying it.
- `void espeak_TerminateForTextToIpa(void)`: optional application shutdown.

`map` contains consecutive `(sourceIndex, ipaIndex)` pairs, ending at negative
sentinel. Capacity is number of **ints**, not pairs. Reserve at least twice the
UTF-8 byte count plus two ints for a safe bound for input capped at 2000 characters;
upstream suggests twice the word count but punctuation/number expansion needs room.
A full buffer silently truncates mappings. Offsets count Unicode code points,
not UTF-8 bytes or UTF-16 units; C# must convert before slicing strings.
Engine state is global: do not convert concurrently from multiple native threads.

## Browser ABI

`RodyFrench_Convert(textPtr, requestId, receiverPtr, moduleUrlPtr)` takes UTF-8
pointers and an integer request ID. Module URL:
`Application.streamingAssetsPath + "/RodyFrench/ephone.mjs"`.

The bridge loads the bundled module/Romance pack once, selects `fr`, and calls
`SendMessage(receiver, "RodyFrenchConverted", json)` with:

```json
{"requestId":1,"ipa":"le-z amˈi.","sourceMap":[{"source":0,"ipa":0},{"source":4,"ipa":5}],"error":""}
```

On failure, `error` contains the diagnostic, `ipa` is empty, and `sourceMap` is
empty. Offsets use the same code-point convention as native.

## Observed limits and verification

Direct execution of the delivered native dylib and Node execution of the
bundled browser module produced matching output for French liaison/h aspiré,
`président` noun versus verb, decimals, and a sample Rody sentence. In particular,
`les amis` gained /z/ while `les héros` did not. `Rody` is guessed as an English
name by upstream; the application’s authored-name dictionary overrides it with
`r_o_d_i`. Typos are pronounced approximately.

The converter can omit a source period in IPA (observed after the first
`président.` in `un président. ils président.`). Playback pauses therefore belong
to **source punctuation**, not returned IPA punctuation. Decimal commas are part
of numbers, not pauses. Alignment can split an expanded number into several
spans. Unicode supplementary characters need code-point-to-UTF16 conversion.

Unity compilation, Unity player builds and browser-in-Unity playback have not
been run as part of building this dependency.
