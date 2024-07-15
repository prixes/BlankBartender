using BlankBartender.Shared;
using BlankBartender.WebApi.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace BlankBartender.WebApi.Services
{
    public class CocktailService : ICocktailService
    {
        private string _drinkConfigJson;
        private string _pumpConfigJson;

        private string drinkFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", "drink-config.json");
        private string pumpFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", "pump-config.json");

        private ICollection<Pump> _pumps;
        private ICollection<Drink> _drinks;

        public IEnumerable<Drink> GetAllCocktails()
        {
            Console.WriteLine($"Get all cocktails");
            if (System.IO.File.Exists(drinkFilePath))
            {
                _drinkConfigJson = System.IO.File.ReadAllText(drinkFilePath);
            }
            if (!string.IsNullOrEmpty(_drinkConfigJson))
            {
                JObject drinkJsonObject = JObject.Parse(_drinkConfigJson);

                _drinks = drinkJsonObject["drinks"].Select(d => new Drink
                {
                    Name = d["name"].ToString(),
                    Id = byte.Parse(d["id"].ToString()),
                    Type = byte.Parse(d["type"].ToString()),
                    Ingradients = d["ingredients"].Children<JObject>().Select(ing => new
                    {
                        Key = ing.Properties().First().Name,
                        Value = decimal.Parse(ing.Properties().First().Value.ToString()),
                    }).ToDictionary(k => k.Key, v => v.Value),
                    Garnishes = d["garnishes"].Values<string>().ToList()
                }).ToList();

                Console.WriteLine($"Success getting all cocktails");
                return _drinks;
            }
            return null;
        }

        public IEnumerable<Drink> GetAvaiableCocktails()
        {
            Console.WriteLine($"Get avaiable cocktails");
            if (System.IO.File.Exists(drinkFilePath))
            {
                _drinkConfigJson = System.IO.File.ReadAllText(drinkFilePath);
            }
            if (System.IO.File.Exists(pumpFilePath))
            {
                _pumpConfigJson = System.IO.File.ReadAllText(pumpFilePath);
            }
            if (!string.IsNullOrEmpty(_drinkConfigJson) && !string.IsNullOrEmpty(_pumpConfigJson))
            {
                JObject drinkJsonObject = JObject.Parse(_drinkConfigJson);
                JObject pumpJsonObject = JObject.Parse(_pumpConfigJson);

                _pumps = pumpJsonObject["pumps"].Select(p => new Pump
                {
                    Number = int.Parse(p["number"].ToString()),
                    Pin = short.Parse(p["pin"].ToString()),
                    Value = p["value"].ToString()
                }).ToList();

                _drinks = drinkJsonObject["drinks"].Select(d => new Drink
                {
                    Name = d["name"].ToString(),
                    Id = int.Parse(d["id"].ToString()),
                    Type = byte.Parse(d["type"].ToString()),
                    Ingradients = d["ingredients"].Children<JObject>().Select(ing => new
                    {
                        Key = ing.Properties().First().Name,
                        Value = decimal.Parse(ing.Properties().First().Value.ToString()),
                    }).ToDictionary(k => k.Key, v => v.Value),
                    Garnishes = d["garnishes"].Values<string>().ToList()
                }).ToList();

                var pumpAlcohols = _pumps.Select(x => x.Value).ToList();
                var availableDrinks = _drinks.Where(cocktail => cocktail.Ingradients.All(ingredient => pumpAlcohols.Contains(ingredient.Key)));
                Console.WriteLine($"Success getting avaiable cocktails");
                return availableDrinks;
            }
            return null;
        }

    }
}
