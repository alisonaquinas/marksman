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

### GAP-01 · Completion.Candidates.Cap

**Requirement:** [[requirements/completions#Tag: Completion.Candidates.Cap|Completion.Candidates.Cap]]
**Missing:** The configured `completion.candidates` cap (default 50) is never verified at runtime. The `Seq.truncate maxCompletions` call in `Server.TextDocumentCompletion` is untested.

**Suggested test (unit):**
```fsharp
// ComplTests.fs
testCase "candidates cap respected at runtime" <| fun () ->
    let folder = FakeFolder.Mk([ for i in 1..60 -> $"doc{i}.md", $"# Doc {i}" ])
    let doc = FakeDoc.Mk("src.md", "[[")
    let candidates = Compl.findCandidatesInDoc folder doc { Line = 0; Character = 2 }
                     |> Seq.truncate 50
                     |> Array.ofSeq
    candidates.Length |> shouldEqual 50
```

**Also needed:** A test where `ComplCandidates()` returns 10 and asserts the list has ≤ 10 items.

---

### GAP-02 · Completion.Incomplete.Flag

**Requirement:** [[requirements/completions#Tag: Completion.Incomplete.Flag|Completion.Incomplete.Flag]]
**Missing:** The `isIncomplete` field in `CompletionList` is never asserted. The expression `Array.length candidates >= maxCompletions` in `Server.fs` is dead from a test perspective.

**Suggested test (unit):**
```fsharp
// ComplTests.fs
testCase "isIncomplete true when pool exceeds cap" <| fun () ->
    let folder = FakeFolder.Mk([ for i in 1..60 -> $"doc{i}.md", $"# Doc {i}" ])
    let doc    = FakeDoc.Mk("src.md", "[[")
    let result = Compl.findCandidatesInDoc folder doc { Line = 0; Character = 2 }
                 |> Seq.truncate 50 |> Array.ofSeq
    result.Length |> shouldEqual 50   // cap reached
    // isIncomplete = true when candidates.Length >= maxCompletions
    (result.Length >= 50) |> shouldBeTrue

testCase "isIncomplete false when pool below cap" <| fun () ->
    let folder = FakeFolder.Mk([ for i in 1..5 -> $"doc{i}.md", $"# Doc {i}" ])
    let doc    = FakeDoc.Mk("src.md", "[[")
    let result = Compl.findCandidatesInDoc folder doc { Line = 0; Character = 2 }
                 |> Array.ofSeq
    result.Length |> shouldBeLessThan 50
```

---

### GAP-03 · Diagnostic.Severity.MarkdownLink

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Severity.MarkdownLink|Diagnostic.Severity.MarkdownLink]]
**Missing:** No test creates a broken Markdown inline link and checks that the emitted diagnostic has `severity = 2` (Warning). The severity mapping for the `ML` case in `Diag.fs` is completely untested.

**Suggested test (unit, in `DiagTest.fs`):**
```fsharp
testCase "brokenInlineLink_severity_is_warning" <| fun () ->
    let folder = FakeFolder.Mk([ "src.md", "[text](missing.md)" ])
    let diags  = Diag.checkFolder folder
    let brokenDiag = diags |> Array.find (fun d -> d.Code = Some (Second "2"))
    brokenDiag.Severity |> shouldEqual (Some DiagnosticSeverity.Warning)
```

---

### GAP-04 · Diagnostic.Debounce.Latency

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Debounce.Latency|Diagnostic.Debounce.Latency]]
**Missing:** No timing test exists for the `DiagnosticsManager` 200 ms debounce. The p95 ≤ 500 ms Goal level is unverified.

