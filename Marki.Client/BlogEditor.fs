namespace Marki.Client.Components

open System
open System.Threading.Tasks
open FSharp.Data.Adaptive
open FSharp.Control.Reactive
open Fun.Blazor
open Marki.Core
open Marki.Core.Http
open Microsoft.AspNetCore.Components

module private Components =

    let searchBox (tags: string seq, onQuery: string -> unit, onSelectTag: string seq -> unit) =
        let tagList =
            let tags = cval (tags |> Seq.map (fun s -> (false, s)))

            let handleClick tag setTags (event: ChangeEventArgs) =
                printfn "%A" event.Value
                let value = tags.Value |> Seq.map(fun (s, t) -> if t = tag then (event.Value :?> bool, t) else (s, t))

                value |> setTags
                value |> Seq.filter (fun (s, _) -> s) |> Seq.map snd |> onSelectTag

            adaptiview () {
                let! tags, setTags = tags.WithSetter()

                ul {
                    childContent
                        [ for (sel, tag) in tags do
                              li {
                                  Elts.label {
                                      input {
                                          onchange (handleClick tag setTags)
                                          type' "checkbox"
                                          ``checked`` sel
                                      }

                                      $"{tag}"
                                  }
                              } ]
                }
            }


        aside {
            class' "blog-search"

            section { h4 { "search by title" } }

            input {
                placeholder "Search a blog post"
                oninput (fun event -> event.Value |> string |> onQuery)
            }

            tagList
        }

    let blogList (paginatedContent: PaginatedResponse<BlogPost>) =
        ul {
            class' "blog-list"

            childContent
                [ for post in paginatedContent.Items do
                      li { h3 { post.Title } } ]
        }

type BlogEditor =
    static member BlogList(?text: string) =
        html.inject (fun (hook: IComponentHook) ->
            let content = hook.UseStore { Items = Seq.empty; Count = 0L }

            let tags = hook.UseStore Seq.empty<string>

            let searchText = hook.UseStore(Option.defaultValue "" text)
            let tagsQuery = hook.UseStore(Seq.empty<string>)


            [ // fetch when the component initializes
              hook.OnInitialized
              |> Observable.map (Blogs.Find)
              |> Observable.switchTask
              |> Observable.subscribe content.Publish

              hook.OnInitialized
              |> Observable.map (Blogs.FindTags)
              |> Observable.switchTask
              |> Observable.subscribe tags.Publish
              // fetch blogs when searchbox is used
              searchText.Observable
              |> Observable.throttle (TimeSpan.FromSeconds(0.528))
              |> Observable.distinctUntilChanged
              |> Observable.map (fun text -> Blogs.Find(text))
              |> Observable.switchTask
              |> Observable.subscribe content.Publish

              tagsQuery.Observable
              |> Observable.throttle (TimeSpan.FromSeconds(0.528))
              |> Observable.distinctUntilChanged
              |> Observable.map (fun tags -> Blogs.Find(searchText.Current, tags))
              |> Observable.switchTask
              |> Observable.subscribe content.Publish ]
            // dispose every subscription
            |> hook.AddDisposes

            adaptiview () {
                let! paginatedContent = hook.UseAVal content
                let! tags = hook.UseAVal tags

                article {
                    class' "blog-page"
                    Components.searchBox (tags, searchText.Publish, tagsQuery.Publish)

                    section {
                        class' "blog-content"

                        header {
                            class' "page-header"
                            h1 { "The F# Web Assembly Blog" }
                        }

                        Components.blogList paginatedContent
                    }
                }
            })

    static member NewBlog() =
        article {
            class' "blog-page"
            "New Blog"
        }

    static member EditBlog(blogId: string) =

        section {
            class' "editor-area"

            styleElt {
                ruleset ".editor-area" {
                    displayFlex
                    justifyContentSpaceAround
                }

                ruleset ".form-section" {
                    displayFlex
                    flexDirectionColumn
                    gap "8px"
                }

                ruleset ".editor-preview" { fontSize "1.5rem" }
                ruleset ".editor__title" { margin "0" }
            }

            Elts.form {
                section {
                    class' "form-section"
                    Elts.label { "Markdown Editor" }

                    textarea { rows 10 }
                }
            }

            article {
                class' "editor-preview"

                h4 {
                    class' "editor__title"
                    "Preview"
                }

                section { childContentRaw (Markdown.toHtml "") }
            }
        }
