#!/bin/bash
# Post-build script for WebGL deployment to docs/
# Run this after building WebGL to docs/RodyCollection

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
DOCS_DIR="$PROJECT_DIR/docs"
BUILD_SOURCE="$DOCS_DIR/RodyCollection/Build"
BUILD_DEST="$DOCS_DIR/Build"

echo "🎮 Deploying WebGL build to docs/Build..."

# Check if source exists
if [ ! -d "$BUILD_SOURCE" ]; then
    echo "❌ Error: Build not found at $BUILD_SOURCE"
    echo "   Make sure you built to docs/ with name 'RodyCollection'"
    exit 1
fi

# Remove old build files
echo "🗑️  Removing old build..."
rm -rf "$BUILD_DEST"

# Move new build
echo "📦 Moving new build..."
mv "$BUILD_SOURCE" "$BUILD_DEST"

# Clean up the RodyCollection folder Unity created
rm -rf "$DOCS_DIR/RodyCollection"

echo "✅ Done! Build deployed to docs/Build/"
echo ""
echo "Next steps:"
echo "  git add docs/"
echo "  git commit -m 'Update WebGL build'"
echo "  git push"
