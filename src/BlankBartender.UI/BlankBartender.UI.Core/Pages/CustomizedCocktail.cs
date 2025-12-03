using BlankBartender.Shared;
using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace BlankBartender.UI.Core.Pages
{
    public partial class CustomizedCocktail : ComponentBase
    {
        [Parameter]
        public required string Model { get; set; }
        [Inject]
        public IDrinkService Service { get; set; } = default!;
        [Inject]
        public IStatusService StatusService { get; set; } = default!;
        public required Drink Drink { get; set; }
        public required Dictionary<string, decimal> OriginalIngredients { get; set; }

        public string SliderFormat { get; set; }

        public string imageSrc = "/images/cocktail.png";
        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] private IImageSourceService ImageService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            var modelJson = WebUtility.UrlDecode(Model);
            Drink = JsonSerializer.Deserialize<Drink>(modelJson);
            OriginalIngredients = new Dictionary<string, decimal>(Drink.Ingredients);
            await StatusService.StartHub();
            StatusService.OnChange += OnChangeHandler;

            imageSrc = await ImageService.GetCocktailImageAsync(Drink.Id);
        }

        protected async Task ProcessCustomDrink(Drink drink)
        {
            drink.IsProcessing = true;
            await Service.ProcessCustomDrink(drink);
            drink.IsProcessing = false;
        }

        private async void OnChangeHandler()
        {
            await InvokeAsync(StateHasChanged);
        }

        private void ResetValues()
        {
            foreach (var key in OriginalIngredients.Keys)
            {
                Drink.Ingredients[key] = OriginalIngredients[key];
            }
        }
    }
}
