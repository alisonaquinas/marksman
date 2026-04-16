---
title: "LSP 3.17 — Overview & What's New"
source: https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
date: 2026-04-16
tags:
  - wiki/input
  - research/lsp
---

# Language Server Protocol Specification - 3.17


This document describes the 3.17.x version of the language server protocol. An implementation for node of the 3.17.x version of the protocol can be found [here](https://github.com/Microsoft/vscode-languageserver-node).

**Note:** edits to this specification can be made via a pull request against this markdown [document](https://github.com/Microsoft/language-server-protocol/blob/gh-pages/_specifications/lsp/3.17/specification.md).

## What’s new in 3.17

All new 3.17 features are tagged with a corresponding since version 3.17 text or in JSDoc using `@since 3.17.0` annotation. Major new feature are: type hierarchy, inline values, inlay hints, notebook document support and a meta model that describes the 3.17 LSP version.

A detailed list of the changes can be found in the [change log](#version_3_17_0)

The version of the specification is used to group features into a new specification release and to refer to their first appearance. Features in the spec are kept compatible using so called capability flags which are exchanged between the client and the server during initialization.

