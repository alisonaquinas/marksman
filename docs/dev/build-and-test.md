---
title: Build and Test
tags:
  - wiki/dev
---

# Build and Test

Marksman uses a ==Makefile== as its primary developer interface, wrapping `dotnet` commands with convenient targets. The project targets **.NET 9.0** and is written in F# (with one C# sub-project, `MarkdigPatches`).

## Prerequisites

- .NET SDK 9.0.x
- GNU Make (or `make` via Git for Windows / WSL on Windows)

Run `make setup` once after cloning to install Fantomas, the F# code formatter:

```bash
make setup   # installs dotnet-fantomas as a local tool
```

## Make Targets

| Target | Equivalent `dotnet` command | Description |
|--------|----------------------------|-------------|
| `make build` | `dotnet build` | Compile the full solution |
| `make test` | `dotnet test` | Run all xunit tests |
| `make fmt` | `dotnet fantomas Marksman` | Format all F# source files in-place |
| `make check` | `dotnet fantomas --check Marksman` + lint | Verify formatting without modifying files; **this is the CI gate** |
| `make run` | `dotnet run --project Marksman` | Start the LSP server on stdin/stdout |
| `make run ARGS="server --verbose 3"` | `dotnet run -- server --verbose 3` | Start server with extra arguments |
| `make bench` | `dotnet run --project Benchmarks -c Release` | Run BenchmarkDotNet performance benchmarks |
| `make clean` | `dotnet clean` | Remove build artifacts |
| `make install` | copies binary | Install to `$HOME/.local/bin` |

## Running a Single Test

`dotnet test` accepts standard xunit filters:

```bash
dotnet test --filter "FullyQualifiedName~Toc"        # run TOC tests only
dotnet test --filter "DisplayName=MyTestClass.MyMethod"
dotnet test Tests/ --logger "console;verbosity=detailed"
```

Snapshot tests use the ==Snapper== library. When a snapshot changes intentionally, delete the corresponding `.snap` file and re-run the test to regenerate it.

## Fantomas Formatter

Formatting rules are defined in `.editorconfig` at the repo root:

- **Line length:** 100 characters
- **Indent size:** 4 spaces (no tabs)

Always run `make fmt` before committing, or configure your editor to format on save using the Fantomas LSP/plugin. The CI pipeline runs `make check` and fails if any file is not correctly formatted.

> [!WARNING]
> Do not format with a version of Fantomas different from the one pinned in `.config/dotnet-tools.json`. Version mismatches cause spurious diff noise. `make setup` installs exactly the pinned version.

## CI Matrix

GitHub Actions runs the full pipeline on every pull request and push to `main`:

| Job | Platforms | Steps |
|-----|-----------|-------|
| `build-and-test` | Ubuntu, Windows, macOS | `dotnet restore` → `make check` → `make build` → `make test` |

`make check` (formatting + lint) must pass before tests are run. A failing format check fails the CI without running tests, which keeps the build log clean.

## Release Pipeline

```bash
make publishTo DEST=out    # self-contained, trimmed, single-file binary for current RID
make macosUniversalBinary  # fat binary combining x64 + arm64 slices for macOS
```

`make publishTo` calls `dotnet publish` with `-r <RID> --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true`. The output is a single executable with no external .NET runtime dependency.

### Version from git describe

The build version is derived automatically at compile time from `git describe --tags`. There is no manually maintained `version.txt` or `AssemblyInfo.fs` to keep in sync. Ensure your working tree has the upstream tags fetched (`git fetch --tags`) before building a release binary.

> [!ABSTRACT]
> For reproducible release builds, prefer running `make publishTo` in a clean clone on a tagged commit. The resulting binary embeds the exact tag as its version string, which is reported in LSP `initialize` responses.

## Related Pages

- [[Overview]] — architecture and project layout
- [[Config Reference]] — runtime configuration options
