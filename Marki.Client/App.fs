#nowarn "0020"

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Fun.Blazor
open Fun.Blazor.Router
open Marki.Client.Components

// define our application
let app =
    article {
        class' "marki-app"

        main {
            // define our routes and the functions
            // that will handle the view for those routes
            html.route [ routeCi "/" (FsBlog.List())
                         routeCif "/read/%s" FsBlog.View
                         routeCi "/new" (FsBlog.New())
                         routeCif "/edit/%s" (fun id -> FsBlog.View(id, true)) ]
        }

        footer { "Angel D. Munoz 2022" }
    }

// Do the Host Orchestration
let builder =
    WebAssemblyHostBuilder.CreateDefault(Environment.GetCommandLineArgs())

// Set root of the app
builder.AddFunBlazor("#app", app)

// Add Services and other dependency injection features
builder.Services.AddFunBlazorWasm()

// Start the application
builder.Build().RunAsync()
