using BlankBartender.UI.Core.Interfaces;
using BlankBartender.UI.Core.Services;
using BlankBartender.UI.Web;
using BlankBartender.UI.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddHttpClient();

var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
httpClient.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");
httpClient.DefaultRequestHeaders.Add("Access-Control-Allow-Credentials", "true");
builder.Services.AddScoped(_ => httpClient);
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IDrinkClient>(_ => new DrinkClient(builder.HostEnvironment.BaseAddress, httpClient));
builder.Services.AddScoped<IConfigurationClient>(_ => new ConfigurationClient(builder.HostEnvironment.BaseAddress, httpClient));
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IDrinkService, DrinkService>();
builder.Services.AddScoped<IStatusService, StatusService>();

await builder.Build().RunAsync();
