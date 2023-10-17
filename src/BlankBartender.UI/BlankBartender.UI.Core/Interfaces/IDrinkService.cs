using BlankBartender.Shared;

namespace BlankBartender.UI.Core.Interfaces
{
    public interface IDrinkService
    {
        public Task<IEnumerable<Drink>> GetAll();
        public Task<IEnumerable<Drink>> GetAvailableAll();
        public Task<bool> ProcessCustomDrink(Drink drink);
        public Task<bool> ProcessDrinkId(int id);

    }
}
