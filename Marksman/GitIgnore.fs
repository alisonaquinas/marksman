/// Gitignore-style pattern matching used to exclude files and directories during workspace
/// scanning. Supports the standard <c>.gitignore</c> pattern syntax via the GlobExpressions
/// library.
module Marksman.GitIgnore

open System
open System.IO
open GlobExpressions
open Ionide.LanguageServerProtocol.Logging

/// A compiled glob that either re-includes (<c>!</c>-prefixed) or excludes a path.
type GlobPattern =
    | Include of Glob
    | Exclude of Glob

let private logger = LogProvider.getLoggerByName "GitIgnore"

/// Converts a single gitignore pattern line to one or two compiled <c>Glob</c> objects.
///
/// Directory patterns (trailing <c>/</c>) produce two globs: one matching the directory entry
/// itself and one matching all of its descendants. Returns an empty array for blank lines or
/// unsupported patterns.
let patternToGlob (pat: string) : array<Glob> =
    if String.IsNullOrWhiteSpace(pat) then
        [||]
    else
        let firstSlashIdx = pat.IndexOf('/')
        let isAbsolute = firstSlashIdx <> pat.Length - 1
        let isDir = pat[pat.Length - 1] = '/'
        let pat = if pat.StartsWith("/") then pat.Substring(1) else pat
        let pat = if isAbsolute then pat else "**/" + pat

        let opts = GlobOptions.Compiled

        try
            if isDir then
                [|
                    Glob(pat + "**", opts)
                    Glob(pat.Substring(0, pat.Length - 1), opts)
                |]
            else
                [| Glob(pat, opts) |]
        with :? GlobPatternException ->
            logger.warn (Log.setMessage "Unsupported glob pattern" >> Log.addContext "pat" pat)
            [||]

/// Parses a gitignore line into typed <c>GlobPattern</c> values.
///
/// Lines starting with <c>#</c> are comments and return an empty array.
/// Lines starting with <c>!</c> are negation patterns and produce <c>Include</c> entries.
/// All other lines produce <c>Exclude</c> entries.
let mkGlobPattern (pat: string) : array<GlobPattern> =
    if pat.StartsWith("#") then
        [||]
    else if pat.StartsWith("!") then
        let pat = pat.Substring(1)
        patternToGlob pat |> Array.map Include
    else
        patternToGlob pat |> Array.map Exclude

/// A set of compiled gitignore patterns anchored to a specific root directory.
type GlobMatcher = { root: string; patterns: array<GlobPattern> }

module GlobMatcher =

    /// Builds a <c>GlobMatcher</c> from a root directory and an array of raw gitignore lines.
    let mk (root: string) (lines: array<string>) : GlobMatcher =
        let patterns = lines |> Array.collect mkGlobPattern

        { root = root; patterns = patterns }

    /// Creates a matcher that ignores the standard VCS metadata directories (<c>.git</c>, <c>.hg</c>).
    let mkDefault (root: string) : GlobMatcher = mk root [| ".git"; ".hg" |]

    /// Returns true if <paramref name="path"/> should be ignored according to the matcher's
    /// patterns, evaluating them in order and honouring negation (<c>Include</c>) entries.
    let ignores (matcher: GlobMatcher) (path: string) : bool =
        let relPath = Path.GetRelativePath(matcher.root, path)

        let checkGlob g =
            match g with
            | Include glob -> if glob.IsMatch(relPath) then Some false else None
            | Exclude glob -> if glob.IsMatch(relPath) then Some true else None


        match matcher.patterns |> Seq.map checkGlob |> Seq.tryFind Option.isSome with
        | None -> false
        | Some(Some r) -> r
        | Some None -> failwith "Unreachable: GlobMatcher.ignores"

    /// Returns true if any matcher in <paramref name="matchers"/> ignores <paramref name="path"/>.
    let ignoresAny (matchers: seq<GlobMatcher>) (path: string) : bool =
        Seq.exists (fun m -> ignores m path) matchers
