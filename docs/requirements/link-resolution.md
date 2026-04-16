---
title: "Requirements — Link Resolution"
date: 2026-04-16
tags:
  - wiki/requirements
  - requirements/link-resolution
---

# Link Resolution Requirements

Requirements governing how Marksman resolves wiki-links and Markdown links to documents and headings, including style binding, mode-based scoping, ignore-glob filtering, and URL suppression.

> [!NOTE] Scope
> These requirements apply to the three supported link syntaxes: wiki-links (`[[doc]]`, `[[doc#heading]]`, `[[#heading]]`), inline Markdown links (`[text](path.md#heading)`), and reference-style Markdown links (`[text][label]` + `[label]: url`). "Resolution" means the server can map a link to zero, one, or more destination `(doc, scope)` pairs.

---

## Tag: Link.Wiki.StyleBinding

**Gist:** Every wiki-link completion item and rename edit must reflect the document name form prescribed by the active `completion.wiki.style` setting.

**Ambition:** Users who configure a consistent naming convention must not see a mixture of title-derived and file-stem-derived slugs in completion lists or rename results. A style mismatch silently corrupts a vault's internal link graph when documents are later renamed or moved.

**Scale:** Percentage of wiki-link completion items returned by `textDocument/completion`, and wiki-link text edits produced by `textDocument/rename`, whose label or replacement text conforms to the active style rule, measured across all three style modes (`title-slug`, `file-stem`, `file-path-stem`) in a controlled workspace.

**Meter:** Integration test: open a workspace containing at least five documents with distinct titles and file stems. Set `completion.wiki.style` to each of the three modes in turn. Invoke `textDocument/completion` at a `[[` cursor in each mode. Assert that every item's `insertText` matches the expected form. Repeat `textDocument/rename` on one heading (title-slug mode) and one file (file-stem mode). Count conforming items ÷ total items × 100.

**Fail:** Any single completion item or rename edit contradicts the configured style — i.e., a `file-stem` item appears when `title-slug` is active, or vice versa. (0 % non-conformance is the threshold; any non-zero rate is a fail.)

**Goal:** 100 % of items and edits conform to the active style in all three modes.

**Stakeholders:** Wiki authors, editor-plugin developers, QA engineers.

**Owner:** Marksman contributors.

**Source:** `docs/configuration.md` — `completion.wiki.style`; `Marksman/Compl.fs` style dispatch; `Marksman/Refactor.fs`.

---

## Tag: Link.Resolution.ModeScope

**Gist:** In single-file mode, cross-file link resolution must be suppressed; the server must not offer definitions, references, completions, or diagnostics for links that point outside the single open document.

**Ambition:** Single-file mode exists for users who open individual Markdown files without a project root. Offering cross-file results in that mode would require scanning an undefined file set, producing incorrect or noisy output. Suppressing cross-file resolution makes the mode's limits predictable and honest.

**Scale:** For each LSP feature (completion, go-to-definition, find-references, diagnostics), the binary outcome — does the response omit cross-file results when the server is in single-file mode? Measured as: number of LSP responses in single-file mode that contain at least one cross-file result ÷ total responses × 100 (must equal 0 %).

**Meter:** Integration test: open a single `.md` file with a `[[other-doc]]` wiki-link, where `other-doc.md` does not exist in any loaded folder. Assert that `textDocument/completion` does not list `other-doc`, `textDocument/definition` returns `null`, `textDocument/references` returns `[]`, and `textDocument/publishDiagnostics` contains no `BrokenLink` diagnostic for the cross-file link.

**Fail:** Any LSP response in single-file mode includes a cross-file completion candidate, definition location, reference location, or diagnostic for a cross-file link.

**Goal:** 0 cross-file results in all four feature responses when operating in single-file mode.

**Stakeholders:** Individual note authors, plugin developers.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — single-file mode section; `Marksman/Server.fs` — `TextDocumentDidOpen` singleton-folder branch; `Marksman/Diag.fs` — resolution scope guard.

---

## Tag: Link.Resolution.IgnoreGlob

**Gist:** Files matched by active ignore rules (from `.gitignore`, `.hgignore`, or `.ignore` files found in any sub-folder) must be excluded from the workspace index and must not appear in completion, definition, or reference results.

**Ambition:** Ignore-glob support lets teams exclude generated output files, vendor trees, or archived notes from navigation. Without reliable filtering, noisy completion lists and spurious definition results undermine the server's usefulness on large repositories.

**Scale:** Percentage of on-disk Markdown files that match at least one active ignore pattern and are absent from (a) completion candidate lists, (b) go-to-definition results, and (c) find-references results, measured in a workspace with a representative ignore file.

**Meter:** Integration test: create a workspace with ten `.md` files and a `.gitignore` file that excludes five of them by name. Invoke `textDocument/completion` at a `[[` cursor and assert that none of the excluded file titles or stems appear. Invoke `textDocument/definition` from a link targeting an excluded file and assert `null` is returned. Count excluded files absent from results ÷ excluded files total × 100.

**Fail:** Any ignored file appears in a completion candidate list, definition result, or reference result.

**Goal:** 100 % of ignored files absent from all index-derived results.

**Stakeholders:** Repository maintainers, wiki authors with large vaults.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — ignore patterns section; `Marksman/GitIgnore.fs`.

---

## Tag: Link.Inline.URLSkip

**Gist:** Inline Markdown links whose target is a URL (not a local Markdown file path) must produce no diagnostic, regardless of whether the URL is reachable.

**Ambition:** Marksman cannot and should not validate remote URLs. Raising a diagnostic for every external link would produce false positives that make the server unusable in documentation repositories with heavy external linking. The rule must be applied consistently so authors can predict which links are checked.

**Scale:** Number of `textDocument/publishDiagnostics` responses that include a `BrokenLink` or `AmbiguousLink` diagnostic for an inline link whose target is an HTTP/HTTPS URL or any URI scheme other than a bare relative file path, across a representative document set.

**Meter:** Integration test: open a document containing five inline links with `https://` targets and five with bare relative `.md` targets (some broken). Assert that no diagnostic is emitted for any of the `https://` links. Count URL-link diagnostics ÷ total URL links × 100 (must equal 0 %).

**Fail:** Any diagnostic of code `"1"` or `"2"` is emitted for an inline link whose target begins with a URI scheme (`http://`, `https://`, `mailto:`, etc.).

**Goal:** 0 diagnostics emitted for any URL-schemed inline link target.

**Stakeholders:** Documentation authors, technical writers.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — link diagnostics section; `Marksman/Refs.fs` — URL classification guard.

---

## Related

- [[requirements/index|Requirements Index]] — master tag list
- [[design/domain-layer|Domain Layer]] — `Link.Resolution.ModeScope` corresponds to BC3 (Reference Resolution) scope invariant
- [[design/behavior-layer|Behavior Layer]] — Feature: Wiki-link resolution scenarios
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — completion and definition spec
