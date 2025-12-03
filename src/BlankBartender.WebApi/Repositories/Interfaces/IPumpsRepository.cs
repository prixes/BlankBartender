using BlankBartender.Shared;

namespace BlankBartender.WebApi.Repositories.Interfaces;

public interface IPumpsRepository
{
    Task<PumpsConfiguration> GetConfigurationAsync();
    Task UpdateConfigurationAsync(PumpsConfiguration config);
    Task<bool> UpdatePumpLiquidAsync(int pumpNumber, string liquid);
    Task<IEnumerable<string>> GetAvailableLiquidsAsync();
}
