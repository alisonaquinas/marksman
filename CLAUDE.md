# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
make setup        # Install dotnet tools (Fantomas formatter) — run once
make build        # Build the solution
make test         # Run the test suite
make fmt          # Format all F# code with Fantomas
make check        # Check formatting + lint (CI gate)
make run          # Start the LSP server (stdin/stdout)
make run ARGS="server --verbose 3"  # Run with args
make bench        # Run BenchmarkDotNet benchmarks
make clean        # Remove build artifacts
make install      # Install binary to $HOME/.local/bin
```

Direct dotnet equivalents:
```bash
dotnet restore
dotnet build
dotnet test
dotnet fantomas --check Marksman    # formatting check only
```

## Architecture

Marksman is an LSP server for Markdown. It implements full LSP JSON-RPC over stdin/stdout, providing completions, go-to-definition, find-references, rename, diagnostics, and document symbols for Markdown files with wiki-link and Zettelkasten support.

**Solution projects:**
- `Marksman/` — main F# executable; entry point is `Program.fs`
- `LanguageServerProtocol/` — F# wrapper over `StreamJsonRpc` for JSON-RPC
- `MarkdigPatches/` — C# patch project on top of the Markdig parser
- `Tests/` — xunit + Snapper snapshot tests
- `Benchmarks/` — BenchmarkDotNet perf tests

**Compile order within `Marksman/` (reflects data-flow dependencies):**

| Layer | Modules |
|-------|---------|
| Foundation | `Misc`, `SuffixTree`, `MMap`, `Mapping`, `Graph` |
| Names/Paths | `Paths`, `Names`, `GitIgnore`, `Config` |
| Parsing | `Text`, `Syms`, `Ast`, `Cst`, `Structure`, `Parser` |
| Index/Workspace | `Index`, `Conn`, `Doc`, `Folder`, `Workspace` |
| LSP features | `Semato`, `Refs`, `Diag`, `State`, `Fatality`, `Toc` |
| Code actions | `CodeActions`, `Compl`, `Refactor`, `Symbols`, `Lenses` |
| Server | `Server`, `Program` |

**Key design patterns:**
- `Doc` represents a parsed Markdown document; `Folder` is a collection of docs sharing a root; `Workspace` holds multiple folders.
- `Conn` (connections) tracks cross-document link relationships; `Index` maps names/headings to symbols.
- `Refs` resolves symbolic references; `Diag` generates LSP diagnostics from broken refs.
- Project root is detected by `.marksman.toml` presence; single-file mode works without a project root.

## Tech Stack

- **Language:** F# (.NET 9.0); one C# sub-project (`MarkdigPatches`)
- **Formatter:** Fantomas (config in `.editorconfig` — 100-char line limit, 4-space indent)
- **Key libs:** Markdig (parsing), FSharpPlus (functional utilities), Tomlyn (TOML config), Serilog (logging), StreamJsonRpc (JSON-RPC transport)
- **Tests:** xunit with Snapper for snapshot assertions
- **CI:** GitHub Actions — build + `make check` + test on Ubuntu, Windows, macOS

## Release

```bash
make publishTo DEST=out    # Self-contained single-file binary for current RID
make macosUniversalBinary  # macOS x64 + arm64 fat binary
```

Version is derived automatically from `git describe` at build time.
