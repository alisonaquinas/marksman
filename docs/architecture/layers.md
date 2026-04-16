---
title: Layers
tags:
  - wiki/architecture
aliases:
  - Compile Layers
  - Module Layers
related:
  - Overview
  - Symbol Model
  - Workspace Model
  - Connection Graph
---

# Layers

Marksman's `Marksman/` project is structured as ==seven ordered layers==. Because F# compiles modules strictly in the order they are listed in the `.fsproj` file, this ordering is both a compiler requirement and an intentional architectural boundary: no module may depend on anything listed after it.

> [!ABSTRACT] Why compile order matters
> F#'s top-to-bottom compile order eliminates circular dependencies by construction. If a module tries to reference something defined later in the project file, the build fails. The layer structure below reflects real dependency relationships, not just convention.

## Layer Table

| # | Layer | Modules | Responsibility |
| --- | --- | --- | --- |
| 1 | Foundation | `Misc`, `SuffixTree`, `MMap`, `Mapping`, `Graph` | String utilities, Slug types, Difference tracking, fuzzy suffix-tree matching, multi-valued maps, bijective maps, undirected graph primitives |
| 2 | Names / Paths | `Paths`, `Names`, `GitIgnore`, `Config` | Strongly-typed path wrappers (`AbsPath`, `RelPath`, `RootedRelPath`, `UriWith`), document and folder identity types (`DocId`, `FolderId`), `.gitignore` evaluation, all TOML configuration knobs and `ParserSettings` |
| 3 | Parsing | `Text`, `Syms`, `Ast`, `Cst`, `Structure`, `Parser` | Text buffer with incremental edit application, symbol type hierarchy (`Sym`/`Def`/`Ref`/`Tag`/`Scope`), abstract and concrete syntax trees, combined `Structure` record, Markdig-to-Structure conversion |
| 4 | Index / Workspace | `Index`, `Conn`, `Doc`, `Folder`, `Workspace` | Per-document lookup tables, cross-document connection graph with Oracle, the `Doc`/`Folder`/`Workspace` value types |
| 5 | LSP Features | `Semato`, `Refs`, `Diag`, `State`, `Fatality`, `Toc` | Semantic tokens, reference resolution, broken-link diagnostics, mutable server state, crash handling, table-of-contents generation |
| 6 | Code Actions | `CodeActions`, `Compl`, `Refactor`, `Symbols`, `Lenses` | Completion candidates, rename refactoring, workspace symbol search, code lenses |
| 7 | Server | `Server`, `Program` | LSP JSON-RPC message loop, CLI argument parsing and startup |

## Layer Details

### Layer 1 — Foundation

==`Misc`== provides string helpers and the `Slug` type used throughout the codebase to normalize heading text for comparison. `SuffixTree` powers fuzzy completion matching. `MMap` (multi-map) and `Mapping` (bijective map) are custom collection types used heavily in `Index` and `Conn`. `Graph` is a simple undirected graph used by `Conn` to track document relationships.

### Layer 2 — Names / Paths

==`Paths`== introduces a family of discriminated union wrappers around raw strings (`AbsPath`, `RelPath`, `RootPath`, `RootedRelPath`, `UriWith`) to prevent accidental mixing of path kinds at the type level. `Names` defines `DocId` and `FolderId`. `Config` deserializes `.marksman.toml` and exposes `ParserSettings` that control which Markdown extensions are active.

### Layer 3 — Parsing

==`Syms`== defines the core symbol algebra: every meaningful element in a document (a heading definition, a wiki-link reference, a tag) is represented as a `Sym` variant. `Cst` (concrete syntax tree) preserves the exact source spans of `H` (heading), `WL` (wiki-link), `ML` (Markdown link), `MLD` (Markdown link definition), `T` (tag), and `YML` (frontmatter) nodes. `Parser` drives Markdig with `MarkdigPatches` applied, then walks the Markdig AST to produce a `Structure` value containing both the `Cst` and extracted `Syms`.

> [!NOTE] MarkdigPatches
> The C# `MarkdigPatches` project patches Markdig's extension pipeline to correctly handle edge cases — such as off-by-one ranges for headings containing supplementary Unicode characters (fixed in commit `4340227`). The patch results feed directly into `Parser.fs`.

### Layer 4 — Index / Workspace

==`Index`== builds per-document lookup tables: headings keyed by `Slug`, links keyed by source position. ==`Conn`== maintains a cross-document directed graph: for every `Ref` symbol in any document, `Conn` tracks which `Def` it resolves to (or that it is broken). An ==Oracle== abstraction inside `Conn` allows unit tests to inject a deterministic resolution function. `Doc` bundles a document's id, version, raw text, `Structure`, and `Index` into one immutable record. `Folder` holds all `Doc`s under a root plus the shared `Conn`. `Workspace` holds multiple `Folder`s.

### Layer 5 — LSP Features

==`Refs`== is the primary reference-resolution engine: given a `Ref` symbol and a `Folder`, it queries `Conn` to find the matching `Def` and returns its location. ==`Diag`== iterates all broken refs in `Conn` and converts them to LSP `Diagnostic` objects. `State` holds a mutable reference to the current `Workspace` and serializes all mutations through a single async agent.

### Layers 6 & 7 — Actions and Server

The code-action layer translates LSP requests into calls on the workspace/folder/doc types and formats results as LSP response payloads. `Server.fs` is the outermost shell: it registers LSP capability handlers, dispatches incoming requests to the appropriate feature module, and sends responses back over the JSON-RPC transport.

## Further Reading

- [[Overview]] — what Marksman is and how it runs
- [[Symbol Model]] — the `Sym`/`Def`/`Ref` type hierarchy in depth
- [[Workspace Model]] — `Doc`, `Folder`, `Workspace` composition
- [[Connection Graph]] — how `Conn` tracks and resolves cross-document links
