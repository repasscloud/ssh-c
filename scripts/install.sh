#!/bin/bash
set -e

PLATFORM=$(uname | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

case "$PLATFORM" in
  linux) RID="linux-x64";;
  darwin)
    if [[ "$ARCH" == "arm64" ]]; then
      RID="osx-arm64"
    else
      RID="osx-x64"
    fi
    ;;
  *) echo "❌ Unsupported OS: $PLATFORM"; exit 1;;
esac

echo "📦 Downloading ssh-c for $RID..."
curl -LO "https://github.com/repasscloud/ssh-c/releases/latest/download/ssh-c-$RID.zip"

# Remove quarantine from ZIP first (before unzip)
if [[ "$PLATFORM" == "darwin" ]]; then
  echo "🧼 Removing quarantine from ZIP..."
  xattr -d com.apple.quarantine "ssh-c-$RID.zip" || true
fi

unzip -o "ssh-c-$RID.zip"
chmod +x ssh-c

# Fully strip xattrs from binary before install (macOS only)
if [[ "$PLATFORM" == "darwin" ]]; then
  echo "🚫 Stripping Gatekeeper metadata..."
  xattr -cr ssh-c || true
fi

# Move to final location
sudo mv ssh-c /usr/local/bin/ssh-c

# Confirm clean install
if [[ "$PLATFORM" == "darwin" ]]; then
  echo "🔍 Final xattrs on binary:"
  xattr /usr/local/bin/ssh-c || echo "✅ No extended attributes"
fi

echo "✅ Installed ssh-c to /usr/local/bin/ssh-c"