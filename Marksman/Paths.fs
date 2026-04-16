/// Path abstraction layer providing typed wrappers for absolute, relative, rooted, and URI-based
/// file-system paths, along with conversion utilities between system paths and LSP document URIs.
module Marksman.Paths

open System
open System.IO
open System.Runtime.InteropServices
open System.Text

open Marksman.Misc

open Ionide.LanguageServerProtocol.Types

// https://github.dev/fsharp/FsAutoComplete/blob/d90597c2e073b7e88390f2b1933b031ff8a9a009/src/FsAutoComplete.Core/Utils.fs#L635
/// Converts a system file path to a <c>file://</c> URI string, percent-encoding special characters
/// and normalising Windows backslashes to forward slashes.
let systemPathToUriString (filePath: string) : string =
    let uri = StringBuilder(filePath.Length)

    for c in filePath do
        if
            (c >= 'a' && c <= 'z')
            || (c >= 'A' && c <= 'Z')
            || (c >= '0' && c <= '9')
            || c = '+'
            || c = '/'
            || c = '.'
            || c = '-'
            || c = '_'
            || c = '~'
            || c > '\xFF'
        then
            uri.Append(c) |> ignore
        // handle windows path separator chars.
        // we _would_ use Path.DirectorySeparator/AltDirectorySeparator, but those vary per-platform and we want this
        // logic to work cross-platform (for tests)
        else if c = '\\' then
            uri.Append('/') |> ignore
        else
            // It's important to have UTF8 encoding here as otherwise paths with accented characters
            // won't work. Url-encoding uses UTF8 by default.
            uri.Append(Uri.EscapeDataString(c.ToString())) |> ignore

    if uri.Length >= 2 && uri[0] = '/' && uri[1] = '/' then // UNC path
        "file:" + uri.ToString()
    else
        "file:///" + uri.ToString().TrimStart('/')

/// Converts a <c>file://</c> URI to a local system path, normalising the Windows drive letter to
/// lower-case.
let uriToSystemPath (uri: DocumentUri) : string =
    let unescaped = Uri.UnescapeDataString(uri)
    let uri = Uri(unescaped)
    let localPath = uri.LocalPath

    let isWin =
        localPath.Length >= 2
        && Char.IsLetter(localPath[0])
        && localPath[1] = ':'

    let localPath =
        if isWin then
            Char.ToLower(localPath[0]).ToString() + localPath[1..]
        else
            localPath

    localPath

/// Splits a path string on both forward and backward slashes, discarding empty segments.
let pathComponents (path: string) =
    let components = path.Split([| '\\'; '/' |])
    Array.filter (String.IsNullOrEmpty >> not) components

/// Host operating-system platform discriminator.
type Platform =
    | Unix
    | Win

/// Directory separator style.
type DirSeparator =
    | Forward
    | Backward

module DirSeparator =
    let sepChar =
        function
        | Forward -> '/'
        | Backward -> '\\'

    let sepString =
        function
        | Forward -> "/"
        | Backward -> "\\"

    let platformSep =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            Backward
        else
            Forward

/// An absolute file-system path (starts with a drive letter on Windows or <c>/</c> on Unix).
[<Struct>]
type AbsPath =
    | AbsPath of string

    member this.GetDirectoryName() : AbsPath =
        let (AbsPath raw) = this
        AbsPath(Path.GetDirectoryName(raw))

/// A relative file-system path (no leading root component).
and [<Struct>] RelPath =
    | RelPath of string

    member this.GetDirectoryName() : AbsPath =
        let (RelPath raw) = this
        AbsPath(Path.GetDirectoryName(raw))

/// A file-system path that is either absolute or relative.
and LocalPath =
    | Abs of AbsPath
    | Rel of RelPath

    member this.Raw: string =
        match this with
        | Abs(AbsPath str)
        | Rel(RelPath str) -> str

