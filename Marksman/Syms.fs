/// Core symbol type hierarchy used throughout Marksman for reference resolution and navigation.
module Marksman.Syms

open Marksman.Misc
open Marksman.Names
open Marksman.Paths


/// A reference that targets a destination within the same document.
[<StructuredFormatDisplay("{AsString}")>]
type IntraRef =
    /// Reference to a heading section within the same document, identified by its slug.
    | IntraSection of section: Slug
    /// Reference to a link definition within the same document, identified by its label.
    | IntraLinkDef of LinkLabel

    override this.ToString() =
        match this with
        | IntraSection section -> $"[[#{Slug.toString section}]]"
        | IntraLinkDef linkLabel -> $"[{linkLabel}]"

    member this.AsString = this.ToString()

/// A reference that targets a destination in a different document.
[<StructuredFormatDisplay("{AsString}")>]
type CrossRef =
    /// Reference to another document, identified by its name or path string.
    | CrossDoc of doc: string
    /// Reference to a heading section in another document.
    | CrossSection of doc: string * section: Slug

    override this.ToString() =
        match this with
        | CrossDoc doc -> $"[[{doc}]]"
        | CrossSection(doc, section) -> $"[[{doc}#{Slug.toString section}]]"

    member this.AsString = this.ToString()

    member this.Doc =
        match this with
        | CrossDoc doc
        | CrossSection(doc, _) -> doc

/// A link reference — either within the same document or across documents.
[<StructuredFormatDisplay("{AsString}")>]
type Ref =
    /// A same-document reference.
    | IntraRef of IntraRef
    /// A cross-document reference.
    | CrossRef of CrossRef

    override this.ToString() =
        match this with
        | IntraRef intraRef -> intraRef.ToString()
        | CrossRef crossRef -> crossRef.ToString()

    member this.AsString = this.ToString()

module Ref =
    let isIntra =
        function
        | IntraRef _ -> true
        | _ -> false

    let isCross =
        function
        | CrossRef _ -> true
        | _ -> false

    let trySection =
        function
        | CrossRef(CrossSection(_, section))
        | IntraRef(IntraSection section) -> Some section
        | _ -> None

/// A Zettelkasten-style tag, e.g. <c>#mytag</c>.
[<Struct>]
[<StructuredFormatDisplay("{AsString}")>]
type Tag =
    | Tag of string


    member this.AsString = this.ToString()

    member this.Name =
        let (Tag name) = this
        name

    override this.ToString() = $"#{this.Name}"

/// A symbol definition — something that can be targeted by a reference.
[<StructuredFormatDisplay("{AsString}")>]
type Def =
    /// The document itself as an implicit definition target.
    | Doc
    /// The document's title heading (level-1 heading treated as title).
    | Title of string
    /// A non-title heading, identified by its level and slug id.
    | Header of level: int * id: string
    /// A Markdown link definition (<c>[label]: url</c>).
    | LinkDef of LinkLabel

    override this.ToString() =
        match this with
        | Doc -> "Doc"
        | Title t -> $"T {{{t}}}"
        | Header(l, h) -> $"H{l} {{{h}}}"
        | LinkDef ld -> $"[{ld}]:"

    member this.AsString = this.ToString()

module Def =
    let isDoc =
        function
        | Doc -> true
        | _ -> false

    let isTitle =
        function
        | Title _ -> true
        | _ -> false

    let isHeaderOrTitle =
        function
        | Title _
        | Header _ -> true
        | _ -> false

    let isHeaderOrTitleWithId id =
        function
        | Title id'
        | Header(_, id') -> id = id'
        | _ -> false

    let isLinkDefWithLabel label =
        function
        | LinkDef label' when label = label' -> true
        | _ -> false

    let asHeader =
        function
        | Title id -> Some(1, id)
        | Header(level, id) -> Some(level, id)
        | _ -> None

/// Top-level symbol discriminated union: every named entity in a document is either a definition, a reference, or a tag.
[<RequireQualifiedAccess>]
[<StructuredFormatDisplay("{AsString}")>]
type Sym =
    /// A definition that other symbols can point to.
    | Def of Def
    /// A reference that points to a definition.
    | Ref of Ref
    /// A Zettelkasten tag.
    | Tag of Tag

    override this.ToString() =
        match this with
        | Ref link -> link.ToString()
        | Def def -> def.ToString()
        | Tag tag -> tag.ToString()

    member this.AsString = this.ToString()

/// The visibility scope of a symbol: either confined to a single document or visible workspace-wide.
[<RequireQualifiedAccess>]
[<StructuredFormatDisplay("{CompactFormat}")>]
type Scope =
    /// Symbol belongs to a specific document.
    | Doc of DocId
    /// Symbol is visible across the entire workspace.
    | Global

    override this.ToString() =
        match this with
        | Doc docId -> $"{docId}"
        | Global -> "Global"

    member this.CompactFormat = this.ToString()

module Scope =
    let asDoc =
        function
        | Scope.Doc id -> Some id
        | Scope.Global -> None

/// A symbol paired with the scope it belongs to.
type ScopedSym = Scope * Sym

module ScopedSym =
    let asScopedRef =
        function
        | scope, Sym.Ref ref -> Some(scope, ref)
        | _ -> None

type ScopeSlug = ScopeSlug of Slug

module ScopeSlug =
    let private ofDocId (docId: DocId) =
        docId.Path |> RootedRelPath.filenameStem |> Slug.ofString |> ScopeSlug

    let private ofScopedDefAux (docId: DocId) (def: Def) =
        match def with
        | LinkDef _ -> None
        | Doc
        | Header _ -> Some(ofDocId docId)
        | Title id -> Some(ScopeSlug(Slug.ofString id))

    let ofScopedDef (scope: Scope, def: Def) =
        match Scope.asDoc scope with
        | Some docId -> ofScopedDefAux docId def
        | None -> None

module Sym =
    let asRef =
        function
        | Sym.Ref ref -> Some ref
        | Sym.Def _
        | Sym.Tag _ -> None

    let asDef =
        function
        | Sym.Def def -> Some def
        | Sym.Ref _
        | Sym.Tag _ -> None

    let asTag =
        function
        | Sym.Tag tag -> Some tag
        | Sym.Ref _
        | Sym.Def _ -> None

    let isDoc sym = asDef sym |> Option.exists Def.isDoc

    let isTitle sym = asDef sym |> Option.exists Def.isTitle

    let isRefWithExplicitDoc sym = asRef sym |> Option.exists Ref.isCross

    let scoped (scope: Scope) (sym: Sym) = scope, sym

    let scopedToDoc (docId: DocId) (sym: Sym) = Scope.Doc docId, sym

    let allScopedToDoc (docId: DocId) (syms: seq<Sym>) = syms |> Seq.map (scopedToDoc docId)
