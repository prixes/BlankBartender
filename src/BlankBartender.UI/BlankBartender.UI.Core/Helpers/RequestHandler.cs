using BlankBartender.UI.Core.Http;
using System.Text.Json;

namespace BlankBartender.UI.Core.Helpers
{
    public static class RequestHandler
    {
        private static readonly JsonSerializerOptions _defaultOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task ValidateResponseAsync(FileResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            var status = (int)response.StatusCode;
            if (status != 200)
            {
                // Read remaining content (if any) for diagnostic purposes
                using var streamReader = new StreamReader(response.Stream);
                var responseData = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                throw new ApiException($"The HTTP status code of the response was not expected ({status}).", status, responseData, response.Headers, null);
            }
        }

        public static async Task<T> ParseResponseJsonAsync<T>(FileResponse response, string key)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            // Parse directly from the response stream to avoid buffering the whole content in memory
            using var jsonDoc = await JsonDocument.ParseAsync(response.Stream, new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            }).ConfigureAwait(false);

            if (!jsonDoc.RootElement.TryGetProperty(key, out JsonElement valueElement))
                throw new KeyNotFoundException($"Key '{key}' not found in JSON response.");

            var result = JsonSerializer.Deserialize<T>(valueElement.GetRawText(), _defaultOptions);
            return result == null ? throw new InvalidOperationException($"Deserialization of key '{key}' returned null.") : result;
        }
    }
}
