---
title: "LSP 3.17 — LSP Overview & JSON Structures"
source: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
date: 2026-04-16
tags:
  - wiki/input
  - research/lsp
---

## Language Server Protocol

The language server protocol defines a set of JSON-RPC request, response and notification messages which are exchanged using the above base protocol. This section starts describing the basic JSON structures used in the protocol. The document uses TypeScript interfaces in strict mode to describe these. This means for example that a `null` value has to be explicitly listed and that a mandatory property must be listed even if a falsy value might exist. Based on the basic JSON structures, the actual requests with their responses and the notifications are described.

An example would be a request sent from the client to the server to request a hover value for a symbol at a certain position in a text document. The request’s method would be `textDocument/hover` with a parameter like this:


``` highlight
interface HoverParams {
    textDocument: string; /** The text document's URI in string form */
    position: { line: uinteger; character: uinteger; };
}
```


The result of the request would be the hover to be presented. In its simple form it can be a string. So the result looks like this:


``` highlight
interface HoverResult {
    value: string;
}
```


Please also note that a response return value of `null` indicates no result. It doesn’t tell the client to resend the request.

In general, the language server protocol supports JSON-RPC messages, however the base protocol defined here uses a convention such that the parameters passed to request/notification messages should be of `object` type (if passed at all). However, this does not disallow using `Array` parameter types in custom messages.

The protocol currently assumes that one server serves one tool. There is currently no support in the protocol to share one server between different tools. Such sharing would require additional protocol e.g. to lock a document to support concurrent editing.

### Capabilities

Not every language server can support all features defined by the protocol. LSP therefore provides ‘capabilities’. A capability groups a set of language features. A development tool and the language server announce their supported features using capabilities. As an example, a server announces that it can handle the `textDocument/hover` request, but it might not handle the `workspace/symbol` request. Similarly, a development tool announces its ability to provide `about to save` notifications before a document is saved, so that a server can compute textual edits to format the edited document before it is saved.

