namespace Marki.Client.Components

open Fun.Blazor
open Marki.Core

[<RequireQualifiedAccess>]
module Blog =
    let View (blog: BlogPost) =
        section {
            class' "blog-view"

            header {
                class' "page-header"

                h1 {
                    a {
                        href $"/"
                        Template.html $"<sl-icon name='arrow-left-circle'></sl-icon>"
                    }

                    $" {blog.Title}"
                }
            }

            article {
                class' "blog-text inner-card"
                childContentRaw (Markdown.toHtml (blog.Content))
            }
        }

    let Editor (blog: BlogPost) =
        section {
            class' "editor-area"

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
