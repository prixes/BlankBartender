using BlankBartender.UI.Core.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            using var streamReader = new StreamReader(response.Stream);
            var responseData = await streamReader.ReadToEndAsync();
            JObject jsonObject = JObject.Parse(responseData);
            return JsonConvert.DeserializeObject<T>(jsonObject[key].ToString());
        }
    }
}
