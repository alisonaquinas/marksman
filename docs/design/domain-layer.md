---
title: "Domain Layer"
date: 2026-04-16
tags:
  - wiki/design
  - design/ddd
aliases:
  - Domain Model
  - Bounded Contexts
---

# Domain Layer

DDD analysis of Marksman's domain structure: subdomains, bounded contexts, aggregates, value objects, domain services, and the context map that ties them together.

> [!NOTE] Scope
> This document covers strategic and tactical design — not implementation details. For module-level internals see [[architecture/layers|Layers]] and the [[architecture/data-flow|Data Flow]] walk-through. For the LSP protocol that sits above this layer see [[research/lsp/02-json-structures|LSP 3.17 — JSON Structures]].

---

## The Domain

Marksman's problem domain is **structured navigation and cross-referencing of a linked Markdown document collection**. The core capability — one that no generic text editor provides — is knowing that `[[other-doc#some-heading]]` resolves to exactly one heading in exactly one document, incrementally, as both documents change.

Everything else (path normalisation, LSP wire protocol, configuration) exists to support or expose that capability.

---

## Subdomains

| Subdomain | Type | Description |
|-----------|------|-------------|
| **Cross-document reference resolution** | Core | Slug-based matching of wiki-links to headings across a document collection; incremental consistency under concurrent edits. This is the differentiating capability. |
| **Document lifecycle** | Supporting | Loading, parsing, versioning, and text-change application for individual documents. Important but not the source of differentiation. |
| **Path and identity management** | Supporting | Normalising OS paths, computing document IDs, classifying link texts as exact-absolute / exact-relative / approximate. Necessary but commodity once the rules are known. |
| **Symbol extraction** | Supporting | Turning a parsed CST into the typed `Sym` vocabulary (`Def`, `Ref`, `Tag`). Couples tightly to the core but is mechanically stable. |
| **LSP protocol handling** | Generic | JSON-RPC message framing, capability negotiation, request/response dispatch. Solved by `StreamJsonRpc`; Marksman conforms to it. |
| **Configuration** | Generic | TOML parsing and per-folder / per-user merge. Straightforward layered map with no business rules. |

---

## Ubiquitous Language

Terms that carry precise meanings across the entire codebase. Ambiguity in these terms marks a context boundary.

| Term | Meaning |
|------|---------|
| **Doc** | A single parsed Markdown file; the primary entity. Identity determined by `DocId` (URI + rooted path). |
| **Folder** | A directory tree treated as one project; contains many `Doc`s and owns the connection graph for that tree. Identified by `FolderId` (URI + root path). |
| **Workspace** | The full set of open `Folder`s served by one Marksman process. |
| **Sym** | Any symbol extracted from a document: a `Def` (something linkable), a `Ref` (a link), or a `Tag`. |
| **Def** | A linkable definition: `Doc` (the file itself), `Title` (h1 heading), `Header` (other headings), `LinkDef` (MD link-reference definition). |
| **Ref** | A reference: `IntraRef` (same-document: `[[#heading]]` or link label) or `CrossRef` (cross-document: `[[doc]]` or `[[doc#heading]]`). |
| **Slug** | A case-folded, trimmed string used as the universal match key. `Slug.ofString` is the canonical normaliser. |
| **Scope** | Where a symbol lives: `Doc of DocId` (belongs to a specific document) or `Global` (workspace-wide, used for tags). |
| **ScopedSym** | The unit of currency in the connection graph: a `(Scope × Sym)` pair. |
| **InternName** | A raw link text classified as a potential internal reference; carries the source `DocId`. |
| **InternPath** | How an `InternName` resolves as a path: `ExactAbs` (rooted `/path`), `ExactRel` (contains `./` or `../`), or `Approx` (unanchored — match anywhere in workspace). |
| **Oracle** | A dependency-injection interface that answers two questions: *which scopes could this ref reach?* and *which defs match within a scope?* Decouples `Conn` from the name-matching logic in `Folder`. |
| **Conn** | The connection graph: a bipartite, incrementally maintained graph from `Ref` nodes to `Def` nodes, with a separate `unresolved` side-graph for broken links. |
| **Unresolved** | A broken reference: `FullyUnknown` (no document matched) or `InScope` (document found, no matching heading). |
| **lastTouched** | The set of `ScopedSym`s affected by the most recent `Conn.update` call; a delta, not a cumulative set. |
| **ScopeSlug** | The normalised slug used for cross-document def matching; excludes `LinkDef` (link definitions are never cross-document targets). |
| **Dest** | A fully resolved navigation target: `Doc`, `Heading`, `LinkDef`, or `Tag` — enriched with range information for the LSP go-to-definition response. |

