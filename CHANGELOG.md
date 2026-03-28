# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-03-29

### Fixed
- Resolved issue #13: in-app help now uses `--flag=<VALUE>` syntax throughout, matching the README format
- `--remove` now prints explicit success/not-found feedback instead of silently returning
- `ExportCommand` error messages now go to stderr
- Migration from legacy `~/.shh-c` path no longer emits an emoji that could break non-UTF8 terminals

### Added
- Cross-platform SSH binary discovery — checks `/usr/bin/ssh`, `/bin/ssh`, `/usr/local/bin/ssh` on Unix and `%SystemRoot%\System32\OpenSSH\ssh.exe` on Windows before falling back to `ssh` on `PATH`; removes the hardcoded `/usr/bin/ssh` that broke non-standard installs
- `--add` now validates `--auth-type` is exactly `cert` or `password`
- `--add --auth-type=cert` now requires `--identity-file` to be provided
- `Main` returns a proper exit code — SSH exit codes are propagated to the shell; errors return `1`
- All error messages are written to stderr, keeping stdout clean for piping and scripting

### Changed
- `ConfigRoot.Hosts` initialised to an empty list (was nullable), removing downstream null checks
- `SaveConfig` no longer calls `EnsureMigrated` — migration is only relevant on read

## [1.0.0] - 2025-11-20

### Changed
- Full rewrite as a Native AOT, single-file, self-contained binary
- Restructured codebase into `Helpers/`, `Models/`, and `Services/` namespaces
- Replaced reflection-based JSON serialization with source-generated `AppJsonContext` for AOT compatibility
- Added ANSI colour output with `--no-color` / `NO_COLOR` support
- Improved `--list` output: aligned table with colour-coded alias, user, host, port, and auth type
- `--version` output expanded to include runtime, platform, copyright, and license
- `--check-updates` uses GitHub Releases API with proper `User-Agent` and `Accept` headers
- Verbose mode (`-v` / `--verbose`) prints the underlying `ssh` command before connecting

### Added
- `--export <ALIAS>` command to print the raw SSH command for an alias
- Config migration from `~/.shh-c` (old typo path) to `~/.ssh-c`
- `--no-color` flag and `NO_COLOR` environment variable support

## [0.1.1] - 2025-05-17

### Fixed
- Version bump tooling correction (0.0.9 → 0.1.0 → 0.1.1 sequence)

## [0.1.0] - 2025-05-17

### Fixed
- Issue #11: Non-interactive SSH session (`tcsetattr: Input/output error`) — added `-tt` flag to force pseudo-TTY allocation

### Removed
- Removed bundled zsh and sh install scripts from C# build output

## [0.0.9] - 2025-05-17

### Changed
- Internal version bump for release pipeline

## [0.0.8] - 2025-05-10

### Fixed
- Issue #8: JSON config was serialized in PascalCase; updated `AppJsonContext.cs` with `JsonSourceGenerationOptions` to enforce camelCase and `WriteIndented = true`

## [0.0.7] - 2025-05-10

### Fixed
- Issue #6: Version print output reformatted
- Issue #7: Application identifier updated to `com.repasscloud.ssh-c`

### Removed
- Removed stale compiled binary from repository

## [0.0.6] - 2025-05-10

### Fixed
- Issue #3: Auto check-for-updates was not working correctly
- Issue #4: `GetVersion()` was including the git commit hash suffix; now strips everything after `+`

## [0.0.5] - 2025-05-10

### Added
- Initial public release
- Add, list, remove SSH aliases stored in `~/.ssh-c/config.json`
- Certificate and password authentication types
- `--check-updates` via GitHub Releases API
- Install script for Linux and macOS

[Unreleased]: https://github.com/repasscloud/ssh-c/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/repasscloud/ssh-c/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/repasscloud/ssh-c/compare/v0.1.1...v1.0.0
[0.1.1]: https://github.com/repasscloud/ssh-c/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/repasscloud/ssh-c/compare/v0.0.9...v0.1.0
[0.0.9]: https://github.com/repasscloud/ssh-c/compare/v0.0.8...v0.0.9
[0.0.8]: https://github.com/repasscloud/ssh-c/compare/v0.0.7...v0.0.8
[0.0.7]: https://github.com/repasscloud/ssh-c/compare/v0.0.6...v0.0.7
[0.0.6]: https://github.com/repasscloud/ssh-c/compare/v0.0.5...v0.0.6
[0.0.5]: https://github.com/repasscloud/ssh-c/releases/tag/v0.0.5
