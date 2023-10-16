
using BlankBartender.UI.Core.Services;

namespace BlankBartender.UI.Core.Helpers
{
    public static class RequestHandler
    {
        public static async Task<HttpResponseMessage> getResponse(HttpClient client, string url_, string? body)
        {
            var cancellationToken = new CancellationToken();
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    //(client_, request_, urlBuilder_);

                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    //PrepareRequest(client_, request_, url_);

                    var response_ = await client.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            //foreach (var item_ in response_.Content.Headers)
                            //headers_[item_.Key] = item_.Value;
                        }

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            return response_;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client.Dispose();
            }
        }
    }
}
