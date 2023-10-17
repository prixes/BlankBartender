using BlankBartender.Shared;
using BlankBartender.UI.Core.Helpers;
using BlankBartender.UI.Core.Interfaces;

namespace BlankBartender.UI.Core.Services
{
    public class DrinkService : IDrinkService
    {
        private readonly IDrinkClient _drinkClient;

        public DrinkService(IDrinkClient drinkClient)
        {
            _drinkClient = drinkClient;
        }

        public async Task<IEnumerable<Drink>> GetAll()
        {
            var response = await _drinkClient.GetDrinksAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return await RequestHandler.ParseResponseJsonAsync<IEnumerable<Drink>>(response, "drinks");
        }

        public async Task<IEnumerable<Drink>> GetAvailableAll()
        {
            var response = await _drinkClient.GetAvailableDrinksAsync();
            await RequestHandler.ValidateResponseAsync(response);
            return await RequestHandler.ParseResponseJsonAsync<IEnumerable<Drink>>(response, "drinks");
        }

        public async Task<bool> ProcessDrinkId(int id)
        {
            var response = await _drinkClient.MakeCocktailAsync(id);
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }

        public async Task<bool> ProcessCustomDrink(Drink drink)
        {
            var response = await _drinkClient.MakeCustomCocktailAsync(drink);
            await RequestHandler.ValidateResponseAsync(response);
            return true;
        }


        //      public async Task<IEnumerable<Drink>> GetAll()
        //{
        //	var cancellationToken = new CancellationToken();
        //	var urlBuilder_ = new System.Text.StringBuilder();
        //	urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/drink/all/");

        //	var response = await RequestHandler.getResponse(_httpClient, urlBuilder_.ToString(), null);
        //	var objectResponseString_ = await response.Content.ReadAsStringAsync();

        //	JObject objectResponseJObject = JObject.Parse(objectResponseString_);
        //	var drinksObjectResponse_ = JsonConvert.DeserializeObject<IEnumerable<Drink>>(objectResponseJObject["drinks"].ToString());

        //	var objectResponse_ = drinksObjectResponse_;

        //	if (objectResponse_ == null)
        //	{
        //		var headers_ = System.Linq.Enumerable.ToDictionary(response.Headers, h_ => h_.Key, h_ => h_.Value);
        //		var status_ = (int)response.StatusCode;
        //		throw new ApiException("Response was null which was not expected.", status_, objectResponseString_, headers_, null);
        //	}
        //	return objectResponse_;

        //}

        //      public async Task<IEnumerable<Drink>> GetAvailableAll()
        //      {
        //          var cancellationToken = new CancellationToken();
        //          var urlBuilder_ = new System.Text.StringBuilder();
        //          urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/drink/available/all/");

        //          var response = await RequestHandler.getResponse(_httpClient, urlBuilder_.ToString(), null);
        //          var objectResponseString_ = await response.Content.ReadAsStringAsync();

        //          JObject objectResponseJObject = JObject.Parse(objectResponseString_);
        //          var drinksObjectResponse_ = JsonConvert.DeserializeObject<IEnumerable<Drink>>(objectResponseJObject["drinks"].ToString());

        //          var objectResponse_ = drinksObjectResponse_;

        //          if (objectResponse_ == null)
        //          {
        //              var headers_ = System.Linq.Enumerable.ToDictionary(response.Headers, h_ => h_.Key, h_ => h_.Value);
        //              var status_ = (int)response.StatusCode;
        //              throw new ApiException("Response was null which was not expected.", status_, objectResponseString_, headers_, null);
        //          }
        //          return objectResponse_;

        //      }

        //      public async Task<bool> Process(int id)
        //{
        //	var cancellationToken = new CancellationToken();
        //	var urlBuilder_ = new System.Text.StringBuilder();
        //	urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/drink/make/cocktail/{id}");

        //	var client_ = _httpClient;
        //	var disposeClient_ = false;
        //	try
        //	{
        //		using (var request_ = new System.Net.Http.HttpRequestMessage())
        //		{
        //			request_.Method = new System.Net.Http.HttpMethod("GET");
        //			//request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

        //			var url_ = urlBuilder_.ToString();
        //			request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

        //			var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        //			var disposeResponse_ = true;
        //			try
        //			{
        //				var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
        //				if (response_.Content != null && response_.Content.Headers != null)
        //				{
        //					//foreach (var item_ in response_.Content.Headers)
        //					//headers_[item_.Key] = item_.Value;
        //				}

        //				var status_ = (int)response_.StatusCode;
        //				if (status_ == 200)
        //				{
        //					return await Task.FromResult(true);
        //				}
        //				else
        //				{
        //					var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
        //					throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
        //				}
        //			}
        //			finally
        //			{
        //				if (disposeResponse_)
        //					response_.Dispose();
        //			}
        //		}
        //	}
        //	finally
        //	{
        //		if (disposeClient_)
        //			client_.Dispose();
        //	}
        //}

        //public async Task<bool> ProcessCustomDrink(Drink drink)
        //{
        //	var cancellationToken = new CancellationToken();
        //	var urlBuilder_ = new System.Text.StringBuilder();
        //	urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/drink/make/cocktail/custom");

        //	var client_ = _httpClient;
        //	var disposeClient_ = false;
        //	try
        //	{
        //		using (var request_ = new System.Net.Http.HttpRequestMessage())
        //		{
        //			request_.Method = new System.Net.Http.HttpMethod("POST");
        //			request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

