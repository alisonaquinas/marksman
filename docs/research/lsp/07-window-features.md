---
title: "LSP 3.17 — Window Features"
source: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
date: 2026-04-16
tags:
  - wiki/input
  - research/lsp
---

### Window Features

#### ShowMessage Notification (←)

The show message notification is sent from a server to a client to ask the client to display a particular message in the user interface.

*Notification*:

- method: ‘window/showMessage’
- params: `ShowMessageParams` defined as follows:


``` highlight
interface ShowMessageParams {
    /**
     * The message type. See {@link MessageType}.
     */
    type: MessageType;

    /**
     * The actual message.
     */
    message: string;
}
```


Where the type is defined as follows:


``` highlight
export namespace MessageType {
    /**
     * An error message.
     */
    export const Error = 1;
    /**
     * A warning message.
     */
    export const Warning = 2;
    /**
     * An information message.
     */
    export const Info = 3;
    /**
     * A log message.
     */
    export const Log = 4;
    /**
     * A debug message.
     *
     * @since 3.18.0
     * @proposed
     */
    export const Debug = 5;
}

export type MessageType = 1 | 2 | 3 | 4 | 5;
```


#### ShowMessage Request (↪)

The show message request is sent from a server to a client to ask the client to display a particular message in the user interface. In addition to the show message notification the request allows to pass actions and to wait for an answer from the client.

*Client Capability*:

- property path (optional): `window.showMessage`
- property type: `ShowMessageRequestClientCapabilities` defined as follows:


``` highlight
/**
 * Show message request client capabilities
 */
export interface ShowMessageRequestClientCapabilities {
    /**
     * Capabilities specific to the `MessageActionItem` type.
     */
    messageActionItem?: {
        /**
         * Whether the client supports additional attributes which
         * are preserved and sent back to the server in the
         * request's response.
         */
        additionalPropertiesSupport?: boolean;
    };
}
```


*Request*:

- method: ‘window/showMessageRequest’
- params: `ShowMessageRequestParams` defined as follows:


``` highlight
interface ShowMessageRequestParams {
    /**
     * The message type. See {@link MessageType}
     */
    type: MessageType;

    /**
     * The actual message
     */
    message: string;

    /**
     * The message action items to present.
     */
    actions?: MessageActionItem[];
}
```


Where the `MessageActionItem` is defined as follows:


``` highlight
interface MessageActionItem {
    /**
     * A short title like 'Retry', 'Open Log' etc.
     */
    title: string;
}
```


*Response*:

- result: the selected `MessageActionItem` \| `null` if none got selected.
- error: code and message set in case an exception happens during showing a message.

#### Show Document Request (↪)

> New in version 3.16.0

The show document request is sent from a server to a client to ask the client to display a particular resource referenced by a URI in the user interface.

*Client Capability*:

- property path (optional): `window.showDocument`
- property type: `ShowDocumentClientCapabilities` defined as follows:


``` highlight
/**
 * Client capabilities for the show document request.
 *
 * @since 3.16.0
 */
export interface ShowDocumentClientCapabilities {
    /**
     * The client has support for the show document
     * request.
     */
    support: boolean;
}
```


*Request*:

- method: ‘window/showDocument’
- params: `ShowDocumentParams` defined as follows:


``` highlight
/**
 * Params to show a resource.
 *
 * @since 3.16.0
 */
export interface ShowDocumentParams {
    /**
     * The uri to show.
     */
    uri: URI;

    /**
     * Indicates to show the resource in an external program.
     * To show, for example, `https://code.visualstudio.com/`
     * in the default WEB browser set `external` to `true`.
     */
    external?: boolean;

    /**
     * An optional property to indicate whether the editor
     * showing the document should take focus or not.
     * Clients might ignore this property if an external
     * program is started.
     */
    takeFocus?: boolean;

    /**
     * An optional selection range if the document is a text
     * document. Clients might ignore the property if an
     * external program is started or the file is not a text
     * file.
     */
    selection?: Range;
}
```


*Response*:

- result: `ShowDocumentResult` defined as follows:


``` highlight
/**
 * The result of an show document request.
 *
 * @since 3.16.0
 */
export interface ShowDocumentResult {
    /**
     * A boolean indicating if the show was successful.
     */
    success: boolean;
}
```


- error: code and message set in case an exception happens during showing a document.

#### LogMessage Notification (←)

The log message notification is sent from the server to the client to ask the client to log a particular message.

*Notification*:

- method: ‘window/logMessage’
- params: `LogMessageParams` defined as follows:


``` highlight
interface LogMessageParams {
    /**
     * The message type. See {@link MessageType}
     */
    type: MessageType;

