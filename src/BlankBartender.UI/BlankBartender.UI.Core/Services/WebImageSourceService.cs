using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlankBartender.UI.Core.Services;

public class WebImageSourceService : IImageSourceService
{
    private readonly string base_url;
    public WebImageSourceService(NavigationManager NavigationManager)  => base_url = $"http://{new Uri(NavigationManager.BaseUri).Host}:5000";
    public Task<string> GetCocktailImageAsync(int id)
    {
        var url = $"{base_url}/images/cocktails/{id}.png";
        return Task.FromResult(url);
    }
}
