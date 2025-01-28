using BlankBartender.UI.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlankBartender.UI.Core.Services
{
    public class WebImageSourceService : IImageSourceService
    {
        private readonly IConfiguration _config;
        public WebImageSourceService(IConfiguration config) => _config = config;

        public Task<string> GetCocktailImageAsync(int id)
        {
            var url = $"{_config["ApiUrl"]}/images/cocktails/{id}.png";
            return Task.FromResult(url);
        }
    }
}
