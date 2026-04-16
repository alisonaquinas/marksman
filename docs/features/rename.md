---
title: Rename
tags:
  - wiki/feature
---

# Rename

Marksman supports ==LSP rename== (`textDocument/rename`) for headings. Renaming a heading updates every wiki link and Markdown link across the workspace that resolves to it, producing a single `WorkspaceEdit` that editors apply atomically.

## What Can Be Renamed

Currently, the rename target must be a **heading** (H1–H6). Renaming the H1 of a document is the most common operation: it changes the document's canonical title, which is what `[[doc]]` links resolve against when `core.title_from_heading = true`.

> [!NOTE]
> Filename rename is not currently part of the LSP rename operation. Marksman renames the heading text and updates all inbound links, but does not rename the `.md` file on disk. Some editors (e.g., Neovim with nvim-lspconfig) offer a separate file-rename command that can be combined with Marksman's heading rename.

## RenameResult and WorkspaceEdit Construction

The rename pipeline lives in `Refactor.fs`:

1. **Locate the definition.** The heading at the cursor is resolved to its `ScopedSym` via `Index`.
2. **Gather all references.** `Refs.findElementRefs` queries the [[Connection Graph]] for all `Conn.resolved` edges whose target is the heading's `ScopedSym`. This produces a list of `(docUri, linkRange)` pairs spanning the entire workspace.
3. **Build text changes.** For each reference, a `TextEdit` is generated that rewrites the link text (the part inside `[[…]]` or `[…](…)`) to the new heading slug. For the definition itself, the heading line is updated in place.
4. **Assemble the WorkspaceEdit.** All edits are grouped by `documentUri` into a `WorkspaceEdit`, which the LSP client applies as a single undoable transaction.

> [!WARNING]
> If a wiki link uses a custom display label (`[[doc|My Label]]`), only the link target portion is rewritten. The display label is left unchanged, since it was explicitly set by the user.

## Cross-file Scope

Because `Refs.findElementRefs` traverses the full [[Connection Graph]], rename is inherently cross-file. A heading referenced in twenty documents will produce twenty `TextEdit` entries across those documents, all within the same `WorkspaceEdit`. The client shows a preview diff before applying.

> [!TIP]
> Use the **prepare rename** request (`textDocument/prepareRename`) before committing. Marksman validates that the cursor is on a renameable symbol and returns the current name range, which editors use to pre-populate the rename input field.

## Related Pages

- [[Navigation]] — find-references, which powers the reference collection step
- [[Connection Graph]] — the edge store queried by `Refs.findElementRefs`
