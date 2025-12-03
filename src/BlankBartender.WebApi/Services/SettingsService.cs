using BlankBartender.WebApi.Configuration;
using BlankBartender.WebApi.Repositories.Interfaces;
using BlankBartender.WebApi.Services.Interfaces;

namespace BlankBartender.WebApi.Services;

public class SettingsService : ISettingsService
{
    private readonly SettingsValues _settingsValues = new();
    private volatile bool _settingsLoaded;

    private readonly SemaphoreSlim _settingsSemaphore = new(1, 1);

    private readonly IConfigurationFileService _fileService;
    private readonly ILiquidsRepository _liquidsRepository;
    private readonly ILogger<SettingsService>? _logger;

    public SettingsService(IConfigurationFileService fileService, ILiquidsRepository liquidsRepository, ILogger<SettingsService>? logger = null)
    {
        _fileService = fileService;
        _liquidsRepository = liquidsRepository;
        _logger = logger;
    }

    public async Task<SettingsValues> GetMachineSettingsAsync()
    {
        await _settingsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_settingsLoaded)
                return _settingsValues;

            var parsed = await _fileService.ReadJsonAsync<SettingsValues>(ConfigurationPaths.SettingsFileName).ConfigureAwait(false);
            if (parsed != null)
            {
                _settingsValues.UseCameraAI = parsed.UseCameraAI;
                _settingsValues.UseStirrer = parsed.UseStirrer;
                _settingsValues.UseIceDispenser = parsed.UseIceDispenser;
            }

            _settingsLoaded = true;
            return _settingsValues;
        }
        finally
        {
            _settingsSemaphore.Release();
        }
    }

    public async Task SetMachineSettingsAsync(bool useCameraAI, bool useStitter, bool useIceDispenser)
    {
        await _settingsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            _settingsValues.UseCameraAI = useCameraAI;
            _settingsValues.UseStirrer = useStitter;
            _settingsValues.UseIceDispenser = useIceDispenser;

            await _fileService.WriteJsonAsync(ConfigurationPaths.SettingsFileName, _settingsValues).ConfigureAwait(false);

            _settingsLoaded = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write settings file '{Path}'", _fileService.GetFullPath(ConfigurationPaths.SettingsFileName));
            throw;
        }
        finally
        {
            _settingsSemaphore.Release();
        }
    }

    public Task AddLiquidAsync(string newLiquid) => _liquidsRepository.AddAsync(newLiquid);

    public Task RemoveLiquidAsync(string liquidToRemove) => _liquidsRepository.RemoveAsync(liquidToRemove);
}
