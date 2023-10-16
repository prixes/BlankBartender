using BlankBartender.Shared;
using BlankBartender.UI.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Text.Json;

namespace BlankBartender.UI.Core.Pages
{
    public partial class CustomizedCocktail
    {
        [Parameter]
        public string model { get; set; }
        [Inject]
        public IDrinkService _service { get; set; } = default!;
        [Inject]
        public IStatusService _statusService { get; set; } = default!;
        private Drink drink { get; set; }
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            var modelJson = WebUtility.UrlDecode(model);
            drink = JsonSerializer.Deserialize<Drink>(modelJson);
            await _statusService.StartHub();
            _statusService.OnChange += OnChangeHandler;
        }

        protected async Task ProcessCustomDrink(Drink drink)
        {
            drink.IsProcessing = true;
            await _service.ProcessCustomDrink(drink);
            drink.IsProcessing = false;
        }

        private void UpdateValue(Tuple<string, float> ingredient)
        {
            drink.Ingradients[ingredient.Item1] = ingredient.Item2;
        }
        private async void OnChangeHandler()
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}
