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
        public string model { get; set; }
        [Inject]
        public IDrinkService _service { get; set; } = default!;
        [Inject]
        public IStatusService _statusService { get; set; } = default!;
        private Drink drink { get; set; }
        private Dictionary<string, decimal> originalIngredients { get; set; }

        public string SliderFormat { get; set; }

        private string imageSrc = "/images/cocktail.png";
        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] private IImageSourceService ImageService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            var modelJson = WebUtility.UrlDecode(model);
            drink = JsonSerializer.Deserialize<Drink>(modelJson);
            originalIngredients = new Dictionary<string, decimal>(drink.Ingredients);
            await _statusService.StartHub();
            _statusService.OnChange += OnChangeHandler;

            imageSrc = await ImageService.GetCocktailImageAsync(drink.Id);
        }

        protected async Task ProcessCustomDrink(Drink drink)
        {
            drink.IsProcessing = true;
            await _service.ProcessCustomDrink(drink);
            drink.IsProcessing = false;
        }

        private async void OnChangeHandler()
        {
            await InvokeAsync(StateHasChanged);
        }

        private void ResetValues()
        {
            foreach (var key in originalIngredients.Keys)
            {
                drink.Ingredients[key] = originalIngredients[key];
            }
        }

        private void FallbackImage()
        {
            imageSrc = "/images/cocktail.png";
        }
    }
}
