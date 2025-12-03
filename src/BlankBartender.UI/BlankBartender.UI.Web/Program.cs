using BlankBartender.UI.Core.Interfaces;
using BlankBartender.UI.Core.Services;
using BlankBartender.UI.Web;
using BlankBartender.UI.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Polly;
using BlankBartender.UI.Core.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// compute api base
var hostUri = new Uri(builder.HostEnvironment.BaseAddress);
var apiBase = $"http://{hostUri.Host}:5000/";

// configure a named HttpClient for API calls (policies applied here) and also register typed clients below
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// make default HttpClient resolve to the named client
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

builder.Services.AddScoped<IPlatformService, PlatformService>();
Console.WriteLine(apiBase);

// Register generated NSwag clients using factories matching their constructors
builder.Services.AddScoped<IDrinkClient>(sp => new DrinkClient(apiBase, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
builder.Services.AddScoped<ILiquidsClient>(sp => new LiquidsClient(apiBase, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
builder.Services.AddScoped<IPumpsClient>(sp => new PumpsClient(apiBase, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
builder.Services.AddScoped<ISettingsClient>(sp => new SettingsClient(apiBase, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
builder.Services.AddScoped<ISystemClient>(sp => new SystemClient(apiBase, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));

// Register the small adapter (optional) and high-level services
builder.Services.AddHttpClient<BlankBartender.UI.Core.Http.IConfigurationApi, BlankBartender.UI.Core.Http.HttpConfigurationApi>(client => client.BaseAddress = new Uri(apiBase));

builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IDrinkService, DrinkService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddSingleton<IImageSourceService, WebImageSourceService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