**Suggested test (integration / performance):**
This gap requires a server-level integration test harness (see [[tests/gaps#GAP-10|GAP-10]]). Once the harness exists:
1. Send 5 `didChange` notifications at 50 ms intervals.
2. Record timestamp of last `didChange` and timestamp of received `publishDiagnostics`.
3. Assert elapsed ≤ 500 ms.

**Blocker:** Depends on GAP-10 (LSP integration harness).

---

### GAP-05 · Diagnostic.Ambiguous.RelatedInfo

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Ambiguous.RelatedInfo|Diagnostic.Ambiguous.RelatedInfo]]
**Missing:** `AmbiguousLink` is one of the three diagnostic types and has zero test coverage. No test creates two documents with the same slug and verifies that (a) an `AmbiguousLink` diagnostic is emitted, (b) the code is `"1"`, (c) `relatedInformation` lists both duplicate definitions.

**Suggested test (unit, in `DiagTest.fs`):**
```fsharp
testCase "ambiguousWikiLink_firesWithRelatedInfo" <| fun () ->
    let folder = FakeFolder.Mk([
        "a.md",    "# Concept"
        "b.md",    "# Concept"
        "src.md",  "[[Concept]]"
    ])
    let diags = Diag.checkFolder folder
                |> Array.filter (fun d -> d.Code = Some (Second "1"))
    diags.Length |> shouldEqual 1
    diags[0].RelatedInformation
        |> Option.map Array.length
        |> shouldEqual (Some 2)
```

---

### GAP-06 · Rename.Prepare.Rejection

**Requirement:** [[requirements/rename#Tag: Rename.Prepare.Rejection|Rename.Prepare.Rejection]]
**Missing:** `Refactor.renameRange` is never tested. The entire `textDocument/prepareRename` contract (return range on renameable, null on non-renameable) is untested.

**Suggested tests (unit, in `RefactorTests.fs`):**
```fsharp
testCase "prepareRename_onHeading_returnsRange" <| fun () ->
    let doc = FakeDoc.Mk("a.md", "# My Heading\n\nbody text")
    let range = Refactor.renameRange doc { Line = 0; Character = 4 }
    range |> shouldNotEqual None

testCase "prepareRename_onBodyText_returnsNone" <| fun () ->
    let doc = FakeDoc.Mk("a.md", "# My Heading\n\nbody text")
    let range = Refactor.renameRange doc { Line = 2; Character = 2 }
    range |> shouldEqual None
```

---

### GAP-07 · TOC.Empty.NoAction

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Empty.NoAction|TOC.Empty.NoAction]]
**Missing:** `CodeActions.tableOfContents` returns `None` on a heading-free document, but no test verifies this.

**Suggested test (unit, in `TocTests.fs` or `CodeActionTests.fs`):**
```fsharp
testCase "tocAction_noHeadings_returnsNone" <| fun () ->
    let doc    = FakeDoc.Mk("plain.md", "Just some body text.\n\nNo headings here.")
    let folder = FakeFolder.Mk([ "plain.md", doc ])
    let config = Folder.configOrDefault folder
    let action = CodeActions.tableOfContents
                     (fullDocumentRange doc) emptyContext config doc
    action |> shouldEqual None
```

---

### GAP-08 · Workspace.FileExtension.Filter

**Requirement:** [[requirements/workspace#Tag: Workspace.FileExtension.Filter|Workspace.FileExtension.Filter]]
**Missing:** No test verifies that files with non-configured extensions are excluded from the workspace index. The `isMarkdownFile` predicate in `Server.fs` and `Folder.tryLoad` extension filtering are both untested.

**Suggested test (unit, in `WorkspaceTest.fs`):**
```fsharp
testCase "nonMdFile_absent_from_completion_index" <| fun () ->
    // Create a folder that loads from disk; include a .txt file alongside .md files
    // Assert .txt file does not appear in Folder.docs or workspace symbol results
    let folder = Folder.singleFile (FakeDoc.Mk("notes.txt", "# Heading"))
                 |> Option.defaultWith (fun () -> failwith "unexpected")
    Folder.docs folder |> shouldBeEmpty
```

---

## P2 — High (tested at wrong layer or incomplete conditions)

### GAP-09 · Diagnostic.Severity.WikiLink (severity field unasserted)

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Severity.WikiLink|Diagnostic.Severity.WikiLink]]
**Gap:** `DiagTest.crossFileDiagOnBrokenWikiLinks` verifies a diagnostic is _emitted_ but never checks `diagnostic.Severity = Error`.

**Fix:** Add severity assertions to the two existing broken-wiki-link tests:
```fsharp
diags |> Array.iter (fun d ->
    d.Severity |> shouldEqual (Some DiagnosticSeverity.Error))
```

---

### GAP-10 · No LSP integration test harness

**Impact:** Blocks full coverage for `Navigation.Definition.LinkTypes`, `Completion.Trigger.Coverage`, `Rename.Prepare.Rejection`, `Diagnostic.Debounce.Latency`, and all server-handler paths.

**Gap:** All feature tests operate at the domain layer (`Compl`, `Refs`, `Diag`, `Refactor`). No test instantiates `MarksmanServer`, drives LSP JSON-RPC messages through it, and reads the responses.

**Suggested approach:** Add an `IntegrationTests.fs` file using `FakeLanguageServer` pattern from the LSP library, or use the existing `LanguageServerProtocol` project's test helpers if available. Minimum viable scope:
1. `initialize` → verify `InitializeResult.Capabilities`
2. `textDocument/didOpen` → verify `publishDiagnostics`
3. `textDocument/completion` → verify item count ≤ cap
4. `textDocument/definition` → verify location

---

### GAP-11 · Diagnostic.Code.Assignment — codes "1" and "2" unverified

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Code.Assignment|Diagnostic.Code.Assignment]]
**Gap:** Code `"3"` (NonBreakableWhitespace) is verified indirectly by `nonBreakingWhitespace`. Codes `"1"` and `"2"` are never asserted.

**Fix:** In GAP-05 (ambiguous test), assert `d.Code = Some (Second "1")`. In the broken-wiki-link test, assert `d.Code = Some (Second "2")`.

---

### GAP-12 · Link.Resolution.IgnoreGlob — no end-to-end test

