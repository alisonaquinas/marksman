---
title: "LSP 3.17 — Workspace Features"
source: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
date: 2026-04-16
tags:
  - wiki/input
  - research/lsp
---

### Workspace Features

#### Workspace Symbols Request (↩)

The workspace symbol request is sent from the client to the server to list project-wide symbols matching the query string. Since 3.17.0 servers can also provide a handler for `workspaceSymbol/resolve` requests. This allows servers to return workspace symbols without a range for a `workspace/symbol` request. Clients then need to resolve the range when necessary using the `workspaceSymbol/resolve` request. Servers can only use this new model if clients advertise support for it via the `workspace.symbol.resolveSupport` capability.

*Client Capability*:

- property path (optional): `workspace.symbol`
- property type: `WorkspaceSymbolClientCapabilities` defined as follows:


``` highlight
interface WorkspaceSymbolClientCapabilities {
    /**
     * Symbol request supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * Specific capabilities for the `SymbolKind` in the `workspace/symbol`
     * request.
     */
    symbolKind?: {
        /**
         * The symbol kind values the client supports. When this
         * property exists the client also guarantees that it will
         * handle values outside its set gracefully and falls back
         * to a default value when unknown.
         *
         * If this property is not present the client only supports
         * the symbol kinds from `File` to `Array` as defined in
         * the initial version of the protocol.
         */
        valueSet?: SymbolKind[];
    };

    /**
     * The client supports tags on `SymbolInformation` and `WorkspaceSymbol`.
     * Clients supporting tags have to handle unknown tags gracefully.
     *
     * @since 3.16.0
     */
    tagSupport?: {
        /**
         * The tags supported by the client.
         */
        valueSet: SymbolTag[];
    };

    /**
     * The client support partial workspace symbols. The client will send the
     * request `workspaceSymbol/resolve` to the server to resolve additional
     * properties.
     *
     * @since 3.17.0 - proposedState
     */
    resolveSupport?: {
        /**
         * The properties that a client can resolve lazily. Usually
         * `location.range`
         */
        properties: string[];
    };
}
```


*Server Capability*:

- property path (optional): `workspaceSymbolProvider`
- property type: `boolean | WorkspaceSymbolOptions` where `WorkspaceSymbolOptions` is defined as follows:


``` highlight
export interface WorkspaceSymbolOptions extends WorkDoneProgressOptions {
    /**
     * The server provides support to resolve additional
     * information for a workspace symbol.
     *
     * @since 3.17.0
     */
    resolveProvider?: boolean;
}
```


*Registration Options*: `WorkspaceSymbolRegistrationOptions` defined as follows:


``` highlight
export interface WorkspaceSymbolRegistrationOptions
    extends WorkspaceSymbolOptions {
}
```


*Request*:

- method: ‘workspace/symbol’
- params: `WorkspaceSymbolParams` defined as follows:


``` highlight
/**
 * The parameters of a Workspace Symbol Request.
 */
interface WorkspaceSymbolParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * A query string to filter symbols by. Clients may send an empty
     * string here to request all symbols.
     */
    query: string;
}
```


*Response*:

- result: `SymbolInformation[]` \| `WorkspaceSymbol[]` \| `null`. See above for the definition of `SymbolInformation`. It is recommended that you use the new `WorkspaceSymbol`. However whether the workspace symbol can return a location without a range depends on the client capability `workspace.symbol.resolveSupport`. `WorkspaceSymbol`which is defined as follows:


``` highlight
/**
 * A special workspace symbol that supports locations without a range
 *
 * @since 3.17.0
 */
export interface WorkspaceSymbol {
    /**
     * The name of this symbol.
     */
    name: string;

    /**
     * The kind of this symbol.
     */
    kind: SymbolKind;

    /**
     * Tags for this completion item.
     */
    tags?: SymbolTag[];

    /**
     * The name of the symbol containing this symbol. This information is for
     * user interface purposes (e.g. to render a qualifier in the user interface
     * if necessary). It can't be used to re-infer a hierarchy for the document
     * symbols.
     */
    containerName?: string;

    /**
     * The location of this symbol. Whether a server is allowed to
     * return a location without a range depends on the client
     * capability `workspace.symbol.resolveSupport`.
     *
     * See also `SymbolInformation.location`.
     */
    location: Location | { uri: DocumentUri };

    /**
     * A data entry field that is preserved on a workspace symbol between a
     * workspace symbol request and a workspace symbol resolve request.
     */
    data?: LSPAny;
}
```