---

## Bounded Contexts

### BC 1 — Path and Identity

**Language**: paths, URIs, OS differences, encoding.

**Owns**: `AbsPath`, `RelPath`, `LocalPath`, `Platform`, `UrlEncoded`, `WikiEncoded`, `FolderId`, `DocId`, `RootedRelPath`, `CanonDocPath`.

**Boundary reason**: URI normalisation rules differ sharply from file-system rules, and Windows vs Unix path semantics mean the same concept requires different representations. This context isolates that complexity so the rest of the model never handles raw strings.

**Key invariants**:
- `AbsPath` construction fails for anything that is neither a Unix absolute path nor a Windows drive-letter path.
- `DocId` equality uses only the URI string — the construction path (from LSP vs from disk) is irrelevant.
- `CanonDocPath` is always lower-cased on Windows so slug lookups remain case-insensitive.

**No aggregates** — all types are value objects with smart constructors.

---

### BC 2 — Document Lifecycle

**Language**: load, parse, open, change, close, version, re-parse.

**Owns**: `Doc`, `Text`, `Structure`, `Index`, `Cst`, `Ast`.

**Boundary reason**: a `Doc` is the unit of LSP state. Its lifecycle is driven by LSP events (`didOpen`, `didChange`, `didClose`, `didSave`) and must stay consistent: text in → parse → index, atomically. No other context needs to know how parsing works.

**Aggregate: `Doc`**

> [!TIP] Aggregate root
> `Doc` is identified by `DocId` and is the single write target for document state.

| Aspect | Detail |
|--------|--------|
| Identity | `DocId` (URI + rooted relative path) |
| State | `text : Text`, `structure : Structure`, `index : Index`, `version : option<int>` |
| Invariant | Text change always triggers full re-parse; the `(text, structure, index)` triple is never partially stale. |
| Invariant | A failed parse raises `DocumentError` — a corrupt CST is never stored silently. |
| Invariant | `version` is `None` for disk-loaded docs; `Some n` for editor-open docs. The two are intentionally distinguishable. |

**Commands and transitions**:

```
Doc.mk              text → Doc                  (factory; raises DocumentError on parse failure)
Doc.fromLsp         TextDocumentItem → Doc      (editor open; sets version)
Doc.tryLoad         LocalPath → option<Doc>     (disk load; None on FileNotFound)
Doc.withText        Text → Doc → Doc            (re-parse; version unchanged)
Doc.applyLspChange  LspParams → Doc → Doc       (apply incremental edits; advance version)
```

**Derived values** (`Doc` computes these on demand):
- `Doc.name` — title heading text, or filename stem if no title present.
- `Doc.slug` — slug of `Doc.name`; the identity used in cross-document matching.
- `Doc.syms` — full `Sym` sequence; synthesises a companion `CrossDoc` ref for every `CrossSection` ref (an invariant of the connection graph).

---

### BC 3 — Cross-Document Reference Resolution

**Language**: resolve, slug, match, scope, orbit, anchor, broken, ambiguous, incremental, paranoid.

**Owns**: `Sym`, `Def`, `Ref`, `IntraRef`, `CrossRef`, `Tag`, `Scope`, `ScopedSym`, `ScopeSlug`, `Conn`, `Oracle`, `Unresolved`, `Defs`, `InternName`, `InternPath`, `Dest`, `FileLink`, `FileLinkKind`, `DocLink`.

**Boundary reason**: this is the core subdomain. The language of "resolution" is fundamentally different from the language of "parsing" (`Doc` context) or "protocol messages" (LSP context). The `Oracle` interface is the formal seam.

#### Aggregate: `Conn`

> [!TIP] Aggregate root
> `Conn` is the consistency boundary for the reference graph. It is owned by `Folder`; `Folder.withDoc` is the only public command that triggers a `Conn` state transition.

