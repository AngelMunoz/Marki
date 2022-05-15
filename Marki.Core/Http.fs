module Marki.Core.Http

open System
open System.Runtime.InteropServices
open System.Threading.Tasks
open Flurl
open Flurl.Http
open Marki.Core

type Blogs =

    static member FindTags() =
        "http://localhost:5000/tags"
            .GetJsonAsync<string seq>()

    static member Find([<Optional>] ?text: string, [<Optional>] ?tags: string seq) =
        task {
            let text =
                match text with
                | Some text when String.IsNullOrWhiteSpace text |> not -> text
                // In this case we do want it to be null if we don't have
                // a value in the parameter
                | _ -> null

            let tags =
                match tags with
                | Some tags -> tags
                | None -> null

            let! response =
                "http://localhost:5000/blogposts"
                    .SetQueryParams({| title = text; tags = tags |}, NullValueHandling.Remove)
                    .WithHeader("Accept", "application/json")
                    .GetJsonAsync<PaginatedResponse<BlogPost>>()

            return response
        }

    static member Create(title: string, content: string, [<Optional>] ?tags: string seq) =
        let tags = defaultArg tags [ "uncategorized" ]

        let payload =
            { Title = title
              Content = content
              Tags = tags }

        task {
            let! res =
                "https://localhost:5001/blogposts"
                    .PostJsonAsync(payload)

            return! res.GetJsonAsync<BlogPost>()
        }

    static member Update(post: BlogPost) : Task =
        "https://localhost:5001/blogposts"
            .PutJsonAsync(post)
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.StartAsTask
        :> Task
