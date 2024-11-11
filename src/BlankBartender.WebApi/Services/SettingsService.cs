using BlankBartender.WebApi.Configuration;
using BlankBartender.WebApi.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace BlankBartender.WebApi.Services
{
    public class SettingsService : ISettingsService
    {
        private const string _settingsFileName = "settings.json";

        private string _liquidConfigJson;

        private readonly string _settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", _settingsFileName);
        private readonly string liquidFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", "liquids-config.json");

        private SettingsValues _settinsValues;
        private string? _settingsJson;
        public SettingsService()
        {
            _settinsValues = new SettingsValues();
        }

        public SettingsValues GetMachineSettings()
        {            if (System.IO.File.Exists(_settingsPath))
            {
                _settingsJson = System.IO.File.ReadAllText(_settingsPath);
            }
            JObject settingsJsonObject = JObject.Parse(_settingsJson);

            _settinsValues.UseCameraAI = settingsJsonObject["useCameraAI"].Value<bool>();
            _settinsValues.UseStirrer = settingsJsonObject["useStirrer"].Value<bool>();

            return _settinsValues;
        }

        public async Task SetMachineSettings(bool useCameraAI, bool useStitter)
        {
            _settinsValues.UseCameraAI = useCameraAI;
            _settinsValues.UseStirrer = useStitter;
            ;
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(_settinsValues, serializeOptions);
            using StreamWriter file = new(_settingsPath);
            await file.WriteLineAsync(json);

        }

        public void AddLiquid(string newLiquid)
        {
            Console.WriteLine($"Adding new liquid: {newLiquid}");
            JArray liquidsArray;

            if (System.IO.File.Exists(liquidFilePath))
            {
                _liquidConfigJson = System.IO.File.ReadAllText(liquidFilePath);
                if (!string.IsNullOrEmpty(_liquidConfigJson))
                {
                    liquidsArray = JArray.Parse(_liquidConfigJson);
                }
                else
                {
                    liquidsArray = new JArray();
                }
            }
            else
            {
                liquidsArray = new JArray();
            }

            // Add new liquid if it doesn't already exist in the array
            if (!liquidsArray.Contains(newLiquid))
            {
                liquidsArray.Add(newLiquid);
                System.IO.File.WriteAllText(liquidFilePath, liquidsArray.ToString());
                Console.WriteLine("Liquid added successfully");
            }
            else
            {
                Console.WriteLine("Liquid already exists in the list");
            }
        }

        public void RemoveLiquid(string liquidToRemove)
        {
            Console.WriteLine($"Removing liquid: {liquidToRemove}");
            List<string> liquidsArray;

            if (System.IO.File.Exists(liquidFilePath))
            {
                _liquidConfigJson = System.IO.File.ReadAllText(liquidFilePath);
                if (!string.IsNullOrEmpty(_liquidConfigJson))
                {
                    liquidsArray = JArray.Parse(_liquidConfigJson).ToObject<List<string>>(); ;

                    // Remove liquid if it exists in the array
                    if (liquidsArray.Contains(liquidToRemove))
                    {
                        liquidsArray.Remove(liquidToRemove);
                        System.IO.File.WriteAllText(liquidFilePath, JArray.FromObject(liquidsArray).ToString());
                        Console.WriteLine("Liquid removed successfully");
                    }
                    else
                    {
                        Console.WriteLine("Liquid not found in the list");
                    }
                }
            }
            else
            {
                Console.WriteLine("Liquid configuration file does not exist");
            }
        }
    }
}
