---
title: Wiki Index
aliases:
  - index
tags:
  - wiki/meta
---

# Wiki Index

Master content catalog. Every wiki page appears here exactly once.
See [[WIKI]] for operating conventions.

---

## Architecture

| Page | Summary |
|------|---------|
| [[Overview]] | Bird's-eye view of Marksman: what it is, how it fits together, key entry points |
| [[Layers]] | The seven compile-order layers in `Marksman/` and what each is responsible for |
| [[Data Flow]] | How an LSP request travels from JSON-RPC in to a response out |

---

## Concepts

| Page | Summary |
|------|---------|
| [[Symbol Model]] | The `Sym`/`Def`/`Ref`/`Tag`/`Scope` type hierarchy — the lingua franca of the codebase |
| [[Workspace Model]] | `Doc` → `Folder` → `Workspace`: how Marksman organises parsed documents |
| [[Connection Graph]] | `Conn` and the `Oracle`: incremental cross-document reference resolution |
| [[Path Model]] | `AbsPath` / `RelPath` / `RootPath` / `RootedRelPath` and URI conversion |

---

## Features

| Page | Summary |
|------|---------|
| [[Wiki Links]] | Wiki-link syntax, title vs filename resolution, completion style config |
| [[Completions]] | Completion triggers, candidate generation, wiki and MD link variants |
| [[Diagnostics]] | Broken-link and ambiguous-link diagnostics: how they're detected and reported |
| [[Navigation]] | Go-to-definition, find-references, hover preview implementation notes |
| [[Rename]] | Cross-document rename refactoring via `Refactor.fs` |
| [[Table of Contents]] | TOC code action: detection, rendering, insertion-point logic |

---

## Configuration

| Page | Summary |
|------|---------|
| [[Config Reference]] | All `.marksman.toml` / `config.toml` knobs with types, defaults, and effects |

---

## Development

| Page | Summary |
|------|---------|
| [[Build and Test]] | Make targets, dotnet commands, formatter, CI matrix, release pipeline |

---

## Research

Primary input material for the wiki — scraped specs, converted documents, and investigation notes.

| Page | Summary |
|------|---------|
| [[research/index\|Research Index]] | Full catalog of all research files with per-topic tables |
| [[research/Zettelkasten\|Zettelkasten]] | Wikipedia article on the Zettelkasten PKM method (PDF → OFM) |
| [[research/lsp/00-overview\|LSP 3.17 — Overview]] | What's new in LSP 3.17; feature tagging conventions |
| [[research/lsp/01-base-protocol\|LSP 3.17 — Base Protocol]] | JSON-RPC framing, header format, message types, error codes |
| [[research/lsp/02-json-structures\|LSP 3.17 — JSON Structures]] | Core type definitions: Position, Range, Location, TextEdit, WorkspaceEdit |
| [[research/lsp/03-lifecycle\|LSP 3.17 — Lifecycle]] | `initialize`, capability negotiation, `shutdown`, `exit` |
| [[research/lsp/04-synchronization\|LSP 3.17 — Synchronization]] | `textDocument/didOpen`, `didChange`, `didSave`, `didClose`; notebook sync |
| [[research/lsp/05-language-features\|LSP 3.17 — Language Features]] | Completion, diagnostics, rename, hover, code actions, and 40+ other features |
| [[research/lsp/06-workspace-features\|LSP 3.17 — Workspace Features]] | Workspace symbols, file events, configuration, workspace edits |
| [[research/lsp/07-window-features\|LSP 3.17 — Window Features]] | Progress, showMessage, logMessage, meta model |
| [[research/lsp/08-changelog\|LSP 3.17 — Change Log]] | Version history from 3.0 through 3.17 |

---

## Meta

| Page | Summary |
|------|---------|
| [[WIKI]] | Schema: directory layout, conventions, ingest/query/lint operations |
| [[log]] | Append-only operation history |
