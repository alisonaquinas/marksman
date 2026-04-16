---
title: Symbol Model
aliases:
  - Sym
  - Syms
  - Symbol Hierarchy
tags:
  - wiki/concept
related:
  - "[[Connection Graph]]"
  - "[[Workspace Model]]"
---

# Symbol Model

> [!ABSTRACT]
> The symbol model is the vocabulary Marksman uses to describe every named entity in a Markdown document. Every heading, link, tag, and document identity is classified as a `Sym` — a definition, a reference, or a tag — living in a typed `Scope`. The [[Connection Graph]] resolves references to definitions using this model.

---

## Overview

Every piece of meaningful structure extracted from a parsed document is represented as a ==`Sym`==. The `Sym` type is a three-way discriminated union defined in `Marksman/Syms.fs`:

```fsharp
type Sym =
    | Def  of Def
    | Ref  of Ref
    | Tag  of Tag
```

A `Def` is something that can be *targeted*; a `Ref` is something that *targets*; a `Tag` is a Zettelkasten label that lives in the global namespace.

---

## Definitions (`Def`)

==`Def`== classifies four kinds of named targets:

| Case | Meaning |
|------|---------|
| `Doc` | The document itself as an implicit definition target. Every `.md` file has exactly one `Doc` def, allowing wiki-links like `[[filename]]` to resolve to the file. |
| `Title of string` | The level-1 heading treated as the document's canonical name. When present, cross-doc resolution prefers matching against the title slug rather than the filename. |
| `Header of level:int * id:string` | Any non-title heading, stored with its nesting level and slug id. Used for section-level navigation and `[[doc#heading]]` links. |
| `LinkDef of LinkLabel` | A Markdown reference-style link definition (`[label]: url`). Scoped to its document; enables `[text][label]` inline links. |

> [!NOTE]
> `Def.Title` and `Def.Doc` serve different roles even though both represent "the document". `Doc` is always present and acts as the fallback resolution target; `Title` is only present when an H1 heading exists and provides a human-readable name that can differ from the filename.

---

## References (`Ref`)

==`Ref`== classifies where a link is *pointing*:

```fsharp
type IntraRef =
    | IntraSection of Slug       // [[#heading]] — same-doc section
    | IntraLinkDef of LinkLabel  // [text][label] — same-doc link def

type CrossRef =
    | CrossDoc of string              // [[doc]] — another document by name
    | CrossSection of string * Slug   // [[doc#heading]] — section in another doc

type Ref =
    | IntraRef of IntraRef
    | CrossRef of CrossRef
```

- ==`IntraSection`== — a same-document heading reference, e.g. `[[#introduction]]`. The `Slug` is the normalised heading text.
- ==`IntraLinkDef`== — a reference to a link definition in the same file.
- ==`CrossDoc`== — a wiki-link targeting another document by name string, e.g. `[[my-note]]`.
- ==`CrossSection`== — a composite cross-document link, e.g. `[[my-note#summary]]`. The Conn graph records a synthetic `CrossDoc` dependency alongside every `CrossSection` so that renaming the target document correctly invalidates both.

> [!WARNING]
> When `Doc.syms` serialises symbols for the connection graph it emits an extra synthetic `CrossDoc` ref for every `CrossSection` ref. This keeps the `Conn` dependency graph consistent for invalidation purposes (see [[Connection Graph#refDeps and CrossSection invalidation]]).

---

## Tags (`Tag`)

==`Tag of string`== is a Zettelkasten-style tag (`#mytag`). Unlike definitions and references, tags always resolve into the `Global` scope rather than a document scope. The connection graph adds a direct resolved edge from `(Scope.Doc docId, Sym.Tag tag)` to `(Scope.Global, Sym.Tag tag)` immediately on insertion, with no Oracle lookup needed.

---

## Scope

==`Scope`== describes where a symbol lives:

```fsharp
type Scope =
    | Doc of DocId   // confined to a single document
    | Global         // visible workspace-wide
```

Definitions and references are always assigned `Scope.Doc`; tags resolve to `Scope.Global`. Scope is the key that partitions the [[Connection Graph]]'s ref and def maps, so two documents with identical heading slugs never collide during resolution.

---

## ScopedSym

==`ScopedSym`== is the pairing `Scope * Sym` — the graph's node type. Every edge in the resolved and unresolved graphs connects two `ScopedSym` values. The `lastTouched` field on `Conn` is a `Set<ScopedSym>` and drives diagnostic emission after each incremental update.

---

## ScopeSlug

`ScopeSlug` is a secondary index key used by `Defs.bySlug` inside `Conn`. It is derived from a document's title slug (if a `Title` def exists) or its filename stem (for `Doc` and `Header` defs). The slug-keyed map allows the Oracle to efficiently find which document scopes could satisfy a `CrossDoc` ref without scanning every document.

---

## See Also

- [[Connection Graph]] — uses `ScopedSym` as graph vertices and resolves `Ref` → `Def` edges
- [[Workspace Model]] — the `Structure` layer extracts `Sym` sets from parsed documents
