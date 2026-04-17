/// In-process server harness for integration tests.
/// Creates real files on disk, initialises a MarksmanServer, and exposes helpers
/// for sending LSP notifications and collecting the resulting diagnostics.
module Marksman.ServerHarness

open System
open System.IO
open Ionide.LanguageServerProtocol.Types
open Marksman.Server
open Marksman.TestClient

/// Write (relative-path, content) pairs to a fresh temp directory; return the directory path.
/// A `.marksman.toml` marker file is always written so Marksman treats the directory
/// as a real workspace folder (see `Folder.isRealWorkspaceFolder`).
let mkTempRoot (files: (string * string) list) : string =
    let dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory(dir) |> ignore

    // Required: Marksman validates workspace folders by checking for .marksman.toml / .git etc.
    File.WriteAllText(Path.Combine(dir, ".marksman.toml"), "")

    for (relPath, content) in files do
        let full = Path.Combine(dir, relPath)
        let parent = Path.GetDirectoryName(full)
        if not (Directory.Exists(parent)) then
            Directory.CreateDirectory(parent) |> ignore
        File.WriteAllText(full, content)

    dir

/// Build a minimal InitializeParams for a single-folder workspace rooted at `rootUri`.
let mkInitParams (rootUri: string) : InitializeParams = {
    ProcessId = None
    ClientInfo = None
    Locale = None
    RootPath = None
    RootUri = Some rootUri
    InitializationOptions = None
    Capabilities = None
    trace = None
    WorkspaceFolders = Some [| { Uri = rootUri; Name = "test" } |]
}

/// An in-process MarksmanServer backed by real files in a temp directory.
/// Cleans up the temp directory when disposed.
///
/// `files`      — (relative path, content) pairs written before initialisation.
/// `debounceMs` — diagnostic debounce timeout in ms (default 10 for fast tests).
type TestServer(files: (string * string) list, ?debounceMs: int) =
    let rootDir = mkTempRoot files

    // Normalise to a file:// URI that works on both Windows and Unix.
    let rootUri = Uri(rootDir).AbsoluteUri

    let capture = CaptureClient()
    let server = new MarksmanServer(capture.Client, defaultArg debounceMs 10)

    do
        server.Initialize(mkInitParams rootUri) |> Async.RunSynchronously |> ignore
        server.Initialized(InitializedParams()) |> Async.RunSynchronously

    /// The underlying server (for calling handlers not covered by helpers below).
    member _.Server = server

    /// The capture client — inspect PublishedDiagnostics after triggering work.
    member _.Capture = capture

    /// Root directory on disk.
    member _.RootDir = rootDir

    /// Root URI (file:///…) as passed to Initialize.
    member _.RootUri = rootUri

    /// Notify the server that a file was opened (triggers the diagnostic pipeline).
    member _.DidOpen(relPath: string, content: string) =
        let uri = Uri(Path.Combine(rootDir, relPath)).AbsoluteUri

        let p: DidOpenTextDocumentParams = {
            TextDocument = {
                Uri = uri
                LanguageId = "markdown"
                Version = 1
                Text = content
            }
        }

        server.TextDocumentDidOpen(p) |> Async.RunSynchronously

    /// Poll until at least one diagnostic notification is published or the timeout elapses.
    /// Under parallel test load the MailboxProcessor may be delayed well past the debounce
    /// interval, so we poll in small increments up to maxWaitMs = debounceMs * 20 + extraMs.
    member _.WaitDiagnostics(?extraMs: int) =
        let maxWaitMs = (defaultArg debounceMs 10) * 20 + defaultArg extraMs 0
        let pollMs = 5
        let sw = System.Diagnostics.Stopwatch.StartNew()

        let rec poll () =
            let diags = capture.PublishedDiagnostics
            if diags.Length > 0 || sw.ElapsedMilliseconds >= int64 maxWaitMs then
                diags
            else
                Async.Sleep(pollMs) |> Async.RunSynchronously
                poll ()

        poll ()

    interface IDisposable with
        member _.Dispose() =
            (server :> IDisposable).Dispose()

            if Directory.Exists(rootDir) then
                try
                    Directory.Delete(rootDir, recursive = true)
                with _ ->
                    () // best-effort cleanup; don't fail the test teardown
