using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using KidsGameLauncher;
using KidsGameLauncher.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// AppDataService owns all persisted state (profiles, games, access rules).
// AppState is per-session only (which profile is currently active) and is
// intentionally NOT persisted to storage.
builder.Services.AddScoped<AppDataService>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<PlayTimeTracker>();

builder.Services
    .AddBlazorise(options => options.Immediate = true)
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

await builder.Build().RunAsync();
