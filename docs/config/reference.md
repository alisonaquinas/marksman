---
title: Config Reference
tags:
  - wiki/config
---

# Config Reference

Marksman is configured via TOML files. All settings have sensible defaults; you only need to override what you want to change. See the precedence rules below for how multiple config files interact.

## Config File Locations

| Scope | Platform | Path |
|-------|----------|------|
| Project | All | `<project-root>/.marksman.toml` |
| User | Linux / macOS | `$XDG_CONFIG_HOME/marksman/config.toml` (falls back to `~/.config/marksman/config.toml`) |
| User | Windows | `%APPDATA%\marksman\config.toml` |

The ==project root== is detected by the presence of `.marksman.toml` in a directory. Without this file, Marksman operates in single-file mode and applies only the user config.

## Precedence Order

```
project .marksman.toml  >  user config.toml  >  built-in defaults
```

Settings in the project config override the user config for that key. Unset keys fall through to the next level.

## All Config Knobs

### `core` Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `core.markdown.file_extensions` | string list | `["md","markdown"]` | File extensions Marksman treats as Markdown documents |
| `core.markdown.glfm_heading_ids.enable` | bool | `true` | Use GitHub Labeled Flavored Markdown rules for heading anchor slugs |
| `core.text_sync` | string | `"full"` | LSP text sync mode: `"full"` sends the entire document on each change; `"incremental"` sends only the changed ranges |
| `core.title_from_heading` | bool | `true` | Derive a document's canonical title from its first H1 heading |
| `core.incremental_references` | bool | `false` | Experimental: update only directly affected connection graph edges on change (faster for large vaults) |
| `core.paranoid` | bool | `false` | Enable extra internal consistency assertions; useful when debugging Marksman itself |

### `completion` Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `completion.wiki.style` | string | `"title-slug"` | Determines what text is inserted for wiki-link completions: `"title-slug"`, `"file-stem"`, or `"file-path-stem"` |
| `completion.candidates` | int | `50` | Maximum number of completion items returned per request |

See [[Completions]] and [[Wiki Links]] for how these settings affect behaviour at runtime.

### `code_action` Section

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `code_action.toc.enable` | bool | `true` | Offer the "Insert/Update Table of Contents" code action |
| `code_action.toc.include` | int list | `[1,2,3,4,5,6]` | Heading levels to include in the generated TOC |
| `code_action.create_missing_file.enable` | bool | `true` | Offer a "Create missing file" code action when a wiki link targets a non-existent document |

See [[Table of Contents]] for full details on the TOC code action.

## Minimal Example `.marksman.toml`

```toml
[completion.wiki]
style = "file-stem"

[completion]
candidates = 100

[code_action.toc]
include = [2, 3]
```

> [!TIP]
> You can create a project config with just the keys you want to override. Missing keys are inherited from the user config or defaults — you do not need to repeat every setting.

> [!NOTE]
> The `.marksman.toml` file serves double duty: its presence marks the project root **and** carries per-project settings. An empty `.marksman.toml` is sufficient to establish a project root while using all default settings.

## Related Pages

- [[Wiki Links]] — `completion.wiki.style`, `core.title_from_heading`, GLFM IDs
- [[Completions]] — `completion.wiki.style`, `completion.candidates`
- [[Table of Contents]] — `code_action.toc.enable`, `code_action.toc.include`
- [[Build and Test]] — how to build and run Marksman from source
