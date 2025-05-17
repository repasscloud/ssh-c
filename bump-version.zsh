#!/bin/zsh

# Path to .csproj file
CSProjFile="ssh-c.csproj"

# Confirm file exists
if [[ ! -f $CSProjFile ]]; then
  echo "❌ File not found: $CSProjFile"
  exit 1
fi

# Extract current version from <Version>X.Y.Z</Version>
currentVersion=$(grep "<Version>" "$CSProjFile" | gsed -E 's/.*<Version>([0-9]+)\.([0-9]+)\.([0-9]+)<\/Version>.*/\1.\2.\3/')
IFS='.' read -r major minor patch <<< "$currentVersion"

# Determine bump type (default: patch)
bumpType="${1:-patch}"

case "$bumpType" in
  major)
    ((major++))
    minor=0
    patch=0
    ;;
  minor)
    ((minor++))
    patch=0
    ;;
  patch)
    ((patch++))
    ;;
  *)
    echo "❌ Invalid bump type: $bumpType (use: major, minor, patch)"
    exit 1
    ;;
esac

newVersion="${major}.${minor}.${patch}"

echo "🔧 Bumping version: $currentVersion → $newVersion"

# Update .csproj file using GNU sed with proper in-place syntax
gsed -i'' -E "s|<Version>[0-9]+\.[0-9]+\.[0-9]+</Version>|<Version>$newVersion</Version>|" "$CSProjFile"

echo "✅ Updated version in $CSProjFile → $newVersion"

# Set script root dir
SCRIPT_DIR=$(cd -- "$(dirname -- "${(%):-%x}")" &> /dev/null && pwd)

## Make sure we're on dev branch
git checkout dev

# Add and commit version bump
git add "$SCRIPT_DIR"
git commit -m "🔧 Bumping version: $currentVersion → $newVersion"

# Rebase if needed (to avoid non-fast-forward)
git pull --rebase origin dev

# Push commit and tag
git push -u origin dev
git tag -s "v$newVersion" -m "Release v$newVersion"
git push origin "v$newVersion"

echo "🔖 Tagged and pushed release: https://github.com/repasscloud/ssh-c/releases/tag/v$newVersion"
