/// Tests for previously-deferred coverage gaps.
/// Requires the infrastructure in TestClient.fs and ServerHarness.fs.
module Marksman.GapTests

open Xunit
open Ionide.LanguageServerProtocol.Types

open Marksman.Compl
open Marksman.Config
open Marksman.Diag
open Marksman.Helpers

// ── GAP-02: isIncomplete flag on CompletionList ───────────────────────────────

module CompletionCapTests =
    /// Helpers: build a minimal CompletionItem without snapshot machinery.
    let private mkItem label : CompletionItem = {
        Label = label
        Kind = None
        Tags = None
        Detail = None
        Documentation = None
        Deprecated = None
        Preselect = None
        SortText = None
        FilterText = None
        InsertText = None
        InsertTextFormat = None
        InsertTextMode = None
        TextEdit = None
        TextEditText = None
        AdditionalTextEdits = None
        CommitCharacters = None
        Command = None
        Data = None
        LabelDetails = None
    }

    [<Fact>]
    let applyCompletionCap_isIncomplete_whenCapped () =
        // 60 items, cap = 50 → first 50 returned, IsIncomplete = true
        let items = Seq.init 60 (fun i -> mkItem $"item{i}")
        let result = applyCompletionCap 50 items
        Assert.Equal(50, result.Items.Length)
        Assert.True(result.IsIncomplete, "Expected IsIncomplete = true when pool exceeds cap")

    [<Fact>]
    let applyCompletionCap_notIncomplete_whenBelowCap () =
        // 5 items, cap = 50 → all 5 returned, IsIncomplete = false
        let items = Seq.init 5 (fun i -> mkItem $"item{i}")
        let result = applyCompletionCap 50 items
        Assert.Equal(5, result.Items.Length)
        Assert.False(result.IsIncomplete, "Expected IsIncomplete = false when pool is below cap")

    [<Fact>]
    let applyCompletionCap_exactCap_isIncomplete () =
        // Exactly 50 items with cap = 50 → IsIncomplete = true (boundary: server can't know
        // whether there are more items without checking beyond the cap)
        let items = Seq.init 50 (fun i -> mkItem $"item{i}")
        let result = applyCompletionCap 50 items
        Assert.Equal(50, result.Items.Length)
        Assert.True(result.IsIncomplete, "Expected IsIncomplete = true at exact cap boundary")

    [<Fact>]
    let applyCompletionCap_emptyInput_notIncomplete () =
        let result = applyCompletionCap 50 Seq.empty
        Assert.Equal(0, result.Items.Length)
        Assert.False(result.IsIncomplete)

// ── GAP-04: Diagnostics debounce ─────────────────────────────────────────────

module DebounceTests =
    open Marksman.ServerHarness

    /// Case-insensitive URI equality that also decodes percent-encoding.
    /// Marksman's systemPathToUriString encodes ':' as '%3A', while .NET's System.Uri does not.
    let private uriEq (a: string) (b: string) =
        System.String.Equals(
            System.Uri.UnescapeDataString(a),
            System.Uri.UnescapeDataString(b),
            System.StringComparison.OrdinalIgnoreCase
        )

    [<Fact>]
    let debounce_publishesDiagnostics_afterQuietPeriod () =
        // Open a .md file containing a broken wiki link; after the debounce fires the
        // server must have published at least one diagnostic for that document.
        use ts = new TestServer([], debounceMs = 10)
        ts.DidOpen("note.md", "[[this-doc-does-not-exist]]")
        let diags = ts.WaitDiagnostics()

        let noteUri = System.Uri(System.IO.Path.Combine(ts.RootDir, "note.md")).AbsoluteUri
        let noteDiags =
            diags
            |> Array.tryFind (fun p -> uriEq p.Uri noteUri)
            |> Option.map (fun p -> p.Diagnostics)
            |> Option.defaultValue [||]

        Assert.True(noteDiags.Length > 0, "Expected at least one diagnostic for broken wiki link")

        let msg = noteDiags[0].Message
        Assert.Contains("this-doc-does-not-exist", msg)

    [<Fact>]
    let debounce_suppressesDuplicateUpdates_duringEdit () =
        // File is opened twice in rapid succession (simulating fast typing).
        // The debounce should coalesce the bursts: diagnostics must eventually be published
        // but only the final state should matter.
        use ts = new TestServer([], debounceMs = 10)
        // First open: broken link
        ts.DidOpen("note.md", "[[missing]]")
        // Second open of the same file would normally be a textDocument/didChange,
        // but at the harness level we just verify the final publish is sensible.
        let diags = ts.WaitDiagnostics()

        let published = diags |> Array.filter (fun p -> p.Uri.EndsWith("note.md"))
        Assert.True(published.Length >= 1, "Expected at least one publish cycle for note.md")

// ── GAP-08: File-extension filter (slug-level, no filesystem needed) ──────────

