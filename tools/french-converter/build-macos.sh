#!/bin/bash
# Rebuild the macOS converter and restore the pinned browser/data assets.
set -euo pipefail
repo_root="$(cd "$(dirname "$0")/../.." && pwd)"
converter_temp="$(mktemp -d "${TMPDIR:-/tmp}/rody-french.XXXXXX")"
trap 'rm -rf "$converter_temp"' EXIT
revision=4f6d246c1d3acf67a4d814e20da02fa3967bc92d
curl --fail --location --silent --show-error "https://codeload.github.com/sjmik/ephone-js/tar.gz/$revision" -o "$converter_temp/source.tgz"
curl --fail --location --silent --show-error 'https://registry.npmjs.org/ephone/-/ephone-1.0.2.tgz' -o "$converter_temp/npm.tgz"
(cd "$converter_temp" && echo 'a2d68b463730754d7f3cdd598a77072631dcf9b343c38ffeee538a8ce082955e  source.tgz' | shasum -a 256 -c -)
(cd "$converter_temp" && echo '20f654d6b92b9ef2e96244c19640d9def4457bbdd7e3132fcc1129921e727cbb  npm.tgz' | shasum -a 256 -c -)
mkdir "$converter_temp/source" "$converter_temp/npm"
tar -xzf "$converter_temp/source.tgz" --strip-components=1 -C "$converter_temp/source"
tar -xzf "$converter_temp/npm.tgz" --strip-components=1 -C "$converter_temp/npm"
# Dictionary compilation resolves voice names; allow its complete voice catalog here.
# Only the French voice and English fallback are shipped below.
cmake -S "$converter_temp/source" -B "$converter_temp/build" \
    -DCMAKE_BUILD_TYPE=Release -DCMAKE_OSX_ARCHITECTURES='arm64;x86_64' \
    -DDICT_COMPILE_REGEX='^(en|fr)$' -DLANG_COPY_REGEX=''
cmake --build "$converter_temp/build" -j 1
plugin="$repo_root/Assets/Plugins/RodyFrench"
assets="$repo_root/Assets/StreamingAssets/RodyFrench"
mkdir -p "$plugin" "$assets/lang"
clang -dynamiclib -arch arm64 -arch x86_64 \
    -Wl,-install_name,@rpath/libRodyFrench.dylib \
    "$repo_root/tools/french-converter/free.c" \
    "-Wl,-force_load,$converter_temp/build/src/libespeak-ng/libespeak-ng.a" \
    "$converter_temp/build/src/ucd-tools/libucd.a" -lm \
    -o "$plugin/libRodyFrench.dylib"
cp "$converter_temp/npm/ephone.js" "$assets/ephone.mjs"
cp "$converter_temp/npm/lang/roa.js" "$assets/lang/roa.js"
cp "$converter_temp/source/COPYING" "$assets/COPYING.txt"
cp "$converter_temp/source/src/ucd-tools/COPYING.UCD" "$assets/COPYING.Unicode.txt"
cp "$converter_temp/source/COPYING" "$repo_root/tools/french-converter/COPYING"
for name in phontab phonindex en_dict fr_dict lang/roa/fr lang/gmw/en-US; do
    mkdir -p "$assets/espeak-ng-data/$(dirname "$name")"
    cp "$converter_temp/build/espeak-ng-data/$name" "$assets/espeak-ng-data/$name"
done
