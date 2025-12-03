using BlankBartender.Shared;
using BlankBartender.UI.Core.Http;
using BlankBartender.UI.Core.Interfaces;

namespace BlankBartender.UI.Core.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationApi _api;

    // Simple in-memory caches to reduce network traffic on the client
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private IEnumerable<string>? _liquidsCache;
    private DateTime _liquidsCacheTime;

    private IEnumerable<Pump>? _pumpsCache;
    private DateTime _pumpsCacheTime;

    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

    public ConfigurationService(IConfigurationApi api) => _api = api;

    public async Task<bool> StartPumps(CancellationToken cancellationToken)
    {
        await _api.StartAllPumpsAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> StartPump(int pumpNumber, CancellationToken cancellationToken)
    {
        await _api.StartPumpAsync(pumpNumber, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> StopPumps(CancellationToken cancellationToken)
    {
        await _api.StopAllPumpsAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> StopPump(int pumpNumber, CancellationToken cancellationToken)
    {
        await _api.StopPumpAsync(pumpNumber, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> InitializeLiquidFlow(CancellationToken cancellationToken)
    {
        await _api.InitializeLiquidFlowAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<string>> GetAllLiquids(CancellationToken cancellationToken)
    {
        if (_liquidsCache != null && (DateTime.UtcNow - _liquidsCacheTime) < _cacheDuration)
            return _liquidsCache;

        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_liquidsCache != null && (DateTime.UtcNow - _liquidsCacheTime) < _cacheDuration)
                return _liquidsCache;

            var liquids = await _api.GetAllLiquidsAsync(cancellationToken).ConfigureAwait(false);
            _liquidsCache = liquids.ToList();
            _liquidsCacheTime = DateTime.UtcNow;
            return _liquidsCache;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<IEnumerable<string>> GetReadAvailableLiquids(CancellationToken cancellationToken)
    {
        var pumps = await GetPumpConfiguration(cancellationToken).ConfigureAwait(false);
        return pumps.Select(p => p.Value).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().OrderBy(v => v).ToList();
    }

    public async Task AddLiquid(string liquidName, CancellationToken cancellationToken)
    {
        await _api.AddLiquidAsync(liquidName, cancellationToken).ConfigureAwait(false);

        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _liquidsCache = null;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task RemoveLiquid(string liquidName, CancellationToken cancellationToken)
    {
        await _api.RemoveLiquidAsync(liquidName, cancellationToken).ConfigureAwait(false);

        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _liquidsCache = null;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<IEnumerable<Pump>> GetPumpConfiguration(CancellationToken cancellationToken)
    {
        if (_pumpsCache != null && (DateTime.UtcNow - _pumpsCacheTime) < _cacheDuration)
            return _pumpsCache;

        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_pumpsCache != null && (DateTime.UtcNow - _pumpsCacheTime) < _cacheDuration)
                return _pumpsCache;

            var pumps = await _api.GetPumpsAsync(cancellationToken).ConfigureAwait(false);
            _pumpsCache = pumps.ToList();
            _pumpsCacheTime = DateTime.UtcNow;
            return _pumpsCache;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<bool> PumpLiquidChange(int pumpNumber, string liquid, CancellationToken cancellationToken)
    {
        await _api.ChangePumpLiquidAsync(pumpNumber, liquid, cancellationToken).ConfigureAwait(false);

        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _pumpsCache = null;
        }
        finally
        {
            _cacheLock.Release();
        }

        return true;
    }

    public async Task<(bool, bool, bool)> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await _api.GetSettingsAsync(cancellationToken).ConfigureAwait(false);
        return (settings.UseCameraAI, settings.UseStirrer, settings.UseIceDispenser);
    }

    public async Task<bool> SetSettings(bool UseCameraAI, bool UseStirrer, bool UseIceDispenser, CancellationToken cancellationToken)
    {
        await _api.SetSettingsAsync(UseCameraAI, UseStirrer, UseIceDispenser, cancellationToken).ConfigureAwait(false);
        return true;
    }

    Task<bool> IConfigurationService.StartPumps() => StartPumps(CancellationToken.None);
    Task<bool> IConfigurationService.StartPump(int pumpNumber) => StartPump(pumpNumber, CancellationToken.None);
    Task<bool> IConfigurationService.StopPumps() => StopPumps(CancellationToken.None);
    Task<bool> IConfigurationService.StopPump(int pumpNumber) => StopPump(pumpNumber, CancellationToken.None);
    Task<bool> IConfigurationService.InitializeLiquidFlow() => InitializeLiquidFlow(CancellationToken.None);
    Task<IEnumerable<string>> IConfigurationService.GetAllLiquids() => GetAllLiquids(CancellationToken.None);
    Task<IEnumerable<string>> IConfigurationService.GetReadAvailableLiquids() => GetReadAvailableLiquids(CancellationToken.None);
    Task IConfigurationService.AddLiquid(string liquidName) => AddLiquid(liquidName, CancellationToken.None);
    Task IConfigurationService.RemoveLiquid(string liquidName) => RemoveLiquid(liquidName, CancellationToken.None);
    Task<IEnumerable<Pump>> IConfigurationService.GetPumpConfiguration() => GetPumpConfiguration(CancellationToken.None);
    Task<bool> IConfigurationService.PumpLiquidChange(int pumpNumber, string liquid) => PumpLiquidChange(pumpNumber, liquid, CancellationToken.None);
    Task<(bool, bool, bool)> IConfigurationService.GetSettings() => GetSettings(CancellationToken.None);
    Task<bool> IConfigurationService.SetSettings(bool UseCameraAI, bool UseStirrer, bool UseIceDispenser) => SetSettings(UseCameraAI, UseStirrer, UseIceDispenser, CancellationToken.None);
}
