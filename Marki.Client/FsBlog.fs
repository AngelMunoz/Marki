//hot-reload
namespace Marki.Client.Components

open FSharp.Data.Adaptive
open FSharp.Control.Reactive
open Fun.Blazor
open Marki.Core
open Marki.Core.Http
open Marki.Client
open Marki.Client.Components

type FsBlog =

    static member New() =
        article {
            class' "blog-page"
            "New Blog"
        }

    static member List() =
        html.inject (
            "blog-list",
            fun (hook: IComponentHook) ->
                let paginatedContent = hook.UseStore { Items = Seq.empty; Count = 0L }

                let tags = hook.UseStore Seq.empty<TagItem>
                // Here we can Tap into LifeCycle hooks
                [ // fetch when the component initializes
                  hook.OnInitialized
                  |> Observable.map (Blogs.Find)
                  |> Observable.switchTask
                  |> Observable.subscribe paginatedContent.Publish

                  hook.OnInitialized
                  |> Observable.map (fun _ -> Blogs.FindTags())
                  |> Observable.switchTask
                  |> Observable.map (
                      // transform the string sequence into a
                      // TagItem sequence
                      Seq.mapi (fun i tag ->
                          {| index = i
                             tag = tag
                             selected = false |})
                  )
                  |> Observable.subscribe tags.Publish ]
                // dispose every subscription to avoid memory leaks
                |> hook.AddDisposes

                adaptiview () {
                    // seamlessly transition from stores
                    // to data our components can access and consume
                    let! blogPosts = hook.UseAVal paginatedContent
                    article {
                        class' "blog-page blog-summary"
                        BlogPostSearch(paginatedContent, tags)

                        section {
                            class' "blog-content"

                            header {
                                class' "page-header"
                                h1 { "The F# Web Assembly Blog" }
                            }

                            BlogList blogPosts
                        }
                    }
                }
        )

    static member View(blogId: string, ?edit: bool) =
        let edit = defaultArg edit false

        html.inject (
            "blog-view",
            fun (hook: IComponentHook) ->
                let blog = hook.UseStore None

                hook.OnInitialized
                |> Observable.map (fun _ -> Blogs.FindOne blogId)
                |> Observable.switchTask
                |> Observable.subscribe (fun post -> post |> Some |> blog.Publish)
                |> hook.AddDispose


                adaptiview () {
                    let! blogContent = hook.UseAVal blog

                    article {
                        class' "blog-page blog-view"

                        match blogContent, edit with
                        | Some blog, true -> Blog.Editor blog
                        | Some blog, false -> Blog.View blog
                        | None, _ -> "Blog not found"
                    }
                }
        )
