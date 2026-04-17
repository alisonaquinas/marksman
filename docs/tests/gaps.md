---
title: "Coverage Gaps"
date: 2026-04-16
tags:
  - wiki/tests
  - tests/gaps
---

# Coverage Gaps

Prioritised analysis of all coverage gaps in the Marksman test suite. Each gap is rated by impact on requirement coverage, difficulty to address, and suggested concrete test.

> [!NOTE] Priority key
> **P1 Critical** — requirement has zero tests; a defect could ship undetected.
> **P2 High** — tested at wrong layer or incomplete conditions; edge cases uncovered.
> **P3 Medium** — unit-only coverage; integration or end-to-end gap.
> **P4 Low** — infrastructure or data-structure edge cases unlikely to affect users.

---

## P1 — Critical (zero test coverage for requirement)

> [!NOTE] All P1 gaps resolved
> All eight previously-critical requirements now have at least one test. The entries below are preserved for historical context.

### GAP-01 · Completion.Candidates.Cap ✅ Resolved

**Requirement:** [[requirements/completions#Tag: Completion.Candidates.Cap|Completion.Candidates.Cap]]
**Resolved by:** `ComplTests.CandidatesCap.rawFunction_notCapped_whenPoolExceedsCap` and `rawFunction_allReturned_whenPoolBelowCap`; `GapTests.CompletionCapTests.*`

---

### GAP-02 · Completion.Incomplete.Flag ✅ Resolved

**Requirement:** [[requirements/completions#Tag: Completion.Incomplete.Flag|Completion.Incomplete.Flag]]
**Resolved by:** `GapTests.CompletionCapTests.*` — four tests covering capped, below-cap, exact-cap, and empty-pool cases.

---

### GAP-03 · Diagnostic.Severity.MarkdownLink ✅ Resolved

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Severity.MarkdownLink|Diagnostic.Severity.MarkdownLink]]
**Resolved by:** `DiagTest.brokenMarkdownLink_hasSeverityWarning`

---

### GAP-04 · Diagnostic.Debounce.Latency ⚠ Partially resolved

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Debounce.Latency|Diagnostic.Debounce.Latency]]
**Resolved by:** `GapTests.DebounceTests.*` — debounce fires and publishes diagnostics after a quiet period; rapid updates coalesce.

**Remaining gap:** p95 ≤ 500 ms timing metric not formally measured. A performance/timing test would require a high-resolution timer harness beyond the current integration tests. See [[tests/gaps#GAP-10|GAP-10]] for context.

---

### GAP-05 · Diagnostic.Ambiguous.RelatedInfo ✅ Resolved

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Ambiguous.RelatedInfo|Diagnostic.Ambiguous.RelatedInfo]]
**Resolved by:** `DiagTest.ambiguousWikiLink_hasCodeOneAndRelatedInfo` — two docs with same slug, asserts code `"1"` and two `relatedInformation` entries.

---

### GAP-06 · Rename.Prepare.Rejection ✅ Resolved

**Requirement:** [[requirements/rename#Tag: Rename.Prepare.Rejection|Rename.Prepare.Rejection]]
**Resolved by:** `RefactorTests.PrepareRenameTests.prepareRename_onHeading_returnsRange` and `prepareRename_onBodyText_returnsNone`

---

### GAP-07 · TOC.Empty.NoAction ✅ Resolved

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Empty.NoAction|TOC.Empty.NoAction]]
**Resolved by:** `TocTests.TocEmptyTests.tocAction_noHeadings_returnsNone`

---

### GAP-08 · Workspace.FileExtension.Filter ✅ Resolved

**Requirement:** [[requirements/workspace#Tag: Workspace.FileExtension.Filter|Workspace.FileExtension.Filter]]
**Resolved by:** `GapTests.FileExtensionTests.*` — three tests covering custom extension, default extension exclusion, and control case.

---

## P2 — High (tested at wrong layer or incomplete conditions)

### GAP-09 · Diagnostic.Severity.WikiLink ✅ Resolved

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Severity.WikiLink|Diagnostic.Severity.WikiLink]]
**Resolved by:** `DiagTest.brokenWikiLink_hasSeverityError` — asserts `severity = Error` on a broken wiki-link diagnostic.

---

### GAP-10 · LSP integration test harness ✅ Resolved

**Resolved by:** `Tests/TestClient.fs` (`CaptureClient`), `Tests/ServerHarness.fs` (`TestServer`), and `GapTests.IntegrationTests.*`.

