---
title: "Requirements — Diagnostics"
date: 2026-04-16
tags:
  - wiki/requirements
  - requirements/diagnostics
---

# Diagnostics Requirements

Requirements governing how Marksman emits, codes, and times its three diagnostic types: `BrokenLink` (code `"2"`), `AmbiguousLink` (code `"1"`), and `NonBreakableWhitespace` (code `"3"`).

> [!NOTE] Diagnostic types
> All diagnostics carry `source = "Marksman"`. Severity differs by link syntax (wiki-link vs Markdown link) and diagnostic kind. The 200 ms debounce in `DiagnosticsManager` is the primary latency driver; compute time is additive.

---

## Tag: Diagnostic.Severity.WikiLink

**Gist:** A `BrokenLink` or `AmbiguousLink` diagnostic on a wiki-link (`[[…]]`) must carry severity `Error`.

**Ambition:** Wiki-links are Marksman's primary navigation construct. A broken or ambiguous wiki-link represents a structural failure in the document graph that warrants an editor error marker, not merely a warning. Correct severity lets users configure editor UI (gutter icons, problem counts) to surface wiki-link failures prominently.

**Scale:** Percentage of `BrokenLink` (code `"2"`) and `AmbiguousLink` (code `"1"`) diagnostics emitted for wiki-link elements that carry LSP `severity = 1` (Error), measured across a workspace containing known broken and ambiguous wiki-links.

**Meter:** Integration test: create a document containing one broken wiki-link (`[[does-not-exist]]`) and one ambiguous wiki-link (`[[dup]]` where two documents share the slug). Receive `publishDiagnostics`. Assert that both diagnostics have `severity = 1`.

Count Error-severity wiki-link diagnostics ÷ total wiki-link broken/ambiguous diagnostics × 100.

**Fail:** Any broken or ambiguous wiki-link diagnostic has `severity ≠ 1`.

**Goal:** 100 % of broken and ambiguous wiki-link diagnostics carry `severity = 1` (Error).

**Stakeholders:** Wiki authors, editor-plugin developers.

**Owner:** Marksman contributors.

**Source:** `Marksman/Diag.fs` — `WL` severity mapping; `docs/features.md` — diagnostics severity table.

---

## Tag: Diagnostic.Severity.MarkdownLink

**Gist:** A `BrokenLink` or `AmbiguousLink` diagnostic on a Markdown inline or reference link must carry severity `Warning` (not Error).

**Ambition:** Standard Markdown links are looser by convention — many point to external resources or are intentionally placeholder. Using Warning rather than Error keeps the signal proportional: the author is alerted without the document being flagged as containing an error. The severity difference also allows users to filter diagnostics by severity to focus on wiki-link failures first.

**Scale:** Percentage of `BrokenLink` and `AmbiguousLink` diagnostics emitted for Markdown inline (`[text](path)`) and reference-style (`[text][label]`) link elements that carry LSP `severity = 2` (Warning).

**Meter:** Integration test: create a document with one broken inline link (`[text](missing.md)`) and one broken reference link (`[text][missing-label]` with no definition). Receive `publishDiagnostics`. Assert both diagnostics have `severity = 2`.

Count Warning-severity markdown-link diagnostics ÷ total markdown-link broken/ambiguous diagnostics × 100.

**Fail:** Any broken or ambiguous Markdown link diagnostic has `severity ≠ 2`.

**Goal:** 100 % of broken and ambiguous Markdown inline/reference-link diagnostics carry `severity = 2` (Warning).

**Stakeholders:** Documentation authors, editor-plugin developers.

**Owner:** Marksman contributors.

**Source:** `Marksman/Diag.fs` — `ML` severity mapping; `docs/features.md` — diagnostics severity table.

---

## Tag: Diagnostic.Code.Assignment

**Gist:** Each diagnostic type must carry its designated LSP diagnostic code: `"1"` for `AmbiguousLink`, `"2"` for `BrokenLink`, `"3"` for `NonBreakableWhitespace`.

**Ambition:** Stable, distinct codes allow editor users and scripts to filter or suppress specific diagnostic types (e.g., suppress all code-`"3"` whitespace warnings in a legacy vault). If codes are incorrect, missing, or swapped, client-side suppression rules break silently.

**Scale:** Percentage of emitted diagnostics whose `code` field matches the expected value for their type, measured across at least one instance of each of the three diagnostic kinds.

