using BlankBartender.WebApi.Repositories.Interfaces;
using BlankBartender.WebApi.Services.Interfaces;

namespace BlankBartender.WebApi.Services;

public class ConfigurationInitializationService : IHostedService
{
    private readonly ILiquidsRepository _liquidsRepository;
    private readonly IPumpsRepository _pumpsRepository;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ConfigurationInitializationService> _logger;

    public ConfigurationInitializationService(ILiquidsRepository liquidsRepository,
                                              IPumpsRepository pumpsRepository,
                                              ISettingsService settingsService,
                                              ILogger<ConfigurationInitializationService> logger)
    {
        _liquidsRepository = liquidsRepository;
        _pumpsRepository = pumpsRepository;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Preloading configuration data...");
            await _liquidsRepository.GetAllAsync();
            await _pumpsRepository.GetConfigurationAsync();
            await _settingsService.GetMachineSettingsAsync();
            _logger.LogInformation("Configuration preload completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preload configuration");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