| Aspect | Detail |
|--------|--------|
| Identity | Implicit — one `Conn` per `Folder` |
| State | `refs`, `defs`, `tags`, `resolved` graph, `unresolved` graph, `refDeps` graph, `lastTouched` |
| Invariant | Every `CrossSection` ref is always accompanied by a synthetic `CrossDoc` ref in `refDeps`. A title change therefore invalidates all dependent section links. |
| Invariant | Tags always resolve to `Global` scope; they never produce `BrokenLink` diagnostics. |
| Invariant | `lastTouched` is a delta — it is reset on every `update` call, not accumulated. |
| Invariant | With `core.paranoid = true`, every incremental update is validated against a full `Conn.mk` rebuild. Divergence is a hard error. |

**Commands and transitions**:

```
Conn.mk             Oracle → SymMap → Conn           (full rebuild from scratch)
Conn.update         Oracle → Difference<ScopedSym> → Conn → Conn  (incremental update)
```

`Conn.update` is the incremental algorithm's heart:
1. Remove stale tags, refs, defs.
2. Collect reverse-edges pointing to removed defs → `toResolveSet`.
3. Add new tags → resolve immediately to `Global`.
4. Add new defs → re-attempt resolution of `FullyUnknown` unresolved refs.
5. Resolve `toResolveSet`: route each `(scope, ref)` through `Oracle` to edges in `resolved` or `unresolved`.

#### Domain Service: `Oracle`

The `Oracle` record is the anti-corruption layer between `Conn` and the name-matching implementation in `Folder`.

```fsharp
type Oracle = {
    resolveToScope : Scope -> Ref -> Scope[]
    // "which documents could this ref target?"
    resolveInScope : Ref -> Scope -> Def[]
    // "which defs within this document match the ref?"
}
```

`Conn` is pure: it never touches `Folder` state directly. `Folder.fs` provides the `Oracle` implementation by closing over its `FolderLookup` and `SuffixTree`.

#### Domain Service: `Refs`

Higher-level resolution that the LSP feature layer consumes:

| Function | Purpose |
|----------|---------|
| `FileLink.filterMatchingDocs` | Exact-name match: slug or path match via `FolderLookup` |
| `FileLink.filterFuzzyMatchingDocs` | Substring match for completion candidates |
| `FileLinkKind.detect` | Classifies *how* a link text matched its target (path / stem / title) |
| `findElementRefs` | Traverses `Conn.resolved` to enumerate all refs pointing to a def |
| `resolveElement` | Resolves a CST element at a cursor position to a `Dest` |

---

### BC 4 — Folder and Workspace Organisation

**Language**: root, multi-file, single-file, workspace marker, config merge, eviction.

**Owns**: `Folder`, `Workspace`, `FolderData`, `MultiFile`, `SingleFile`, `FolderLookup`.

**Boundary reason**: the rules for how folders and workspaces compose — workspace marker detection, single-file eviction, config layering — are organisational policy, not resolution logic. Keeping them separate prevents `Conn` from knowing about folders and prevents `Doc` from knowing about config inheritance.

#### Aggregate: `Folder`

| Aspect | Detail |
|--------|--------|
| Identity | `FolderId` (URI + root path) |
| State | `FolderData` (MultiFile or SingleFile), `FolderLookup`, `Conn` |
| Invariant | `withDoc` enforces that the new doc's root path matches the folder's root. Cross-folder doc moves are rejected. |
| Invariant | A `SingleFile` folder returns `None` from `withoutDoc` — it ceases to exist when its only doc is removed. |
| Invariant | `closeDoc` on a `MultiFile` folder reloads from disk, not from the editor buffer. The transition on close is disk-read. |
| Invariant | When `CoreIncrementalReferences = false`, `withDoc` always rebuilds `Conn` from scratch (`Conn.mk`). When `true`, it calls `Conn.update` with the `Sym` difference. |

**Commands**:

```
Folder.singleFile    Doc → Folder            (single-file mode factory)
Folder.multiFile     FolderId → seq<Doc> → Folder  (multi-file mode factory)
Folder.tryLoad       FolderId → option<Folder>      (disk load with .gitignore filtering)
Folder.withDoc       Doc → Folder → Folder          (add/replace; updates Conn)
Folder.withoutDoc    DocId → Folder → option<Folder>  (remove; updates Conn)
Folder.closeDoc      DocId → Folder → option<Folder>  (reload from disk)
Folder.withConfig    option<Config> → Folder → Folder  (config change; may rebuild Conn)
```

