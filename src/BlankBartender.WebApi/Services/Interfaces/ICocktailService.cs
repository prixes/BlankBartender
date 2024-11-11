using BlankBartender.Shared;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface ICocktailService
    {
        public IEnumerable<Drink> GetAllCocktails();
        public IEnumerable<Drink> GetAvaiableCocktails();
        public void AddCocktail(Drink newDrink);
    }
}
