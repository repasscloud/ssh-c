<# 
    ssh-c PowerShell Installer
    VERSION: 1.0.0
    SOURCE: https://github.com/<your-org>/ssh-c
    LICENSE: MIT
#>

# PowerShell installer for ssh-c (Windows)

# Detect architecture
$arch = (Get-CimInstance Win32_OperatingSystem).OSArchitecture
switch ($arch) {
    "64-bit" { $RID = "win-x64" }
    "32-bit" { $RID = "win-x86" }
    default  { Write-Error "❌ Unsupported architecture: $arch"; exit 1 }
}

$target = "$env:ProgramData\ssh-c"
$zipName = "ssh-c-$RID.zip"
$exeName = "ssh-c.exe"
$exePath = Join-Path $target $exeName

Write-Host "📦 Downloading ssh-c for $RID..."
Invoke-WebRequest "https://github.com/<your-org>/ssh-c/releases/latest/download/$zipName" -OutFile $zipName

if (-Not (Test-Path $target)) {
    New-Item -ItemType Directory -Path $target | Out-Null
}

Expand-Archive -Path $zipName -DestinationPath $target -Force

# Rename if it's signed and saved with a different name
if (Test-Path (Join-Path $target "ssh-c.signed.exe")) {
    Remove-Item $exePath -Force -ErrorAction SilentlyContinue
    Rename-Item -Path (Join-Path $target "ssh-c.signed.exe") -NewName $exeName
}

if (Test-Path $exePath) {
    Write-Host "✅ Installed ssh-c to $exePath"
    Write-Host "👉 Add '$target' to PATH permanently:"
    Write-Host "[System.Environment]::SetEnvironmentVariable('PATH', \$env:PATH + ';$target', [System.EnvironmentVariableTarget]::Machine)"
} else {
    Write-Error "❌ Installation failed. File not found: $exePath"
    exit 1
}
