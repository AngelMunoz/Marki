namespace Marki.Client.Components

open System
open FSharp.Data.Adaptive
open FSharp.Control.Reactive
open Fun.Blazor
open Marki.Core
open Marki.Core.Http
open Microsoft.AspNetCore.Components
open Marki.Client

[<AutoOpen>]
module Search =
    [<AutoOpen>]
    module private InnerComponents =
        let TagListItem (item: TagItem, tagChanged: TagItem -> unit) =

            li {
                class' "tag-list-item"

                Elts.label {
                    input {
                        onchange (fun event ->
                            tagChanged
                                {| item with
                                    selected = unbox event.Value |})

                        type' "checkbox"
                        checked item.selected
                    }

                    $"{item.tag}"
                }
            }

    let BlogPostSearch (paginated: IStore<PaginatedResponse<BlogPost>>, tags: IStore<TagItem seq>) =
        html.inject (
            "search-box",
            fun (hook: IComponentHook) ->

                let textQuery = hook.UseStore ""

                let tagQuery = hook.UseStore(Seq.empty<string>)

                let tagChanged (item: TagItem) =
                    tags.Current
                    |> Seq.updateAt item.index item
                    |> tags.Publish

                [ // fetch blogs when searchbox is used
                  textQuery.Observable
                  |> Observable.throttle (TimeSpan.FromSeconds(0.528))
                  |> Observable.distinctUntilChanged
                  |> Observable.map (fun text -> Blogs.Find(text))
                  |> Observable.switchTask
                  |> Observable.subscribe paginated.Publish
                  // fetch blogs when the user changes tag selection
                  tagQuery.Observable
                  |> Observable.throttle (TimeSpan.FromSeconds(0.528))
                  |> Observable.distinctUntilChanged
                  |> Observable.map (fun tags -> Blogs.Find(textQuery.Current, tags))
                  |> Observable.switchTask
                  |> Observable.subscribe paginated.Publish ]
                |> hook.AddDisposes

                adaptiview () {
                    let! tags = hook.UseCVal tags

                    aside {
                        class' "blog-search inner-card"
                        section { h4 { "Search By Title And Tags" } }

                        input {
                            placeholder "Search a blog post"
                            oninput (fun event -> event.Value |> string |> textQuery.Publish)
                        }

                        ul {
                            class' "tag-list"

                            childContent
                                [ for item in tags do
                                      TagListItem(item, tagChanged) ]
                        }
                    }
                }
        )
