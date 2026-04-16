---
title: Wiki Schema
aliases:
  - WIKI
  - Wiki Schema
tags:
  - wiki/meta
---

# Wiki Schema

This document is the operating manual for the Marksman LLM wiki.
It defines structure, conventions, and the three core operations: **ingest**, **query**, and **lint**.
Humans and LLMs co-evolve this file over time.

> [!NOTE] What is this wiki?
> A persistent, compounding knowledge base synthesized from the Marksman source code, `docs/raw/`, and `docs/research/`.
> Rather than re-deriving answers from raw sources on every query, knowledge is compiled once and kept current.
> Cross-references are pre-built. Contradictions are flagged. The wiki grows incrementally.

---

## Directory Layout

```
docs/
  raw/            # Immutable external sources (articles, changelogs, papers)
  research/       # Living investigation notes authored in this repo
  wiki/
    WIKI.md       # ← you are here: schema and operating manual
    index.md      # Master content catalog (all pages, by category)
    log.md        # Append-only chronological operation log
    architecture/ # System design: overview, layers, data-flow
    concepts/     # Domain vocabulary: key types and their relationships
    features/     # LSP feature descriptions and implementation notes
    config/       # Configuration reference
    dev/          # Build, test, release workflow
```

---

## Primary Input Sources

| Source | Path | Notes |
|--------|------|-------|
| Source code | `Marksman/` | Canonical truth; takes precedence over all other sources |
| Existing docs | `docs/*.md` | features, configuration, install, demo |
| Raw sources | `docs/raw/` | External articles and references; never modified |
| Research notes | `docs/research/` | Investigation notes; may evolve |

When sources contradict each other, `Marksman/` source code wins.

---

## Page Conventions

### Frontmatter

Every wiki page must have a YAML frontmatter block:

```yaml
---
title: Page Title
aliases:
  - Alternative Name   # optional; enables [[Alternative Name]] wikilinks
tags:
  - wiki/architecture  # or wiki/concept, wiki/feature, wiki/config, wiki/dev
related:
  - "[[Other Page]]"   # optional; explicit cross-references
---
```

### Tag Taxonomy

| Tag | Used on |
|-----|---------|
| `wiki/meta` | Schema, index, log |
| `wiki/architecture` | Architecture pages |
| `wiki/concept` | Domain concept pages |
| `wiki/feature` | LSP feature pages |
| `wiki/config` | Configuration pages |
| `wiki/dev` | Development workflow pages |
| `wiki/input` | Raw and research input directories |

### Cross-References

- Use `[[Page Title]]` wikilinks for all internal references — never relative Markdown links inside `wiki/`.
- Use `[[Page Title#Heading]]` to link to a specific section.
- Use `[[Page Title|display text]]` when the note title reads awkwardly in context.

### Callout Conventions

| Callout | Purpose |
|---------|---------|
| `> [!NOTE]` | General context or background |
| `> [!TIP]` | Practical guidance |
| `> [!WARNING]` | Gotchas and non-obvious behaviour |
| `> [!QUESTION]` | Open questions to investigate |
| `> [!ABSTRACT]` | TL;DR summary at top of long pages |

---

## Operations

### `ingest`

Run when new content arrives in `docs/raw/`, `docs/research/`, or when significant source changes land.

1. Read the new material.
2. Identify which wiki pages are affected (typically 5–15 pages per ingest).
3. Update those pages: add findings, correct stale claims, add cross-references.
4. Create new pages if a genuinely new concept emerges.
5. Append an entry to [[log]] using the format:

```markdown
## [YYYY-MM-DD] ingest | <source title or commit range>

- Updated: [[Page A]], [[Page B]]
- Created: [[New Page]]
- Notes: one-line summary of key finding
```

### `query`

Run when answering a question that benefits from wiki synthesis.

1. Identify the 2–5 most relevant wiki pages via [[index]].
2. Read those pages.
3. Synthesize an answer grounded in wiki content.
4. If the answer reveals a gap or correction, update the relevant pages.
5. Append an entry to [[log]]:

```markdown
## [YYYY-MM-DD] query | <question summary>

- Consulted: [[Page A]], [[Page B]]
- Gap found: yes/no
- Action: updated [[Page X]] with finding / no action needed
```

### `lint`

Run periodically (or before a milestone) to maintain wiki health.

Check for:
- [ ] Broken wikilinks (target page does not exist)
- [ ] Stale claims (code changed; wiki page not updated)
- [ ] Orphaned pages (no page links to them; not in [[index]])
- [ ] Missing cross-references (two related pages don't link to each other)
- [ ] Contradictions between pages on the same topic
- [ ] Pages with no frontmatter or missing required tags

Append findings to [[log]]:

```markdown
## [YYYY-MM-DD] lint | <scope>

- Broken links: list or "none"
- Stale claims: list or "none"
- Orphaned pages: list or "none"
- Action taken: summary
```

---

## Editing Rules

- **Never** modify files in `docs/raw/`. They are immutable sources.
- **Do** update `docs/research/` files as investigations evolve.
- **Do** prefer updating existing pages over creating new ones.
- **Do** keep each page focused on one concept, feature, or topic.
- **Do not** invent information not supported by the source material.
- **Do not** leave bootstrap/placeholder text in pages after the first review pass.
- Every create or update must produce a [[log]] entry.

---

## See Also

- [[index]] — master content catalog
- [[log]] — operation history
- [[Overview]] — start here for the architecture
