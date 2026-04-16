---
title: Data Flow
tags:
  - wiki/architecture
aliases:
  - LSP Data Flow
  - Request Flow
related:
  - Overview
  - Layers
  - Connection Graph
  - Workspace Model
---

# Data Flow

This page traces how data moves through Marksman — from a Markdown file arriving off disk, through parsing and indexing, and finally out as an LSP response. There are three distinct flows worth understanding: the ==document lifecycle==, the ==notification flow== for editor-driven changes, and the ==request flow== for on-demand queries.

## Document Lifecycle

When Marksman first opens a workspace folder, or when a new file is discovered, a document travels through the following stages:

**1. Raw text acquisition.** The file's content arrives either by reading from disk (on startup) or from the editor's in-memory buffer (after `textDocument/didOpen`). In both cases the text is wrapped in a `Text.fs` buffer value, which supports incremental edit application.

**2. Parsing.** `Parser.fs` feeds the raw text through Markdig (with `MarkdigPatches` applied) and walks the resulting AST to produce a `Structure` value. `Structure` bundles the ==concrete syntax tree== (`Cst`) — preserving heading, wiki-link, Markdown link, tag, and frontmatter nodes with exact source ranges — and the extracted `Syms` list of `Def` and `Ref` symbols.

**3. Indexing.** `Index.fs` consumes the `Structure` to build fast lookup tables for the document: headings indexed by `Slug` (for title-based resolution) and links indexed by source position (for cursor-based lookup). The result is a lightweight `Index` value attached to the `Doc`.

**4. Doc construction.** `Doc.fs` assembles a `Doc` record from the document's `DocId`, version number, raw text, `Structure`, and `Index`. A `Doc` is ==immutable==: every subsequent edit produces a fresh `Doc` rather than mutating the existing one.

**5. Folder integration.** The new `Doc` is inserted into its parent `Folder`. `Folder` then triggers a selective update to the ==`Conn`== (connection graph): only the entries that involve the changed document — outgoing refs from it, and incoming refs targeting it — are re-evaluated. The rest of the graph is untouched.

> [!TIP] Incremental updates
> Because `Conn` updates are scoped to a single document at a time, editing one note in a thousand-file vault does not re-resolve every link in the workspace. Only the affected edges are recomputed.

## LSP Notification Flow

The editor sends notifications (no response expected) to inform Marksman of state changes.

**`textDocument/didOpen`** — The editor sends the full document text. `Server.fs` creates a fresh `Doc` via `Parser`, builds an `Index`, and calls `State.fs` to insert it into the `Workspace`. `State` holds the live `Workspace` behind a serialized async agent so that concurrent notifications do not race.

**`textDocument/didChange`** — The editor sends one or more incremental text edits (or a full replacement). `Server.fs` extracts the change list and passes it to `Text.applyChanges`, producing an updated text buffer. The updated buffer is re-parsed from scratch by `Parser`, a new `Index` is built, a new `Doc` is assembled, and `State` replaces the old `Doc` in the `Folder` and updates `Conn`.

**`textDocument/didClose`** — The editor indicates it is no longer tracking the document. Marksman removes any editor-provided text and reverts to the on-disk version (or drops the document entirely from the workspace if no on-disk counterpart exists).

> [!WARNING] Diagnostics are pushed, not pulled
> After any notification that modifies a `Doc` or `Conn`, `Server.fs` proactively calls `Diag.fs` to recompute broken-link diagnostics for the affected documents and pushes `textDocument/publishDiagnostics` notifications to the editor. The editor does not request diagnostics; Marksman volunteers them.

## LSP Request Flow

Requests expect a response. The worked example here is ==`textDocument/definition`== (go-to-definition).

**Step 1 — Receive.** `Server.fs` receives the JSON-RPC `textDocument/definition` message and extracts the `TextDocumentPositionParams` (URI + cursor position).

**Step 2 — State lookup.** `Server.fs` calls into `State.fs` to retrieve the current `Workspace`, then navigates `Workspace → Folder → Doc` using the document URI.

**Step 3 — Position resolution.** `Index.linkAtPos` is called with the cursor position. `Index` uses its position-keyed link table to find the `WL` (wiki-link) or `ML` (Markdown link) node that the cursor sits inside, returning the corresponding `Ref` symbol.

**Step 4 — Reference resolution.** `Refs.fs` receives the `Ref` and the enclosing `Folder`. It queries `Conn` — which already holds a fully resolved graph — to find the `Def` that this `Ref` points to. The `Def` carries the target `DocId` and the source range of the heading or document title.

**Step 5 — Response.** `Server.fs` converts the `Def`'s `DocId` to a URI (via `Paths.fs`) and packages the `Def`'s range as an LSP `Location`. This is serialized to JSON-RPC and sent back to the editor as the `textDocument/definition` response.

> [!NOTE] Other request types
> The same pattern — State lookup → Index probe → Refs/Conn resolution → LSP payload — applies to `textDocument/references`, `textDocument/rename`, `textDocument/completion`, and `textDocument/documentSymbol`. Only the feature module called in steps 3–4 differs.

## Further Reading

- [[Overview]] — architecture overview and design philosophy
- [[Layers]] — compile-order layers and module responsibilities
- [[Connection Graph]] — how `Conn` stores and resolves cross-document links
- [[Workspace Model]] — `Doc`, `Folder`, `Workspace` types and their relationships
