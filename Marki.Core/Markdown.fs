namespace Marki.Core

open Markdig
open Markdig.Prism

[<RequireQualifiedAccess>]
module Markdown =
    let private pipeline =
        lazy
            (MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePreciseSourceLocation()
                .DisableHtml()
                .UsePrism()
                .Build())


    let toHtml text =
        let document = Markdown.Parse text
        Markdown.ToHtml(document, pipeline.Value)

    let toText text =
        Markdown.ToPlainText(text, pipeline.Value)