- partial result: `SymbolInformation[]` \| `WorkspaceSymbol[]` as defined above.
- error: code and message set in case an exception happens during the workspace symbol request.

#### Workspace Symbol Resolve Request (↩)

The request is sent from the client to the server to resolve additional information for a given workspace symbol.

*Request*:

- method: ‘workspaceSymbol/resolve’
- params: `WorkspaceSymbol`

*Response*:

- result: `WorkspaceSymbol`
- error: code and message set in case an exception happens during the workspace symbol resolve request.

#### Configuration Request (↪)

> *Since version 3.6.0*

The `workspace/configuration` request is sent from the server to the client to fetch configuration settings from the client. The request can fetch several configuration settings in one roundtrip. The order of the returned configuration settings correspond to the order of the passed `ConfigurationItems` (e.g. the first item in the response is the result for the first configuration item in the params).

A `ConfigurationItem` consists of the configuration section to ask for and an additional scope URI. The configuration section asked for is defined by the server and doesn’t necessarily need to correspond to the configuration store used by the client. So a server might ask for a configuration `cpp.formatterOptions` but the client stores the configuration in an XML store layout differently. It is up to the client to do the necessary conversion. If a scope URI is provided the client should return the setting scoped to the provided resource. If the client for example uses [EditorConfig](http://editorconfig.org/) to manage its settings the configuration should be returned for the passed resource URI. If the client can’t provide a configuration setting for a given scope then `null` needs to be present in the returned array.

This pull model replaces the old push model were the client signaled configuration change via an event. If the server still needs to react to configuration changes (since the server caches the result of `workspace/configuration` requests) the server should register for an empty configuration change using the following registration pattern:


``` highlight
connection.client.register(DidChangeConfigurationNotification.type, undefined);
```


*Client Capability*:

- property path (optional): `workspace.configuration`
- property type: `boolean`

*Request*:

- method: ‘workspace/configuration’
- params: `ConfigurationParams` defined as follows


``` highlight
export interface ConfigurationParams {
    items: ConfigurationItem[];
}
```


``` highlight
export interface ConfigurationItem {
    /**
     * The scope to get the configuration section for.
     */
    scopeUri?: URI;

    /**
     * The configuration section asked for.
     */
    section?: string;
}
```


*Response*:

- result: LSPAny\[\]
- error: code and message set in case an exception happens during the ‘workspace/configuration’ request

#### DidChangeConfiguration Notification (→)

A notification sent from the client to the server to signal the change of configuration settings.

*Client Capability*:

- property path (optional): `workspace.didChangeConfiguration`
- property type: `DidChangeConfigurationClientCapabilities` defined as follows:


``` highlight
export interface DidChangeConfigurationClientCapabilities {
    /**
     * Did change configuration notification supports dynamic registration.
     *
     * @since 3.6.0 to support the new pull model.
     */
    dynamicRegistration?: boolean;
}
```


*Notification*:

- method: ‘workspace/didChangeConfiguration’,
- params: `DidChangeConfigurationParams` defined as follows:


``` highlight
interface DidChangeConfigurationParams {
    /**
     * The actual changed settings
     */
    settings: LSPAny;
}
```


#### Workspace folders request (↪)

> *Since version 3.6.0*

Many tools support more than one root folder per workspace. Examples for this are VS Code’s multi-root support, Atom’s project folder support or Sublime’s project support. If a client workspace consists of multiple roots then a server typically needs to know about this. The protocol up to now assumes one root folder which is announced to the server by the `rootUri` property of the `InitializeParams`. If the client supports workspace folders and announces them via the corresponding `workspaceFolders` client capability, the `InitializeParams` contain an additional property `workspaceFolders` with the configured workspace folders when the server starts.

The `workspace/workspaceFolders` request is sent from the server to the client to fetch the current open list of workspace folders. Returns `null` in the response if only a single file is open in the tool. Returns an empty array if a workspace is open but no folders are configured.

*Client Capability*:

- property path (optional): `workspace.workspaceFolders`
- property type: `boolean`

*Server Capability*:

- property path (optional): `workspace.workspaceFolders`
- property type: `WorkspaceFoldersServerCapabilities` defined as follows:


``` highlight
export interface WorkspaceFoldersServerCapabilities {
    /**
     * The server has support for workspace folders
     */
    supported?: boolean;

    /**
     * Whether the server wants to receive workspace folder
     * change notifications.
     *
     * If a string is provided, the string is treated as an ID
     * under which the notification is registered on the client
     * side. The ID can be used to unregister for these events
     * using the `client/unregisterCapability` request.
     */
    changeNotifications?: string | boolean;
}
```


*Request*:

- method: `workspace/workspaceFolders`
- params: none

*Response*:

- result: `WorkspaceFolder[] | null` defined as follows:


``` highlight
export interface WorkspaceFolder {
    /**
     * The associated URI for this workspace folder.
     */
    uri: URI;

    /**
     * The name of the workspace folder. Used to refer to this
     * workspace folder in the user interface.
     */
    name: string;
}
```


- error: code and message set in case an exception happens during the ‘workspace/workspaceFolders’ request

#### DidChangeWorkspaceFolders Notification (→)

> *Since version 3.6.0*

The `workspace/didChangeWorkspaceFolders` notification is sent from the client to the server to inform the server about workspace folder configuration changes. A server can register for this notification by using either the *server capability* `workspace.workspaceFolders.changeNotifications` or by using the dynamic capability registration mechanism. To dynamically register for the `workspace/didChangeWorkspaceFolders` send a `client/registerCapability` request from the server to the client. The registration parameter must have a `registrations` item of the following form, where `id` is a unique id used to unregister the capability (the example uses a UUID):


``` highlight
{
    id: "28c6150c-bd7b-11e7-abc4-cec278b6b50a",
    method: "workspace/didChangeWorkspaceFolders"
}
```


*Notification*:

- method: ‘workspace/didChangeWorkspaceFolders’
- params: `DidChangeWorkspaceFoldersParams` defined as follows:


``` highlight
export interface DidChangeWorkspaceFoldersParams {
    /**
     * The actual workspace folder change event.
     */
    event: WorkspaceFoldersChangeEvent;
}
```


``` highlight
/**
 * The workspace folder change event.
 */
export interface WorkspaceFoldersChangeEvent {
    /**
     * The array of added workspace folders
     */
    added: WorkspaceFolder[];

    /**
     * The array of the removed workspace folders
     */
    removed: WorkspaceFolder[];
}
```


#### WillCreateFiles Request (↩)

The will create files request is sent from the client to the server before files are actually created as long as the creation is triggered from within the client either by a user action or by applying a workspace edit. The request can return a `WorkspaceEdit` which will be applied to workspace before the files are created. Hence the `WorkspaceEdit` can not manipulate the content of the files to be created. Please note that clients might drop results if computing the edit took too long or if a server constantly fails on this request. This is done to keep creates fast and reliable.

*Client Capability*:

- property name (optional): `workspace.fileOperations.willCreate`
- property type: `boolean`

The capability indicates that the client supports sending `workspace/willCreateFiles` requests.

*Server Capability*:

- property name (optional): `workspace.fileOperations.willCreate`
- property type: `FileOperationRegistrationOptions` where `FileOperationRegistrationOptions` is defined as follows:


``` highlight
/**
 * The options to register for file operations.
 *
 * @since 3.16.0
 */
interface FileOperationRegistrationOptions {
    /**
     * The actual filters.
     */
    filters: FileOperationFilter[];
}
```


``` highlight
/**
 * A pattern kind describing if a glob pattern matches a file a folder or
 * both.
 *
 * @since 3.16.0
 */
export namespace FileOperationPatternKind {
    /**
     * The pattern matches a file only.
     */
    export const file: 'file' = 'file';

    /**
     * The pattern matches a folder only.
     */
    export const folder: 'folder' = 'folder';
}

export type FileOperationPatternKind = 'file' | 'folder';
```


``` highlight
/**
 * Matching options for the file operation pattern.
 *
 * @since 3.16.0
 */
export interface FileOperationPatternOptions {

    /**
     * The pattern should be matched ignoring casing.
     */
    ignoreCase?: boolean;
}
```


``` highlight
/**
 * A pattern to describe in which file operation requests or notifications
 * the server is interested in.
 *
 * @since 3.16.0
 */
interface FileOperationPattern {
    /**
     * The glob pattern to match. Glob patterns can have the following syntax:
     * - `*` to match zero or more characters in a path segment
     * - `?` to match on one character in a path segment
     * - `**` to match any number of path segments, including none
     * - `{}` to group sub patterns into an OR expression. (e.g. `**​/*.{ts,js}`
     *   matches all TypeScript and JavaScript files)
     * - `[]` to declare a range of characters to match in a path segment
     *   (e.g., `example.[0-9]` to match on `example.0`, `example.1`, …)
     * - `[!...]` to negate a range of characters to match in a path segment
     *   (e.g., `example.[!0-9]` to match on `example.a`, `example.b`, but
     *   not `example.0`)
     */
    glob: string;

    /**
     * Whether to match files or folders with this pattern.
     *
     * Matches both if undefined.
     */
    matches?: FileOperationPatternKind;

    /**
     * Additional options used during matching.
     */
    options?: FileOperationPatternOptions;
}
```


``` highlight
/**
 * A filter to describe in which file operation requests or notifications
 * the server is interested in.
 *
 * @since 3.16.0
 */
