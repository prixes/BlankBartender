using BlankBartender.Shared;
using BlankBartender.UI.Core.Helpers;

namespace BlankBartender.UI.Core.Interfaces
{
    public interface IConfigurationService
    {

        public Task<bool> StartPumps();
        public Task<bool> StartPump(int pumpNumber);
        public Task<bool> StopPumps();
        public Task<bool> StopPump(int pumpNumber);
        public Task<bool> InitializeLiquidFlow();
        public Task<IEnumerable<string>> GetAllPumpLiquids();

        public Task<IEnumerable<Pump>> GetPumpConfiguration();

        public Task<bool> PumpLiquidChange(int pumpNumber, string liquid);

        public Task<(bool, bool)> GetSettings();
        public Task<bool> SetSettings(bool UseCameraAI, bool UseStirrer);
    }
}
