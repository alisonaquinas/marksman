---
title: "Test Catalog"
date: 2026-04-16
tags:
  - wiki/tests
  - tests/catalog
---

# Test Catalog

Per-file listing of every test in the Marksman test suite. Tests are grouped by file with feature area, testing level, and every test name. Requirement tags are noted where a test directly exercises a specified requirement.

> [!TIP] Finding a test
> Use Obsidian search or `Ctrl+F` on this page. Each section header matches the source file name.

---

## AstTests

**Source:** `Tests/AstTests.fs` · **Level:** Unit · **Feature:** Parsing, symbol extraction

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `testAstShape` | Overall element list: headings, wiki-links, inline links, tags, link defs | — |
| `testAstLookup` | `Structure.findConcreteForAbstract` / `findMatchingAbstract` round-trip for a heading | — |
| `testSymsWhenTitleFromHeadingIsOff` | Symbol list with `titleFromHeading = false` | `Config.Precedence.Layering` |
| `testSymsWhenRepeatedHeadingsGlfm` | Slug de-duplication with GLFM mode on | `TOC.Slug.GLFM` (partial) |
| `testSymsWhenRepeatedHeadingsNoGlfm` | Slug de-duplication with GLFM mode off | `TOC.Slug.GLFM` (partial) |
| `testSymsWhenTitleFromHeadingIsOn` | Symbol list with `titleFromHeading = true` (H1 → `T`) | `Config.Precedence.Layering` |

---

## CodeActionTests

**Source:** `Tests/CodeActionTests.fs` · **Level:** Unit · **Feature:** Code actions

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `CreateMissingFileTests.shouldCreateWhenNoFileExists` | `createMissingFile` returns Some when linked file is absent | — |
| `CreateMissingFileTests.shouldNotCreateWhenRefBrokenButFileExists` | `createMissingFile` returns None when file exists but anchor is broken | — |

---

## ComplTests

**Source:** `Tests/ComplTests.fs` · **Level:** Unit / Snapshot · **Feature:** Completions

### PartialElementWiki

| Test | What it verifies |
|------|-----------------|
| `empty` | No partial element at position 0 |
| `emptyEof` | `[[` at EOF yields partial wiki-link |
| `emptyEol` | `[[` before newline |
| `emptyNonEol` | `[[ ` (space-terminated) |
| `someEof` | `[[t` at EOF |
| `someEol` | `[[to` before newline |
| `someWs` | `[[t ` (space-terminated) |
| `someAndTextAfter` | `[[t other` |
| `emptyHeading` | `[[#` heading-only partial |
| `nonEmptyHeading` | `[[#hea] ` truncated partial |

### PartialElementReference

| Test | What it verifies |
|------|-----------------|
| `empty` | Empty text |
| `emptyEof` | `[` at EOF |
| `emptyEol` | `[` before newline |
| `emptyNonEol` | `[ ` with space |
| `someEof` | `[t` at EOF |
| `someEol` | `[t` before newline |
| `someWs` | `[t ` space-terminated |
| `someAndTextAfter` | `[t other` |
| `emptyBrackets` | `[]` |
| `partialReference` | `[l][` partial full reference |

### PartialElementInline

| Test | What it verifies |
|------|-----------------|
| `empty` | Empty text |
| `emptyEof` | `(` at EOF |
| `emptyEol` | `(` before newline |
| `emptyLinkEof` | `](` |
| `emptyLinkEol` | `](` before newline |
| `emptyNonEol` | `]( ` space-terminated |
| `emptyNonEolFurther` | Position past space → no partial |
| `someEol` | `](t` |
| `someWs` | `](t ` space-terminated |
| `someAndTextAfter` | `](t other` |
| `someAndTextBeforeAfter` | `before](t other` offset test |
| `emptyBrackets` | `]()` |
| `bracketsAndOpenParen` | `](` |
| `bracketsWithTextAndOpenParen` | `[b](` |
| `bracketsWithSpacedTextAndOpenParen` | `[a b](c` |
| `anchor1`–`anchor6` | Cursor positions in `](t# …)` / `(#a …)` patterns |

### PartialElementTag

| Test | What it verifies |
|------|-----------------|
| `opening1` | `#` alone |
| `opening2` | `# ` (space after hash) |
| `opening3` | `## ` (two hashes — not a tag) |
| `opening4` | `hello# ` (hash mid-word) |

