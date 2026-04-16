---
title: "Requirements — Navigation"
date: 2026-04-16
tags:
  - wiki/requirements
  - requirements/navigation
---

# Navigation Requirements

Requirements governing go-to-definition, find-references, and the reference-count code lens that appears on each heading.

> [!NOTE] Scope
> "Navigation" here covers read-only operations that help an author move between documents. Write operations (rename) are governed by [[requirements/rename|Rename Requirements]]. Single-file mode restrictions on cross-file navigation are governed by [[requirements/link-resolution#Tag: Link.Resolution.ModeScope|Link.Resolution.ModeScope]].

---

## Tag: Navigation.Definition.LinkTypes

**Gist:** Go-to-definition must resolve a link to its target document and optional heading scope for all three supported link syntaxes: wiki-link, Markdown inline link, and Markdown reference link.

**Ambition:** Authors use all three syntaxes in the same vault. A go-to-definition that works only for wiki-links forces authors to remember which syntax is supported, creates inconsistency, and leaves a significant portion of in-document links unnavigable. Uniform support across syntaxes is the baseline expectation for a fully functional LSP server.

**Scale:** For each of the three link syntaxes, the binary outcome: does `textDocument/definition` return at least one `Location` when the cursor is on a link that resolves to a known document? Expressed as number of syntaxes returning a non-null result ÷ 3 × 100.

**Meter:** Integration test with a workspace containing two documents. In a source document, place:
1. A wiki-link `[[target-doc]]` pointing to the second document.
2. An inline link `[text](target-doc.md)` pointing to the same document.
3. A reference link `[text][ref]` + `[ref]: target-doc.md` pointing to the same document.

Invoke `textDocument/definition` with the cursor on each link. Assert each returns a `Location` whose `uri` resolves to `target-doc.md`. Count successful ÷ 3 × 100.

**Fail:** Any of the three syntaxes returns `null` for a link that resolves to a known document in the workspace.

**Goal:** 100 % of the three link syntax types return a valid `Location` when the link resolves.

**Stakeholders:** Wiki authors, editor-plugin developers.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — go-to-definition section; `Marksman/Server.fs` — `TextDocumentDefinition`; `Marksman/Refs.fs` — `Dest.tryResolveElement`.

---

## Tag: Navigation.References.Completeness

**Gist:** Find-references must return every link element across all documents in the enclosing folder that resolves to the target heading or document, with no false positives and no false negatives.

**Ambition:** An incomplete reference list misleads the author about how widely a heading is linked. Missing references mean a rename or deletion will silently leave orphaned links. False positives add noise and erode trust. The reference list must be the ground truth for the current workspace state.

**Scale:** For a target element (heading or document) with N known inbound links in a folder, the ratio:
- **Recall:** links found ÷ N × 100 % — no false negatives.
- **Precision:** links found that are genuine ÷ links found × 100 % — no false positives.

Both measured across a controlled workspace with a known reference graph.

**Meter:** Integration test: create a folder with 5 documents. Document A has one heading `## Concept`. Documents B, C, D each contain one wiki-link to `[[A#Concept]]`; document E contains an inline link `[text](a.md#concept)`. Invoke `textDocument/references` on the heading in A with `includeDeclaration = false`. Assert result contains exactly 4 locations (B, C, D, E). Assert no location points to the heading definition itself.

Repeat with `includeDeclaration = true`; assert 5 locations (B, C, D, E, plus the definition in A).

**Fail:** Recall < 100 % (any genuine reference missing) or Precision < 100 % (any spurious location included).

**Goal:** 100 % recall and 100 % precision for `textDocument/references` in a workspace with a complete, known reference graph.

**Stakeholders:** Wiki authors, refactoring tool users.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — find references section; `Marksman/Server.fs` — `TextDocumentReferences`; `Marksman/Refs.fs` — `Dest.findElementRefs`.

**Open questions:**
- Is recall guaranteed when `core.incremental_references = true`? Are there known consistency windows?
- Does `includeDeclaration` behaviour differ between heading and document references?

---

## Tag: Navigation.CodeLens.Count

**Gist:** The server must offer a code lens on each heading that displays the count of references to that heading, enabling authors to assess the heading's importance without running an explicit find-references query.

**Ambition:** A visible reference count on each heading gives authors at-a-glance insight into which concepts are heavily cross-referenced. This guides decisions about restructuring, splitting, or renaming sections. The count must be accurate; an incorrect count is worse than no count.

**Scale:** For each heading in an open document, the binary: is a `CodeLens` item returned by `textDocument/codeLens` at that heading's line? And: does the lens title display a count equal to the actual number of references in the folder? Expressed as percentage of headings with correct code lenses.

**Meter:** Integration test: workspace with documents A (3 headings), B (links to heading 1 of A twice), C (link to heading 2 of A once). Invoke `textDocument/codeLens` on A. Assert:
- Heading 1 has a lens with title containing `"2"`.
- Heading 2 has a lens with title containing `"1"`.
- Heading 3 has a lens with title containing `"0"` (or no lens — implementation choice; test documents actual behaviour).

Count headings with correct-count lens ÷ total headings × 100.

**Fail:** Any heading with at least one reference lacks a code lens, or any code lens displays an incorrect reference count.

**Goal:** 100 % of headings in open documents have a code lens, and each lens count equals the actual reference count in the folder.

**Stakeholders:** Wiki authors.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — code lens section; `Marksman/Lenses.fs`; `Marksman/Server.fs` — `TextDocumentCodeLens`.

**Open questions:**
- Is a `"0"` count lens shown for unreferenced headings, or is the lens omitted? The implementation choice should be reflected in the goal.
- Does the `SupportsLensFindReferences` client capability affect whether lenses are returned at all?

---

## Related

- [[requirements/index|Requirements Index]] — master tag list
- [[requirements/rename|Rename Requirements]] — write counterpart to navigation
- [[requirements/link-resolution#Tag: Link.Resolution.ModeScope|Link.Resolution.ModeScope]] — single-file mode suppression
- [[design/behavior-layer|Behavior Layer]] — Feature: Go-to-definition and Find-references scenarios
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — definition, references, codeLens specification
