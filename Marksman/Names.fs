/// Typed string wrappers for URL-encoded and wiki-encoded names, plus identifier types for
/// folders, documents, and internal cross-document references.
module Marksman.Names

open System
open Marksman.Misc
open Marksman.Paths

/// A string value that has been URL-percent-encoded.
type UrlEncoded = UrlEncoded of string

module UrlEncoded =
    let mkUnchecked = UrlEncoded
    let encode (str: string) = UrlEncoded(str.UrlEncode())

    let decode (UrlEncoded str) = str.UrlDecode()
    let raw (UrlEncoded str) = str

/// A string value encoded in wiki-link form (spaces replaced with URL-safe characters).
type WikiEncoded = WikiEncoded of string

module WikiEncoded =
    let mkUnchecked = WikiEncoded
    let encode (str: string) = WikiEncoded(str.EncodeForWiki())
    let decode (WikiEncoded str) = str.UrlDecode()
    let raw (WikiEncoded raw) = raw
    let encodeAsString (str: string) : string = encode str |> raw

/// Uniquely identifies a workspace root folder by its URI paired with a typed <c>RootPath</c>.
type FolderId = UriWith<RootPath>

module FolderId =
    let ofUri uri = UriWith.mkRoot uri

/// Uniquely identifies a document within a workspace root.
///
/// Equality and comparison are based solely on the document URI string so that two <c>DocId</c>
/// values for the same file are always considered equal regardless of how the path was constructed.
[<Struct>]
[<StructuredFormatDisplay("{ShortFormat}")>]
[<CustomEquality; CustomComparison>]
type DocId =
    | DocId of UriWith<RootedRelPath>

    member this.Raw =
        let (DocId uri) = this
        uri

    member this.Path =
        let (DocId uri) = this
        uri.data

    member this.RelPathForced = this.Path |> RootedRelPath.relPathForced

    member this.Uri =
        let (DocId uri) = this
        uri.uri

    member this.ShortFormat = this.RelPathForced |> RelPath.toSystem

    interface IEquatable<DocId> with
        member this.Equals(that: DocId) =
            let (DocId thisUri) = this
            let (DocId thatUri) = that
            thisUri.uri = thatUri.uri

    override this.Equals(that) =
        match that with
        | :? DocId as that -> (this :> IEquatable<_>).Equals(that)
        | _ -> false

    override this.GetHashCode() =
        let (DocId uri) = this
        hash uri.uri

    interface IComparable<DocId> with
        member this.CompareTo(that: DocId) =
            let (DocId thisUri) = this
            let (DocId thatUri) = that
            compare thisUri.uri thatUri.uri

    interface IComparable with
        member this.CompareTo(that: obj) =
            match that with
            | :? DocId as that -> (this :> IComparable<_>).CompareTo(that)
            | _ -> failwith $"Can't compare DocId with other types: {that}"


/// A raw internal-reference name together with the document it was found in.
type InternName = { src: DocId; name: string }

/// The resolved form of an internal reference path.
///
/// <c>ExactAbs</c> – the reference used an absolute-rooted path (leading <c>/</c>).
/// <c>ExactRel</c> – the reference used a relative path with <c>.</c>/<c>..</c> components,
///   resolved against the source document's directory.
/// <c>Approx</c>   – an unanchored name that must be matched by filename anywhere in the workspace.
type InternPath =
    | ExactAbs of RootedRelPath
    | ExactRel of src: DocId * path: RootedRelPath
    | Approx of RelPath

module InternPath =
    let toRel =
        function
        | ExactAbs rooted
        | ExactRel(_, rooted) -> RootedRelPath.relPathForced rooted
        | Approx path -> path

module InternName =
    let mkUnchecked src name = { src = src; name = name }

    /// Returns <c>Some</c> only when <paramref name="name"/> looks like a potential internal
    /// reference for the given file extensions; rejects absolute URIs and other non-internal forms.
    let mkChecked exts src name =
        if isPotentiallyInternalRef exts name then
            Some({ src = src; name = name })
        else
            None

    let name { name = name } = name

    let slug { name = name } = Slug.ofString name

    let src { src = src } = src

    /// Tries to interpret the name string as a file path, classifying it as <c>ExactAbs</c>,
    /// <c>ExactRel</c>, or <c>Approx</c>. Returns <c>None</c> for absolute URIs and on parse
    /// errors.
    let tryAsPath ({ src = src; name = name }: InternName) =
        if Uri.IsWellFormedUriString(name, UriKind.Absolute) then
            None
        else if name.StartsWith('/') then
            let relPathOpt =
                LocalPath.tryOfSystem (
                    UrlEncoded.mkUnchecked (name.TrimStart('/')) |> UrlEncoded.decode
                )

            relPathOpt
            |> Option.map (fun relPath ->
                let rootPath = RootedRelPath.rootPath src.Path
                let namePath = RootedRelPath.mk rootPath relPath
                ExactAbs namePath)
        else
            try
                let rawNamePath =
                    LocalPath.ofSystem (UrlEncoded.mkUnchecked name |> UrlEncoded.decode)

                if LocalPath.hasDotComponents rawNamePath then
                    RootedRelPath.directory src.Path
                    |> Option.bind (fun dir -> RootedRelPath.combine dir rawNamePath)
                    |> Option.map (fun x -> ExactRel(src, x))
                else
                    match rawNamePath with
                    | Abs _ -> None
                    | Rel path -> Some(Approx path)

            with
            | :? UriFormatException
            | :? InvalidOperationException -> None

    let asPath name =
        tryAsPath name
        |> Option.defaultWith (fun () -> failwith $"Can't convert InternName {name} to a path")
