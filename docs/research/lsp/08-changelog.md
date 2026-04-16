---
title: "LSP 3.17 — Change Log"
source: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
date: 2026-04-16
tags:
  - wiki/input
  - research/lsp
---

### Change Log

#### 3.17.0 (05/10/2022)

- Specify how clients will handle stale requests.
- Add support for a completion item label details.
- Add support for workspace symbol resolve request.
- Add support for label details and insert text mode on completion items.
- Add support for shared values on CompletionItemList.
- Add support for HTML tags in Markdown.
- Add support for collapsed text in folding.
- Add support for trigger kinds on code action requests.
- Add the following support to semantic tokens:
  - server cancelable
  - augmentation of syntax tokens
- Add support to negotiate the position encoding.
- Add support for relative patterns in file watchers.
- Add support for type hierarchies
- Add support for inline values.
- Add support for inlay hints.
- Add support for notebook documents.
- Add support for diagnostic pull model.

#### 3.16.0 (12/14/2020)

- Add support for tracing.
- Add semantic token support.
- Add call hierarchy support.
- Add client capability for resolving text edits on completion items.
- Add support for client default behavior on renames.
- Add support for insert and replace ranges on `CompletionItem`.
- Add support for diagnostic code descriptions.
- Add support for document symbol provider label.
- Add support for tags on `SymbolInformation` and `DocumentSymbol`.
- Add support for moniker request method.
- Add support for code action `data` property.
- Add support for code action `disabled` property.
- Add support for code action resolve request.
- Add support for diagnostic `data` property.
- Add support for signature information `activeParameter` property.
- Add support for `workspace/didCreateFiles` notifications and `workspace/willCreateFiles` requests.
- Add support for `workspace/didRenameFiles` notifications and `workspace/willRenameFiles` requests.
- Add support for `workspace/didDeleteFiles` notifications and `workspace/willDeleteFiles` requests.
- Add client capability to signal whether the client normalizes line endings.
- Add support to preserve additional attributes on `MessageActionItem`.
- Add support to provide the clients locale in the initialize call.
- Add support for opening and showing a document in the client user interface.
- Add support for linked editing.
- Add support for change annotations in text edits as well as in create file, rename file and delete file operations.

#### 3.15.0 (01/14/2020)

- Add generic progress reporting support.
- Add specific work done progress reporting support to requests where applicable.
- Add specific partial result progress support to requests where applicable.
- Add support for `textDocument/selectionRange`.
- Add support for server and client information.
- Add signature help context.
- Add Erlang and Elixir to the list of supported programming languages
- Add `version` on `PublishDiagnosticsParams`
- Add `CodeAction#isPreferred` support.
- Add `CompletionItem#tag` support.
- Add `Diagnostic#tag` support.
- Add `DocumentLink#tooltip` support.
- Add `trimTrailingWhitespace`, `insertFinalNewline` and `trimFinalNewlines` to `FormattingOptions`.
- Clarified `WorkspaceSymbolParams#query` parameter.

#### 3.14.0 (12/13/2018)

- Add support for signature label offsets.
- Add support for location links.
- Add support for `textDocument/declaration` request.

#### 3.13.0 (9/11/2018)

- Add support for file and folder operations (create, rename, move) to workspace edits.

#### 3.12.0 (8/23/2018)

- Add support for `textDocument/prepareRename` request.

#### 3.11.0 (8/21/2018)

- Add support for CodeActionOptions to allow a server to provide a list of code action it supports.

#### 3.10.0 (7/23/2018)

- Add support for hierarchical document symbols as a valid response to a `textDocument/documentSymbol` request.
- Add support for folding ranges as a valid response to a `textDocument/foldingRange` request.

#### 3.9.0 (7/10/2018)

- Add support for `preselect` property in `CompletionItem`

#### 3.8.0 (6/11/2018)

- Added support for CodeAction literals to the `textDocument/codeAction` request.
- ColorServerCapabilities.colorProvider can also be a boolean
- Corrected ColorPresentationParams.colorInfo to color (as in the `d.ts` and in implementations)

#### 3.7.0 (4/5/2018)

- Added support for related information to Diagnostics.

#### 3.6.0 (2/22/2018)

Merge the proposed protocol for workspace folders, configuration, go to type definition, go to implementation and document color provider into the main branch of the specification. For details see:

- [Get Workspace Folders](https://microsoft.github.io/language-server-protocol/specification#workspace_workspaceFolders)
- [DidChangeWorkspaceFolders Notification](https://microsoft.github.io/language-server-protocol/specification#workspace_didChangeWorkspaceFolders)
- [Get Configuration](https://microsoft.github.io/language-server-protocol/specification#workspace_configuration)
- [Go to Type Definition](https://microsoft.github.io/language-server-protocol/specification#textDocument_typeDefinition)
- [Go to Implementation](https://microsoft.github.io/language-server-protocol/specification#textDocument_implementation)
- [Document Color](https://microsoft.github.io/language-server-protocol/specification#textDocument_documentColor)
- [Color Presentation](https://microsoft.github.io/language-server-protocol/specification#textDocument_colorPresentation)

In addition we enhanced the `CompletionTriggerKind` with a new value `TriggerForIncompleteCompletions: 3 = 3` to signal the a completion request got trigger since the last result was incomplete.

#### 3.5.0

Decided to skip this version to bring the protocol version number in sync the with npm module vscode-languageserver-protocol.

#### 3.4.0 (11/27/2017)

- [extensible completion item and symbol kinds](https://github.com/Microsoft/language-server-protocol/issues/129)

#### 3.3.0 (11/24/2017)

- Added support for `CompletionContext`
- Added support for `MarkupContent`
- Removed old New and Updated markers.

#### 3.2.0 (09/26/2017)

- Added optional `commitCharacters` property to the `CompletionItem`

#### 3.1.0 (02/28/2017)

- Make the `WorkspaceEdit` changes backwards compatible.
- Updated the specification to correctly describe the breaking changes from 2.x to 3.x around `WorkspaceEdit`and `TextDocumentEdit`.

#### 3.0 Version

- add support for client feature flags to support that servers can adapt to different client capabilities. An example is the new `textDocument/willSaveWaitUntil` request which not all clients might be able to support. If the feature is disabled in the client capabilities sent on the initialize request, the server can’t rely on receiving the request.
- add support to experiment with new features. The new `ClientCapabilities.experimental` section together with feature flags allow servers to provide experimental feature without the need of ALL clients to adopt them immediately.
- servers can more dynamically react to client features. Capabilities can now be registered and unregistered after the initialize request using the new `client/registerCapability` and `client/unregisterCapability`. This for example allows servers to react to settings or configuration changes without a restart.
- add support for `textDocument/willSave` notification and `textDocument/willSaveWaitUntil` request.
- add support for `textDocument/documentLink` request.
- add a `rootUri` property to the initializeParams in favor of the `rootPath` property.


Base Protocol


- Header Part
- Content Part
- Base Types
- Request Message
- Response Message
- Notification Message
- Cancellation Support
- Progress Support


Language Server Protocol


- Overview
- Capabilities
- Message Ordering
- Message Documentation


Basic JSON Structures


- URI
- Regular Expression
- Enumerations
- Text Documents
- Position
- Range
- Text Document Item
- Text Document Identifier
- Versioned Text Document Identifier
- Text Document Position Params
- Document Filter
- Text Edit
- Text Edit Array
- Text Document Edit
- Location
- Location Link
- Diagnostic
- Command
- Markup Content
- File Resource Changes
- Workspace Edit
- Work Done Progress
- Client Initiated Progress
- Server Initiated Progress
- Partial Results
- Partial Result Params
- Trace Value


Lifecycle Messages


- Overview
- Initialize
- Initialized
- Register Capability
- Unregister Capability
- Set Trace
- Log Trace
- Shutdown
- Exit


Document Synchronization


- Overview - Text Document
- Did Open Text Document
- Did Change Text Document
- Will Save Text Document
- Will Save Document Wait Until
- Did Save Text Document
- Did Close Text Document
- Rename a Text Document
- Overview - Notebook Document
- Did Open Notebook Document
- Did Change Notebook Document
- Did Save Notebook Document
- Did Close Notebook Document


Language Features


- Overview
- Go to Declaration
- Go to Definition
- Go to Type Definition
- Go to Implementation
- Find References
- Prepare Call Hierarchy
- Call Hierarchy Incoming Calls
- Call Hierarchy Outgoing Calls
- Prepare Type Hierarchy
- Type Hierarchy Super Types
- Type Hierarchy Sub Types
- Document Highlight
- Document Link
- Document Link Resolve
- Hover
- Code Lens
- Code Lens Refresh
- Folding Range
- Selection Range
- Document Symbols
- Semantic Tokens
- Inline Value
- Inline Value Refresh
- Inlay Hint
- Inlay Hint Resolve
- Inlay Hint Refresh
- Moniker
- Completion Proposals
- Completion Item Resolve
- Publish Diagnostics
- Pull Diagnostics
- Signature Help
- Code Action
- Code Action Resolve
- Document Color
- Color Presentation
- Formatting
- Range Formatting
- On type Formatting
- Rename
- Prepare Rename
- Linked Editing Range


Workspace Features


- Workspace Symbols
- Workspace Symbol Resolve
- Get Configuration
- Did Change Configuration
- Workspace Folders
- Did Change Workspace Folders
- Will Create Files
- Did Create Files
- Will Rename Files
- Did Rename Files
- Will Delete Files
- Did Delete Files
- Did Change Watched Files
- Execute Command
- Apply Edit


Window Features


- Show Message Notification
- Show Message Request
- Log Message
- Show Document
- Create Work Done Progress
- Cancel a Work Done Progress
- Sent Telemetry


Miscellaneous


- Implementation Considerations
- Meta Model


Change Log


- 3.17.0
- 3.16.0
- 3.15.0
- 3.14.0
- 3.13.0
- 3.12.0
- 3.11.0
- 3.10.0
- 3.9.0
- 3.8.0
- 3.7.0
- 3.6.0
- 3.5.0
- 3.4.0
- 3.3.0
- 3.2.0
- 3.1.0
- 3.0