    /**
     * The actual message
     */
    message: string;
}
```


#### Create Work Done Progress (↪)

The `window/workDoneProgress/create` request is sent from the server to the client to ask the client to create a work done progress.

*Client Capability*:

- property name (optional): `window.workDoneProgress`
- property type: `boolean`

*Request*:

- method: ‘window/workDoneProgress/create’
- params: `WorkDoneProgressCreateParams` defined as follows:


``` highlight
export interface WorkDoneProgressCreateParams {
    /**
     * The token to be used to report progress.
     */
    token: ProgressToken;
}
```


*Response*:

- result: void
- error: code and message set in case an exception happens during the ‘window/workDoneProgress/create’ request. In case an error occurs a server must not send any progress notification using the token provided in the `WorkDoneProgressCreateParams`.

#### Cancel a Work Done Progress (→)

The `window/workDoneProgress/cancel` notification is sent from the client to the server to cancel a progress initiated on the server side using the `window/workDoneProgress/create`. The progress need not be marked as `cancellable` to be cancelled and a client may cancel a progress for any number of reasons: in case of error, reloading a workspace etc.

*Notification*:

- method: ‘window/workDoneProgress/cancel’
- params: `WorkDoneProgressCancelParams` defined as follows:


``` highlight
export interface WorkDoneProgressCancelParams {
    /**
     * The token to be used to report progress.
     */
    token: ProgressToken;
}
```


#### Telemetry Notification (←)

The telemetry notification is sent from the server to the client to ask the client to log a telemetry event. The protocol doesn’t specify the payload since no interpretation of the data happens in the protocol. Most clients even don’t handle the event directly but forward them to the extensions owing the corresponding server issuing the event.

*Notification*:

- method: ‘telemetry/event’
- params: ‘object’ \| ‘array’;

#### Miscellaneous

#### Implementation Considerations

Language servers usually run in a separate process and clients communicate with them in an asynchronous fashion. Additionally clients usually allow users to interact with the source code even if request results are pending. We recommend the following implementation pattern to avoid that clients apply outdated response results:

- if a client sends a request to the server and the client state changes in a way that it invalidates the response it should do the following:
  - cancel the server request and ignore the result if the result is not useful for the client anymore. If necessary the client should resend the request.
  - keep the request running if the client can still make use of the result by, for example, transforming it to a new result by applying the state change to the result.
- servers should therefore not decide by themselves to cancel requests simply due to that fact that a state change notification is detected in the queue. As said the result could still be useful for the client.
- if a server detects an internal state change (for example, a project context changed) that invalidates the result of a request in execution the server can error these requests with `ContentModified`. If clients receive a `ContentModified` error, it generally should not show it in the UI for the end-user. Clients can resend the request if they know how to do so. It should be noted that for all position based requests it might be especially hard for clients to re-craft a request.
- a client should not send resolve requests for out of date objects (for example, code lenses, …). If a server receives a resolve request for an out of date object the server can error these requests with `ContentModified`.
- if a client notices that a server exits unexpectedly, it should try to restart the server. However clients should be careful not to restart a crashing server endlessly. VS Code, for example, doesn’t restart a server which has crashed 5 times in the last 180 seconds.

Servers usually support different communication channels (e.g. stdio, pipes, …). To ease the usage of servers in different clients it is highly recommended that a server implementation supports the following command line arguments to pick the communication channel:

- **stdio**: uses stdio as the communication channel.
- **pipe**: use pipes (Windows) or socket files (Linux, Mac) as the communication channel. The pipe / socket file name is passed as the next arg or with `--pipe=`.
- **socket**: uses a socket as the communication channel. The port is passed as next arg or with `--port=`.
- **node-ipc**: use node IPC communication between the client and the server. This is only supported if both client and server run under node.

To support the case that the editor starting a server crashes an editor should also pass its process id to the server. This allows the server to monitor the editor process and to shutdown itself if the editor process dies. The process id passed on the command line should be the same as the one passed in the initialize parameters. The command line argument to use is `--clientProcessId`.

#### Meta Model

Since 3.17 there is a meta model describing the LSP protocol:

- [metaModel.json](../metaModel/metaModel.json): The actual meta model for the LSP 3.17 specification
- [metaModel.ts](../metaModel/metaModel.ts): A TypeScript file defining the data types that make up the meta model.
- [metaModel.schema.json](../metaModel/metaModel.schema.json): A JSON schema file defining the data types that make up the meta model. Can be used to generate code to read the meta model JSON file.

