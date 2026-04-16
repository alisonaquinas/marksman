---
title: Raw Sources
tags:
  - wiki/input
  - wiki/raw
---

# Raw Sources

Immutable primary input documents for the [[WIKI|LLM wiki]].

Drop articles, papers, release notes, changelogs, or any external reference material here.
The `ingest` operation reads from this directory and updates wiki pages accordingly.
Files here are **never modified** by the wiki maintenance process.

## Conventions

- One file per source document.
- Preferred filename pattern: `YYYY-MM-DD-slug.md` for dated material; `slug.md` for evergreen references.
- Include frontmatter where possible:

```markdown
---
title: "Source Title"
source: https://original-url
date: YYYY-MM-DD
tags:
  - raw/article
---
```

> [!TIP] What belongs here?
> External articles, upstream changelogs, RFC texts, benchmark results, competitor analysis — anything you want the wiki to synthesize but that you did not write yourself.
