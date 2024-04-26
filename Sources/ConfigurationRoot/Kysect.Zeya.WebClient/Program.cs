using Kysect.Zeya.WebApiClient;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Kysect.Zeya.WebClient;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var uri = new Uri("https://localhost:7062/");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = uri });
builder.Services.AddZeyaRefit(uri);
builder.Services.AddFluentUIComponents();
await builder.Build().RunAsync();