The set of capabilities is exchanged between the client and server during the [initialize](#initialize) request.

### Request, Notification and Response Ordering

Responses to requests should be sent in roughly the same order as the requests appear on the server or client side. So for example if a server receives a `textDocument/completion` request and then a `textDocument/signatureHelp` request it will usually first return the response for the `textDocument/completion` and then the response for `textDocument/signatureHelp`.

However, the server may decide to use a parallel execution strategy and may wish to return responses in a different order than the requests were received. The server may do so as long as this reordering doesn’t affect the correctness of the responses. For example, reordering the result of `textDocument/completion` and `textDocument/signatureHelp` is allowed, as each of these requests usually won’t affect the output of the other. On the other hand, the server most likely should not reorder `textDocument/definition` and `textDocument/rename` requests, since executing the latter may affect the result of the former.

### Message Documentation

As said LSP defines a set of requests, responses and notifications. Each of those are documented using the following format:

- a header describing the request
- an optional *Client capability* section describing the client capability of the request. This includes the client capabilities property path and JSON structure.
- an optional *Server Capability* section describing the server capability of the request. This includes the server capabilities property path and JSON structure. Clients should ignore server capabilities they don’t understand (e.g. the initialize request shouldn’t fail in this case).
- an optional *Registration Options* section describing the registration option if the request or notification supports dynamic capability registration. See the [register](#client_registerCapability) and [unregister](#client_unregisterCapability) request for how this works in detail.
- a *Request* section describing the format of the request sent. The method is a string identifying the request, the params are documented using a TypeScript interface. It is also documented whether the request supports work done progress and partial result progress.
- a *Response* section describing the format of the response. The result item describes the returned data in case of a success. The optional partial result item describes the returned data of a partial result notification. The error.data describes the returned data in case of an error. Please remember that in case of a failure the response already contains an error.code and an error.message field. These fields are only specified if the protocol forces the use of certain error codes or messages. In cases where the server can decide on these values freely they aren’t listed here.

### Basic JSON Structures

There are quite some JSON structures that are shared between different requests and notifications. Their structure and capabilities are documented in this section.

#### URI

URI’s are transferred as strings. The URI’s format is defined in <https://tools.ietf.org/html/rfc3986>


``` highlight
  foo://example.com:8042/over/there?name=ferret#nose
  \_/   \______________/\_________/ \_________/ \__/
   |           |            |            |        |
scheme     authority       path        query   fragment
   |   _____________________|__
  / \ /                        \
  urn:example:animal:ferret:nose
```


We also maintain a node module to parse a string into `scheme`, `authority`, `path`, `query`, and `fragment` URI components. The GitHub repository is <https://github.com/Microsoft/vscode-uri>, and the npm module is <https://www.npmjs.com/package/vscode-uri>.

Many of the interfaces contain fields that correspond to the URI of a document. For clarity, the type of such a field is declared as a `DocumentUri`. Over the wire, it will still be transferred as a string, but this guarantees that the contents of that string can be parsed as a valid URI.

Care should be taken to handle encoding in URIs. For example, some clients (such as VS Code) may encode colons in drive letters while others do not. The URIs below are both valid, but clients and servers should be consistent with the form they use themselves to ensure the other party doesn’t interpret them as distinct URIs. Clients and servers should not assume that each other are encoding the same way (for example a client encoding colons in drive letters cannot assume server responses will have encoded colons). The same applies to casing of drive letters - one party should not assume the other party will return paths with drive letters cased the same as itself.


``` highlight
file:///c:/project/readme.md
file:///C%3A/project/readme.md
```


``` highlight
type DocumentUri = string;
```


There is also a tagging interface for normal non document URIs. It maps to a `string` as well.


``` highlight
type URI = string;
```


#### Regular Expressions

Regular expression are a powerful tool and there are actual use cases for them in the language server protocol. However the downside with them is that almost every programming language has its own set of regular expression features so the specification can not simply refer to them as a regular expression. So the LSP uses a two step approach to support regular expressions:

- the client will announce which regular expression engine it will use. This will allow server that are written for a very specific client make full use of the regular expression capabilities of the client
- the specification will define a set of regular expression features that should be supported by a client. Instead of writing a new specification LSP will refer to the [ECMAScript Regular Expression specification](https://tc39.es/ecma262/#sec-regexp-regular-expression-objects) and remove features from it that are not necessary in the context of LSP or hard to implement for other clients.

*Client Capability*:

The following client capability is used to announce a client’s regular expression engine

- property path (optional): `general.regularExpressions`
- property type: `RegularExpressionsClientCapabilities` defined as follows:


``` highlight
/**
 * Client capabilities specific to regular expressions.
 */
export interface RegularExpressionsClientCapabilities {
    /**
     * The engine's name.
     */
    engine: string;

    /**
     * The engine's version.
     */
    version?: string;
}
```


The following table lists the well known engine values. Please note that the table should be driven by the community which integrates LSP into existing clients. It is not the goal of the spec to list all available regular expression engines.

| Engine | Version | Documentation |
|----|----|----|
| ECMAScript | `ES2020` | [ECMAScript 2020](https://tc39.es/ecma262/#sec-regexp-regular-expression-objects) & [MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_Expressions) |

*Regular Expression Subset*:

The following features from the [ECMAScript 2020](https://tc39.es/ecma262/#sec-regexp-regular-expression-objects) regular expression specification are NOT mandatory for a client:

- *Assertions*: Lookahead assertion, Negative lookahead assertion, lookbehind assertion, negative lookbehind assertion.
- *Character classes*: matching control characters using caret notation (e.g. `\cX`) and matching UTF-16 code units (e.g. `\uhhhh`).
- *Group and ranges*: named capturing groups.
- *Unicode property escapes*: none of the features needs to be supported.

The only regular expression flag that a client needs to support is ‘i’ to specify a case insensitive search.

#### Enumerations

The protocol supports two kind of enumerations: (a) integer based enumerations and (b) string based enumerations. Integer based enumerations usually start with `1`. The ones that don’t are historical and they were kept to stay backwards compatible. If appropriate, the value set of an enumeration is announced by the defining side (e.g. client or server) and transmitted to the other side during the initialize handshake. An example is the `CompletionItemKind` enumeration. It is announced by the client using the `textDocument.completion.completionItemKind` client property.

To support the evolution of enumerations the using side of an enumeration shouldn’t fail on an enumeration value it doesn’t know. It should simply ignore it as a value it can use and try to do its best to preserve the value on round trips. Lets look at the `CompletionItemKind` enumeration as an example again: if in a future version of the specification an additional completion item kind with the value `n` gets added and announced by a client an (older) server not knowing about the value should not fail but simply ignore the value as a usable item kind.

#### Text Documents

The current protocol is tailored for textual documents whose content can be represented as a string. There is currently no support for binary documents. A position inside a document (see Position definition below) is expressed as a zero-based line and character offset.

> New in 3.17

Prior to 3.17 the offsets were always based on a UTF-16 string representation. So in a string of the form `a𐐀b` the character offset of the character `a` is 0, the character offset of `𐐀` is 1 and the character offset of b is 3 since `𐐀` is represented using two code units in UTF-16. Since 3.17 clients and servers can agree on a different string encoding representation (e.g. UTF-8). The client announces it’s supported encoding via the client capability [`general.positionEncodings`](#clientCapabilities). The value is an array of position encodings the client supports, with decreasing preference (e.g. the encoding at index `0` is the most preferred one). To stay backwards compatible the only mandatory encoding is UTF-16 represented via the string `utf-16`. The server can pick one of the encodings offered by the client and signals that encoding back to the client via the initialize result’s property [`capabilities.positionEncoding`](#serverCapabilities). If the string value `utf-16` is missing from the client’s capability `general.positionEncodings` servers can safely assume that the client supports UTF-16. If the server omits the position encoding in its initialize result the encoding defaults to the string value `utf-16`. Implementation considerations: since the conversion from one encoding into another requires the content of the file / line the conversion is best done where the file is read which is usually on the server side.

To ensure that both client and server split the string into the same line representation the protocol specifies the following end-of-line sequences: ‘\n’, ‘\r\n’ and ‘\r’. Positions are line end character agnostic. So you can not specify a position that denotes `\r|\n` or `\n|` where `|` represents the character offset.


``` highlight
export const EOL: string[] = ['\n', '\r\n', '\r'];
```


#### Position

Position in a text document expressed as zero-based line and zero-based character offset. A position is between two characters like an ‘insert’ cursor in an editor. Special values like for example `-1` to denote the end of a line are not supported.


``` highlight
interface Position {
    /**
     * Line position in a document (zero-based).
     */
    line: uinteger;

    /**
     * Character offset on a line in a document (zero-based). The meaning of this
     * offset is determined by the negotiated `PositionEncodingKind`.
     *
     * If the character value is greater than the line length it defaults back
     * to the line length.
     */
    character: uinteger;
}
```


When describing positions the protocol needs to specify how offsets (specifically character offsets) should be interpreted. The corresponding `PositionEncodingKind` is negotiated between the client and the server during initialization.


``` highlight
/**
 * A type indicating how positions are encoded,
 * specifically what column offsets mean.
 *
 * @since 3.17.0
 */
export type PositionEncodingKind = string;

/**
 * A set of predefined position encoding kinds.
 *
 * @since 3.17.0
 */
export namespace PositionEncodingKind {

    /**
     * Character offsets count UTF-8 code units (e.g bytes).
     */
    export const UTF8: PositionEncodingKind = 'utf-8';

    /**
     * Character offsets count UTF-16 code units.
     *
     * This is the default and must always be supported
     * by servers
     */
    export const UTF16: PositionEncodingKind = 'utf-16';

    /**
     * Character offsets count UTF-32 code units.
     *
     * Implementation note: these are the same as Unicode code points,
     * so this `PositionEncodingKind` may also be used for an
     * encoding-agnostic representation of character offsets.
     */
    export const UTF32: PositionEncodingKind = 'utf-32';
}
```


#### Range

A range in a text document expressed as (zero-based) start and end positions. A range is comparable to a selection in an editor. Therefore, the end position is exclusive. If you want to specify a range that contains a line including the line ending character(s) then use an end position denoting the start of the next line. For example:


``` highlight
{
    start: { line: 5, character: 23 },
    end : { line: 6, character: 0 }
}
```


``` highlight
interface Range {
    /**
     * The range's start position.
     */
    start: Position;

    /**
     * The range's end position.
     */
    end: Position;
}
```


#### TextDocumentItem

An item to transfer a text document from the client to the server.


``` highlight
interface TextDocumentItem {
    /**
     * The text document's URI.
     */
    uri: DocumentUri;

    /**
     * The text document's language identifier.
     */
    languageId: string;

    /**
     * The version number of this document (it will increase after each
     * change, including undo/redo).
     */
    version: integer;

    /**
     * The content of the opened text document.
     */
    text: string;
}
```


Text documents have a language identifier to identify a document on the server side when it handles more than one language to avoid re-interpreting the file extension. If a document refers to one of the programming languages listed below it is recommended that clients use those ids.

| Language | Identifier |
|----|----|
| ABAP | `abap` |
| Windows Bat | `bat` |
| BibTeX | `bibtex` |
| Clojure | `clojure` |
| Coffeescript | `coffeescript` |
| C | `c` |
| C++ | `cpp` |
| C# | `csharp` |
| CSS | `css` |
| Diff | `diff` |
| Dart | `dart` |
| Dockerfile | `dockerfile` |
| Elixir | `elixir` |
| Erlang | `erlang` |
| F# | `fsharp` |
| Git | `git-commit` and `git-rebase` |
| Go | `go` |
| Groovy | `groovy` |
| Handlebars | `handlebars` |
| HTML | `html` |
| Ini | `ini` |
| Java | `java` |
| JavaScript | `javascript` |
| JavaScript React | `javascriptreact` |
| JSON | `json` |
| LaTeX | `latex` |
| Less | `less` |
| Lua | `lua` |
| Makefile | `makefile` |
| Markdown | `markdown` |
| Objective-C | `objective-c` |
| Objective-C++ | `objective-cpp` |
| Perl | `perl` |
| Perl 6 | `perl6` |
| PHP | `php` |
| Powershell | `powershell` |
| Pug | `jade` |
| Python | `python` |
| R | `r` |
| Razor (cshtml) | `razor` |
| Ruby | `ruby` |
| Rust | `rust` |
| SCSS | `scss` (syntax using curly brackets), `sass` (indented syntax) |
| Scala | `scala` |
| ShaderLab | `shaderlab` |
| Shell Script (Bash) | `shellscript` |
| SQL | `sql` |
| Swift | `swift` |
| TypeScript | `typescript` |
| TypeScript React | `typescriptreact` |
| TeX | `tex` |
| Visual Basic | `vb` |
| XML | `xml` |
| XSL | `xsl` |
| YAML | `yaml` |

#### TextDocumentIdentifier

Text documents are identified using a URI. On the protocol level, URIs are passed as strings. The corresponding JSON structure looks like this:


``` highlight
interface TextDocumentIdentifier {
    /**
     * The text document's URI.
     */
    uri: DocumentUri;
}
```


#### VersionedTextDocumentIdentifier

An identifier to denote a specific version of a text document. This information usually flows from the client to the server.


``` highlight
interface VersionedTextDocumentIdentifier extends TextDocumentIdentifier {
    /**
     * The version number of this document.
     *
     * The version number of a document will increase after each change,
     * including undo/redo. The number doesn't need to be consecutive.
     */
    version: integer;
}
```


An identifier which optionally denotes a specific version of a text document. This information usually flows from the server to the client.


``` highlight
interface OptionalVersionedTextDocumentIdentifier extends TextDocumentIdentifier {
    /**
     * The version number of this document. If an optional versioned text document
     * identifier is sent from the server to the client and the file is not
     * open in the editor (the server has not received an open notification
     * before) the server can send `null` to indicate that the version is
     * known and the content on disk is the master (as specified with document
     * content ownership).
     *
     * The version number of a document will increase after each change,
     * including undo/redo. The number doesn't need to be consecutive.
     */
    version: integer | null;
}
```


#### TextDocumentPositionParams

Was `TextDocumentPosition` in 1.0 with inlined parameters.

A parameter literal used in requests to pass a text document and a position inside that document. It is up to the client to decide how a selection is converted into a position when issuing a request for a text document. The client can for example honor or ignore the selection direction to make LSP request consistent with features implemented internally.


``` highlight
interface TextDocumentPositionParams {
    /**
     * The text document.
     */
    textDocument: TextDocumentIdentifier;

    /**
     * The position inside the text document.
     */
    position: Position;
}
```


#### DocumentFilter

A document filter denotes a document through properties like `language`, `scheme` or `pattern`. An example is a filter that applies to TypeScript files on disk. Another example is a filter that applies to JSON files with name `package.json`:


``` highlight
{ language: 'typescript', scheme: 'file' }
{ language: 'json', pattern: '**/package.json' }
```


``` highlight
export interface DocumentFilter {
    /**
     * A language id, like `typescript`.
     */
    language?: string;

    /**
     * A Uri scheme, like `file` or `untitled`.
     */
    scheme?: string;

    /**
     * A glob pattern, like `*.{ts,js}`.
     *
     * Glob patterns can have the following syntax:
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
    pattern?: string;
}
```


Please note that for a document filter to be valid at least one of the properties for `language`, `scheme`, or `pattern` must be set. To keep the type definition simple all properties are marked as optional.

A document selector is the combination of one or more document filters.


``` highlight
export type DocumentSelector = DocumentFilter[];
```


#### TextEdit &amp; AnnotatedTextEdit

> New in version 3.16: Support for `AnnotatedTextEdit`.

A textual edit applicable to a text document.


``` highlight
interface TextEdit {
    /**
     * The range of the text document to be manipulated. To insert
     * text into a document create a range where start === end.
     */
    range: Range;

    /**
     * The string to be inserted. For delete operations use an
     * empty string.
     */
    newText: string;
}
```


Since 3.16.0 there is also the concept of an annotated text edit which supports to add an annotation to a text edit. The annotation can add information describing the change to the text edit.


``` highlight
/**
 * Additional information that describes document changes.
 *
 * @since 3.16.0
 */
export interface ChangeAnnotation {
    /**
     * A human-readable string describing the actual change. The string
     * is rendered prominent in the user interface.
     */
    label: string;

    /**
     * A flag which indicates that user confirmation is needed
     * before applying the change.
     */
    needsConfirmation?: boolean;

    /**
     * A human-readable string which is rendered less prominent in
     * the user interface.
     */
    description?: string;
}
```


Usually clients provide options to group the changes along the annotations they are associated with. To support this in the protocol an edit or resource operation refers to a change annotation using an identifier and not the change annotation literal directly. This allows servers to use the identical annotation across multiple edits or resource operations which then allows clients to group the operations under that change annotation. The actual change annotations together with their identifiers are managed by the workspace edit via the new property `changeAnnotations`.


``` highlight
/**
 * An identifier referring to a change annotation managed by a workspace
 * edit.
 *
 * @since 3.16.0.
 */
export type ChangeAnnotationIdentifier = string;
```


``` highlight
/**
 * A special text edit with an additional change annotation.
 *
 * @since 3.16.0.
 */
export interface AnnotatedTextEdit extends TextEdit {
    /**
     * The actual annotation identifier.
     */
    annotationId: ChangeAnnotationIdentifier;
}
```


#### TextEdit[]

Complex text manipulations are described with an array of `TextEdit`’s or `AnnotatedTextEdit`’s, representing a single change to the document.

All text edits ranges refer to positions in the document they are computed on. They therefore move a document from state S1 to S2 without describing any intermediate state. Text edits ranges must never overlap, that means no part of the original document must be manipulated by more than one edit. However, it is possible that multiple edits have the same start position: multiple inserts, or any number of inserts followed by a single remove or replace edit. If multiple inserts have the same position, the order in the array defines the order in which the inserted strings appear in the resulting text.

#### TextDocumentEdit

> New in version 3.16: support for `AnnotatedTextEdit`. The support is guarded by the client capability `workspace.workspaceEdit.changeAnnotationSupport`. If a client doesn’t signal the capability, servers shouldn’t send `AnnotatedTextEdit` literals back to the client.

Describes textual changes on a single text document. The text document is referred to as a `OptionalVersionedTextDocumentIdentifier` to allow clients to check the text document version before an edit is applied. A `TextDocumentEdit` describes all changes on a version Si and after they are applied move the document to version Si+1. So the creator of a `TextDocumentEdit` doesn’t need to sort the array of edits or do any kind of ordering. However the edits must be non overlapping.


``` highlight
export interface TextDocumentEdit {
    /**
     * The text document to change.
     */
    textDocument: OptionalVersionedTextDocumentIdentifier;

    /**
     * The edits to be applied.
     *
     * @since 3.16.0 - support for AnnotatedTextEdit. This is guarded by the
     * client capability `workspace.workspaceEdit.changeAnnotationSupport`
     */
    edits: (TextEdit | AnnotatedTextEdit)[];
}
```


#### Location

Represents a location inside a resource, such as a line inside a text file.


``` highlight
interface Location {
    uri: DocumentUri;
    range: Range;
}
```


#### LocationLink

Represents a link between a source and a target location.


``` highlight
interface LocationLink {

    /**
     * Span of the origin of this link.
     *
     * Used as the underlined span for mouse interaction. Defaults to the word
     * range at the mouse position.
     */
    originSelectionRange?: Range;

    /**
     * The target resource identifier of this link.
     */
    targetUri: DocumentUri;

    /**
     * The full target range of this link. If the target for example is a symbol
     * then target range is the range enclosing this symbol not including
     * leading/trailing whitespace but everything else like comments. This
     * information is typically used to highlight the range in the editor.
     */
    targetRange: Range;

    /**
     * The range that should be selected and revealed when this link is being
     * followed, e.g the name of a function. Must be contained by the
     * `targetRange`. See also `DocumentSymbol#range`
     */
    targetSelectionRange: Range;
}
```


#### Diagnostic

Represents a diagnostic, such as a compiler error or warning. Diagnostic objects are only valid in the scope of a resource.


``` highlight
export interface Diagnostic {
    /**
     * The range at which the message applies.
     */
    range: Range;

    /**
     * The diagnostic's severity. To avoid interpretation mismatches when a
     * server is used with different clients it is highly recommended that
     * servers always provide a severity value. If omitted, it’s recommended
     * for the client to interpret it as an Error severity.
     */
    severity?: DiagnosticSeverity;

    /**
     * The diagnostic's code, which might appear in the user interface.
     */
    code?: integer | string;

    /**
     * An optional property to describe the error code.
     *
     * @since 3.16.0
     */
    codeDescription?: CodeDescription;

    /**
     * A human-readable string describing the source of this
     * diagnostic, e.g. 'typescript' or 'super lint'.
     */
    source?: string;

    /**
     * The diagnostic's message.
     */
    message: string;

    /**
     * Additional metadata about the diagnostic.
     *
     * @since 3.15.0
     */
    tags?: DiagnosticTag[];

    /**
     * An array of related diagnostic information, e.g. when symbol-names within
     * a scope collide all definitions can be marked via this property.
     */
    relatedInformation?: DiagnosticRelatedInformation[];

    /**
     * A data entry field that is preserved between a
     * `textDocument/publishDiagnostics` notification and
     * `textDocument/codeAction` request.
     *
     * @since 3.16.0
     */
    data?: LSPAny;
}
```


The protocol currently supports the following diagnostic severities and tags:


``` highlight
export namespace DiagnosticSeverity {
    /**
     * Reports an error.
     */
    export const Error: 1 = 1;
    /**
     * Reports a warning.
     */
    export const Warning: 2 = 2;
    /**
     * Reports an information.
     */
    export const Information: 3 = 3;
    /**
     * Reports a hint.
     */
    export const Hint: 4 = 4;
}

export type DiagnosticSeverity = 1 | 2 | 3 | 4;
```


``` highlight
/**
 * The diagnostic tags.
 *
 * @since 3.15.0
 */
export namespace DiagnosticTag {
    /**
     * Unused or unnecessary code.
     *
     * Clients are allowed to render diagnostics with this tag faded out
     * instead of having an error squiggle.
     */
    export const Unnecessary: 1 = 1;
    /**
     * Deprecated or obsolete code.
     *
     * Clients are allowed to rendered diagnostics with this tag strike through.
     */
    export const Deprecated: 2 = 2;
}

export type DiagnosticTag = 1 | 2;
```


`DiagnosticRelatedInformation` is defined as follows:


``` highlight
/**
 * Represents a related message and source code location for a diagnostic.
 * This should be used to point to code locations that cause or are related to
 * a diagnostics, e.g when duplicating a symbol in a scope.
 */
export interface DiagnosticRelatedInformation {
    /**
     * The location of this related diagnostic information.
     */
    location: Location;

    /**
     * The message of this related diagnostic information.
     */
    message: string;
}
```


`CodeDescription` is defined as follows:


``` highlight
/**
 * Structure to capture a description for an error code.
 *
 * @since 3.16.0
 */
export interface CodeDescription {
    /**
     * An URI to open with more information about the diagnostic error.
     */
    href: URI;
}
```


#### Command

Represents a reference to a command. Provides a title which will be used to represent a command in the UI. Commands are identified by a string identifier. The recommended way to handle commands is to implement their execution on the server side if the client and server provides the corresponding capabilities. Alternatively the tool extension code could handle the command. The protocol currently doesn’t specify a set of well-known commands.


``` highlight
interface Command {
    /**
     * Title of the command, like `save`.
     */
    title: string;
    /**
     * The identifier of the actual command handler.
     */
    command: string;
    /**
     * Arguments that the command handler should be
     * invoked with.
     */
    arguments?: LSPAny[];
}
```


#### MarkupContent

A `MarkupContent` literal represents a string value which content can be represented in different formats. Currently `plaintext` and `markdown` are supported formats. A `MarkupContent` is usually used in documentation properties of result literals like `CompletionItem` or `SignatureInformation`. If the format is `markdown` the content should follow the [GitHub Flavored Markdown Specification](https://github.github.com/gfm/).


``` highlight
/**
 * Describes the content type that a client supports in various
 * result literals like `Hover`, `ParameterInfo` or `CompletionItem`.
 *
 * Please note that `MarkupKinds` must not start with a `$`. This kinds
 * are reserved for internal usage.
 */
export namespace MarkupKind {
    /**
     * Plain text is supported as a content format
     */
    export const PlainText: 'plaintext' = 'plaintext';

    /**
     * Markdown is supported as a content format
     */
    export const Markdown: 'markdown' = 'markdown';
}
export type MarkupKind = 'plaintext' | 'markdown';
```


``` highlight
/**
 * A `MarkupContent` literal represents a string value which content is
 * interpreted base on its kind flag. Currently the protocol supports
 * `plaintext` and `markdown` as markup kinds.
 *
 * If the kind is `markdown` then the value can contain fenced code blocks like
 * in GitHub issues.
 *
 * Here is an example how such a string can be constructed using
 * JavaScript / TypeScript:
 * ```typescript
 * let markdown: MarkdownContent = {
 *  kind: MarkupKind.Markdown,
 *  value: [
 *      '# Header',
 *      'Some text',
 *      '```typescript',
 *      'someCode();',
 *      '```'
 *  ].join('\n')
 * };
 * ```
 *
 * *Please Note* that clients might sanitize the return markdown. A client could
 * decide to remove HTML from the markdown to avoid script execution.
 */
export interface MarkupContent {
    /**
     * The type of the Markup
     */
    kind: MarkupKind;

    /**
     * The content itself
     */
    value: string;
}
```


In addition clients should signal the markdown parser they are using via the client capability `general.markdown` introduced in version 3.16.0 defined as follows:


``` highlight
/**
 * Client capabilities specific to the used markdown parser.
 *
 * @since 3.16.0
 */
export interface MarkdownClientCapabilities {
    /**
     * The name of the parser.
     */
    parser: string;

    /**
     * The version of the parser.
     */
    version?: string;

    /**
     * A list of HTML tags that the client allows / supports in
     * Markdown.
     *
     * @since 3.17.0
     */
    allowedTags?: string[];
}
```


Known markdown parsers used by clients right now are:

| Parser | Version | Documentation |
|----|----|----|
| marked | 1.1.0 | [Marked Documentation](https://marked.js.org/) |
| Python-Markdown | 3.2.2 | [Python-Markdown Documentation](https://python-markdown.github.io) |

### File Resource changes

> New in version 3.13. Since version 3.16 file resource changes can carry an additional property `changeAnnotation` to describe the actual change in more detail. Whether a client has support for change annotations is guarded by the client capability `workspace.workspaceEdit.changeAnnotationSupport`.

File resource changes allow servers to create, rename and delete files and folders via the client. Note that the names talk about files but the operations are supposed to work on files and folders. This is in line with other naming in the Language Server Protocol (see file watchers which can watch files and folders). The corresponding change literals look as follows:


``` highlight
/**
 * Options to create a file.
 */
export interface CreateFileOptions {
    /**
     * Overwrite existing file. Overwrite wins over `ignoreIfExists`
     */
    overwrite?: boolean;

    /**
     * Ignore if exists.
     */
    ignoreIfExists?: boolean;
}
```


``` highlight
/**
 * Create file operation
 */
export interface CreateFile {
    /**
     * A create
     */
    kind: 'create';

    /**
     * The resource to create.
     */
    uri: DocumentUri;

    /**
     * Additional options
     */
    options?: CreateFileOptions;

    /**
     * An optional annotation identifier describing the operation.
     *
     * @since 3.16.0
     */
    annotationId?: ChangeAnnotationIdentifier;
}
```


``` highlight
/**
 * Rename file options
 */
export interface RenameFileOptions {
    /**
     * Overwrite target if existing. Overwrite wins over `ignoreIfExists`
     */
    overwrite?: boolean;

    /**
     * Ignores if target exists.
     */
    ignoreIfExists?: boolean;
}
```


``` highlight
/**
 * Rename file operation
 */
export interface RenameFile {
    /**
     * A rename
     */
    kind: 'rename';

    /**
     * The old (existing) location.
     */
    oldUri: DocumentUri;

    /**
     * The new location.
     */
    newUri: DocumentUri;

    /**
     * Rename options.
     */
    options?: RenameFileOptions;

    /**
     * An optional annotation identifier describing the operation.
     *
     * @since 3.16.0
     */
    annotationId?: ChangeAnnotationIdentifier;
}
```


``` highlight
/**
 * Delete file options
 */
export interface DeleteFileOptions {
    /**
     * Delete the content recursively if a folder is denoted.
     */
    recursive?: boolean;

    /**
     * Ignore the operation if the file doesn't exist.
     */
    ignoreIfNotExists?: boolean;
}
```


``` highlight
/**
 * Delete file operation
 */
export interface DeleteFile {
    /**
     * A delete
     */
    kind: 'delete';

    /**
     * The file to delete.
     */
    uri: DocumentUri;

    /**
     * Delete options.
     */
    options?: DeleteFileOptions;

    /**
     * An optional annotation identifier describing the operation.
     *
     * @since 3.16.0
     */
    annotationId?: ChangeAnnotationIdentifier;
}
```


#### WorkspaceEdit

A workspace edit represents changes to many resources managed in the workspace. The edit should either provide `changes` or `documentChanges`. If the client can handle versioned document edits and if `documentChanges` are present, the latter are preferred over `changes`.

Since version 3.13.0 a workspace edit can contain resource operations (create, delete or rename files and folders) as well. If resource operations are present clients need to execute the operations in the order in which they are provided. So a workspace edit for example can consist of the following two changes: (1) create file a.txt and (2) a text document edit which insert text into file a.txt. An invalid sequence (e.g. (1) delete file a.txt and (2) insert text into file a.txt) will cause failure of the operation. How the client recovers from the failure is described by the client capability: `workspace.workspaceEdit.failureHandling`


``` highlight
export interface WorkspaceEdit {
    /**
     * Holds changes to existing resources.
     */
    changes?: { [uri: DocumentUri]: TextEdit[]; };

    /**
     * Depending on the client capability
     * `workspace.workspaceEdit.resourceOperations` document changes are either
     * an array of `TextDocumentEdit`s to express changes to n different text
     * documents where each text document edit addresses a specific version of
     * a text document. Or it can contain above `TextDocumentEdit`s mixed with
     * create, rename and delete file / folder operations.
     *
     * Whether a client supports versioned document edits is expressed via
     * `workspace.workspaceEdit.documentChanges` client capability.
     *
     * If a client neither supports `documentChanges` nor
     * `workspace.workspaceEdit.resourceOperations` then only plain `TextEdit`s
     * using the `changes` property are supported.
     */
    documentChanges?: (
        TextDocumentEdit[] |
        (TextDocumentEdit | CreateFile | RenameFile | DeleteFile)[]
    );

    /**
     * A map of change annotations that can be referenced in
     * `AnnotatedTextEdit`s or create, rename and delete file / folder
     * operations.
     *
     * Whether clients honor this property depends on the client capability
     * `workspace.changeAnnotationSupport`.
     *
     * @since 3.16.0
     */
    changeAnnotations?: {
        [id: string /* ChangeAnnotationIdentifier */]: ChangeAnnotation;
    };
}
```


##### WorkspaceEditClientCapabilities

> New in version 3.13: `ResourceOperationKind` and `FailureHandlingKind` and the client capability `workspace.workspaceEdit.resourceOperations` as well as `workspace.workspaceEdit.failureHandling`.

The capabilities of a workspace edit has evolved over the time. Clients can describe their support using the following client capability:

*Client Capability*:

- property path (optional): `workspace.workspaceEdit`
- property type: `WorkspaceEditClientCapabilities` defined as follows:


``` highlight
export interface WorkspaceEditClientCapabilities {
    /**
     * The client supports versioned document changes in `WorkspaceEdit`s
     */
    documentChanges?: boolean;

    /**
     * The resource operations the client supports. Clients should at least
     * support 'create', 'rename' and 'delete' files and folders.
     *
     * @since 3.13.0
     */
    resourceOperations?: ResourceOperationKind[];

    /**
     * The failure handling strategy of a client if applying the workspace edit
     * fails.
     *
     * @since 3.13.0
     */
    failureHandling?: FailureHandlingKind;

    /**
     * Whether the client normalizes line endings to the client specific
     * setting.
     * If set to `true` the client will normalize line ending characters
     * in a workspace edit to the client specific new line character(s).
     *
     * @since 3.16.0
     */
    normalizesLineEndings?: boolean;

    /**
     * Whether the client in general supports change annotations on text edits,
     * create file, rename file and delete file changes.
     *
     * @since 3.16.0
     */
    changeAnnotationSupport?: {
        /**
         * Whether the client groups edits with equal labels into tree nodes,
         * for instance all edits labelled with "Changes in Strings" would
         * be a tree node.
         */
        groupsOnLabel?: boolean;
    };
}
```


``` highlight
/**
 * The kind of resource operations supported by the client.
 */
export type ResourceOperationKind = 'create' | 'rename' | 'delete';

export namespace ResourceOperationKind {

    /**
     * Supports creating new files and folders.
     */
    export const Create: ResourceOperationKind = 'create';

    /**
     * Supports renaming existing files and folders.
     */
    export const Rename: ResourceOperationKind = 'rename';

    /**
     * Supports deleting existing files and folders.
     */
    export const Delete: ResourceOperationKind = 'delete';
}
```


``` highlight
export type FailureHandlingKind = 'abort' | 'transactional' | 'undo'
    | 'textOnlyTransactional';

export namespace FailureHandlingKind {

    /**
     * Applying the workspace change is simply aborted if one of the changes
     * provided fails. All operations executed before the failing operation
     * stay executed.
     */
    export const Abort: FailureHandlingKind = 'abort';

    /**
     * All operations are executed transactional. That means they either all
     * succeed or no changes at all are applied to the workspace.
     */
    export const Transactional: FailureHandlingKind = 'transactional';


    /**
     * If the workspace edit contains only textual file changes they are
     * executed transactional. If resource changes (create, rename or delete
     * file) are part of the change the failure handling strategy is abort.
     */
    export const TextOnlyTransactional: FailureHandlingKind
        = 'textOnlyTransactional';

    /**
     * The client tries to undo the operations already executed. But there is no
     * guarantee that this is succeeding.
     */
    export const Undo: FailureHandlingKind = 'undo';
}
```


#### Work Done Progress

> *Since version 3.15.0*

Work done progress is reported using the generic [`$/progress`](#progress) notification. The value payload of a work done progress notification can be of three different forms.

##### Work Done Progress Begin

To start progress reporting a `$/progress` notification with the following payload must be sent:


``` highlight
export interface WorkDoneProgressBegin {

    kind: 'begin';

    /**
     * Mandatory title of the progress operation. Used to briefly inform about
     * the kind of operation being performed.
     *
     * Examples: "Indexing" or "Linking dependencies".
     */
    title: string;

    /**
     * Controls if a cancel button should show to allow the user to cancel the
     * long running operation. Clients that don't support cancellation are
     * allowed to ignore the setting.
     */
    cancellable?: boolean;

    /**
     * Optional, more detailed associated progress message. Contains
     * complementary information to the `title`.
     *
     * Examples: "3/25 files", "project/src/module2", "node_modules/some_dep".
     * If unset, the previous progress message (if any) is still valid.
     */
    message?: string;

    /**
     * Optional progress percentage to display (value 100 is considered 100%).
     * If not provided infinite progress is assumed and clients are allowed
     * to ignore the `percentage` value in subsequent report notifications.
     *
     * The value should be steadily rising. Clients are free to ignore values
     * that are not following this rule. The value range is [0, 100].
     */
    percentage?: uinteger;
}
```


##### Work Done Progress Report

Reporting progress is done using the following payload:


``` highlight
export interface WorkDoneProgressReport {

    kind: 'report';

    /**
     * Controls enablement state of a cancel button. This property is only valid
     * if a cancel button got requested in the `WorkDoneProgressBegin` payload.
     *
     * Clients that don't support cancellation or don't support control the
     * button's enablement state are allowed to ignore the setting.
     */
    cancellable?: boolean;

    /**
     * Optional, more detailed associated progress message. Contains
     * complementary information to the `title`.
     *
     * Examples: "3/25 files", "project/src/module2", "node_modules/some_dep".
     * If unset, the previous progress message (if any) is still valid.
     */
    message?: string;

    /**
     * Optional progress percentage to display (value 100 is considered 100%).
     * If not provided infinite progress is assumed and clients are allowed
     * to ignore the `percentage` value in subsequent report notifications.
     *
     * The value should be steadily rising. Clients are free to ignore values
     * that are not following this rule. The value range is [0, 100].
     */
    percentage?: uinteger;
}
```


##### Work Done Progress End

Signaling the end of a progress reporting is done using the following payload:


``` highlight
export interface WorkDoneProgressEnd {

    kind: 'end';

    /**
     * Optional, a final message indicating to for example indicate the outcome
     * of the operation.
     */
    message?: string;
}
```


##### Initiating Work Done Progress

Work Done progress can be initiated in two different ways:

1.  by the sender of a request (mostly clients) using the predefined `workDoneToken` property in the requests parameter literal. The document will refer to this kind of progress as client initiated progress.
2.  by a server using the request `window/workDoneProgress/create`. The document will refer to this kind of progress as server initiated progress.

###### Client Initiated Progress

Consider a client sending a `textDocument/reference` request to a server and the client accepts work done progress reporting on that request. To signal this to the server the client would add a `workDoneToken` property to the reference request parameters. Something like this:


``` highlight
{
    "textDocument": {
        "uri": "file:///folder/file.ts"
    },
    "position": {
        "line": 9,
        "character": 5
    },
    "context": {
        "includeDeclaration": true
    },
    // The token used to report work done progress.
    "workDoneToken": "1d546990-40a3-4b77-b134-46622995f6ae"
}
```


The corresponding type definition for the parameter property looks like this:


``` highlight
export interface WorkDoneProgressParams {
    /**
     * An optional token that a server can use to report work done progress.
     */
    workDoneToken?: ProgressToken;
}
```


A server uses the `workDoneToken` to report progress for the specific `textDocument/reference`. For the above request the `$/progress` notification params look like this:


``` highlight
{
    "token": "1d546990-40a3-4b77-b134-46622995f6ae",
    "value": {
        "kind": "begin",
        "title": "Finding references for A#foo",
        "cancellable": false,
        "message": "Processing file X.ts",
        "percentage": 0
    }
}
```


The token received via the `workDoneToken` property in a request’s param literal is only valid as long as the request has not send a response back. Canceling work done progress is done by simply canceling the corresponding request.

There is no specific client capability signaling whether a client will send a progress token per request. The reason for this is that this is in many clients not a static aspect and might even change for every request instance for the same request type. So the capability is signal on every request instance by the presence of a `workDoneToken` property.

To avoid that clients set up a progress monitor user interface before sending a request but the server doesn’t actually report any progress a server needs to signal general work done progress reporting support in the corresponding server capability. For the above find references example a server would signal such a support by setting the `referencesProvider` property in the server capabilities as follows:


``` highlight
{
    "referencesProvider": {
        "workDoneProgress": true
    }
}
```


The corresponding type definition for the server capability looks like this:


``` highlight
export interface WorkDoneProgressOptions {
    workDoneProgress?: boolean;
}
```


###### Server Initiated Progress

Servers can also initiate progress reporting using the `window/workDoneProgress/create` request. This is useful if the server needs to report progress outside of a request (for example the server needs to re-index a database). The token can then be used to report progress using the same notifications used as for client initiated progress. The token provided in the create request should only be used once (e.g. only one begin, many report and one end notification should be sent to it).

To keep the protocol backwards compatible servers are only allowed to use `window/workDoneProgress/create` request if the client signals corresponding support using the client capability `window.workDoneProgress` which is defined as follows:


``` highlight
 /**
     * Window specific client capabilities.
     */
    window?: {
        /**
         * Whether client supports server initiated progress using the
         * `window/workDoneProgress/create` request.
         */
        workDoneProgress?: boolean;
    };
```


#### Partial Result Progress

> *Since version 3.15.0*

Partial results are also reported using the generic [`$/progress`](#progress) notification. The value payload of a partial result progress notification is in most cases the same as the final result. For example the `workspace/symbol` request has `SymbolInformation[]` \| `WorkspaceSymbol[]` as the result type. Partial result is therefore also of type `SymbolInformation[]` \| `WorkspaceSymbol[]`. Whether a client accepts partial result notifications for a request is signaled by adding a `partialResultToken` to the request parameter. For example, a `textDocument/reference` request that supports both work done and partial result progress might look like this:


``` highlight
{
    "textDocument": {
        "uri": "file:///folder/file.ts"
    },
    "position": {
        "line": 9,
        "character": 5
    },
    "context": {
        "includeDeclaration": true
    },
    // The token used to report work done progress.
    "workDoneToken": "1d546990-40a3-4b77-b134-46622995f6ae",
    // The token used to report partial result progress.
    "partialResultToken": "5f6f349e-4f81-4a3b-afff-ee04bff96804"
}
```


The `partialResultToken` is then used to report partial results for the find references request.

If a server reports partial result via a corresponding `$/progress`, the whole result must be reported using n `$/progress` notifications. Each of the n `$/progress` notification appends items to the result. The final response has to be empty in terms of result values. This avoids confusion about how the final result should be interpreted, e.g. as another partial result or as a replacing result.

If the response errors the provided partial results should be treated as follows:

- the `code` equals to `RequestCancelled`: the client is free to use the provided results but should make clear that the request got canceled and may be incomplete.
- in all other cases the provided partial results shouldn’t be used.

#### PartialResultParams

A parameter literal used to pass a partial result token.


``` highlight
export interface PartialResultParams {
    /**
     * An optional token that a server can use to report partial results (e.g.
     * streaming) to the client.
     */
    partialResultToken?: ProgressToken;
}
```


#### TraceValue

A `TraceValue` represents the level of verbosity with which the server systematically reports its execution trace using [\$/logTrace](#logTrace) notifications. The initial trace value is set by the client at initialization and can be modified later using the [\$/setTrace](#setTrace) notification.


``` highlight
export type TraceValue = 'off' | 'messages' | 'verbose';
```

## Related

- [[Symbol Model]] — Marksman's internal symbol types, which map to LSP `DocumentSymbol` and related structures
- [[Data Flow]] — how JSON structures flow through the server pipeline