export interface FileOperationFilter {

    /**
     * A Uri like `file` or `untitled`.
     */
    scheme?: string;

    /**
     * The actual file operation pattern.
     */
    pattern: FileOperationPattern;
}
```


The capability indicates that the server is interested in receiving `workspace/willCreateFiles` requests.

*Registration Options*: none

*Request*:

- method: ‘workspace/willCreateFiles’
- params: `CreateFilesParams` defined as follows:


``` highlight
/**
 * The parameters sent in notifications/requests for user-initiated creation
 * of files.
 *
 * @since 3.16.0
 */
export interface CreateFilesParams {

    /**
     * An array of all files/folders created in this operation.
     */
    files: FileCreate[];
}
```


``` highlight
/**
 * Represents information on a file/folder create.
 *
 * @since 3.16.0
 */
export interface FileCreate {

    /**
     * A file:// URI for the location of the file/folder being created.
     */
    uri: string;
}
```


*Response*:

- result:`WorkspaceEdit` \| `null`
- error: code and message set in case an exception happens during the `willCreateFiles` request.

#### DidCreateFiles Notification (→)

The did create files notification is sent from the client to the server when files were created from within the client.

*Client Capability*:

- property name (optional): `workspace.fileOperations.didCreate`
- property type: `boolean`

The capability indicates that the client supports sending `workspace/didCreateFiles` notifications.

*Server Capability*:

- property name (optional): `workspace.fileOperations.didCreate`
- property type: `FileOperationRegistrationOptions`

The capability indicates that the server is interested in receiving `workspace/didCreateFiles` notifications.

*Notification*:

- method: ‘workspace/didCreateFiles’
- params: `CreateFilesParams`

#### WillRenameFiles Request (↩)

The will rename files request is sent from the client to the server before files are actually renamed as long as the rename is triggered from within the client either by a user action or by applying a workspace edit. The request can return a WorkspaceEdit which will be applied to workspace before the files are renamed. Please note that clients might drop results if computing the edit took too long or if a server constantly fails on this request. This is done to keep renames fast and reliable.

*Client Capability*:

- property name (optional): `workspace.fileOperations.willRename`
- property type: `boolean`

The capability indicates that the client supports sending `workspace/willRenameFiles` requests.

*Server Capability*:

- property name (optional): `workspace.fileOperations.willRename`
- property type: `FileOperationRegistrationOptions`

The capability indicates that the server is interested in receiving `workspace/willRenameFiles` requests.

*Registration Options*: none

*Request*:

- method: ‘workspace/willRenameFiles’
- params: `RenameFilesParams` defined as follows:


``` highlight
/**
 * The parameters sent in notifications/requests for user-initiated renames
 * of files.
 *
 * @since 3.16.0
 */