### Candidates

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `noDupsOnAchor_intraFile` | No duplicate headings in intra-file anchor completion | `Completion.Trigger.Coverage` |
| `noExtraHash_wikiHeading_intraFile_issue174` | No extra `#` in `[[#]]` completion | — |
| `noDupsOnAchor_crossFile` | No duplicates in cross-file anchor completion | `Completion.Trigger.Coverage` |
| `fileWithSpaces_anchor` | Anchor completion for inline link → file with spaces | `Completion.Trigger.Coverage` |
| `docAndHeadingFuzzy` | Fuzzy-match `[[do#]]` resolves to multiple docs | — |
| `referenceEmptyBrackets` | Completing `[]` against link definitions | `Completion.Trigger.Coverage` |
| `referenceNonEmptyBrackets` | Completing `[l]` against link defs | `Completion.Trigger.Coverage` |
| `inlineEmpty` | Completing `[]()`  | `Completion.Trigger.Coverage` |
| `partialWikiDoc` | `[[` with multiple docs | `Completion.Trigger.Coverage` |
| `partialWikiHeading` | `[[#` with headings in same doc | `Completion.Trigger.Coverage` |
| `partialWikiDocHeading` | `[[d#` cross-file | `Completion.Trigger.Coverage` |
| `partialWikiDocHeading_FilePathStem` | Same with `FilePathStem` style | `Link.Wiki.StyleBinding` |
| `partialWikiDocHeading_FileStem` | Same with `FileStem` style | `Link.Wiki.StyleBinding` |
| `partialWikiDoc_FileStem_ArbitraryPath` | `FileStem` style for date-named file | `Link.Wiki.StyleBinding` |
| `partialReferenceEmpty` | Completing `[` against link defs | `Completion.Trigger.Coverage` |
| `partialInlineHeading` | Completing `[link](#` | `Completion.Trigger.Coverage` |
| `partialInlineDoc` | Completing `[](` | `Completion.Trigger.Coverage` |
| `WikiWithSpaces_TitleSlug.test1`–`test3` | `[[a]]` with space-containing titles | `Link.Wiki.StyleBinding` |
| `WikiWithSpaces_FileStem.test1`–`test3` | `[[do]]` against `doc one.md` / `doc two.md` | `Link.Wiki.StyleBinding` |
| `wiki_HeadingWithSpecialChars_NotEncoded` | `Foo / Bar` heading not encoded in completion | — |
| `wiki_FileWithSpecialChars_Subtitle_NoCompletionProvided` | `#`-in-filename gives no subtitle completion | — |
| `wiki_CrossHeading_TitleNameVsSlug` | Title text vs slug in cross-heading completion | `Link.Wiki.StyleBinding` |

### Tags

| Test | What it verifies |
|------|-----------------|
| `tagOpening` | Tag completion on `# ` (opening hash with space) |
| `tagWithName` | Tag completion on partial `#ta ` |

### CandidatesCap

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `rawFunction_notCapped_whenPoolExceedsCap` | `findCandidatesInDoc` returns more than cap when pool exceeds cap (cap applied at server layer) | `Completion.Candidates.Cap` |
| `rawFunction_allReturned_whenPoolBelowCap` | `findCandidatesInDoc` returns fewer than cap when pool is small | `Completion.Candidates.Cap` |

---

## ConfigTests

**Source:** `Tests/ConfigTests.fs` · **Level:** Unit · **Feature:** Configuration

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `testParse_0` | Empty TOML → `Config.Empty` | `Config.Fault.Isolation` |
| `testParse_1` | `[code_action]` only → `Config.Empty` | — |
| `testParse_2` | `toc.enable = false` | — |
| `testParse_tocInclude` | `toc.include = [2, 3, 4]` | `TOC.Levels.Filter` |
| `testParse_3` | `completion.wiki.style = "file-stem"` → `FileStem` | `Link.Wiki.StyleBinding` |
| `testParse_4` | `core.text_sync = "incremental"` → `Incremental` | `Config.TextSync.Default` |
| `testParse_5` | `core.incremental_references = true` | — |
| `testParse_6` | `core.paranoid = true` | — |
| `testParse_7` | `completion.candidates = 100` | `Completion.Candidates.Cap` (parse only) |
| `testParse_8` | `core.markdown.glfm_heading_ids.enable = true` | `TOC.Slug.GLFM` |
| `testParse_broken_0` | Bare `blah` → `None` | `Config.Fault.Isolation` |
| `testParse_broken_1` | `markdown.file_extensions` with integers → `None` | `Config.Fault.Isolation` |
| `testParse_broken_2`–`broken_4` | Mixed/wrong-type arrays → `None` | `Config.Fault.Isolation` |
| `testParse_broken_5` | `candidates = -1` (negative) → `None` | `Config.Validation.Candidates` |
| `testParse_broken_6` | `glfm_heading_ids.enable = -1` (integer) → `None` | `Config.Fault.Isolation` |
| `testParse_broken_tocInclude` | `toc.include = [1, -1]` negative level → `None` | `Config.Fault.Isolation` |
| `testParse_broken_zeroCandidates` | `candidates = 0` (zero, not strictly positive) → `None` | `Config.Validation.Candidates` |
| `testDefault` | Embedded `default.marksman.toml` parses to `Config.Default` | `Config.Precedence.Layering` |
| `testDefault_titleVsCompletionStyle` | `title_from_heading = false` → `FileStem` style | `Link.Wiki.StyleBinding` |

