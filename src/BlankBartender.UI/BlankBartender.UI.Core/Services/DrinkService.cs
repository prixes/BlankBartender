using BlankBartender.Shared;
using BlankBartender.UI.Core.Helpers;
using BlankBartender.UI.Core.Http;
using BlankBartender.UI.Core.Interfaces;

namespace BlankBartender.UI.Core.Services;

public class DrinkService : IDrinkService
{
    private readonly IDrinkClient _drinkClient;

    public DrinkService(IDrinkClient drinkClient)
    {
        _drinkClient = drinkClient;
    }

    public async Task<IEnumerable<Drink>> GetAll()
    {
        var response = await _drinkClient.GetDrinksAsync();
        await RequestHandler.ValidateResponseAsync(response);
        return await RequestHandler.ParseResponseJsonAsync<IEnumerable<Drink>>(response, "drinks");
    }

    public async Task<IEnumerable<Drink>> GetAvailableAll()
    {
        var response = await _drinkClient.GetAvailableDrinksAsync();
        await RequestHandler.ValidateResponseAsync(response);
        return await RequestHandler.ParseResponseJsonAsync<IEnumerable<Drink>>(response, "drinks");
    }

    public async Task<bool> ProcessDrinkId(int id)
    {
        var response = await _drinkClient.MakeCocktailAsync(id);
        await RequestHandler.ValidateResponseAsync(response);
        return true;
    }

    public async Task<bool> ProcessCustomDrink(Drink drink)
    {
        var response = await _drinkClient.MakeCustomCocktailAsync(drink);
        await RequestHandler.ValidateResponseAsync(response);
        return true;
    }

    public async Task<bool> SaveCocktail(Drink drink)
    {
        var response = await _drinkClient.AddCocktailAsync(drink);
        await RequestHandler.ValidateResponseAsync(response);
        return true;
    }

}