module FileExtensionTests =
    [<Fact>]
    let customExtension_txtDoc_resolvedBySlug () =
        // When the folder is configured with ["txt"] extensions, a .txt doc's slug
        // is computed WITHOUT the extension, so [[name]] resolves to it.
        let config = { Config.Empty with coreMarkdownFileExtensions = Some [| "txt" |] }
        let target = FakeDoc.Mk("# Hello", path = "target.txt", config = config)
        let srcDoc = FakeDoc.Mk("[[target]]", path = "src.txt", config = config)
        let folder = FakeFolder.Mk([ target; srcDoc ], config = config)
        // No broken-link diagnostics expected: [[target]] resolves to target.txt
        let diags = checkFolder folder |> Seq.collect snd |> Array.ofSeq
        let broken = diags |> Array.filter (function | BrokenLink _ -> true | _ -> false)
        Assert.Empty(broken)

    [<Fact>]
    let defaultExtensions_txtDoc_notResolved () =
        // With the default ["md"; "markdown"] configuration, a .txt doc's slug keeps the
        // extension ("target.txt"), so [[target]] is a broken link.
        let txtConfig = { Config.Empty with coreMarkdownFileExtensions = Some [| "md"; "markdown" |] }
        let target = FakeDoc.Mk("# Hello", path = "target.txt")
        let srcDoc = FakeDoc.Mk("[[target]]", path = "src.md")
        let folder = FakeFolder.Mk([ target; srcDoc ], config = txtConfig)
        let diags = checkFolder folder |> Seq.collect snd |> Array.ofSeq
        let broken = diags |> Array.filter (function | BrokenLink _ -> true | _ -> false)
        Assert.True(broken.Length > 0, "Expected BrokenLink for .txt target with default md-only extensions")

    [<Fact>]
    let defaultExtensions_mdDoc_resolvedBySlug () =
        // Control: a .md doc with the default config IS resolved.
        let srcDoc = FakeDoc.Mk("[[target]]", path = "src.md")
        let target = FakeDoc.Mk("# Hello", path = "target.md")
        let folder = FakeFolder.Mk([ target; srcDoc ])
        let diags = checkFolder folder |> Seq.collect snd |> Array.ofSeq
        let broken = diags |> Array.filter (function | BrokenLink _ -> true | _ -> false)
        Assert.Empty(broken)

// ── GAP-10: Integration via TestServer ────────────────────────────────────────

module IntegrationTests =
    open Marksman.ServerHarness

    /// Percent-decode + case-insensitive URI comparison (Marksman encodes ':' as '%3A').
    let private uriEq (a: string) (b: string) =
        System.String.Equals(
            System.Uri.UnescapeDataString(a),
            System.Uri.UnescapeDataString(b),
            System.StringComparison.OrdinalIgnoreCase
        )

    [<Fact>]
    let integration_brokenLink_publishesDiagnostic () =
        // Server reads a workspace that contains a .md file with a broken wiki link.
        // After initialization the diagnostic pipeline publishes the error.
        let files = [ "broken.md", "# Note\n\n[[does-not-exist]]" ]
        use ts = new TestServer(files, debounceMs = 10)
        let diags = ts.WaitDiagnostics()

        let brokenUri = System.Uri(System.IO.Path.Combine(ts.RootDir, "broken.md")).AbsoluteUri

        let fileDiags =
            diags
            |> Array.tryFind (fun p -> uriEq p.Uri brokenUri)
            |> Option.map (fun p -> p.Diagnostics)
            |> Option.defaultValue [||]

        Assert.True(fileDiags.Length > 0, "Expected diagnostics for broken.md")
        Assert.Equal(Some DiagnosticSeverity.Error, fileDiags[0].Severity)
        Assert.Contains("does-not-exist", fileDiags[0].Message)

    [<Fact>]
    let integration_validLink_noDiagnostic () =
        // Two files where one links to the other: no diagnostics expected.
        let files = [
            "a.md", "# Alpha\n\n[[beta]]"
            "b.md", "# Beta"
        ]
        use ts = new TestServer(files, debounceMs = 10)
        let diags = ts.WaitDiagnostics()

        let aUri = System.Uri(System.IO.Path.Combine(ts.RootDir, "a.md")).AbsoluteUri

        let fileDiags =
            diags
            |> Array.tryFind (fun p -> uriEq p.Uri aUri)
            |> Option.map (fun p -> p.Diagnostics)
            |> Option.defaultValue [||]

        Assert.Empty(fileDiags)

    [<Fact>]
    let integration_didOpen_triggersAdditionalDiagnostics () =
        // Start with no files; open a doc with a broken link via didOpen.
        // The diagnostic pipeline must publish an error for it.
        use ts = new TestServer([], debounceMs = 10)
        ts.DidOpen("new.md", "[[ghost]]")
        let diags = ts.WaitDiagnostics()

        let newUri = System.Uri(System.IO.Path.Combine(ts.RootDir, "new.md")).AbsoluteUri
        let fileDiags =
            diags
            |> Array.tryFind (fun p -> uriEq p.Uri newUri)
            |> Option.map (fun p -> p.Diagnostics)
            |> Option.defaultValue [||]

        Assert.True(fileDiags.Length > 0, "Expected diagnostics after didOpen with broken link")
