---
title: Wiki Log
aliases:
  - log
tags:
  - wiki/meta
---

# Wiki Log

Append-only chronological record of all wiki operations.
Format: `## [YYYY-MM-DD] <operation> | <subject>`
Operations: `ingest`, `query`, `lint`, `init`.

See [[WIKI#Operations]] for operation conventions.

---

## [2026-04-16] ingest | research/ — index research into wiki

- Added `## Research` section to [[index]] (11 LSP spec files + Zettelkasten)
- Added `## Related` sections to all 9 LSP 3.17 spec files pointing to relevant wiki pages
- Added `## Related` section to [[research/Zettelkasten|Zettelkasten]] pointing to [[Wiki Links]]
- Added research back-links to all 11 wiki pages: [[Wiki Links]], [[Completions]], [[Diagnostics]], [[Navigation]], [[Rename]], [[Table of Contents]], [[Overview]], [[Data Flow]], [[Workspace Model]], [[Connection Graph]], [[Config Reference]]

---

## [2026-04-16] init | Marksman codebase — initial wiki setup

- Created: [[WIKI]], [[index]], [[log]]
- Created: [[Overview]], [[Layers]], [[Data Flow]]
- Created: [[Symbol Model]], [[Workspace Model]], [[Connection Graph]], [[Path Model]]
- Created: [[Wiki Links]], [[Completions]], [[Diagnostics]], [[Navigation]], [[Rename]], [[Table of Contents]]
- Created: [[Config Reference]], [[Build and Test]]
- Sources ingested: `Marksman/` source (32 F# files), `docs/features.md`, `docs/configuration.md`, `docs/install.md`, `docs/demo.md`, `README.md`
- Notes: initial ingest from `llm-wiki` branch; zero prior wiki content. All pages are first-draft; run `lint` after each subsequent ingest to catch drift.
