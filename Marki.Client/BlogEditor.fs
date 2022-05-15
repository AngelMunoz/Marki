namespace Marki.Client.Components

open System
open System.Threading.Tasks
open FSharp.Data.Adaptive
open FSharp.Control.Reactive
open Fun.Blazor
open Marki.Core
open Marki.Core.Http
open Microsoft.AspNetCore.Components

module private BlogList =
    type TagItem = {| selected: bool; tag: string |}

    let private searchTagTpl (item: TagItem) (handleChange: TagItem -> ChangeEventArgs -> unit) =
        li {
            Elts.label {
                input {
                    onchange (handleChange item)
                    type' "checkbox"
                    ``checked`` item.selected
                }

                $"{item.tag}"
            }
        }

    let searchBox (tags: TagItem seq, onQuery: string -> unit, onSelectTag: TagItem -> unit) =

        aside {
            class' "blog-search"

            section { h4 { "search by title" } }

            input {
                placeholder "Search a blog post"
                oninput (fun event -> event.Value |> string |> onQuery)
            }

            ul {
                childContent
                    [ for item in tags do
                          searchTagTpl item (fun tag event ->
                              onSelectTag
                                  {| item with
                                      selected = unbox event.Value |}) ]
            }
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
        html.inject (
            "blog-list",
            fun (hook: IComponentHook) ->
                let content = hook.UseStore { Items = Seq.empty; Count = 0L }

                let tags = hook.UseStore Seq.empty<BlogList.TagItem>

                let searchText = hook.UseStore(Option.defaultValue "" text)
                let tagsQuery = hook.UseStore(Seq.empty<string>)


                [ // fetch when the component initializes
                  hook.OnInitialized
                  |> Observable.map (Blogs.Find)
                  |> Observable.switchTask
                  |> Observable.subscribe content.Publish

                  hook.OnInitialized
                  |> Observable.map (fun _ ->
                      task {
                          let! tags = Blogs.FindTags()

                          return
                              tags
                              |> Seq.map (fun t -> {| selected = false; tag = t |})
                      })
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

                let updateTags (item: BlogList.TagItem) =
                    // update the tag list with the selections
                    let updatedValues =
                        tags.Current
                        |> Seq.map (fun existing ->
                            if existing.tag = item.tag then
                                item
                            else
                                existing)

                    updatedValues |> tags.Publish
                    // push selected tags into the tagsQuery
                    updatedValues
                    |> Seq.filter (fun item -> item.selected)
                    |> Seq.map (fun item -> item.tag)
                    |> tagsQuery.Publish

                adaptiview () {
                    let! paginatedContent = hook.UseAVal content
                    let! tagValues = hook.UseAVal tags

                    article {
                        class' "blog-page"
                        BlogList.searchBox (tagValues, searchText.Publish, updateTags)

                        section {
                            class' "blog-content"

                            header {
                                class' "page-header"
                                h1 { "The F# Web Assembly Blog" }
                            }

                            BlogList.blogList paginatedContent
                        }
                    }
                }
        )

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
