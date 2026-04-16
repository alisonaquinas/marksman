/// Per-document lookup index built from CST elements; provides fast access to
/// headings, links, tags, and YAML front matter by slug, label, or source position.
module Marksman.Index

open Ionide.LanguageServerProtocol.Types

open Marksman.Cst
open Marksman.Misc

type Dictionary<'K, 'V> = System.Collections.Generic.Dictionary<'K, 'V>

// TODO: get rid of this; use Structure directly
/// Fast per-document lookup table derived from the CST; groups headings, links,
/// tags, and YAML front matter into arrays and slug-keyed maps.
type Index = {
    titles: array<Node<Heading>>
    headings: array<Node<Heading>>
    headingsBySlug: Map<Slug, list<Node<Heading>>>
    wikiLinks: array<Node<WikiLink>>
    mdLinks: array<Node<MdLink>>
    linkDefs: array<Node<MdLinkDef>>
    tags: array<Node<Tag>>
    yamlFrontMatter: option<TextNode>
}

module Index =
    /// Build an Index by scanning all CST elements once.
    let ofCst (cels: Cst.Element[]) : Index =
        let titles = ResizeArray()
        let headingsBySlug = Dictionary<Slug, ResizeArray<Node<Heading>>>()
        let wikiLinks = ResizeArray()
        let headings = ResizeArray()
        let mdLinks = ResizeArray()
        let linkDefs = ResizeArray()
        let tags = ResizeArray()
        let mutable yaml = None

        for el in cels do
            match el with
            | H hn ->
                let slug = Heading.slug hn.data

                if not (headingsBySlug.ContainsKey(slug)) then
                    headingsBySlug[slug] <- ResizeArray()

                headingsBySlug[slug].Add(hn)

                if Heading.isTitle hn.data then
                    titles.Add(hn)

                headings.Add(hn)
            | WL wl -> wikiLinks.Add(wl)
            | ML ml -> mdLinks.Add(ml)
            | MLD linkDef -> linkDefs.Add(linkDef)
            | T t -> tags.Add(t)
            | YML yml -> yaml <- Some yml

        let headingsBySlug =
            seq {
                for KeyValue(slug, headings) in headingsBySlug do
                    yield slug, headings |> List.ofSeq
            }
            |> Map.ofSeq

        let titles = titles.ToArray()
        let wikiLinks = wikiLinks.ToArray()
        let mdLinks = mdLinks.ToArray()
        let linkDefs = linkDefs.ToArray()
        let headings = headings.ToArray()
        let tags = tags.ToArray()

        {
            titles = titles
            headings = headings
            headingsBySlug = headingsBySlug
            wikiLinks = wikiLinks
            mdLinks = mdLinks
            linkDefs = linkDefs
            tags = tags
            yamlFrontMatter = yaml
        }

    let titles index = index.titles

    let title = titles >> Array.tryHead

    let wikiLinks index = index.wikiLinks

    let mdLinks index = index.mdLinks

    let links index =
        Seq.append (wikiLinks index |> Seq.map WL) (mdLinks index |> Seq.map ML)

    let linkDefs index = index.linkDefs

    let tags index = index.tags

    let filterTagsByName name index =
        index.tags
        |> Array.filter (fun { data = tag } -> tag.name.text = name)

    /// Find the link-reference definition whose normalised label equals the given label.
    let tryFindLinkDef (label: LinkLabel) index =
        index.linkDefs
        |> Array.tryFind (fun { data = ld } -> (MdLinkDef.normalizedLabel ld) = label)

    let filterLinkDefs (pred: LinkLabel -> bool) index =
        index.linkDefs
        |> Seq.filter (fun { data = ld } -> pred (MdLinkDef.normalizedLabel ld))

    let headingsBySlug index = index.headingsBySlug

    let headings index = index.headings

    /// Return all headings whose slug matches the given slug.
    let filterHeadingBySlug slug index =
        index.headingsBySlug |> Map.tryFind slug |> Option.defaultValue []

    /// Find the wiki-link or MD link whose range contains the given position.
    let linkAtPos (pos: Position) index =
        let matching el =
            let range = Node.range el
            range.Start <= pos && pos < range.End

        let fromWiki () = Array.tryFind matching index.wikiLinks |> Option.map WL
        let fromMd () = Array.tryFind matching index.mdLinks |> Option.map ML

        fromWiki () |> Option.orElseWith fromMd

    /// Find the heading or link-reference definition whose range contains the given position.
    let declAtPos (pos: Position) (index: Index) : option<Element> =
        let matching el =
            let range = Node.range el
            range.Start <= pos && pos < range.End

        let fromHeadings () = Seq.tryFind matching (headings index) |> Option.map H
        let fromLinkDefs () = Array.tryFind matching index.linkDefs |> Option.map MLD

        fromHeadings () |> Option.orElseWith fromLinkDefs
