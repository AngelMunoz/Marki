// hot-reload
// hot-reload is the flag to let cli know this file should be included
// It has dependency requirement: the root is the app which is used in the Startup.fs
// All other files which want have hot reload, need to drill down to that file, and all the middle file should also add the '// hot-reload' flag at the top of taht file
[<AutoOpen>]
module Marki.Client.App

open FSharp.Data.Adaptive
open Fun.Blazor
open Fun.Blazor.Router
open Marki.Client.Components
open FsToolkit.ErrorHandling

let router =
    [ routeCi "/" (BlogEditor.BlogList())
      routeCif "/read/%s" (fun id -> BlogEditor.EditBlog(id))
      routeCi "/new" (BlogEditor.NewBlog())
      routeCif "/edit/%s" (fun id -> BlogEditor.EditBlog(id, true)) ]

let app =
    adaptiview () {
        article {
            class' "marki-app"
            main { html.route router }
            footer { "Angel D. Munoz 2022" }
        }
    }
