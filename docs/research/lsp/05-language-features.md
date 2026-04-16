---
title: "LSP 3.17 — Language Features"
source: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
date: 2026-04-16
tags:
  - wiki/input
  - research/lsp
---

### Language Features

Language Features provide the actual smarts in the language server protocol. They are usually executed on a \[text document, position\] tuple. The main language feature categories are:

- code comprehension features like Hover or Goto Definition.
- coding features like diagnostics, code complete or code actions.

The language features should be computed on the [synchronized state](#textDocument_synchronization) of the document.

#### Goto Declaration Request (↩)

> *Since version 3.14.0*

The go to declaration request is sent from the client to the server to resolve the declaration location of a symbol at a given text document position.

The result type [`LocationLink`](#locationLink)\[\] got introduced with version 3.14.0 and depends on the corresponding client capability `textDocument.declaration.linkSupport`.

*Client Capability*:

- property name (optional): `textDocument.declaration`
- property type: `DeclarationClientCapabilities` defined as follows:


``` highlight
export interface DeclarationClientCapabilities {
    /**
     * Whether declaration supports dynamic registration. If this is set to
     * `true` the client supports the new `DeclarationRegistrationOptions`
     * return value for the corresponding server capability as well.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports additional metadata in the form of declaration links.
     */
    linkSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `declarationProvider`
- property type: `boolean | DeclarationOptions | DeclarationRegistrationOptions` where `DeclarationOptions` is defined as follows:


``` highlight
export interface DeclarationOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `DeclarationRegistrationOptions` defined as follows:


``` highlight
export interface DeclarationRegistrationOptions extends DeclarationOptions,
    TextDocumentRegistrationOptions, StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/declaration`
- params: `DeclarationParams` defined as follows:


``` highlight
export interface DeclarationParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
}
```


*Response*:

- result: [`Location`](#location) \| [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\] \|`null`
- partial result: [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\]
- error: code and message set in case an exception happens during the declaration request.

#### Goto Definition Request (↩)

The go to definition request is sent from the client to the server to resolve the definition location of a symbol at a given text document position.

The result type [`LocationLink`](#locationLink)\[\] got introduced with version 3.14.0 and depends on the corresponding client capability `textDocument.definition.linkSupport`.

*Client Capability*:

- property name (optional): `textDocument.definition`
- property type: `DefinitionClientCapabilities` defined as follows:


``` highlight
export interface DefinitionClientCapabilities {
    /**
     * Whether definition supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports additional metadata in the form of definition links.
     *
     * @since 3.14.0
     */
    linkSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `definitionProvider`
- property type: `boolean | DefinitionOptions` where `DefinitionOptions` is defined as follows:


``` highlight
export interface DefinitionOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `DefinitionRegistrationOptions` defined as follows:


``` highlight
export interface DefinitionRegistrationOptions extends
    TextDocumentRegistrationOptions, DefinitionOptions {
}
```


*Request*:

- method: `textDocument/definition`
- params: `DefinitionParams` defined as follows:


``` highlight
export interface DefinitionParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
}
```


*Response*:

- result: [`Location`](#location) \| [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\] \| `null`
- partial result: [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\]
- error: code and message set in case an exception happens during the definition request.

#### Goto Type Definition Request (↩)

> *Since version 3.6.0*

The go to type definition request is sent from the client to the server to resolve the type definition location of a symbol at a given text document position.

The result type [`LocationLink`](#locationLink)\[\] got introduced with version 3.14.0 and depends on the corresponding client capability `textDocument.typeDefinition.linkSupport`.

*Client Capability*:

- property name (optional): `textDocument.typeDefinition`
- property type: `TypeDefinitionClientCapabilities` defined as follows:


``` highlight
export interface TypeDefinitionClientCapabilities {
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true` the client supports the new `TypeDefinitionRegistrationOptions`
     * return value for the corresponding server capability as well.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports additional metadata in the form of definition links.
     *
     * @since 3.14.0
     */
    linkSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `typeDefinitionProvider`
- property type: `boolean | TypeDefinitionOptions | TypeDefinitionRegistrationOptions` where `TypeDefinitionOptions` is defined as follows:


``` highlight
export interface TypeDefinitionOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `TypeDefinitionRegistrationOptions` defined as follows:


``` highlight
export interface TypeDefinitionRegistrationOptions extends
    TextDocumentRegistrationOptions, TypeDefinitionOptions,
    StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/typeDefinition`
- params: `TypeDefinitionParams` defined as follows:


``` highlight
export interface TypeDefinitionParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
}
```


*Response*:

- result: [`Location`](#location) \| [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\] \| `null`
- partial result: [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\]
- error: code and message set in case an exception happens during the definition request.

#### Goto Implementation Request (↩)

> *Since version 3.6.0*

The go to implementation request is sent from the client to the server to resolve the implementation location of a symbol at a given text document position.

The result type [`LocationLink`](#locationLink)\[\] got introduced with version 3.14.0 and depends on the corresponding client capability `textDocument.implementation.linkSupport`.

*Client Capability*:

- property name (optional): `textDocument.implementation`
- property type: `ImplementationClientCapabilities` defined as follows:


``` highlight
export interface ImplementationClientCapabilities {
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true` the client supports the new `ImplementationRegistrationOptions`
     * return value for the corresponding server capability as well.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports additional metadata in the form of definition links.
     *
     * @since 3.14.0
     */
    linkSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `implementationProvider`
- property type: `boolean | ImplementationOptions | ImplementationRegistrationOptions` where `ImplementationOptions` is defined as follows:


``` highlight
export interface ImplementationOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `ImplementationRegistrationOptions` defined as follows:


``` highlight
export interface ImplementationRegistrationOptions extends
    TextDocumentRegistrationOptions, ImplementationOptions,
    StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/implementation`
- params: `ImplementationParams` defined as follows:


``` highlight
export interface ImplementationParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
}
```


*Response*:

- result: [`Location`](#location) \| [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\] \| `null`
- partial result: [`Location`](#location)\[\] \| [`LocationLink`](#locationLink)\[\]
- error: code and message set in case an exception happens during the definition request.

#### Find References Request (↩)

The references request is sent from the client to the server to resolve project-wide references for the symbol denoted by the given text document position.

*Client Capability*:

- property name (optional): `textDocument.references`
- property type: `ReferenceClientCapabilities` defined as follows:


``` highlight
export interface ReferenceClientCapabilities {
    /**
     * Whether references supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `referencesProvider`
- property type: `boolean | ReferenceOptions` where `ReferenceOptions` is defined as follows:


``` highlight
export interface ReferenceOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `ReferenceRegistrationOptions` defined as follows:


``` highlight
export interface ReferenceRegistrationOptions extends
    TextDocumentRegistrationOptions, ReferenceOptions {
}
```


*Request*:

- method: `textDocument/references`
- params: `ReferenceParams` defined as follows:


``` highlight
export interface ReferenceParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
    context: ReferenceContext;
}
```


``` highlight
export interface ReferenceContext {
    /**
     * Include the declaration of the current symbol.
     */
    includeDeclaration: boolean;
}
```


*Response*:

- result: [`Location`](#location)\[\] \| `null`
- partial result: [`Location`](#location)\[\]
- error: code and message set in case an exception happens during the reference request.

#### Prepare Call Hierarchy Request (↩)

> *Since version 3.16.0*

The call hierarchy request is sent from the client to the server to return a call hierarchy for the language element of given text document positions. The call hierarchy requests are executed in two steps:

1.  first a call hierarchy item is resolved for the given text document position
2.  for a call hierarchy item the incoming or outgoing call hierarchy items are resolved.

*Client Capability*:

- property name (optional): `textDocument.callHierarchy`
- property type: `CallHierarchyClientCapabilities` defined as follows:


``` highlight
interface CallHierarchyClientCapabilities {
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true` the client supports the new `(TextDocumentRegistrationOptions &
     * StaticRegistrationOptions)` return value for the corresponding server
     * capability as well.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `callHierarchyProvider`
- property type: `boolean | CallHierarchyOptions | CallHierarchyRegistrationOptions` where `CallHierarchyOptions` is defined as follows:


``` highlight
export interface CallHierarchyOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `CallHierarchyRegistrationOptions` defined as follows:


``` highlight
export interface CallHierarchyRegistrationOptions extends
    TextDocumentRegistrationOptions, CallHierarchyOptions,
    StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/prepareCallHierarchy`
- params: `CallHierarchyPrepareParams` defined as follows:


``` highlight
export interface CallHierarchyPrepareParams extends TextDocumentPositionParams,
    WorkDoneProgressParams {
}
```


*Response*:

- result: `CallHierarchyItem[] | null` defined as follows:


``` highlight
export interface CallHierarchyItem {
    /**
     * The name of this item.
     */
    name: string;

    /**
     * The kind of this item.
     */
    kind: SymbolKind;

    /**
     * Tags for this item.
     */
    tags?: SymbolTag[];

    /**
     * More detail for this item, e.g. the signature of a function.
     */
    detail?: string;

    /**
     * The resource identifier of this item.
     */
    uri: DocumentUri;

    /**
     * The range enclosing this symbol not including leading/trailing whitespace
     * but everything else, e.g. comments and code.
     */
    range: Range;

    /**
     * The range that should be selected and revealed when this symbol is being
     * picked, e.g. the name of a function. Must be contained by the
     * [`range`](#CallHierarchyItem.range).
     */
    selectionRange: Range;

    /**
     * A data entry field that is preserved between a call hierarchy prepare and
     * incoming calls or outgoing calls requests.
     */
    data?: LSPAny;
}
```


- error: code and message set in case an exception happens during the ‘textDocument/prepareCallHierarchy’ request

#### Call Hierarchy Incoming Calls (↩)

> *Since version 3.16.0*

The request is sent from the client to the server to resolve incoming calls for a given call hierarchy item. The request doesn’t define its own client and server capabilities. It is only issued if a server registers for the [`textDocument/prepareCallHierarchy` request](#textDocument_prepareCallHierarchy).

*Request*:

- method: `callHierarchy/incomingCalls`
- params: `CallHierarchyIncomingCallsParams` defined as follows:


``` highlight
export interface CallHierarchyIncomingCallsParams extends
    WorkDoneProgressParams, PartialResultParams {
    item: CallHierarchyItem;
}
```


*Response*:

- result: `CallHierarchyIncomingCall[] | null` defined as follows:


``` highlight
export interface CallHierarchyIncomingCall {

    /**
     * The item that makes the call.
     */
    from: CallHierarchyItem;

    /**
     * The ranges at which the calls appear. This is relative to the caller
     * denoted by [`this.from`](#CallHierarchyIncomingCall.from).
     */
    fromRanges: Range[];
}
```


- partial result: `CallHierarchyIncomingCall[]`
- error: code and message set in case an exception happens during the ‘callHierarchy/incomingCalls’ request

#### Call Hierarchy Outgoing Calls (↩)

> *Since version 3.16.0*

The request is sent from the client to the server to resolve outgoing calls for a given call hierarchy item. The request doesn’t define its own client and server capabilities. It is only issued if a server registers for the [`textDocument/prepareCallHierarchy` request](#textDocument_prepareCallHierarchy).

*Request*:

- method: `callHierarchy/outgoingCalls`
- params: `CallHierarchyOutgoingCallsParams` defined as follows:


``` highlight
export interface CallHierarchyOutgoingCallsParams extends
    WorkDoneProgressParams, PartialResultParams {
    item: CallHierarchyItem;
}
```


*Response*:

- result: `CallHierarchyOutgoingCall[] | null` defined as follows:


``` highlight
export interface CallHierarchyOutgoingCall {

    /**
     * The item that is called.
     */
    to: CallHierarchyItem;

    /**
     * The range at which this item is called. This is the range relative to
     * the caller, e.g the item passed to `callHierarchy/outgoingCalls` request.
     */
    fromRanges: Range[];
}
```


- partial result: `CallHierarchyOutgoingCall[]`
- error: code and message set in case an exception happens during the ‘callHierarchy/outgoingCalls’ request

#### Prepare Type Hierarchy Request (↩)

> *Since version 3.17.0*

The type hierarchy request is sent from the client to the server to return a type hierarchy for the language element of given text document positions. Will return `null` if the server couldn’t infer a valid type from the position. The type hierarchy requests are executed in two steps:

1.  first a type hierarchy item is prepared for the given text document position.
2.  for a type hierarchy item the supertype or subtype type hierarchy items are resolved.

*Client Capability*:

- property name (optional): `textDocument.typeHierarchy`
- property type: `TypeHierarchyClientCapabilities` defined as follows:


``` highlight
type TypeHierarchyClientCapabilities = {
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true` the client supports the new `(TextDocumentRegistrationOptions &
     * StaticRegistrationOptions)` return value for the corresponding server
     * capability as well.
     */
    dynamicRegistration?: boolean;
};
```


*Server Capability*:

- property name (optional): `typeHierarchyProvider`
- property type: `boolean | TypeHierarchyOptions | TypeHierarchyRegistrationOptions` where `TypeHierarchyOptions` is defined as follows:


``` highlight
export interface TypeHierarchyOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `TypeHierarchyRegistrationOptions` defined as follows:


``` highlight
export interface TypeHierarchyRegistrationOptions extends
    TextDocumentRegistrationOptions, TypeHierarchyOptions,
    StaticRegistrationOptions {
}
```


*Request*:

- method: ‘textDocument/prepareTypeHierarchy’
- params: `TypeHierarchyPrepareParams` defined as follows:


``` highlight
export interface TypeHierarchyPrepareParams extends TextDocumentPositionParams,
    WorkDoneProgressParams {
}
```


*Response*:

- result: `TypeHierarchyItem[] | null` defined as follows:


``` highlight
export interface TypeHierarchyItem {
    /**
     * The name of this item.
     */
    name: string;

    /**
     * The kind of this item.
     */
    kind: SymbolKind;

    /**
     * Tags for this item.
     */
    tags?: SymbolTag[];

    /**
     * More detail for this item, e.g. the signature of a function.
     */
    detail?: string;

    /**
     * The resource identifier of this item.
     */
    uri: DocumentUri;

    /**
     * The range enclosing this symbol not including leading/trailing whitespace
     * but everything else, e.g. comments and code.
     */
    range: Range;

    /**
     * The range that should be selected and revealed when this symbol is being
     * picked, e.g. the name of a function. Must be contained by the
     * [`range`](#TypeHierarchyItem.range).
     */
    selectionRange: Range;

    /**
     * A data entry field that is preserved between a type hierarchy prepare and
     * supertypes or subtypes requests. It could also be used to identify the
     * type hierarchy in the server, helping improve the performance on
     * resolving supertypes and subtypes.
     */
    data?: LSPAny;
}
```


- error: code and message set in case an exception happens during the ‘textDocument/prepareTypeHierarchy’ request

#### Type Hierarchy Supertypes(↩)

> *Since version 3.17.0*

The request is sent from the client to the server to resolve the supertypes for a given type hierarchy item. Will return `null` if the server couldn’t infer a valid type from `item` in the params. The request doesn’t define its own client and server capabilities. It is only issued if a server registers for the [`textDocument/prepareTypeHierarchy` request](#textDocument_prepareTypeHierarchy).

*Request*:

- method: ‘typeHierarchy/supertypes’
- params: `TypeHierarchySupertypesParams` defined as follows:


``` highlight
export interface TypeHierarchySupertypesParams extends
    WorkDoneProgressParams, PartialResultParams {
    item: TypeHierarchyItem;
}
```


*Response*:

- result: `TypeHierarchyItem[] | null`
- partial result: `TypeHierarchyItem[]`
- error: code and message set in case an exception happens during the ‘typeHierarchy/supertypes’ request

#### Type Hierarchy Subtypes(↩)

> *Since version 3.17.0*

The request is sent from the client to the server to resolve the subtypes for a given type hierarchy item. Will return `null` if the server couldn’t infer a valid type from `item` in the params. The request doesn’t define its own client and server capabilities. It is only issued if a server registers for the [`textDocument/prepareTypeHierarchy` request](#textDocument_prepareTypeHierarchy).

*Request*:

- method: ‘typeHierarchy/subtypes’
- params: `TypeHierarchySubtypesParams` defined as follows:


``` highlight
export interface TypeHierarchySubtypesParams extends
    WorkDoneProgressParams, PartialResultParams {
    item: TypeHierarchyItem;
}
```


*Response*:

- result: `TypeHierarchyItem[] | null`
- partial result: `TypeHierarchyItem[]`
- error: code and message set in case an exception happens during the ‘typeHierarchy/subtypes’ request

#### Document Highlights Request (↩)

The document highlight request is sent from the client to the server to resolve document highlights for a given text document position. For programming languages this usually highlights all references to the symbol scoped to this file. However, we kept ‘textDocument/documentHighlight’ and ‘textDocument/references’ separate requests since the first one is allowed to be more fuzzy. Symbol matches usually have a `DocumentHighlightKind` of `Read` or `Write` whereas fuzzy or textual matches use `Text` as the kind.

*Client Capability*:

- property name (optional): `textDocument.documentHighlight`
- property type: `DocumentHighlightClientCapabilities` defined as follows:


``` highlight
export interface DocumentHighlightClientCapabilities {
    /**
     * Whether document highlight supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `documentHighlightProvider`
- property type: `boolean | DocumentHighlightOptions` where `DocumentHighlightOptions` is defined as follows:


``` highlight
export interface DocumentHighlightOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `DocumentHighlightRegistrationOptions` defined as follows:


``` highlight
export interface DocumentHighlightRegistrationOptions extends
    TextDocumentRegistrationOptions, DocumentHighlightOptions {
}
```


*Request*:

- method: `textDocument/documentHighlight`
- params: `DocumentHighlightParams` defined as follows:


``` highlight
export interface DocumentHighlightParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
}
```


*Response*:

- result: `DocumentHighlight[]` \| `null` defined as follows:


``` highlight
/**
 * A document highlight is a range inside a text document which deserves
 * special attention. Usually a document highlight is visualized by changing
 * the background color of its range.
 *
 */
export interface DocumentHighlight {
    /**
     * The range this highlight applies to.
     */
    range: Range;

    /**
     * The highlight kind, default is DocumentHighlightKind.Text.
     */
    kind?: DocumentHighlightKind;
}
```


``` highlight
/**
 * A document highlight kind.
 */
export namespace DocumentHighlightKind {
    /**
     * A textual occurrence.
     */
    export const Text = 1;

    /**
     * Read-access of a symbol, like reading a variable.
     */
    export const Read = 2;

    /**
     * Write-access of a symbol, like writing to a variable.
     */
    export const Write = 3;
}

export type DocumentHighlightKind = 1 | 2 | 3;
```


- partial result: `DocumentHighlight[]`
- error: code and message set in case an exception happens during the document highlight request.

#### Document Link Request (↩)

The document links request is sent from the client to the server to request the location of links in a document.

*Client Capability*:

- property name (optional): `textDocument.documentLink`
- property type: `DocumentLinkClientCapabilities` defined as follows:


``` highlight
export interface DocumentLinkClientCapabilities {
    /**
     * Whether document link supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * Whether the client supports the `tooltip` property on `DocumentLink`.
     *
     * @since 3.15.0
     */
    tooltipSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `documentLinkProvider`
- property type: `DocumentLinkOptions` defined as follows:


``` highlight
export interface DocumentLinkOptions extends WorkDoneProgressOptions {
    /**
     * Document links have a resolve provider as well.
     */
    resolveProvider?: boolean;
}
```


*Registration Options*: `DocumentLinkRegistrationOptions` defined as follows:


``` highlight
export interface DocumentLinkRegistrationOptions extends
    TextDocumentRegistrationOptions, DocumentLinkOptions {
}
```


*Request*:

- method: `textDocument/documentLink`
- params: `DocumentLinkParams` defined as follows:


``` highlight
interface DocumentLinkParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The document to provide document links for.
     */
    textDocument: TextDocumentIdentifier;
}
```


*Response*:

- result: `DocumentLink[]` \| `null`.


``` highlight
/**
 * A document link is a range in a text document that links to an internal or
 * external resource, like another text document or a web site.
 */
interface DocumentLink {
    /**
     * The range this link applies to.
     */
    range: Range;

    /**
     * The uri this link points to. If missing a resolve request is sent later.
     */
    target?: URI;

    /**
     * The tooltip text when you hover over this link.
     *
     * If a tooltip is provided, is will be displayed in a string that includes
     * instructions on how to trigger the link, such as `{0} (ctrl + click)`.
     * The specific instructions vary depending on OS, user settings, and
     * localization.
     *
     * @since 3.15.0
     */
    tooltip?: string;

    /**
     * A data entry field that is preserved on a document link between a
     * DocumentLinkRequest and a DocumentLinkResolveRequest.
     */
    data?: LSPAny;
}
```


- partial result: `DocumentLink[]`
- error: code and message set in case an exception happens during the document link request.

#### Document Link Resolve Request (↩)

The document link resolve request is sent from the client to the server to resolve the target of a given document link.

*Request*:

- method: `documentLink/resolve`
- params: `DocumentLink`

*Response*:

- result: `DocumentLink`
- error: code and message set in case an exception happens during the document link resolve request.

#### Hover Request (↩)

The hover request is sent from the client to the server to request hover information at a given text document position.

When the client sends a hover request, the position typically refers to the position immediately to the left of the character being hovered over. For example, when a user hovers over a character `c` at offset `n`, the client typically sends position `n` (the position before the character). However, how servers interpret this position and what hover information they return is language and implementation specific.

*Client Capability*:

- property name (optional): `textDocument.hover`
- property type: `HoverClientCapabilities` defined as follows:


``` highlight
export interface HoverClientCapabilities {
    /**
     * Whether hover supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * Client supports the follow content formats if the content
     * property refers to a `literal of type MarkupContent`.
     * The order describes the preferred format of the client.
     */
    contentFormat?: MarkupKind[];
}
```


*Server Capability*:

- property name (optional): `hoverProvider`
- property type: `boolean | HoverOptions` where `HoverOptions` is defined as follows:


``` highlight
export interface HoverOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `HoverRegistrationOptions` defined as follows:


``` highlight
export interface HoverRegistrationOptions
    extends TextDocumentRegistrationOptions, HoverOptions {
}
```


*Request*:

- method: `textDocument/hover`
- params: `HoverParams` defined as follows:


``` highlight
export interface HoverParams extends TextDocumentPositionParams,
    WorkDoneProgressParams {
}
```


*Response*:

- result: `Hover` \| `null` defined as follows:


``` highlight
/**
 * The result of a hover request.
 */
export interface Hover {
    /**
     * The hover's content
     */
    contents: MarkedString | MarkedString[] | MarkupContent;

    /**
     * An optional range is a range inside a text document
     * that is used to visualize a hover, e.g. by changing the background color.
     */
    range?: Range;
}
```


Where `MarkedString` is defined as follows:


``` highlight
/**
 * MarkedString can be used to render human readable text. It is either a
 * markdown string or a code-block that provides a language and a code snippet.
 * The language identifier is semantically equal to the optional language
 * identifier in fenced code blocks in GitHub issues.
 *
 * The pair of a language and a value is an equivalent to markdown:
 * ```${language}
 * ${value}
 * ```
 *
 * Note that markdown strings will be sanitized - that means html will be
 * escaped.
 *
 * @deprecated use MarkupContent instead.
 */
type MarkedString = string | { language: string; value: string };
```


- error: code and message set in case an exception happens during the hover request.

#### Code Lens Request (↩)

The code lens request is sent from the client to the server to compute code lenses for a given text document.

*Client Capability*:

- property name (optional): `textDocument.codeLens`
- property type: `CodeLensClientCapabilities` defined as follows:


``` highlight
export interface CodeLensClientCapabilities {
    /**
     * Whether code lens supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `codeLensProvider`
- property type: `CodeLensOptions` defined as follows:


``` highlight
export interface CodeLensOptions extends WorkDoneProgressOptions {
    /**
     * Code lens has a resolve provider as well.
     */
    resolveProvider?: boolean;
}
```


*Registration Options*: `CodeLensRegistrationOptions` defined as follows:


``` highlight
export interface CodeLensRegistrationOptions extends
    TextDocumentRegistrationOptions, CodeLensOptions {
}
```


*Request*:

- method: `textDocument/codeLens`
- params: `CodeLensParams` defined as follows:


``` highlight
interface CodeLensParams extends WorkDoneProgressParams, PartialResultParams {
    /**
     * The document to request code lens for.
     */
    textDocument: TextDocumentIdentifier;
}
```


*Response*:

- result: `CodeLens[]` \| `null` defined as follows:


``` highlight
/**
 * A code lens represents a command that should be shown along with
 * source text, like the number of references, a way to run tests, etc.
 *
 * A code lens is _unresolved_ when no command is associated to it. For
 * performance reasons the creation of a code lens and resolving should be done
 * in two stages.
 */
interface CodeLens {
    /**
     * The range in which this code lens is valid. Should only span a single
     * line.
     */
    range: Range;

    /**
     * The command this code lens represents.
     */
    command?: Command;

    /**
     * A data entry field that is preserved on a code lens item between
     * a code lens and a code lens resolve request.
     */
    data?: LSPAny;
}
```


- partial result: `CodeLens[]`
- error: code and message set in case an exception happens during the code lens request.

#### Code Lens Resolve Request (↩)

The code lens resolve request is sent from the client to the server to resolve the command for a given code lens item.

*Request*:

- method: `codeLens/resolve`
- params: `CodeLens`

*Response*:

- result: `CodeLens`
- error: code and message set in case an exception happens during the code lens resolve request.

#### Code Lens Refresh Request (↪)

> *Since version 3.16.0*

The `workspace/codeLens/refresh` request is sent from the server to the client. Servers can use it to ask clients to refresh the code lenses currently shown in editors. As a result the client should ask the server to recompute the code lenses for these editors. This is useful if a server detects a configuration change which requires a re-calculation of all code lenses. Note that the client still has the freedom to delay the re-calculation of the code lenses if for example an editor is currently not visible.

*Client Capability*:

- property name (optional): `workspace.codeLens`
- property type: `CodeLensWorkspaceClientCapabilities` defined as follows:


``` highlight
export interface CodeLensWorkspaceClientCapabilities {
    /**
     * Whether the client implementation supports a refresh request sent from the
     * server to the client.
     *
     * Note that this event is global and will force the client to refresh all
     * code lenses currently shown. It should be used with absolute care and is
     * useful for situation where a server for example detect a project wide
     * change that requires such a calculation.
     */
    refreshSupport?: boolean;
}
```


*Request*:

- method: `workspace/codeLens/refresh`
- params: none

*Response*:

- result: void
- error: code and message set in case an exception happens during the ‘workspace/codeLens/refresh’ request

#### Folding Range Request (↩)

> *Since version 3.10.0*

The folding range request is sent from the client to the server to return all folding ranges found in a given text document.

*Client Capability*:

- property name (optional): `textDocument.foldingRange`
- property type: `FoldingRangeClientCapabilities` defined as follows:


``` highlight
export interface FoldingRangeClientCapabilities {
    /**
     * Whether implementation supports dynamic registration for folding range
     * providers. If this is set to `true` the client supports the new
     * `FoldingRangeRegistrationOptions` return value for the corresponding
     * server capability as well.
     */
    dynamicRegistration?: boolean;

    /**
     * The maximum number of folding ranges that the client prefers to receive
     * per document. The value serves as a hint, servers are free to follow the
     * limit.
     */
    rangeLimit?: uinteger;

    /**
     * If set, the client signals that it only supports folding complete lines.
     * If set, client will ignore specified `startCharacter` and `endCharacter`
     * properties in a FoldingRange.
     */
    lineFoldingOnly?: boolean;

    /**
     * Specific options for the folding range kind.
     *
     * @since 3.17.0
     */
    foldingRangeKind? : {
        /**
         * The folding range kind values the client supports. When this
         * property exists the client also guarantees that it will
         * handle values outside its set gracefully and falls back
         * to a default value when unknown.
         */
        valueSet?: FoldingRangeKind[];
    };

    /**
     * Specific options for the folding range.
     * @since 3.17.0
     */
    foldingRange?: {
        /**
        * If set, the client signals that it supports setting collapsedText on
        * folding ranges to display custom labels instead of the default text.
        *
        * @since 3.17.0
        */
        collapsedText?: boolean;
    };
}
```


*Server Capability*:

- property name (optional): `foldingRangeProvider`
- property type: `boolean | FoldingRangeOptions | FoldingRangeRegistrationOptions` where `FoldingRangeOptions` is defined as follows:


``` highlight
export interface FoldingRangeOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `FoldingRangeRegistrationOptions` defined as follows:


``` highlight
export interface FoldingRangeRegistrationOptions extends
    TextDocumentRegistrationOptions, FoldingRangeOptions,
    StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/foldingRange`
- params: `FoldingRangeParams` defined as follows


``` highlight
export interface FoldingRangeParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;
}
```


*Response*:

- result: `FoldingRange[] | null` defined as follows:


``` highlight
/**
 * A set of predefined range kinds.
 */
export namespace FoldingRangeKind {
    /**
     * Folding range for a comment
     */
    export const Comment = 'comment';

    /**
     * Folding range for imports or includes
     */
    export const Imports = 'imports';

    /**
     * Folding range for a region (e.g. `#region`)
     */
    export const Region = 'region';
}

/**
 * The type is a string since the value set is extensible
 */
export type FoldingRangeKind = string;
```


``` highlight
/**
 * Represents a folding range. To be valid, start and end line must be bigger
 * than zero and smaller than the number of lines in the document. Clients
 * are free to ignore invalid ranges.
 */
export interface FoldingRange {

    /**
     * The zero-based start line of the range to fold. The folded area starts
     * after the line's last character. To be valid, the end must be zero or
     * larger and smaller than the number of lines in the document.
     */
    startLine: uinteger;

    /**
     * The zero-based character offset from where the folded range starts. If
     * not defined, defaults to the length of the start line.
     */
    startCharacter?: uinteger;

    /**
     * The zero-based end line of the range to fold. The folded area ends with
     * the line's last character. To be valid, the end must be zero or larger
     * and smaller than the number of lines in the document.
     */
    endLine: uinteger;

    /**
     * The zero-based character offset before the folded range ends. If not
     * defined, defaults to the length of the end line.
     */
    endCharacter?: uinteger;

    /**
     * Describes the kind of the folding range such as `comment` or `region`.
     * The kind is used to categorize folding ranges and used by commands like
     * 'Fold all comments'. See [FoldingRangeKind](#FoldingRangeKind) for an
     * enumeration of standardized kinds.
     */
    kind?: FoldingRangeKind;

    /**
     * The text that the client should show when the specified range is
     * collapsed. If not defined or not supported by the client, a default
     * will be chosen by the client.
     *
     * @since 3.17.0 - proposed
     */
    collapsedText?: string;
}
```


- partial result: `FoldingRange[]`
- error: code and message set in case an exception happens during the ‘textDocument/foldingRange’ request

#### Selection Range Request (↩)

> *Since version 3.15.0*

The selection range request is sent from the client to the server to return suggested selection ranges at an array of given positions. A selection range is a range around the cursor position which the user might be interested in selecting.

A selection range in the return array is for the position in the provided parameters at the same index. Therefore positions\[i\] must be contained in result\[i\].range. To allow for results where some positions have selection ranges and others do not, result\[i\].range is allowed to be the empty range at positions\[i\].

Typically, but not necessary, selection ranges correspond to the nodes of the syntax tree.

*Client Capability*:

- property name (optional): `textDocument.selectionRange`
- property type: `SelectionRangeClientCapabilities` defined as follows:


``` highlight
export interface SelectionRangeClientCapabilities {
    /**
     * Whether implementation supports dynamic registration for selection range
     * providers. If this is set to `true` the client supports the new
     * `SelectionRangeRegistrationOptions` return value for the corresponding
     * server capability as well.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `selectionRangeProvider`
- property type: `boolean | SelectionRangeOptions | SelectionRangeRegistrationOptions` where `SelectionRangeOptions` is defined as follows:


``` highlight
export interface SelectionRangeOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `SelectionRangeRegistrationOptions` defined as follows:


``` highlight
export interface SelectionRangeRegistrationOptions extends
    SelectionRangeOptions, TextDocumentRegistrationOptions,
    StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/selectionRange`
- params: `SelectionRangeParams` defined as follows:


``` highlight
export interface SelectionRangeParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The positions inside the text document.
     */
    positions: Position[];
}
```


*Response*:

- result: `SelectionRange[] | null` defined as follows:


``` highlight
export interface SelectionRange {
    /**
     * The [range](#Range) of this selection range.
     */
    range: Range;
    /**
     * The parent selection range containing this range. Therefore
     * `parent.range` must contain `this.range`.
     */
    parent?: SelectionRange;
}
```


- partial result: `SelectionRange[]`
- error: code and message set in case an exception happens during the ‘textDocument/selectionRange’ request

#### Document Symbols Request (↩)

The document symbol request is sent from the client to the server. The returned result is either

- `SymbolInformation[]` which is a flat list of all symbols found in a given text document. Then neither the symbol’s location range nor the symbol’s container name should be used to infer a hierarchy.
- `DocumentSymbol[]` which is a hierarchy of symbols found in a given text document.

Servers should whenever possible return `DocumentSymbol` since it is the richer data structure.

*Client Capability*:

- property name (optional): `textDocument.documentSymbol`
- property type: `DocumentSymbolClientCapabilities` defined as follows:


``` highlight
export interface DocumentSymbolClientCapabilities {
    /**
     * Whether document symbol supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * Specific capabilities for the `SymbolKind` in the
     * `textDocument/documentSymbol` request.
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
     * The client supports hierarchical document symbols.
     */
    hierarchicalDocumentSymbolSupport?: boolean;

    /**
     * The client supports tags on `SymbolInformation`. Tags are supported on
     * `DocumentSymbol` if `hierarchicalDocumentSymbolSupport` is set to true.
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
     * The client supports an additional label presented in the UI when
     * registering a document symbol provider.
     *
     * @since 3.16.0
     */
    labelSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `documentSymbolProvider`
- property type: `boolean | DocumentSymbolOptions` where `DocumentSymbolOptions` is defined as follows:


``` highlight
export interface DocumentSymbolOptions extends WorkDoneProgressOptions {
    /**
     * A human-readable string that is shown when multiple outlines trees
     * are shown for the same document.
     *
     * @since 3.16.0
     */
    label?: string;
}
```


*Registration Options*: `DocumentSymbolRegistrationOptions` defined as follows:


``` highlight
export interface DocumentSymbolRegistrationOptions extends
    TextDocumentRegistrationOptions, DocumentSymbolOptions {
}
```


*Request*:

- method: `textDocument/documentSymbol`
- params: `DocumentSymbolParams` defined as follows:


``` highlight
export interface DocumentSymbolParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;
}
```


*Response*:

- result: `DocumentSymbol[]` \| `SymbolInformation[]` \| `null` defined as follows:


``` highlight
/**
 * A symbol kind.
 */
export namespace SymbolKind {
    export const File = 1;
    export const Module = 2;
    export const Namespace = 3;
    export const Package = 4;
    export const Class = 5;
    export const Method = 6;
    export const Property = 7;
    export const Field = 8;
    export const Constructor = 9;
    export const Enum = 10;
    export const Interface = 11;
    export const Function = 12;
    export const Variable = 13;
    export const Constant = 14;
    export const String = 15;
    export const Number = 16;
    export const Boolean = 17;
    export const Array = 18;
    export const Object = 19;
    export const Key = 20;
    export const Null = 21;
    export const EnumMember = 22;
    export const Struct = 23;
    export const Event = 24;
    export const Operator = 25;
    export const TypeParameter = 26;
}

export type SymbolKind = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 24 | 25 | 26;
```


``` highlight
/**
 * Symbol tags are extra annotations that tweak the rendering of a symbol.
 *
 * @since 3.16
 */
export namespace SymbolTag {

    /**
     * Render a symbol as obsolete, usually using a strike-out.
     */
    export const Deprecated: 1 = 1;
}

export type SymbolTag = 1;
```


``` highlight
/**
 * Represents programming constructs like variables, classes, interfaces etc.
 * that appear in a document. Document symbols can be hierarchical and they
 * have two ranges: one that encloses its definition and one that points to its
 * most interesting range, e.g. the range of an identifier.
 */
export interface DocumentSymbol {

    /**
     * The name of this symbol. Will be displayed in the user interface and
     * therefore must not be an empty string or a string only consisting of
     * white spaces.
     */
    name: string;

    /**
     * More detail for this symbol, e.g the signature of a function.
     */
    detail?: string;

    /**
     * The kind of this symbol.
     */
    kind: SymbolKind;

    /**
     * Tags for this document symbol.
     *
     * @since 3.16.0
     */
    tags?: SymbolTag[];

    /**
     * Indicates if this symbol is deprecated.
     *
     * @deprecated Use tags instead
     */
    deprecated?: boolean;

    /**
     * The range enclosing this symbol not including leading/trailing whitespace
     * but everything else like comments. This information is typically used to
     * determine if the clients cursor is inside the symbol to reveal it  in the
     * UI.
     */
    range: Range;

    /**
     * The range that should be selected and revealed when this symbol is being
     * picked, e.g. the name of a function. Must be contained by the `range`.
     */
    selectionRange: Range;

    /**
     * Children of this symbol, e.g. properties of a class.
     */
    children?: DocumentSymbol[];
}
```


``` highlight
/**
 * Represents information about programming constructs like variables, classes,
 * interfaces etc.
 *
 * @deprecated use DocumentSymbol or WorkspaceSymbol instead.
 */
export interface SymbolInformation {
    /**
     * The name of this symbol.
     */
    name: string;

    /**
     * The kind of this symbol.
     */
    kind: SymbolKind;

    /**
     * Tags for this symbol.
     *
     * @since 3.16.0
     */
    tags?: SymbolTag[];

    /**
     * Indicates if this symbol is deprecated.
     *
     * @deprecated Use tags instead
     */
    deprecated?: boolean;

    /**
     * The location of this symbol. The location's range is used by a tool
     * to reveal the location in the editor. If the symbol is selected in the
     * tool the range's start information is used to position the cursor. So
     * the range usually spans more then the actual symbol's name and does
     * normally include things like visibility modifiers.
     *
     * The range doesn't have to denote a node range in the sense of an abstract
     * syntax tree. It can therefore not be used to re-construct a hierarchy of
     * the symbols.
     */
    location: Location;

    /**
     * The name of the symbol containing this symbol. This information is for
     * user interface purposes (e.g. to render a qualifier in the user interface
     * if necessary). It can't be used to re-infer a hierarchy for the document
     * symbols.
     */
    containerName?: string;
}
```


- partial result: `DocumentSymbol[]` \| `SymbolInformation[]`. `DocumentSymbol[]` and `SymbolInformation[]` can not be mixed. That means the first chunk defines the type of all the other chunks.
- error: code and message set in case an exception happens during the document symbol request.

#### Semantic Tokens (↩)

> *Since version 3.16.0*

The request is sent from the client to the server to resolve semantic tokens for a given file. Semantic tokens are used to add additional color information to a file that depends on language specific symbol information. A semantic token request usually produces a large result. The protocol therefore supports encoding tokens with numbers. In addition optional support for deltas is available.

*General Concepts*

Tokens are represented using one token type combined with n token modifiers. A token type is something like `class` or `function` and token modifiers are like `static` or `async`. The protocol defines a set of token types and modifiers but clients are allowed to extend these and announce the values they support in the corresponding client capability. The predefined values are:


``` highlight
export enum SemanticTokenTypes {
    namespace = 'namespace',
    /**
     * Represents a generic type. Acts as a fallback for types which
     * can't be mapped to a specific type like class or enum.
     */
    type = 'type',
    class = 'class',
    enum = 'enum',
    interface = 'interface',
    struct = 'struct',
    typeParameter = 'typeParameter',
    parameter = 'parameter',
    variable = 'variable',
    property = 'property',
    enumMember = 'enumMember',
    event = 'event',
    function = 'function',
    method = 'method',
    macro = 'macro',
    keyword = 'keyword',
    modifier = 'modifier',
    comment = 'comment',
    string = 'string',
    number = 'number',
    regexp = 'regexp',
    operator = 'operator',
    /**
     * @since 3.17.0
     */
    decorator = 'decorator'
}
```


``` highlight
export enum SemanticTokenModifiers {
    declaration = 'declaration',
    definition = 'definition',
    readonly = 'readonly',
    static = 'static',
    deprecated = 'deprecated',
    abstract = 'abstract',
    async = 'async',
    modification = 'modification',
    documentation = 'documentation',
    defaultLibrary = 'defaultLibrary'
}
```


The protocol defines an additional token format capability to allow future extensions of the format. The only format that is currently specified is `relative` expressing that the tokens are described using relative positions (see Integer Encoding for Tokens below).


``` highlight
export namespace TokenFormat {
    export const Relative: 'relative' = 'relative';
}

export type TokenFormat = 'relative';
```


*Integer Encoding for Tokens*

On the capability level types and modifiers are defined using strings. However the real encoding happens using numbers. The server therefore needs to let the client know which numbers it is using for which types and modifiers. They do so using a legend, which is defined as follows:


``` highlight
export interface SemanticTokensLegend {
    /**
     * The token types a server uses.
     */
    tokenTypes: string[];

    /**
     * The token modifiers a server uses.
     */
    tokenModifiers: string[];
}
```


Token types are looked up by index, so a `tokenType` value of `1` means `tokenTypes[1]`. Since a token type can have n modifiers, multiple token modifiers can be set by using bit flags, so a `tokenModifier` value of `3` is first viewed as binary `0b00000011`, which means `[tokenModifiers[0], tokenModifiers[1]]` because bits 0 and 1 are set.

There are different ways how the position of a token can be expressed in a file. Absolute positions or relative positions. The protocol for the token format `relative` uses relative positions, because most tokens remain stable relative to each other when edits are made in a file. This simplifies the computation of a delta if a server supports it. So each token is represented using 5 integers. A specific token `i` in the file consists of the following array indices:

- at index `5*i` - `deltaLine`: token line number, relative to the start of the previous token
- at index `5*i+1` - `deltaStart`: token start character, relative to the start of the previous token (relative to 0 or the previous token’s start if they are on the same line)
- at index `5*i+2` - `length`: the length of the token.
- at index `5*i+3` - `tokenType`: will be looked up in `SemanticTokensLegend.tokenTypes`. We currently ask that `tokenType` \< 65536.
- at index `5*i+4` - `tokenModifiers`: each set bit will be looked up in `SemanticTokensLegend.tokenModifiers`

The `deltaStart` and the `length` values must be encoded using the encoding the client and server agrees on during the `initialize` request (see also [TextDocuments](#textDocuments)). Whether a token can span multiple lines is defined by the client capability `multilineTokenSupport`. If multiline tokens are not supported and a tokens length takes it past the end of the line, it should be treated as if the token ends at the end of the line and will not wrap onto the next line.

The client capability `overlappingTokenSupport` defines whether tokens can overlap each other.

Lets look at a concrete example which uses single line tokens without overlaps for encoding a file with 3 tokens in a number array. We start with absolute positions to demonstrate how they can easily be transformed into relative positions:


``` highlight
{ line: 2, startChar:  5, length: 3, tokenType: "property",
    tokenModifiers: ["private", "static"]
},
{ line: 2, startChar: 10, length: 4, tokenType: "type", tokenModifiers: [] },
{ line: 5, startChar:  2, length: 7, tokenType: "class", tokenModifiers: [] }
```


First of all, a legend must be devised. This legend must be provided up-front on registration and capture all possible token types and modifiers. For the example we use this legend:


``` highlight
{
   tokenTypes: ['property', 'type', 'class'],
   tokenModifiers: ['private', 'static']
}
```


The first transformation step is to encode `tokenType` and `tokenModifiers` as integers using the legend. As said, token types are looked up by index, so a `tokenType` value of `1` means `tokenTypes[1]`. Multiple token modifiers can be set by using bit flags, so a `tokenModifier` value of `3` is first viewed as binary `0b00000011`, which means `[tokenModifiers[0], tokenModifiers[1]]` because bits 0 and 1 are set. Using this legend, the tokens now are:


``` highlight
{ line: 2, startChar:  5, length: 3, tokenType: 0, tokenModifiers: 3 },
{ line: 2, startChar: 10, length: 4, tokenType: 1, tokenModifiers: 0 },
{ line: 5, startChar:  2, length: 7, tokenType: 2, tokenModifiers: 0 }
```


The next step is to represent each token relative to the previous token in the file. In this case, the second token is on the same line as the first token, so the `startChar` of the second token is made relative to the `startChar` of the first token, so it will be `10 - 5`. The third token is on a different line than the second token, so the `startChar` of the third token will not be altered:


``` highlight
{ deltaLine: 2, deltaStartChar: 5, length: 3, tokenType: 0, tokenModifiers: 3 },
{ deltaLine: 0, deltaStartChar: 5, length: 4, tokenType: 1, tokenModifiers: 0 },
{ deltaLine: 3, deltaStartChar: 2, length: 7, tokenType: 2, tokenModifiers: 0 }
```


Finally, the last step is to inline each of the 5 fields for a token in a single array, which is a memory friendly representation:


``` highlight
// 1st token,  2nd token,  3rd token
[  2,5,3,0,3,  0,5,4,1,0,  3,2,7,2,0 ]
```


Now assume that the user types a new empty line at the beginning of the file which results in the following tokens in the file:


``` highlight
{ line: 3, startChar:  5, length: 3, tokenType: "property",
    tokenModifiers: ["private", "static"]
},
{ line: 3, startChar: 10, length: 4, tokenType: "type", tokenModifiers: [] },
{ line: 6, startChar:  2, length: 7, tokenType: "class", tokenModifiers: [] }
```


Running the same transformations as above will result in the following number array:


``` highlight
// 1st token,  2nd token,  3rd token
[  3,5,3,0,3,  0,5,4,1,0,  3,2,7,2,0]
```


The delta is now expressed on these number arrays without any form of interpretation what these numbers mean. This is comparable to the text document edits send from the server to the client to modify the content of a file. Those are character based and don’t make any assumption about the meaning of the characters. So `[ 2,5,3,0,3, 0,5,4,1,0, 3,2,7,2,0 ]` can be transformed into `[ 3,5,3,0,3, 0,5,4,1,0, 3,2,7,2,0]` using the following edit description: `{ start: 0, deleteCount: 1, data: [3] }` which tells the client to simply replace the first number (e.g. `2`) in the array with `3`.

Semantic token edits behave conceptually like [text edits](#textEditArray) on documents: if an edit description consists of n edits all n edits are based on the same state Sm of the number array. They will move the number array from state Sm to Sm+1. A client applying the edits must not assume that they are sorted. An easy algorithm to apply them to the number array is to sort the edits and apply them from the back to the front of the number array.

*Client Capability*:

The following client capabilities are defined for semantic token requests sent from the client to the server:

- property name (optional): `textDocument.semanticTokens`
- property type: `SemanticTokensClientCapabilities` defined as follows:


``` highlight
interface SemanticTokensClientCapabilities {
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true` the client supports the new `(TextDocumentRegistrationOptions &
     * StaticRegistrationOptions)` return value for the corresponding server
     * capability as well.
     */
    dynamicRegistration?: boolean;

    /**
     * Which requests the client supports and might send to the server
     * depending on the server's capability. Please note that clients might not
     * show semantic tokens or degrade some of the user experience if a range
     * or full request is advertised by the client but not provided by the
     * server. If for example the client capability `requests.full` and
     * `request.range` are both set to true but the server only provides a
     * range provider the client might not render a minimap correctly or might
     * even decide to not show any semantic tokens at all.
     */
    requests: {
        /**
         * The client will send the `textDocument/semanticTokens/range` request
         * if the server provides a corresponding handler.
         */
        range?: boolean | {
        };

        /**
         * The client will send the `textDocument/semanticTokens/full` request
         * if the server provides a corresponding handler.
         */
        full?: boolean | {
            /**
             * The client will send the `textDocument/semanticTokens/full/delta`
             * request if the server provides a corresponding handler.
             */
            delta?: boolean;
        };
    };

    /**
     * The token types that the client supports.
     */
    tokenTypes: string[];

    /**
     * The token modifiers that the client supports.
     */
    tokenModifiers: string[];

    /**
     * The formats the clients supports.
     */
    formats: TokenFormat[];

    /**
     * Whether the client supports tokens that can overlap each other.
     */
    overlappingTokenSupport?: boolean;

    /**
     * Whether the client supports tokens that can span multiple lines.
     */
    multilineTokenSupport?: boolean;

    /**
     * Whether the client allows the server to actively cancel a
     * semantic token request, e.g. supports returning
     * ErrorCodes.ServerCancelled. If a server does the client
     * needs to retrigger the request.
     *
     * @since 3.17.0
     */
    serverCancelSupport?: boolean;

    /**
     * Whether the client uses semantic tokens to augment existing
     * syntax tokens. If set to `true` client side created syntax
     * tokens and semantic tokens are both used for colorization. If
     * set to `false` the client only uses the returned semantic tokens
     * for colorization.
     *
     * If the value is `undefined` then the client behavior is not
     * specified.
     *
     * @since 3.17.0
     */
    augmentsSyntaxTokens?: boolean;
}
```


*Server Capability*:

The following server capabilities are defined for semantic tokens:

- property name (optional): `semanticTokensProvider`
- property type: `SemanticTokensOptions | SemanticTokensRegistrationOptions` where `SemanticTokensOptions` is defined as follows:


``` highlight
export interface SemanticTokensOptions extends WorkDoneProgressOptions {
    /**
     * The legend used by the server
     */
    legend: SemanticTokensLegend;

    /**
     * Server supports providing semantic tokens for a specific range
     * of a document.
     */
    range?: boolean | {
    };

    /**
     * Server supports providing semantic tokens for a full document.
     */
    full?: boolean | {
        /**
         * The server supports deltas for full documents.
         */
        delta?: boolean;
    };
}
```


*Registration Options*: `SemanticTokensRegistrationOptions` defined as follows:


``` highlight
export interface SemanticTokensRegistrationOptions extends
    TextDocumentRegistrationOptions, SemanticTokensOptions,
    StaticRegistrationOptions {
}
```


Since the registration option handles range, full and delta requests the method used to register for semantic tokens requests is `textDocument/semanticTokens` and not one of the specific methods described below.

**Requesting semantic tokens for a whole file**

*Request*:


- method: `textDocument/semanticTokens/full`
- params: `SemanticTokensParams` defined as follows:


``` highlight
export interface SemanticTokensParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;
}
```


*Response*:

- result: `SemanticTokens | null` where `SemanticTokens` is defined as follows:


``` highlight
export interface SemanticTokens {
    /**
     * An optional result id. If provided and clients support delta updating
     * the client will include the result id in the next semantic token request.
     * A server can then instead of computing all semantic tokens again simply
     * send a delta.
     */
    resultId?: string;

    /**
     * The actual tokens.
     */
    data: uinteger[];
}
```


- partial result: `SemanticTokensPartialResult` defines as follows:


``` highlight
export interface SemanticTokensPartialResult {
    data: uinteger[];
}
```


- error: code and message set in case an exception happens during the ‘textDocument/semanticTokens/full’ request

**Requesting semantic token delta for a whole file**

*Request*:


- method: `textDocument/semanticTokens/full/delta`
- params: `SemanticTokensDeltaParams` defined as follows:


``` highlight
export interface SemanticTokensDeltaParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The result id of a previous response. The result Id can either point to
     * a full response or a delta response depending on what was received last.
     */
    previousResultId: string;
}
```


*Response*:

- result: `SemanticTokens | SemanticTokensDelta | null` where `SemanticTokensDelta` is defined as follows:


``` highlight
export interface SemanticTokensDelta {
    readonly resultId?: string;
    /**
     * The semantic token edits to transform a previous result into a new
     * result.
     */
    edits: SemanticTokensEdit[];
}
```


``` highlight
export interface SemanticTokensEdit {
    /**
     * The start offset of the edit.
     */
    start: uinteger;

    /**
     * The count of elements to remove.
     */
    deleteCount: uinteger;

    /**
     * The elements to insert.
     */
    data?: uinteger[];
}
```


- partial result: `SemanticTokensDeltaPartialResult` defines as follows:


``` highlight
export interface SemanticTokensDeltaPartialResult {
    edits: SemanticTokensEdit[];
}
```


- error: code and message set in case an exception happens during the ‘textDocument/semanticTokens/full/delta’ request

**Requesting semantic tokens for a range**

There are two uses cases where it can be beneficial to only compute semantic tokens for a visible range:

- for faster rendering of the tokens in the user interface when a user opens a file. In this use cases servers should also implement the `textDocument/semanticTokens/full` request as well to allow for flicker free scrolling and semantic coloring of a minimap.
- if computing semantic tokens for a full document is too expensive servers can only provide a range call. In this case the client might not render a minimap correctly or might even decide to not show any semantic tokens at all.

A server is allowed to compute the semantic tokens for a broader range than requested by the client. However if the server does the semantic tokens for the broader range must be complete and correct. If a token at the beginning or end only partially overlaps with the requested range the server should include those tokens in the response.

*Request*:


- method: `textDocument/semanticTokens/range`
- params: `SemanticTokensRangeParams` defined as follows:


``` highlight
export interface SemanticTokensRangeParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The range the semantic tokens are requested for.
     */
    range: Range;
}
```


*Response*:

- result: `SemanticTokens | null`
- partial result: `SemanticTokensPartialResult`
- error: code and message set in case an exception happens during the ‘textDocument/semanticTokens/range’ request

**Requesting a refresh of all semantic tokens**

The `workspace/semanticTokens/refresh` request is sent from the server to the client. Servers can use it to ask clients to refresh the editors for which this server provides semantic tokens. As a result the client should ask the server to recompute the semantic tokens for these editors. This is useful if a server detects a project wide configuration change which requires a re-calculation of all semantic tokens. Note that the client still has the freedom to delay the re-calculation of the semantic tokens if for example an editor is currently not visible.

*Client Capability*:

- property name (optional): `workspace.semanticTokens`
- property type: `SemanticTokensWorkspaceClientCapabilities` defined as follows:


``` highlight
export interface SemanticTokensWorkspaceClientCapabilities {
    /**
     * Whether the client implementation supports a refresh request sent from
     * the server to the client.
     *
     * Note that this event is global and will force the client to refresh all
     * semantic tokens currently shown. It should be used with absolute care
     * and is useful for situation where a server for example detect a project
     * wide change that requires such a calculation.
     */
    refreshSupport?: boolean;
}
```


*Request*:


- method: `workspace/semanticTokens/refresh`
- params: none

*Response*:

- result: void
- error: code and message set in case an exception happens during the ‘workspace/semanticTokens/refresh’ request

#### Inlay Hint Request (↩)

> *Since version 3.17.0*

The inlay hints request is sent from the client to the server to compute inlay hints for a given \[text document, range\] tuple that may be rendered in the editor in place with other text.

*Client Capability*:

- property name (optional): `textDocument.inlayHint`
- property type: `InlayHintClientCapabilities` defined as follows:


``` highlight
/**
 * Inlay hint client capabilities.
 *
 * @since 3.17.0
 */
export interface InlayHintClientCapabilities {

    /**
     * Whether inlay hints support dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * Indicates which properties a client can resolve lazily on an inlay
     * hint.
     */
    resolveSupport?: {

        /**
         * The properties that a client can resolve lazily.
         */
        properties: string[];
    };
}
```


*Server Capability*:

- property name (optional): `inlayHintProvider`
- property type: `InlayHintOptions` defined as follows:


``` highlight
/**
 * Inlay hint options used during static registration.
 *
 * @since 3.17.0
 */
export interface InlayHintOptions extends WorkDoneProgressOptions {
    /**
     * The server provides support to resolve additional
     * information for an inlay hint item.
     */
    resolveProvider?: boolean;
}
```


*Registration Options*: `InlayHintRegistrationOptions` defined as follows:


``` highlight
/**
 * Inlay hint options used during static or dynamic registration.
 *
 * @since 3.17.0
 */
export interface InlayHintRegistrationOptions extends InlayHintOptions,
    TextDocumentRegistrationOptions, StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/inlayHint`
- params: `InlayHintParams` defined as follows:


``` highlight
/**
 * A parameter literal used in inlay hint requests.
 *
 * @since 3.17.0
 */
export interface InlayHintParams extends WorkDoneProgressParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The visible document range for which inlay hints should be computed.
     */
    range: Range;
}
```


*Response*:

- result: `InlayHint[]` \| `null` defined as follows:


``` highlight
/**
 * Inlay hint information.
 *
 * @since 3.17.0
 */
export interface InlayHint {

    /**
     * The position of this hint.
     *
     * If multiple hints have the same position, they will be shown in the order
     * they appear in the response.
     */
    position: Position;

    /**
     * The label of this hint. A human readable string or an array of
     * InlayHintLabelPart label parts.
     *
     * *Note* that neither the string nor the label part can be empty.
     */
    label: string | InlayHintLabelPart[];

    /**
     * The kind of this hint. Can be omitted in which case the client
     * should fall back to a reasonable default.
     */
    kind?: InlayHintKind;

    /**
     * Optional text edits that are performed when accepting this inlay hint.
     *
     * *Note* that edits are expected to change the document so that the inlay
     * hint (or its nearest variant) is now part of the document and the inlay
     * hint itself is now obsolete.
     *
     * Depending on the client capability `inlayHint.resolveSupport` clients
     * might resolve this property late using the resolve request.
     */
    textEdits?: TextEdit[];

    /**
     * The tooltip text when you hover over this item.
     *
     * Depending on the client capability `inlayHint.resolveSupport` clients
     * might resolve this property late using the resolve request.
     */
    tooltip?: string | MarkupContent;

    /**
     * Render padding before the hint.
     *
     * Note: Padding should use the editor's background color, not the
     * background color of the hint itself. That means padding can be used
     * to visually align/separate an inlay hint.
     */
    paddingLeft?: boolean;

    /**
     * Render padding after the hint.
     *
     * Note: Padding should use the editor's background color, not the
     * background color of the hint itself. That means padding can be used
     * to visually align/separate an inlay hint.
     */
    paddingRight?: boolean;


    /**
     * A data entry field that is preserved on an inlay hint between
     * a `textDocument/inlayHint` and a `inlayHint/resolve` request.
     */
    data?: LSPAny;
}
```


``` highlight
/**
 * An inlay hint label part allows for interactive and composite labels
 * of inlay hints.
 *
 * @since 3.17.0
 */
export interface InlayHintLabelPart {

    /**
     * The value of this label part.
     */
    value: string;

    /**
     * The tooltip text when you hover over this label part. Depending on
     * the client capability `inlayHint.resolveSupport` clients might resolve
     * this property late using the resolve request.
     */
    tooltip?: string | MarkupContent;

    /**
     * An optional source code location that represents this
     * label part.
     *
     * The editor will use this location for the hover and for code navigation
     * features: This part will become a clickable link that resolves to the
     * definition of the symbol at the given location (not necessarily the
     * location itself), it shows the hover that shows at the given location,
     * and it shows a context menu with further code navigation commands.
     *
     * Depending on the client capability `inlayHint.resolveSupport` clients
     * might resolve this property late using the resolve request.
     */
    location?: Location;

    /**
     * An optional command for this label part.
     *
     * Depending on the client capability `inlayHint.resolveSupport` clients
     * might resolve this property late using the resolve request.
     */
    command?: Command;
}
```


``` highlight
/**
 * Inlay hint kinds.
 *
 * @since 3.17.0
 */
export namespace InlayHintKind {

    /**
     * An inlay hint that for a type annotation.
     */
    export const Type = 1;

    /**
     * An inlay hint that is for a parameter.
     */
    export const Parameter = 2;
}

export type InlayHintKind = 1 | 2;
```


- error: code and message set in case an exception happens during the inlay hint request.

#### Inlay Hint Resolve Request (↩)

> *Since version 3.17.0*

The request is sent from the client to the server to resolve additional information for a given inlay hint. This is usually used to compute the `tooltip`, `location` or `command` properties of an inlay hint’s label part to avoid its unnecessary computation during the `textDocument/inlayHint` request.

Consider the client announcing the `label.location` property as a property that can be resolved lazily using the client capability


``` highlight
textDocument.inlayHint.resolveSupport = { properties: ['label.location'] };
```


then an inlay hint with a label part without a location needs to be resolved using the `inlayHint/resolve` request before it can be used.

*Client Capability*:

- property name (optional): `textDocument.inlayHint.resolveSupport`
- property type: `{ properties: string[]; }`

*Request*:

- method: `inlayHint/resolve`
- params: `InlayHint`

*Response*:

- result: `InlayHint`
- error: code and message set in case an exception happens during the completion resolve request.

#### Inlay Hint Refresh Request (↪)

> *Since version 3.17.0*

The `workspace/inlayHint/refresh` request is sent from the server to the client. Servers can use it to ask clients to refresh the inlay hints currently shown in editors. As a result the client should ask the server to recompute the inlay hints for these editors. This is useful if a server detects a configuration change which requires a re-calculation of all inlay hints. Note that the client still has the freedom to delay the re-calculation of the inlay hints if for example an editor is currently not visible.

*Client Capability*:

- property name (optional): `workspace.inlayHint`
- property type: `InlayHintWorkspaceClientCapabilities` defined as follows:


``` highlight
/**
 * Client workspace capabilities specific to inlay hints.
 *
 * @since 3.17.0
 */
export interface InlayHintWorkspaceClientCapabilities {
    /**
     * Whether the client implementation supports a refresh request sent from
     * the server to the client.
     *
     * Note that this event is global and will force the client to refresh all
     * inlay hints currently shown. It should be used with absolute care and
     * is useful for situation where a server for example detects a project wide
     * change that requires such a calculation.
     */
    refreshSupport?: boolean;
}
```


*Request*:

- method: `workspace/inlayHint/refresh`
- params: none

*Response*:

- result: void
- error: code and message set in case an exception happens during the ‘workspace/inlayHint/refresh’ request

#### Inline Value Request (↩)

> *Since version 3.17.0*

The inline value request is sent from the client to the server to compute inline values for a given text document that may be rendered in the editor at the end of lines.

*Client Capability*:

- property name (optional): `textDocument.inlineValue`
- property type: `InlineValueClientCapabilities` defined as follows:


``` highlight
/**
 * Client capabilities specific to inline values.
 *
 * @since 3.17.0
 */
export interface InlineValueClientCapabilities {
    /**
     * Whether implementation supports dynamic registration for inline
     * value providers.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `inlineValueProvider`
- property type: `InlineValueOptions` defined as follows:


``` highlight
/**
 * Inline value options used during static registration.
 *
 * @since 3.17.0
 */
export interface InlineValueOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `InlineValueRegistrationOptions` defined as follows:


``` highlight
/**
 * Inline value options used during static or dynamic registration.
 *
 * @since 3.17.0
 */
export interface InlineValueRegistrationOptions extends InlineValueOptions,
    TextDocumentRegistrationOptions, StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/inlineValue`
- params: `InlineValueParams` defined as follows:


``` highlight
/**
 * A parameter literal used in inline value requests.
 *
 * @since 3.17.0
 */
export interface InlineValueParams extends WorkDoneProgressParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The document range for which inline values should be computed.
     */
    range: Range;

    /**
     * Additional information about the context in which inline values were
     * requested.
     */
    context: InlineValueContext;
}
```


``` highlight
/**
 * @since 3.17.0
 */
export interface InlineValueContext {
    /**
     * The stack frame (as a DAP Id) where the execution has stopped.
     */
    frameId: integer;

    /**
     * The document range where execution has stopped.
     * Typically the end position of the range denotes the line where the
     * inline values are shown.
     */
    stoppedLocation: Range;
}
```


*Response*:

- result: `InlineValue[]` \| `null` defined as follows:


``` highlight
/**
 * Provide inline value as text.
 *
 * @since 3.17.0
 */
export interface InlineValueText {
    /**
     * The document range for which the inline value applies.
     */
    range: Range;

    /**
     * The text of the inline value.
     */
    text: string;
}
```


``` highlight
/**
 * Provide inline value through a variable lookup.
 *
 * If only a range is specified, the variable name will be extracted from
 * the underlying document.
 *
 * An optional variable name can be used to override the extracted name.
 *
 * @since 3.17.0
 */
export interface InlineValueVariableLookup {
    /**
     * The document range for which the inline value applies.
     * The range is used to extract the variable name from the underlying
     * document.
     */
    range: Range;

    /**
     * If specified the name of the variable to look up.
     */
    variableName?: string;

    /**
     * How to perform the lookup.
     */
    caseSensitiveLookup: boolean;
}
```


``` highlight
/**
 * Provide an inline value through an expression evaluation.
 *
 * If only a range is specified, the expression will be extracted from the
 * underlying document.
 *
 * An optional expression can be used to override the extracted expression.
 *
 * @since 3.17.0
 */
export interface InlineValueEvaluatableExpression {
    /**
     * The document range for which the inline value applies.
     * The range is used to extract the evaluatable expression from the
     * underlying document.
     */
    range: Range;

    /**
     * If specified the expression overrides the extracted expression.
     */
    expression?: string;
}
```


``` highlight
/**
 * Inline value information can be provided by different means:
 * - directly as a text value (class InlineValueText).
 * - as a name to use for a variable lookup (class InlineValueVariableLookup)
 * - as an evaluatable expression (class InlineValueEvaluatableExpression)
 * The InlineValue types combines all inline value types into one type.
 *
 * @since 3.17.0
 */
export type InlineValue = InlineValueText | InlineValueVariableLookup
    | InlineValueEvaluatableExpression;
```


- error: code and message set in case an exception happens during the inline values request.

#### Inline Value Refresh Request (↪)

> *Since version 3.17.0*

The `workspace/inlineValue/refresh` request is sent from the server to the client. Servers can use it to ask clients to refresh the inline values currently shown in editors. As a result the client should ask the server to recompute the inline values for these editors. This is useful if a server detects a configuration change which requires a re-calculation of all inline values. Note that the client still has the freedom to delay the re-calculation of the inline values if for example an editor is currently not visible.

*Client Capability*:

- property name (optional): `workspace.inlineValue`
- property type: `InlineValueWorkspaceClientCapabilities` defined as follows:


``` highlight
/**
 * Client workspace capabilities specific to inline values.
 *
 * @since 3.17.0
 */
export interface InlineValueWorkspaceClientCapabilities {
    /**
     * Whether the client implementation supports a refresh request sent from
     * the server to the client.
     *
     * Note that this event is global and will force the client to refresh all
     * inline values currently shown. It should be used with absolute care and
     * is useful for situation where a server for example detect a project wide
     * change that requires such a calculation.
     */
    refreshSupport?: boolean;
}
```


*Request*:

- method: `workspace/inlineValue/refresh`
- params: none

*Response*:

- result: void
- error: code and message set in case an exception happens during the ‘workspace/inlineValue/refresh’ request

#### Monikers (↩)

> *Since version 3.16.0*

Language Server Index Format (LSIF) introduced the concept of symbol monikers to help associate symbols across different indexes. This request adds capability for LSP server implementations to provide the same symbol moniker information given a text document position. Clients can utilize this method to get the moniker at the current location in a file user is editing and do further code navigation queries in other services that rely on LSIF indexes and link symbols together.

The `textDocument/moniker` request is sent from the client to the server to get the symbol monikers for a given text document position. An array of Moniker types is returned as response to indicate possible monikers at the given location. If no monikers can be calculated, an empty array or `null` should be returned.

*Client Capabilities*:

- property name (optional): `textDocument.moniker`
- property type: `MonikerClientCapabilities` defined as follows:


``` highlight
interface MonikerClientCapabilities {
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true` the client supports the new `(TextDocumentRegistrationOptions &
     * StaticRegistrationOptions)` return value for the corresponding server
     * capability as well.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `monikerProvider`
- property type: `boolean | MonikerOptions | MonikerRegistrationOptions` is defined as follows:


``` highlight
export interface MonikerOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `MonikerRegistrationOptions` defined as follows:


``` highlight
export interface MonikerRegistrationOptions extends
    TextDocumentRegistrationOptions, MonikerOptions {
}
```


*Request*:

- method: `textDocument/moniker`
- params: `MonikerParams` defined as follows:


``` highlight
export interface MonikerParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
}
```


*Response*:

- result: `Moniker[] | null`
- partial result: `Moniker[]`
- error: code and message set in case an exception happens during the ‘textDocument/moniker’ request

`Moniker` is defined as follows:


``` highlight
/**
 * Moniker uniqueness level to define scope of the moniker.
 */
export enum UniquenessLevel {
    /**
     * The moniker is only unique inside a document
     */
    document = 'document',

    /**
     * The moniker is unique inside a project for which a dump got created
     */
    project = 'project',

    /**
     * The moniker is unique inside the group to which a project belongs
     */
    group = 'group',

    /**
     * The moniker is unique inside the moniker scheme.
     */
    scheme = 'scheme',

    /**
     * The moniker is globally unique
     */
    global = 'global'
}
```


``` highlight
/**
 * The moniker kind.
 */
export enum MonikerKind {
    /**
     * The moniker represent a symbol that is imported into a project
     */
    import = 'import',

    /**
     * The moniker represents a symbol that is exported from a project
     */
    export = 'export',

    /**
     * The moniker represents a symbol that is local to a project (e.g. a local
     * variable of a function, a class not visible outside the project, ...)
     */
    local = 'local'
}
```


``` highlight
/**
 * Moniker definition to match LSIF 0.5 moniker definition.
 */
export interface Moniker {
    /**
     * The scheme of the moniker. For example tsc or .Net
     */
    scheme: string;

    /**
     * The identifier of the moniker. The value is opaque in LSIF however
     * schema owners are allowed to define the structure if they want.
     */
    identifier: string;

    /**
     * The scope in which the moniker is unique
     */
    unique: UniquenessLevel;

    /**
     * The moniker kind if known.
     */
    kind?: MonikerKind;
}
```


##### Notes

Server implementations of this method should ensure that the moniker calculation matches to those used in the corresponding LSIF implementation to ensure symbols can be associated correctly across IDE sessions and LSIF indexes.

#### Completion Request (↩)

The Completion request is sent from the client to the server to compute completion items at a given cursor position. Completion items are presented in the [IntelliSense](https://code.visualstudio.com/docs/editor/intellisense) user interface. If computing full completion items is expensive, servers can additionally provide a handler for the completion item resolve request (‘completionItem/resolve’). This request is sent when a completion item is selected in the user interface. A typical use case is for example: the `textDocument/completion` request doesn’t fill in the `documentation` property for returned completion items since it is expensive to compute. When the item is selected in the user interface then a ‘completionItem/resolve’ request is sent with the selected completion item as a parameter. The returned completion item should have the documentation property filled in. By default the request can only delay the computation of the `detail` and `documentation` properties. Since 3.16.0 the client can signal that it can resolve more properties lazily. This is done using the `completionItem#resolveSupport` client capability which lists all properties that can be filled in during a ‘completionItem/resolve’ request. All other properties (usually `sortText`, `filterText`, `insertText` and `textEdit`) must be provided in the `textDocument/completion` response and must not be changed during resolve.

The language server protocol uses the following model around completions:

- to achieve consistency across languages and to honor different clients usually the client is responsible for filtering and sorting. This has also the advantage that client can experiment with different filter and sorting models. However servers can enforce different behavior by setting a `filterText` / `sortText`
- for speed clients should be able to filter an already received completion list if the user continues typing. Servers can opt out of this using a `CompletionList` and mark it as `isIncomplete`.

A completion item provides additional means to influence filtering and sorting. They are expressed by either creating a `CompletionItem` with a `insertText` or with a `textEdit`. The two modes differ as follows:

- **Completion item provides an insertText / label without a text edit**: in the model the client should filter against what the user has already typed using the word boundary rules of the language (e.g. resolving the word under the cursor position). The reason for this mode is that it makes it extremely easy for a server to implement a basic completion list and get it filtered on the client.

- **Completion Item with text edits**: in this mode the server tells the client that it actually knows what it is doing. If you create a completion item with a text edit at the current cursor position no word guessing takes place and no automatic filtering (like with an `insertText`) should happen. This mode can be combined with a sort text and filter text to customize two things. If the text edit is a replace edit then the range denotes the word used for filtering. If the replace changes the text it most likely makes sense to specify a filter text to be used.

*Client Capability*:

- property name (optional): `textDocument.completion`
- property type: `CompletionClientCapabilities` defined as follows:


``` highlight
export interface CompletionClientCapabilities {
    /**
     * Whether completion supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports the following `CompletionItem` specific
     * capabilities.
     */
    completionItem?: {
        /**
         * Client supports snippets as insert text.
         *
         * A snippet can define tab stops and placeholders with `$1`, `$2`
         * and `${3:foo}`. `$0` defines the final tab stop, it defaults to
         * the end of the snippet. Placeholders with equal identifiers are
         * linked, that is typing in one will update others too.
         */
        snippetSupport?: boolean;

        /**
         * Client supports commit characters on a completion item.
         */
        commitCharactersSupport?: boolean;

        /**
         * Client supports the follow content formats for the documentation
         * property. The order describes the preferred format of the client.
         */
        documentationFormat?: MarkupKind[];

        /**
         * Client supports the deprecated property on a completion item.
         */
        deprecatedSupport?: boolean;

        /**
         * Client supports the preselect property on a completion item.
         */
        preselectSupport?: boolean;

        /**
         * Client supports the tag property on a completion item. Clients
         * supporting tags have to handle unknown tags gracefully. Clients
         * especially need to preserve unknown tags when sending a completion
         * item back to the server in a resolve call.
         *
         * @since 3.15.0
         */
        tagSupport?: {
            /**
             * The tags supported by the client.
             */
            valueSet: CompletionItemTag[];
        };

        /**
         * Client supports insert replace edit to control different behavior if
         * a completion item is inserted in the text or should replace text.
         *
         * @since 3.16.0
         */
        insertReplaceSupport?: boolean;

        /**
         * Indicates which properties a client can resolve lazily on a
         * completion item. Before version 3.16.0 only the predefined properties
         * `documentation` and `detail` could be resolved lazily.
         *
         * @since 3.16.0
         */
        resolveSupport?: {
            /**
             * The properties that a client can resolve lazily.
             */
            properties: string[];
        };

        /**
         * The client supports the `insertTextMode` property on
         * a completion item to override the whitespace handling mode
         * as defined by the client (see `insertTextMode`).
         *
         * @since 3.16.0
         */
        insertTextModeSupport?: {
            valueSet: InsertTextMode[];
        };

        /**
         * The client has support for completion item label
         * details (see also `CompletionItemLabelDetails`).
         *
         * @since 3.17.0
         */
        labelDetailsSupport?: boolean;
    };

    completionItemKind?: {
        /**
         * The completion item kind values the client supports. When this
         * property exists the client also guarantees that it will
         * handle values outside its set gracefully and falls back
         * to a default value when unknown.
         *
         * If this property is not present the client only supports
         * the completion items kinds from `Text` to `Reference` as defined in
         * the initial version of the protocol.
         */
        valueSet?: CompletionItemKind[];
    };

    /**
     * The client supports to send additional context information for a
     * `textDocument/completion` request.
     */
    contextSupport?: boolean;

    /**
     * The client's default when the completion item doesn't provide a
     * `insertTextMode` property.
     *
     * @since 3.17.0
     */
    insertTextMode?: InsertTextMode;

    /**
     * The client supports the following `CompletionList` specific
     * capabilities.
     *
     * @since 3.17.0
     */
    completionList?: {
        /**
         * The client supports the following itemDefaults on
         * a completion list.
         *
         * The value lists the supported property names of the
         * `CompletionList.itemDefaults` object. If omitted
         * no properties are supported.
         *
         * @since 3.17.0
         */
        itemDefaults?: string[];
    }
}
```


*Server Capability*:

- property name (optional): `completionProvider`
- property type: `CompletionOptions` defined as follows:


``` highlight
/**
 * Completion options.
 */
export interface CompletionOptions extends WorkDoneProgressOptions {
    /**
     * The additional characters, beyond the defaults provided by the client (typically
     * [a-zA-Z]), that should automatically trigger a completion request. For example
     * `.` in JavaScript represents the beginning of an object property or method and is
     * thus a good candidate for triggering a completion request.
     *
     * Most tools trigger a completion request automatically without explicitly
     * requesting it using a keyboard shortcut (e.g. Ctrl+Space). Typically they
     * do so when the user starts to type an identifier. For example if the user
     * types `c` in a JavaScript file code complete will automatically pop up
     * present `console` besides others as a completion item. Characters that
     * make up identifiers don't need to be listed here.
     */
    triggerCharacters?: string[];

    /**
     * The list of all possible characters that commit a completion. This field
     * can be used if clients don't support individual commit characters per
     * completion item. See client capability
     * `completion.completionItem.commitCharactersSupport`.
     *
     * If a server provides both `allCommitCharacters` and commit characters on
     * an individual completion item the ones on the completion item win.
     *
     * @since 3.2.0
     */
    allCommitCharacters?: string[];

    /**
     * The server provides support to resolve additional
     * information for a completion item.
     */
    resolveProvider?: boolean;

    /**
     * The server supports the following `CompletionItem` specific
     * capabilities.
     *
     * @since 3.17.0
     */
    completionItem?: {
        /**
         * The server has support for completion item label
         * details (see also `CompletionItemLabelDetails`) when receiving
         * a completion item in a resolve call.
         *
         * @since 3.17.0
         */
        labelDetailsSupport?: boolean;
    }
}
```


*Registration Options*: `CompletionRegistrationOptions` options defined as follows:


``` highlight
export interface CompletionRegistrationOptions
    extends TextDocumentRegistrationOptions, CompletionOptions {
}
```


*Request*:

- method: `textDocument/completion`
- params: `CompletionParams` defined as follows:


``` highlight
export interface CompletionParams extends TextDocumentPositionParams,
    WorkDoneProgressParams, PartialResultParams {
    /**
     * The completion context. This is only available if the client specifies
     * to send this using the client capability
     * `completion.contextSupport === true`
     */
    context?: CompletionContext;
}
```


``` highlight
/**
 * How a completion was triggered
 */
export namespace CompletionTriggerKind {
    /**
     * Completion was triggered by typing an identifier (24x7 code
     * complete), manual invocation (e.g Ctrl+Space) or via API.
     */
    export const Invoked: 1 = 1;

    /**
     * Completion was triggered by a trigger character specified by
     * the `triggerCharacters` properties of the
     * `CompletionRegistrationOptions`.
     */
    export const TriggerCharacter: 2 = 2;

    /**
     * Completion was re-triggered as the current completion list is incomplete.
     */
    export const TriggerForIncompleteCompletions: 3 = 3;
}
export type CompletionTriggerKind = 1 | 2 | 3;
```


``` highlight
/**
 * Contains additional information about the context in which a completion
 * request is triggered.
 */
export interface CompletionContext {
    /**
     * How the completion was triggered.
     */
    triggerKind: CompletionTriggerKind;

    /**
     * The trigger character (a single character) that has trigger code
     * complete. Is undefined if
     * `triggerKind !== CompletionTriggerKind.TriggerCharacter`
     */
    triggerCharacter?: string;
}
```


*Response*:

- result: `CompletionItem[]` \| `CompletionList` \| `null`. If a `CompletionItem[]` is provided it is interpreted to be complete. So it is the same as `{ isIncomplete: false, items }`


``` highlight
/**
 * Represents a collection of [completion items](#CompletionItem) to be
 * presented in the editor.
 */
export interface CompletionList {
    /**
     * This list is not complete. Further typing should result in recomputing
     * this list.
     *
     * Recomputed lists have all their items replaced (not appended) in the
     * incomplete completion sessions.
     */
    isIncomplete: boolean;

    /**
     * In many cases the items of an actual completion result share the same
     * value for properties like `commitCharacters` or the range of a text
     * edit. A completion list can therefore define item defaults which will
     * be used if a completion item itself doesn't specify the value.
     *
     * If a completion list specifies a default value and a completion item
     * also specifies a corresponding value the one from the item is used.
     *
     * Servers are only allowed to return default values if the client
     * signals support for this via the `completionList.itemDefaults`
     * capability.
     *
     * @since 3.17.0
     */
    itemDefaults?: {
        /**
         * A default commit character set.
         *
         * @since 3.17.0
         */
        commitCharacters?: string[];

        /**
         * A default edit range
         *
         * @since 3.17.0
         */
        editRange?: Range | {
            insert: Range;
            replace: Range;
        };

        /**
         * A default insert text format
         *
         * @since 3.17.0
         */
        insertTextFormat?: InsertTextFormat;

        /**
         * A default insert text mode
         *
         * @since 3.17.0
         */
        insertTextMode?: InsertTextMode;

        /**
         * A default data value.
         *
         * @since 3.17.0
         */
        data?: LSPAny;
    }

    /**
     * The completion items.
     */
    items: CompletionItem[];
}
```


``` highlight
/**
 * Defines whether the insert text in a completion item should be interpreted as
 * plain text or a snippet.
 */
export namespace InsertTextFormat {
    /**
     * The primary text to be inserted is treated as a plain string.
     */
    export const PlainText = 1;

    /**
     * The primary text to be inserted is treated as a snippet.
     *
     * A snippet can define tab stops and placeholders with `$1`, `$2`
     * and `${3:foo}`. `$0` defines the final tab stop, it defaults to
     * the end of the snippet. Placeholders with equal identifiers are linked,
     * that is typing in one will update others too.
     */
    export const Snippet = 2;
}

export type InsertTextFormat = 1 | 2;
```


``` highlight
/**
 * Completion item tags are extra annotations that tweak the rendering of a
 * completion item.
 *
 * @since 3.15.0
 */
export namespace CompletionItemTag {
    /**
     * Render a completion as obsolete, usually using a strike-out.
     */
    export const Deprecated = 1;
}

export type CompletionItemTag = 1;
```


``` highlight
/**
 * A special text edit to provide an insert and a replace operation.
 *
 * @since 3.16.0
 */
export interface InsertReplaceEdit {
    /**
     * The string to be inserted.
     */
    newText: string;

    /**
     * The range if the insert is requested
     */
    insert: Range;

    /**
     * The range if the replace is requested.
     */
    replace: Range;
}
```


``` highlight
/**
 * How whitespace and indentation is handled during completion
 * item insertion.
 *
 * @since 3.16.0
 */
export namespace InsertTextMode {
    /**
     * The insertion or replace strings is taken as it is. If the
     * value is multi line the lines below the cursor will be
     * inserted using the indentation defined in the string value.
     * The client will not apply any kind of adjustments to the
     * string.
     */
    export const asIs: 1 = 1;

    /**
     * The editor adjusts leading whitespace of new lines so that
     * they match the indentation up to the cursor of the line for
     * which the item is accepted.
     *
     * Consider a line like this: <2tabs><cursor><3tabs>foo. Accepting a
     * multi line completion item is indented using 2 tabs and all
     * following lines inserted will be indented using 2 tabs as well.
     */
    export const adjustIndentation: 2 = 2;
}

export type InsertTextMode = 1 | 2;
```


``` highlight
/**
 * Additional details for a completion item label.
 *
 * @since 3.17.0
 */
export interface CompletionItemLabelDetails {

    /**
     * An optional string which is rendered less prominently directly after
     * {@link CompletionItem.label label}, without any spacing. Should be
     * used for function signatures or type annotations.
     */
    detail?: string;

    /**
     * An optional string which is rendered less prominently after
     * {@link CompletionItemLabelDetails.detail}. Should be used for fully qualified
     * names or file path.
     */
    description?: string;
}
```


``` highlight
export interface CompletionItem {

    /**
     * The label of this completion item.
     *
     * The label property is also by default the text that
     * is inserted when selecting this completion.
     *
     * If label details are provided the label itself should
     * be an unqualified name of the completion item.
     */
    label: string;

    /**
     * Additional details for the label
     *
     * @since 3.17.0
     */
    labelDetails?: CompletionItemLabelDetails;


    /**
     * The kind of this completion item. Based of the kind
     * an icon is chosen by the editor. The standardized set
     * of available values is defined in `CompletionItemKind`.
     */
    kind?: CompletionItemKind;

    /**
     * Tags for this completion item.
     *
     * @since 3.15.0
     */
    tags?: CompletionItemTag[];

    /**
     * A human-readable string with additional information
     * about this item, like type or symbol information.
     */
    detail?: string;

    /**
     * A human-readable string that represents a doc-comment.
     */
    documentation?: string | MarkupContent;

    /**
     * Indicates if this item is deprecated.
     *
     * @deprecated Use `tags` instead if supported.
     */
    deprecated?: boolean;

    /**
     * Select this item when showing.
     *
     * *Note* that only one completion item can be selected and that the
     * tool / client decides which item that is. The rule is that the *first*
     * item of those that match best is selected.
     */
    preselect?: boolean;

    /**
     * A string that should be used when comparing this item
     * with other items. When omitted the label is used
     * as the sort text for this item.
     */
    sortText?: string;

    /**
     * A string that should be used when filtering a set of
     * completion items. When omitted the label is used as the
     * filter text for this item.
     */
    filterText?: string;

    /**
     * A string that should be inserted into a document when selecting
     * this completion. When omitted the label is used as the insert text
     * for this item.
     *
     * The `insertText` is subject to interpretation by the client side.
     * Some tools might not take the string literally. For example
     * VS Code when code complete is requested in this example
     * `con<cursor position>` and a completion item with an `insertText` of
     * `console` is provided it will only insert `sole`. Therefore it is
     * recommended to use `textEdit` instead since it avoids additional client
     * side interpretation.
     */
    insertText?: string;

    /**
     * The format of the insert text. The format applies to both the
     * `insertText` property and the `newText` property of a provided
     * `textEdit`. If omitted defaults to `InsertTextFormat.PlainText`.
     *
     * Please note that the insertTextFormat doesn't apply to
     * `additionalTextEdits`.
     */
    insertTextFormat?: InsertTextFormat;

    /**
     * How whitespace and indentation is handled during completion
     * item insertion. If not provided the client's default value depends on
     * the `textDocument.completion.insertTextMode` client capability.
     *
     * @since 3.16.0
     * @since 3.17.0 - support for `textDocument.completion.insertTextMode`
     */
    insertTextMode?: InsertTextMode;

    /**
     * An edit which is applied to a document when selecting this completion.
     * When an edit is provided the value of `insertText` is ignored.
     *
     * *Note:* The range of the edit must be a single line range and it must
     * contain the position at which completion has been requested.
     *
     * Most editors support two different operations when accepting a completion
     * item. One is to insert a completion text and the other is to replace an
     * existing text with a completion text. Since this can usually not be
     * predetermined by a server it can report both ranges. Clients need to
     * signal support for `InsertReplaceEdit`s via the
     * `textDocument.completion.completionItem.insertReplaceSupport` client
     * capability property.
     *
     * *Note 1:* The text edit's range as well as both ranges from an insert
     * replace edit must be a [single line] and they must contain the position
     * at which completion has been requested.
     * *Note 2:* If an `InsertReplaceEdit` is returned the edit's insert range
     * must be a prefix of the edit's replace range, that means it must be
     * contained and starting at the same position.
     *
     * @since 3.16.0 additional type `InsertReplaceEdit`
     */
    textEdit?: TextEdit | InsertReplaceEdit;

    /**
     * The edit text used if the completion item is part of a CompletionList and
     * CompletionList defines an item default for the text edit range.
     *
     * Clients will only honor this property if they opt into completion list
     * item defaults using the capability `completionList.itemDefaults`.
     *
     * If not provided and a list's default range is provided the label
     * property is used as a text.
     *
     * @since 3.17.0
     */
    textEditText?: string;

    /**
     * An optional array of additional text edits that are applied when
     * selecting this completion. Edits must not overlap (including the same
     * insert position) with the main edit nor with themselves.
     *
     * Additional text edits should be used to change text unrelated to the
     * current cursor position (for example adding an import statement at the
     * top of the file if the completion item will insert an unqualified type).
     */
    additionalTextEdits?: TextEdit[];

    /**
     * An optional set of characters that when pressed while this completion is
     * active will accept it first and then type that character. *Note* that all
     * commit characters should have `length=1` and that superfluous characters
     * will be ignored.
     */
    commitCharacters?: string[];

    /**
     * An optional command that is executed *after* inserting this completion.
     * *Note* that additional modifications to the current document should be
     * described with the additionalTextEdits-property.
     */
    command?: Command;

    /**
     * A data entry field that is preserved on a completion item between
     * a completion and a completion resolve request.
     */
    data?: LSPAny;
}
```


``` highlight
/**
 * The kind of a completion entry.
 */
export namespace CompletionItemKind {
    export const Text = 1;
    export const Method = 2;
    export const Function = 3;
    export const Constructor = 4;
    export const Field = 5;
    export const Variable = 6;
    export const Class = 7;
    export const Interface = 8;
    export const Module = 9;
    export const Property = 10;
    export const Unit = 11;
    export const Value = 12;
    export const Enum = 13;
    export const Keyword = 14;
    export const Snippet = 15;
    export const Color = 16;
    export const File = 17;
    export const Reference = 18;
    export const Folder = 19;
    export const EnumMember = 20;
    export const Constant = 21;
    export const Struct = 22;
    export const Event = 23;
    export const Operator = 24;
    export const TypeParameter = 25;
}

export type CompletionItemKind = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 24 | 25;
```


- partial result: `CompletionItem[]` or `CompletionList` followed by `CompletionItem[]`. If the first provided result item is of type `CompletionList` subsequent partial results of `CompletionItem[]` add to the `items` property of the `CompletionList`.
- error: code and message set in case an exception happens during the completion request.

Completion items support snippets (see `InsertTextFormat.Snippet`). The snippet format is as follows:

##### Snippet Syntax

The `body` of a snippet can use special constructs to control cursors and the text being inserted. The following are supported features and their syntaxes:

##### Tab stops

With tab stops, you can make the editor cursor move inside a snippet. Use `$1`, `$2` to specify cursor locations. The number is the order in which tab stops will be visited, whereas `$0` denotes the final cursor position. Multiple tab stops are linked and updated in sync.

##### Placeholders

Placeholders are tab stops with values, like `${1:foo}`. The placeholder text will be inserted and selected such that it can be easily changed. Placeholders can be nested, like `${1:another ${2:placeholder}}`.

##### Choice

Placeholders can have choices as values. The syntax is a comma separated enumeration of values, enclosed with the pipe-character, for example `${1|one,two,three|}`. When the snippet is inserted and the placeholder selected, choices will prompt the user to pick one of the values.

##### Variables

With `$name` or `${name:default}` you can insert the value of a variable. When a variable isn’t set, its *default* or the empty string is inserted. When a variable is unknown (that is, its name isn’t defined) the name of the variable is inserted and it is transformed into a placeholder.

The following variables can be used:

- `TM_SELECTED_TEXT` The currently selected text or the empty string
- `TM_CURRENT_LINE` The contents of the current line
- `TM_CURRENT_WORD` The contents of the word under cursor or the empty string
- `TM_LINE_INDEX` The zero-index based line number
- `TM_LINE_NUMBER` The one-index based line number
- `TM_FILENAME` The filename of the current document
- `TM_FILENAME_BASE` The filename of the current document without its extensions
- `TM_DIRECTORY` The directory of the current document
- `TM_FILEPATH` The full file path of the current document

##### Variable Transforms

Transformations allow you to modify the value of a variable before it is inserted. The definition of a transformation consists of three parts:

1.  A [regular expression](#regExp) that is matched against the value of a variable, or the empty string when the variable cannot be resolved.
2.  A “format string” that allows to reference matching groups from the regular expression. The format string allows for conditional inserts and simple modifications.
3.  Options that are passed to the regular expression.

The following example inserts the name of the current file without its ending, so from `foo.txt` it makes `foo`.


``` highlight
${TM_FILENAME/(.*)\..+$/$1/}
  |           |         | |
  |           |         | |-> no options
  |           |         |
  |           |         |-> references the contents of the first
  |           |             capture group
  |           |
  |           |-> regex to capture everything before
  |               the final `.suffix`
  |
  |-> resolves to the filename
```


##### Grammar

Below is the EBNF ([extended Backus-Naur form](https://en.wikipedia.org/wiki/Extended_Backus-Naur_form)) for snippets. With `\` (backslash), you can escape `$`, `}` and `\`. Within choice elements, the backslash also escapes comma and pipe characters.


``` highlight
any         ::= tabstop | placeholder | choice | variable | text
tabstop     ::= '$' int | '${' int '}'
placeholder ::= '${' int ':' any '}'
choice      ::= '${' int '|' text (',' text)* '|}'
variable    ::= '$' var | '${' var }'
                | '${' var ':' any '}'
                | '${' var '/' regex '/' (format | text)+ '/' options '}'
format      ::= '$' int | '${' int '}'
                | '${' int ':' '/upcase' | '/downcase' | '/capitalize' '}'
                | '${' int ':+' if '}'
                | '${' int ':?' if ':' else '}'
                | '${' int ':-' else '}' | '${' int ':' else '}'
regex       ::= Regular Expression value (ctor-string)
options     ::= Regular Expression option (ctor-options)
var         ::= [_a-zA-Z] [_a-zA-Z0-9]*
int         ::= [0-9]+
text        ::= .*
if          ::= text
else        ::= text
```


#### Completion Item Resolve Request (↩)

The request is sent from the client to the server to resolve additional information for a given completion item.

*Request*:

- method: `completionItem/resolve`
- params: `CompletionItem`

*Response*:

- result: `CompletionItem`
- error: code and message set in case an exception happens during the completion resolve request.

#### PublishDiagnostics Notification (←)

Diagnostics notifications are sent from the server to the client to signal results of validation runs.

Diagnostics are “owned” by the server so it is the server’s responsibility to clear them if necessary. The following rule is used for VS Code servers that generate diagnostics:

- if a language is single file only (for example HTML) then diagnostics are cleared by the server when the file is closed. Please note that open / close events don’t necessarily reflect what the user sees in the user interface. These events are ownership events. So with the current version of the specification it is possible that problems are not cleared although the file is not visible in the user interface since the client has not closed the file yet.
- if a language has a project system (for example C#) diagnostics are not cleared when a file closes. When a project is opened all diagnostics for all files are recomputed (or read from a cache).

When a file changes it is the server’s responsibility to re-compute diagnostics and push them to the client. If the computed set is empty it has to push the empty array to clear former diagnostics. Newly pushed diagnostics always replace previously pushed diagnostics. There is no merging that happens on the client side.

See also the [Diagnostic](#diagnostic) section.

*Client Capability*:

- property name (optional): `textDocument.publishDiagnostics`
- property type: `PublishDiagnosticsClientCapabilities` defined as follows:


``` highlight
export interface PublishDiagnosticsClientCapabilities {
    /**
     * Whether the clients accepts diagnostics with related information.
     */
    relatedInformation?: boolean;

    /**
     * Client supports the tag property to provide meta data about a diagnostic.
     * Clients supporting tags have to handle unknown tags gracefully.
     *
     * @since 3.15.0
     */
    tagSupport?: {
        /**
         * The tags supported by the client.
         */
        valueSet: DiagnosticTag[];
    };

    /**
     * Whether the client interprets the version property of the
     * `textDocument/publishDiagnostics` notification's parameter.
     *
     * @since 3.15.0
     */
    versionSupport?: boolean;

    /**
     * Client supports a codeDescription property
     *
     * @since 3.16.0
     */
    codeDescriptionSupport?: boolean;

    /**
     * Whether code action supports the `data` property which is
     * preserved between a `textDocument/publishDiagnostics` and
     * `textDocument/codeAction` request.
     *
     * @since 3.16.0
     */
    dataSupport?: boolean;
}
```


*Notification*:

- method: `textDocument/publishDiagnostics`
- params: `PublishDiagnosticsParams` defined as follows:


``` highlight
interface PublishDiagnosticsParams {
    /**
     * The URI for which diagnostic information is reported.
     */
    uri: DocumentUri;

    /**
     * Optional the version number of the document the diagnostics are published
     * for.
     *
     * @since 3.15.0
     */
    version?: integer;

    /**
     * An array of diagnostic information items.
     */
    diagnostics: Diagnostic[];
}
```


#### Pull Diagnostics

Diagnostics are currently published by the server to the client using a notification. This model has the advantage that for workspace wide diagnostics the server has the freedom to compute them at a server preferred point in time. On the other hand the approach has the disadvantage that the server can’t prioritize the computation for the file in which the user types or which are visible in the editor. Inferring the client’s UI state from the `textDocument/didOpen` and `textDocument/didChange` notifications might lead to false positives since these notifications are ownership transfer notifications.

The specification therefore introduces the concept of diagnostic pull requests to give a client more control over the documents for which diagnostics should be computed and at which point in time.

*Client Capability*:

- property name (optional): `textDocument.diagnostic`
- property type: `DiagnosticClientCapabilities` defined as follows:


``` highlight
/**
 * Client capabilities specific to diagnostic pull requests.
 *
 * @since 3.17.0
 */
export interface DiagnosticClientCapabilities {
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true` the client supports the new
     * `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
     * return value for the corresponding server capability as well.
     */
    dynamicRegistration?: boolean;

    /**
     * Whether the clients supports related documents for document diagnostic
     * pulls.
     */
    relatedDocumentSupport?: boolean;

    /**
     * Whether the clients accepts diagnostics with related information.
     */
    relatedInformation?: boolean;

    /**
     * Client supports the tag property to provide meta data about a diagnostic.
     * Clients supporting tags have to handle unknown tags gracefully.
     */
    tagSupport?: ClientDiagnosticsTagOptions;

    /**
     * Client supports a codeDescription property
     */
    codeDescriptionSupport?: boolean;

    /**
     * Whether code action supports the `data` property which is
     * preserved between a `textDocument/publishDiagnostics` and
     * `textDocument/codeAction` request.
     */
    dataSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `diagnosticProvider`
- property type: `DiagnosticOptions` defined as follows:


``` highlight
/**
 * Diagnostic options.
 *
 * @since 3.17.0
 */
export interface DiagnosticOptions extends WorkDoneProgressOptions {
    /**
     * An optional identifier under which the diagnostics are
     * managed by the client.
     */
    identifier?: string;

    /**
     * Whether the language has inter file dependencies meaning that
     * editing code in one file can result in a different diagnostic
     * set in another file. Inter file dependencies are common for
     * most programming languages and typically uncommon for linters.
     */
    interFileDependencies: boolean;

    /**
     * The server provides support for workspace diagnostics as well.
     */
    workspaceDiagnostics: boolean;
}
```


*Registration Options*: `DiagnosticRegistrationOptions` options defined as follows:


``` highlight
/**
 * Diagnostic registration options.
 *
 * @since 3.17.0
 */
export interface DiagnosticRegistrationOptions extends
    TextDocumentRegistrationOptions, DiagnosticOptions,
    StaticRegistrationOptions {
}
```


##### Document Diagnostics(↩)

The text document diagnostic request is sent from the client to the server to ask the server to compute the diagnostics for a given document. As with other pull requests the server is asked to compute the diagnostics for the currently synced version of the document.

*Request*:

- method: `textDocument/diagnostic`.
- params: `DocumentDiagnosticParams` defined as follows:


``` highlight
/**
 * Parameters of the document diagnostic request.
 *
 * @since 3.17.0
 */
export interface DocumentDiagnosticParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The additional identifier  provided during registration.
     */
    identifier?: string;

    /**
     * The result id of a previous response if provided.
     */
    previousResultId?: string;
}
```


*Response*:

- result: `DocumentDiagnosticReport` defined as follows:


``` highlight
/**
 * The result of a document diagnostic pull request. A report can
 * either be a full report containing all diagnostics for the
 * requested document or a unchanged report indicating that nothing
 * has changed in terms of diagnostics in comparison to the last
 * pull request.
 *
 * @since 3.17.0
 */
export type DocumentDiagnosticReport = RelatedFullDocumentDiagnosticReport
    | RelatedUnchangedDocumentDiagnosticReport;
```


``` highlight
/**
 * The document diagnostic report kinds.
 *
 * @since 3.17.0
 */
export namespace DocumentDiagnosticReportKind {
    /**
     * A diagnostic report with a full
     * set of problems.
     */
    export const Full = 'full';

    /**
     * A report indicating that the last
     * returned report is still accurate.
     */
    export const Unchanged = 'unchanged';
}

export type DocumentDiagnosticReportKind = 'full' | 'unchanged';
```


``` highlight
/**
 * A diagnostic report with a full set of problems.
 *
 * @since 3.17.0
 */
export interface FullDocumentDiagnosticReport {
    /**
     * A full document diagnostic report.
     */
    kind: DocumentDiagnosticReportKind.Full;

    /**
     * An optional result id. If provided it will
     * be sent on the next diagnostic request for the
     * same document.
     */
    resultId?: string;

    /**
     * The actual items.
     */
    items: Diagnostic[];
}
```


``` highlight
/**
 * A diagnostic report indicating that the last returned
 * report is still accurate.
 *
 * @since 3.17.0
 */
export interface UnchangedDocumentDiagnosticReport {
    /**
     * A document diagnostic report indicating
     * no changes to the last result. A server can
     * only return `unchanged` if result ids are
     * provided.
     */
    kind: DocumentDiagnosticReportKind.Unchanged;

    /**
     * A result id which will be sent on the next
     * diagnostic request for the same document.
     */
    resultId: string;
}
```


``` highlight
/**
 * A full diagnostic report with a set of related documents.
 *
 * @since 3.17.0
 */
export interface RelatedFullDocumentDiagnosticReport extends
    FullDocumentDiagnosticReport {
    /**
     * Diagnostics of related documents. This information is useful
     * in programming languages where code in a file A can generate
     * diagnostics in a file B which A depends on. An example of
     * such a language is C/C++ where macro definitions in a file
     * a.cpp and result in errors in a header file b.hpp.
     *
     * @since 3.17.0
     */
    relatedDocuments?: {
        [uri: string /** DocumentUri */]:
            FullDocumentDiagnosticReport | UnchangedDocumentDiagnosticReport;
    };
}
```


``` highlight
/**
 * An unchanged diagnostic report with a set of related documents.
 *
 * @since 3.17.0
 */
export interface RelatedUnchangedDocumentDiagnosticReport extends
    UnchangedDocumentDiagnosticReport {
    /**
     * Diagnostics of related documents. This information is useful
     * in programming languages where code in a file A can generate
     * diagnostics in a file B which A depends on. An example of
     * such a language is C/C++ where macro definitions in a file
     * a.cpp and result in errors in a header file b.hpp.
     *
     * @since 3.17.0
     */
    relatedDocuments?: {
        [uri: string /** DocumentUri */]:
            FullDocumentDiagnosticReport | UnchangedDocumentDiagnosticReport;
    };
}
```


- partial result: The first literal send need to be a `DocumentDiagnosticReport` followed by n `DocumentDiagnosticReportPartialResult` literals defined as follows:


``` highlight
/**
 * A partial result for a document diagnostic report.
 *
 * @since 3.17.0
 */
export interface DocumentDiagnosticReportPartialResult {
    relatedDocuments: {
        [uri: string /** DocumentUri */]:
            FullDocumentDiagnosticReport | UnchangedDocumentDiagnosticReport;
    };
}
```


- error: code and message set in case an exception happens during the diagnostic request. A server is also allowed to return an error with code `ServerCancelled` indicating that the server can’t compute the result right now. A server can return a `DiagnosticServerCancellationData` data to indicate whether the client should re-trigger the request. If no data is provided it defaults to `{ retriggerRequest: true }`:


``` highlight
/**
 * Cancellation data returned from a diagnostic request.
 *
 * @since 3.17.0
 */
export interface DiagnosticServerCancellationData {
    retriggerRequest: boolean;
}
```


##### Workspace Diagnostics(↩)

The workspace diagnostic request is sent from the client to the server to ask the server to compute workspace wide diagnostics which previously were pushed from the server to the client. In contrast to the document diagnostic request the workspace request can be long running and is not bound to a specific workspace or document state. If the client supports streaming for the workspace diagnostic pull it is legal to provide a document diagnostic report multiple times for the same document URI. The last one reported will win over previous reports.

If a client receives a diagnostic report for a document in a workspace diagnostic request for which the client also issues individual document diagnostic pull requests the client needs to decide which diagnostics win and should be presented. In general:

- diagnostics for a higher document version should win over those from a lower document version (e.g. note that document versions are steadily increasing)
- diagnostics from a document pull should win over diagnostics from a workspace pull.

*Request*:

- method: `workspace/diagnostic`.
- params: `WorkspaceDiagnosticParams` defined as follows:


``` highlight
/**
 * Parameters of the workspace diagnostic request.
 *
 * @since 3.17.0
 */
export interface WorkspaceDiagnosticParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The additional identifier provided during registration.
     */
    identifier?: string;

    /**
     * The currently known diagnostic reports with their
     * previous result ids.
     */
    previousResultIds: PreviousResultId[];
}
```


``` highlight
/**
 * A previous result id in a workspace pull request.
 *
 * @since 3.17.0
 */
export interface PreviousResultId {
    /**
     * The URI for which the client knows a
     * result id.
     */
    uri: DocumentUri;

    /**
     * The value of the previous result id.
     */
    value: string;
}
```


*Response*:

- result: `WorkspaceDiagnosticReport` defined as follows:


``` highlight
/**
 * A workspace diagnostic report.
 *
 * @since 3.17.0
 */
export interface WorkspaceDiagnosticReport {
    items: WorkspaceDocumentDiagnosticReport[];
}
```


``` highlight
/**
 * A full document diagnostic report for a workspace diagnostic result.
 *
 * @since 3.17.0
 */
export interface WorkspaceFullDocumentDiagnosticReport extends
    FullDocumentDiagnosticReport {

    /**
     * The URI for which diagnostic information is reported.
     */
    uri: DocumentUri;

    /**
     * The version number for which the diagnostics are reported.
     * If the document is not marked as open `null` can be provided.
     */
    version: integer | null;
}
```


``` highlight
/**
 * An unchanged document diagnostic report for a workspace diagnostic result.
 *
 * @since 3.17.0
 */
export interface WorkspaceUnchangedDocumentDiagnosticReport extends
    UnchangedDocumentDiagnosticReport {

    /**
     * The URI for which diagnostic information is reported.
     */
    uri: DocumentUri;

    /**
     * The version number for which the diagnostics are reported.
     * If the document is not marked as open `null` can be provided.
     */
    version: integer | null;
};
```


``` highlight
/**
 * A workspace diagnostic document report.
 *
 * @since 3.17.0
 */
export type WorkspaceDocumentDiagnosticReport =
    WorkspaceFullDocumentDiagnosticReport
    | WorkspaceUnchangedDocumentDiagnosticReport;
```


- partial result: The first literal send need to be a `WorkspaceDiagnosticReport` followed by n `WorkspaceDiagnosticReportPartialResult` literals defined as follows:


``` highlight
/**
 * A partial result for a workspace diagnostic report.
 *
 * @since 3.17.0
 */
export interface WorkspaceDiagnosticReportPartialResult {
    items: WorkspaceDocumentDiagnosticReport[];
}
```


- error: code and message set in case an exception happens during the diagnostic request. A server is also allowed to return and error with code `ServerCancelled` indicating that the server can’t compute the result right now. A server can return a `DiagnosticServerCancellationData` data to indicate whether the client should re-trigger the request. If no data is provided it defaults to `{ retriggerRequest: true }`:

##### Diagnostics Refresh(↪)

The `workspace/diagnostic/refresh` request is sent from the server to the client. Servers can use it to ask clients to refresh all needed document and workspace diagnostics. This is useful if a server detects a project wide configuration change which requires a re-calculation of all diagnostics.

*Client Capability*:

- property name (optional): `workspace.diagnostics`
- property type: `DiagnosticWorkspaceClientCapabilities` defined as follows:


``` highlight
/**
 * Workspace client capabilities specific to diagnostic pull requests.
 *
 * @since 3.17.0
 */
export interface DiagnosticWorkspaceClientCapabilities {
    /**
     * Whether the client implementation supports a refresh request sent from
     * the server to the client.
     *
     * Note that this event is global and will force the client to refresh all
     * pulled diagnostics currently shown. It should be used with absolute care
     * and is useful for situation where a server for example detects a project
     * wide change that requires such a calculation.
     */
    refreshSupport?: boolean;
}
```


*Request*:

- method: `workspace/diagnostic/refresh`
- params: none

*Response*:

- result: void
- error: code and message set in case an exception happens during the ‘workspace/diagnostic/refresh’ request

##### Implementation Considerations

Generally the language server specification doesn’t enforce any specific client implementation since those usually depend on how the client UI behaves. However since diagnostics can be provided on a document and workspace level here are some tips:

- a client should pull actively for the document the users types in.
- if the server signals inter file dependencies a client should also pull for visible documents to ensure accurate diagnostics. However the pull should happen less frequently.
- if the server signals workspace pull support a client should also pull for workspace diagnostics. It is recommended for clients to implement partial result progress for the workspace pull to allow servers to keep the request open for a long time. If a server closes a workspace diagnostic pull request the client should re-trigger the request.

#### Signature Help Request (↩)

The signature help request is sent from the client to the server to request signature information at a given cursor position.

*Client Capability*:

- property name (optional): `textDocument.signatureHelp`
- property type: `SignatureHelpClientCapabilities` defined as follows:


``` highlight
export interface SignatureHelpClientCapabilities {
    /**
     * Whether signature help supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports the following `SignatureInformation`
     * specific properties.
     */
    signatureInformation?: {
        /**
         * Client supports the follow content formats for the documentation
         * property. The order describes the preferred format of the client.
         */
        documentationFormat?: MarkupKind[];

        /**
         * Client capabilities specific to parameter information.
         */
        parameterInformation?: {
            /**
             * The client supports processing label offsets instead of a
             * simple label string.
             *
             * @since 3.14.0
             */
            labelOffsetSupport?: boolean;
        };

        /**
         * The client supports the `activeParameter` property on
         * `SignatureInformation` literal.
         *
         * @since 3.16.0
         */
        activeParameterSupport?: boolean;
    };

    /**
     * The client supports to send additional context information for a
     * `textDocument/signatureHelp` request. A client that opts into
     * contextSupport will also support the `retriggerCharacters` on
     * `SignatureHelpOptions`.
     *
     * @since 3.15.0
     */
    contextSupport?: boolean;
}
```


*Server Capability*:

- property name (optional): `signatureHelpProvider`
- property type: `SignatureHelpOptions` defined as follows:


``` highlight
export interface SignatureHelpOptions extends WorkDoneProgressOptions {
    /**
     * The characters that trigger signature help
     * automatically.
     */
    triggerCharacters?: string[];

    /**
     * List of characters that re-trigger signature help.
     *
     * These trigger characters are only active when signature help is already
     * showing. All trigger characters are also counted as re-trigger
     * characters.
     *
     * @since 3.15.0
     */
    retriggerCharacters?: string[];
}
```


*Registration Options*: `SignatureHelpRegistrationOptions` defined as follows:


``` highlight
export interface SignatureHelpRegistrationOptions
    extends TextDocumentRegistrationOptions, SignatureHelpOptions {
}
```


*Request*:

- method: `textDocument/signatureHelp`
- params: `SignatureHelpParams` defined as follows:


``` highlight
export interface SignatureHelpParams extends TextDocumentPositionParams,
    WorkDoneProgressParams {
    /**
     * The signature help context. This is only available if the client
     * specifies to send this using the client capability
     * `textDocument.signatureHelp.contextSupport === true`
     *
     * @since 3.15.0
     */
    context?: SignatureHelpContext;
}
```


``` highlight
/**
 * How a signature help was triggered.
 *
 * @since 3.15.0
 */
export namespace SignatureHelpTriggerKind {
    /**
     * Signature help was invoked manually by the user or by a command.
     */
    export const Invoked: 1 = 1;
    /**
     * Signature help was triggered by a trigger character.
     */
    export const TriggerCharacter: 2 = 2;
    /**
     * Signature help was triggered by the cursor moving or by the document
     * content changing.
     */
    export const ContentChange: 3 = 3;
}
export type SignatureHelpTriggerKind = 1 | 2 | 3;
```


``` highlight
/**
 * Additional information about the context in which a signature help request
 * was triggered.
 *
 * @since 3.15.0
 */
export interface SignatureHelpContext {
    /**
     * Action that caused signature help to be triggered.
     */
    triggerKind: SignatureHelpTriggerKind;

    /**
     * Character that caused signature help to be triggered.
     *
     * This is undefined when triggerKind !==
     * SignatureHelpTriggerKind.TriggerCharacter
     */
    triggerCharacter?: string;

    /**
     * `true` if signature help was already showing when it was triggered.
     *
     * Retriggers occur when the signature help is already active and can be
     * caused by actions such as typing a trigger character, a cursor move, or
     * document content changes.
     */
    isRetrigger: boolean;

    /**
     * The currently active `SignatureHelp`.
     *
     * The `activeSignatureHelp` has its `SignatureHelp.activeSignature` field
     * updated based on the user navigating through available signatures.
     */
    activeSignatureHelp?: SignatureHelp;
}
```


*Response*:

- result: `SignatureHelp` \| `null` defined as follows:


``` highlight
/**
 * Signature help represents the signature of something
 * callable. There can be multiple signature but only one
 * active and only one active parameter.
 */
export interface SignatureHelp {
    /**
     * One or more signatures. If no signatures are available the signature help
     * request should return `null`.
     */
    signatures: SignatureInformation[];

    /**
     * The active signature. If omitted or the value lies outside the
     * range of `signatures` the value defaults to zero or is ignore if
     * the `SignatureHelp` as no signatures.
     *
     * Whenever possible implementors should make an active decision about
     * the active signature and shouldn't rely on a default value.
     *
     * In future version of the protocol this property might become
     * mandatory to better express this.
     */
    activeSignature?: uinteger;

    /**
     * The active parameter of the active signature. If omitted or the value
     * lies outside the range of `signatures[activeSignature].parameters`
     * defaults to 0 if the active signature has parameters. If
     * the active signature has no parameters it is ignored.
     *
     * Since version 3.16.0 the `SignatureInformation` itself provides a
     * `activeParameter` property and it should be used instead of this one.
     */
    activeParameter?: uinteger;
}
```


``` highlight
/**
 * Represents the signature of something callable. A signature
 * can have a label, like a function-name, a doc-comment, and
 * a set of parameters.
 */
export interface SignatureInformation {
    /**
     * The label of this signature. Will be shown in
     * the UI.
     */
    label: string;

    /**
     * The human-readable doc-comment of this signature. Will be shown
     * in the UI but can be omitted.
     */
    documentation?: string | MarkupContent;

    /**
     * The parameters of this signature.
     */
    parameters?: ParameterInformation[];

    /**
     * The index of the active parameter.
     *
     * If provided, this is used in place of `SignatureHelp.activeParameter`.
     *
     * @since 3.16.0
     */
    activeParameter?: uinteger;
}
```


``` highlight
/**
 * Represents a parameter of a callable-signature. A parameter can
 * have a label and a doc-comment.
 */
export interface ParameterInformation {

    /**
     * The label of this parameter information.
     *
     * Either a string or an inclusive start and exclusive end offsets within
     * its containing signature label. (see SignatureInformation.label). The
     * offsets are based on a UTF-16 string representation as `Position` and
     * `Range` does.
     *
     * *Note*: a label of type string should be a substring of its containing
     * signature label. Its intended use case is to highlight the parameter
     * label part in the `SignatureInformation.label`.
     */
    label: string | [uinteger, uinteger];

    /**
     * The human-readable doc-comment of this parameter. Will be shown
     * in the UI but can be omitted.
     */
    documentation?: string | MarkupContent;
}
```


- error: code and message set in case an exception happens during the signature help request.

#### Code Action Request (↩)

The code action request is sent from the client to the server to compute commands for a given text document and range. These commands are typically code fixes to either fix problems or to beautify/refactor code. The result of a `textDocument/codeAction` request is an array of `Command` literals which are typically presented in the user interface. To ensure that a server is useful in many clients the commands specified in a code actions should be handled by the server and not by the client (see `workspace/executeCommand` and `ServerCapabilities.executeCommandProvider`). If the client supports providing edits with a code action then that mode should be used.

*Since version 3.16.0:* a client can offer a server to delay the computation of code action properties during a ‘textDocument/codeAction’ request:

This is useful for cases where it is expensive to compute the value of a property (for example the `edit` property). Clients signal this through the `codeAction.resolveSupport` capability which lists all properties a client can resolve lazily. The server capability `codeActionProvider.resolveProvider` signals that a server will offer a `codeAction/resolve` route. To help servers to uniquely identify a code action in the resolve request, a code action literal can optional carry a data property. This is also guarded by an additional client capability `codeAction.dataSupport`. In general, a client should offer data support if it offers resolve support. It should also be noted that servers shouldn’t alter existing attributes of a code action in a codeAction/resolve request.

> *Since version 3.8.0:* support for CodeAction literals to enable the following scenarios:

- the ability to directly return a workspace edit from the code action request. This avoids having another server roundtrip to execute an actual code action. However server providers should be aware that if the code action is expensive to compute or the edits are huge it might still be beneficial if the result is simply a command and the actual edit is only computed when needed.
- the ability to group code actions using a kind. Clients are allowed to ignore that information. However it allows them to better group code action for example into corresponding menus (e.g. all refactor code actions into a refactor menu).

Clients need to announce their support for code action literals (e.g. literals of type `CodeAction`) and code action kinds via the corresponding client capability `codeAction.codeActionLiteralSupport`.

*Client Capability*:

- property name (optional): `textDocument.codeAction`
- property type: `CodeActionClientCapabilities` defined as follows:


``` highlight
export interface CodeActionClientCapabilities {
    /**
     * Whether code action supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * The client supports code action literals as a valid
     * response of the `textDocument/codeAction` request.
     *
     * @since 3.8.0
     */
    codeActionLiteralSupport?: {
        /**
         * The code action kind is supported with the following value
         * set.
         */
        codeActionKind: {

            /**
             * The code action kind values the client supports. When this
             * property exists the client also guarantees that it will
             * handle values outside its set gracefully and falls back
             * to a default value when unknown.
             */
            valueSet: CodeActionKind[];
        };
    };

    /**
     * Whether code action supports the `isPreferred` property.
     *
     * @since 3.15.0
     */
    isPreferredSupport?: boolean;

    /**
     * Whether code action supports the `disabled` property.
     *
     * @since 3.16.0
     */
    disabledSupport?: boolean;

    /**
     * Whether code action supports the `data` property which is
     * preserved between a `textDocument/codeAction` and a
     * `codeAction/resolve` request.
     *
     * @since 3.16.0
     */
    dataSupport?: boolean;


    /**
     * Whether the client supports resolving additional code action
     * properties via a separate `codeAction/resolve` request.
     *
     * @since 3.16.0
     */
    resolveSupport?: {
        /**
         * The properties that a client can resolve lazily.
         */
        properties: string[];
    };

    /**
     * Whether the client honors the change annotations in
     * text edits and resource operations returned via the
     * `CodeAction#edit` property by for example presenting
     * the workspace edit in the user interface and asking
     * for confirmation.
     *
     * @since 3.16.0
     */
    honorsChangeAnnotations?: boolean;
}
```


*Server Capability*:

- property name (optional): `codeActionProvider`
- property type: `boolean | CodeActionOptions` where `CodeActionOptions` is defined as follows:


``` highlight
export interface CodeActionOptions extends WorkDoneProgressOptions {
    /**
     * CodeActionKinds that this server may return.
     *
     * The list of kinds may be generic, such as `CodeActionKind.Refactor`,
     * or the server may list out every specific kind they provide.
     */
    codeActionKinds?: CodeActionKind[];

    /**
     * The server provides support to resolve additional
     * information for a code action.
     *
     * @since 3.16.0
     */
    resolveProvider?: boolean;
}
```


*Registration Options*: `CodeActionRegistrationOptions` defined as follows:


``` highlight
export interface CodeActionRegistrationOptions extends
    TextDocumentRegistrationOptions, CodeActionOptions {
}
```


*Request*:

- method: `textDocument/codeAction`
- params: `CodeActionParams` defined as follows:


``` highlight
/**
 * Params for the CodeActionRequest
 */
export interface CodeActionParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The document in which the command was invoked.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The range for which the command was invoked.
     */
    range: Range;

    /**
     * Context carrying additional information.
     */
    context: CodeActionContext;
}
```


``` highlight
/**
 * The kind of a code action.
 *
 * Kinds are a hierarchical list of identifiers separated by `.`,
 * e.g. `"refactor.extract.function"`.
 *
 * The set of kinds is open and client needs to announce the kinds it supports
 * to the server during initialization.
 */
export type CodeActionKind = string;

/**
 * A set of predefined code action kinds.
 */
export namespace CodeActionKind {

    /**
     * Empty kind.
     */
    export const Empty: CodeActionKind = '';

    /**
     * Base kind for quickfix actions: 'quickfix'.
     */
    export const QuickFix: CodeActionKind = 'quickfix';

    /**
     * Base kind for refactoring actions: 'refactor'.
     */
    export const Refactor: CodeActionKind = 'refactor';

    /**
     * Base kind for refactoring extraction actions: 'refactor.extract'.
     *
     * Example extract actions:
     *
     * - Extract method
     * - Extract function
     * - Extract variable
     * - Extract interface from class
     * - ...
     */
    export const RefactorExtract: CodeActionKind = 'refactor.extract';

    /**
     * Base kind for refactoring inline actions: 'refactor.inline'.
     *
     * Example inline actions:
     *
     * - Inline function
     * - Inline variable
     * - Inline constant
     * - ...
     */
    export const RefactorInline: CodeActionKind = 'refactor.inline';

    /**
     * Base kind for refactoring rewrite actions: 'refactor.rewrite'.
     *
     * Example rewrite actions:
     *
     * - Convert JavaScript function to class
     * - Add or remove parameter
     * - Encapsulate field
     * - Make method static
     * - Move method to base class
     * - ...
     */
    export const RefactorRewrite: CodeActionKind = 'refactor.rewrite';

    /**
     * Base kind for source actions: `source`.
     *
     * Source code actions apply to the entire file.
     */
    export const Source: CodeActionKind = 'source';

    /**
     * Base kind for an organize imports source action:
     * `source.organizeImports`.
     */
    export const SourceOrganizeImports: CodeActionKind =
        'source.organizeImports';

    /**
     * Base kind for a 'fix all' source action: `source.fixAll`.
     *
     * 'Fix all' actions automatically fix errors that have a clear fix that
     * do not require user input. They should not suppress errors or perform
     * unsafe fixes such as generating new types or classes.
     *
     * @since 3.17.0
     */
    export const SourceFixAll: CodeActionKind = 'source.fixAll';
}
```


``` highlight
/**
 * Contains additional diagnostic information about the context in which
 * a code action is run.
 */
export interface CodeActionContext {
    /**
     * An array of diagnostics known on the client side overlapping the range
     * provided to the `textDocument/codeAction` request. They are provided so
     * that the server knows which errors are currently presented to the user
     * for the given range. There is no guarantee that these accurately reflect
     * the error state of the resource. The primary parameter
     * to compute code actions is the provided range.
     */
    diagnostics: Diagnostic[];

    /**
     * Requested kind of actions to return.
     *
     * Actions not of this kind are filtered out by the client before being
     * shown. So servers can omit computing them.
     */
    only?: CodeActionKind[];

    /**
     * The reason why code actions were requested.
     *
     * @since 3.17.0
     */
    triggerKind?: CodeActionTriggerKind;
}
```


``` highlight
/**
 * The reason why code actions were requested.
 *
 * @since 3.17.0
 */
export namespace CodeActionTriggerKind {
    /**
     * Code actions were explicitly requested by the user or by an extension.
     */
    export const Invoked: 1 = 1;

    /**
     * Code actions were requested automatically.
     *
     * This typically happens when current selection in a file changes, but can
     * also be triggered when file content changes.
     */
    export const Automatic: 2 = 2;
}

export type CodeActionTriggerKind = 1 | 2;
```


*Response*:

- result: `(Command | CodeAction)[]` \| `null` where `CodeAction` is defined as follows:


``` highlight
/**
 * A code action represents a change that can be performed in code, e.g. to fix
 * a problem or to refactor code.
 *
 * A CodeAction must set either `edit` and/or a `command`. If both are supplied,
 * the `edit` is applied first, then the `command` is executed.
 */
export interface CodeAction {

    /**
     * A short, human-readable, title for this code action.
     */
    title: string;

    /**
     * The kind of the code action.
     *
     * Used to filter code actions.
     */
    kind?: CodeActionKind;

    /**
     * The diagnostics that this code action resolves.
     */
    diagnostics?: Diagnostic[];

    /**
     * Marks this as a preferred action. Preferred actions are used by the
     * `auto fix` command and can be targeted by keybindings.
     *
     * A quick fix should be marked preferred if it properly addresses the
     * underlying error. A refactoring should be marked preferred if it is the
     * most reasonable choice of actions to take.
     *
     * @since 3.15.0
     */
    isPreferred?: boolean;

    /**
     * Marks that the code action cannot currently be applied.
     *
     * Clients should follow the following guidelines regarding disabled code
     * actions:
     *
     * - Disabled code actions are not shown in automatic lightbulbs code
     *   action menus.
     *
     * - Disabled actions are shown as faded out in the code action menu when
     *   the user request a more specific type of code action, such as
     *   refactorings.
     *
     * - If the user has a keybinding that auto applies a code action and only
     *   a disabled code actions are returned, the client should show the user
     *   an error message with `reason` in the editor.
     *
     * @since 3.16.0
     */
    disabled?: {

        /**
         * Human readable description of why the code action is currently
         * disabled.
         *
         * This is displayed in the code actions UI.
         */
        reason: string;
    };

    /**
     * The workspace edit this code action performs.
     */
    edit?: WorkspaceEdit;

    /**
     * A command this code action executes. If a code action
     * provides an edit and a command, first the edit is
     * executed and then the command.
     */
    command?: Command;

    /**
     * A data entry field that is preserved on a code action between
     * a `textDocument/codeAction` and a `codeAction/resolve` request.
     *
     * @since 3.16.0
     */
    data?: LSPAny;
}
```


- partial result: `(Command | CodeAction)[]`
- error: code and message set in case an exception happens during the code action request.

#### Code Action Resolve Request (↩)

> *Since version 3.16.0*

The request is sent from the client to the server to resolve additional information for a given code action. This is usually used to compute the `edit` property of a code action to avoid its unnecessary computation during the `textDocument/codeAction` request.

Consider the client announcing the `edit` property as a property that can be resolved lazily using the client capability


``` highlight
textDocument.codeAction.resolveSupport = { properties: ['edit'] };
```


then a code action


``` highlight
{
    "title": "Do Foo"
}
```


needs to be resolved using the `codeAction/resolve` request before it can be applied.

*Client Capability*:

- property name (optional): `textDocument.codeAction.resolveSupport`
- property type: `{ properties: string[]; }`

*Request*:

- method: `codeAction/resolve`
- params: `CodeAction`

*Response*:

- result: `CodeAction`
- error: code and message set in case an exception happens during the code action resolve request.

#### Document Color Request (↩)

> *Since version 3.6.0*

The document color request is sent from the client to the server to list all color references found in a given text document. Along with the range, a color value in RGB is returned.

Clients can use the result to decorate color references in an editor. For example:

- Color boxes showing the actual color next to the reference
- Show a color picker when a color reference is edited

*Client Capability*:

- property name (optional): `textDocument.colorProvider`
- property type: `DocumentColorClientCapabilities` defined as follows:


``` highlight
export interface DocumentColorClientCapabilities {
    /**
     * Whether document color supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `colorProvider`
- property type: `boolean | DocumentColorOptions | DocumentColorRegistrationOptions` where `DocumentColorOptions` is defined as follows:


``` highlight
export interface DocumentColorOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `DocumentColorRegistrationOptions` defined as follows:


``` highlight
export interface DocumentColorRegistrationOptions extends
    TextDocumentRegistrationOptions, StaticRegistrationOptions,
    DocumentColorOptions {
}
```


*Request*:

- method: `textDocument/documentColor`
- params: `DocumentColorParams` defined as follows


``` highlight
interface DocumentColorParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;
}
```


*Response*:

- result: `ColorInformation[]` defined as follows:


``` highlight
interface ColorInformation {
    /**
     * The range in the document where this color appears.
     */
    range: Range;

    /**
     * The actual color value for this color range.
     */
    color: Color;
}
```


``` highlight
/**
 * Represents a color in RGBA space.
 */
interface Color {

    /**
     * The red component of this color in the range [0-1].
     */
    readonly red: decimal;

    /**
     * The green component of this color in the range [0-1].
     */
    readonly green: decimal;

    /**
     * The blue component of this color in the range [0-1].
     */
    readonly blue: decimal;

    /**
     * The alpha component of this color in the range [0-1].
     */
    readonly alpha: decimal;
}
```


- partial result: `ColorInformation[]`
- error: code and message set in case an exception happens during the ‘textDocument/documentColor’ request

#### Color Presentation Request (↩)

> *Since version 3.6.0*

The color presentation request is sent from the client to the server to obtain a list of presentations for a color value at a given location. Clients can use the result to

- modify a color reference.
- show in a color picker and let users pick one of the presentations

This request has no special capabilities and registration options since it is send as a resolve request for the `textDocument/documentColor` request.

*Request*:

- method: `textDocument/colorPresentation`
- params: `ColorPresentationParams` defined as follows


``` highlight
interface ColorPresentationParams extends WorkDoneProgressParams,
    PartialResultParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The color information to request presentations for.
     */
    color: Color;

    /**
     * The range where the color would be inserted. Serves as a context.
     */
    range: Range;
}
```


*Response*:

- result: `ColorPresentation[]` defined as follows:


``` highlight
interface ColorPresentation {
    /**
     * The label of this color presentation. It will be shown on the color
     * picker header. By default this is also the text that is inserted when
     * selecting this color presentation.
     */
    label: string;
    /**
     * An [edit](#TextEdit) which is applied to a document when selecting
     * this presentation for the color. When omitted the
     * [label](#ColorPresentation.label) is used.
     */
    textEdit?: TextEdit;
    /**
     * An optional array of additional [text edits](#TextEdit) that are applied
     * when selecting this color presentation. Edits must not overlap with the
     * main [edit](#ColorPresentation.textEdit) nor with themselves.
     */
    additionalTextEdits?: TextEdit[];
}
```


- partial result: `ColorPresentation[]`
- error: code and message set in case an exception happens during the ‘textDocument/colorPresentation’ request

#### Document Formatting Request (↩)

The document formatting request is sent from the client to the server to format a whole document.

*Client Capability*:

- property name (optional): `textDocument.formatting`
- property type: `DocumentFormattingClientCapabilities` defined as follows:


``` highlight
export interface DocumentFormattingClientCapabilities {
    /**
     * Whether formatting supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `documentFormattingProvider`
- property type: `boolean | DocumentFormattingOptions` where `DocumentFormattingOptions` is defined as follows:


``` highlight
export interface DocumentFormattingOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `DocumentFormattingRegistrationOptions` defined as follows:


``` highlight
export interface DocumentFormattingRegistrationOptions extends
    TextDocumentRegistrationOptions, DocumentFormattingOptions {
}
```


*Request*:

- method: `textDocument/formatting`
- params: `DocumentFormattingParams` defined as follows


``` highlight
interface DocumentFormattingParams extends WorkDoneProgressParams {
    /**
     * The document to format.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The format options.
     */
    options: FormattingOptions;
}
```


``` highlight
/**
 * Value-object describing what options formatting should use.
 */
interface FormattingOptions {
    /**
     * Size of a tab in spaces.
     */
    tabSize: uinteger;

    /**
     * Prefer spaces over tabs.
     */
    insertSpaces: boolean;

    /**
     * Trim trailing whitespace on a line.
     *
     * @since 3.15.0
     */
    trimTrailingWhitespace?: boolean;

    /**
     * Insert a newline character at the end of the file if one does not exist.
     *
     * @since 3.15.0
     */
    insertFinalNewline?: boolean;

    /**
     * Trim all newlines after the final newline at the end of the file.
     *
     * @since 3.15.0
     */
    trimFinalNewlines?: boolean;

    /**
     * Signature for further properties.
     */
    [key: string]: boolean | integer | string;
}
```


*Response*:

- result: [`TextEdit[]`](#textEdit) \| `null` describing the modification to the document to be formatted.
- error: code and message set in case an exception happens during the formatting request.

#### Document Range Formatting Request (↩)

The document range formatting request is sent from the client to the server to format a given range in a document.

*Client Capability*:

- property name (optional): `textDocument.rangeFormatting`
- property type: `DocumentRangeFormattingClientCapabilities` defined as follows:


``` highlight
export interface DocumentRangeFormattingClientCapabilities {
    /**
     * Whether formatting supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `documentRangeFormattingProvider`
- property type: `boolean | DocumentRangeFormattingOptions` where `DocumentRangeFormattingOptions` is defined as follows:


``` highlight
export interface DocumentRangeFormattingOptions extends
    WorkDoneProgressOptions {
}
```


*Registration Options*: `DocumentFormattingRegistrationOptions` defined as follows:


``` highlight
export interface DocumentRangeFormattingRegistrationOptions extends
    TextDocumentRegistrationOptions, DocumentRangeFormattingOptions {
}
```


*Request*:

- method: `textDocument/rangeFormatting`,
- params: `DocumentRangeFormattingParams` defined as follows:


``` highlight
interface DocumentRangeFormattingParams extends WorkDoneProgressParams {
    /**
     * The document to format.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The range to format
     */
    range: Range;

    /**
     * The format options
     */
    options: FormattingOptions;
}
```


*Response*:

- result: [`TextEdit[]`](#textEdit) \| `null` describing the modification to the document to be formatted.
- error: code and message set in case an exception happens during the range formatting request.

#### Document on Type Formatting Request (↩)

The document on type formatting request is sent from the client to the server to format parts of the document during typing.

*Client Capability*:

- property name (optional): `textDocument.onTypeFormatting`
- property type: `DocumentOnTypeFormattingClientCapabilities` defined as follows:


``` highlight
export interface DocumentOnTypeFormattingClientCapabilities {
    /**
     * Whether on type formatting supports dynamic registration.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `documentOnTypeFormattingProvider`
- property type: `DocumentOnTypeFormattingOptions` defined as follows:


``` highlight
export interface DocumentOnTypeFormattingOptions {
    /**
     * A character on which formatting should be triggered, like `{`.
     */
    firstTriggerCharacter: string;

    /**
     * More trigger characters.
     */
    moreTriggerCharacter?: string[];
}
```


*Registration Options*: `DocumentOnTypeFormattingRegistrationOptions` defined as follows:


``` highlight
export interface DocumentOnTypeFormattingRegistrationOptions extends
    TextDocumentRegistrationOptions, DocumentOnTypeFormattingOptions {
}
```


*Request*:

- method: `textDocument/onTypeFormatting`
- params: `DocumentOnTypeFormattingParams` defined as follows:


``` highlight
interface DocumentOnTypeFormattingParams {

    /**
     * The document to format.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The position around which the on type formatting should happen.
     * This is not necessarily the exact position where the character denoted
     * by the property `ch` got typed.
     */
    position: Position;

    /**
     * The character that has been typed that triggered the formatting
     * on type request. That is not necessarily the last character that
     * got inserted into the document since the client could auto insert
     * characters as well (e.g. like automatic brace completion).
     */
    ch: string;

    /**
     * The formatting options.
     */
    options: FormattingOptions;
}
```


*Response*:

- result: [`TextEdit[]`](#textEdit) \| `null` describing the modification to the document.
- error: code and message set in case an exception happens during the range formatting request.

#### Rename Request (↩)

The rename request is sent from the client to the server to ask the server to compute a workspace change so that the client can perform a workspace-wide rename of a symbol.

*Client Capability*:

- property name (optional): `textDocument.rename`
- property type: `RenameClientCapabilities` defined as follows:


``` highlight
export namespace PrepareSupportDefaultBehavior {
    /**
     * The client's default behavior is to select the identifier
     * according to the language's syntax rule.
     */
     export const Identifier: 1 = 1;
}

export type PrepareSupportDefaultBehavior = 1;
```


``` highlight
export interface RenameClientCapabilities {
    /**
     * Whether rename supports dynamic registration.
     */
    dynamicRegistration?: boolean;

    /**
     * Client supports testing for validity of rename operations
     * before execution.
     *
     * @since version 3.12.0
     */
    prepareSupport?: boolean;

    /**
     * Client supports the default behavior result
     * (`{ defaultBehavior: boolean }`).
     *
     * The value indicates the default behavior used by the
     * client.
     *
     * @since version 3.16.0
     */
    prepareSupportDefaultBehavior?: PrepareSupportDefaultBehavior;

    /**
     * Whether the client honors the change annotations in
     * text edits and resource operations returned via the
     * rename request's workspace edit by for example presenting
     * the workspace edit in the user interface and asking
     * for confirmation.
     *
     * @since 3.16.0
     */
    honorsChangeAnnotations?: boolean;
}
```


*Server Capability*:

- property name (optional): `renameProvider`
- property type: `boolean | RenameOptions` where `RenameOptions` is defined as follows:

`RenameOptions` may only be specified if the client states that it supports `prepareSupport` in its initial `initialize` request.


``` highlight
export interface RenameOptions extends WorkDoneProgressOptions {
    /**
     * Renames should be checked and tested before being executed.
     */
    prepareProvider?: boolean;
}
```


*Registration Options*: `RenameRegistrationOptions` defined as follows:


``` highlight
export interface RenameRegistrationOptions extends
    TextDocumentRegistrationOptions, RenameOptions {
}
```


*Request*:

- method: `textDocument/rename`
- params: `RenameParams` defined as follows


``` highlight
interface RenameParams extends TextDocumentPositionParams,
    WorkDoneProgressParams {
    /**
     * The new name of the symbol. If the given name is not valid the
     * request must return a [ResponseError](#ResponseError) with an
     * appropriate message set.
     */
    newName: string;
}
```


*Response*:

- result: [`WorkspaceEdit`](#workspaceedit) \| `null` describing the modification to the workspace. `null` should be treated the same was as [`WorkspaceEdit`](#workspaceedit) with no changes (no change was required).
- error: code and message set in case when rename could not be performed for any reason. Examples include: there is nothing at given `position` to rename (like a space), given symbol does not support renaming by the server or the code is invalid (e.g. does not compile).

#### Prepare Rename Request (↩)

> *Since version 3.12.0*

The prepare rename request is sent from the client to the server to setup and test the validity of a rename operation at a given location.

*Request*:

- method: `textDocument/prepareRename`
- params: `PrepareRenameParams` defined as follows:


``` highlight
export interface PrepareRenameParams extends TextDocumentPositionParams, WorkDoneProgressParams {
}
```


*Response*:

- result: `Range | { range: Range, placeholder: string } | { defaultBehavior: boolean } | null` describing a [`Range`](#range) of the string to rename and optionally a placeholder text of the string content to be renamed. If `{ defaultBehavior: boolean }` is returned (since 3.16) the rename position is valid and the client should use its default behavior to compute the rename range. If `null` is returned then it is deemed that a ‘textDocument/rename’ request is not valid at the given position.
- error: code and message set in case the element can’t be renamed. Clients should show the information in their user interface.

#### Linked Editing Range(↩)

> *Since version 3.16.0*

The linked editing request is sent from the client to the server to return for a given position in a document the range of the symbol at the position and all ranges that have the same content. Optionally a word pattern can be returned to describe valid contents. A rename to one of the ranges can be applied to all other ranges if the new content is valid. If no result-specific word pattern is provided, the word pattern from the client’s language configuration is used.

*Client Capabilities*:

- property name (optional): `textDocument.linkedEditingRange`
- property type: `LinkedEditingRangeClientCapabilities` defined as follows:


``` highlight
export interface LinkedEditingRangeClientCapabilities {
    /**
     * Whether the implementation supports dynamic registration.
     * If this is set to `true` the client supports the new
     * `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
     * return value for the corresponding server capability as well.
     */
    dynamicRegistration?: boolean;
}
```


*Server Capability*:

- property name (optional): `linkedEditingRangeProvider`
- property type: `boolean` \| `LinkedEditingRangeOptions` \| `LinkedEditingRangeRegistrationOptions` defined as follows:


``` highlight
export interface LinkedEditingRangeOptions extends WorkDoneProgressOptions {
}
```


*Registration Options*: `LinkedEditingRangeRegistrationOptions` defined as follows:


``` highlight
export interface LinkedEditingRangeRegistrationOptions extends
    TextDocumentRegistrationOptions, LinkedEditingRangeOptions,
    StaticRegistrationOptions {
}
```


*Request*:

- method: `textDocument/linkedEditingRange`
- params: `LinkedEditingRangeParams` defined as follows:


``` highlight
export interface LinkedEditingRangeParams extends TextDocumentPositionParams,
    WorkDoneProgressParams {
}
```


*Response*:

- result: `LinkedEditingRanges` \| `null` defined as follows:


``` highlight
export interface LinkedEditingRanges {
    /**
     * A list of ranges that can be renamed together. The ranges must have
     * identical length and contain identical text content. The ranges cannot
     * overlap.
     */
    ranges: Range[];

    /**
     * An optional word pattern (regular expression) that describes valid
     * contents for the given ranges. If no pattern is provided, the client
     * configuration's word pattern will be used.
     */
    wordPattern?: string;
}
```


- error: code and message set in case an exception happens during the ‘textDocument/linkedEditingRange’ request

## Related

- [[Completions]] — `textDocument/completion` implementation
- [[Diagnostics]] — `textDocument/publishDiagnostics` and pull-diagnostics implementation
- [[Navigation]] — `textDocument/definition`, `textDocument/references`, `textDocument/hover`
- [[Rename]] — `textDocument/rename` and `textDocument/prepareRename`
- [[Table of Contents]] — `textDocument/codeAction` (TOC generation)
- [[Wiki Links]] — document link and document symbol protocol used for wiki links
