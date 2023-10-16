using BlankBartender.Shared;

namespace BlankBartender.UI.Core.Interfaces
{
    public interface IDrinkService
    {
        public Task<IEnumerable<Drink>> GetAll();
        public Task<IEnumerable<Drink>> GetAvailableAll();
        public Task<bool> ProcessCustomDrink(Drink drink);
        public Task<bool> Process(int id);
        public Task<bool> StartPumps();
        public Task<bool> StopPumps();
        public Task<bool> InitializeLiquidFlow();
        public Task<IEnumerable<string>> GetAllPumpLiquids();

        public Task<IEnumerable<Pump>> GetPumpConfiguration();

        public Task<bool> PumpLiquidChange(int pumpNumber, string liquid);

	}
}
