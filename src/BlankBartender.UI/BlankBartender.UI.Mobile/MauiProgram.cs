using BlankBartender.UI.Core.Interfaces;
using BlankBartender.UI.Core.Services;
using BlankBartender.UI.Mobile.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Reflection;

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
        //HttpClientHandler insecureHandler = GetInsecureHandler();
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

        HttpClient httpClient = new HttpClient(handler);
        builder.Services.AddScoped<IDrinkClient>(_ => new DrinkClient(builder.Configuration["ApiUrl"], httpClient));
        builder.Services.AddScoped<IConfigurationClient>(_ => new ConfigurationClient(builder.Configuration["ApiUrl"], httpClient));
        builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
        builder.Services.AddScoped<IDrinkService, DrinkService>();
        builder.Services.AddScoped<IPlatformService, PlatformService>();
        builder.Services.AddScoped<IStatusService, StatusService>();

        var app = builder.Build();

        Services = app.Services;

        return app;
    }
    public static IServiceProvider Services { get; private set; }

    //public static HttpClientHandler GetInsecureHandler()
    //{
    //    HttpClientHandler handler = new HttpClientHandler();
    //    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    //    {
    //        if (cert.Issuer.Equals("CN=192.168.101.254"))
    //            return true;
    //        return errors == System.Net.Security.SslPolicyErrors.None;
    //    };
    //    return handler;
    //}

}