---

## ConnTest

**Source:** `Tests/ConnTest.fs` · **Level:** Integration / Snapshot · **Feature:** Connection graph

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `ConnGraphTests.emptyGraph` | Empty oracle/MMap → empty connection | — |
| `ConnGraphTests.initGraph` | Initial graph from 4 docs with wiki-links and link-defs | `Navigation.References.Completeness` |
| `ConnGraphTests.removeDoc` | Removing a doc updates the graph | `Navigation.References.Completeness` |
| `ConnGraphTests.addDoc` | Adding a doc updates outgoing link resolution | `Navigation.References.Completeness` |
| `ConnGraphTests.addLinkDef` | Adding a link definition resolves previously broken refs | `Navigation.References.Completeness` |
| `ConnGraphTests.removeHeading` | Removing a heading updates dependent connections | `Navigation.References.Completeness` |
| `ConnGraphTests.removeTitle_PARANOID` | Removing H1 in paranoid mode; incremental = from-scratch | — |
| `ConnGraphTests.addTitle_SameAsFileName` | Title matches filename; incremental converges | `Link.Wiki.StyleBinding` |
| `ConnGraphTests.addTitle_CrossSection` | Renaming title referenced from another doc's section link | `Rename.Refactoring.Completeness` |
| `ConnGraphTests.fixRef` | Fixing a broken intra-doc heading reference | `Navigation.References.Completeness` |
| `ConnGraphTests.breakCrossRef` | Renaming a title breaks a cross-doc ref | `Rename.Refactoring.Completeness` |
| `ConnGraphTests.addingEmptyHeader` | Adding empty `# ` doesn't break the graph | — |
| `RenameTests.renameCrossRef_D1_then_D2` | Rename title in d1, fix ref in d2 (order 1) | `Rename.Refactoring.Completeness` |
| `RenameTests.renameCrossRef_D2_then_D1` | Fix ref in d2 first, rename title in d1 (order 2) | `Rename.Refactoring.Completeness` |
| `ConnGraphTests.addDocThenTitle` | Add doc without title, then add title; converges | `Navigation.References.Completeness` |
| `ConnGraphTests.addSecondTitle` | Empty doc → first title → second title; converges | — |
| `ConnGraphTests.initGraphWithTags` | Tag connections across docs | — |
| `ConnGraphTests.removingTag` | Removing a tag updates the connection graph | — |
| `ConnGraphTests_TitleLess.updateH1` | H1 text update in title-less mode | `Link.Wiki.StyleBinding` |

---

## DiagTest

**Source:** `Tests/DiagTest.fs` · **Level:** Unit · **Feature:** Diagnostics

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `documentIndex_1` | `Doc.index` extracts two titles correctly | — |
| `nonBreakingWhitespace` | Detects U+00A0 after `##`; verifies range | `Diagnostic.Code.Assignment` (code `"3"`) |
| `noDiagOnShortcutLinks` | Shortcut links produce no diagnostic; broken `[[#h42]]` fires | `Diagnostic.Severity.WikiLink` |
| `noDiagOnRealUrls` | `https://` URLs produce no broken-link diagnostic | `Link.Inline.URLSkip` |
| `noDiagOnNonMarkdownFiles` | Links to folders produce no diagnostic; `.md` links do | — |
| `crossFileDiagOnBrokenWikiLinks` | `[[bad]]` with no matching file fires a diagnostic | `Diagnostic.Severity.WikiLink` |
| `noCrossFileDiagOnSingleFileFolders` | Single-file folder suppresses cross-file diagnostics | `Link.Resolution.ModeScope` |
| `brokenWikiLink_hasSeverityError` | Broken wiki-link diagnostic `severity = Error` asserted | `Diagnostic.Severity.WikiLink` |
| `brokenMarkdownLink_hasSeverityWarning` | Broken inline MD link `severity = Warning` asserted | `Diagnostic.Severity.MarkdownLink` |
| `brokenWikiLink_hasCodeTwo` | Broken wiki-link diagnostic `code = "2"` asserted | `Diagnostic.Code.Assignment` |
| `ambiguousWikiLink_hasCodeOneAndRelatedInfo` | Two docs with same slug → code `"1"`, `relatedInformation` has 2 entries | `Diagnostic.Ambiguous.RelatedInfo`, `Diagnostic.Code.Assignment` |

---

## GitIgnoreTest