module AbsPath =
    /// Returns <c>Some</c> if <paramref name="str"/> looks like a Windows absolute path (e.g. <c>c:\…</c>).
    let isRawWinAbsPath str = String.length str >= 2 && Char.IsLetter(str[0]) && str[1] = ':'

    /// Returns <c>Some</c> if <paramref name="str"/> looks like a Unix absolute path (starts with <c>/</c>).
    let isRawUnixAbsPath str = String.length str > 0 && str[0] = '/'

    /// Returns true if <paramref name="str"/> is a filesystem root component (<c>/</c> or a Windows drive root such as <c>c:</c>).
    let isRootComponent str =
        str = "/"
        || (String.length str = 2 && Char.IsLetter(str[0]) && str[1] = ':')

    /// Tries to parse a raw string as an absolute path; returns <c>None</c> if it is not absolute.
    let tryOfSystem str =
        if isRawWinAbsPath str then
            // TODO: normalize the drive letter? Leave as-is for now until the requirements are
            // clearer
            AbsPath str |> Some
        else if isRawUnixAbsPath str then
            AbsPath str |> Some
        else
            None

    /// Parses a raw string as an absolute path; throws if the string is not absolute.
    let ofSystem str =
        tryOfSystem str
        |> Option.defaultWith (fun () -> failwith $"Bad absolute path: {str}")

    /// Extracts the absolute path from a <c>file://</c> URI.
    let ofUri (rawUri: DocumentUri) : AbsPath = ofSystem (uriToSystemPath rawUri)

    let toSystem (AbsPath raw) = raw

    let toUri path = systemPathToUriString (toSystem path)

    /// Appends a bare filename (no directory component) to an absolute path.
    let appendFile (AbsPath raw) (filename: string) = AbsPath(Path.Combine(raw, filename))

    /// Appends a relative path to an absolute path.
    let append (AbsPath abs) (RelPath rel) = AbsPath(Path.Combine(abs, rel))

    /// Resolves <c>.</c> and <c>..</c> components via <c>Path.GetFullPath</c>.
    let resolve (AbsPath raw) = Path.GetFullPath(raw) |> AbsPath

    /// Returns true if <paramref name="inner"/> starts with <paramref name="outer"/> (i.e. is a descendant).
    let contains (AbsPath outer) (AbsPath inner) = inner.StartsWith(outer)

    let filename (AbsPath sys) = Path.GetFileName(sys)
    let filenameStem (AbsPath sys) = Path.GetFileNameWithoutExtension(sys)

module RelPath =
    let ofStringUnchecked str = RelPath str
    let toSystem (RelPath p) = p
    let filename (RelPath p) = Path.GetFileName(p)
    let filenameStem (RelPath p) = Path.GetFileNameWithoutExtension(p)

    let directory (RelPath p) = Path.GetDirectoryName(p) |> RelPath

    let hasExtension (RelPath p) = Path.GetExtension(p).IsEmpty() |> not


