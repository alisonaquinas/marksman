---
title: "Requirements — Completions"
date: 2026-04-16
tags:
  - wiki/requirements
  - requirements/completions
---

# Completions Requirements

Requirements governing completion candidate generation: the hard cap on result size, trigger-character coverage, and the incomplete-list signal sent to clients when the cap is reached.

> [!NOTE] Scope
> These requirements cover the `textDocument/completion` method only. The style binding between completion item labels and the configured wiki style is governed by [[requirements/link-resolution#Tag: Link.Wiki.StyleBinding|Link.Wiki.StyleBinding]].

---

## Tag: Completion.Candidates.Cap

**Gist:** The server must never return more completion items than the value of `completion.candidates` (default 50) in a single `textDocument/completion` response.

**Ambition:** An unbounded candidate list stalls editors and wastes network bandwidth on large Zettelkasten vaults with hundreds of documents. The configurable cap lets workspace owners tune the tradeoff between recall and responsiveness without a server-side change.

**Scale:** Count of `CompletionItem` entries in each `textDocument/completion` response, measured when the total matching candidates exceeds the configured cap. Condition: a workspace containing more documents than the configured `completion.candidates` value, with a `[[` trigger that would match all of them.

**Meter:** Integration test: create a workspace with 60 `.md` files. Leave `completion.candidates` at the default value of 50. Invoke `textDocument/completion` at a `[[` cursor with an empty partial text (all 60 documents match). Assert that `response.items.length ≤ 50`.

Repeat with `completion.candidates = 10`: assert `response.items.length ≤ 10`.

**Fail:** `response.items.length > completion.candidates` in any response where the underlying candidate pool exceeds the cap.

**Goal:** `response.items.length ≤ completion.candidates` in 100 % of responses, for all configured cap values ≥ 1.

**Stakeholders:** Editor-plugin developers, wiki authors with large vaults.

**Owner:** Marksman contributors.

**Source:** `Marksman/Config.fs` — `ComplCandidates` default 50; `Marksman/Server.fs` `TextDocumentCompletion` — `Seq.truncate maxCompletions`.

---

## Tag: Completion.Trigger.Coverage

**Gist:** The server must advertise and honour completion triggers for the characters `[`, `#`, and `(`, returning non-empty candidate lists in contexts where those characters begin a valid link element.

**Ambition:** Editors use the advertised trigger characters to decide when to automatically open the completion UI. If a trigger character is advertised but returns no candidates in its primary context, users see a distracting empty popup. If a valid trigger context is missing from the advertised set, users must manually request completion.

**Scale:** For each of the three trigger characters, the boolean: does a `textDocument/completion` request at a cursor position where that character begins a valid link element return at least one item? Measured across a workspace with at least one document, one heading, and one existing link.

**Meter:** Integration test with one workspace folder containing three documents A, B, C:
1. `[` trigger — position cursor at `[[` prefix; assert at least one item returned (document title or stem).
2. `#` trigger — position cursor at `[[A#` prefix; assert at least one item returned (heading from doc A).
3. `(` trigger — position cursor at `[text](` prefix; assert at least one item returned (file path or stem).

Pass = all three assertions succeed. Fail = any assertion fails.

**Fail:** Any of the three trigger characters fails to produce at least one candidate in its canonical trigger context.

**Goal:** All three trigger characters return at least one candidate in their canonical context in 100 % of test runs.

**Stakeholders:** Editor-plugin developers.

**Owner:** Marksman contributors.

**Source:** `Marksman/Server.fs` — `mkServerCaps` `TriggerCharacters = Some [| '['; '#'; '(' |]`; `Marksman/Compl.fs` — candidate dispatch by link kind.

---

## Tag: Completion.Incomplete.Flag

**Gist:** When the candidate pool exceeds the configured cap, the server must set `CompletionList.isIncomplete = true` in the response so clients know to re-query as the user types more characters.

**Ambition:** Without `isIncomplete`, a client that caches a completion list treats the 50-item response as the complete set and stops querying even when the user narrows the prefix to a single match. Setting `isIncomplete` correctly keeps the completion UI accurate as the user types.

**Scale:** Percentage of `textDocument/completion` responses where the underlying candidate pool exceeds `completion.candidates` and `isIncomplete` is set to `true`. Condition: same as `Completion.Candidates.Cap`.

**Meter:** Integration test: workspace with 60 documents, cap = 50. Invoke completion with an empty partial (pool = 60 > 50). Assert `response.isIncomplete = true`.

Separately: workspace with 5 documents, cap = 50. Invoke completion with an empty partial (pool = 5 < 50). Assert `response.isIncomplete = false`.

**Fail:** `isIncomplete = false` when the underlying pool exceeds the cap, or `isIncomplete = true` when the pool is within the cap.

**Goal:** `isIncomplete` is set correctly (true when pool > cap, false when pool ≤ cap) in 100 % of responses.

**Stakeholders:** Editor-plugin developers.

**Owner:** Marksman contributors.

**Source:** `Marksman/Server.fs` — `let isIncomplete = Array.length candidates >= maxCompletions`; LSP 3.17 `CompletionList.isIncomplete` semantics.

---

## Related

- [[requirements/index|Requirements Index]] — master tag list
- [[requirements/link-resolution#Tag: Link.Wiki.StyleBinding|Link.Wiki.StyleBinding]] — completion item label style
- [[design/behavior-layer|Behavior Layer]] — Feature: Completions BDD scenarios
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — `textDocument/completion` specification
