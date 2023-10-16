using BlankBartender.Shared;
using BlankBartender.UI.Core.Interfaces;
using BlankBartender.UI.Core.Services;
using Microsoft.AspNetCore.Components;

namespace BlankBartender.UI.Core.Pages
{
    public partial class AllCocktails
    {
        [Inject]
        public IDrinkService _drinkService { get; set; } = default!;

        private IEnumerable<Drink>? _drinks;
        private IEnumerable<Drink>? _filteredDrinks;
        private string _searchQuery = "";

        protected override void OnParametersSet()
        {
            _filteredDrinks = _drinks.Where(drink => 
                drink.Name.ToLower().Contains(_searchQuery.ToLower()) 
             || drink.Garnishes.Any(g => g.ToLower().Contains(_searchQuery.ToLower()))
             || drink.Ingradients.Any(i => i.Key.ToLower().Contains(_searchQuery.ToLower()))
            );
        }

        protected override async Task OnInitializedAsync()
        {
            _drinks = await _drinkService.GetAll();
            _filteredDrinks = _drinks;
        }

        protected async Task Process(Drink drink)
        {
            drink.IsProcessing = true;
            await _drinkService.Process(drink.Id);
            drink.IsProcessing = false;
        }

        private async void OnChangeHandler()
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}

