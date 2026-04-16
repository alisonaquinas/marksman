---
title: Path Model
aliases:
  - Paths
  - DocId
  - RootedRelPath
  - CanonDocPath
tags:
  - wiki/concept
related:
  - "[[Workspace Model]]"
  - "[[Symbol Model]]"
---

# Path Model

> [!ABSTRACT]
> The path model is Marksman's typed wrapper layer over raw file-system strings. It prevents accidental mixing of absolute paths, relative paths, URI strings, and folder-relative paths by encoding each kind as a distinct F# struct or record type. The top of the hierarchy — `DocId` — is the unique identity of a document across the entire workspace.

---

## The Type Hierarchy

```
AbsPath   RelPath
   \         /
    LocalPath
       |
    RootPath  (always AbsPath, marks a workspace root)
       |
  RootedRelPath  { root: RootPath; path: option<RelPath> }
       |
  UriWith<RootedRelPath>
       |
     DocId
```

All types are defined in `Marksman/Paths.fs`.

---

## Primitive Path Types

==`AbsPath`== wraps an absolute system path string. On Windows this starts with a lower-cased drive letter (`c:\...`); on Unix it starts with `/`. The struct wrapper ensures an `AbsPath` is never passed where a `RelPath` is expected.

==`RelPath`== wraps a path string with no leading root component. It may contain subdirectory segments (`notes/daily/today.md`).

==`LocalPath`== is a discriminated union `Abs of AbsPath | Rel of RelPath`, used in contexts that can accept either form — for example, when processing a link target that may be a relative reference from the current document or an absolute vault path.

> [!NOTE]
> `LocalPath.normalize` resolves `.` and `..` components without touching the file system and preserves the original separator style (forward vs backward), making it safe to call in tests on any platform.

---

## RootPath

==`RootPath`== wraps an `AbsPath` that has been designated as a workspace folder root. Project roots are detected by the presence of a `.marksman.toml` file. The distinction from a plain `AbsPath` exists to make the containment check `RootPath.contains` self-documenting: it tests whether an inner path starts with the resolved root path, modulo symlink resolution via `Path.GetFullPath`.

---

## RootedRelPath

==`RootedRelPath`== is the key doc-identity type within a folder:

```fsharp
type RootedRelPath = { root: RootPath; path: option<RelPath> }
```

The `path` field is `None` only when the document *is* the root itself (an unusual edge case). For every normal document, `path` holds the relative path from the folder root to the file (`notes/daily/today.md`).

`RootedRelPath.mk root localPath` computes the relative portion by calling `Path.GetRelativePath(root, abs)`, ensuring that two different absolute paths resolving to the same file produce the same `RootedRelPath`.

`RootedRelPath` exists because a folder-relative path is sufficient to identify a document within its folder, and keeping the `root` attached means the absolute path can always be recovered without threading a separate root argument through every call site.

---

## UriWith and DocId

==`UriWith<'T>`== pairs an LSP `DocumentUri` string with a pre-parsed typed path value:

```fsharp
type UriWith<'T> = { uri: DocumentUri; data: 'T }
```

This avoids re-parsing the URI on every operation while keeping the original URI string available for LSP responses.

==`DocId`== is defined as `DocId of UriWith<RootedRelPath>`. It is the globally unique identity of a document: equality is defined on the full `RootedRelPath` (root + relative path), so two documents at the same path in different folders are distinct.

---

## CanonDocPath

==`CanonDocPath`== is the key used in `Folder`'s `docs: Map<CanonDocPath, Doc>`:

```fsharp
type CanonDocPath = private CanonDocPath of string
```

It is constructed from a `RelPath` by stripping the Markdown file extension (`.md`, `.markdown`, or any extension listed in `Config.CoreMarkdownFileExtensions`). Stripping the extension means a wiki-link `[[my-note]]` can match `my-note.md`, `my-note.markdown`, or any configured extension without special-casing in the resolver.

> [!WARNING]
> `CanonDocPath` does **not** normalise case on its own. Case-insensitive matching depends on the host file system or explicit `String.OrdinalIgnoreCase` comparisons at lookup sites. On case-sensitive Linux file systems, `[[My-Note]]` and `[[my-note]]` can be distinct documents.

---

## URI Conversion

The two free functions `systemPathToUriString` and `uriToSystemPath` handle the boundary between LSP `DocumentUri` strings and system paths:

- `systemPathToUriString` percent-encodes non-URL-safe characters, converts Windows backslashes to forward slashes, and prepends `file:///`.
- `uriToSystemPath` unescapes percent-encoding, constructs a `System.Uri`, extracts `LocalPath`, and lower-cases the Windows drive letter so that `C:\` and `c:\` produce the same `AbsPath`.

> [!WARNING]
> Windows drive letters are normalised to lower-case during URI→path conversion. If an `AbsPath` is constructed directly from a Win32 API call that returns an upper-case drive letter (`C:\Users\...`) and then compared against a URI-derived path (`c:\Users\...`), the comparison will fail. Always obtain paths through `AbsPath.ofUri` or `AbsPath.ofSystem` rather than constructing them manually.

---

## Platform Handling

`Platform` (Unix | Win) and `DirSeparator` (Forward | Backward) types, along with `DirSeparator.platformSep`, are used in path construction utilities. However, the URI conversion functions deliberately handle both backslash and forward-slash regardless of `RuntimeInformation.IsOSPlatform`, so that cross-platform test fixtures with Unix-style paths work correctly on Windows.

---

## See Also

- [[Workspace Model]] — `DocId` and `CanonDocPath` are used as keys in `Folder`
- [[Symbol Model]] — `Scope.Doc` carries a `DocId` to scope symbols to their document
