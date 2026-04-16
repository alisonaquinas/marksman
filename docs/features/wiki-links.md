---
title: Wiki Links
tags:
  - wiki/feature
aliases:
  - Wiki-Links
  - Wikilinks
---

# Wiki Links

==Wiki links== are the primary cross-reference mechanism in Marksman, enabling Zettelkasten-style note navigation. Marksman parses, resolves, and validates wiki-link syntax throughout a [[Workspace Model|workspace]].

## Syntax

| Form | Meaning |
|------|---------|
| `[[doc]]` | Link to another document by title or filename |
| `[[doc#heading]]` | Link to a specific heading inside another document |
| `[[#heading]]` | Intra-document link to a heading in the current file |
| `[[doc\|label]]` | Link with a custom display label |

All three forms participate in go-to-definition, find-references, hover preview, and broken-link diagnostics. See [[Navigation]] for how each resolves at the LSP level.

## Resolution Algorithm

When Marksman resolves `[[doc]]`, it walks through a two-step lookup implemented in `Refs.fs` via `InternName.tryAsPath`:

1. **Title match** — the document whose first H1 heading matches the slug.
2. **File-stem match** — the document whose filename stem (without extension) matches the slug.

The active resolution strategy is controlled by `completion.wiki.style` in [[Config Reference]]:

| Style | Resolution priority |
|-------|---------------------|
| `title-slug` *(default)* | H1 title → file stem |
| `file-stem` | file stem only |
| `file-path-stem` | path-qualified file stem |

> [!NOTE]
> When `core.title_from_heading = true` (the default), the document's H1 is used as its canonical name. Changing a heading therefore affects all wiki links pointing to that document—rename support keeps them consistent. See [[Rename]].

## GLFM Heading IDs

When `core.markdown.glfm_heading_ids.enable = true` (the default), section anchors in `[[doc#heading]]` follow GitHub Labeled Flavored Markdown slugging rules (lowercase, hyphens for spaces, stripped punctuation). This ensures compatibility with GitHub-rendered previews of the same notes.

## Completion

Typing `[[` or a partial `[[doc` triggers the [[Completions]] engine. Candidates are drawn from all documents in the current [[Workspace Model|folder]] and ranked by title. The number of candidates is capped by `completion.candidates` (default 50). The `completion.wiki.style` setting determines whether titles or file stems are inserted as the completion text.

## Cross-document vs Intra-document

- **Cross-document** (`[[doc]]`, `[[doc#heading]]`): resolved against the full folder index. Missing targets become broken-link diagnostics (see [[Diagnostics]]).
- **Intra-document** (`[[#heading]]`): resolved against the headings of the current file only. No cross-folder lookup is performed.

> [!TIP]
> Use `[[#heading]]` for internal navigation tables-of-contents and `[[doc#heading]]` for cross-note deep links. Both forms are completable at the cursor.

## Related Pages

- [[Completions]] — trigger contexts and candidate generation
- [[Diagnostics]] — how broken or ambiguous wiki links produce LSP diagnostics
- [[Config Reference]] — `completion.wiki.style`, `core.title_from_heading`, GLFM heading IDs
- [[Symbol Model]] — how headings become resolvable symbols
- [[research/Zettelkasten|Zettelkasten]] — the card-index PKM method that directly inspired Marksman's wiki-link feature set
