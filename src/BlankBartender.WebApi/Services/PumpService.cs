using BlankBartender.Shared;
using BlankBartender.WebApi.Controllers;
using Newtonsoft.Json.Linq;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public class PumpService : IPumpService
    {
        private const string _pumpConfigFileName = "pump-config.json";
        public IEnumerable<Pump> pumps;

        private readonly string _pumpsFilePath;
        private string? _pumpsConfigJson;

        public PumpService()
        {
            _pumpsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", _pumpConfigFileName);

            if (System.IO.File.Exists(_pumpsFilePath))
            {
                _pumpsConfigJson = System.IO.File.ReadAllText(_pumpsFilePath);
            }
            JObject pumpJsonObject = JObject.Parse(_pumpsConfigJson);
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
            if (System.IO.File.Exists(_pumpsFilePath))
            {
                _pumpsConfigJson = System.IO.File.ReadAllText(_pumpsFilePath);
            }
            JObject pumpJsonObject = JObject.Parse(_pumpsConfigJson);

            pumps = pumpJsonObject["pumps"].Select(p => new Pump
            {
                Number = int.Parse(p["number"].ToString()),
                Pin = short.Parse(p["pin"].ToString()),
                FlowRate = decimal.Parse(p["flowRate"].ToString()),
                Value = p["value"].ToString()
            }).ToList();
            return pumps;
        }
    }
}