**Workspace detection**: A directory is treated as a project root when it contains `.marksman.toml`, `.git`, `.hg`, `.svn`, or `.jj`. Absent these markers, a warning is logged but the folder is accepted (client-compatibility behaviour).

#### Aggregate: `Workspace`

| Aspect | Detail |
|--------|--------|
| Identity | Implicit — one per server process |
| State | `folders : Map<FolderId, Folder>`, `config : option<Config>` |
| Invariant | Adding a `MultiFile` folder automatically evicts any `SingleFile` folders whose root path is enclosed by the new folder's root. |
| Invariant | User config is always merged into each folder when the folder is added. `Workspace` is the authority for user-level config propagation. |

**Commands**:

```
Workspace.ofFolders     seq<Folder> → Workspace
Workspace.withFolder    Folder → Workspace → Workspace   (evicts enclosed SingleFile folders)
Workspace.withoutFolder FolderId → Workspace → Workspace
```

---

### BC 5 — Diagnostics

**Language**: broken link, ambiguous link, non-breakable whitespace, suppressed.

**Owns**: `Diag.Entry` (`AmbiguousLink`, `BrokenLink`, `NonBreakableWhitespace`).

**Boundary reason**: diagnostic rules involve business policy decisions — *which broken refs are worth reporting?* — that are distinct from the resolution mechanism itself. A broken ref is a fact from `Conn`; whether to surface it as an LSP diagnostic involves suppression rules that change independently of the resolution logic.

**Domain Service: `Diag`**

| Rule | Policy |
|------|--------|
| `BrokenLink` | Suppressed for shortcut-style MD refs (`[label]` without `[]`) — too noisy in normal prose. |
| `BrokenLink` | Suppressed for inline links whose URL does not look like a Markdown file. |
| `BrokenLink` / `AmbiguousLink` | Both suppressed for cross-document refs when the folder is in single-file mode. |
| `NonBreakableWhitespace` | Emitted when a heading line contains U+00A0 after the `#` marker — a frequent copy-paste error. |

No aggregate — `Diag` is a pure function from `(Folder, Index)` to `list<Entry>`.

---

## Context Map

```
┌─────────────────────────────────────────────────────────────┐
│  BC 5: LSP Protocol (Server.fs / State.fs)                  │
│  Generic — conforms to LSP spec published language          │
└──────────────┬─────────────────────────────────────────────┘
               │ application service
               ▼
┌──────────────────────────────────────────────────────────────┐
│  BC 4: Folder & Workspace Organisation                       │
│  Commands: withDoc / withFolder / withConfig                 │
│  Owns: Folder aggregate, Workspace aggregate                 │
└──────────┬─────────────────────────┬────────────────────────┘
           │ owns Conn               │ owns Docs
           ▼                         ▼
┌──────────────────────┐   ┌─────────────────────────────────┐
│  BC 3: Reference     │   │  BC 2: Document Lifecycle       │
│  Resolution          │◄──│  Doc aggregate                  │
│  Conn aggregate      │   │  Commands: withText /           │
│  Oracle service      │   │            applyLspChange       │
│  Refs service        │   └────────────────┬────────────────┘
└──────────────────────┘                    │ uses
                                            ▼
                              ┌─────────────────────────┐
                              │  BC 1: Path & Identity  │
                              │  Value objects only     │
                              │  DocId, AbsPath, Slug…  │
                              └─────────────────────────┘
```

**Integration styles**:

| Relationship | Style | Notes |
|-------------|-------|-------|
| BC 4 → BC 3 (`Conn`) | **Customer–Supplier** — BC 4 owns `Conn` and calls `Conn.update` | `Folder.withDoc` drives all state changes |
| BC 3 ← `Oracle` | **Anti-Corruption Layer** — `Oracle` record separates `Conn`'s resolution graph from `Folder`'s name-matching logic | `Conn` never imports `Folder` |
| BC 4 → BC 2 (`Doc`) | **Conformist** — `Folder` stores `Doc`s unchanged | `Doc` is parsed outside `Folder`; `Folder` only indexes and routes |
| BC 5 → BC 4 | **Open Host** — `State.fs` is the application-service facade mapping LSP events to workspace mutations | |
| All → BC 1 | **Shared kernel** — `DocId`, `AbsPath`, `Slug` etc. are imported everywhere | Safe because they are pure value types with no business behaviour |
| BC 5 → BC 5 Diag | **Published Language** — `Diag.Entry` is directly serialised into LSP `Diagnostic` JSON | |

