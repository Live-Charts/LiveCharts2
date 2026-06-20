using BlazorSample;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

#if UI_TESTING
using Factos.Blazor;
#endif

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

#if UI_TESTING
builder.Services.AddScoped<BlazorInterop>();
#endif

await builder.Build().RunAsync();