**Source:** `Tests/GitIgnoreTest.fs` · **Level:** Unit · **Feature:** File exclusion

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `patternToGlob_Empty` | Empty pattern → empty globs | `Link.Resolution.IgnoreGlob` |
| `absGlob_Unix` | Absolute pattern on Unix root | `Link.Resolution.IgnoreGlob` |
| `relGlob_Unix_1`–`relGlob_Unix_3` | Relative patterns on Unix | `Link.Resolution.IgnoreGlob` |
| `absGlob_Win` | Absolute pattern on Windows | `Link.Resolution.IgnoreGlob` |
| `relGlob_Win_1`–`relGlob_Win_3` | Relative patterns on Windows | `Link.Resolution.IgnoreGlob` |
| `issue_218` | Character-class glob `*.foo[o,p]` (not supported) | `Link.Resolution.IgnoreGlob` |

---

## LensesTests

**Source:** `Tests/LensesTests.fs` · **Level:** Unit · **Feature:** Code lenses

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `basicHeaderLenses` | Heading lenses show "1 reference" / "2 references"; unreferenced heading omitted | `Navigation.CodeLens.Count` |
| `basicHeaderLenses_withCommandArguments` | Client with `codeLensFindReferences` cap → lens carries location data | `Navigation.CodeLens.Count` |
| `basicLinkDefLenses` | Link-def lens shows "2 references"; usages without defs not lensed | `Navigation.CodeLens.Count` |

---

## MMapTests

**Source:** `Tests/MMapTests.fs` · **Level:** Unit · **Feature:** MMap data structure

| Test | What it verifies |
|------|-----------------|
| `DifferenceTests.keyIntersection` | `MMap.difference` identifies added/removed/changed keys |

---

## MiscTests

**Source:** `Tests/MiscTests.fs` · **Level:** Unit · **Feature:** Utilities

| Test | What it verifies |
|------|-----------------|
| `isSubSequenceOf_1`–`isSubSequenceOf_5` | Subsequence matching (case-sensitivity, empty) |
| `slug_1`–`slug_5` | Slug generation for English, Cyrillic, hyphens, empty |
| `lines_1`–`lines_3` | Line-splitting with `\n` and `\r\n` |
| `abspath_urlencode_1`–`abspath_urlencode_5` | Absolute-path URI round-trip (spaces, `#`, folders) |
| `trimSuffix_1`–`trimSuffix_2` | Suffix trimming |
| `encodeForWiki_1`–`encodeForWiki_2` | Wiki encoding of special chars (`#`, `[`, `]`, `|`) |
| `LinkLabelTest.caseSensitivity` | Link labels case-insensitive |
| `LinkLabelTest.consecutiveWhitespace` | Multiple spaces collapsed |
| `LinkLabelTest.surroundingWhitespace` | Leading/trailing spaces stripped |
| `WatchGlobTest.test1` | `mkWatchGlob` produces correct glob pattern |

---

## ParserTests

**Source:** `Tests/ParserTests.fs` · **Level:** Unit / Snapshot · **Feature:** Parsing

### HeadingTests

| Test | What it verifies |
|------|-----------------|
| `parse_empty` | Empty document |
| `parse_title_single` | Single H1 |
| `parse_title_multiple` | Three H1s with mixed line endings |
| `parse_title_with_child_paragraph` | Heading + paragraph text |
| `parse_nested_headings` | H1 with two H2s |

### WikiLinkTests

| Test | What it verifies |
|------|-----------------|
| `parser_xref_note` | `[[note]]` basic wiki-link |
| `parser_xref_note_heading` | `[[note#heading]]` |
| `parser_xref_text_before`/`after`/`around` | Text surrounding wiki-link |
| `parser_xref_2nd_line` | Wiki-link on second line |
| `parse_wiki_empty_heading` | `[[T#]]` |
| `parse_wiki_escaped_hash`/`_and_heading`/`_with_hash` | Escaped `\#` variants |
| `parse_wiki_with_title` | `[[T#head\|title]]` |
| `parse_wiki_empty_title` | `[[T#head\|]]` |
| `parse_wiki_no_doc_and_title` | `[[#head\|title]]` |
| `parse_wiki_no_doc_and_no_title` | `[[\|]]` |
| `parse_wiki_all_empty` | `[[]]` |
| `complex_example_1` | Multi-heading doc with partial/broken wiki-links |

### MdLinkTest

| Test | What it verifies |
|------|-----------------|
| `parser_link_1`–`parser_link_14` | Full spectrum of inline, collapsed, full reference, and shortcut links |

### Other