module LocalPath =
    /// Tries to parse a string as an absolute or relative path; returns <c>None</c> for null/empty input.
    let tryOfSystem str =
        if String.IsNullOrEmpty str then
            None
        else
            match AbsPath.tryOfSystem str with
            | Some path -> Some(Abs path)
            | None -> Some(Rel(RelPath.ofStringUnchecked str))

    /// Parses a string as an absolute or relative path; throws on null/empty input.
    let ofSystem str =
        tryOfSystem str
        |> Option.defaultWith (fun () -> failwith $"String {str} couldn't be converted to a path")

    let toSystem (path: LocalPath) = path.Raw

    let isAbsolute =
        function
        | Abs _ -> true
        | Rel _ -> false

    /// Unwraps to <c>AbsPath</c>; throws if the path is relative.
    let asAbsolute =
        function
        | Abs path -> path
        | Rel path -> failwith $"Path {path} is not absolute"

    let isRelative =
        function
        | Abs _ -> false
        | Rel _ -> true

    let components path = toSystem path |> pathComponents

    /// Returns true if any path component is <c>.</c> or <c>..</c>.
    let hasDotComponents path = components path |> Array.exists (fun x -> x = "." || x = "..")

    /// Reconstructs a <c>LocalPath</c> from an array of path component strings.
    let ofComponents comps =
        assert (Array.length comps > 0)

        let sysPath =
            if comps[0] = "/" then
                "/" + String.Join(Path.DirectorySeparatorChar, comps[1..])
            else
                String.Join(Path.DirectorySeparatorChar, comps)

        ofSystem sysPath

    // TODO: this is pretty ridiculous. Think of a better way.
    /// Returns the directory separator character (<c>/</c> or <c>\</c>) that appears most often in the path.
    let dominatingDirectorySeparator path =
        let raw = toSystem path

        let numForward, numBackward =
            raw.ToCharArray()
            |> Array.fold
                (fun (numForward, numBackward) c ->
                    if c = '/' then (numForward + 1, numBackward)
                    else if c = '\\' then (numForward, numBackward + 1)
                    else (numForward, numBackward))
                (0, 0)

        if numForward >= numBackward then '/' else '\\'

    let startsWithString (str: string) path = (toSystem path).StartsWith(str)

    /// Resolves <c>.</c> and <c>..</c> components without hitting the filesystem, preserving the
    /// original separator style.
    let normalize path =
        let comps = components path |> Array.toList
        let isAbs = isAbsolute path

        let normalizer (acc: list<string>) (comp: string) =
            match acc, comp with
            | [], ".." when isAbs -> []
            | [], "." -> []
            | [], comp -> [ comp ]
            | acc, "." -> acc
            | [ accHd ], ".." when AbsPath.isRootComponent accHd -> acc
            | ".." :: _, ".." -> ".." :: acc
            | _ :: accTl, ".." -> accTl
            | _, _ -> comp :: acc

        let comps = List.fold normalizer [] comps |> List.rev
        let sep = (dominatingDirectorySeparator path).ToString()
        let newPath = String.Join(sep, comps)
        let newPath = if startsWithString "/" path then "/" + newPath else newPath

        tryOfSystem newPath
        |> Option.defaultWith (fun () -> failwith "Path normalization failed")

    let ofUri (rawUri: DocumentUri) : LocalPath = ofSystem (uriToSystemPath rawUri)

    let toUri path = systemPathToUriString (toSystem path)

    let appendFile path filename =
        let sysPath = toSystem path
        let extended = Path.Combine(sysPath, filename)

        match path with
        | Abs _ -> Abs(AbsPath extended)
        | Rel _ -> Rel(RelPath extended)

    /// Combines two paths: if <paramref name="p2"/> is absolute it replaces <paramref name="p1"/>;
    /// otherwise the two paths are joined and the result kind follows <paramref name="p1"/>.
    let combine p1 p2 =
        match p1, p2 with
        | _, Abs _ -> p2
        | _, Rel _ ->
            let sys1 = toSystem p1
            let sys2 = toSystem p2
            let sysComb = Path.Combine(sys1, sys2)

            match p1 with
            | Rel _ -> Rel(RelPath sysComb)
            | Abs _ -> Abs(AbsPath sysComb)

    let filename path = Path.GetFileName(toSystem path)

    let filenameStem path = Path.GetFileNameWithoutExtension(toSystem path)

    let directory path =
        let dir = Path.GetDirectoryName(toSystem path)
        ofSystem dir

/// The absolute path of a workspace root folder.
[<Struct>]
type RootPath =
    | RootPath of AbsPath

    member this.Path =
        let (RootPath path) = this
        path

module RootPath =
    let ofPath path = RootPath path
    let toLocal (RootPath p) = Abs p
    let toAbs (RootPath p) = p
    let toSystem (RootPath p) = AbsPath.toSystem p
    let toUri (RootPath p) = AbsPath.toUri p
    let resolve (RootPath p) = AbsPath.resolve p
    let append (RootPath p) relPath = AbsPath.append p relPath
    let appendFile (RootPath p) filename = AbsPath.appendFile p filename

    /// Returns true if <paramref name="path"/> is relative (always within a root) or if its
    /// resolved absolute form starts with the resolved root path.
    let contains (RootPath enclosing) (path: LocalPath) =
        match path with
        | Rel _ -> true
        | Abs enclosed ->
            let (AbsPath enclosing) = AbsPath.resolve enclosing
            let (AbsPath enclosed) = AbsPath.resolve enclosed
            enclosed.StartsWith(enclosing)

    let filename (RootPath p) = AbsPath.filename p
    let filenameStem (RootPath p) = AbsPath.filenameStem p

