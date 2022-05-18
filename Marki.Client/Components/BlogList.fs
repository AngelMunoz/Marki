namespace Marki.Client.Components

open Fun.Blazor
open Marki.Core

[<AutoOpen>]
module BlogList =

    [<AutoOpen>]
    module private Utils =
        let truncateTo (post: BlogPost) =
            if post.Content.Length < 100 then
                post.Content.Length
            else
                100

        let titleIndex (post: BlogPost) = post.Content.IndexOf("##")

        let postSummary (post: BlogPost) =
            let titleIndex = titleIndex post
            let truncateTo = truncateTo post
            Markdown.toText (post.Content.Substring(titleIndex, truncateTo))

    let BlogList (paginatedContent: PaginatedResponse<BlogPost>) =
        ul {
            class' "blog-list inner-card"

            childContent
                [ for post in paginatedContent.Items do
                      li {
                          class' "blog-list-item"

                          h4 {
                              a {
                                  href $"/read/{post._id}"
                                  $"{post.Title}"
                              }
                          }

                          p { $"%s{postSummary post}..." }
                      } ]
        }