| Test | What it verifies |
|------|-----------------|
| `FootnoteTests.footnote_1` | **SKIPPED** — footnote syntax |
| `TagsTests.tags_1`–`tags_nested` | Tag parsing including nested paths |
| `DocUrlTests.test1`–`test3` | Doc URL parsing with anchors |
| `RegressionTests.no156`, `no235`, `no334`, `no453` | Known edge cases and crash fixes |
| `MathBlockTests.math_block_*`, `inline_math_*`, `math_and_regular_*` | Math blocks suppress wiki-link parsing |

---

## PathsTests

**Source:** `Tests/PathsTests.fs` · **Level:** Unit · **Feature:** Path / URI

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `LocalPathTests.testWinPath` | Windows path components | — |
| `LocalPathTests.testUnixPath` | Unix path, trailing slash, `..` | — |
| `LocalPathTests.testNormalize` | `..` traversal normalisation | — |
| `PathUriTests.testWinPathFromUri` | Percent-encoded drive letter in URI | — |
| `PathUriTests.testWinPathFromPath` | Windows path with spaces | — |
| `PathUriTests.testWinDocUriFromUri` | `UriWith.mkAbs` preserves URI | — |
| `PathUriTests.testWinDocUriFromPath` | Path → URI with drive letter | — |
| `PathUriTests.testRootedRel_SameRootRel` | `UriWith.mkRooted` round-trip | — |
| `PathUriTests.testAccented_issue274` | Accented char `/activité.md` percent-encoded correctly | — |

---

## RefactorTests

**Source:** `Tests/RefactorTests.fs` · **Level:** Unit · **Feature:** Rename

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `ReferenceLinks.onRefLabel` | Rename link label at usage site; all occurrences updated | `Rename.Refactoring.Completeness` |
| `ReferenceLinks.onDefLabel` | Rename link label at definition site; all occurrences updated | `Rename.Refactoring.Completeness` |
| `HeadingLinks.onTitle` | Rename H1 title; updates wiki-links in other docs | `Rename.Refactoring.Completeness`, `Rename.StyleBinding.Consistency` |
| `HeadingLinks.onSubtitle` | Rename H2 heading; updates wiki-links and inline links | `Rename.Refactoring.Completeness`, `Rename.StyleBinding.Consistency` |
| `PrepareRenameTests.prepareRename_onHeading_returnsRange` | Cursor on heading → `renameRange` returns `Some range` | `Rename.Prepare.Rejection` |
| `PrepareRenameTests.prepareRename_onBodyText_returnsNone` | Cursor on body text → `renameRange` returns `None` | `Rename.Prepare.Rejection` |

---

## RefsTests

**Source:** `Tests/RefsTests.fs` · **Level:** Unit · **Feature:** Reference resolution

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `InternNameTests.relPath_1`/`relPath_2` | Relative path resolution from subdirectory | — |
| `InternNameTests.relPath_non_exist` | Escaping folder root → `None` | — |
| `InternNameTests.rootPath` | Absolute URI path → root-relative | — |
| `InternNameTests.url_no_schema_FP` | `www.google.com` treated as intern path (known FP) | `Link.Inline.URLSkip` (boundary) |
| `InternNameTests.url_schema` | `http://…` → `None` | `Link.Inline.URLSkip` |
| `FileLinkTests.fileName_Partial` | Partial name no match; exact stem match; URL-encoded match | `Navigation.Definition.LinkTypes` |
| `FileLinkTests.fileName_RelativeAsAbs` | `FilePathStem` style subdirectory matching | `Navigation.Definition.LinkTypes` |
| `FileLinkTests.heading_Partial` | `TitleSlug` matching by title slug | `Navigation.Definition.LinkTypes` |
| `FileLinkTests.titleSimilarToName` | `FileLinkKind.detect` distinguishes title vs file-name | `Link.Wiki.StyleBinding` |
| `BasicRefsTests.refToTag_atTag` | Refs from tag definition (excl. declaration) | `Navigation.References.Completeness` |
| `BasicRefsTests.refToTag_atTag_withDecl` | Same including declaration | `Navigation.References.Completeness` |
| `BasicRefsTests.refToLinkDef_atDef` | Refs from link definition | `Navigation.References.Completeness` |
| `BasicRefsTests.refToLinkDef_atDef_withDecl` | Same including definition | `Navigation.References.Completeness` |
| `BasicRefsTests.refToLinkDef_atLink` | Refs from link usage (case-insensitive) | `Navigation.References.Completeness` |
| `BasicRefsTests.refToLinkDef_atLink_withDecl` | Same including definition | `Navigation.References.Completeness` |
| `BasicRefsTests.refToFootnote_atLink` | **SKIPPED** — footnote refs | — |
| `BasicRefsTests.refToDoc_atTitle` | Refs from H1 title to all cross-doc links | `Navigation.References.Completeness` |
| `BasicRefsTests.refToDoc_atTitle_withDecl` | Same including self | `Navigation.References.Completeness` |
| `BasicRefsTests.refToDoc_atLink` | Refs from wiki-link to matching links | `Navigation.References.Completeness` |
| `BasicRefsTests.refToDoc_atLink_withDecl` | Same including heading declaration | `Navigation.References.Completeness` |
| `LinkKindRefsTests.atWiki_VariousFilenames` | Wiki-link resolves across all link variants | `Navigation.Definition.LinkTypes` |
| `LinkKindRefsTests.atWiki_Filenames_Subfolder` | Wiki-link from subfolder doc | `Navigation.Definition.LinkTypes` |
| `EncodingTests.headingNotEncoding` | `[[#Heading 1]]` (non-encoded space) resolves | `Navigation.Definition.LinkTypes` |
| `EncodingTests.headingUrlEncoded` | `[[#Heading%201]]` (URL-encoded space) resolves | `Navigation.Definition.LinkTypes` |
| `EncodingTests.docNotEncoded` | `[[Doc 2]]` resolves to doc | `Navigation.Definition.LinkTypes` |
| `EncodingTests.docUrlEncoded` | `[[Doc%202]]` resolves | `Navigation.Definition.LinkTypes` |
| `EncodingTests.docHeadingMixedEncoding` | `[[Doc 2#Heading %231]]` mixed encoding | `Navigation.Definition.LinkTypes` |
| `EncodingTests.inlineDocUrlEncoding` | `[](Doc%202)` inline link | `Navigation.Definition.LinkTypes` |
| `EncodingTests.inlineDocHeadingMixedEncoding` | `[](Doc%202#heading-1)` mixed | `Navigation.Definition.LinkTypes` |
| `EncodingTests.docFileNameWithDots` | `doc.3.with.dots.md` with and without `.md` | `Navigation.Definition.LinkTypes` |
| `TitleLess.refToH1` | H1 not cross-referenceable in title-less mode | `Link.Resolution.ModeScope` (partial) |
| `RegressionTests.rootLink_issue275` | `[](/)`  → no resolution | — |

