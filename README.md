# ssh-c

> ✨ A fast, self-contained, cross-platform SSH launcher with alias-based config, versioned releases, and enterprise-grade tooling.

## 🧩 What is ssh-c?

`ssh-c` is a tiny CLI tool that:

- Connects to SSH targets using aliases
- Stores credentials/config in `~/.shh-c/config.json`
- Supports `cert`-based or `password`-based login
- Has auto-updating installers and multi-platform release support
- Fully self-contained (no .NET install required)

## 🚀 Features

- 🔒 Alias-based connection config
- 🔐 SSH key or password support
- 📦 Single-file binary (macOS/Linux/Windows)
- 🧼 Trimmed + signed release builds
- 📑 `--list`, `--export`, `--remove` support
- 🆕 Auto-versioned GitHub releases
- 🔄 `--check-updates` support with version comparison

## 🛠️ Installation

### 🧼 macOS / Linux

```bash
curl -LO https://github.com/<your-org>/ssh-c/releases/latest/download/install.sh
chmod +x install.sh
./install.sh
```

### 🪟 Windows (PowerShell)

```powershell
iwr https://github.com/<your-org>/ssh-c/releases/latest/download/install.ps1 -OutFile install.ps1
./install.ps1
```

## 📝 Usage

```bash
ssh-c <alias> [--verbose]           # Connect to alias
ssh-c --add --name=alias --host=host --user=user --auth-type=cert|password [--port=22] [--identity-file=path]
ssh-c --list                        # Show all aliases
ssh-c --export alias               # Print SSH command for alias
ssh-c --remove alias               # Delete an alias
ssh-c --version                    # Print version from assembly
ssh-c --check-updates              # Check latest GitHub release vs local version
```

### 📦 Example

```bash
ssh-c --add \
  --name=prod-server \
  --host=192.168.0.12 \
  --user=admin \
  --auth-type=cert \
  --identity-file=~/.ssh/id_rsa

ssh-c prod-server
```

## 🔄 Updating

Run anytime to check for newer versions:
```bash
ssh-c --check-updates
```

## 🔧 Configuration

Saved at:

```bash
~/.shh-c/config.json
```

Format:

```json
{
  "hosts": [
    {
      "name": "prod-server",
      "user": "admin",
      "host": "192.168.0.12",
      "port": 22,
      "auth": {
        "type": "cert",
        "identityFile": "~/.ssh/id_rsa"
      }
    }
  ]
}
```

## 🔐 Code Signing & macOS Trust

If you're building locally and get a Gatekeeper error:

```bash
xattr -d com.apple.quarantine ./ssh-c
```

To properly sign and notarize:

- Use a Developer ID Application certificate
- `codesign --sign "Developer ID Application: You" ./ssh-c`

## 🧪 Dev & Build

```bash
dotnet publish -c Release -r osx-arm64 --self-contained true \
  /p:PublishSingleFile=true /p:PublishTrimmed=true /p:EnableCompressionInSingleFile=true
```

## 📦 Release

Push a Git tag like:

```bash
git tag v1.3.0
git push origin v1.3.0
```

Triggers GitHub Actions:

- Builds cross-platform binaries
- Zips + hashes
- Uploads `install.sh` + `install.ps1`
- Appends auto-generated changelog

## 📖 License

MIT © 2025 RePass Cloud Pty Ltd
