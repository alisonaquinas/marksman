/// Table of Contents generation: builds, renders, detects existing TOC blocks, and computes insertion points.
module Marksman.Toc

open Ionide.LanguageServerProtocol.Types
open Ionide.LanguageServerProtocol.Logging

open Marksman.Cst
open Marksman.Index
open Marksman.Misc
open Marksman.Text
open Marksman.Doc

[<Literal>]
let StartMarker = """<!--toc:start-->"""

[<Literal>]
let EndMarker = """<!--toc:end-->"""

[<Literal>]
let EmptyLine = ""

type Title = string
type EntryLevel = int

/// A single TOC entry: heading level, display title, and the slug used as the anchor link.
type Entry = { level: EntryLevel; title: Title; link: Slug }

module Entry =
    let Mk (level: EntryLevel, title: Title) = {
        level = level
        title = title
        link = Slug.ofString title
    }

    let renderLink entry minLevel =
        let offset = String.replicate (entry.level - minLevel) "  "
        let slug = entry.link |> Slug.toString
        $"{offset}- [{entry.title}](#{slug})"

    let fromHeading (heading: Heading) : Entry =
        let slug = Heading.slug heading
        { level = heading.level; link = slug; title = heading.title.text }

/// Where the rendered TOC block should be placed or replaced in the document.
type InsertionPoint =
    | After of Range
    | Replacing of Range
    | DocumentBeginning

/// An ordered list of TOC entries derived from the document's headings.
type TableOfContents = { entries: array<Entry> }

module TableOfContents =

    let logger = LogProvider.getLoggerByName "TocAgent"

    /// Builds a TOC from the headings in `index`, filtering to `includeLevels`; returns None if the document has no headings.
    let mk (includeLevels: array<int>) (index: Marksman.Index.Index) : option<TableOfContents> =
        let headings =
            index.headings
            |> Array.map (fun x -> x.data)
            |> Array.filter (fun h -> Array.contains h.level includeLevels)

        if Array.isEmpty index.headings then
            None
        else
            Some { entries = Array.map Entry.fromHeading headings }

    /// Determines where in the document a new TOC block should be inserted.
    let insertionPoint (doc: Doc) : InsertionPoint =
        let index = Doc.index doc

        match (Array.toList index.titles) with
        // if there's only a single title
        | [ singleTitle ] -> After singleTitle.range
        | _ ->
            match index.yamlFrontMatter with
            | None -> DocumentBeginning
            | Some yml -> After yml.range


    /// Renders the TOC to a Markdown string wrapped in start/end marker comments.
    let render (toc: TableOfContents) =
        let offset =
            if Array.isEmpty toc.entries then
                1
            else
                (Array.minBy (fun x -> x.level) toc.entries).level

        let tocLinks = Array.map (fun x -> Entry.renderLink x offset) toc.entries
        let startMarkerLines = [| StartMarker |]
        let endMarkerLines = [| EndMarker |]

        let lines = Array.concat [| startMarkerLines; tocLinks; endMarkerLines |]

        concatLines lines

    type State =
        | BeforeMarker
        | Collecting of Range
        | Collected of Range

    /// Scans `text` for an existing TOC block delimited by start/end markers and returns its range.
    let detect (text: Text) : Range option =
        let lines = text.lineMap
        let maxIndex = lines.NumLines

        let rec go i st =
            if i.Equals(maxIndex) then
                st
            else
                let lineRange = text.LineContentRange(i)
                let lineContent = text.LineContent(i)
                let isStartMarker = lineContent.Trim().Equals(StartMarker)
                let isEndMarker = lineContent.Trim().Equals(EndMarker)
                let expandToThisLine (range: Range) = { range with End = lineRange.End }

                match st with
                // if we found the marker, start collecting text from here
                | BeforeMarker ->
                    if isStartMarker then
                        go (i + 1) (Collecting lineRange)
                    else
                        go (i + 1) BeforeMarker
                // if we are in collecting mode, just expand the selection
                | Collecting range ->
                    let toThisLine = expandToThisLine range

                    if isEndMarker then
                        Collected toThisLine
                    else
                        go (i + 1) (Collecting toThisLine)
                | _ -> st


        match (go 0 BeforeMarker) with
        // marker was never found
        | Collected range -> Some range
        | BeforeMarker -> None
        | other ->
            logger.warn (
                Log.setMessage "TOC detection failed - end marker was not found"
                >> Log.addContext "finalState" other
            )

            None


    let isSame (tocA: string) (tocB: string) =
        // Have to do this line-by-line comparison to make sure that things work as expected both
        // on Unix and Windows and when mixed line-endings are used.
        let contentA = tocA.TrimBoth(StartMarker, EndMarker).Trim().Lines()
        let contentB = tocB.TrimBoth(StartMarker, EndMarker).Trim().Lines()

        if contentA.Length <> contentB.Length then
            false
        else
            Array.forall2 (=) contentA contentB
