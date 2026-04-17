---
title: "Requirements Coverage"
date: 2026-04-16
tags:
  - wiki/tests
  - tests/coverage
---

# Requirements Coverage

Requirement-by-requirement cross-reference: for each of the 29 Planguage requirement tags, which tests cover it, at what level, and what conditions remain untested.

**Status key:**
- ✅ **Covered** — scale and meter conditions are exercised by at least one test
- ⚠ **Partial** — at least one test touches the requirement, but key scale/meter conditions are absent
- ❌ **None** — no tests exist for this requirement

---

## Link Resolution

### Link.Wiki.StyleBinding ⚠ Partial

**Requirement:** [[requirements/link-resolution#Tag: Link.Wiki.StyleBinding|link-resolution § Style Binding]]

| Test | File | Coverage |
|------|------|---------|
| `Candidates.partialWikiDocHeading_FilePathStem` | `ComplTests.fs` | `FilePathStem` completions produce correct slugs |
| `Candidates.partialWikiDocHeading_FileStem` | `ComplTests.fs` | `FileStem` completions produce correct slugs |
| `Candidates.partialWikiDoc_FileStem_ArbitraryPath` | `ComplTests.fs` | `FileStem` with date-named file |
| `WikiWithSpaces_TitleSlug.test1`–`test3` | `ComplTests.fs` | `TitleSlug` with space-containing titles |
| `WikiWithSpaces_FileStem.test1`–`test3` | `ComplTests.fs` | `FileStem` with space-containing filenames |
| `Candidates.wiki_CrossHeading_TitleNameVsSlug` | `ComplTests.fs` | Title text vs slug distinction |
| `HeadingLinks.onTitle` | `RefactorTests.fs` | Title rename updates wiki-links |
| `HeadingLinks.onSubtitle` | `RefactorTests.fs` | Subtitle rename updates wiki-links and inline links |
| `ConnGraphTests_TitleLess.updateH1` | `ConnTest.fs` | H1 in title-less mode |
| `FileLinkTests.titleSimilarToName` | `RefsTests.fs` | `FileLinkKind.detect` title vs file-name distinction |
| `testParse_3` | `ConfigTests.fs` | `file-stem` style parses from TOML |
| `testDefault_titleVsCompletionStyle` | `ConfigTests.fs` | `title_from_heading = false` → `FileStem` |

**Untested conditions:**
- No test asserts that 100 % of items in a single completion response conform to the style (only individual snapshot cases)
- `FilePathStem` style not tested for rename operations
- No test for switching style mid-session and verifying completion list updates

---

### Link.Resolution.ModeScope ⚠ Partial

**Requirement:** [[requirements/link-resolution#Tag: Link.Resolution.ModeScope|link-resolution § Mode Scope]]

| Test | File | Coverage |
|------|------|---------|
| `noCrossFileDiagOnSingleFileFolders` | `DiagTest.fs` | Diagnostics suppressed in single-file mode |
| `folderAdded_evictSingleFile` | `WorkspaceTest.fs` | Single-file folder evicted when multi-file folder covers it |
| `StateTests.folderFind_singleFile` | `StateTests.fs` | `State.tryFindFolderAndDoc` locates single-file folder |
| `TitleLess.refToH1` | `RefsTests.fs` | H1 not cross-referenceable in title-less mode |

**Untested conditions:**
- Completion does not offer cross-file items in single-file mode
- `textDocument/definition` returns `null` for cross-file links in single-file mode
- `textDocument/references` returns `[]` for cross-file links in single-file mode

---

### Link.Resolution.IgnoreGlob ⚠ Partial

**Requirement:** [[requirements/link-resolution#Tag: Link.Resolution.IgnoreGlob|link-resolution § Ignore Glob]]

| Test | File | Coverage |
|------|------|---------|
| `patternToGlob_Empty`/`absGlob_*`/`relGlob_*` | `GitIgnoreTest.fs` | Pattern-to-glob translation for Unix and Windows |
| `issue_218` | `GitIgnoreTest.fs` | Known limitation: character-class globs not supported |

**Untested conditions:**
- No end-to-end test loading a `.gitignore` and verifying the ignored file is absent from completion candidates
- No test verifying ignored files return `null` from `textDocument/definition`
- Negation patterns (`!important.md`) not tested

---

### Link.Inline.URLSkip ✅ Covered

**Requirement:** [[requirements/link-resolution#Tag: Link.Inline.URLSkip|link-resolution § URL Skip]]

| Test | File | Coverage |
|------|------|---------|
| `noDiagOnRealUrls` | `DiagTest.fs` | `https://` URLs produce no broken-link diagnostic |
| `url_schema` | `RefsTests.fs` | `http://…` InternName returns `None` |
| `url_no_schema_FP` | `RefsTests.fs` | Documents known false positive for `www.` without schema |

---

## Completions

### Completion.Candidates.Cap ❌ None

**Requirement:** [[requirements/completions#Tag: Completion.Candidates.Cap|completions § Candidates Cap]]

| Test | File | Coverage |
|------|------|---------|
| `testParse_7` | `ConfigTests.fs` | `candidates = 100` parses from TOML (config parse only; runtime cap not tested) |

**Untested conditions:**
- `Seq.truncate maxCompletions` never verified to limit response length
- No test with a workspace larger than the default cap (50)
- No test for a custom cap value being respected at runtime

---

### Completion.Trigger.Coverage ⚠ Partial

**Requirement:** [[requirements/completions#Tag: Completion.Trigger.Coverage|completions § Trigger Coverage]]

| Test | File | Coverage |
|------|------|---------|
| `PartialElementWiki.emptyEof`/`emptyEol` | `ComplTests.fs` | `[[` prefix detects wiki-link partial |
| `PartialElementInline.emptyLinkEof`/`emptyLinkEol` | `ComplTests.fs` | `](` prefix detects inline-link partial |
| `Candidates.partialWikiDoc`/`partialWikiHeading` | `ComplTests.fs` | `[[` and `[[#` return candidates |
| `Candidates.partialInlineDoc`/`partialInlineHeading` | `ComplTests.fs` | `[](` and `[link](#` return candidates |
| `Candidates.partialReferenceEmpty` | `ComplTests.fs` | `[` returns link-def candidates |

**Untested conditions:**
- No test verifies the server _advertises_ `[`, `#`, `(` in `CompletionProvider.triggerCharacters`
- No integration test that drives an actual LSP `textDocument/completion` request

---

### Completion.Incomplete.Flag ❌ None

**Requirement:** [[requirements/completions#Tag: Completion.Incomplete.Flag|completions § Incomplete Flag]]

No tests exist for `isIncomplete` being set or cleared.

**Untested conditions:**
- `isIncomplete = true` when pool > cap
- `isIncomplete = false` when pool ≤ cap

---

## Diagnostics

### Diagnostic.Severity.WikiLink ⚠ Partial

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Severity.WikiLink|diagnostics § Severity WikiLink]]

| Test | File | Coverage |
|------|------|---------|
| `crossFileDiagOnBrokenWikiLinks` | `DiagTest.fs` | Broken wiki-link fires a diagnostic (existence verified) |
| `noDiagOnShortcutLinks` | `DiagTest.fs` | Broken `[[#h42]]` fires a diagnostic |

**Untested conditions:**
- `severity` field of emitted diagnostic never asserted to equal `1` (Error)
- `AmbiguousLink` diagnostic severity never tested at all

---

### Diagnostic.Severity.MarkdownLink ❌ None

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Severity.MarkdownLink|diagnostics § Severity MarkdownLink]]

No tests verify the `severity = 2` (Warning) field on broken or ambiguous Markdown link diagnostics.

**Untested conditions:**
- Broken inline link `[text](missing.md)` → `severity = 2`
- Broken reference link → `severity = 2`

---

### Diagnostic.Code.Assignment ⚠ Partial

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Code.Assignment|diagnostics § Code Assignment]]

| Test | File | Coverage |
|------|------|---------|
| `nonBreakingWhitespace` | `DiagTest.fs` | Code `"3"` for `NonBreakableWhitespace` verified indirectly via range check |

**Untested conditions:**
- Code `"1"` for `AmbiguousLink` never tested (no ambiguous-link test exists)
- Code `"2"` for `BrokenLink` never asserted
- `source = "Marksman"` on any diagnostic never asserted

---

### Diagnostic.Debounce.Latency ❌ None

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Debounce.Latency|diagnostics § Debounce Latency]]

No timing tests exist for the `DiagnosticsManager` 200 ms debounce or end-to-end diagnostic delivery latency.

---

### Diagnostic.Ambiguous.RelatedInfo ❌ None

**Requirement:** [[requirements/diagnostics#Tag: Diagnostic.Ambiguous.RelatedInfo|diagnostics § Ambiguous RelatedInfo]]

`AmbiguousLink` (diagnostic code `"1"`) has zero test coverage. No test creates two documents with the same slug and verifies that `relatedInformation` entries are populated.

---

## Navigation

### Navigation.Definition.LinkTypes ⚠ Partial

**Requirement:** [[requirements/navigation#Tag: Navigation.Definition.LinkTypes|navigation § Definition Link Types]]

| Test | File | Coverage |
|------|------|---------|
| `FileLinkTests.fileName_Partial`/`fileName_RelativeAsAbs` | `RefsTests.fs` | File-stem resolution (domain level) |
| `FileLinkTests.heading_Partial` | `RefsTests.fs` | Title-slug heading resolution |
| `LinkKindRefsTests.atWiki_VariousFilenames` | `RefsTests.fs` | Wiki-link resolution across filename variants |
| `LinkKindRefsTests.atWiki_Filenames_Subfolder` | `RefsTests.fs` | Wiki-link from subfolder |
| `EncodingTests.*` | `RefsTests.fs` | URL-encoded paths for inline links |
| `gotoDefTime` | `Benchmarks/Program.fs` | `Dest.tryResolveElement` performance |

**Untested conditions:**
- No integration test drives `textDocument/definition` LSP request end-to-end
- Reference-style link go-to-definition not tested separately from wiki-link
- No test for `GotoResult.Multiple` (ambiguous definition) from the LSP handler

---

### Navigation.References.Completeness ⚠ Partial

**Requirement:** [[requirements/navigation#Tag: Navigation.References.Completeness|navigation § References Completeness]]

| Test | File | Coverage |
|------|------|---------|
| `BasicRefsTests.refToDoc_atTitle`/`_withDecl` | `RefsTests.fs` | Cross-doc refs from H1 title |
| `BasicRefsTests.refToDoc_atLink`/`_withDecl` | `RefsTests.fs` | Cross-doc refs from wiki-link |
| `BasicRefsTests.refToLinkDef_*` | `RefsTests.fs` | Link-def refs with/without declaration |
| `BasicRefsTests.refToTag_*` | `RefsTests.fs` | Tag refs with/without declaration |
| `ConnGraphTests.initGraph`/`addDoc`/`fixRef`/`breakCrossRef` | `ConnTest.fs` | Graph-level completeness after mutations |
| `findRefsTime` | `Benchmarks/Program.fs` | `Dest.findElementRefs` performance at scale |

**Untested conditions:**
- No formal recall/precision measurement across a known ground-truth reference graph
- `includeDeclaration` behaviour for document vs heading references not cross-tested
- No test for `Dest.findElementRefs` with `core.incremental_references = true`

---

### Navigation.CodeLens.Count ✅ Covered

**Requirement:** [[requirements/navigation#Tag: Navigation.CodeLens.Count|navigation § Code Lens Count]]

| Test | File | Coverage |
|------|------|---------|
| `basicHeaderLenses` | `LensesTests.fs` | "1 reference" / "2 references" on headings; unreferenced heading omitted |
| `basicHeaderLenses_withCommandArguments` | `LensesTests.fs` | Lens carries location data when client supports `codeLensFindReferences` |
| `basicLinkDefLenses` | `LensesTests.fs` | Link-def reference count lens |

**Open question addressed in test:** unreferenced headings are omitted (no lens), which answers the open question in `Navigation.CodeLens.Count`.

---

## Rename

### Rename.Refactoring.Completeness ⚠ Partial

**Requirement:** [[requirements/rename#Tag: Rename.Refactoring.Completeness|rename § Refactoring Completeness]]

| Test | File | Coverage |
|------|------|---------|
| `HeadingLinks.onTitle` | `RefactorTests.fs` | H1 rename updates wiki-links in other docs |
| `HeadingLinks.onSubtitle` | `RefactorTests.fs` | H2 rename updates wiki-links and inline links |
| `ReferenceLinks.onRefLabel` | `RefactorTests.fs` | Reference-link label rename at usage site |
| `ReferenceLinks.onDefLabel` | `RefactorTests.fs` | Reference-link label rename at definition site |
| `ConnGraphTests.addTitle_CrossSection` | `ConnTest.fs` | Cross-section title rename at graph level |
| `ConnGraphTests.breakCrossRef` | `ConnTest.fs` | Breaking cross-doc ref at graph level |
| `RenameTests.renameCrossRef_D1_then_D2` | `ConnTest.fs` | Multi-step rename, both ordering variants |

**Untested conditions:**
- Formal count of edits vs known N references not asserted (tests verify specific expected edits, not coverage completeness)
- `supportsDocumentEdit = false` code path not tested
- Tag rename not tested
- File rename (not heading rename) not tested

---

### Rename.Prepare.Rejection ❌ None

**Requirement:** [[requirements/rename#Tag: Rename.Prepare.Rejection|rename § Prepare Rejection]]

No tests exist for `textDocument/prepareRename`. The `Refactor.renameRange` function is called by the handler but never tested in isolation or via the server.

---

### Rename.StyleBinding.Consistency ⚠ Partial

**Requirement:** [[requirements/rename#Tag: Rename.StyleBinding.Consistency|rename § Style Binding Consistency]]

| Test | File | Coverage |
|------|------|---------|
| `HeadingLinks.onTitle` | `RefactorTests.fs` | Title rename updates title-slug-bound wiki-links |
| `HeadingLinks.onSubtitle` | `RefactorTests.fs` | H2 rename updates heading-slug-bound links |

**Untested conditions:**
- No test with both binding styles present in the same workspace verifying only style-bound refs are edited
- `FilePathStem` style renames not tested

---

## Table of Contents

### TOC.Generation.Markers ✅ Covered

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Generation.Markers|table-of-contents § Generation Markers]]

| Test | File | Coverage |
|------|------|---------|
| `DetectToc.detectToc_1` | `TocTests.fs` | No markers → no detection |
| `DetectToc.detectToc_noMarker` | `TocTests.fs` | TOC-like list without markers → no detection |
| `DetectToc.detectToc_withMarker` | `TocTests.fs` | Markers detected at correct range |
| `DocumentEdit.insert_afterYaml`/`insert_afterTopHeading`/`insert_documentBeginning` | `TocTests.fs` | Full round-trips verify marker presence after insert |
| `DocumentEdit.update_atBeginningOfFile` | `TocTests.fs` | Existing TOC replaced; markers preserved |

---

### TOC.Levels.Filter ✅ Covered

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Levels.Filter|table-of-contents § Levels Filter]]

| Test | File | Coverage |
|------|------|---------|
| `testParse_tocInclude` | `ConfigTests.fs` | `toc.include = [2, 3, 4]` parses correctly |
| `RenderToc.createToc_filteredLevels` | `TocTests.fs` | `[2; 3]` filter: H1 and H4 excluded from rendered TOC |
| `CreateToc.createToc` | `TocTests.fs` | Default two-heading TOC renders all included levels |

---

### TOC.Slug.GLFM ⚠ Partial

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Slug.GLFM|table-of-contents § Slug GLFM]]

| Test | File | Coverage |
|------|------|---------|
| `testSymsWhenRepeatedHeadingsGlfm` | `AstTests.fs` | GLFM slug de-duplication at symbol extraction level |
| `testSymsWhenRepeatedHeadingsNoGlfm` | `AstTests.fs` | Without GLFM: no de-duplication at symbol level |
| `testParse_8` | `ConfigTests.fs` | `glfm_heading_ids.enable = true` parses from TOML |

**Untested conditions:**
- No test at the TOC render level that verifies anchor links use `-1`, `-2` suffixes for duplicate headings
- No round-trip test: document with two `## Introduction` headings → TOC → assert `#introduction` and `#introduction-1`

---

### TOC.Empty.NoAction ❌ None

**Requirement:** [[requirements/table-of-contents#Tag: TOC.Empty.NoAction|table-of-contents § Empty No Action]]

No test verifies that `textDocument/codeAction` returns no TOC action for a heading-free document.

---

## Workspace

### Workspace.ProjectDetection.Root ⚠ Partial

**Requirement:** [[requirements/workspace#Tag: Workspace.ProjectDetection.Root|workspace § Project Detection Root]]

| Test | File | Coverage |
|------|------|---------|
| `FolderTest.rooPath_singleFile` | `WorkspaceTest.fs` | `Folder.rootPath` for a single-file folder |
| `FolderTest.updateDoc` | `WorkspaceTest.fs` | Updating docs in a folder |

**Untested conditions:**
- No test explicitly creates a `.marksman.toml` file and verifies that `Folder.tryLoad` successfully loads the directory as a project root
- VCS root detection not tested

---

### Workspace.MultiFolder.Isolation ⚠ Partial

**Requirement:** [[requirements/workspace#Tag: Workspace.MultiFolder.Isolation|workspace § Multi-Folder Isolation]]

| Test | File | Coverage |
|------|------|---------|
| `WorkspaceTest.folderFind_singleFile` | `WorkspaceTest.fs` | `tryFindFolderEnclosing` returns `None` for non-enclosed URI |
| `WorkspaceTest.folderAdded_evictSingleFile` | `WorkspaceTest.fs` | Adding folder evicts single-file folders it encloses |

**Untested conditions:**
- No test with two independent multi-file folders verifying a link in folder A does not resolve to a document in folder B
- No completion test verifying items from the wrong folder are absent

---

### Workspace.FileExtension.Filter ❌ None

**Requirement:** [[requirements/workspace#Tag: Workspace.FileExtension.Filter|workspace § File Extension Filter]]

No tests exist for extension-based file filtering. No test creates a workspace with `.txt`, `.rst`, or other non-Markdown files and verifies they are excluded from the index.

---

## Configuration

### Config.Precedence.Layering ✅ Covered

**Requirement:** [[requirements/configuration#Tag: Config.Precedence.Layering|configuration § Precedence Layering]]

| Test | File | Coverage |
|------|------|---------|
| `textSync_UserConfigEmptyWS` | `ServerTests.fs` | User config overrides default |
| `textSync_NonEmptyWS_PreferIncrButConfigTakesPrecedence` | `ServerTests.fs` | Workspace config overrides client opt |
| `folderConfig_noUserConfig` | `WorkspaceTest.fs` | Folder config preserved without user config |
| `folderConfig_userConfig` | `WorkspaceTest.fs` | User config overrides folder config |
| `folderConfig_userConfig_folderAdd` | `WorkspaceTest.fs` | User config applied to later-added folder |
| `testSymsWhenTitleFromHeadingIsOff` | `AstTests.fs` | Config field `titleFromHeading` observable at symbol level |

---

### Config.Validation.Candidates ⚠ Partial

**Requirement:** [[requirements/configuration#Tag: Config.Validation.Candidates|configuration § Validation Candidates]]

| Test | File | Coverage |
|------|------|---------|
| `testParse_broken_5` | `ConfigTests.fs` | `candidates = -1` (negative) → `None` at parse level |

**Untested conditions:**
- `candidates = 0` (zero, not negative) never tested — the requirement says strictly positive
- No test verifies the server falls back to the default value (50) when the config is rejected

---

### Config.Fault.Isolation ⚠ Partial

**Requirement:** [[requirements/configuration#Tag: Config.Fault.Isolation|configuration § Fault Isolation]]

| Test | File | Coverage |
|------|------|---------|
| `testParse_broken_0` | `ConfigTests.fs` | Bare invalid TOML → `None` |
| `testParse_broken_1`–`broken_6` | `ConfigTests.fs` | Various wrong-type values → `None` |
| `InitOptionTests.extractMalformed` | `StateTests.fs` | Malformed `initializationOptions` → `None` field |

**Untested conditions:**
- No integration test verifying the server _continues serving requests_ after a malformed config is dropped
- No test for a malformed user config combined with a valid project config
- No test for an error-level log entry being emitted on config rejection

---

### Config.TextSync.Default ✅ Covered

**Requirement:** [[requirements/configuration#Tag: Config.TextSync.Default|configuration § Text Sync Default]]

| Test | File | Coverage |
|------|------|---------|
| `textSync_NoConfigEmptyWS` | `ServerTests.fs` | No config → `Full` |
| `textSync_NoConfigEmptyWS_PreferIncr` | `ServerTests.fs` | Client opt present → `clientOption Incremental` |
| `extractCorrect` | `StateTests.fs` | `preferredTextSyncKind` parsing from `initializationOptions` |

---

## Related

- [[tests/index|Test Index]] — summary table and coverage heat map
- [[tests/catalog|Test Catalog]] — full per-file test listing
- [[tests/gaps|Coverage Gaps]] — prioritised gap analysis with suggested tests
- [[requirements/index|Requirements Index]] — the 29 requirements
