/// A capturing LSP client for use in tests: records every diagnostic notification
/// published by the server so tests can assert on the results.
module Marksman.TestClient

open System.Collections.Concurrent
open Ionide.LanguageServerProtocol.Server  // ClientNotificationSender, ClientRequestSender
open Ionide.LanguageServerProtocol.Types
open Marksman.Server

/// Stub ClientRequestSender — Marksman never sends requests to the client,
/// but the MarksmanClient constructor requires one.
let private stubRequestSender =
    { new ClientRequestSender with
        member _.Send<'a> _method _data =
            AsyncLspResult.notImplemented<'a> }

/// Wraps MarksmanClient with a notification-capturing sender.
/// Captures every `textDocument/publishDiagnostics` call for test assertions.
type CaptureClient() =
    let diags = ConcurrentQueue<PublishDiagnosticsParams>()

    let notiSender (method': string) (data: obj) : AsyncLspResult<unit> =
        if method' = "textDocument/publishDiagnostics" then
            diags.Enqueue(data :?> PublishDiagnosticsParams)

        AsyncLspResult.success ()

    /// The MarksmanClient instance to pass to MarksmanServer.
    member _.Client = MarksmanClient(notiSender, stubRequestSender)

    /// Snapshot of all diagnostics published so far (in arrival order).
    member _.PublishedDiagnostics = diags |> Seq.toArray

    /// Clear all captured diagnostics (useful between test phases).
    member _.Clear() = diags.Clear()
