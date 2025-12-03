using System.Text.Json;
using BlankBartender.WebApi.Configuration;
using BlankBartender.WebApi.Services.Interfaces;

namespace BlankBartender.WebApi.Services
{
    public class ConfigurationFileService : IConfigurationFileService
    {
        private readonly ILogger<ConfigurationFileService>? _logger;

        public ConfigurationFileService(ILogger<ConfigurationFileService>? logger = null)
        {
            _logger = logger;
        }

        public string GetFullPath(string fileName) => ConfigurationPaths.GetFullPath(fileName);

        public async Task<T?> ReadJsonAsync<T>(string fileName, CancellationToken cancellationToken = default)
        {
            var path = GetFullPath(fileName);
            if (!File.Exists(path))
                return default;

            try
            {
                var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json))
                    return default;
                return JsonSerializer.Deserialize<T>(json, JsonOptions.Default);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to read configuration file '{Path}'", path);
                return default;
            }
        }

        public async Task WriteJsonAsync<T>(string fileName, T data, CancellationToken cancellationToken = default)
        {
            var path = GetFullPath(fileName);
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(data, JsonOptions.Default);
                await File.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to write configuration file '{Path}'", path);
                throw;
            }
        }
    }
}