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

## Meta

| Page | Summary |
|------|---------|
| [[WIKI]] | Schema: directory layout, conventions, ingest/query/lint operations |
| [[log]] | Append-only operation history |
