namespace Marki.Core

open System
open Markdig
open Markdig.Prism
open Marki.Core

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

    let getSummary (post: BlogPost) =
        let truncateTo =
            if post.Content.Length < 100 then
                post.Content.Length
            else 100
        
        post.Content.Substring(0, truncateTo) |> toHtml 
