---
title: "Behavior Layer"
date: 2026-04-16
tags:
  - wiki/design
  - design/bdd
aliases:
  - BDD Scenarios
  - Executable Specifications
---

# Behavior Layer

Executable specifications for Marksman's observable behaviors, written as Given/When/Then scenarios at the LSP component level. Each scenario encodes a rule that matters to at least two of these roles: editor-plugin author, server contributor, QA engineer.

> [!NOTE] Automation level
> All scenarios in this document target the **component level**: one in-process Marksman server, no real editor, no network. The right automation harness is **Reqnroll** (the .NET successor to SpecFlow) bound to LSP request helpers. Step definitions map `Given` clauses to workspace fixture builders and `Then` clauses to LSP response assertions. See [[#Automation Notes]] for wiring details.

> [!TIP] Scope boundary
> Scenarios here cover behaviors visible across the LSP boundary. Low-level rules (slug normalization algorithm, InternPath classification, TOML parsing edge cases, incremental `Conn` graph invariants) belong in unit tests — see [[#What Stays in Unit Tests]].

---

## Ubiquitous Language (BDD Vocabulary)

| Term | Meaning in scenarios |
|------|---------------------|
| **workspace** | A set of Markdown folders served by one Marksman process |
| **document** | A single `.md` file, either open in an editor or loaded from disk |
| **follows the link** | User invokes `textDocument/definition` with cursor on a link |
| **link target** | The document and position that go-to-definition resolves to |
| **diagnostic** | An LSP `Diagnostic` entry returned via `textDocument/publishDiagnostics` |
| **completion list** | The candidates returned by `textDocument/completion` |
| **references** | Locations returned by `textDocument/references` |
| **single-file mode** | Server opened one file directly, without a workspace folder root |
| **slug** | Case-folded, trimmed string used to match link text to document names |

---

## Feature: Wiki-Link Resolution

> The core domain behavior. A wiki-link `[[target]]` or `[[target#section]]` must resolve to the right document and heading using slug-based approximate matching.

```gherkin
Feature: Wiki-link resolution

  Background:
    Given a Marksman server is running
    And the workspace contains a folder rooted at "notes/"

  # ── Happy path ─────────────────────────────────────────────────────────────

  Scenario: Exact slug match resolves to the document
    Given the workspace contains "notes/Animals.md"
    And the current document contains the link [[Animals]]
    When the user follows the link [[Animals]]
    Then the link target is "notes/Animals.md"

  Scenario: Slug matching is case-insensitive
    Given the workspace contains "notes/My Notes.md"
    And the current document contains the link [[my notes]]
    When the user follows the link [[my notes]]
    Then the link target is "notes/My Notes.md"

  Scenario: Section link resolves to the matching heading
    Given "notes/Animals.md" contains the heading "## Mammals" at line 8
    And the current document contains the link [[Animals#Mammals]]
    When the user follows the link [[Animals#Mammals]]
    Then the link target is line 8 of "notes/Animals.md"

  Scenario: Approximate path match finds a document in a subdirectory
    Given the workspace contains "notes/science/Biology.md"
    And no document named "Biology.md" exists directly under "notes/"
    And the current document contains the link [[Biology]]
    When the user follows the link [[Biology]]
    Then the link target is "notes/science/Biology.md"

  Scenario: Title heading is preferred over filename when style is title-slug
    Given "notes/2024-01-15-field-notes.md" has the title "Field Notes"
    And completion.wiki.style is "title-slug"
    And the current document contains the link [[Field Notes]]
    When the user follows the link [[Field Notes]]
    Then the link target is "notes/2024-01-15-field-notes.md"

  Scenario: Intra-document section link resolves within the same file
    Given the current document contains "## Overview" at line 4
    And the current document contains the link [[#Overview]]
    When the user follows the link [[#Overview]]
    Then the link target is line 4 of the current document

  # ── Broken and ambiguous ───────────────────────────────────────────────────

  Scenario: Broken link — no document matches the slug
    Given no document named "Missing" exists in the workspace
    And the current document contains the link [[Missing]]
    Then a BrokenLink error diagnostic is reported on [[Missing]]

  Scenario: Ambiguous link — multiple documents share a slug
    Given "notes/Animals.md" and "archive/Animals.md" both exist
    And the current document contains the link [[Animals]]
    Then an AmbiguousLink warning diagnostic is reported on [[Animals]]

  Scenario: Broken section link — document exists but heading does not
    Given "notes/Animals.md" exists with no heading named "Reptiles"
    And the current document contains the link [[Animals#Reptiles]]
    Then a BrokenLink error diagnostic is reported on [[Animals#Reptiles]]

  # ── Dependency invalidation ────────────────────────────────────────────────

  Scenario: Renaming a document breaks existing links to its old slug
    Given "notes/Animals.md" exists
    And "notes/Overview.md" contains [[Animals]]
    When "notes/Animals.md" is renamed to "notes/Fauna.md"
    Then a BrokenLink diagnostic appears on [[Animals]] in "notes/Overview.md"

  Scenario: Cross-section link is invalidated when the document's title changes
    Given "notes/Plants.md" has the title "Plants" and heading "## Flowers"
    And "notes/Overview.md" contains [[Plants#Flowers]]
    When the title of "notes/Plants.md" is changed to "Botany"
    Then a BrokenLink diagnostic appears on [[Plants#Flowers]] in "notes/Overview.md"
```

---

## Feature: Completions

> Completion candidates must reflect the current workspace state, respect the configured style, and be scoped to the active folder.

```gherkin
Feature: Completions

  Background:
    Given a Marksman server is running
    And the workspace contains a folder rooted at "notes/"

  # ── Document name completion ────────────────────────────────────────────────

  Scenario: Completing an open wiki-link bracket suggests matching documents
    Given the workspace contains "notes/Animals.md" and "notes/Plants.md"
    When the user types [[ in the current document
    Then the completion list includes "Animals" and "Plants"

  Scenario: Partial text filters the completion list
    Given the workspace contains "notes/Animals.md", "notes/Archive.md", and "notes/Plants.md"
    When the user types [[An
    Then the completion list includes "Animals" and "Archive"
    But the completion list does not include "Plants"

  # ── Section / heading completion ───────────────────────────────────────────

  Scenario: Typing a hash after a document slug suggests its headings
    Given "notes/Animals.md" contains headings "## Mammals" and "## Birds"
    When the user types [[Animals#
    Then the completion list includes "Mammals" and "Birds"

  Scenario: Intra-document heading completion after bare hash
    Given the current document contains headings "## Introduction" and "## Summary"
    When the user types [[#
    Then the completion list includes "Introduction" and "Summary"

  # ── Completion style ────────────────────────────────────────────────────────

  Scenario Outline: Completion style controls the candidate text
    Given "notes/2024-01-15-field-notes.md" has the title "Field Notes"
    And completion.wiki.style is "<style>"
    When the user types [[ in the current document
    Then the completion list includes "<expected>"

    Examples:
      | style       | expected          |
      | title-slug  | Field Notes       |
      | file-stem   | 2024-01-15-field-notes |
      | file-path-stem | notes/2024-01-15-field-notes |

  # ── Folder scoping ──────────────────────────────────────────────────────────

  Scenario: Completion candidates are scoped to the current folder
    Given a workspace with folders "notes/" containing "Animals.md"
    And a second folder "archive/" containing "OldAnimals.md"
    And the user is editing a document inside "notes/"
    When the user types [[
    Then the completion list includes "Animals"
    But the completion list does not include "OldAnimals"
```

---

## Feature: Diagnostics

> Diagnostic rules encode which broken or ambiguous states are worth reporting and which must be suppressed. The suppression policy is as important as the detection logic.

```gherkin
Feature: Diagnostics

  Background:
    Given a Marksman server is running

  # ── Reporting ───────────────────────────────────────────────────────────────

  Scenario: BrokenLink error is reported for an unresolved cross-document wiki-link
    Given a workspace with no document named "Ghost"
    And the current document contains [[Ghost]]
    Then a BrokenLink error diagnostic is reported at the range of [[Ghost]]

  Scenario: AmbiguousLink warning is reported when a slug matches multiple documents
    Given "notes/Animals.md" and "archive/Animals.md" both exist in the workspace
    And the current document contains [[Animals]]
    Then an AmbiguousLink warning diagnostic is reported at the range of [[Animals]]

  Scenario: Non-breaking whitespace after a heading marker is reported
    Given the current document contains a line "# \u00A0My Heading"
    Then a NonBreakableWhitespace diagnostic is reported on that line

  # ── Suppression policy ──────────────────────────────────────────────────────

  Scenario: Cross-document diagnostics are suppressed in single-file mode
    Given the server opened one file directly without a workspace folder
    And that file contains [[AnotherDoc]]
    Then no diagnostic is reported for [[AnotherDoc]]

  Scenario: Shortcut-style reference links are not diagnosed
    Given the current document contains "[some label]" as a shortcut reference
    And no link definition for "some label" exists anywhere in the document
    Then no BrokenLink diagnostic is reported for [some label]

  Scenario: Inline links to non-Markdown URLs are not diagnosed
    Given the current document contains [Visit](https://example.com)
    Then no BrokenLink diagnostic is reported for that link

  Scenario: Inline links to missing Markdown files are diagnosed
    Given the current document contains [Notes](./missing.md)
    And "./missing.md" does not exist
    Then a BrokenLink error diagnostic is reported for that link

  # ── Lifecycle ───────────────────────────────────────────────────────────────

  Scenario: BrokenLink diagnostic clears when the missing document is created
    Given the current document contains [[NewDoc]]
    And no document named "NewDoc" exists
    And a BrokenLink error is active for [[NewDoc]]
    When "NewDoc.md" is added to the workspace folder
    Then the BrokenLink diagnostic for [[NewDoc]] is no longer present

  Scenario: AmbiguousLink diagnostic clears when one duplicate is removed
    Given "notes/Animals.md" and "archive/Animals.md" both exist
    And an AmbiguousLink warning is active for [[Animals]]
    When "archive/Animals.md" is removed from the workspace
    Then the AmbiguousLink diagnostic for [[Animals]] is no longer present
```

---

## Feature: Go-to-Definition

> Navigation must land at the most semantically precise location — the title heading for document links, the specific heading line for section links.

> [!WARNING] Coverage gap
> No LSP-level go-to-definition tests exist today. The domain logic is exercised via `RefsTests` and `EncodingTests` at a lower level, but the full `textDocument/definition` response format is not asserted anywhere. These scenarios are the canonical specification for filling that gap.

```gherkin
Feature: Go-to-definition

  Background:
    Given a Marksman server is running
    And a workspace folder is open

  Scenario: Go-to-definition on a document link navigates to its title heading
    Given "notes/Animals.md" contains "# Animals" at line 1
    And the current document contains [[Animals]]
    When the user invokes go-to-definition on [[Animals]]
    Then the response points to line 1 of "notes/Animals.md"

  Scenario: Go-to-definition on a document link without a title navigates to the top of the file
    Given "notes/Notes.md" has no level-1 heading
    And the current document contains [[Notes]]
    When the user invokes go-to-definition on [[Notes]]
    Then the response points to line 1 of "notes/Notes.md"

  Scenario: Go-to-definition on a section link navigates to the specific heading
    Given "notes/Animals.md" contains "## Mammals" at line 10
    And the current document contains [[Animals#Mammals]]
    When the user invokes go-to-definition on [[Animals#Mammals]]
    Then the response points to line 10 of "notes/Animals.md"

  Scenario: Go-to-definition on an intra-document section link navigates within the same file
    Given the current document contains "## Summary" at line 20
    And the current document contains [[#Summary]] at line 5
    When the user invokes go-to-definition on [[#Summary]]
    Then the response points to line 20 of the current document

  Scenario: Go-to-definition returns no result for a broken link
    Given the current document contains [[NoSuchDoc]]
    And no document named "NoSuchDoc" exists
    When the user invokes go-to-definition on [[NoSuchDoc]]
    Then the response contains no locations
```

---

## Feature: Find References

> Find-references must traverse the full connection graph and return every location that links to a given definition — across all documents in the folder.

```gherkin
Feature: Find references

  Background:
    Given a Marksman server is running
    And a workspace folder is open

  Scenario: Find references on a heading returns all cross-document links to it
    Given "notes/Animals.md" contains "## Mammals" at line 8
    And "notes/Overview.md" contains [[Animals#Mammals]]
    And "notes/Biology.md" contains [[Animals#Mammals]]
    When the user invokes find-references on the "Mammals" heading in "notes/Animals.md"
    Then the response contains 2 locations
    And one location is in "notes/Overview.md"
    And one location is in "notes/Biology.md"

  Scenario: Find references on a document title returns all document-level links
    Given "notes/Animals.md" has the title "# Animals"
    And "notes/Overview.md" contains [[Animals]]
    And "notes/Index.md" contains [[Animals]]
    When the user invokes find-references on the title heading in "notes/Animals.md"
    Then the response contains 2 locations

  Scenario: Find references on a heading returns no results when no links point to it
    Given "notes/Animals.md" contains "## Reptiles"
    And no other document contains [[Animals#Reptiles]]
    When the user invokes find-references on the "Reptiles" heading
    Then the response contains 0 locations

  Scenario: Find references does not cross folder boundaries
    Given a workspace with folders "notes/" and "archive/"
    And "notes/Animals.md" contains "## Mammals"
    And "archive/OldNotes.md" contains [[Animals#Mammals]]
    When the user invokes find-references on "Mammals" from within "notes/"
    Then the response does not include any location in "archive/"
```

---

## Feature: Rename

> Rename must produce a `WorkspaceEdit` that updates every reference atomically. The editor previews the full diff before applying.

> [!WARNING] Coverage gap
> `RefactorTests` covers single-folder heading rename but does not test the LSP `WorkspaceEdit` response shape or the `textDocument/prepareRename` round-trip.

```gherkin
Feature: Rename

  Background:
    Given a Marksman server is running
    And a workspace folder is open

  Scenario: Renaming a heading updates all cross-document links to it
    Given "notes/Animals.md" contains "## Mammals" at line 8
    And "notes/Overview.md" contains [[Animals#Mammals]]
    And "notes/Biology.md" contains [[Animals#Mammals]]
    When the user renames the "Mammals" heading to "Warm-Blooded"
    Then the workspace edit contains changes to "notes/Animals.md", "notes/Overview.md", and "notes/Biology.md"
    And "notes/Overview.md" now contains [[Animals#Warm-Blooded]]
    And the old text [[Animals#Mammals]] no longer appears in any document

  Scenario: Prepare-rename confirms cursor is on a renameable symbol
    Given the cursor is on the "Mammals" heading in "notes/Animals.md"
    When the user invokes prepare-rename
    Then the response contains the current name "Mammals" and its range

  Scenario: Prepare-rename fails when cursor is not on a heading or link
    Given the cursor is on a plain prose paragraph
    When the user invokes prepare-rename
    Then the response indicates no renameable symbol at that position

  Scenario: Rename on a heading with no inbound links changes only the heading text
    Given "notes/Animals.md" contains "## Reptiles"
    And no document links to [[Animals#Reptiles]]
    When the user renames "Reptiles" to "Cold-Blooded"
    Then the workspace edit contains changes only to "notes/Animals.md"
```

---

## Feature: Table of Contents

> The TOC code action inserts or replaces a structured link list derived from the document's headings, filtered by configured levels.

```gherkin
Feature: Table of Contents

  Background:
    Given a Marksman server is running

  Scenario: TOC code action is offered for a document with multiple headings
    Given the current document has headings at levels 1, 2, and 3
    When the user requests code actions at the top of the document
    Then the response includes a "Create Table of Contents" action

  Scenario: Applying the TOC action inserts an intra-document link list
    Given the current document contains:
      """
      # Animals
      ## Mammals
      ## Birds
      """
    When the user applies the "Create Table of Contents" action
    Then the document contains a list with [[#Mammals]] and [[#Birds]]

  Scenario: TOC respects the configured include levels
    Given code_action.toc.include is [2, 3]
    And the current document contains headings at levels 1, 2, and 3
    When the user applies the "Create Table of Contents" action
    Then the TOC list contains only h2 and h3 headings
    And the h1 heading does not appear in the TOC

  Scenario: Re-applying the TOC action replaces the existing block
    Given the current document already contains a TOC block with [[#Introduction]]
    And a new heading "## Summary" has been added to the document
    When the user applies the "Create Table of Contents" action again
    Then the TOC block is updated to include [[#Summary]]
    And there is exactly one TOC block in the document

  Scenario: TOC code action is disabled when code_action.toc.enable is false
    Given code_action.toc.enable is false
    And the current document has multiple headings
    When the user requests code actions
    Then the response does not include a "Create Table of Contents" action
```

---

## Feature: Document and Folder Lifecycle

> The server's view of the workspace must stay consistent as documents are opened, changed, closed, and as folders are added or removed.

```gherkin
Feature: Document and folder lifecycle

  Background:
    Given a Marksman server is running

  Scenario: Opening a new document adds it as a link target
    Given the workspace folder "notes/" contains "Animals.md"
    And "notes/Plants.md" does not yet exist as a file on disk
    When the LSP client sends didOpen for "notes/Plants.md" with initial content
    Then [[Plants]] in any other document resolves to "notes/Plants.md"

  Scenario: Closing an editor-modified document reverts to the disk version
    Given "notes/Animals.md" was opened and its text changed in the editor
    When the LSP client sends didClose for "notes/Animals.md"
    Then Marksman indexes the on-disk version of "notes/Animals.md"
    And the in-editor changes are no longer visible to the server

  Scenario: Adding a workspace folder indexes all Markdown documents in it
    Given the LSP client sends workspace/didChangeWorkspaceFolders to add "notes/"
    And "notes/" contains "Animals.md" and "Plants.md"
    Then both documents are indexed and available as link targets

  Scenario: Removing a workspace folder clears its documents from the workspace
    Given "notes/" is an active workspace folder with "Animals.md"
    When the LSP client removes "notes/" as a workspace folder
    Then [[Animals]] no longer resolves and produces a BrokenLink diagnostic

  Scenario: A folder without a workspace marker is accepted with a warning logged
    Given a directory "plain-dir/" with no .marksman.toml, .git, .hg, or .svn
    When the LSP client adds "plain-dir/" as a workspace folder
    Then Marksman indexes documents in "plain-dir/" normally
    And a warning is written to the server log

  Scenario: A single-file folder is superseded when its root is added as a workspace folder
    Given the server opened "notes/Animals.md" in single-file mode
    When the LSP client adds "notes/" as a workspace folder
    Then the single-file Animals context is evicted
    And all Markdown documents in "notes/" are indexed together
    And [[Animals]] resolves within the multi-file context
```

---

## Feature: Configuration

> Configuration must layer correctly: defaults < user config < per-folder `.marksman.toml`. A folder-level setting must always win over its user-level counterpart.

```gherkin
Feature: Configuration

  Scenario: Per-folder config overrides the user-level config for the same key
    Given the user config sets completion.wiki.style to "title-slug"
    And "notes/.marksman.toml" sets completion.wiki.style to "file-stem"
    And the user is editing a document in "notes/"
    When completions are requested
    Then the file-stem style is applied to completion candidates

  Scenario: An empty .marksman.toml is sufficient to mark a project root
    Given "notes/.marksman.toml" is empty
    When the LSP client opens "notes/" as a workspace folder
    Then Marksman treats "notes/" as a project root
    And all default settings are in effect

  Scenario: User config applies to all folders that do not override it
    Given the user config sets core.paranoid to true
    And "notes/" has no .marksman.toml
    And "archive/" has no .marksman.toml
    Then both folders operate in paranoid mode

  Scenario: Changing the workspace config triggers a folder reload
    Given a workspace with folder "notes/"
    When the user updates "notes/.marksman.toml" to set completion.wiki.style to "file-stem"
    And the LSP client notifies the server of the configuration change
    Then subsequent completions in "notes/" use the file-stem style
```

---

## Automation Notes

### Harness: Reqnroll (.NET)

Marksman is a .NET 9 xunit project. The recommended BDD harness is **Reqnroll** (install via `dotnet new install Reqnroll.Templates.DotNet`), bound to an in-process LSP fixture.

**Project layout suggestion**

```
Tests.BDD/
  Features/
    WikiLinkResolution.feature
    Completions.feature
    ...
  Steps/
    WorkspaceSteps.cs     # Given: workspace with document X
    LspSteps.cs           # When: follows link / requests completion
    DiagnosticSteps.cs    # Then: diagnostic present/absent
    NavigationSteps.cs    # Then: link target is …
  Fixtures/
    LspFixture.fs         # in-process server with fake LSP client
    FolderBuilder.fs      # builds FakeFolder instances from scenario context
```

**Step binding sketch (F# / Reqnroll)**

```fsharp
[<Binding>]
type WorkspaceSteps(fixture: LspFixture) =
    [<Given("the workspace contains {string}")>]
    member _.GivenDoc(path: string) =
        fixture.AddDocument(path, content = "")

    [<When("the user follows the link \\[\\[(.+)\\]\\]")>]
    member _.WhenFollowsLink(linkText: string) =
        fixture.LastResponse <- fixture.SendDefinitionRequest(linkText)

    [<Then("the link target is {string}")>]
    member _.ThenLinkTarget(expected: string) =
        let loc = fixture.LastResponse |> Seq.exactlyOne
        Assert.EndsWith(expected, loc.Uri)
```

### Dry-Run Validation in CI

Reqnroll supports `--dry-run` to validate that all scenario steps have bindings before executing tests. Add this to CI as a gate before the full suite:

```yaml
- run: dotnet test Tests.BDD --no-build -- --reqnroll:dryRun=true
```

---

## What Stays in Unit Tests

The following rules are too granular or too algorithmic for Gherkin, and are better expressed as F# xunit tests directly. Converting them to scenarios would add ceremony without adding communication value.

| Rule | Current test location |
|------|-----------------------|
| Slug normalization: case-folding, trim, whitespace | `MiscTests.slug` |
| `InternPath` classification (ExactAbs / ExactRel / Approx) | `RefsTests.InternNameTests` |
| `Conn.update` incremental correctness (all graph mutation cases) | `ConnTest.ConnGraphTests` |
| TOML config parsing: valid and malformed inputs | `ConfigTests` |
| Path / URI normalization (Windows backslash, drive-letter casing) | `PathsTests` |
| Text change application (incremental LSP edits) | `TextTests` |
| CST/AST parsing: wiki-link syntax variants, edge cases | `ParserTests` |
| `SuffixTree` insert/remove/filter correctness | `SuffixTreeTests` |
| `Index.linkAtPos` / `declAtPos` position lookup | `AstTests` |
| Individual `DiagEntry` construction rules | `DiagTest` (existing thin coverage) |

---

## Coverage Map

Relation between these scenarios and existing test coverage:

| Feature | Existing coverage | Gap |
|---------|------------------|-----|
| Wiki-link resolution | `RefsTests`, `ConnTest`, `EncodingTests` (component level) | No LSP `definition` response shape tests |
| Completions | `ComplTests` (comprehensive snapshots) | No `file-path-stem` style test; no cross-folder scoping test |
| Diagnostics | `DiagTest` (7 cases), `ConnTest` | Ambiguous-link scenario; diagnostic lifecycle (clear on resolve) |
| Go-to-definition | `RefsTests.BasicRefsTests` (domain only) | **All LSP-level scenarios are unimplemented** |
| Find references | `RefsTests.BasicRefsTests` (domain only) | Cross-folder boundary exclusion not asserted |
| Rename | `RefactorTests` (2 heading cases) | `prepareRename` response; no-inbound-links case; LSP `WorkspaceEdit` shape |
| Table of Contents | `TocTests` (comprehensive) | `toc.enable = false` suppression; exact TOC replacement edit |
| Lifecycle | `WorkspaceTest`, `StateTests` (partial) | didClose disk-revert; folder removal clears resolution |
| Configuration | `WorkspaceTest.folderConfig_*` | Config-change-triggers-reload scenario |

---

## Related

- [[design/domain-layer|Domain Layer]] — bounded contexts and aggregates that these behaviors operate on
- [[features/wiki-links|Wiki Links]] — wiki-link syntax and resolution rule details
- [[features/completions|Completions]] — completion style config and candidate generation
- [[features/diagnostics|Diagnostics]] — diagnostic suppression policy details
- [[features/navigation|Navigation]] — go-to-definition and find-references implementation
- [[features/rename|Rename]] — cross-document rename via `Refactor.fs`
- [[features/toc|Table of Contents]] — TOC detection, rendering, and insertion logic
- [[research/lsp/04-synchronization|LSP 3.17 — Document Synchronization]] — `didOpen`/`didChange`/`didClose` message definitions
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — `definition`, `references`, `rename`, `completion` protocol specifications