export interface RenameFilesParams {

    /**
     * An array of all files/folders renamed in this operation. When a folder
     * is renamed, only the folder will be included, and not its children.
     */
    files: FileRename[];
}
```


``` highlight
/**
 * Represents information on a file/folder rename.
 *
 * @since 3.16.0
 */
export interface FileRename {

    /**
     * A file:// URI for the original location of the file/folder being renamed.
     */
    oldUri: string;

    /**
     * A file:// URI for the new location of the file/folder being renamed.
     */
    newUri: string;
}
```


*Response*:

- result:`WorkspaceEdit` \| `null`
- error: code and message set in case an exception happens during the `workspace/willRenameFiles` request.

#### DidRenameFiles Notification (→)

The did rename files notification is sent from the client to the server when files were renamed from within the client.

*Client Capability*:

- property name (optional): `workspace.fileOperations.didRename`
- property type: `boolean`

The capability indicates that the client supports sending `workspace/didRenameFiles` notifications.

*Server Capability*:

- property name (optional): `workspace.fileOperations.didRename`
- property type: `FileOperationRegistrationOptions`

The capability indicates that the server is interested in receiving `workspace/didRenameFiles` notifications.

*Notification*:

- method: ‘workspace/didRenameFiles’
- params: `RenameFilesParams`

#### WillDeleteFiles Request (↩)

The will delete files request is sent from the client to the server before files are actually deleted as long as the deletion is triggered from within the client either by a user action or by applying a workspace edit. The request can return a WorkspaceEdit which will be applied to workspace before the files are deleted. Please note that clients might drop results if computing the edit took too long or if a server constantly fails on this request. This is done to keep deletes fast and reliable.

*Client Capability*:

- property name (optional): `workspace.fileOperations.willDelete`
- property type: `boolean`

The capability indicates that the client supports sending `workspace/willDeleteFiles` requests.

*Server Capability*:

- property name (optional): `workspace.fileOperations.willDelete`
- property type: `FileOperationRegistrationOptions`

The capability indicates that the server is interested in receiving `workspace/willDeleteFiles` requests.

*Registration Options*: none

*Request*:

- method: `workspace/willDeleteFiles`
- params: `DeleteFilesParams` defined as follows:


``` highlight
/**
 * The parameters sent in notifications/requests for user-initiated deletes
 * of files.
 *
 * @since 3.16.0
 */