---

## SematoTests

**Source:** `Tests/SematoTests.fs` · **Level:** Unit · **Feature:** Semantic tokens

| Test | What it verifies |
|------|-----------------|
| `testEncoding` | `Token.ofIndexEncoded` produces 5 correctly delta-encoded tokens for a 5-element document |

---

## ServerTests

**Source:** `Tests/ServerTests.fs` · **Level:** Unit · **Feature:** Server / text-sync

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `textSync_UserConfigEmptyWS` | User config `Incremental` wins over defaults | `Config.Precedence.Layering` |
| `textSync_NoConfigEmptyWS` | No config → `Full` | `Config.TextSync.Default` |
| `textSync_NoConfigEmptyWS_PreferIncr` | Client opt `preferredTextSyncKind = Incremental` | `Config.TextSync.Default` |
| `textSync_NoConfigNonEmptyWS_PreferIncr` | Client opt in non-empty workspace | `Config.Precedence.Layering` |
| `textSync_NonEmptyWS_PreferIncrButConfigTakesPrecedence` | Workspace folder config `Full` overrides client opt | `Config.Precedence.Layering` |

---

## StateTests

**Source:** `Tests/StateTests.fs` · **Level:** Unit · **Feature:** Server state

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `InitOptionTests.extractEmpty` | Empty JSON → `InitOptions.empty` | — |
| `InitOptionTests.extractCorrect` | `preferredTextSyncKind = 1`/`2` → `Full`/`Incremental` | `Config.TextSync.Default` |
| `InitOptionTests.extractMalformed` | Out-of-range / string value → `None` field | `Config.Fault.Isolation` |
| `StateTests.folderFind_singleFile` | `State.tryFindFolderAndDoc` finds single-file folder | `Link.Resolution.ModeScope` |

---

## SuffixTreeTests

**Source:** `Tests/SuffixTreeTests.fs` · **Level:** Unit · **Feature:** SuffixTree

| Test | What it verifies |
|------|-----------------|
| `ImplTests.filterTest` | `filterMatchingValues` with single- and multi-segment keys |
| `ImplTests.removeTest` | `remove` existing / non-existing / root / partial keys |

---

## SymbolsTests

**Source:** `Tests/SymbolsTests.fs` · **Level:** Unit · **Feature:** Symbols

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `WorkspaceSymbol.symbols_noQuery` | `workspaceSymbols ""` returns all headings and tags | — |
| `WorkspaceSymbol.symbols_withQuery` | `workspaceSymbols "Tag:"` filters results | — |
| `DocSymbols.order_noHierarchy` | Flat symbol list (headings then tags) | — |
| `DocSymbols.order_Hierarchy` | Hierarchical `DocumentSymbol[]` with children | — |

