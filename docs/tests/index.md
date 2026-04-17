---
title: "Test Index"
date: 2026-04-16
tags:
  - wiki/meta
  - tests/index
---

# Test Index

Complete test coverage documentation for the Marksman test suite. Every test file is catalogued, every requirement is mapped to the tests that cover it, and every coverage gap is recorded.

> [!NOTE] Test framework
> Tests use **xunit** with **Snapper** for snapshot assertions and **FsCheck** for property-based tests. Test discovery uses `Expecto` style `testList`/`testCase` via the `Marksman.Tests` project. Benchmarks are in a separate `Benchmarks/` project using **BenchmarkDotNet**.

---

## File Summary

| File | Module | Tests | Level | Feature area |
|------|--------|------:|-------|-------------|
| [[tests/catalog#AstTests\|AstTests.fs]] | `AstTests` | 6 | Unit | Parsing, symbol extraction |
| [[tests/catalog#CodeActionTests\|CodeActionTests.fs]] | `CodeActionTests` | 2 | Unit | Code actions (create-missing-file) |
| [[tests/catalog#ComplTests\|ComplTests.fs]] | `ComplTests` | 73 | Unit / Snapshot | Completions |
| [[tests/catalog#ConfigTests\|ConfigTests.fs]] | `ConfigTests` | 20 | Unit | Configuration |
| [[tests/catalog#ConnTest\|ConnTest.fs]] | `ConnTest` | 19 | Integration / Snapshot | Connection graph |
| [[tests/catalog#DiagTest\|DiagTest.fs]] | `DiagTest` | 7 | Unit | Diagnostics |
| [[tests/catalog#GitIgnoreTest\|GitIgnoreTest.fs]] | `GitIgnoreTest` | 10 | Unit | File exclusion |
| [[tests/catalog#Helpers\|Helpers.fs]] | `Helpers` | 0 | N/A | Test infrastructure |
| [[tests/catalog#LensesTests\|LensesTests.fs]] | `LensesTests` | 3 | Unit | Code lenses |
| [[tests/catalog#MMapTests\|MMapTests.fs]] | `MMapTests` | 1 | Unit | MMap data structure |
| [[tests/catalog#MiscTests\|MiscTests.fs]] | `MiscTests` | 26 | Unit | Utilities, slugs, paths |
| [[tests/catalog#ParserTests\|ParserTests.fs]] | `ParserTests` | 50 | Unit / Snapshot | Parsing |
| [[tests/catalog#PathsTests\|PathsTests.fs]] | `PathsTests` | 9 | Unit | Path / URI handling |
| [[tests/catalog#RefactorTests\|RefactorTests.fs]] | `RefactorTests` | 4 | Unit | Rename refactoring |
| [[tests/catalog#RefsTests\|RefsTests.fs]] | `RefsTests` | 33 | Unit | Reference resolution |
| [[tests/catalog#SematoTests\|SematoTests.fs]] | `SematoTests` | 1 | Unit | Semantic tokens |
| [[tests/catalog#ServerTests\|ServerTests.fs]] | `ServerTests` | 5 | Unit | Server / text-sync negotiation |
| [[tests/catalog#StateTests\|StateTests.fs]] | `StateTests` | 4 | Unit | Server state |
| [[tests/catalog#SuffixTreeTests\|SuffixTreeTests.fs]] | `SuffixTreeTests` | 2 | Unit | SuffixTree data structure |
| [[tests/catalog#SymbolsTests\|SymbolsTests.fs]] | `SymbolsTests` | 4 | Unit | Workspace / doc symbols |
| [[tests/catalog#TextTests\|TextTests.fs]] | `TextTests` | 10 | Unit | Text model / incremental edit |
| [[tests/catalog#TocTests\|TocTests.fs]] | `TocTests` | 17 | Unit | Table of Contents |
| [[tests/catalog#WorkspaceTest\|WorkspaceTest.fs]] | `WorkspaceTest` | 10 | Unit / Integration | Workspace, folder, doc |
| [[tests/catalog#Benchmarks\|Benchmarks/Program.fs]] | `Benchmark` | 2 | Performance | goto-def, find-refs |

**Total runnable tests: ~321** (2 skipped — footnote parsing not implemented)
**Benchmark scenarios: 6** (2 benchmarks × 3 folder sizes)

---

## Requirements Coverage Summary

Full cross-reference at [[tests/requirements-coverage|Requirements Coverage]].

| Tag | Covered by | Status |
|-----|-----------|--------|
| `Link.Wiki.StyleBinding` | `ComplTests` (style modes), `RefactorTests` | ⚠ Partial |
| `Link.Resolution.ModeScope` | `DiagTest`, `WorkspaceTest` | ⚠ Partial |
| `Link.Resolution.IgnoreGlob` | `GitIgnoreTest` (pattern only) | ⚠ Partial |
| `Link.Inline.URLSkip` | `DiagTest.noDiagOnRealUrls` | ✅ Covered |
| `Completion.Candidates.Cap` | — | ❌ No tests |
| `Completion.Trigger.Coverage` | `ComplTests` (partial-element detection) | ⚠ Partial |
| `Completion.Incomplete.Flag` | — | ❌ No tests |
| `Diagnostic.Severity.WikiLink` | `DiagTest` (existence only, not severity field) | ⚠ Partial |
| `Diagnostic.Severity.MarkdownLink` | — | ❌ No tests |
| `Diagnostic.Code.Assignment` | `DiagTest.nonBreakingWhitespace` (code `"3"` only) | ⚠ Partial |
| `Diagnostic.Debounce.Latency` | — | ❌ No tests |
| `Diagnostic.Ambiguous.RelatedInfo` | — | ❌ No tests |
| `Navigation.Definition.LinkTypes` | `RefsTests` (resolution logic) | ⚠ Partial |
| `Navigation.References.Completeness` | `RefsTests.BasicRefsTests.*` | ⚠ Partial |
| `Navigation.CodeLens.Count` | `LensesTests.basicHeaderLenses` | ✅ Covered |
| `Rename.Refactoring.Completeness` | `RefactorTests.HeadingLinks.*`, `ReferenceLinks.*` | ⚠ Partial |
| `Rename.Prepare.Rejection` | — | ❌ No tests |
| `Rename.StyleBinding.Consistency` | `RefactorTests.HeadingLinks.*` | ⚠ Partial |
| `TOC.Generation.Markers` | `TocTests.DocumentEdit.*` | ✅ Covered |
| `TOC.Levels.Filter` | `TocTests.RenderToc.createToc_filteredLevels` | ✅ Covered |
| `TOC.Slug.GLFM` | `AstTests.testSymsWhenRepeatedHeadingsGlfm` (sym level only) | ⚠ Partial |
| `TOC.Empty.NoAction` | — | ❌ No tests |
| `Workspace.ProjectDetection.Root` | `WorkspaceTest.FolderTest.*` | ⚠ Partial |
| `Workspace.MultiFolder.Isolation` | `WorkspaceTest.folderFind_singleFile` | ⚠ Partial |
| `Workspace.FileExtension.Filter` | — | ❌ No tests |
| `Config.Precedence.Layering` | `WorkspaceTest` (config tests), `ServerTests` | ✅ Covered |
| `Config.Validation.Candidates` | `ConfigTests.testParse_broken_5` | ⚠ Partial |
| `Config.Fault.Isolation` | `ConfigTests.testParse_broken_*` | ⚠ Partial |
| `Config.TextSync.Default` | `ServerTests.textSync_NoConfigEmptyWS` | ✅ Covered |

**Legend:** ✅ Covered · ⚠ Partial (requirement logic tested but not all scale/meter conditions) · ❌ No tests

**Score: 5 fully covered · 16 partial · 8 with no tests**

---

## Gap Heat Map

Full gap analysis at [[tests/gaps|Coverage Gaps]].

### Critical (requirement has no tests at all)

- `Completion.Candidates.Cap` — hard cap (default 50) never verified
- `Completion.Incomplete.Flag` — `isIncomplete` signal never tested
- `Diagnostic.Severity.MarkdownLink` — Warning severity on MD broken links never verified
- `Diagnostic.Debounce.Latency` — 200 ms debounce / p95 ≤ 500 ms never measured
- `Diagnostic.Ambiguous.RelatedInfo` — `AmbiguousLink` diagnostic type has zero test coverage
- `Rename.Prepare.Rejection` — `prepareRename` null-return contract never tested
- `TOC.Empty.NoAction` — action suppression on heading-free doc never tested
- `Workspace.FileExtension.Filter` — extension filtering never verified end-to-end

### High (tested at wrong layer or incomplete conditions)

- `Diagnostic.Severity.WikiLink` — diagnostics emitted but `severity` field not asserted
- `Diagnostic.Code.Assignment` — only code `"3"` tested; codes `"1"` and `"2"` untested
- `Link.Resolution.IgnoreGlob` — pattern matching tested in isolation; no end-to-end index exclusion test
- `TOC.Slug.GLFM` — GLFM slug disambiguation tested at symbol level, not at TOC anchor level

---

## Related

- [[tests/catalog|Test Catalog]] — full per-file test name listing
- [[tests/requirements-coverage|Requirements Coverage]] — requirement-by-requirement cross-reference
- [[tests/gaps|Coverage Gaps]] — prioritised gap analysis with suggested tests
- [[requirements/index|Requirements Index]] — the 29 requirements being covered
- [[design/behavior-layer|Behavior Layer]] — BDD scenarios (most unautomated — represent additional coverage targets)
