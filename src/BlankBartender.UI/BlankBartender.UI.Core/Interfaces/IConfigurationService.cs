using BlankBartender.Shared;

namespace BlankBartender.UI.Core.Interfaces
{
    public interface IConfigurationService
    {

        public Task<bool> StartPumps();
        public Task<bool> StopPumps();
        public Task<bool> InitializeLiquidFlow();
        public Task<IEnumerable<string>> GetAllPumpLiquids();

        public Task<IEnumerable<Pump>> GetPumpConfiguration();

        public Task<bool> PumpLiquidChange(int pumpNumber, string liquid);
    }
}
