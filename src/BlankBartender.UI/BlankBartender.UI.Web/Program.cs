using BlankBartender.UI.Core.Interfaces;
using BlankBartender.UI.Core.Services;
using BlankBartender.UI.Web;
using BlankBartender.UI.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddHttpClient();


var config = builder.Configuration;
var apiUrl = config["ApiUrl"];

var httpClient = new HttpClient { BaseAddress = new Uri(apiUrl) };
httpClient.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");
httpClient.DefaultRequestHeaders.Add("Access-Control-Allow-Credentials", "true");
builder.Services.AddScoped(_ => httpClient);
builder.Services.AddScoped<IPlatformService, PlatformService>();
Console.WriteLine(apiUrl);
builder.Services.AddScoped<IDrinkClient>(_ => new DrinkClient(apiUrl, httpClient));
builder.Services.AddScoped<IConfigurationClient>(_ => new ConfigurationClient(apiUrl, httpClient));
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IDrinkService, DrinkService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddSingleton<IImageSourceService, WebImageSourceService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
