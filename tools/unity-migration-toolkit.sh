#!/bin/bash
#===============================================================================
# Unity Migration Toolkit
# Comprehensive toolbox for Unity project migrations:
# - BetterEvent/Odin serialization discovery and decoding
# - Missing scripts detection and recovery
# - Unused asset detection
# - GUID management
#
# Usage: ./tools/unity-migration-toolkit.sh <command> [args]
# Run without args or with 'help' to see all commands.
#===============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

#===============================================================================
# BETTER EVENT / ODIN COMMANDS
#===============================================================================

# List all scripts using BetterEvent with their GUIDs
cmd_be_list_scripts() {
    echo -e "${BLUE}=== Scripts Using BetterEvent ===${NC}"
    echo ""
    printf "%-40s %s\n" "SCRIPT" "GUID"
    printf "%-40s %s\n" "------" "----"

    grep -rl "BetterEvent" "$PROJECT_ROOT/Assets" --include="*.cs" 2>/dev/null | while read -r script; do
        local meta="${script}.meta"
        if [ -f "$meta" ]; then
            local guid=$(grep "^guid:" "$meta" | head -1 | awk '{print $2}')
            local name=$(basename "$script" .cs)
            printf "%-40s %s\n" "$name" "$guid"
        fi
    done
}

# Output just the GUIDs (for piping)
cmd_be_guids() {
    grep -rl "BetterEvent" "$PROJECT_ROOT/Assets" --include="*.cs" 2>/dev/null | while read -r script; do
        local meta="${script}.meta"
        if [ -f "$meta" ]; then
            grep "^guid:" "$meta" | head -1 | awk '{print $2}'
        fi
    done
}

# Find BetterEvent component usages in a file
cmd_be_find_usages() {
    local target="$1"
    [ -z "$target" ] && { echo "Usage: $0 be-find-usages <scene-or-prefab>"; exit 1; }
    [ ! -f "$target" ] && target="$PROJECT_ROOT/$target"
    [ ! -f "$target" ] && { echo -e "${RED}File not found: $target${NC}"; exit 1; }

    echo -e "${BLUE}=== BetterEvent Usages in $(basename "$target") ===${NC}"
    echo ""

    local guids=$(cmd_be_guids)
    echo "$guids" | while read -r guid; do
        if [ -n "$guid" ]; then
            local count=$(grep -c "guid: $guid" "$target" 2>/dev/null | tr -d '\n' || echo "0")
            if [ "$count" -gt 0 ] 2>/dev/null; then
                local script=$(grep -rl "^guid: $guid" "$PROJECT_ROOT/Assets" --include="*.meta" 2>/dev/null | head -1)
                local name=$(basename "${script%.meta}" .cs 2>/dev/null || echo "unknown")
                echo -e "${GREEN}$count x $name${NC} ($guid)"
            fi
        fi
    done

    local odin=$(grep -c "unityReferences:" "$target" 2>/dev/null | tr -d '\n' || echo "0")
    [ "$odin" -gt 0 ] 2>/dev/null && echo -e "\n${YELLOW}$odin Odin serialization block(s) found${NC}"
}

# Decode Odin binary hex to readable text
cmd_be_decode() {
    local hex="$1"
    [ -z "$hex" ] && { echo "Usage: $0 be-decode <hex-string>"; exit 1; }
    echo "$hex" | sed 's/ //g' | sed 's/\(..\)00/\1/g' | xxd -r -p 2>/dev/null
    echo ""
}