export interface DeleteFilesParams {

    /**
     * An array of all files/folders deleted in this operation.
     */
    files: FileDelete[];
}
```


``` highlight
/**
 * Represents information on a file/folder delete.
 *
 * @since 3.16.0
 */
export interface FileDelete {

    /**
     * A file:// URI for the location of the file/folder being deleted.
     */
    uri: string;
}
```


*Response*:

- result:`WorkspaceEdit` \| `null`
- error: code and message set in case an exception happens during the `workspace/willDeleteFiles` request.

#### DidDeleteFiles Notification (→)

The did delete files notification is sent from the client to the server when files were deleted from within the client.

*Client Capability*:

- property name (optional): `workspace.fileOperations.didDelete`
- property type: `boolean`

The capability indicates that the client supports sending `workspace/didDeleteFiles` notifications.

*Server Capability*:

- property name (optional): `workspace.fileOperations.didDelete`
- property type: `FileOperationRegistrationOptions`

The capability indicates that the server is interested in receiving `workspace/didDeleteFiles` notifications.

*Notification*:

- method: ‘workspace/didDeleteFiles’
- params: `DeleteFilesParams`

#### DidChangeWatchedFiles Notification (→)

The watched files notification is sent from the client to the server when the client detects changes to files and folders watched by the language client (note although the name suggest that only file events are sent it is about file system events which include folders as well). It is recommended that servers register for these file system events using the registration mechanism. In former implementations clients pushed file events without the server actively asking for it.

Servers are allowed to run their own file system watching mechanism and not rely on clients to provide file system events. However this is not recommended due to the following reasons:

- to our experience getting file system watching on disk right is challenging, especially if it needs to be supported across multiple OSes.
- file system watching is not for free especially if the implementation uses some sort of polling and keeps a file system tree in memory to compare time stamps (as for example some node modules do)
- a client usually starts more than one server. If every server runs its own file system watching it can become a CPU or memory problem.
- in general there are more server than client implementations. So this problem is better solved on the client side.

*Client Capability*:

- property path (optional): `workspace.didChangeWatchedFiles`
- property type: `DidChangeWatchedFilesClientCapabilities` defined as follows:


``` highlight
export interface DidChangeWatchedFilesClientCapabilities {
    /**
     * Did change watched files notification supports dynamic registration.
     * Please note that the current protocol doesn't support static
     * configuration for file changes from the server side.
     */
    dynamicRegistration?: boolean;