/// A path within a workspace root: a <c>RootPath</c> plus an optional relative path from that
/// root. When <c>path</c> is <c>None</c> the record refers to the root itself.
[<Struct>]
type RootedRelPath = { root: RootPath; path: option<RelPath> }

module RootedRelPath =
    /// Constructs a <c>RootedRelPath</c> from a root and a <c>LocalPath</c>, computing the
    /// relative portion for absolute paths by calling <c>Path.GetRelativePath</c>.
    let mk root path =
        match path with
        | Rel path -> { root = root; path = Some path }
        | Abs path ->
            let (AbsPath sysRoot) = RootPath.resolve root
            let (AbsPath sysOther) = AbsPath.resolve path

            if sysRoot = sysOther then
                { root = root; path = None }
            else
                let relPath = Path.GetRelativePath(sysRoot, sysOther)
                { root = root; path = Some(RelPath relPath) }

    let toAbs rooted =
        match rooted.path with
        | None -> RootPath.toAbs rooted.root
        | Some path -> RootPath.append rooted.root path

    let toLocal path = Abs(toAbs path)
    let toSystem path = toAbs path |> AbsPath.toSystem
    let toUri path = LocalPath.toUri (toLocal path)
    let filename path = Path.GetFileName(toSystem path)
    let filenameStem path = Path.GetFileNameWithoutExtension(toSystem path)

    let directory rooted =
        let mkDir relPath =
            let dir = relPath |> RelPath.directory
            { root = rooted.root; path = Some dir }

        rooted.path |> Option.map mkDir

    /// Combines a rooted path with another local path, returning <c>None</c> if the result
    /// escapes the root or resolves to a relative path.
    let combine (root: RootedRelPath) (path: LocalPath) : option<RootedRelPath> =
        match LocalPath.combine (toLocal root) path with
        | Rel _ -> None
        | Abs out ->
            if RootPath.contains root.root (Abs out) then
                mk root.root (Abs out) |> Some
            else
                None

    let rootPath { root = root } = root
    let relPathForced { path = path } = path |> Option.defaultValue (RelPath ".")

/// Pairs an LSP <c>DocumentUri</c> with a typed, pre-parsed path value so that callers can use
/// either the raw URI string or the structured path without re-parsing.
type UriWith<'T> = { uri: DocumentUri; data: 'T }

module UriWith =
    let mkAbs uri = { uri = uri; data = AbsPath.ofUri uri }
    let mkRoot uri = { uri = uri; data = AbsPath.ofUri uri |> RootPath }

    /// Builds a <c>UriWith&lt;RootedRelPath&gt;</c> from a rooted-URI root and a local path,
    /// re-deriving the URI from the resulting rooted relative path.
    let mkRooted root path =
        let relPath = RootedRelPath.mk root.data path
        { uri = RootedRelPath.toUri relPath; data = relPath }

    /// Converts a <c>UriWith&lt;RootedRelPath&gt;</c> to a <c>UriWith&lt;AbsPath&gt;</c>,
    /// keeping the original URI string.
    let rootedRelToAbs uri =
        let localPath = RootedRelPath.toAbs uri.data
        { uri = uri.uri; data = localPath }

/// A canonical document path: the relative path to a Markdown document with its extension
/// stripped, used as a stable key for cross-document references.
type CanonDocPath = private CanonDocPath of string

module CanonDocPath =
    let mk mdExts (RelPath relPath) = CanonDocPath(chopMarkdownExt mdExts relPath)

    let components (CanonDocPath canonPath) = pathComponents canonPath |> List.ofArray

    let toRel (CanonDocPath canonPath) = RelPath canonPath
