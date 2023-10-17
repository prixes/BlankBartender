using BlankBartender.Shared;
using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Text.Json;

namespace BlankBartender.UI.Core.Pages;

public partial class NonAlcoholicCocktails
{
    [Inject]
    public IDrinkService _drinkService { get; set; } = default!;
    [Inject]
    public IStatusService _statusService { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    private IEnumerable<Drink>? _drinks;
    private IEnumerable<Drink>? _filteredDrinks;
    private string _searchQuery = "";

    protected override void OnParametersSet()
    {
        _filteredDrinks = _drinks.Where(drink => drink.Type == 0)
            .Where(drink =>
            drink.Name.ToLower().Contains(_searchQuery.ToLower())
         || drink.Garnishes.Any(g => g.ToLower().Contains(_searchQuery.ToLower()))
         || drink.Ingradients.Any(i => i.Key.ToLower().Contains(_searchQuery.ToLower()))
        );
    }
    private void NavigateToCustomCocktailPage(Drink drink)
    {
        var modelJson = JsonSerializer.Serialize(drink);
        var encodedModel = WebUtility.UrlEncode(modelJson);
        var url = $"/custom/cocktail/{encodedModel}";
        NavigationManager.NavigateTo(url);
    }
    protected override async Task OnInitializedAsync()
    {
        _drinks = await _drinkService.GetAvailableAll();
        await _statusService.StartHub();
        _statusService.OnChange += OnChangeHandler;
    }

    protected async Task Process(Drink drink)
    {
        drink.IsProcessing = true;
        await _drinkService.ProcessDrinkId(drink.Id);
        drink.IsProcessing = false;
    }

    private async void OnChangeHandler()
    {
        await InvokeAsync(StateHasChanged);
    }
}
