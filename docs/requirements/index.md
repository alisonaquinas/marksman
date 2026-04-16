---
title: "Requirements Index"
date: 2026-04-16
tags:
  - wiki/meta
  - requirements/index
---

# Requirements Index

Planguage-style functional requirements for Marksman, organised by feature area.
Each requirement carries one stable `Tag`, a measurable `Scale`, a reproducible `Meter`, and target levels grounded in source evidence.

> [!NOTE] Evidence policy
> Fail and Goal levels are set only when the source material (code, configuration docs, feature docs) provides concrete evidence. Requirements that lack sufficient numeric evidence are recorded as tagged skeletons with explicit open questions.

---

## Requirement Files

| File | Feature area | Requirement count |
|------|-------------|------------------:|
| [[requirements/link-resolution\|link-resolution.md]] | Wiki-link and Markdown link resolution | 4 |
| [[requirements/completions\|completions.md]] | Completion candidates and style | 3 |
| [[requirements/diagnostics\|diagnostics.md]] | Broken-link, ambiguous-link, and whitespace diagnostics | 5 |
| [[requirements/navigation\|navigation.md]] | Go-to-definition, find references, and code lenses | 3 |
| [[requirements/rename\|rename.md]] | Rename refactoring and prepare-rename | 3 |
| [[requirements/table-of-contents\|table-of-contents.md]] | TOC code action generation | 4 |
| [[requirements/workspace\|workspace.md]] | Project detection, multi-folder, and file filtering | 3 |
| [[requirements/configuration\|configuration.md]] | Config layering, validation, and fault isolation | 4 |

---

## Master Tag Index

| Tag | Gist | File |
|-----|------|------|
| `Link.Wiki.StyleBinding` | Wiki-link items match the configured wiki style | [[requirements/link-resolution\|link-resolution]] |
| `Link.Resolution.ModeScope` | Single-file mode suppresses cross-file resolution | [[requirements/link-resolution\|link-resolution]] |
| `Link.Resolution.IgnoreGlob` | Ignored files absent from completion and definition index | [[requirements/link-resolution\|link-resolution]] |
| `Link.Inline.URLSkip` | Non-Markdown URLs in inline links produce no diagnostic | [[requirements/link-resolution\|link-resolution]] |
| `Completion.Candidates.Cap` | Candidate list capped at configured limit | [[requirements/completions\|completions]] |
| `Completion.Trigger.Coverage` | All three trigger characters return candidates in context | [[requirements/completions\|completions]] |
| `Completion.Incomplete.Flag` | `isIncomplete` set when candidate cap is reached | [[requirements/completions\|completions]] |
| `Diagnostic.Severity.WikiLink` | Broken/ambiguous wiki-links get Error severity | [[requirements/diagnostics\|diagnostics]] |
| `Diagnostic.Severity.MarkdownLink` | Broken/ambiguous markdown links get Warning severity | [[requirements/diagnostics\|diagnostics]] |
| `Diagnostic.Code.Assignment` | Each diagnostic type carries its assigned numeric code | [[requirements/diagnostics\|diagnostics]] |
| `Diagnostic.Debounce.Latency` | Diagnostics published within acceptable time after last change | [[requirements/diagnostics\|diagnostics]] |
| `Diagnostic.Ambiguous.RelatedInfo` | Ambiguous-link diagnostics list all duplicate definition locations | [[requirements/diagnostics\|diagnostics]] |
| `Navigation.Definition.LinkTypes` | Go-to-definition works for all three link syntaxes | [[requirements/navigation\|navigation]] |
| `Navigation.References.Completeness` | Find-references returns all refs in folder that resolve to target | [[requirements/navigation\|navigation]] |
| `Navigation.CodeLens.Count` | Each heading shows a reference-count code lens | [[requirements/navigation\|navigation]] |
| `Rename.Refactoring.Completeness` | All cross-document refs to the renamed element are updated | [[requirements/rename\|rename]] |
| `Rename.Prepare.Rejection` | prepareRename rejects non-renameable cursor positions | [[requirements/rename\|rename]] |
| `Rename.StyleBinding.Consistency` | Rename only updates refs bound via the active wiki style | [[requirements/rename\|rename]] |
| `TOC.Generation.Markers` | Generated TOC includes both HTML comment delimiters | [[requirements/table-of-contents\|table-of-contents]] |
| `TOC.Levels.Filter` | TOC entries only include configured heading levels | [[requirements/table-of-contents\|table-of-contents]] |
| `TOC.Slug.GLFM` | Duplicate headings receive GLFM numeric suffixes when enabled | [[requirements/table-of-contents\|table-of-contents]] |
| `TOC.Empty.NoAction` | Documents with no headings produce no TOC code action | [[requirements/table-of-contents\|table-of-contents]] |
| `Workspace.ProjectDetection.Root` | Folders with `.marksman.toml` or VCS root detected as projects | [[requirements/workspace\|workspace]] |
| `Workspace.MultiFolder.Isolation` | Cross-root link resolution is not performed | [[requirements/workspace\|workspace]] |
| `Workspace.FileExtension.Filter` | Only files with configured extensions enter the index | [[requirements/workspace\|workspace]] |
| `Config.Precedence.Layering` | Project config overrides user config overrides built-in defaults | [[requirements/configuration\|configuration]] |
| `Config.Validation.Candidates` | `completion.candidates` must be strictly positive | [[requirements/configuration\|configuration]] |
| `Config.Fault.Isolation` | Malformed config is dropped without crashing the server | [[requirements/configuration\|configuration]] |
| `Config.TextSync.Default` | Absent text-sync config defaults to Full | [[requirements/configuration\|configuration]] |

---

## Related

- [[design/domain-layer|Domain Layer]] — aggregates and invariants that requirements constrain
- [[design/behavior-layer|Behavior Layer]] — BDD scenarios that validate these requirements
- [[design/api-layer|API Layer]] — LSP method catalog and sequence diagrams
- [[architecture/overview|Overview]] — system architecture context
