---
title: Navigation
tags:
  - wiki/feature
aliases:
  - Go-to-Definition
  - Find References
---

# Navigation

Marksman implements four LSP navigation features: ==go-to-definition==, ==find references==, hover preview, document symbols, and workspace symbols. All are backed by the `Refs.fs` module and the [[Connection Graph|connection graph]].

## Go-to-Definition

**LSP method:** `textDocument/definition`

When the cursor is on a wiki link or Markdown link, `Server.fs` calls `Index.linkAtPos` to find the link AST node at that position. The node is then passed to `Refs.fs`, which performs the same two-step resolution used at index time (title slug → file stem). The result is a `LocationLink` containing:

- The **target URI** — the file URI of the resolved document.
- The **target range** — for `[[doc]]`, the range of the H1 heading; for `[[doc#heading]]`, the range of the specific heading.
- The **origin selection range** — the span of the link token in the source document.

If the target does not exist, no location is returned and the client typically shows a "no definition found" message. The broken link will also be flagged by [[Diagnostics]].

> [!TIP]
> Go-to-definition also works on `[[#heading]]` intra-document links, jumping to the heading within the same file.

## Find References

**LSP method:** `textDocument/references`

Find-references inverts go-to-definition: given a heading symbol, it returns every wiki link or Markdown link that points to it. The implementation queries `Conn.resolved`, walking the directed graph of `(source ScopedSym → target ScopedSym)` edges and collecting all sources whose target matches the definition at the cursor position.

Because `Conn` spans the entire [[Workspace Model|folder]], find-references is cross-document by default. References in documents from other folders are not included unless those folders are part of the same workspace root.

> [!NOTE]
> Find-references on a heading returns all link forms that resolve to it: `[[doc]]`, `[[doc#heading]]`, and standard Markdown links `[label](doc.md)` alike.

## Hover Preview

**LSP method:** `textDocument/hover`

Hovering over a wiki link renders a Markdown snippet of the link target. The preview content is the first non-empty paragraph of the target document (or the heading context for section links). This reuses the same resolution path as go-to-definition and is computed lazily — only the referenced `Doc`'s `Cst` is read, not re-parsed.

## Document Symbols

**LSP method:** `textDocument/documentSymbol`

Marksman exposes every heading in a document as an LSP `DocumentSymbol`, preserving the heading hierarchy (H1 → H2 → H3 nesting). Editors display these in an outline panel. The heading range includes the full line; the selection range covers only the heading text (excluding the `#` prefix).

## Workspace Symbols

**LSP method:** `workspace/symbol`

The workspace symbol query accepts a free-text string and returns headings across all documents in all open folders. Matching uses ==subsequence matching==: the query characters must appear in order in the symbol name, but need not be contiguous. This lets short queries like `"cfg"` match `"Config Reference"`.

> [!ABSTRACT]
> Subsequence matching is implemented in `Misc.fs` and is the same algorithm used internally by many fuzzy finders. It scores by match position so that prefix matches rank higher than scattered matches.

## Related Pages

- [[Connection Graph]] — the edge store powering find-references
- [[Symbol Model]] — how headings are indexed as symbols
- [[Workspace Model]] — folder and document structure