---

## Domain Events

Marksman is not event-sourced, but the following state transitions carry domain significance:

| Event (implicit) | Trigger | Consumed by |
|-----------------|---------|------------|
| **DocumentTextChanged** | `Doc.withText` or `Doc.applyLspChange` | `Folder.withDoc` detects symbol difference → `Conn.update` |
| **SymbolsChanged** | `Doc.symsDifference` returns non-empty diff | `Conn.update` via `Folder.withDoc` |
| **ReferenceResolved** | `Conn.update` adds edge in `resolved` graph | `lastTouched` set; `Diag` cleared for that ref |
| **ReferenceUnresolved** | `Conn.update` adds edge in `unresolved` graph | `lastTouched` set; `Diag` emits `BrokenLink` |
| **DocumentClosed** | `Folder.closeDoc` | Triggers disk-reload; buffer state discarded |
| **FolderEnclosed** | `Workspace.withFolder` adds MultiFile folder over an existing SingleFile root | SingleFile folders evicted; their docs subsumed |

> [!WARNING] Not event-sourced
> These are not persisted events — they are state-transition semantics expressed through pure functional pipelines. `lastTouched` is the closest thing to an event log, but it is a mutable delta reset on every `Conn.update` call.

---

## Design Notes and Tradeoffs

> [!NOTE] Why `Conn` is inside `Folder`, not `Workspace`
> Each `Folder` owns its own `Conn` instance. Cross-folder links are not currently resolved — a `CrossRef` in one folder never resolves to a `Def` in another. This is a deliberate simplification: workspaces with multiple roots are unusual in Markdown wiki workflows, and cross-folder resolution would require a workspace-level `Conn` with a much more complex `Oracle`. The design accepts this limitation in exchange for simpler incremental invalidation.

> [!NOTE] Incremental vs full-rebuild policy
> `Conn.update` (incremental) is the default path and is controlled by `core.incremental_references`. The full-rebuild path (`Conn.mk`) is always available as a fallback and is used by paranoid mode to verify correctness. This gives a performance / safety dial without coupling the two algorithms.

> [!NOTE] The `CrossSection` → `CrossDoc` synthesis
> `Doc.syms` synthesises a companion `CrossDoc` ref for every `CrossSection` ref. This feels like a workaround, but it encodes a real domain rule: *a heading link depends on both the document name and the heading*. If the document is renamed, the section link breaks too. The synthesis ensures `refDeps` captures this dependency without `Conn` needing to know the semantics of `CrossSection`.

> [!NOTE] Single-file mode boundary
> `SingleFile` mode is not just a configuration choice — it changes what invariants hold. In single-file mode, cross-document diagnostics are suppressed (there are no other documents to resolve against). `Folder.isSingleFile` is therefore a domain predicate, not just a flag.

> [!NOTE] Where to draw the aggregate boundary for `Conn`
> `Conn` is logically an aggregate root (it guards the consistency of the ref graph) but it is held inside `Folder`. This means `Folder` is a large aggregate that spans two concerns: document storage *and* reference consistency. A future refactoring could extract `Conn` as a first-class aggregate with its own identity (`FolderId`) and treat `Folder` as the factory — but the current flat structure is simpler and the consistency boundary (`withDoc` → `Conn.update`) is clear.

---

## Related

- [[architecture/overview|Overview]] — runtime architecture context
- [[architecture/layers|Layers]] — compile-order layers and module responsibilities
- [[concepts/symbol-model|Symbol Model]] — `Sym`/`Def`/`Ref`/`Tag` type details
- [[concepts/workspace-model|Workspace Model]] — `Doc`/`Folder`/`Workspace` types
- [[concepts/connection-graph|Connection Graph]] — `Conn` internals
- [[concepts/path-model|Path Model]] — `AbsPath`/`DocId`/`RootedRelPath` details
- [[research/lsp/02-json-structures|LSP 3.17 — JSON Structures]] — protocol types that cross the BC 5 boundary
- [[research/Zettelkasten|Zettelkasten]] — the PKM method that motivates the core domain
