namespace Marki.Core

open Markdig

[<RequireQualifiedAccess>]
module Markdown =
    let private pipeline =
        lazy
            (MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build())

    
    let toHtml text =
        let document = Markdown.Parse text
        Markdown.ToHtml(document, pipeline.Value)
        
    let toText text =
        Markdown.ToPlainText(text, pipeline.Value)
