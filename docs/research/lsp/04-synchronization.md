---
title: "LSP 3.17 — Document Synchronization"
source: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
date: 2026-04-16
tags:
  - wiki/input
  - research/lsp
---

### Text Document Synchronization

Client support for `textDocument/didOpen`, `textDocument/didChange` and `textDocument/didClose` notifications is mandatory in the protocol and clients can not opt out supporting them. This includes both full and incremental synchronization in the `textDocument/didChange` notification. In addition a server must either implement all three of them or none. Their capabilities are therefore controlled via a combined client and server capability. Opting out of text document synchronization makes only sense if the documents shown by the client are read only. Otherwise the server might receive request for documents, for which the content is managed in the client (e.g. they might have changed).

Client Capability:

- property path (optional): `textDocument.synchronization.dynamicRegistration`
- property type: `boolean`

Controls whether text document synchronization supports dynamic registration.

Server Capability:

- property path (optional): `textDocumentSync`
- property type: `TextDocumentSyncKind | TextDocumentSyncOptions`. The below definition of the `TextDocumentSyncOptions` only covers the properties specific to the open, change and close notifications. A complete definition covering all properties can be found [here](#textDocument_didClose):


``` highlight
/**
 * Defines how the host (editor) should sync document changes to the language
 * server.
 */
export namespace TextDocumentSyncKind {
    /**
     * Documents should not be synced at all.
     */
    export const None = 0;

    /**
     * Documents are synced by always sending the full content
     * of the document.
     */
    export const Full = 1;

    /**
     * Documents are synced by sending the full content on open.
     * After that only incremental updates to the document are
     * sent.
     */
    export const Incremental = 2;
}

export type TextDocumentSyncKind = 0 | 1 | 2;
```


``` highlight
export interface TextDocumentSyncOptions {
    /**
     * Open and close notifications are sent to the server. If omitted open
     * close notifications should not be sent.
     */
    openClose?: boolean;

    /**
     * Change notifications are sent to the server. See
     * TextDocumentSyncKind.None, TextDocumentSyncKind.Full and
     * TextDocumentSyncKind.Incremental. If omitted it defaults to
     * TextDocumentSyncKind.None.
     */
    change?: TextDocumentSyncKind;
}
```


#### DidOpenTextDocument Notification (→)

The document open notification is sent from the client to the server to signal newly opened text documents. The document’s content is now managed by the client and the server must not try to read the document’s content using the document’s Uri. Open in this sense means it is managed by the client. It doesn’t necessarily mean that its content is presented in an editor. An open notification must not be sent more than once without a corresponding close notification send before. This means open and close notification must be balanced and the max open count for a particular textDocument is one. Note that a server’s ability to fulfill requests is independent of whether a text document is open or closed.

The `DidOpenTextDocumentParams` contain the language id the document is associated with. If the language id of a document changes, the client needs to send a `textDocument/didClose` to the server followed by a `textDocument/didOpen` with the new language id if the server handles the new language id as well.

*Client Capability*: See general synchronization [client capabilities](#textDocument_synchronization_cc).

*Server Capability*: See general synchronization [server capabilities](#textDocument_synchronization_sc).

*Registration Options*: [`TextDocumentRegistrationOptions`](#textDocumentRegistrationOptions)

*Notification*:

- method: ‘textDocument/didOpen’
- params: `DidOpenTextDocumentParams` defined as follows:


``` highlight
interface DidOpenTextDocumentParams {
    /**
     * The document that was opened.
     */
    textDocument: TextDocumentItem;
}
```


#### DidChangeTextDocument Notification (→)

The document change notification is sent from the client to the server to signal changes to a text document. Before a client can change a text document it must claim ownership of its content using the `textDocument/didOpen` notification. In 2.0 the shape of the params has changed to include proper version numbers.

Before requesting information from the server (e.g., `textDocument/completion` or `textDocument/signatureHelp`), the client must ensure that the document’s state is synchronized with the server to guarantee reliable results.

The following example shows how the client should synchronize the state when the user has continuous input, assuming user input triggered `textDocument/completion`:

| Document Version | User Input | Client Behavior | Request |
|----|----|----|----|
| 5 | document change one | sync document `v5` to the server | `textDocument/didChange` |
| 5 | \- | request from the server, based on document `v5` | `textDocument/completion` |
| 6 | document change two | sync document `v6` to the server | `textDocument/didChange` |

*Client Capability*: See general synchronization [client capabilities](#textDocument_synchronization_cc).

*Server Capability*: See general synchronization [server capabilities](#textDocument_synchronization_sc).

*Registration Options*: `TextDocumentChangeRegistrationOptions` defined as follows:


``` highlight
/**
 * Describe options to be used when registering for text document change events.
 */
export interface TextDocumentChangeRegistrationOptions
    extends TextDocumentRegistrationOptions {
    /**
     * How documents are synced to the server. See TextDocumentSyncKind.Full
     * and TextDocumentSyncKind.Incremental.
     */
    syncKind: TextDocumentSyncKind;
}
```


*Notification*:

- method: `textDocument/didChange`
- params: `DidChangeTextDocumentParams` defined as follows:


``` highlight
interface DidChangeTextDocumentParams {
    /**
     * The document that did change. The version number points
     * to the version after all provided content changes have
     * been applied.
     */
    textDocument: VersionedTextDocumentIdentifier;

    /**
     * The actual content changes. The content changes describe single state
     * changes to the document. So if there are two content changes c1 (at
     * array index 0) and c2 (at array index 1) for a document in state S then
     * c1 moves the document from S to S' and c2 from S' to S''. So c1 is
     * computed on the state S and c2 is computed on the state S'.
     *
     * To mirror the content of a document using change events use the following
     * approach:
     * - start with the same initial content
     * - apply the 'textDocument/didChange' notifications in the order you
     *   receive them.
     * - apply the `TextDocumentContentChangeEvent`s in a single notification
     *   in the order you receive them.
     */
    contentChanges: TextDocumentContentChangeEvent[];
}
```


``` highlight
/**
 * An event describing a change to a text document. If only a text is provided
 * it is considered to be the full content of the document.
 */
export type TextDocumentContentChangeEvent = {
    /**
     * The range of the document that changed.
     */
    range: Range;

    /**
     * The optional length of the range that got replaced.
     *
     * @deprecated use range instead.
     */
    rangeLength?: uinteger;

    /**
     * The new text for the provided range.
     */
    text: string;
} | {
    /**
     * The new text of the whole document.
     */
    text: string;
};
```


#### WillSaveTextDocument Notification (→)

The document will save notification is sent from the client to the server before the document is actually saved. If a server has registered for open / close events clients should ensure that the document is open before a `willSave` notification is sent since clients can’t change the content of a file without ownership transferal.

*Client Capability*:

- property name (optional): `textDocument.synchronization.willSave`
- property type: `boolean`

The capability indicates that the client supports `textDocument/willSave` notifications.

*Server Capability*:

- property name (optional): `textDocumentSync.willSave`
- property type: `boolean`

The capability indicates that the server is interested in `textDocument/willSave` notifications.

*Registration Options*: `TextDocumentRegistrationOptions`

*Notification*:

- method: ‘textDocument/willSave’
- params: `WillSaveTextDocumentParams` defined as follows:


``` highlight
/**
 * The parameters send in a will save text document notification.
 */
export interface WillSaveTextDocumentParams {
    /**
     * The document that will be saved.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The 'TextDocumentSaveReason'.
     */
    reason: TextDocumentSaveReason;
}
```


``` highlight
/**
 * Represents reasons why a text document is saved.
 */
export namespace TextDocumentSaveReason {

    /**
     * Manually triggered, e.g. by the user pressing save, by starting
     * debugging, or by an API call.
     */
    export const Manual = 1;

    /**
     * Automatic after a delay.
     */
    export const AfterDelay = 2;

    /**
     * When the editor lost focus.
     */
    export const FocusOut = 3;
}

export type TextDocumentSaveReason = 1 | 2 | 3;
```


#### WillSaveWaitUntilTextDocument Request (↩)

The document will save request is sent from the client to the server before the document is actually saved. The request can return an array of TextEdits which will be applied to the text document before it is saved. Please note that clients might drop results if computing the text edits took too long or if a server constantly fails on this request. This is done to keep the save fast and reliable. If a server has registered for open / close events clients should ensure that the document is open before a `willSaveWaitUntil` notification is sent since clients can’t change the content of a file without ownership transferal.

*Client Capability*:

- property name (optional): `textDocument.synchronization.willSaveWaitUntil`
- property type: `boolean`

The capability indicates that the client supports `textDocument/willSaveWaitUntil` requests.

*Server Capability*:

- property name (optional): `textDocumentSync.willSaveWaitUntil`
- property type: `boolean`

The capability indicates that the server is interested in `textDocument/willSaveWaitUntil` requests.

*Registration Options*: `TextDocumentRegistrationOptions`

*Request*:

- method: `textDocument/willSaveWaitUntil`
- params: `WillSaveTextDocumentParams`

*Response*:

- result: [`TextEdit[]`](#textEdit) \| `null`
- error: code and message set in case an exception happens during the `textDocument/willSaveWaitUntil` request.

#### DidSaveTextDocument Notification (→)

The document save notification is sent from the client to the server when the document was saved in the client.

*Client Capability*:

- property name (optional): `textDocument.synchronization.didSave`
- property type: `boolean`

The capability indicates that the client supports `textDocument/didSave` notifications.

*Server Capability*:

- property name (optional): `textDocumentSync.save`
- property type: `boolean | SaveOptions` where `SaveOptions` is defined as follows:


``` highlight
export interface SaveOptions {
    /**
     * The client is supposed to include the content on save.
     */
    includeText?: boolean;
}
```


The capability indicates that the server is interested in `textDocument/didSave` notifications.

*Registration Options*: `TextDocumentSaveRegistrationOptions` defined as follows:


``` highlight
export interface TextDocumentSaveRegistrationOptions
    extends TextDocumentRegistrationOptions {
    /**
     * The client is supposed to include the content on save.
     */
    includeText?: boolean;
}
```


*Notification*:

- method: `textDocument/didSave`
- params: `DidSaveTextDocumentParams` defined as follows:


``` highlight
interface DidSaveTextDocumentParams {
    /**
     * The document that was saved.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * Optional the content when saved. Depends on the includeText value
     * when the save notification was requested.
     */
    text?: string;
}
```


#### DidCloseTextDocument Notification (→)

The document close notification is sent from the client to the server when the document got closed in the client. The document’s master now exists where the document’s Uri points to (e.g. if the document’s Uri is a file Uri the master now exists on disk). As with the open notification the close notification is about managing the document’s content. Receiving a close notification doesn’t mean that the document was open in an editor before. A close notification requires a previous open notification to be sent. Note that a server’s ability to fulfill requests is independent of whether a text document is open or closed.

*Client Capability*: See general synchronization [client capabilities](#textDocument_synchronization_cc).

*Server Capability*: See general synchronization [server capabilities](#textDocument_synchronization_sc).

*Registration Options*: `TextDocumentRegistrationOptions`

*Notification*:

- method: `textDocument/didClose`
- params: `DidCloseTextDocumentParams` defined as follows:


``` highlight
interface DidCloseTextDocumentParams {
    /**
     * The document that was closed.
     */
    textDocument: TextDocumentIdentifier;
}
```


#### Renaming a document

Document renames should be signaled to a server sending a document close notification with the document’s old name followed by an open notification using the document’s new name. Major reason is that besides the name other attributes can change as well like the language that is associated with the document. In addition the new document could not be of interest for the server anymore.

Servers can participate in a document rename by subscribing for the [`workspace/didRenameFiles`](#workspace_didRenameFiles) notification or the [`workspace/willRenameFiles`](#workspace_willRenameFiles) request.

The final structure of the `TextDocumentSyncClientCapabilities` and the `TextDocumentSyncOptions` server options look like this


``` highlight
export interface TextDocumentSyncClientCapabilities {
    /**
     * Whether text document synchronization supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports sending will save notifications.
     */
    willSave?: boolean;

    /**
     * The client supports sending a will save request and
     * waits for a response providing text edits which will
     * be applied to the document before it is saved.
     */
    willSaveWaitUntil?: boolean;

    /**
     * The client supports did save notifications.
     */
    didSave?: boolean;
}
```


``` highlight
export interface TextDocumentSyncOptions {
    /**
     * Open and close notifications are sent to the server. If omitted open
     * close notification should not be sent.
     */
    openClose?: boolean;
    /**
     * Change notifications are sent to the server. See
     * TextDocumentSyncKind.None, TextDocumentSyncKind.Full and
     * TextDocumentSyncKind.Incremental. If omitted it defaults to
     * TextDocumentSyncKind.None.
     */
    change?: TextDocumentSyncKind;
    /**
     * If present will save notifications are sent to the server. If omitted
     * the notification should not be sent.
     */
    willSave?: boolean;
    /**
     * If present will save wait until requests are sent to the server. If
     * omitted the request should not be sent.
     */
    willSaveWaitUntil?: boolean;
    /**
     * If present save notifications are sent to the server. If omitted the
     * notification should not be sent.
     */
    save?: boolean | SaveOptions;
}
```


### Notebook Document Synchronization

Notebooks are becoming more and more popular. Adding support for them to the language server protocol allows notebook editors to reuse language smarts provided by the server inside a notebook or a notebook cell, respectively. To reuse protocol parts and therefore server implementations notebooks are modeled in the following way in LSP:

- *notebook document*: a collection of notebook cells typically stored in a file on disk. A notebook document has a type and can be uniquely identified using a resource URI.
- *notebook cell*: holds the actual text content. Cells have a kind (either code or markdown). The actual text content of the cell is stored in a text document which can be synced to the server like all other text documents. Cell text documents have an URI however servers should not rely on any format for this URI since it is up to the client on how it will create these URIs. The URIs must be unique across ALL notebook cells and can therefore be used to uniquely identify a notebook cell or the cell’s text document.

The two concepts are defined as follows:


``` highlight
/**
 * A notebook document.
 *
 * @since 3.17.0
 */
export interface NotebookDocument {

    /**
     * The notebook document's URI.
     */
    uri: URI;

    /**
     * The type of the notebook.
     */
    notebookType: string;

    /**
     * The version number of this document (it will increase after each
     * change, including undo/redo).
     */
    version: integer;

    /**
     * Additional metadata stored with the notebook
     * document.
     */
    metadata?: LSPObject;

    /**
     * The cells of a notebook.
     */
    cells: NotebookCell[];
}
```


``` highlight
/**
 * A notebook cell.
 *
 * A cell's document URI must be unique across ALL notebook
 * cells and can therefore be used to uniquely identify a
 * notebook cell or the cell's text document.
 *
 * @since 3.17.0
 */
export interface NotebookCell {

    /**
     * The cell's kind
     */
    kind: NotebookCellKind;

    /**
     * The URI of the cell's text document
     * content.
     */
    document: DocumentUri;

    /**
     * Additional metadata stored with the cell.
     */
    metadata?: LSPObject;

    /**
     * Additional execution summary information
     * if supported by the client.
     */
    executionSummary?: ExecutionSummary;
}
```


``` highlight
/**
 * A notebook cell kind.
 *
 * @since 3.17.0
 */
export namespace NotebookCellKind {

    /**
     * A markup-cell is formatted source that is used for display.
     */
    export const Markup: 1 = 1;

    /**
     * A code-cell is source code.
     */
    export const Code: 2 = 2;
}
```


``` highlight
export interface ExecutionSummary {
    /**
     * A strict monotonically increasing value
     * indicating the execution order of a cell
     * inside a notebook.
     */
    executionOrder: uinteger;

    /**
     * Whether the execution was successful or
     * not if known by the client.
     */
    success?: boolean;
}
```


Next we describe how notebooks, notebook cells and the content of a notebook cell should be synchronized to a language server.

Syncing the text content of a cell is relatively easy since clients should model them as text documents. However since the URI of a notebook cell’s text document should be opaque, servers can not know its scheme nor its path. However what is know is the notebook document itself. We therefore introduce a special filter for notebook cell documents:


``` highlight
/**
 * A notebook cell text document filter denotes a cell text
 * document by different properties.
 *
 * @since 3.17.0
 */
export interface NotebookCellTextDocumentFilter {
    /**
     * A filter that matches against the notebook
     * containing the notebook cell. If a string
     * value is provided it matches against the
     * notebook type. '*' matches every notebook.
     */
    notebook: string | NotebookDocumentFilter;

    /**
     * A language id like `python`.
     *
     * Will be matched against the language id of the
     * notebook cell document. '*' matches every language.
     */
    language?: string;
}
```


``` highlight
/**
 * A notebook document filter denotes a notebook document by
 * different properties.
 *
 * @since 3.17.0
 */
export type NotebookDocumentFilter = {
    /** The type of the enclosing notebook. */
    notebookType: string;

    /** A Uri scheme, like `file` or `untitled`. */
    scheme?: string;

    /** A glob pattern. */
    pattern?: string;
} | {
    /** The type of the enclosing notebook. */
    notebookType?: string;

    /** A Uri scheme, like `file` or `untitled`.*/
    scheme: string;

    /** A glob pattern. */
    pattern?: string;
} | {
    /** The type of the enclosing notebook. */
    notebookType?: string;

    /** A Uri scheme, like `file` or `untitled`. */
    scheme?: string;

    /** A glob pattern. */
    pattern: string;
};
```


Given these structures a Python cell document in a Jupyter notebook stored on disk in a folder having `books1` in its path can be identified as follows;


``` highlight
{
    notebook: {
        scheme: 'file',
        pattern '**/books1/**',
        notebookType: 'jupyter-notebook'
    },
    language: 'python'
}
```


A `NotebookCellTextDocumentFilter` can be used to register providers for certain requests like code complete or hover. If such a provider is registered the client will send the corresponding `textDocument/*` requests to the server using the cell text document’s URI as the document URI.

There are cases where simply only knowing about a cell’s text content is not enough for a server to reason about the cells content and to provide good language smarts. Sometimes it is necessary to know all cells of a notebook document including the notebook document itself. Consider a notebook that has two JavaScript cells with the following content

Cell one:


``` highlight
function add(a, b) {
    return a + b;
}
```


Cell two:


``` highlight
add/*<cursor>*/;
```


Requesting code assist in cell two at the marked cursor position should propose the function `add` which is only possible if the server knows about cell one and cell two and knows that they belong to the same notebook document.

The protocol will therefore support two modes when it comes to synchronizing cell text content:

- *cellContent*: in this mode only the cell text content is synchronized to the server using the standard `textDocument/did*` notification. No notebook document and no cell structure is synchronized. This mode allows for easy adoption of notebooks since servers can reuse most of it implementation logic.
- *notebook*: in this mode the notebook document, the notebook cells and the notebook cell text content is synchronized to the server. To allow servers to create a consistent picture of a notebook document the cell text content is NOT synchronized using the standard `textDocument/did*` notifications. It is instead synchronized using special `notebookDocument/did*` notifications. This ensures that the cell and its text content arrives on the server using one open, change or close event.

In both modes, notebook cell text documents are treated as regular text documents. They are always synchronized using incremental sync.

To request the cell content only a normal document selector can be used. For example the selector `[{ language: 'python' }]` will synchronize Python notebook document cells to the server. However since this might synchronize unwanted documents as well a document filter can also be a `NotebookCellTextDocumentFilter`. So `{ notebook: { scheme: 'file', notebookType: 'jupyter-notebook' }, language: 'python' }` synchronizes all Python cells in a Jupyter notebook stored on disk.

To synchronize the whole notebook document a server provides a `notebookDocumentSync` in its server capabilities. For example:


``` highlight
{
    notebookDocumentSync: {
        notebookSelector: [
            {
                notebook: { scheme: 'file', notebookType: 'jupyter-notebook' },
                cells: [{ language: 'python' }]
            }
        ]
    }
}
```


Synchronizes the notebook including all Python cells to the server if the notebook is stored on disk.

*Client Capability*:

The following client capabilities are defined for notebook documents:

- property name (optional): `notebookDocument.synchronization`
- property type: `NotebookDocumentSyncClientCapabilities` defined as follows


``` highlight
/**
 * Notebook specific client capabilities.
 *
 * @since 3.17.0
 */
export interface NotebookDocumentSyncClientCapabilities {

    /**
     * Whether implementation supports dynamic registration. If this is
     * set to `true` the client supports the new
     * `(NotebookDocumentSyncRegistrationOptions & NotebookDocumentSyncOptions)`
     * return value for the corresponding server capability as well.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports sending execution summary data per cell.
     */
    executionSummarySupport?: boolean;
}
```


*Server Capability*:

The following server capabilities are defined for notebook documents:

- property name (optional): `notebookDocumentSync`
- property type: `NotebookDocumentSyncOptions | NotebookDocumentSyncRegistrationOptions` where `NotebookDocumentOptions` is defined as follows:


``` highlight
/**
 * Options specific to a notebook plus its cells
 * to be synced to the server.
 *
 * If a selector provides a notebook document
 * filter but no cell selector all cells of a
 * matching notebook document will be synced.
 *
 * If a selector provides no notebook document
 * filter but only a cell selector all notebook
 * documents that contain at least one matching
 * cell will be synced.
 *
 * @since 3.17.0
 */
export interface NotebookDocumentSyncOptions {
    /**
     * The notebooks to be synced
     */
    notebookSelector: ({
        /**
         * The notebook to be synced. If a string
         * value is provided it matches against the
         * notebook type. '*' matches every notebook.
         */
        notebook: string | NotebookDocumentFilter;

        /**
         * The cells of the matching notebook to be synced.
         */
        cells?: { language: string }[];
    } | {
        /**
         * The notebook to be synced. If a string
         * value is provided it matches against the
         * notebook type. '*' matches every notebook.
         */
        notebook?: string | NotebookDocumentFilter;

        /**
         * The cells of the matching notebook to be synced.
         */
        cells: { language: string }[];
    })[];

    /**
     * Whether save notification should be forwarded to
     * the server. Will only be honored if mode === `notebook`.
     */
    save?: boolean;
}
```


*Registration Options*: `notebookDocumentSyncRegistrationOptions` defined as follows:


``` highlight
/**
 * Registration options specific to a notebook.
 *
 * @since 3.17.0
 */
export interface NotebookDocumentSyncRegistrationOptions extends
    NotebookDocumentSyncOptions, StaticRegistrationOptions {
}
```


#### DidOpenNotebookDocument Notification (→)

The open notification is sent from the client to the server when a notebook document is opened. It is only sent by a client if the server requested the synchronization mode `notebook` in its `notebookDocumentSync` capability.

*Notification*:

- method: `notebookDocument/didOpen`
- params: `DidOpenNotebookDocumentParams` defined as follows:


``` highlight
/**
 * The params sent in an open notebook document notification.
 *
 * @since 3.17.0
 */
export interface DidOpenNotebookDocumentParams {

    /**
     * The notebook document that got opened.
     */
    notebookDocument: NotebookDocument;

    /**
     * The text documents that represent the content
     * of a notebook cell.
     */
    cellTextDocuments: TextDocumentItem[];
}
```


#### DidChangeNotebookDocument Notification (→)

The change notification is sent from the client to the server when a notebook document changes. It is only sent by a client if the server requested the synchronization mode `notebook` in its `notebookDocumentSync` capability.

*Notification*:

- method: `notebookDocument/didChange`
- params: `DidChangeNotebookDocumentParams` defined as follows:


``` highlight
/**
 * The params sent in a change notebook document notification.
 *
 * @since 3.17.0
 */
export interface DidChangeNotebookDocumentParams {

    /**
     * The notebook document that did change. The version number points
     * to the version after all provided changes have been applied.
     */
    notebookDocument: VersionedNotebookDocumentIdentifier;

    /**
     * The actual changes to the notebook document.
     *
     * The change describes single state change to the notebook document.
     * So it moves a notebook document, its cells and its cell text document
     * contents from state S to S'.
     *
     * To mirror the content of a notebook using change events use the
     * following approach:
     * - start with the same initial content
     * - apply the 'notebookDocument/didChange' notifications in the order
     *   you receive them.
     */
    change: NotebookDocumentChangeEvent;
}
```


``` highlight
/**
 * A versioned notebook document identifier.
 *
 * @since 3.17.0
 */
export interface VersionedNotebookDocumentIdentifier {

    /**
     * The version number of this notebook document.
     */
    version: integer;

    /**
     * The notebook document's URI.
     */
    uri: URI;
}
```


``` highlight
/**
 * A change event for a notebook document.
 *
 * @since 3.17.0
 */
export interface NotebookDocumentChangeEvent {
    /**
     * The changed meta data if any.
     */
    metadata?: LSPObject;

    /**
     * Changes to cells
     */
    cells?: {
        /**
         * Changes to the cell structure to add or
         * remove cells.
         */
        structure?: {
            /**
             * The change to the cell array.
             */
            array: NotebookCellArrayChange;

            /**
             * Additional opened cell text documents.
             */
            didOpen?: TextDocumentItem[];

            /**
             * Additional closed cell text documents.
             */
            didClose?: TextDocumentIdentifier[];
        };

        /**
         * Changes to notebook cells properties like its
         * kind, execution summary or metadata.
         */
        data?: NotebookCell[];

        /**
         * Changes to the text content of notebook cells.
         */
        textContent?: {
            document: VersionedTextDocumentIdentifier;
            changes: TextDocumentContentChangeEvent[];
        }[];
    };
}
```


``` highlight
/**
 * A change describing how to move a `NotebookCell`
 * array from state S to S'.
 *
 * @since 3.17.0
 */
export interface NotebookCellArrayChange {
    /**
     * The start offset of the cell that changed.
     */
    start: uinteger;

    /**
     * The deleted cells
     */
    deleteCount: uinteger;

    /**
     * The new cells, if any
     */
    cells?: NotebookCell[];
}
```


#### DidSaveNotebookDocument Notification (→)

The save notification is sent from the client to the server when a notebook document is saved. It is only sent by a client if the server requested the synchronization mode `notebook` in its `notebookDocumentSync` capability.

*Notification*:


- method: `notebookDocument/didSave`
- params: `DidSaveNotebookDocumentParams` defined as follows:


``` highlight
/**
 * The params sent in a save notebook document notification.
 *
 * @since 3.17.0
 */
export interface DidSaveNotebookDocumentParams {
    /**
     * The notebook document that got saved.
     */
    notebookDocument: NotebookDocumentIdentifier;
}
```


#### DidCloseNotebookDocument Notification (→)

The close notification is sent from the client to the server when a notebook document is closed. It is only sent by a client if the server requested the synchronization mode `notebook` in its `notebookDocumentSync` capability.

*Notification*:


- method: `notebookDocument/didClose`
- params: `DidCloseNotebookDocumentParams` defined as follows:


``` highlight
/**
 * The params sent in a close notebook document notification.
 *
 * @since 3.17.0
 */
export interface DidCloseNotebookDocumentParams {

    /**
     * The notebook document that got closed.
     */
    notebookDocument: NotebookDocumentIdentifier;

    /**
     * The text documents that represent the content
     * of a notebook cell that got closed.
     */
    cellTextDocuments: TextDocumentIdentifier[];
}
```


``` highlight
/**
 * A literal to identify a notebook document in the client.
 *
 * @since 3.17.0
 */
export interface NotebookDocumentIdentifier {
    /**
     * The notebook document's URI.
     */
    uri: URI;
}
```

## Related

- [[Workspace Model]] — how `Doc` is created and updated when sync events arrive
- [[Data Flow]] — document sync events in the request pipeline