The harness creates real files on disk, runs `Initialize` + `Initialized`, and exposes `DidOpen` / `WaitDiagnostics`. Three integration tests verify broken-link diagnostics, valid-link no-diagnostics, and `didOpen`-triggered pipeline firing.

---

### GAP-11 · Diagnostic.Code.Assignment — codes "1" and "2" ✅ Resolved

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Code.Assignment|Diagnostic.Code.Assignment]]
**Resolved by:** `DiagTest.brokenWikiLink_hasCodeTwo` (code `"2"`) and `DiagTest.ambiguousWikiLink_hasCodeOneAndRelatedInfo` (code `"1"`).

**Remaining gap:** `source = "Marksman"` field on diagnostics still never asserted.

---

### GAP-12 · Link.Resolution.IgnoreGlob — no end-to-end test

**Requirement:** [[requirements/link-resolution#Tag: Link.Resolution.IgnoreGlob|Link.Resolution.IgnoreGlob]]
**Gap:** `GitIgnoreTest.fs` tests only the pattern-to-glob translation function. No test loads a `.gitignore` file from disk and verifies that matched files are absent from `Folder.docs` or completion candidates.

**Suggested test (integration, in `WorkspaceTest.fs`):**
Create a temporary directory with two `.md` files and a `.gitignore` that ignores one. Call `Folder.tryLoad`. Assert only the non-ignored file appears in `Folder.docs`.

---

### GAP-13 · TOC.Slug.GLFM ✅ Resolved

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Slug.GLFM|TOC.Slug.GLFM]]
**Resolved by:** `TocTests.GlfmTocTests.glfm_disambiguates_duplicate_headings_in_toc` — two identical headings produce `introduction` and `introduction-1` anchor slugs in the rendered TOC.

---

### GAP-14 · Link.Resolution.ModeScope — completion and definition untested in single-file mode

