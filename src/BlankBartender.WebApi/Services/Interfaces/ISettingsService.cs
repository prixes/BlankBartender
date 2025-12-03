using BlankBartender.WebApi.Configuration;

namespace BlankBartender.WebApi.Services.Interfaces;

public interface ISettingsService
{
    Task<SettingsValues> GetMachineSettingsAsync();
    Task SetMachineSettingsAsync(bool useCameraAI, bool useStitter, bool useIceDispenser);
    Task AddLiquidAsync(string newLiquid);
    Task RemoveLiquidAsync(string liquidToRemove);
}