    /**
     * Whether the client has support for relative patterns
     * or not.
     *
     * @since 3.17.0
     */
    relativePatternSupport?: boolean;
}
```


*Registration Options*: `DidChangeWatchedFilesRegistrationOptions` defined as follows:


``` highlight
/**
 * Describe options to be used when registering for file system change events.
 */
export interface DidChangeWatchedFilesRegistrationOptions {
    /**
     * The watchers to register.
     */
    watchers: FileSystemWatcher[];
}
```


``` highlight
/**
 * The glob pattern to watch relative to the base path. Glob patterns can have
 * the following syntax:
 * - `*` to match zero or more characters in a path segment
 * - `?` to match on one character in a path segment
 * - `**` to match any number of path segments, including none
 * - `{}` to group conditions (e.g. `**​/*.{ts,js}` matches all TypeScript
 *   and JavaScript files)
 * - `[]` to declare a range of characters to match in a path segment
 *   (e.g., `example.[0-9]` to match on `example.0`, `example.1`, …)
 * - `[!...]` to negate a range of characters to match in a path segment
 *   (e.g., `example.[!0-9]` to match on `example.a`, `example.b`,
 *   but not `example.0`)
 *
 * @since 3.17.0
 */
export type Pattern = string;
```


``` highlight
/**
 * A relative pattern is a helper to construct glob patterns that are matched
 * relatively to a base URI. The common value for a `baseUri` is a workspace
 * folder root, but it can be another absolute URI as well.
 *
 * @since 3.17.0
 */
export interface RelativePattern {
    /**
     * A workspace folder or a base URI to which this pattern will be matched
     * against relatively.
     */
    baseUri: WorkspaceFolder | URI;

    /**
     * The actual glob pattern;
     */
    pattern: Pattern;
}
```


``` highlight
/**
 * The glob pattern. Either a string pattern or a relative pattern.
 *
 * @since 3.17.0
 */
export type GlobPattern = Pattern | RelativePattern;
```


``` highlight
export interface FileSystemWatcher {
    /**
     * The glob pattern to watch. See {@link GlobPattern glob pattern}
     * for more detail.
     *
     * @since 3.17.0 support for relative patterns.
     */
    globPattern: GlobPattern;

    /**
     * The kind of events of interest. If omitted it defaults
     * to WatchKind.Create | WatchKind.Change | WatchKind.Delete
     * which is 7.
     */
    kind?: WatchKind;
}
```


``` highlight
export namespace WatchKind {
    /**
     * Interested in create events.
     */
    export const Create = 1;

    /**
     * Interested in change events
     */
    export const Change = 2;