**Requirement:** [[requirements/link-resolution#Tag: Link.Resolution.ModeScope|Link.Resolution.ModeScope]]
**Gap:** Only diagnostic suppression is tested in single-file mode. Completion suppression and definition suppression are untested.

**Fix:** Add two tests in `ComplTests.fs` and `RefsTests.fs`:
1. Single-file folder with a `[[other-doc]]` link → `Compl.findCandidatesInDoc` returns no `other-doc` item.
2. Single-file folder → `Dest.tryResolveElement` on a cross-file link returns `Seq.empty`.

---

### GAP-15 · Rename.StyleBinding.Consistency — no cross-style negative test

**Requirement:** [[requirements/rename#Tag: Rename.StyleBinding.Consistency|Rename.StyleBinding.Consistency]]
**Gap:** The existing rename tests only verify that the correct edits are produced; they do not verify that _incorrect_ edits (to non-bound references) are absent.

**Fix:** In `RefactorTests.HeadingLinks.onTitle`, add a second document with a `file-stem`-bound link and assert it is NOT included in the `WorkspaceEdit`.

---

### GAP-16 · Config.Validation.Candidates — zero value ✅ Resolved

**Requirement:** [[requirements/configuration#Tag: Config.Validation.Candidates|Config.Validation.Candidates]]
**Resolved by:** `ConfigTests.testParse_broken_zeroCandidates` — `candidates = 0` → `None`.

---

## P3 — Medium (unit-only; integration layer missing)

### GAP-17 · Navigation.Definition.LinkTypes — no LSP-level test

**Gap:** `RefsTests.fs` tests the domain-layer resolution function `Dest.tryResolveElement`. No test drives `textDocument/definition` through the actual LSP handler. Blocked by GAP-10.

---

### GAP-18 · Navigation.References.Completeness — no recall/precision measurement

**Gap:** `RefsTests.BasicRefsTests.*` verifies specific known cases but does not construct a ground-truth reference graph and compute recall/precision. Add a parameterised test with a known graph of N references and verify `findElementRefs` returns exactly N results.

---

### GAP-19 · Workspace.ProjectDetection.Root — no .marksman.toml loading test

**Gap:** No test creates a temporary directory with a `.marksman.toml` file and calls `Folder.tryLoad` to verify it detects the project root and loads all `.md` files.

---

### GAP-20 · Workspace.MultiFolder.Isolation — no cross-folder resolution test

**Gap:** No test places two multi-file folders in the same workspace and verifies that a link in folder A does not resolve to a document in folder B.

---

## P4 — Low (infrastructure and data-structure edge cases)

### GAP-21 · Text deletion and cross-line replacement (TextTests)

`TextTests.fs` covers insertion and single-line replacement but has no tests for:
- Deletion (`contentChanges` with empty `text` string)
- Cross-line replacements (range spanning two or more lines)

These exercise `Text.applyTextChange` code paths not currently reached.

---

### GAP-22 · MMap operations beyond difference (MMapTests)

`MMapTests.fs` has one test for `MMap.difference`. Operations `add`, `remove`, `ofSeq`, and `collectValues` are not tested in isolation. These are internal data-structure operations and unlikely to affect user-visible behaviour, but could hide subtle bugs in `Conn` graph incremental updates.

---

### GAP-23 · SuffixTree.add isolation (SuffixTreeTests)

`SuffixTreeTests.fs` tests `filterMatchingValues` and `remove` but not `add` in isolation. The `add` code path is indirectly exercised by `filterTest` but not with edge cases (empty string, duplicate key, very long key).

---

### GAP-24 · Semantic tokens edge cases (SematoTests)

`SematoTests.fs` has one test for a 5-element document. Missing:
- Zero-token document (no headings, no links)
- Document with tokens on adjacent lines (delta-line = 0 check)
- Range-limited encoding (`ofIndexEncodedInRange`)

---

### GAP-25 · Footnote tests (skipped)

Two tests are explicitly skipped with `Skip = "Footnote parsing not implemented"`:
- `ParserTests.FootnoteTests.footnote_1`
- `RefsTests.BasicRefsTests.refToFootnote_atLink`

These should be un-skipped when footnote parsing is implemented, with a corresponding requirement added to `docs/requirements/`.

---

## Gap Summary Table

| Gap ID | Requirement | Priority | Status |
|--------|-------------|----------|--------|
| GAP-01 | `Completion.Candidates.Cap` | P1 | ✅ Resolved |
| GAP-02 | `Completion.Incomplete.Flag` | P1 | ✅ Resolved |
| GAP-03 | `Diagnostic.Severity.MarkdownLink` | P1 | ✅ Resolved |
| GAP-04 | `Diagnostic.Debounce.Latency` | P1 | ⚠ Partial (p95 timing not measured) |
| GAP-05 | `Diagnostic.Ambiguous.RelatedInfo` | P1 | ✅ Resolved |
| GAP-06 | `Rename.Prepare.Rejection` | P1 | ✅ Resolved |
| GAP-07 | `TOC.Empty.NoAction` | P1 | ✅ Resolved |
| GAP-08 | `Workspace.FileExtension.Filter` | P1 | ✅ Resolved |
| GAP-09 | `Diagnostic.Severity.WikiLink` (severity field) | P2 | ✅ Resolved |
| GAP-10 | LSP integration harness | P2 | ✅ Resolved |
| GAP-11 | `Diagnostic.Code.Assignment` codes "1","2" | P2 | ✅ Resolved (source field still untested) |
| GAP-12 | `Link.Resolution.IgnoreGlob` end-to-end | P2 | Open |
| GAP-13 | `TOC.Slug.GLFM` anchor level | P2 | ✅ Resolved |
| GAP-14 | `Link.Resolution.ModeScope` completion/def | P2 | Open |
| GAP-15 | `Rename.StyleBinding.Consistency` negative test | P2 | Open |
| GAP-16 | `Config.Validation.Candidates` zero | P2 | ✅ Resolved |
| GAP-17 | `Navigation.Definition.LinkTypes` LSP level | P3 | Open |
| GAP-18 | `Navigation.References.Completeness` recall metric | P3 | Open |
| GAP-19 | `Workspace.ProjectDetection.Root` file loading | P3 | Open |
| GAP-20 | `Workspace.MultiFolder.Isolation` cross-folder | P3 | Open |
| GAP-21 | Text deletion / cross-line replace | P4 | ⚠ Partial (delete tested; cross-line not yet) |
| GAP-22 | MMap operations | P4 | Open |
| GAP-23 | SuffixTree.add | P4 | Open |
| GAP-24 | Semantic tokens edge cases | P4 | Open |
| GAP-25 | Footnote tests (skipped) | P4 | Deferred |

**Open P2 quick wins (low effort):** GAP-12, GAP-14, GAP-15

---

## Related

- [[tests/index|Test Index]] — coverage status summary
- [[tests/catalog|Test Catalog]] — full per-file test listing
- [[tests/requirements-coverage|Requirements Coverage]] — requirement-by-requirement cross-reference
- [[requirements/index|Requirements Index]] — the 29 requirements
- [[design/behavior-layer|Behavior Layer]] — BDD scenarios (most are additional coverage targets beyond the gaps above)
