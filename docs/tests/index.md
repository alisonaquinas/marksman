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
| [[tests/catalog#ComplTests\|ComplTests.fs]] | `ComplTests` | 75 | Unit / Snapshot | Completions |
| [[tests/catalog#ConfigTests\|ConfigTests.fs]] | `ConfigTests` | 21 | Unit | Configuration |
| [[tests/catalog#ConnTest\|ConnTest.fs]] | `ConnTest` | 19 | Integration / Snapshot | Connection graph |
| [[tests/catalog#DiagTest\|DiagTest.fs]] | `DiagTest` | 11 | Unit | Diagnostics |
| [[tests/catalog#GitIgnoreTest\|GitIgnoreTest.fs]] | `GitIgnoreTest` | 10 | Unit | File exclusion |
| [[tests/catalog#GapTests\|GapTests.fs]] | `GapTests` | 12 | Unit / Integration | Completions, Diagnostics, Workspace, LSP integration |
| [[tests/catalog#Helpers\|Helpers.fs]] | `Helpers` | 0 | N/A | Test infrastructure |
| [[tests/catalog#LensesTests\|LensesTests.fs]] | `LensesTests` | 3 | Unit | Code lenses |
| [[tests/catalog#MMapTests\|MMapTests.fs]] | `MMapTests` | 1 | Unit | MMap data structure |
| [[tests/catalog#MiscTests\|MiscTests.fs]] | `MiscTests` | 26 | Unit | Utilities, slugs, paths |
| [[tests/catalog#ParserTests\|ParserTests.fs]] | `ParserTests` | 50 | Unit / Snapshot | Parsing |
| [[tests/catalog#PathsTests\|PathsTests.fs]] | `PathsTests` | 9 | Unit | Path / URI handling |
| [[tests/catalog#RefactorTests\|RefactorTests.fs]] | `RefactorTests` | 6 | Unit | Rename refactoring |
| [[tests/catalog#RefsTests\|RefsTests.fs]] | `RefsTests` | 33 | Unit | Reference resolution |
| [[tests/catalog#SematoTests\|SematoTests.fs]] | `SematoTests` | 1 | Unit | Semantic tokens |
| [[tests/catalog#ServerHarness\|ServerHarness.fs]] | `ServerHarness` | 0 | N/A | Test infrastructure (in-process LSP server) |
| [[tests/catalog#ServerTests\|ServerTests.fs]] | `ServerTests` | 5 | Unit | Server / text-sync negotiation |
| [[tests/catalog#StateTests\|StateTests.fs]] | `StateTests` | 4 | Unit | Server state |
| [[tests/catalog#SuffixTreeTests\|SuffixTreeTests.fs]] | `SuffixTreeTests` | 2 | Unit | SuffixTree data structure |
| [[tests/catalog#SymbolsTests\|SymbolsTests.fs]] | `SymbolsTests` | 4 | Unit | Workspace / doc symbols |
| [[tests/catalog#TestClient\|TestClient.fs]] | `TestClient` | 0 | N/A | Test infrastructure (LSP notification capture) |
| [[tests/catalog#TextTests\|TextTests.fs]] | `TextTests` | 11 | Unit | Text model / incremental edit |
| [[tests/catalog#TocTests\|TocTests.fs]] | `TocTests` | 19 | Unit | Table of Contents |
| [[tests/catalog#WorkspaceTest\|WorkspaceTest.fs]] | `WorkspaceTest` | 10 | Unit / Integration | Workspace, folder, doc |
| [[tests/catalog#Benchmarks\|Benchmarks/Program.fs]] | `Benchmark` | 2 | Performance | goto-def, find-refs |

**Total runnable tests: 336** (3 skipped — 2 footnote parsing, 1 special-characters path)
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
| `Completion.Candidates.Cap` | `ComplTests (CandidatesCap)`, `GapTests` | ✅ Covered |
| `Completion.Trigger.Coverage` | `ComplTests` (partial-element detection) | ⚠ Partial |
| `Completion.Incomplete.Flag` | `GapTests (CompletionCapTests)` | ✅ Covered |
| `Diagnostic.Severity.WikiLink` | `DiagTest` | ✅ Covered |
| `Diagnostic.Severity.MarkdownLink` | `DiagTest.brokenMarkdownLink_hasSeverityWarning` | ✅ Covered |
| `Diagnostic.Code.Assignment` | `DiagTest` (codes `"1"`, `"2"`, `"3"` tested; `source` field untested) | ⚠ Partial |
| `Diagnostic.Debounce.Latency` | `GapTests (DebounceTests)` | ⚠ Partial |
| `Diagnostic.Ambiguous.RelatedInfo` | `DiagTest.ambiguousWikiLink_hasCodeOneAndRelatedInfo` | ✅ Covered |
| `Navigation.Definition.LinkTypes` | `RefsTests` (resolution logic) | ⚠ Partial |
| `Navigation.References.Completeness` | `RefsTests.BasicRefsTests.*` | ⚠ Partial |
| `Navigation.CodeLens.Count` | `LensesTests.basicHeaderLenses` | ✅ Covered |
| `Rename.Refactoring.Completeness` | `RefactorTests.HeadingLinks.*`, `ReferenceLinks.*` | ⚠ Partial |
| `Rename.Prepare.Rejection` | `RefactorTests (PrepareRenameTests)` | ✅ Covered |
| `Rename.StyleBinding.Consistency` | `RefactorTests.HeadingLinks.*` | ⚠ Partial |
| `TOC.Generation.Markers` | `TocTests.DocumentEdit.*` | ✅ Covered |
| `TOC.Levels.Filter` | `TocTests.RenderToc.createToc_filteredLevels` | ✅ Covered |
| `TOC.Slug.GLFM` | `AstTests`, `ConfigTests`, `TocTests (GlfmTocTests)` | ✅ Covered |
| `TOC.Empty.NoAction` | `TocTests (TocEmptyTests)` | ✅ Covered |
| `Workspace.ProjectDetection.Root` | `WorkspaceTest.FolderTest.*` | ⚠ Partial |
| `Workspace.MultiFolder.Isolation` | `WorkspaceTest.folderFind_singleFile` | ⚠ Partial |
| `Workspace.FileExtension.Filter` | `GapTests (FileExtensionTests)` | ✅ Covered |
| `Config.Precedence.Layering` | `WorkspaceTest` (config tests), `ServerTests` | ✅ Covered |
| `Config.Validation.Candidates` | `ConfigTests` (negative and zero both rejected) | ⚠ Partial |
| `Config.Fault.Isolation` | `ConfigTests.testParse_broken_*` | ⚠ Partial |
| `Config.TextSync.Default` | `ServerTests.textSync_NoConfigEmptyWS` | ✅ Covered |

**Legend:** ✅ Covered · ⚠ Partial (requirement logic tested but not all scale/meter conditions) · ❌ No tests

**Score: 15 fully covered · 14 partial · 0 with no tests**

---

## Gap Heat Map

Full gap analysis at [[tests/gaps|Coverage Gaps]].

### Critical (requirement has no tests at all)

No P1 gaps remain. All eight previously-critical requirements now have at least one test.

### High (tested at wrong layer or incomplete conditions)

- `Diagnostic.Code.Assignment` — codes `"1"`, `"2"`, `"3"` tested; `source = "Marksman"` field still unasserted
- `Diagnostic.Debounce.Latency` — basic debounce firing tested; p95 ≤ 500 ms timing metric not measured
- `Link.Resolution.IgnoreGlob` — pattern matching tested in isolation; no end-to-end index exclusion test

---

## Related

- [[tests/catalog|Test Catalog]] — full per-file test name listing
- [[tests/requirements-coverage|Requirements Coverage]] — requirement-by-requirement cross-reference
- [[tests/gaps|Coverage Gaps]] — prioritised gap analysis with suggested tests
- [[requirements/index|Requirements Index]] — the 29 requirements being covered
- [[design/behavior-layer|Behavior Layer]] — BDD scenarios (most unautomated — represent additional coverage targets)
