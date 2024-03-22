using BlankBartender.WebApi.Configuration;
using BlankBartender.WebApi.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace BlankBartender.WebApi.Services
{
    public class SettingsService : ISettingsService
    {
        private const string _settingsFileName = "settings.json";
        private readonly string _settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", _settingsFileName);
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
    }
}
