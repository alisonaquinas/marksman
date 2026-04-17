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

## [2026-04-16] init | docs/tests/ — test coverage index with requirement cross-reference

- Created [[tests/index|Test Index]]: 23 test files, ~321 tests, 2 skipped; coverage status per all 29 requirements (5 ✅ / 16 ⚠ / 8 ❌)
- Created [[tests/catalog|Test Catalog]]: per-file listing of every test name with requirement tag annotations
- Created [[tests/requirements-coverage|Requirements Coverage]]: requirement-by-requirement cross-reference table
- Created [[tests/gaps|Coverage Gaps]]: 25 prioritised gaps (GAP-01 through GAP-25) with suggested F# test code; 8 P1 critical (zero tests), 8 P2 high, 5 P3 medium, 4 P4 low
- Key P1 gaps: `AmbiguousLink` diagnostic has zero coverage; `isIncomplete` flag never tested; `prepareRename` never tested; `completion.candidates` cap never verified at runtime
- Added `## Tests` section to [[index]]
- Sources: all 23 `.fs` files under `Tests/`, `Benchmarks/Program.fs`

---

## [2026-04-16] init | docs/requirements/ — Planguage functional requirements

- Created [[requirements/index|Requirements Index]] with 29-row master tag table
- Created 8 feature-area requirement files: link-resolution (4 reqs), completions (3), diagnostics (5), navigation (3), rename (3), table-of-contents (4), workspace (3), configuration (4)
- Each requirement carries: Tag, Gist, Ambition, Scale, Meter, Fail/Goal where source evidence supports them; skeletons with Open questions where evidence is insufficient
- Requirements grounded in: `Marksman/Config.fs` (candidate cap default 50, text-sync default Full), `Marksman/Server.fs` (200ms debounce, trigger chars, severity mapping), `docs/features.md`, `docs/configuration.md`
- Added `## Requirements` section to [[index]]

---

## [2026-04-16] init | docs/design/api-layer — LSP API layer design document

- Created [[design/api-layer|API Layer]] (440 lines)
- Contents: transport overview, capability negotiation table, full LSP method catalog (lifecycle / sync / language / workspace), concurrency model diagram, 9 Mermaid sequence diagrams, state lifecycle, error handling table
- Sequence diagrams cover: initialize handshake, didOpen, didChange, completion, go-to-definition, find references, rename (2-step), diagnostic push (background debounce), workspace folder change
- Sources: `Marksman/Server.fs`, `Marksman/State.fs`

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
