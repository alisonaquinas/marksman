# AGENTS.md

## Repo Summary

Marksman is a Markdown language server built in F# on .NET 9.

- Solution: `Marksman.sln`
- Main executable project: `Marksman/Marksman.fsproj`
- Test project: `Tests/Tests.fsproj`
- Supporting projects:
  - `LanguageServerProtocol/LanguageServerProtocol.fsproj`
  - `MarkdigPatches/MarkdigPatches.csproj`
  - `Benchmarks/Benchmarks.fsproj`

Main entrypoint:

- `Marksman/Program.fs`
- Default execution starts the LSP server
- Explicit server command: `dotnet run --project Marksman -- server`

## Important Paths

- `Marksman/`: core server implementation
- `Tests/`: xUnit test suite
- `docs/`: user-facing docs
- `assets/`: README/demo assets
- `scripts/`: install and packaging helpers

Core source areas in `Marksman/`:

- parsing and document model: `Ast.fs`, `Cst.fs`, `Parser.fs`, `Structure.fs`, `Doc.fs`
- workspace and indexing: `Index.fs`, `Folder.fs`, `Workspace.fs`, `State.fs`
- markdown intelligence: `Refs.fs`, `Diag.fs`, `Compl.fs`, `Refactor.fs`, `Symbols.fs`, `CodeActions.fs`
- server wiring: `Server.fs`, `Program.fs`

## Build And Test

Observed toolchain:

- `.NET SDK 9.x`
- `global.json` pins SDK feature line `9.0.100` with `rollForward: latestFeature`

Repo-native commands from `Makefile`:

- build: `dotnet build Marksman/Marksman.fsproj`
- test: `dotnet test --nologo`
- run server: `dotnet run --project Marksman -- server`
- format check: `dotnet tool restore` then `dotnet fantomas --check Marksman`
- format: `make fmt`
- publish self-contained binary: `make publish`

## Validation Expectations

For code changes, prefer:

1. `dotnet test --nologo`
2. relevant targeted runs if only one subsystem changed
3. formatting checks when touching F# source

Do not assume the working tree is clean. Verify with:

- `git status --short --branch`

## Initialization Snapshot

These findings were verified during repo initialization on `2026-04-16` and may drift:

- branch observed: `develop`
- working tree observed: dirty (uncommitted test-infrastructure additions in `Tests/`)
- test result observed: `336 passed`, `3 skipped`, `0 failed`

Re-verify before relying on snapshot data.
