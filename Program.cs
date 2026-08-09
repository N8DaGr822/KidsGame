using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
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

await builder.Build().RunAsync();
