---
title: Completions
tags:
  - wiki/feature
---

# Completions

Marksman provides ==LSP completion== (textDocument/completion) for all link types found in Markdown. The completion engine lives in `Compl.fs` and operates by detecting what kind of link fragment sits at the cursor, then querying the current folder for matching candidates.

## Trigger Contexts

The completer first calls into the `Completable` discriminated union to classify the token at the cursor position:

| Completable type | Example trigger text | What is completed |
|------------------|---------------------|-------------------|
| `PartialWikiLink` | `[[pro` | Document titles or file stems |
| `PartialWikiLinkSection` | `[[doc#he` | Heading anchors within the target doc |
| `PartialMdLink` | `[label](./` | File paths relative to the current document |
| `PartialMdLinkSection` | `[label](doc.md#` | Heading anchors within the referenced file |
| `PartialRefLink` | `[label][ref` | Reference definition labels defined in the file |

> [!NOTE]
> The cursor must be inside a partial link token. Completions are not triggered in fenced code blocks, HTML comments, or YAML frontmatter.

## Candidate Generation

For wiki-link completions, candidates are fetched from the [[Workspace Model|folder]] index:

1. All `Doc` objects in the folder are enumerated.
2. Each doc contributes its canonical name (determined by `completion.wiki.style`) as a title candidate.
3. For section completions, the headings of the target document are fetched from its `Cst` (concrete syntax tree).

The total candidate list is capped at `completion.candidates` (default **50**) to keep response payloads small on large vaults. See [[Config Reference]] to raise or lower this limit.

## Wiki Style Variants

The `completion.wiki.style` setting (in `.marksman.toml` or the user config) directly changes what text is inserted by a wiki-link completion:

| Style | Inserted text | Best for |
|-------|--------------|----------|
| `title-slug` *(default)* | H1 heading slug | Zettelkasten vaults where titles are canonical |
| `file-stem` | Bare filename stem | Wikis where filenames are stable identifiers |
| `file-path-stem` | Path-qualified stem | Multi-folder workspaces with name collisions |

See [[Wiki Links]] for how the same setting affects resolution at runtime.

## Completion Item Detail

Each completion item includes:

- **label** — the link text that will be inserted.
- **detail** — the relative file path of the target document.
- **documentation** — a Markdown snippet showing the first paragraph of the target doc (hover preview content reused).
- **textEdit** — a range-replacing edit that replaces the partial token including the opening `[[` or `(`.

> [!TIP]
> If completions feel slow on a very large vault, reducing `completion.candidates` or switching to `file-stem` style (which skips heading parsing) can improve responsiveness.

> [!WARNING]
> Completion candidates are scoped to the current **folder**. Documents in other folders within a multi-folder workspace are not suggested unless the folder shares the same root. See [[Workspace Model]] for how folders are delimited.

## Related Pages

- [[Wiki Links]] — syntax and resolution rules for wiki links
- [[Config Reference]] — `completion.wiki.style`, `completion.candidates`
- [[Workspace Model]] — how folders and documents are indexed
- [[research/lsp/05-language-features|LSP 3.17 — Language Features]] — `textDocument/completion` protocol specification
