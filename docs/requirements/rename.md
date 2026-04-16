---
title: "Requirements — Rename"
date: 2026-04-16
tags:
  - wiki/requirements
  - requirements/rename
---

# Rename Requirements

Requirements governing the two-step rename refactoring flow (`textDocument/prepareRename` + `textDocument/rename`): cross-document completeness, cursor-position validation, and wiki-style–binding consistency.

> [!NOTE] Two-step flow
> LSP rename works in two steps: the server first validates that the cursor is on a renameable element (`prepareRename`), then computes and returns a multi-file edit (`rename`). Both steps must satisfy their respective requirements independently.

---

## Tag: Rename.Refactoring.Completeness

**Gist:** A rename operation must produce a `WorkspaceEdit` that updates every link in the workspace that is bound to the renamed element via the active wiki style — no references may be left unreplaced.

**Ambition:** An incomplete rename silently orphans links: the renamed heading or file no longer matches its old references, but no diagnostic is raised for a short period until the next diagnostic cycle. An author who trusts the rename result ends up with hidden broken links. Completeness is therefore a hard requirement, not a quality-of-life improvement.

**Scale:** For a target element with N inbound links bound via the active wiki style, the ratio: number of text edit entries in the returned `WorkspaceEdit` that replace the old link text ÷ N × 100 (recall). Also, precision: number of correct edits ÷ total edits in the `WorkspaceEdit` × 100 (no spurious edits to unbound links).

**Meter:** Integration test: workspace with 5 documents. Document A has heading `## Concept`. Documents B, C, D each contain `[[A#Concept]]`; document E contains `[[A#Concept]]` under a different wiki style (file-stem, while active style is title-slug — E's link is not style-bound and must not be edited). Set active style to `title-slug`. Invoke `textDocument/rename` on the heading in A with `newName = "New Concept"`. Assert the `WorkspaceEdit` contains exactly 3 edits (B, C, D), not 4 (E must be excluded).

Count correct edits ÷ N × 100 for recall; correct edits ÷ total edits for precision.

**Fail:** Recall < 100 % (any bound reference left unchanged) or Precision < 100 % (any unbound reference incorrectly edited).

**Goal:** 100 % recall and 100 % precision in the returned `WorkspaceEdit` for a known reference graph under a single active wiki style.

**Stakeholders:** Wiki authors, refactoring tool users.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — rename section; `Marksman/Refactor.fs` — `rename`; `Marksman/Server.fs` — `TextDocumentRename`.

**Open questions:**
- What happens when a document is modified but not yet saved — does rename operate on the in-memory version or the on-disk version?
- Does rename apply within a single folder or across all folders in the workspace?

---

## Tag: Rename.Prepare.Rejection

**Gist:** `textDocument/prepareRename` must return a non-null range only when the cursor is positioned on an element that the server knows how to rename; it must return `null` for all other positions.

**Ambition:** `prepareRename` is the guard that prevents the client from opening a rename dialog when no semantic rename is possible. If the server returns a range for a non-renameable position, the editor opens a rename dialog that then fails at the `rename` step — a poor UX. If the server returns `null` for a renameable position, the editor suppresses a valid operation. Both false positive and false negative are failures.

**Scale:** Two sub-scales:
1. **False positive rate:** percentage of `prepareRename` calls on non-renameable positions (body text, URLs, code spans) that return a non-null range. Goal: 0 %.
2. **False negative rate:** percentage of `prepareRename` calls on known renameable positions (heading text, wiki-link text) that return `null`. Goal: 0 %.

**Meter:** Integration test with a document containing one heading `## Target`, one `[[Target]]` wiki-link, and one paragraph of plain body text.
- Call `prepareRename` with cursor on the heading text. Assert non-null range returned.
- Call `prepareRename` with cursor on the wiki-link text. Assert non-null range returned.
- Call `prepareRename` with cursor on plain body text. Assert `null` returned.
- Call `prepareRename` with cursor on a URL in an inline link. Assert `null` returned.

Count each case as pass/fail.

**Fail:** Any call on a renameable position returns `null`, or any call on a non-renameable position returns a range.

**Goal:** 0 % false positive rate and 0 % false negative rate across all tested cursor positions.

**Stakeholders:** Editor-plugin developers.

**Owner:** Marksman contributors.

**Source:** `Marksman/Server.fs` — `TextDocumentPrepareRename`; `Marksman/Refactor.fs` — `renameRange`; LSP 3.17 `textDocument/prepareRename` specification.

---

## Tag: Rename.StyleBinding.Consistency

**Gist:** Rename must update only the links that are bound to the renamed element via the currently active wiki style; links that would be bound under a different style must be left unchanged.

**Ambition:** Marksman supports two distinct slug-binding models: `title-slug` (heading text drives the link) and `file-stem` (file name drives the link). In `title-slug` mode, renaming a heading updates `[[heading-slug]]` links but must not update file-stem links. In `file-stem` mode, the inverse applies. A rename that edits both binding types simultaneously would corrupt a vault where the two styles coexist or where the user is mid-migration between styles.

**Scale:** For a workspace configured with style S, and a rename operation targeting element E, the percentage of edit entries in the `WorkspaceEdit` that affect links bound via style S ÷ total edit entries. Also: the percentage of links bound via the opposite style S′ that are left unchanged ÷ total links bound via S′.

**Meter:** Integration test using two workspaces (one per style):
- **title-slug workspace:** document A titled "Concept"; document B has `[[Concept]]` (title-bound) and separately `[text](concept.md)` (path-bound). Rename the heading in A. Assert B's wiki-link is updated; assert B's inline path link is not updated.
- **file-stem workspace:** document `concept.md` (no H1); document B has `[[concept]]` (file-stem-bound). Rename the H1 heading. Assert B's wiki-link is not updated. (Heading rename in file-stem mode does not affect file-stem-bound links.)

**Fail:** Any edit in the `WorkspaceEdit` targets a link that is not bound via the active style.

**Goal:** 100 % of edits in the `WorkspaceEdit` target only style-bound links; 0 % of non-style-bound links are modified.

**Stakeholders:** Wiki authors managing large vaults.

**Owner:** Marksman contributors.

**Source:** `docs/configuration.md` — `completion.wiki.style`; `docs/features.md` — rename style interaction; `Marksman/Refactor.fs`.

---

## Related

- [[requirements/index|Requirements Index]] — master tag list
- [[requirements/navigation#Tag: Navigation.References.Completeness|Navigation.References.Completeness]] — the read-only complement: finding all refs
- [[requirements/link-resolution#Tag: Link.Wiki.StyleBinding|Link.Wiki.StyleBinding]] — style binding rules
- [[design/behavior-layer|Behavior Layer]] — Feature: Rename BDD scenarios
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — `textDocument/prepareRename` and `textDocument/rename` specification
