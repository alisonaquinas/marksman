---
title: Connection Graph
aliases:
  - Conn
  - Connection Graph
tags:
  - wiki/concept
related:
  - "[[Symbol Model]]"
  - "[[Workspace Model]]"
  - "[[Diagnostics]]"
---

# Connection Graph

> [!ABSTRACT]
> The Connection Graph (`Conn`) is Marksman's incremental cross-document reference resolver. It maintains two directed graphs — resolved (ref → def edges) and unresolved (ref → scope edges) — and updates them efficiently as documents change, rather than rebuilding from scratch. The set of symbols touched by the last update drives LSP diagnostic emission.

---

## Purpose

Every time a wiki-link or Markdown link is typed in an editor, Marksman must know whether it resolves to an existing heading or document. The ==`Conn`== record (defined in `Marksman/Conn.fs`) holds the complete resolution state for all documents in a [[Workspace Model#Folder]].

---

## Data Model

```fsharp
type Conn = {
    refs: MMap<Scope, Ref>             // all known refs per scope
    defs: Defs                          // all known defs (by scope, and by ScopeSlug)
    tags: MMap<Scope, Tag>
    resolved: Graph<ScopedSym>         // edges: ref ScopedSym → def ScopedSym
    unresolved: Graph<Unresolved>      // edges: Unresolved.Ref → Unresolved.Scope
    refDeps: Graph<Scope * CrossRef>   // CrossSection deps on CrossDoc
    lastTouched: Set<ScopedSym>        // symbols changed in last update
}
```

All types are value-immutable records; `updateAux` returns a fresh `Conn`. The `MMap` (multi-valued map) and `Graph` types come from the foundation layer and support efficient set-based difference operations.

---

## The Oracle Contract

==`Oracle`== is the bridge between the connection graph and the rest of the workspace:

```fsharp
type Oracle = {
    resolveToScope: Scope -> Ref -> Scope[]
    resolveInScope: Ref -> Scope -> Def[]
}
```

- `resolveToScope` answers: *given that this ref originates in scope S, which document scopes could it target?* For a `CrossDoc "notes"` ref this walks the `ScopeSlug` index to find any document whose title or filename slug matches.
- `resolveInScope` answers: *within scope T, which defs match this ref?* For a `CrossSection` ref this looks up the `headingsBySlug` index of the target document.

The Oracle is constructed in `Folder` and captures the current document map by closure. This separation means `Conn` itself has no knowledge of filenames or the file system — it only speaks in `Scope`, `Ref`, and `Def` terms.

---

## Resolved vs Unresolved Graphs

The ==`resolved`== graph stores successful `ref → def` edges as `ScopedSym` pairs. A single ref can resolve to multiple defs (an ambiguous link), and a single def can be targeted by many refs.

The ==`unresolved`== graph stores failed resolutions as edges from an `Unresolved.Ref` to an `Unresolved.Scope`:

| Unresolved.Scope variant | Meaning |
|--------------------------|---------|
| `FullyUnknown` | Oracle returned no candidate scopes at all — document does not exist anywhere. |
| `InScope scope` | Oracle found the target document scope but no matching def inside it — heading does not exist. |

The two variants produce different LSP diagnostics: `FullyUnknown` is a "document not found" error; `InScope` is a "heading not found" error.

---

## Incremental Update Algorithm

`Conn.update oracle (Difference<ScopedSym>) conn` is the core incremental operation. `Difference<ScopedSym>` holds the `added` and `removed` symbol sets from a document edit (computed by `Doc.symsDifference`).

The algorithm proceeds in five phases:

1. **Remove tags** — straightforward; no cascading effects on refs or defs.
2. **Remove refs** — evict from `refs`, remove vertex from `resolved` and `unresolved` graphs. Any pending resolution work for that ref is also cancelled.
3. **Remove defs** — this is the most complex phase. Removing a def may *break* previously resolved refs. The algorithm walks the reverse edges of the resolved graph to find all refs that pointed to the removed def, and enqueues them for re-resolution. For `Doc`/`Title` defs, the `InScope` unresolved-scope node for that document is also cleared so stale "heading not found" edges are removed.
4. **Add symbols** — new refs are enqueued for resolution; new defs trigger invalidation of refs that match the same `ScopeSlug` (a new title could make previously-unresolved refs resolvable); new tags are resolved immediately to `Scope.Global`.
5. **Resolution pass** — all queued `(scope, ref)` pairs are run through the Oracle. For each pair, `resolveToScope` is called first; if it returns empty the ref goes to `FullyUnknown`; otherwise `resolveInScope` is called per target scope and edges are added to `resolved` or `unresolved` accordingly.

> [!WARNING]
> Removing a `Def.Title` or `Def.Doc` triggers additional invalidation: any ref that previously resolved to that document's scope group (looked up via `ScopeSlug`) is also re-queued. This is necessary because renaming a document's H1 heading changes which `CrossDoc` refs match it.

---

## refDeps and CrossSection Invalidation

==`refDeps`== is an auxiliary dependency graph: `Graph<Scope * CrossRef>`. When a `CrossSection(doc, section)` ref is added, a synthetic `CrossDoc(doc)` ref and an edge `(scope, CrossDoc doc) → (scope, CrossSection doc section)` are recorded. This means that when a `CrossDoc` ref is invalidated (e.g. the target document is renamed), the algorithm automatically re-queues all `CrossSection` refs that depend on it.

> [!NOTE]
> `Doc.syms` emits a synthetic `CrossDoc` sym for every `CrossSection` sym precisely so that `refDeps` edges are recorded consistently. Without this, `[[note#section]]` links would survive a document rename when `[[note]]` links correctly broke.

---

## lastTouched as the Diagnostic Signal

==`lastTouched`== (`Set<ScopedSym>`) accumulates every `ScopedSym` added, removed, or re-resolved during the current `updateAux` call. After `Conn.update` returns, `Diag` reads `lastTouched` to know which documents need their diagnostics regenerated, rather than re-scanning all documents. This keeps diagnostic latency proportional to the size of the edit, not the size of the workspace.

---

## Building from Scratch

`Conn.mk oracle symMap` performs a full rebuild by treating all existing symbols as newly added. It calls `update` with a `Difference` whose `removed` set is empty, reusing the same incremental logic. Full rebuilds occur only on folder load or when the paranoid-mode consistency check detects a divergence.

---

## See Also

- [[Symbol Model]] — `ScopedSym`, `Ref`, `Def`, `Scope` type definitions
- [[Workspace Model]] — `Folder` owns the `Conn` and constructs the Oracle
- [[Diagnostics]] — consumes `lastTouched` to emit LSP diagnostic messages
- [[research/lsp/06-workspace-features|LSP 3.17 — Workspace Features]] — workspace symbol and file-event protocol that drives `Conn` updates
