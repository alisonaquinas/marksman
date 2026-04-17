module Marksman.DiagTest

open Xunit
open Ionide.LanguageServerProtocol.Types

open Marksman.Diag
open Marksman.Helpers
open Marksman.Index
open Marksman.Names
open Marksman.Paths
open Marksman.Doc
open Marksman.Folder

let entryToHuman (entry: Entry) =
    let lsp = diagToLsp entry
    lsp.Message

let diagToHuman (diag: seq<DocId * list<Entry>>) : list<string * string> =
    seq {
        for id, entries in diag do
            for e in entries do
                yield id.Path |> RootedRelPath.relPathForced |> RelPath.toSystem, entryToHuman e
    }
    |> List.ofSeq

[<Fact>]
let documentIndex_1 () =
    let doc = FakeDoc.Mk "# T1\n# T2"

    let titles =
        Doc.index >> Index.titles <| doc
        |> Array.map (fun x -> x.data.title.text)

    Assert.Equal<string>([ "T1"; "T2" ], titles)

[<Fact>]
let nonBreakingWhitespace () =
    let nbsp = "\u00a0"
    let doc = FakeDoc.Mk $"# T1\n##{nbsp}T2"

    match (checkNonBreakingWhitespace doc) with
    | [ NonBreakableWhitespace range ] ->
        Assert.Equal(1, range.Start.Line)
        Assert.Equal(1, range.End.Line)

        Assert.Equal(2, range.Start.Character)
        Assert.Equal(3, range.End.Character)
    | _ -> failwith "Expected NonBreakingWhitespace diagnostic"

[<Fact>]
let noDiagOnShortcutLinks () =
    let doc = FakeDoc.Mk([| "# H1"; "## H2"; "[shortcut]"; "[[#h42]]" |])
    let folder = FakeFolder.Mk([ doc ])
    let diag = checkFolder folder |> diagToHuman

    Assert.Equal<string * string>([ "fake.md", "Link to non-existent heading 'h42'" ], diag)

[<Fact>]
let noDiagOnRealUrls () =
    let doc =
        FakeDoc.Mk([| "# H1"; "## H2"; "[](www.bad.md)"; "[](https://www.good.md)" |])

    let folder = FakeFolder.Mk([ doc ])
    let diag = checkFolder folder |> diagToHuman

    Assert.Equal<string * string>([ "fake.md", "Link to non-existent document 'www.bad.md'" ], diag)

[<Fact>]
let noDiagOnNonMarkdownFiles () =
    let doc =
        FakeDoc.Mk(
            [|
                "# H1"
                "## H2"
                "[](bad.md)"
                "[](another%20bad.md)"
                "[](good/folder)"
            |]
        )

    let folder = FakeFolder.Mk([ doc ])
    let diag = checkFolder folder |> diagToHuman

    Assert.Equal<string * string>(
        [
            "fake.md", "Link to non-existent document 'bad.md'"
            "fake.md", "Link to non-existent document 'another bad.md'"
        ],
        diag
    )


[<Fact>]
let crossFileDiagOnBrokenWikiLinks () =
    let doc = FakeDoc.Mk([| "[[bad]]" |])

    let folder = FakeFolder.Mk([ doc ])
    let diag = checkFolder folder |> diagToHuman

    Assert.Equal<string * string>([ "fake.md", "Link to non-existent document 'bad'" ], diag)

[<Fact>]
let noCrossFileDiagOnSingleFileFolders () =
    let doc =
        FakeDoc.Mk(
            [|
                "[](bad.md)" //
                "[[another-bad]]"
                "[bad-ref][bad-ref]"
            |]
        )

    let folder = Folder.singleFile doc None
    let diag = checkFolder folder |> diagToHuman

    Assert.Equal<string * string>(
        [
            "fake.md", "Link to non-existent link definition with the label 'bad-ref'"
        ],
        diag
    )

[<Fact>]
let brokenWikiLink_hasSeverityError () =
    let doc = FakeDoc.Mk([| "[[bad]]" |])
    let folder = FakeFolder.Mk([ doc ])
    let entries = checkFolder folder |> Seq.collect snd |> Array.ofSeq
    Assert.Equal(1, entries.Length)
    let lspDiag = diagToLsp entries[0]
    Assert.Equal(Some DiagnosticSeverity.Error, lspDiag.Severity)

[<Fact>]
let brokenMarkdownLink_hasSeverityWarning () =
    let doc = FakeDoc.Mk([| "[text](missing.md)" |])
    let folder = FakeFolder.Mk([ doc ])
    let entries = checkFolder folder |> Seq.collect snd |> Array.ofSeq
    Assert.Equal(1, entries.Length)
    let lspDiag = diagToLsp entries[0]
    Assert.Equal(Some DiagnosticSeverity.Warning, lspDiag.Severity)

[<Fact>]
let brokenWikiLink_hasCodeTwo () =
    let doc = FakeDoc.Mk([| "[[bad]]" |])
    let folder = FakeFolder.Mk([ doc ])
    let entries = checkFolder folder |> Seq.collect snd |> Array.ofSeq
    Assert.Equal(1, entries.Length)
    let lspDiag = diagToLsp entries[0]
    Assert.Equal(Some "2", lspDiag.Code)

[<Fact>]
let ambiguousWikiLink_hasCodeOneAndRelatedInfo () =
    let docA = FakeDoc.Mk("# Introduction", path = "a.md")
    let docB = FakeDoc.Mk("# Introduction", path = "b.md")
    let srcDoc = FakeDoc.Mk("[[Introduction]]", path = "src.md")
    let folder = FakeFolder.Mk([ docA; docB; srcDoc ])
    let entries = checkFolder folder |> Seq.collect snd |> Array.ofSeq
    let ambiguous = entries |> Array.filter (function | AmbiguousLink _ -> true | _ -> false)
    Assert.Single(ambiguous) |> ignore
    let lspDiag = diagToLsp ambiguous[0]
    Assert.Equal(Some "1", lspDiag.Code)
    Assert.Equal(Some DiagnosticSeverity.Error, lspDiag.Severity)
    Assert.Equal(2, (lspDiag.RelatedInformation |> Option.get).Length)