    /**
     * Interested in delete events
     */
    export const Delete = 4;
}
export type WatchKind = uinteger;
```


*Notification*:

- method: ‘workspace/didChangeWatchedFiles’
- params: `DidChangeWatchedFilesParams` defined as follows:


``` highlight
interface DidChangeWatchedFilesParams {
    /**
     * The actual file events.
     */
    changes: FileEvent[];
}
```


Where FileEvents are described as follows:


``` highlight
/**
 * An event describing a file change.
 */
interface FileEvent {
    /**
     * The file's URI.
     */
    uri: DocumentUri;
    /**
     * The change type.
     */
    type: FileChangeType;
}
```


``` highlight
/**
 * The file event type.
 */
export namespace FileChangeType {
    /**
     * The file got created.
     */
    export const Created = 1;
    /**
     * The file got changed.
     */
    export const Changed = 2;
    /**
     * The file got deleted.
     */
    export const Deleted = 3;
}

export type FileChangeType = 1 | 2 | 3;
```


#### Execute a command (↩)

The `workspace/executeCommand` request is sent from the client to the server to trigger command execution on the server. In most cases the server creates a `WorkspaceEdit` structure and applies the changes to the workspace using the request `workspace/applyEdit` which is sent from the server to the client.

*Client Capability*:

- property path (optional): `workspace.executeCommand`
- property type: `ExecuteCommandClientCapabilities` defined as follows:


``` highlight
export interface ExecuteCommandClientCapabilities {
    /**
     * Execute command supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property path (optional): `executeCommandProvider`
- property type: `ExecuteCommandOptions` defined as follows:


``` highlight
export interface ExecuteCommandOptions extends WorkDoneProgressOptions {
    /**
     * The commands to be executed on the server
     */
    commands: string[];
}
```


*Registration Options*: `ExecuteCommandRegistrationOptions` defined as follows:


``` highlight
/**
 * Execute command registration options.
 */
export interface ExecuteCommandRegistrationOptions
    extends ExecuteCommandOptions {
}
```


*Request*:

- method: ‘workspace/executeCommand’
- params: `ExecuteCommandParams` defined as follows:


``` highlight
export interface ExecuteCommandParams extends WorkDoneProgressParams {

    /**
     * The identifier of the actual command handler.
     */
    command: string;
    /**
     * Arguments that the command should be invoked with.
     */
    arguments?: LSPAny[];
}
```


The arguments are typically specified when a command is returned from the server to the client. Example requests that return a command are `textDocument/codeAction` or `textDocument/codeLens`.

*Response*:

- result: `LSPAny`
- error: code and message set in case an exception happens during the request.

#### Applies a WorkspaceEdit (↪)

The `workspace/applyEdit` request is sent from the server to the client to modify resource on the client side.

*Client Capability*:

- property path (optional): `workspace.applyEdit`
- property type: `boolean`

See also the [WorkspaceEditClientCapabilities](#workspaceEditClientCapabilities) for the supported capabilities of a workspace edit.

*Request*:

- method: ‘workspace/applyEdit’
- params: `ApplyWorkspaceEditParams` defined as follows:


``` highlight
export interface ApplyWorkspaceEditParams {
    /**
     * An optional label of the workspace edit. This label is
     * presented in the user interface for example on an undo
     * stack to undo the workspace edit.
     */
    label?: string;

    /**
     * The edits to apply.
     */
    edit: WorkspaceEdit;
}
```


*Response*:

- result: `ApplyWorkspaceEditResult` defined as follows:


``` highlight
export interface ApplyWorkspaceEditResult {
    /**
     * Indicates whether the edit was applied or not.
     */
    applied: boolean;

    /**
     * An optional textual description for why the edit was not applied.
     * This may be used by the server for diagnostic logging or to provide
     * a suitable error for a request that triggered the edit.
     */
    failureReason?: string;

    /**
     * Depending on the client's failure handling strategy `failedChange`
     * might contain the index of the change that failed. This property is
     * only available if the client signals a `failureHandling` strategy
     * in its client capabilities.
     */
    failedChange?: uinteger;
}
```


- error: code and message set in case an exception happens during the request.

