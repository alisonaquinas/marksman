---
title: Overview
tags:
  - wiki/architecture
aliases:
  - Marksman Overview
  - Architecture Overview
related:
  - Layers
  - Data Flow
  - Workspace Model
---

# Overview

==Marksman== is a Language Server Protocol (LSP) server for Markdown, providing editor-agnostic intelligence for note-taking workflows that use wiki-link and Zettelkasten conventions. It speaks standard ==LSP== over `stdin`/`stdout` using JSON-RPC, which means any editor with an LSP client — Neovim, Emacs, VS Code, Helix — can use it without editor-specific plugins.

## How It Runs

Marksman launches as a child process managed by the editor. All communication happens over `stdin`/`stdout` using the ==StreamJsonRpc== protocol: the editor sends JSON-RPC requests and notifications, and Marksman replies with JSON-RPC responses. There is no HTTP server, no port binding, and no daemon. The entry point is `Program.fs`, which parses CLI flags and then hands off to `Server.fs` to run the JSON-RPC message loop.

> [!NOTE] Single-file mode
> Marksman can operate without any project configuration. When no `.marksman.toml` is found, it falls back to single-file mode, providing completions and symbol navigation for the open file only — without cross-document link resolution.

## Solution Projects

The repository is a .NET 9 solution containing five projects:

| Project | Language | Role |
| --- | --- | --- |
| `Marksman/` | F# | Main executable — all LSP logic |
| `LanguageServerProtocol/` | F# | F# wrapper over StreamJsonRpc for JSON-RPC transport |
| `MarkdigPatches/` | C# | Patches to the Markdig Markdown parser |
| `Tests/` | F# | xunit + Snapper snapshot tests |
| `Benchmarks/` | F# | BenchmarkDotNet performance tests |

## Key Design Philosophy

**Immutable documents.** A ==`Doc`== is parsed once and treated as immutable. When the editor sends a `textDocument/didChange` notification, Marksman applies the reported text edits via `Text.fs` to produce a new `Doc` value rather than mutating the old one. This makes state management in the server straightforward: the server holds a `State` record that is replaced atomically on each mutation.

**Incremental connection updates.** Cross-document link relationships are maintained in a ==`Conn`== (connection) graph per `Folder`. When a single document changes, only the links originating from or targeting that document need to be re-evaluated; the rest of the graph is preserved. This keeps re-indexing fast even in large note vaults.

**Layered, dependency-ordered compilation.** F# requires modules to be listed in compile order. Marksman exploits this constraint to enforce clean layering: foundation utilities compile first, workspace types compile last, and LSP feature modules sit in between. See [[Layers]] for the full breakdown.

## Project Root Detection

Marksman treats a directory containing a ==`.marksman.toml`== file as a project root. When the editor sends `workspace/didChangeWorkspaceFolders` or the initial `initialize` request with workspace folders, Marksman walks each folder looking for `.marksman.toml`. Folders without that file are not treated as Marksman projects unless the server is in single-file mode.

> [!TIP] Multiple roots
> A single Marksman process can serve multiple workspace roots simultaneously. Each root becomes an independent `Folder` inside the single `Workspace` value, with its own `Conn` graph and `Index`.

## Further Reading

- [[Layers]] — compile-order layers and per-module responsibilities
- [[Data Flow]] — how a document travels from disk through parse, index, and into LSP responses
- [[Workspace Model]] — `Doc`, `Folder`, `Workspace`, and how they compose
- [[research/lsp/00-overview|LSP 3.17 — Overview & What's New]] — the protocol specification Marksman implements
- [[research/lsp/01-base-protocol|LSP 3.17 — Base Protocol]] — JSON-RPC framing, message types, and error codes
