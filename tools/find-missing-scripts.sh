#!/bin/bash
# find-missing-scripts.sh - Find missing MonoBehaviour scripts in Unity scenes
# Usage: ./find-missing-scripts.sh <scene_file_or_commit:path>
#
# Examples:
#   ./find-missing-scripts.sh Assets/Scenes/Main.unity           # Current file
#   ./find-missing-scripts.sh 665f705:Assets/Scenes/Main.unity   # Git history

set -e

if [ -z "$1" ]; then
    echo "Usage: $0 <scene_file_or_commit:path>"
    exit 1
fi

INPUT="$1"
CACHE_FILE="/tmp/unity_guid_cache.txt"

# Build GUID cache if it doesn't exist or is old
build_cache() {
    echo "Building GUID cache..." >&2
    find Assets Library/PackageCache -name "*.meta" -exec grep -H "^guid:" {} \; 2>/dev/null | \
        sed 's/:guid: / /' > "$CACHE_FILE"
    echo "Cache built: $(wc -l < "$CACHE_FILE") GUIDs" >&2
}

# Check if cache is stale (older than 1 hour)
if [ ! -f "$CACHE_FILE" ] || [ $(find "$CACHE_FILE" -mmin +60 2>/dev/null | wc -l) -gt 0 ]; then
    build_cache
fi

# Get scene content
if [[ "$INPUT" == *":"* ]]; then
    COMMIT="${INPUT%%:*}"
    FILE="${INPUT#*:}"
    echo "=== Checking: $COMMIT:$FILE ===" >&2
    SCENE_CONTENT=$(git show "$COMMIT:$FILE" 2>/dev/null)
else
    echo "=== Checking: $INPUT ===" >&2
    SCENE_CONTENT=$(cat "$INPUT" 2>/dev/null)
fi

# Extract unique script GUIDs
GUIDS=$(echo "$SCENE_CONTENT" | grep -oE "guid: [a-f0-9]{32}" | sed 's/guid: //' | sort -u)

echo "" >&2
echo "=== Missing Scripts ===" >&2

for GUID in $GUIDS; do
    [ -z "$GUID" ] && continue

    # Fast lookup in cache
    if ! grep -q " $GUID$" "$CACHE_FILE" 2>/dev/null; then
        # Find GameObject using this script
        GO_LINE=$(echo "$SCENE_CONTENT" | grep -B20 "guid: $GUID" | grep "m_GameObject:" | tail -1)
        FILEID=$(echo "$GO_LINE" | grep -oE "fileID: [0-9]+" | sed 's/fileID: //')

        if [ -n "$FILEID" ]; then
            NAME=$(echo "$SCENE_CONTENT" | grep -A15 "^--- !u!1 &$FILEID" | grep "m_Name:" | head -1 | sed 's/.*m_Name: //')
            [ -z "$NAME" ] && NAME="(stripped prefab instance)"
        else
            NAME="(unknown)"
        fi

        # Get config snippet
        CONFIG=$(echo "$SCENE_CONTENT" | grep -A10 "guid: $GUID" | grep -E "^  [a-zA-Z]" | head -3 | tr '\n' ' ')

        echo "❌ $GUID"
        echo "   GameObject: $NAME (fileID: $FILEID)"
        echo "   Config: $CONFIG"
        echo ""
    fi
done

echo "=== Done ===" >&2
