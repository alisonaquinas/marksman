---
title: Table of Contents
tags:
  - wiki/feature
aliases:
  - TOC
---

# Table of Contents

Marksman provides a ==Table of Contents== code action (`codeAction/tableOfContents`) that generates or replaces a Markdown list of links to every heading in the current document. The implementation lives in `Toc.fs`.

## Code Action Trigger

The TOC action is offered when the client requests code actions (`textDocument/codeAction`) anywhere in a Markdown document, provided `code_action.toc.enable = true` (the default). The action label is **"Insert/Update Table of Contents"** and its kind is `source`. See [[Config Reference]] for the enable flag.

## TableOfContents.mk

`TableOfContents.mk` is the entry point. It:

1. Reads the document's `Cst` to enumerate all headings.
2. Filters headings by level using `code_action.toc.include` (default `[1,2,3,4,5,6]`; set to e.g. `[2,3]` to skip H1 and H4+).
3. Builds a `TableOfContents` record containing the ordered heading list with their levels and text.

> [!NOTE]
> Headings inside fenced code blocks are excluded. The parser marks these nodes so `Toc.fs` can skip them without re-implementing fencing logic.

## Insertion Point Detection

`insertionPoint` scans the document for the best location to place the TOC:

- If an existing TOC block is detected (see below), the insertion point is the full range of that block — enabling in-place replacement.
- Otherwise, the insertion point is placed after the document's H1 (if present) or at the very top of the file if no H1 exists.

## Existing TOC Detection

`detect` identifies an existing TOC by looking for a contiguous block of Markdown list items whose link targets are `#heading-anchor` fragments. The detected range includes any surrounding blank lines so the replacement edit is clean.

> [!TIP]
> Re-running the TOC action is idempotent: it replaces the old TOC rather than appending a duplicate. This makes it safe to bind to a keyboard shortcut and run after adding headings.

## Render Format

`render` converts the filtered heading list into a Markdown bulleted list. Each entry is an intra-document wiki link `[[#heading-slug]]` or a standard Markdown link `[Heading Text](#heading-slug)`, depending on the document's link style. Nesting is represented by two-space indentation per heading level relative to the minimum included level.

Example output for a document with H2 and H3 headings and `toc.include = [2,3]`:

```markdown
- [[#Overview]]
  - [[#Background]]
  - [[#Motivation]]
- [[#Implementation]]
  - [[#Algorithm]]
```

> [!WARNING]
> The TOC uses heading slugs for anchors. If `core.markdown.glfm_heading_ids.enable = false`, the slugs may not match what your renderer produces. See [[Wiki Links]] for details on GLFM heading ID behaviour.

## Related Pages

- [[Config Reference]] — `code_action.toc.enable`, `code_action.toc.include`
- [[Wiki Links]] — `[[#heading]]` intra-document link syntax used in TOC entries
