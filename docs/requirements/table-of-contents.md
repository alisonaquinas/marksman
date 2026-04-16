---
title: "Requirements — Table of Contents"
date: 2026-04-16
tags:
  - wiki/requirements
  - requirements/table-of-contents
---

# Table of Contents Requirements

Requirements for the TOC code action: delimiter markers, heading-level filtering, GLFM slug disambiguation, and the no-action guarantee on heading-free documents.

> [!NOTE] Scope
> The TOC feature is surfaced as a `textDocument/codeAction` of kind `source`. It creates or updates a delimited TOC block inside the document. All requirements assume the action is enabled (`code_action.toc.enable = true`, the default).

---

## Tag: TOC.Generation.Markers

**Gist:** Every generated or updated TOC block must be bounded by the exact HTML comment markers `<!--toc:start-->` and `<!--toc:end-->`, with the TOC list contained between them.

**Ambition:** The markers are the contract between Marksman and the document. On a subsequent invocation, Marksman locates the existing TOC by finding these markers and replaces the content between them. If the markers are missing, malformed, or inconsistent, a subsequent TOC action either produces a duplicate block or fails silently. Authors who manually inspect or post-process the document also rely on the markers to identify generated content.

**Scale:** Percentage of `WorkspaceEdit` text edits produced by `textDocument/codeAction` (TOC kind) that, when applied, result in a document containing the exact marker strings `<!--toc:start-->` and `<!--toc:end-->` with the TOC list between them and no content outside the markers that belongs to the TOC block.

**Meter:** Integration test: open a document with 3 headings and no existing TOC. Invoke `textDocument/codeAction` (TOC kind) and apply the edit. Read the resulting document. Assert:
1. The string `<!--toc:start-->` appears exactly once.
2. The string `<!--toc:end-->` appears exactly once.
3. `<!--toc:start-->` precedes `<!--toc:end-->` in the document.
4. The lines between the markers form a non-empty Markdown list.

Invoke the action again on the already-TOC'd document. Assert the above conditions still hold and the markers do not appear a second time.

**Fail:** Any `WorkspaceEdit` application results in a document that is missing either marker, has mismatched marker order, or has more than one occurrence of either marker.

**Goal:** 100 % of TOC edits produce (or preserve) well-formed, single-occurrence start/end markers.

**Stakeholders:** Wiki authors, documentation tooling.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — Table of Contents section; `Marksman/Toc.fs` — marker constants; `Marksman/CodeActions.fs` — TOC action.

---

## Tag: TOC.Levels.Filter

**Gist:** The generated TOC must include entries only for headings whose level is in the `code_action.toc.include` list (default `[1, 2, 3, 4, 5, 6]`); headings at excluded levels must not appear in the TOC.

**Ambition:** Authors with deeply nested documents may want a TOC that shows only top-level structure (e.g., levels 1 and 2). An include filter that is not honoured forces authors to manually delete unwanted entries on every regeneration — defeating the automation value of the feature.

**Scale:** For a document with headings at levels 1–6 and a configured `toc.include` list that excludes one or more levels, the percentage of TOC list entries that correspond to a heading at a level in the include list ÷ total TOC entries × 100 (precision). Also, percentage of headings at included levels that appear in the TOC ÷ total headings at included levels × 100 (recall).

**Meter:** Integration test: document with one heading at each of levels 1–6. Set `code_action.toc.include = [1, 2]`. Invoke TOC action and apply. Parse the resulting TOC list. Assert:
- Exactly 2 entries are present (levels 1 and 2).
- Entries for levels 3–6 are absent.

Precision = 2 ÷ 2 = 100 %. Recall = 2 ÷ 2 = 100 %.

Repeat with default include list `[1, 2, 3, 4, 5, 6]`; assert all 6 headings appear.

**Fail:** Precision < 100 % (TOC contains an entry for an excluded level) or Recall < 100 % (TOC omits an entry for an included level).

**Goal:** 100 % precision and 100 % recall for TOC entries relative to the configured include list.

**Stakeholders:** Wiki authors, documentation authors.

**Owner:** Marksman contributors.

**Source:** `docs/configuration.md` — `code_action.toc.include`; `Marksman/Toc.fs` — level filter; `Marksman/Config.fs` — default include list `[1..6]`.

---

## Tag: TOC.Slug.GLFM

**Gist:** When `core.markdown.glfm_heading_ids.enable = true` (the default), TOC anchor links for duplicate heading texts must use GLFM disambiguation suffixes (`-1`, `-2`, …) so that each anchor uniquely identifies its target heading.

**Ambition:** A document with two `## Introduction` headings produces two identical slugs under basic slug generation. Without disambiguation, both TOC entries link to the same anchor, and clicking the second entry navigates to the first heading. GLFM disambiguation appends a numeric suffix to all occurrences after the first, producing unique anchors that clients (GitHub, GitLab, Obsidian) recognise.

**Scale:** For a document with N headings that share a common text (N ≥ 2), the percentage of TOC anchor links that correctly disambiguate by occurrence order, where "correct" means the 1st occurrence has no suffix, the 2nd has `-1`, the 3rd has `-2`, and so on (per GLFM convention).

**Meter:** Integration test: document with three `## Introduction` headings. Set `core.markdown.glfm_heading_ids.enable = true`. Invoke TOC action and apply. Parse the TOC anchor href values. Assert:
- First entry href = `#introduction`.
- Second entry href = `#introduction-1`.
- Third entry href = `#introduction-2`.

**Fail:** Any anchor in the TOC for a duplicate heading text does not match the expected GLFM-disambiguated slug for its ordinal position.

**Goal:** 100 % of anchor links in the TOC correctly reflect GLFM disambiguation when the feature is enabled.

**Stakeholders:** Authors publishing to GitHub, GitLab, or Obsidian.

**Owner:** Marksman contributors.

**Source:** `docs/configuration.md` — `core.markdown.glfm_heading_ids`; `Marksman/Names.fs` — slug disambiguation; `Marksman/Toc.fs` — anchor generation.

**Open questions:**
- When `core.markdown.glfm_heading_ids.enable = false`, what slug scheme is used for duplicate headings, and what is the expected anchor format?

---

## Tag: TOC.Empty.NoAction

**Gist:** When a document contains no headings, `textDocument/codeAction` must not offer a TOC code action for that document.

**Ambition:** A TOC code action on a heading-free document would produce an empty or marker-only block — noise that the author must manually delete. Suppressing the action entirely is cleaner and respects the principle that code actions should only be offered when they produce a useful result.

**Scale:** Number of `textDocument/codeAction` responses for a heading-free document that include a TOC action item ÷ total requests × 100. Expected: 0 %.

**Meter:** Integration test: open a document consisting of only body text and no `#` headings. Invoke `textDocument/codeAction` with the full document range. Assert the response contains no action item whose `kind` is `source` and whose title includes "TOC" or "table of contents". Count non-empty TOC action responses ÷ total responses × 100.

**Fail:** Any `textDocument/codeAction` response for a heading-free document includes a TOC action item.

**Goal:** 0 % of requests on heading-free documents return a TOC action.

**Stakeholders:** Wiki authors, editor UX.

**Owner:** Marksman contributors.

**Source:** `Marksman/CodeActions.fs` — `tableOfContents` early-return on empty heading list; `Marksman/Toc.fs`.

---

## Related

- [[requirements/index|Requirements Index]] — master tag list
- [[design/behavior-layer|Behavior Layer]] — Feature: Table of Contents BDD scenarios
- [[features/toc|Table of Contents]] — feature overview
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — `textDocument/codeAction` specification
