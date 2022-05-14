#nowarn "0020"

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Fun.Blazor
open Marki.Client

let builder = WebAssemblyHostBuilder.CreateDefault(Environment.GetCommandLineArgs())

#if DEBUG
builder.AddFunBlazor("#app", html.hotReloadComp(app, "Marki.Client.App.app"))
#else
builder.AddFunBlazor("#app", app)
#endif

builder.Services.AddFunBlazorWasm()

builder.Build().RunAsync()