        //			var url_ = urlBuilder_.ToString();
        //			request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);
        //			var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(drink), Encoding.UTF8, "application/json");
        //			request_.Content = content;
        //			//PrepareRequest(client_, request_, url_);

        //			var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        //			var disposeResponse_ = true;
        //			try
        //			{
        //				var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
        //				if (response_.Content != null && response_.Content.Headers != null)
        //				{
        //					//foreach (var item_ in response_.Content.Headers)
        //					//headers_[item_.Key] = item_.Value;
        //				}

        //				var status_ = (int)response_.StatusCode;
        //				if (status_ == 200)
        //				{
        //					return await Task.FromResult(true);
        //				}
        //				else
        //				{
        //					var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
        //					throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
        //				}
        //			}
        //			finally
        //			{
        //				if (disposeResponse_)
        //					response_.Dispose();
        //			}
        //		}
        //	}
        //	finally
        //	{
        //		if (disposeClient_)
        //			client_.Dispose();
        //	}
        //}





        //      public async Task<bool> StartPumps()
        //{
        //	{
        //		var cancellationToken = new CancellationToken();
        //		var urlBuilder_ = new System.Text.StringBuilder();
        //		urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/configuration/pumps/all/start");

        //		var response = await RequestHandler.getResponse(_httpClient, urlBuilder_.ToString(), null);
        //		var objectResponseString_ = await response.Content.ReadAsStringAsync();

        //		return true;
        //	}
        //}
        //public async Task<bool> StopPumps()
        //{
        //	{
        //		var cancellationToken = new CancellationToken();
        //		var urlBuilder_ = new System.Text.StringBuilder();
        //		urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/configuration/pumps/all/stop");

        //		var response = await RequestHandler.getResponse(_httpClient, urlBuilder_.ToString(), null);
        //		var objectResponseString_ = await response.Content.ReadAsStringAsync();

        //		return true;
        //	}
        //}
        //public async Task<bool> InitializeLiquidFlow()
        //{
        //	{
        //		var cancellationToken = new CancellationToken();
        //		var urlBuilder_ = new System.Text.StringBuilder();
        //		urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/configuration/initialize");

        //		var response = await RequestHandler.getResponse(_httpClient, urlBuilder_.ToString(), null);
        //		var objectResponseString_ = await response.Content.ReadAsStringAsync();

        //		return true;
        //	}
        //}

        //public async Task<IEnumerable<string>> GetAllPumpLiquids()
        //{
        //	{
        //		var cancellationToken = new CancellationToken();
        //		var urlBuilder_ = new System.Text.StringBuilder();
        //		urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/configuration/liquids");

        //		var response = await RequestHandler.getResponse(_httpClient, urlBuilder_.ToString(), null);
        //		var objectResponseString_ = await response.Content.ReadAsStringAsync();

        //		JObject objectResponseJObject = JObject.Parse(objectResponseString_);
        //		var liquidsObjectResponse_ = JsonConvert.DeserializeObject<IEnumerable<string>>(objectResponseJObject["liquids"].ToString());

        //		return liquidsObjectResponse_;
        //	}
        //}

        //public async Task<IEnumerable<Pump>> GetPumpConfiguration()
        //{
        //	{
        //		var cancellationToken = new CancellationToken();
        //		var urlBuilder_ = new System.Text.StringBuilder();
        //		urlBuilder_.Append(_baseUrl != null ? _baseUrl.TrimEnd('/') : "").Append($"/configuration/pump");

        //		var response = await RequestHandler.getResponse(_httpClient, urlBuilder_.ToString(), null);
        //		var objectResponseString_ = await response.Content.ReadAsStringAsync();

        //		JObject objectResponseJObject = JObject.Parse(objectResponseString_);
        //		var liquidsObjectResponse_ = JsonConvert.DeserializeObject<IEnumerable<Pump>>(objectResponseJObject["pumps"].ToString());

        //		return liquidsObjectResponse_;
        //	}
        //}

        //      public async Task<bool> PumpLiquidChange(int pumpNumber, string liquid)
        //{
        //	var cancellationToken = new CancellationToken();
        //	var urlBuilder_ = new UriBuilder(_baseUrl != null ? $"{_baseUrl.TrimEnd('/')}/configuration/pump/change" : "");


        //	var query = HttpUtility.ParseQueryString(urlBuilder_.Query);
        //	query["pumpNumber"] = pumpNumber.ToString();
        //	query["liquid"] = liquid;
        //	urlBuilder_.Query = query.ToString();

        //	var client_ = _httpClient;
        //	var disposeClient_ = false;
        //	try
        //	{
        //		using (var request_ = new System.Net.Http.HttpRequestMessage())
        //		{
        //			request_.Method = new System.Net.Http.HttpMethod("POST");
        //			request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));


        //			var url_ = urlBuilder_.ToString();
        //			request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

        //			//PrepareRequest(client_, request_, url_);

        //			var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        //			var disposeResponse_ = true;
        //			try
        //			{
        //				var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
        //				if (response_.Content != null && response_.Content.Headers != null)
        //				{
        //					//foreach (var item_ in response_.Content.Headers)
        //					//headers_[item_.Key] = item_.Value;
        //				}

        //				var status_ = (int)response_.StatusCode;
        //				if (status_ == 200)
        //				{
        //					return await Task.FromResult(true);
        //				}
        //				else
        //				{
        //					var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
        //					throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
        //				}
        //			}
        //			finally
        //			{
        //				if (disposeResponse_)
        //					response_.Dispose();
        //			}
        //		}
        //	}
        //	finally
        //	{
        //		if (disposeClient_)
        //			client_.Dispose();
        //	}
        //}
    }
}
