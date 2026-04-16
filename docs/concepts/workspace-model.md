---
title: Workspace Model
aliases:
  - Workspace
  - Folder
  - Doc
tags:
  - wiki/concept
related:
  - "[[Overview]]"
  - "[[Connection Graph]]"
  - "[[Symbol Model]]"
  - "[[Path Model]]"
---

# Workspace Model

> [!ABSTRACT]
> The workspace model is Marksman's layered representation of the open editor state. Text is parsed into a Structure, indexed into an Index, wrapped into a Doc, collected into a Folder, and aggregated into a Workspace. Each layer adds capabilities: Structure provides symbol extraction; Index provides fast lookup; Folder adds cross-document resolution via the [[Connection Graph]]; Workspace routes LSP requests to the correct Folder.

---

## Layer Stack

```
Text
 └─ Structure  (CST + AST + Sym set)
     └─ Index  (fast slug/label lookup tables)
         └─ Doc  (id + version + Text + Structure + Index)
             └─ Folder  (Map<CanonDocPath, Doc> + Conn + Config)
                 └─ Workspace  (Map<FolderId, Folder> + user Config)
```

---

## Text

==`Text`== is the raw string buffer paired with a `LineMap` — a sorted array of half-open byte-offset ranges, one per line. The `LineMap` is built once at parse time and enables O(log n) conversion between LSP `Position` (line + character) and flat string offsets. All text-edit operations in `applyTextChange` work through `Text` to keep the line map consistent.

---

## Structure

==`Structure`== bridges the parser output to the rest of the system. It holds:

- **CST** (`Cst.Cst`) — the concrete syntax tree produced by the Markdig-based parser; contains source positions.
- **AST** (`Ast.Ast`) — the abstract syntax tree; strips whitespace and merges multi-line constructs.
- **Sym set** (`Set<Sym>`) — all `Sym` values extracted from the AST; this is the raw material fed to [[Connection Graph]].
- **c2a mapping** — bidirectional `Mapping<Cst.Element, Ast.Element>` so that hover/go-to-definition can map a cursor position (CST) to a symbol (AST → Sym).
- **a2s mapping** — `Mapping<Ast.Element, Sym>` for the AST-to-Sym direction.

> [!TIP]
> The dual-mapping design means LSP features only need to walk one direction: a cursor lands on a CST element, which maps to an AST element, which maps to a `Sym`, which is resolved by the [[Connection Graph]].

---

## Index

==`Index`== is a per-document lookup table derived from the CST by a single O(n) scan. It groups parsed nodes into typed arrays and slug-keyed maps for fast access without re-walking the tree:

| Field | Contents |
|-------|----------|
| `titles` | Array of H1 heading nodes |
| `headings` | Array of all heading nodes |
| `headingsBySlug` | `Map<Slug, list<Node<Heading>>>` |
| `wikiLinks` | Array of wiki-link nodes |
| `mdLinks` | Array of Markdown link nodes |
| `linkDefs` | Array of link-definition nodes |
| `tags` | Array of tag nodes |
| `yamlFrontMatter` | Optional YAML front-matter text node |

`Index` is consumed by LSP features (completions, document symbols, diagnostics) that need to iterate or lookup specific node types without touching the full CST.

---

## Doc

==`Doc`== is the central document record:

```fsharp
type Doc = {
    id: DocId
    version: option<int>
    text: Text
    structure: Structure
    index: Index
}
```

`DocId` wraps a `UriWith<RootedRelPath>` — the LSP URI string paired with a typed path relative to the enclosing folder root (see [[Path Model]]). The `version` field tracks the LSP document version number when the file is open in an editor; it is `None` for documents loaded from disk that have no open editor buffer.

`Doc` implements value equality on `(id, text)`, so two `Doc` instances for the same file at different versions compare unequal — important for the symbol-diff logic in `Doc.symsDifference`.

> [!WARNING]
> `Doc.syms` synthesises an extra `CrossDoc` ref for every `CrossSection` ref before feeding them to the [[Connection Graph]]. This is a purposeful consistency hack: without it the `refDeps` dependency edges would be missing for composite links like `[[note#section]]`.

---

## Folder

==`Folder`== exists in two modes:

**MultiFile mode** is the normal operating mode, activated when a directory contains `.marksman.toml`. It holds:
- `root: FolderId` — the URI-keyed root path.
- `docs: Map<CanonDocPath, Doc>` — documents keyed by their canonical path (extension-stripped, for case-insensitive name matching).
- `conn: Conn` — the incremental [[Connection Graph]] for cross-document resolution.
- `config: option<Config>` — merged per-folder configuration.

**SingleFile mode** is activated when the LSP client opens a file that is not inside any known project root. It holds a single `Doc` and an optional `Config`. Single-file folders are automatically evicted when a multi-file folder whose root encloses them is later opened.

> [!NOTE]
> The `CanonDocPath` key strips the file extension and normalises case, which is why `note.md` and `NOTE.md` resolve to the same document on case-insensitive file systems. See [[Path Model]] for details.

---

## Workspace

==`Workspace`== is the top-level aggregate:

```fsharp
type Workspace = {
    config: option<Config>
    folders: Map<FolderId, Folder>
}
```

The `userConfig` is propagated down into each `Folder` at insertion time via `mergeFolderConfig`. LSP request routing uses `Workspace.tryFindFolderEnclosing`: given an `AbsPath` from a document URI, it walks `folders` and returns the first `Folder` whose `RootPath.contains` test succeeds.

> [!TIP]
> Multi-root workspace layouts (multiple `workspaceFolders` in the LSP `initialize` request) are fully supported — each root becomes a separate `Folder` entry in the `folders` map.

---

## See Also

- [[Path Model]] — `DocId`, `RootedRelPath`, `CanonDocPath` type details
- [[Connection Graph]] — the `Conn` value inside each `Folder`
- [[Symbol Model]] — `Sym` types extracted from `Structure`
- [[Overview]] — high-level architecture context
- [[research/lsp/04-synchronization|LSP 3.17 — Document Synchronization]] — the LSP messages that trigger `Doc` creation and updates
- [[research/lsp/06-workspace-features|LSP 3.17 — Workspace Features]] — workspace folder and file-watching protocol
