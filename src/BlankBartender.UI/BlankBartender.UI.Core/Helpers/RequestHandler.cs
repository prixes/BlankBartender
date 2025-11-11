using BlankBartender.UI.Core.Services;
using System.Text.Json;

namespace BlankBartender.UI.Core.Helpers
{
    public static class RequestHandler
    {
        public static async Task ValidateResponseAsync(FileResponse response)
        {
            var status = (int)response.StatusCode;
            if (status != 200)
            {
                using var streamReader = new StreamReader(response.Stream);
                var responseData = await streamReader.ReadToEndAsync();
                throw new ApiException($"The HTTP status code of the response was not expected ({status}).", status, responseData, response.Headers, null);
            }
        }

        public static async Task<T> ParseResponseJsonAsync<T>(FileResponse response, string key)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            // Read the response stream completely
            using var streamReader = new StreamReader(response.Stream);
            var responseData = await streamReader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(responseData))
                throw new InvalidOperationException("Response stream was empty.");

            using var jsonDoc = JsonDocument.Parse(responseData);
            if (!jsonDoc.RootElement.TryGetProperty(key, out JsonElement valueElement))
                throw new KeyNotFoundException($"Key '{key}' not found in JSON: {responseData}");

            // Important: configure to ignore case
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<T>(valueElement.GetRawText(), options);
            if (result == null)
                throw new InvalidOperationException($"Deserialization of key '{key}' returned null.");

            return result;
        }
    }
}