---

## TextTests

**Source:** `Tests/TextTests.fs` · **Level:** Unit · **Feature:** Text model

| Test | What it verifies |
|------|-----------------|
| `lineMap_empty` | Empty string → single sentinel entry |
| `lineMap_finalNewLine` | `\n` alone → two entries |
| `lineMap_singleChar_ascii` | Single character |
| `lineMap_singleLine_ascii` | Nine-character line |
| `lineMap_multiple_lines` | Mixed `\n` and `\r\n` |
| `applyTextChange_insert_single` | Insert at end of single-char document |
| `applyTextChange_insert_multiple` | Two sequential inserts applied in order |
| `applyTextChange_insert_on_empty` | Insert into empty document |
| `applyTextChange_insert_next_line` | Insert at start of second line |
| `applyTextChange_replace_single` | Replace a range |
| `applyTextChange_delete_single` | Delete a range (empty replacement) | 

---

## TocTests

**Source:** `Tests/TocTests.fs` · **Level:** Unit · **Feature:** Table of Contents

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `DetectToc.detectToc_1` | No markers → `None` | `TOC.Generation.Markers` |
| `DetectToc.detectToc_noMarker` | TOC-like list without markers → `None` | `TOC.Generation.Markers` |
| `DetectToc.detectToc_withMarker` | Markers present → correct range detected | `TOC.Generation.Markers` |
| `CreateToc.createToc` | Two headings → TOC with two entries | `TOC.Levels.Filter` |
| `CreateToc.createToc_yamlFrontMatter` | YAML front matter not included as heading | — |
| `InsertToc.insert_documentBeginning` | No H1 → `DocumentBeginning` insertion | — |
| `InsertToc.insert_firstTitle` | H1 → `After` H1 insertion | — |
| `InsertToc.insert_afterYaml` | YAML, no H1 → `After` YAML insertion | — |
| `InsertToc.insert_afterFirstTitle_withYaml` | YAML + H1 → `After` H1 | — |
| `RenderToc.createToc` | Nested levels, correct indentation and anchor links | `TOC.Slug.GLFM` (partial) |
| `RenderToc.createToc_filteredLevels` | `[2; 3]` filter excludes H1 and H4 | `TOC.Levels.Filter` |
| `DocumentEdit.insert_afterYaml` | YAML doc, no H1, TOC inserted after YAML (full round-trip) | `TOC.Generation.Markers` |
| `DocumentEdit.insert_afterTopHeading` | H1 present, TOC inserted after it | `TOC.Generation.Markers` |
| `DocumentEdit.insert_documentBeginning` | No H1, TOC at file start | `TOC.Generation.Markers` |
| `DocumentEdit.update_atBeginningOfFile` | Existing TOC replaced when headings change | `TOC.Generation.Markers` |
| `DocumentEdit.upToDate_noUpdate` | Up-to-date TOC → `None` (no action) | — |
| `DocumentEdit.upToDate_whitespace_noUpdate` | TOC with extra blank line → up-to-date, no action | — |
| `TocEmptyTests.tocAction_noHeadings_returnsNone` | Heading-free doc → `tableOfContentsInner` returns `None` | `TOC.Empty.NoAction` |
| `GlfmTocTests.glfm_disambiguates_duplicate_headings_in_toc` | Two identical headings → TOC anchor slugs use `-1` suffix | `TOC.Slug.GLFM` |

---

## WorkspaceTest

**Source:** `Tests/WorkspaceTest.fs` · **Level:** Unit / Integration · **Feature:** Workspace

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `FolderTest.rooPath_singleFile` | `Folder.rootPath` for single-file folder | `Workspace.ProjectDetection.Root` |
| `FolderTest.updateDoc` | Update, remove, re-add doc; slug/path indices correct | — |
| `DocTest.applyLspChange` | `Doc.applyLspChange` applies incremental edit | — |
| `DocTest.pathFromRoot_SpecialChars` | **SKIPPED** — `#` in filename | — |
| `DocTest.fromLsp_singleFile` | `Doc.fromLsp` parses URI/path from LSP open notification | — |
| `WorkspaceTest.folderFind_singleFile` | `Workspace.tryFindFolderEnclosing` finds/misses correctly | `Workspace.MultiFolder.Isolation` |
| `WorkspaceTest.folderAdded_evictSingleFile` | Adding multi-file folder evicts enclosed single-file folder | `Link.Resolution.ModeScope` |
| `WorkspaceTest.folderConfig_noUserConfig` | Folder config preserved when no user config | `Config.Precedence.Layering` |
| `WorkspaceTest.folderConfig_userConfig` | User config overrides folder config | `Config.Precedence.Layering` |
| `WorkspaceTest.folderConfig_userConfig_folderAdd` | User config applied to folder added after workspace creation | `Config.Precedence.Layering` |

