---
title: "Requirements — Workspace"
date: 2026-04-16
tags:
  - wiki/requirements
  - requirements/workspace
---

# Workspace Requirements

Requirements for how Marksman detects project roots, isolates multi-folder workspaces, and filters files by extension.

> [!NOTE] Scope
> "Workspace" here covers the folder and root-detection layer (BC4 in the domain model). Single-file mode restrictions on feature availability are governed by [[requirements/link-resolution#Tag: Link.Resolution.ModeScope|Link.Resolution.ModeScope]].

---

## Tag: Workspace.ProjectDetection.Root

**Gist:** A directory must be treated as a Marksman project root when it contains a `.marksman.toml` file; all `.md` files within that directory tree must be loaded into the workspace index.

**Ambition:** The `.marksman.toml` file is the explicit project-root signal. Without reliable detection, a sub-folder opened as the workspace root would fail to load sibling documents, breaking cross-file navigation. Authors depend on `.marksman.toml` placement to define the scope of their link graph.

**Scale:** Percentage of directories that contain `.marksman.toml` and are passed as workspace roots to the LSP server that result in a `Folder` being loaded with all of their `.md` files indexed, measured across a controlled set of workspace configurations.

**Meter:** Integration test: create a workspace with a root directory containing `.marksman.toml` and 5 `.md` files at various sub-folder depths. Start the server with that root URI. Invoke `workspace/symbol` with an empty query. Assert the response contains symbol entries from all 5 documents. Count loaded documents ÷ expected documents × 100.

**Fail:** Any directory containing `.marksman.toml` that is passed as a workspace root fails to load its `.md` files, or loads them incompletely.

**Goal:** 100 % of `.marksman.toml`-bearing roots passed to the server are fully loaded.

**Stakeholders:** Wiki authors, repository maintainers.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — workspace setup section; `Marksman/Folder.fs` — `tryLoad`; `Marksman/Server.fs` — `readWorkspace`.

**Open questions:**
- Is VCS root (`.git` presence) also a valid project root signal when no `.marksman.toml` is present? The feature docs suggest yes — a separate requirement (`Workspace.ProjectDetection.VCS`) should be authored once the VCS detection path is confirmed.

---

## Tag: Workspace.MultiFolder.Isolation

**Gist:** In a multi-folder workspace, a link in one folder must not resolve to a document in a different folder; cross-folder resolution must be suppressed.

**Ambition:** Each folder has its own link graph. Allowing cross-folder resolution would create implicit dependencies between independent projects sharing the same LSP session — a user who opens two unrelated vaults simultaneously would see completions and definitions bleed between them. Isolation preserves the folder boundary as the unit of the link namespace.

**Scale:** For a workspace with two folders A and B, the number of `textDocument/definition` or `textDocument/completion` responses that return a result from folder B when the request originates from a document in folder A, divided by total cross-folder requests × 100. Expected: 0 %.

**Meter:** Integration test: open two workspace folders `vault-a/` and `vault-b/`, each with a document named `shared-slug.md`. In `vault-a/doc.md`, place `[[shared-slug]]`. Invoke `textDocument/definition`. Assert the returned location points to `vault-a/shared-slug.md`, not `vault-b/shared-slug.md`. Invoke `textDocument/completion` at `[[`; assert no item from `vault-b/` appears.

**Fail:** Any definition result or completion item crosses a folder boundary.

**Goal:** 0 cross-folder resolution results in 100 % of requests.

**Stakeholders:** Users with multi-project workspaces, plugin developers.

**Owner:** Marksman contributors.

**Source:** `docs/features.md` — multi-folder section; `Marksman/Workspace.fs` — `tryFindFolderEnclosing` scoping; `Marksman/Refs.fs` — resolution confined to folder.

---

## Tag: Workspace.FileExtension.Filter

**Gist:** Only files whose extension (case-insensitive) appears in `core.markdown.file_extensions` (default `["md", "markdown"]`) must be loaded into the workspace index; files with other extensions must be silently excluded.

**Ambition:** Repositories often contain non-Markdown files (`.txt`, `.rst`, `.adoc`) alongside Markdown documents. Loading all text files would pollute completion lists with irrelevant items and waste memory on large repositories. The extension filter is the gating mechanism that keeps the index focused on documents Marksman can meaningfully serve.

**Scale:** Two sub-scales in a workspace with a known set of files:
1. **Inclusion rate:** percentage of files with a configured extension that appear in `workspace/symbol` results or `textDocument/completion` candidates. Expected: 100 %.
2. **Exclusion rate:** percentage of files with a non-configured extension that are absent from all index-derived results. Expected: 100 %.

**Meter:** Integration test: workspace root containing `a.md`, `b.markdown`, `c.txt`, `d.rst`. Default config (`core.markdown.file_extensions = ["md", "markdown"]`). Invoke `workspace/symbol` with empty query. Assert symbols from `a.md` and `b.markdown` are present. Assert no symbols from `c.txt` or `d.rst` are present.

Repeat with `core.markdown.file_extensions = ["md", "markdown", "txt"]`; assert `c.txt` now appears and `d.rst` still does not.

**Fail:** Inclusion rate < 100 % (a configured-extension file is missing) or Exclusion rate < 100 % (a non-configured file appears in results).

**Goal:** 100 % inclusion of configured extensions and 100 % exclusion of non-configured extensions.

**Stakeholders:** Repository maintainers, plugin developers.

**Owner:** Marksman contributors.

**Source:** `docs/configuration.md` — `core.markdown.file_extensions`; `Marksman/Config.fs` — default `["md"; "markdown"]`; `Marksman/Folder.fs` — extension filter.

---

## Related

- [[requirements/index|Requirements Index]] — master tag list
- [[requirements/configuration|Configuration Requirements]] — config that governs workspace behaviour
- [[requirements/link-resolution#Tag: Link.Resolution.ModeScope|Link.Resolution.ModeScope]] — single-file mode
- [[design/domain-layer|Domain Layer]] — BC4: Folder & Workspace Organisation
- [[concepts/workspace-model|Workspace Model]] — conceptual overview
- [[research/lsp/06-workspace-features|LSP 3.17 — Workspace Features]] — workspace folder and file-operation spec