**Meter:** Integration test: produce one document triggering each type — a broken wiki-link (code `"2"`), an ambiguous wiki-link (code `"1"`), and a heading whose `#` is followed by U+00A0 (code `"3"`). Receive `publishDiagnostics`. Assert each diagnostic's `code` field equals the expected string. Count correct-code diagnostics ÷ total diagnostics × 100.

**Fail:** Any diagnostic carries a `code` value that does not match its type (`"1"` for ambiguous, `"2"` for broken, `"3"` for non-breakable whitespace).

**Goal:** 100 % of diagnostics carry the correct code for their type.

**Stakeholders:** Editor-plugin developers, automation tooling authors.

**Owner:** Marksman contributors.

**Source:** `Marksman/Diag.fs` — `diagCode` match; `docs/features.md` — diagnostic codes table.

---

## Tag: Diagnostic.Debounce.Latency

**Gist:** After the last `textDocument/didChange` notification in a burst of edits, the server must publish updated diagnostics within an acceptable elapsed time.

**Ambition:** Diagnostics that arrive too quickly flood the editor during active typing; diagnostics that arrive too slowly feel unresponsive after the user stops. The 200 ms debounce timer in `DiagnosticsManager` is the dominant latency component. The total time from last edit to `publishDiagnostics` must remain within a range that feels instant to an author who has paused after a sentence.

**Scale:** Elapsed wall-clock time in milliseconds from the timestamp of the last `textDocument/didChange` notification in a burst to the timestamp of the corresponding `textDocument/publishDiagnostics` notification, measured on a development-class machine with a workspace of ≤ 100 documents.

**Meter:** Instrumented integration test: send a burst of 5 `didChange` notifications at 50 ms intervals, then stop. Record the timestamp of the last `didChange` and the timestamp of the next `publishDiagnostics`. Compute the difference. Repeat 20 times. Report min, median, p95.

**Fail:** p95 latency > 1 000 ms (diagnostics feel noticeably delayed after the author stops typing).

**Goal:** p95 latency ≤ 500 ms on a 100-document workspace on a development-class machine.

**Stretch:** p95 latency ≤ 300 ms.

**Stakeholders:** Wiki authors, editor UX.

**Owner:** Marksman contributors.

**Source:** `Marksman/Server.fs` — `DiagnosticsManager` `TryReceive(timeout = 200)` debounce; benchmark data not yet collected.

**Open questions:**
- What is the current median diagnostic compute time for a 100-document workspace?
- Does incremental reference mode (`core.incremental_references = true`) materially reduce compute time?
- Should latency be measured per-document or for the full batch?

---

## Tag: Diagnostic.Ambiguous.RelatedInfo

**Gist:** Every `AmbiguousLink` diagnostic must include `relatedInformation` entries that point to each duplicate definition location, so the author can navigate directly to the conflicting definitions.

**Ambition:** An ambiguous-link error is only actionable when the author can see which documents share the conflicting slug. Without `relatedInformation`, the author must manually search for duplicates. With it, the editor can offer "Go to duplicate" navigation directly from the diagnostic hover or Problems panel.

**Scale:** For each `AmbiguousLink` diagnostic emitted, the count of `relatedInformation` entries equals the count of duplicate definitions that caused the ambiguity. Measured as: percentage of emitted `AmbiguousLink` diagnostics where `relatedInformation.length = duplicate_definition_count`.

**Meter:** Integration test: create two documents (`foo.md` titled "Foo" and `foo-copy.md` also titled "Foo") and a third document with `[[Foo]]`. Receive `publishDiagnostics`. Assert the `AmbiguousLink` diagnostic has `relatedInformation` with exactly 2 entries, each pointing to one of the duplicate title definitions.

Count correct-relatedInfo diagnostics ÷ total AmbiguousLink diagnostics × 100.

**Fail:** Any `AmbiguousLink` diagnostic is missing `relatedInformation`, or `relatedInformation.length < duplicate_definition_count`.

**Goal:** 100 % of `AmbiguousLink` diagnostics include one `relatedInformation` entry per duplicate definition.

**Stakeholders:** Wiki authors, editor-plugin developers.

**Owner:** Marksman contributors.

**Source:** `Marksman/Diag.fs` — `relatedInformation` construction; LSP 3.17 `Diagnostic.relatedInformation` field.

---

## Related

- [[requirements/index|Requirements Index]] — master tag list
- [[design/behavior-layer|Behavior Layer]] — Feature: Diagnostics BDD scenarios
- [[design/domain-layer|Domain Layer]] — BC5: Diagnostics bounded context
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — `textDocument/publishDiagnostics` specification