---

## GapTests

**Source:** `Tests/GapTests.fs` · **Level:** Unit / Integration · **Feature:** Completions, Diagnostics, Workspace, LSP integration

### CompletionCapTests

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `applyCompletionCap_isIncomplete_whenCapped` | 60-item pool with cap 50 → 50 items returned, `IsIncomplete = true` | `Completion.Incomplete.Flag` |
| `applyCompletionCap_notIncomplete_whenBelowCap` | 5-item pool with cap 50 → 5 items, `IsIncomplete = false` | `Completion.Incomplete.Flag` |
| `applyCompletionCap_exactCap_isIncomplete` | Exactly 50 items with cap 50 → `IsIncomplete = true` (boundary) | `Completion.Incomplete.Flag` |
| `applyCompletionCap_emptyInput_notIncomplete` | Empty pool → 0 items, `IsIncomplete = false` | `Completion.Incomplete.Flag` |

### DebounceTests

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `debounce_publishesDiagnostics_afterQuietPeriod` | Open file with broken wiki-link; debounce fires and publishes diagnostic | `Diagnostic.Debounce.Latency` |
| `debounce_suppressesDuplicateUpdates_duringEdit` | Rapid identical open calls coalesce; final state published | `Diagnostic.Debounce.Latency` |

### FileExtensionTests

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `customExtension_txtDoc_resolvedBySlug` | Config `coreMarkdownFileExtensions = ["txt"]` → `.txt` slug resolves `[[target]]` | `Workspace.FileExtension.Filter` |
| `defaultExtensions_txtDoc_notResolved` | Default `["md"; "markdown"]` config → `.txt` doc slug includes extension, `[[target]]` is broken | `Workspace.FileExtension.Filter` |
| `defaultExtensions_mdDoc_resolvedBySlug` | Control: `.md` doc with default config resolves correctly | `Workspace.FileExtension.Filter` |

### IntegrationTests

Uses `TestServer` harness; creates real files on disk, drives `Initialize` / `didOpen` through `MarksmanServer`, and asserts on published diagnostics.

| Test | What it verifies | Requirement tag |
|------|-----------------|----------------|
| `integration_brokenLink_publishesDiagnostic` | Workspace file with `[[does-not-exist]]` → Error diagnostic published with matching message | `Diagnostic.Severity.WikiLink` |
| `integration_validLink_noDiagnostic` | Two files where one wiki-links to the other → no diagnostics for linking file | — |
| `integration_didOpen_triggersAdditionalDiagnostics` | `textDocument/didOpen` with broken link → diagnostic pipeline fires and publishes error | `Diagnostic.Debounce.Latency` |

---

## Helpers

**Source:** `Tests/Helpers.fs` · **Level:** N/A · **Feature:** Test infrastructure

No runnable tests. Provides `FakeDoc.Mk`, `FakeFolder.Mk`, and related factory helpers used throughout the test suite.

---

## ServerHarness

**Source:** `Tests/ServerHarness.fs` · **Level:** N/A · **Feature:** Test infrastructure (in-process LSP server)

No runnable tests. Provides `TestServer` — an in-process `MarksmanServer` wrapper that writes real files to a temp directory, calls `Initialize` and `Initialized`, and exposes `DidOpen` / `WaitDiagnostics` helpers for integration tests in `GapTests.IntegrationTests` and `GapTests.DebounceTests`.

---

## TestClient

**Source:** `Tests/TestClient.fs` · **Level:** N/A · **Feature:** Test infrastructure (LSP notification capture)

No runnable tests. Provides `CaptureClient` — a `MarksmanClient` wrapper whose `notiSender` captures every `textDocument/publishDiagnostics` notification in a `ConcurrentQueue` for test assertions.

---

## Benchmarks

**Source:** `Benchmarks/Program.fs` · **Level:** Performance · **Feature:** goto-def, find-refs

| Benchmark | Parameters | What it measures | Requirement tag |
|-----------|-----------|-----------------|----------------|
| `gotoDefTime` | `FolderSize ∈ {10, 50, 250}` | `Dest.tryResolveElement` wall time | `Navigation.Definition.LinkTypes` (perf aspect) |
| `findRefsTime` | `FolderSize ∈ {10, 50, 250}` | `Dest.findElementRefs true` wall time | `Navigation.References.Completeness` (perf aspect) |

---

## Related

- [[tests/index|Test Index]] — summary and coverage status per requirement
- [[tests/requirements-coverage|Requirements Coverage]] — requirement-by-requirement cross-reference
- [[tests/gaps|Coverage Gaps]] — prioritised gap analysis
