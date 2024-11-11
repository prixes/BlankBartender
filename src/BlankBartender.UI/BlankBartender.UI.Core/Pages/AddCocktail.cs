using BlankBartender.Shared;
using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Linq;

namespace BlankBartender.UI.Core.Pages
{
    public partial class AddCocktail : ComponentBase
    {
        [Parameter]
        public string model { get; set; }
        [Inject]
        public IDrinkService _drinkService { get; set; } = default!;
        [Inject]
        public IStatusService _statusService { get; set; } = default!;
        [Inject]
        public IConfigurationService _configurationService { get; set; } = default!;
        private Drink drink { get; set; }
        public List<string> pumpLiquids { get; set; }
        public List<string> allLiquids { get; set; }
        public bool useAvailableLiquids { get; set; } = true;
        public IEnumerable<string> liquids
        {
            get
            {
                return useAvailableLiquids ? pumpLiquids : allLiquids;
            }
        }
        private bool openPopup;

        private void ToggleOpen() => openPopup = !openPopup;

        public string currentLiquid = string.Empty;

        public string SliderFormat { get; set; }
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            drink = new Drink();
            _statusService.OnChange += OnChangeHandler;
            allLiquids = (List<string>)await _configurationService.GetAllLiquids();
            pumpLiquids = (List<string>)await _configurationService.GetReadAvailableLiquids();

            await _statusService.StartHub();
        }
        private void AddIngredient()
        {
            drink.Ingredients.Add(currentLiquid, 50);
            if (allLiquids.Contains(currentLiquid))
            {
                allLiquids.Remove(currentLiquid);
            }
            if (pumpLiquids.Contains(currentLiquid))
            {
                pumpLiquids.Remove(currentLiquid);
            }
            currentLiquid = null;
        }

        public void RemoveIngredient(string ingredient)
        {
            allLiquids.ToList().Add(ingredient);
            if (useAvailableLiquids)
            {
                pumpLiquids.ToList().Add(ingredient);
            }
            drink.Ingredients.Remove(ingredient);
        }

        public async Task OnLiquidPoolChange()
        {
            if (useAvailableLiquids)
            {
                pumpLiquids = (List<string>)await _configurationService.GetReadAvailableLiquids();
                foreach (var ingredient in drink.Ingredients)
                {
                    if (pumpLiquids.Contains(ingredient.Key))
                    {
                        pumpLiquids.Remove(ingredient.Key);
                    }
                    else
                    {
                        allLiquids.ToList().Add(ingredient.Key);
                        drink.Ingredients.Remove(ingredient.Key);
                    }
                }
            }

        }

        private Task<IEnumerable<string>> SearchItems(string searchText)
        {
            var result = liquids.Where(x => x.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result.AsEnumerable());
        }

        protected async Task SaveCocktail(Drink drink)
        {
            drink.IsProcessing = true;
            await _drinkService.SaveCocktail(drink);
            drink.IsProcessing = false;
            openPopup = false;
        }

        protected async Task ProcessCustomDrink(Drink drink)
        {
            drink.Name = "InstaMix";
            drink.IsProcessing = true;
            await _drinkService.ProcessCustomDrink(drink);
            drink.IsProcessing = false;
        }

        private async void OnChangeHandler()
        {
            await InvokeAsync(StateHasChanged);
        }

    }
}
