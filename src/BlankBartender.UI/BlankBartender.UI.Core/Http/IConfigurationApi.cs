using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlankBartender.Shared;

namespace BlankBartender.UI.Core.Http;

public interface IConfigurationApi
{
    Task<IEnumerable<string>> GetAllLiquidsAsync(CancellationToken cancellationToken = default);
    Task AddLiquidAsync(string liquidName, CancellationToken cancellationToken = default);
    Task RemoveLiquidAsync(string liquidName, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetAvailableLiquidsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Pump>> GetPumpsAsync(CancellationToken cancellationToken = default);
    Task ChangePumpLiquidAsync(int pumpNumber, string liquid, CancellationToken cancellationToken = default);

    Task StartAllPumpsAsync(CancellationToken cancellationToken = default);
    Task StopAllPumpsAsync(CancellationToken cancellationToken = default);
    Task StartPumpAsync(int pumpNumber, CancellationToken cancellationToken = default);
    Task StopPumpAsync(int pumpNumber, CancellationToken cancellationToken = default);

    Task InitializeLiquidFlowAsync(CancellationToken cancellationToken = default);

    Task<SettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task SetSettingsAsync(bool useCameraAI, bool useStirrer, bool useIceDispenser, CancellationToken cancellationToken = default);

    public sealed class SettingsDto
    {
        public bool UseCameraAI { get; set; }
        public bool UseStirrer { get; set; }
        public bool UseIceDispenser { get; set; }
    }
}