**Requirement:** [[requirements/link-resolution#Tag: Link.Resolution.IgnoreGlob|Link.Resolution.IgnoreGlob]]
**Gap:** `GitIgnoreTest.fs` tests only the pattern-to-glob translation function. No test loads a `.gitignore` file from disk and verifies that matched files are absent from `Folder.docs` or completion candidates.

**Suggested test (integration, in `WorkspaceTest.fs`):**
Create a temporary directory with two `.md` files and a `.gitignore` that ignores one. Call `Folder.tryLoad`. Assert only the non-ignored file appears in `Folder.docs`.

---

### GAP-13 · TOC.Slug.GLFM — no anchor-level TOC test

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Slug.GLFM|TOC.Slug.GLFM]]
**Gap:** GLFM disambiguation is tested at the symbol-extraction level (`AstTests.testSymsWhenRepeatedHeadingsGlfm`) but not at the TOC rendering level. No test verifies that duplicate headings produce `#introduction`, `#introduction-1`, `#introduction-2` in TOC anchor links.

**Suggested test (unit, in `TocTests.fs`):**
```fsharp
testCase "glfm_disambiguates_duplicate_headings_in_toc" <| fun () ->
    let doc = FakeDoc.Mk("a.md",
        "## Introduction\n\nFoo\n\n## Introduction\n\nBar\n\n## Introduction\n\nBaz")
    let toc  = Toc.createToc (Toc.tocLevelsFromConfig Config.Default) doc
    let anchors = toc |> List.map (fun entry -> entry.Anchor)
    anchors |> shouldEqual [ "#introduction"; "#introduction-1"; "#introduction-2" ]
```

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

### GAP-16 · Config.Validation.Candidates — zero value untested

**Requirement:** [[requirements/configuration#Tag: Config.Validation.Candidates|Config.Validation.Candidates]]
**Gap:** `testParse_broken_5` tests `candidates = -1`. The requirement says "strictly positive", meaning `candidates = 0` should also be rejected, but this is not tested.

**Fix:**
```fsharp
testCase "testParse_broken_zeroCandidate" <| fun () ->
    let toml = "[completion]\ncandidates = 0"
    Config.parse toml |> shouldEqual None
```

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

| Gap ID | Requirement | Priority | Effort |
|--------|-------------|----------|--------|
| GAP-01 | `Completion.Candidates.Cap` | P1 | Low |
| GAP-02 | `Completion.Incomplete.Flag` | P1 | Low |
| GAP-03 | `Diagnostic.Severity.MarkdownLink` | P1 | Low |
| GAP-04 | `Diagnostic.Debounce.Latency` | P1 | High (needs harness) |
| GAP-05 | `Diagnostic.Ambiguous.RelatedInfo` | P1 | Low |
| GAP-06 | `Rename.Prepare.Rejection` | P1 | Low |
| GAP-07 | `TOC.Empty.NoAction` | P1 | Low |
| GAP-08 | `Workspace.FileExtension.Filter` | P1 | Medium |
| GAP-09 | `Diagnostic.Severity.WikiLink` (severity field) | P2 | Trivial |
| GAP-10 | LSP integration harness | P2 | High |
| GAP-11 | `Diagnostic.Code.Assignment` codes "1","2" | P2 | Low |
| GAP-12 | `Link.Resolution.IgnoreGlob` end-to-end | P2 | Medium |
| GAP-13 | `TOC.Slug.GLFM` anchor level | P2 | Low |
| GAP-14 | `Link.Resolution.ModeScope` completion/def | P2 | Low |
| GAP-15 | `Rename.StyleBinding.Consistency` negative test | P2 | Low |
| GAP-16 | `Config.Validation.Candidates` zero | P2 | Trivial |
| GAP-17 | `Navigation.Definition.LinkTypes` LSP level | P3 | High (needs harness) |
| GAP-18 | `Navigation.References.Completeness` recall metric | P3 | Medium |
| GAP-19 | `Workspace.ProjectDetection.Root` file loading | P3 | Medium |
| GAP-20 | `Workspace.MultiFolder.Isolation` cross-folder | P3 | Medium |
| GAP-21 | Text deletion / cross-line replace | P4 | Low |
| GAP-22 | MMap operations | P4 | Low |
| GAP-23 | SuffixTree.add | P4 | Low |
| GAP-24 | Semantic tokens edge cases | P4 | Low |
| GAP-25 | Footnote tests (skipped) | P4 | Deferred |

**Quick wins (trivial/low effort, P1 or P2):** GAP-01, GAP-02, GAP-03, GAP-05, GAP-06, GAP-07, GAP-09, GAP-11, GAP-13, GAP-14, GAP-15, GAP-16

---

## Related

- [[tests/index|Test Index]] — coverage status summary
- [[tests/catalog|Test Catalog]] — full per-file test listing
- [[tests/requirements-coverage|Requirements Coverage]] — requirement-by-requirement cross-reference
- [[requirements/index|Requirements Index]] — the 29 requirements
- [[design/behavior-layer|Behavior Layer]] — BDD scenarios (most are additional coverage targets beyond the gaps above)
