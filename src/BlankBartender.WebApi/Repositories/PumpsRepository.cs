using BlankBartender.Shared;
using BlankBartender.WebApi.Configuration;
using BlankBartender.WebApi.Repositories.Interfaces;
using BlankBartender.WebApi.Services.Interfaces;

namespace BlankBartender.WebApi.Repositories;

public class PumpsRepository : IPumpsRepository
{
    private readonly IConfigurationFileService _fileService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<PumpsRepository>? _logger;

    public PumpsRepository(IConfigurationFileService fileService, ILogger<PumpsRepository>? logger = null)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<PumpsConfiguration> GetConfigurationAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var cfg = await _fileService.ReadJsonAsync<PumpsConfiguration>(ConfigurationPaths.PumpsFileName).ConfigureAwait(false)
                      ?? new PumpsConfiguration { Pumps = [] };
            return cfg;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task UpdateConfigurationAsync(PumpsConfiguration config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await _fileService.WriteJsonAsync(ConfigurationPaths.PumpsFileName, config).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdatePumpLiquidAsync(int pumpNumber, string liquid)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var cfg = await _fileService.ReadJsonAsync<PumpsConfiguration>(ConfigurationPaths.PumpsFileName).ConfigureAwait(false)
                      ?? new PumpsConfiguration { Pumps = new List<Pump>() };

            var pump = cfg.Pumps.FirstOrDefault(p => p.Number == pumpNumber);
            if (pump == null)
            {
                _logger?.LogDebug("Pump {Pump} not found in {Path}", pumpNumber, _fileService.GetFullPath(ConfigurationPaths.PumpsFileName));
                return false;
            }

            pump.Value = liquid;
            await _fileService.WriteJsonAsync(ConfigurationPaths.PumpsFileName, cfg).ConfigureAwait(false);
            _logger?.LogInformation("Updated pump {Pump} liquid to '{Liquid}'", pumpNumber, liquid);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<string>> GetAvailableLiquidsAsync()
    {
        var cfg = await GetConfigurationAsync().ConfigureAwait(false);
        return cfg.Pumps
                  .Select(p => p.Value)
                  .Where(v => !string.IsNullOrWhiteSpace(v))
                  .Distinct()
                  .OrderBy(v => v)
                  .ToList();
    }
}
