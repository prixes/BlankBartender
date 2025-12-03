using BlankBartender.UI.Core.Interfaces;
using BlankBartender.UI.Core.Services;
using BlankBartender.UI.Mobile.Services;
using Microsoft.Extensions.Configuration;
using MudBlazor.Services;
using System.Reflection;
using IImageSourceService = BlankBartender.UI.Core.Interfaces.IImageSourceService;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using BlankBartender.UI.Core.Http;

namespace BlankBartender.UI.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });


        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream("BlankBartender.UI.Mobile.appsettings.json");
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        builder.Configuration.AddConfiguration(config);

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        // Configure a named HttpClient for API calls; include insecure handler for device if necessary
        var apiUrl = builder.Configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl not configured");
        builder.Services.AddHttpClient("api", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

        // Register generated clients using factory registrations (constructor expects baseUrl + HttpClient)
        builder.Services.AddScoped<IDrinkClient>(sp => new DrinkClient(apiUrl, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
        builder.Services.AddScoped<ILiquidsClient>(sp => new LiquidsClient(apiUrl, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
        builder.Services.AddScoped<IPumpsClient>(sp => new PumpsClient(apiUrl, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
        builder.Services.AddScoped<ISettingsClient>(sp => new SettingsClient(apiUrl, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));
        builder.Services.AddScoped<ISystemClient>(sp => new SystemClient(apiUrl, sp.GetRequiredService<IHttpClientFactory>().CreateClient("api")));

        // register our configuration API adapter and high-level services
        builder.Services.AddHttpClient<BlankBartender.UI.Core.Http.IConfigurationApi, BlankBartender.UI.Core.Http.HttpConfigurationApi>(client => client.BaseAddress = new Uri(apiUrl));

        builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
        builder.Services.AddScoped<IDrinkService, DrinkService>();
        builder.Services.AddScoped<IPlatformService, PlatformService>();
        builder.Services.AddScoped<IStatusService, StatusService>();
        builder.Services.AddSingleton<IImageSourceService, MobileImageSourceService>();
        builder.Services.AddMudServices();


        var app = builder.Build();

        Services = app.Services;

        return app;
    }
    public static IServiceProvider Services { get; private set; }

}
