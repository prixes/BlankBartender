using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BlankBartender.Shared;

namespace BlankBartender.UI.Core.Http;

public class HttpConfigurationApi : IConfigurationApi
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public HttpConfigurationApi(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    private HttpClient Client => _client;

    private async Task<T> GetWrappedDataAsync<T>(string url, string key, CancellationToken ct)
    {
        using var resp = await Client.GetAsync(url, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);

        // Try exact match first
        if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty(key, out var element))
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText(), _jsonOptions)!;
            }

            // case-insensitive lookup
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return JsonSerializer.Deserialize<T>(prop.Value.GetRawText(), _jsonOptions)!;
                }
            }

            // also support lowercased key
            var lowerKey = key.ToLowerInvariant();
            if (doc.RootElement.TryGetProperty(lowerKey, out var lowerElement))
            {
                return JsonSerializer.Deserialize<T>(lowerElement.GetRawText(), _jsonOptions)!;
            }

            throw new KeyNotFoundException($"Key '{key}' not found in response from '{url}'");
        }

        // If response is directly the data (array/object), try deserialize entire root
        try
        {
            return JsonSerializer.Deserialize<T>(doc.RootElement.GetRawText(), _jsonOptions)!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Unable to parse response from '{url}'", ex);
        }
    }

    public Task<IEnumerable<string>> GetAllLiquidsAsync(CancellationToken cancellationToken = default) =>
        GetWrappedDataAsync<IEnumerable<string>>("configuration/liquids", "Liquids", cancellationToken);

    public async Task AddLiquidAsync(string liquidName, CancellationToken cancellationToken = default)
    {
        var url = $"configuration/liquids/add?removeLiquid={Uri.EscapeDataString(liquidName ?? string.Empty)}";
        using var resp = await Client.PostAsync(url, null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task RemoveLiquidAsync(string liquidName, CancellationToken cancellationToken = default)
    {
        var url = $"configuration/liquids/remove?removeLiquid={Uri.EscapeDataString(liquidName ?? string.Empty)}";
        using var resp = await Client.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public Task<IEnumerable<string>> GetAvailableLiquidsAsync(CancellationToken cancellationToken = default) =>
        GetWrappedDataAsync<IEnumerable<string>>("configuration/liquids/available", "Liquids", cancellationToken);

    public Task<IEnumerable<Pump>> GetPumpsAsync(CancellationToken cancellationToken = default) =>
        GetWrappedDataAsync<IEnumerable<Pump>>("configuration/pump", "Pumps", cancellationToken);

    public async Task ChangePumpLiquidAsync(int pumpNumber, string liquid, CancellationToken cancellationToken = default)
    {
        var url = $"configuration/pump/change?pumpNumber={pumpNumber}&liquid={Uri.EscapeDataString(liquid ?? string.Empty)}";
        using var resp = await Client.PutAsync(url, null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task StartAllPumpsAsync(CancellationToken cancellationToken = default)
    {
        using var resp = await Client.PostAsync("configuration/pumps/all/start", null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task StopAllPumpsAsync(CancellationToken cancellationToken = default)
    {
        using var resp = await Client.PostAsync("configuration/pumps/all/stop", null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task StartPumpAsync(int pumpNumber, CancellationToken cancellationToken = default)
    {
        using var resp = await Client.PostAsync($"configuration/pump/{pumpNumber}/start", null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task StopPumpAsync(int pumpNumber, CancellationToken cancellationToken = default)
    {
        using var resp = await Client.PostAsync($"configuration/pump/{pumpNumber}/stop", null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task InitializeLiquidFlowAsync(CancellationToken cancellationToken = default)
    {
        using var resp = await Client.PostAsync("configuration/initialize", null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IConfigurationApi.SettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        using var resp = await Client.GetAsync("configuration/settings", cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var dto = await JsonSerializer.DeserializeAsync<IConfigurationApi.SettingsDto>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
        return dto ?? new IConfigurationApi.SettingsDto();
    }

    public async Task SetSettingsAsync(bool useCameraAI, bool useStirrer, bool useIceDispenser, CancellationToken cancellationToken = default)
    {
        var url = $"configuration/settings?useCameraAI={useCameraAI.ToString().ToLowerInvariant()}&useStitter={useStirrer.ToString().ToLowerInvariant()}&useIceDispenser={useIceDispenser.ToString().ToLowerInvariant()}";
        using var resp = await Client.PutAsync(url, null, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
    }
}
