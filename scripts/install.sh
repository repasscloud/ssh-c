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
unzip -o "ssh-c-$RID.zip"
chmod +x ssh-c

# Move to final location
sudo mv ssh-c /usr/local/bin/ssh-c

# macOS Gatekeeper bypass
if [[ "$PLATFORM" == "darwin" ]]; then
  echo "🚫 Bypassing Gatekeeper..."
  sudo xattr -d com.apple.quarantine /usr/local/bin/ssh-c || true
fi

echo "✅ Installed ssh-c to /usr/local/bin/ssh-c"
