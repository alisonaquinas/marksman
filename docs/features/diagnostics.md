---
title: Diagnostics
tags:
  - wiki/feature
---

# Diagnostics

Marksman emits ==LSP diagnostics== for broken and ambiguous wiki-link references. The diagnostic pipeline is implemented in `Diag.fs` and is driven incrementally by the [[Connection Graph|connection graph]] (`Conn.fs`).

## What Triggers a Diagnostic Pass

Diagnostics are not recomputed for the entire workspace on every keystroke. Instead, `Conn.lastTouched` tracks the set of `ScopedSym` entries (symbol + document scope pairs) that were affected by the most recent document change. Only those symbols are re-evaluated, keeping the incremental update cost proportional to the edit rather than the vault size.

The trigger chain is:

1. A document is saved or its text changes (via `textDocument/didChange` or `textDocument/didSave`).
2. The parser re-evaluates the affected `Doc`, updating its `Cst` and `Index`.
3. `Conn` recomputes link edges for the changed document and marks touched symbols.
4. `Diag.fs` iterates over `lastTouched` and re-checks each link symbol against `Conn.resolved` and `Conn.unresolved`.
5. Updated `PublishDiagnostics` notifications are sent to the client for every affected file URI.

## Diagnostic Categories

### Broken Link

A wiki link or Markdown link whose target cannot be found in the [[Connection Graph|Conn]] resolved set. The diagnostic spans the full link text (including brackets) and is reported at severity **Error**.

```
[[NonExistent Doc]]
 ~~~~~~~~~~~~~~~~~ error: Reference not found
```

### Ambiguous Link

A wiki link that resolves to more than one document (multiple docs share the same title slug or file stem). Reported at severity **Warning** because the link is not broken — Marksman picks the first match — but the ambiguity may be unintentional.

> [!WARNING]
> Ambiguous links arise most often when `completion.wiki.style = "file-stem"` and two documents in different subdirectories share the same filename. Switching to `title-slug` with unique H1 titles eliminates most ambiguity.

### No Diagnostics for Intra-document Links

`[[#heading]]` references that target a heading in the same file are validated against the local `Cst` headings only and do not go through `Conn`. A missing local anchor is still reported as an error, but it is resolved faster since no cross-document lookup is needed.

## Per-Folder Aggregation

Each `Folder` maintains its own `WorkspaceDiag` map (file URI → diagnostic list). When multiple folders are open, the server merges their diagnostics before publishing. This means a broken link in folder A does not cause a re-evaluation of folder B's diagnostics.

> [!NOTE]
> Enabling `core.incremental_references = true` changes how `Conn` propagates reference updates. With incremental mode, only directly affected edges are recomputed rather than the full link graph for the folder. This is an experimental setting; see [[Config Reference]].

## Severity Summary

| Condition | Severity |
|-----------|----------|
| Target document not found | Error |
| Target heading not found | Error |
| Multiple matching targets | Warning |

## Related Pages

- [[Connection Graph]] — the `Conn` data structure that tracks resolved/unresolved links
- [[Wiki Links]] — link syntax and what counts as a valid target
- [[Config Reference]] — `core.incremental_references`, `core.paranoid`
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — `textDocument/publishDiagnostics` and pull-diagnostics protocol specifications
