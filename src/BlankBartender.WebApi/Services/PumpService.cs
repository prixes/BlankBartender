using BlankBartender.Shared;
using Newtonsoft.Json.Linq;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public class PumpService : IPumpService
    {
        private const string PumpConfigFileName = "pump-config.json";
        private readonly string _pumpConfigFilePath;
        public IEnumerable<Pump> pumps;

        public PumpService(IHostEnvironment env)
        {
            _pumpConfigFilePath = Path.Combine(env.ContentRootPath, "Configuration", PumpConfigFileName);
            if (!File.Exists(_pumpConfigFilePath))
                throw new Exception($"pump-config.json not found in root");

            var pumpConfigJson = File.ReadAllText(_pumpConfigFilePath);
            if (string.IsNullOrEmpty(pumpConfigJson))
                throw new Exception($"pump-config.json empty or corrupted");

            var pumpJsonObject = JObject.Parse(pumpConfigJson);
            pumps = pumpJsonObject["pumps"].Select(p => new Pump
            {
                Number = int.Parse(p["number"].ToString()),
                Pin = short.Parse(p["pin"].ToString()),
                FlowRate = decimal.Parse(p["flowRate"].ToString()),
                Value = p["value"].ToString()
            }).ToList();
        }

        public IEnumerable<Pump> GetConfiguration()
        {
            return pumps;
        }
    }
}
