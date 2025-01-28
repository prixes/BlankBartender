using BlankBartender.UI.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlankBartender.UI.Core.Services
{
    public class MobileImageSourceService : IImageSourceService
    {
        private readonly IConfiguration _config;
        public MobileImageSourceService(IConfiguration config) => _config = config;

        public async Task<string> GetCocktailImageAsync(int id)
        {
            try
            {
                using var httpClient = CreateHttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(GetImageUrl(id));
                return ConvertToBase64(imageBytes);
            }
            catch
            {
                return GetFallbackImage();
            }
        }

        private HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            return new HttpClient(handler);
        }

        private string GetImageUrl(int id) => $"{_config["ApiUrl"]}/images/cocktails/{id}.png";
        private string ConvertToBase64(byte[] bytes) => $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        private string GetFallbackImage() => "images/cocktail.png";
    }
}
