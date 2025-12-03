using BlankBartender.WebApi.Configuration;
using BlankBartender.WebApi.Services.Interfaces;
using BlankBartender.WebApi.Repositories.Interfaces;

namespace BlankBartender.WebApi.Repositories;

public class LiquidsRepository : ILiquidsRepository
{
    private readonly IConfigurationFileService _fileService;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<LiquidsRepository>? _logger;

    public LiquidsRepository(IConfigurationFileService fileService, ILogger<LiquidsRepository>? logger = null)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<List<string>> GetAllAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var liquids = await _fileService.ReadJsonAsync<List<string>>(ConfigurationPaths.LiquidsFileName).ConfigureAwait(false) ?? [];
            return liquids.OrderBy(l => l).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task AddAsync(string liquidName)
    {
        if (string.IsNullOrWhiteSpace(liquidName))
            throw new ArgumentException("Liquid name must not be empty", nameof(liquidName));

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var liquids = await _fileService.ReadJsonAsync<List<string>>(ConfigurationPaths.LiquidsFileName).ConfigureAwait(false) ?? [];

            if (!liquids.Contains(liquidName))
            {
                liquids.Add(liquidName);
                await _fileService.WriteJsonAsync(ConfigurationPaths.LiquidsFileName, liquids).ConfigureAwait(false);
                _logger?.LogInformation("Added liquid '{Liquid}' to {Path}", liquidName, _fileService.GetFullPath(ConfigurationPaths.LiquidsFileName));
            }
            else
            {
                _logger?.LogDebug("Liquid '{Liquid}' already exists in {Path}", liquidName, _fileService.GetFullPath(ConfigurationPaths.LiquidsFileName));
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoveAsync(string liquidName)
    {
        if (string.IsNullOrWhiteSpace(liquidName))
            throw new ArgumentException("Liquid name must not be empty", nameof(liquidName));

        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var liquids = await _fileService.ReadJsonAsync<List<string>>(ConfigurationPaths.LiquidsFileName).ConfigureAwait(false) ?? [];

            var removed = liquids.Remove(liquidName);
            if (removed)
            {
                await _fileService.WriteJsonAsync(ConfigurationPaths.LiquidsFileName, liquids).ConfigureAwait(false);
                _logger?.LogInformation("Removed liquid '{Liquid}' from {Path}", liquidName, _fileService.GetFullPath(ConfigurationPaths.LiquidsFileName));
            }
            else
            {
                _logger?.LogDebug("Liquid '{Liquid}' not found in {Path}", liquidName, _fileService.GetFullPath(ConfigurationPaths.LiquidsFileName));
            }

            return removed;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}