# Extract BetterEvent serializations from a scene
cmd_be_extract() {
    local target="$1"
    [ -z "$target" ] && { echo "Usage: $0 be-extract <scene-file>"; exit 1; }
    [ ! -f "$target" ] && target="$PROJECT_ROOT/$target"
    [ ! -f "$target" ] && { echo -e "${RED}File not found: $target${NC}"; exit 1; }

    echo -e "${BLUE}=== BetterEvent Serializations in $(basename "$target") ===${NC}"
    echo ""

    grep -n "bytes:" "$target" | while read -r line; do
        local linenum=$(echo "$line" | cut -d: -f1)
        local hex=$(echo "$line" | sed 's/.*bytes: //')

        if [ -n "$hex" ] && [ ${#hex} -gt 10 ]; then
            echo -e "${YELLOW}Line $linenum:${NC}"
            local decoded=$(echo "$hex" | sed 's/\(..\)00/\1/g' | xxd -r -p 2>/dev/null || true)

            # Extract key fields
            local method=$(echo "$decoded" | grep -oE '(SetBool|SetTrigger|SetActive|Play|Stop|Invoke|SetFloat|SetInt|SetColor)' | head -1 || true)
            local type=$(echo "$decoded" | grep -oE 'UnityEngine\.[A-Za-z]+' | head -1 || true)
            local param=$(echo "$decoded" | grep -oE 'is[A-Z][a-zA-Z]*|Fire|true|false' | head -1 || true)

            [ -n "$type" ] && echo "  Target: $type"
            [ -n "$method" ] && echo "  Method: $method"
            [ -n "$param" ] && echo "  Param: $param"

            # Show unity reference
            local ref_line=$((linenum - 2))
            local refs=$(sed -n "${ref_line}p" "$target" 2>/dev/null | grep -oE 'fileID: [0-9]+' || true)
            [ -n "$refs" ] && echo "  Refs: $refs"
            echo ""
        fi
    done
}

# Full BetterEvent audit
cmd_be_audit() {
    local target="$1"
    [ -z "$target" ] && { echo "Usage: $0 be-audit <scene-file>"; exit 1; }
    [ ! -f "$target" ] && target="$PROJECT_ROOT/$target"
    [ ! -f "$target" ] && { echo -e "${RED}File not found: $target${NC}"; exit 1; }

    echo -e "${CYAN}========================================"
    echo "  BetterEvent Audit: $(basename "$target")"
    echo -e "========================================${NC}"
    echo ""

    echo -e "${GREEN}1. Component Usages${NC}"
    echo "-------------------"
    cmd_be_find_usages "$target" 2>/dev/null | grep -v "^===" || echo "None"
    echo ""

    echo -e "${GREEN}2. Serialized Events${NC}"
    echo "--------------------"
    cmd_be_extract "$target" 2>/dev/null | grep -v "^===" || echo "None"

    local bytes=$(grep -c "bytes:" "$target" 2>/dev/null || echo "0")
    local refs=$(grep -c "unityReferences:" "$target" 2>/dev/null || echo "0")
    echo -e "${GREEN}3. Summary${NC}"
    echo "----------"
    echo "Odin serialization blocks: $bytes"
    echo "Unity reference blocks: $refs"
}

#===============================================================================
# MISSING SCRIPTS COMMANDS
#===============================================================================

GUID_CACHE="/tmp/unity_guid_cache.txt"

# Build/refresh GUID cache
cmd_build_cache() {
    echo "Building GUID cache..." >&2
    find "$PROJECT_ROOT/Assets" "$PROJECT_ROOT/Library/PackageCache" -name "*.meta" -exec grep -H "^guid:" {} \; 2>/dev/null | \
        sed 's/:guid: / /' > "$GUID_CACHE"
    echo "Cached $(wc -l < "$GUID_CACHE" | tr -d ' ') GUIDs" >&2
}

# Find missing scripts in a scene
cmd_missing_scripts() {
    local input="$1"
    [ -z "$input" ] && { echo "Usage: $0 missing-scripts <scene-or-commit:path>"; exit 1; }

    # Refresh cache if stale
    if [ ! -f "$GUID_CACHE" ] || [ "$(find "$GUID_CACHE" -mmin +60 2>/dev/null | wc -l)" -gt 0 ]; then
        cmd_build_cache
    fi

    # Get content
    local content
    if [[ "$input" == *":"* ]]; then
        local commit="${input%%:*}"
        local file="${input#*:}"
        echo -e "${BLUE}=== Missing Scripts in $commit:$file ===${NC}" >&2
        content=$(git show "$commit:$file" 2>/dev/null)
    else
        [ ! -f "$input" ] && input="$PROJECT_ROOT/$input"
        echo -e "${BLUE}=== Missing Scripts in $(basename "$input") ===${NC}" >&2
        content=$(cat "$input")
    fi

    # Find missing GUIDs
    local guids=$(echo "$content" | grep -oE "guid: [a-f0-9]{32}" | sed 's/guid: //' | sort -u)

    local found=0
    for guid in $guids; do
        [ -z "$guid" ] && continue

        if ! grep -q " $guid$" "$GUID_CACHE" 2>/dev/null; then
            # Find GameObject name
            local go_line=$(echo "$content" | grep -B20 "guid: $guid" | grep "m_GameObject:" | tail -1)
            local fileid=$(echo "$go_line" | grep -oE "fileID: [0-9]+" | sed 's/fileID: //')
            local name="(unknown)"
            if [ -n "$fileid" ]; then
                name=$(echo "$content" | grep -A15 "^--- !u!1 &$fileid" | grep "m_Name:" | head -1 | sed 's/.*m_Name: //')
                [ -z "$name" ] && name="(prefab instance)"
            fi

            echo -e "${RED}Missing: $guid${NC}"
            echo "  GameObject: $name"
            found=1
        fi
    done

    [ "$found" -eq 0 ] && echo -e "${GREEN}No missing scripts found${NC}"
}

# Look up what a GUID belongs to
cmd_guid_lookup() {
    local guid="$1"
    [ -z "$guid" ] && { echo "Usage: $0 guid-lookup <guid>"; exit 1; }

    echo -e "${BLUE}Looking up: $guid${NC}"

    local meta=$(grep -rl "^guid: $guid" "$PROJECT_ROOT/Assets" --include="*.meta" 2>/dev/null | head -1)
    if [ -n "$meta" ]; then
        echo -e "${GREEN}Found: ${meta%.meta}${NC}"
        return
    fi

    local pkg=$(grep -rl "guid: $guid" "$PROJECT_ROOT/Library/PackageCache" --include="*.meta" 2>/dev/null | head -1)
    if [ -n "$pkg" ]; then
        echo -e "${YELLOW}Package: $pkg${NC}"
        return
    fi

    # Check git history
    local hist=$(git log --all -p --full-history -S "$guid" -- "*.meta" 2>/dev/null | head -20)
    if [ -n "$hist" ]; then
        echo -e "${YELLOW}Found in git history (deleted):${NC}"
        echo "$hist" | head -10
        return
    fi

    echo -e "${RED}Not found${NC}"
}

#===============================================================================
# SCRIPTABLEOBJECT VARIABLE COMMANDS
#===============================================================================

# Find all VariableListener components in scenes/prefabs
cmd_so_find_listeners() {
    local target="${1:-$PROJECT_ROOT/Assets}"
    [ ! -d "$target" ] && target="$PROJECT_ROOT/$target"

    echo -e "${BLUE}=== ScriptableObject VariableListener Usages ===${NC}"
    echo ""

    # Find all *VariableListener scripts (class definitions, not mentions)
    local listener_guids=$(grep -rl "class.*VariableListener" "$PROJECT_ROOT/Assets" --include="*.cs" 2>/dev/null | while read -r script; do
        local meta="${script}.meta"
        if [ -f "$meta" ]; then
            grep "^guid:" "$meta" | head -1 | awk '{print $2}'
        fi
    done)

    if [ -z "$listener_guids" ]; then
        echo "No VariableListener scripts found"
        return
    fi

    echo -e "${CYAN}Listener scripts found:${NC}"
    echo "$listener_guids" | while read -r guid; do
        [ -z "$guid" ] && continue
        local script=$(grep -rl "^guid: $guid" "$PROJECT_ROOT/Assets" --include="*.meta" 2>/dev/null | head -1)
        local name=$(basename "${script%.meta}" .cs 2>/dev/null || echo "unknown")
        echo "  $name ($guid)"
    done
    echo ""

    echo -e "${CYAN}Usages in scenes/prefabs:${NC}"
    find "$target" \( -name "*.unity" -o -name "*.prefab" \) 2>/dev/null | while read -r file; do
        for guid in $listener_guids; do
            [ -z "$guid" ] && continue
            local count=$(grep -c "guid: $guid" "$file" 2>/dev/null || echo "0")
            if [ "$count" -gt 0 ] 2>/dev/null; then
                local script=$(grep -rl "^guid: $guid" "$PROJECT_ROOT/Assets" --include="*.meta" 2>/dev/null | head -1)
                local name=$(basename "${script%.meta}" .cs 2>/dev/null || echo "unknown")
                local relpath="${file#$PROJECT_ROOT/}"
                echo -e "${GREEN}$count x $name${NC} in $relpath"
            fi
        done
    done
}

# Find all usages of a specific ScriptableObject asset
cmd_so_usages() {
    local asset="$1"
    [ -z "$asset" ] && { echo "Usage: $0 so-usages <asset-path-or-guid>"; exit 1; }

    local guid="$asset"
    # If it's a path, get the GUID
    if [[ "$asset" == *.asset ]]; then
        [ ! -f "$asset" ] && asset="$PROJECT_ROOT/$asset"
        local meta="${asset}.meta"
        [ ! -f "$meta" ] && { echo -e "${RED}Meta file not found: $meta${NC}"; exit 1; }
        guid=$(grep "^guid:" "$meta" | head -1 | awk '{print $2}')
    fi

    echo -e "${BLUE}=== Usages of $guid ===${NC}"
    echo ""

    grep -rl "$guid" "$PROJECT_ROOT/Assets" --include="*.unity" --include="*.prefab" --include="*.cs" 2>/dev/null | while read -r file; do
        local relpath="${file#$PROJECT_ROOT/}"
        local count=$(grep -c "$guid" "$file" 2>/dev/null || echo "1")
        echo -e "${GREEN}$count ref(s)${NC} in $relpath"
    done
}

#===============================================================================
# UNUSED FILES COMMANDS
#===============================================================================

cmd_unused_files() {
    local output="${1:-unused_files.log}"

    echo -e "${BLUE}=== Finding Unused Files ===${NC}"
    echo "Output: $output"
    echo ""

    rm -f "$output"

    grep -r . --include='*.meta' -e 'guid' "$PROJECT_ROOT/Assets" 2>/dev/null | \
    grep -v 'Editor' | grep -v 'Gizmos' | while read -r line; do
        local guid=$(echo "$line" | awk '{print $NF}')
        local path=$(echo "$line" | cut -f1 -d":")
        local no_meta="${path%.meta}"

        # Skip directories
        [ -d "$no_meta" ] && continue

        local filename=$(basename "$path" .meta)

        # Check if referenced
        if ! grep -rq "$guid" --include='*.unity' --include='*.anim' --include='*.controller' --include='*.prefab' --include='*.mat' "$PROJECT_ROOT/Assets" 2>/dev/null; then
            # Special handling for scripts
            if [[ "$filename" == *.cs ]]; then
                local classname="${filename%.cs}"
                if ! grep -rq "$classname" --include='*.cs' "$PROJECT_ROOT/Assets" 2>/dev/null; then
                    echo -e "${RED}Unused: $no_meta${NC}"
                    echo "$path" >> "$output"
                fi
            # Special handling for scenes
            elif [[ "$filename" == *.unity ]]; then
                local scene_path="${no_meta#$PROJECT_ROOT/}"
                if ! grep -q "$scene_path" "$PROJECT_ROOT/ProjectSettings/EditorBuildSettings.asset" 2>/dev/null; then
                    echo -e "${RED}Unused scene: $no_meta${NC}"
                    echo "$path" >> "$output"
                fi
            else
                echo -e "${RED}Unused: $no_meta${NC}"
                echo "$path" >> "$output"
            fi
        fi
    done

    local count=$(wc -l < "$output" 2>/dev/null | tr -d ' ' || echo "0")
    echo ""
    echo -e "${GREEN}Found $count unused file(s)${NC}"
}

#===============================================================================
# HELP
#===============================================================================

cmd_help() {
    cat << 'EOF'
Unity Migration Toolkit
=======================

BETTEREVENT COMMANDS:
  be-list-scripts       List all scripts using BetterEvent with GUIDs
  be-guids              Output just GUIDs (for piping)
  be-find-usages <f>    Find BetterEvent components in scene/prefab
  be-decode <hex>       Decode Odin binary to readable text
  be-extract <file>     Extract BetterEvent serializations from scene
  be-audit <file>       Full BetterEvent audit of a scene

MISSING SCRIPTS COMMANDS:
  missing-scripts <f>   Find missing scripts in scene (supports commit:path)
  build-cache           Rebuild GUID cache
  guid-lookup <guid>    Look up what a GUID belongs to

SCRIPTABLEOBJECT VARIABLE COMMANDS:
  so-find-listeners [d] Find all VariableListener usages in scenes/prefabs
  so-usages <asset>     Find all usages of a ScriptableObject asset (path or GUID)

UNUSED FILES COMMANDS:
  unused-files [out]    Find unused assets (default: unused_files.log)

EXAMPLES:
  ./tools/unity-migration-toolkit.sh be-audit Assets/Scenes/Main.unity
  ./tools/unity-migration-toolkit.sh missing-scripts 665f705:Assets/Scenes/Main.unity
  ./tools/unity-migration-toolkit.sh be-decode '53006500740042006f006f006c00'
  ./tools/unity-migration-toolkit.sh guid-lookup db2f33d946594fbcbe1cd91669ccfd39

EOF
}

#===============================================================================
# MAIN
#===============================================================================

case "${1:-help}" in
    # BetterEvent commands
    be-list-scripts)    cmd_be_list_scripts ;;
    be-guids)           cmd_be_guids ;;
    be-find-usages)     cmd_be_find_usages "$2" ;;
    be-decode)          cmd_be_decode "$2" ;;
    be-extract)         cmd_be_extract "$2" ;;
    be-audit)           cmd_be_audit "$2" ;;

    # Missing scripts
    missing-scripts)    cmd_missing_scripts "$2" ;;
    build-cache)        cmd_build_cache ;;
    guid-lookup)        cmd_guid_lookup "$2" ;;

    # ScriptableObject variables
    so-find-listeners)  cmd_so_find_listeners "$2" ;;
    so-usages)          cmd_so_usages "$2" ;;

    # Unused files
    unused-files)       cmd_unused_files "$2" ;;

    help|--help|-h)     cmd_help ;;
    *)
        echo "Unknown command: $1"
        echo "Run '$0 help' for usage"
        exit 1
        ;;
esac
