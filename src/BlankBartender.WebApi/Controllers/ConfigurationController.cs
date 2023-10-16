using BlankBartender.Shared;
using BlankBartender.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BlankBartender.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private const string _liquidsConfigFileName = "liquids-config.json";
        private const string _pumpConfigFileName = "pump-config.json";

        private readonly string _liquidsFilePath;
        private readonly string _pumpsFilePath;

        private IEnumerable<string> liquids;
        private readonly ILogger<DrinkController> _logger;
        private string _pumpsConfigJson;

        private PumpsConfiguration _pumpsConfiguration;

        private readonly IDisplayService _displayService;
        private readonly IPinService _pinService;
        private readonly IStatusService _statusService;
        public ConfigurationController(ILogger<DrinkController> logger, IPinService pinService, IDisplayService displayService, IStatusService statusService)
        {
            _logger = logger;
            _pinService = pinService;
            _pumpsConfiguration = new PumpsConfiguration();
            _displayService = displayService;
            _statusService = statusService;

            _liquidsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration", _liquidsConfigFileName);
            _pumpsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration", _pumpConfigFileName);
            ReadCurrentLiquids();
            ReadCurrentPumpConfiguration();

        }

        [Route("liquids")]
        public async Task<ActionResult> ReadCurrentLiquids()
        {
            if (System.IO.File.Exists(_liquidsFilePath))
            {
                var liquidsConfigJson = System.IO.File.ReadAllText(_liquidsFilePath);

                liquids = JsonConvert.DeserializeObject<List<string>>(liquidsConfigJson);
            }
            //TODO give propper exception
            return new JsonResult(new
            {
                Liquids = liquids
            });
        }

        [Route("pump")]
        public async Task<ActionResult> ReadCurrentPumpConfiguration()
        {
            if (System.IO.File.Exists(_pumpsFilePath))
            {
                _pumpsConfigJson = System.IO.File.ReadAllText(_pumpsFilePath);
            }
            JObject pumpJsonObject = JObject.Parse(_pumpsConfigJson);

            _pumpsConfiguration.Pumps = pumpJsonObject["pumps"].Select(p => new Pump
            {
                Number = int.Parse(p["number"].ToString()),
                Pin = short.Parse(p["pin"].ToString()),
                Value = p["value"].ToString()
            }).ToList();
            return new JsonResult(new
            {
                Pumps = _pumpsConfiguration.Pumps
            });
        }

        [Route("pump/change")]
        public async Task<ActionResult> ChangePumpLiquid(int pumpNumber, string liquid)
        {
            _statusService.StartRunning();
            await ReadCurrentPumpConfiguration();
            _pumpsConfiguration.Pumps[pumpNumber - 1].Value = liquid;
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(_pumpsConfiguration, serializeOptions);
            using StreamWriter file = new(_pumpsFilePath);
            await file.WriteLineAsync(json);
            _statusService.StopRunning();

            return Ok();
        }

        [Route("pumps/all/start")]
        public async Task<ActionResult> StartAllPumps()
        {
            _statusService.StartRunning();
            foreach (var pump in _pumpsConfiguration.Pumps)
            {
                _pinService.SwitchPin(pump.Pin, true);
            }
            _statusService.StopRunning();
            return Ok();
        }

        [Route("pumps/all/stop")]
        public async Task<ActionResult> StopAllPumps()
        {
            _statusService.StartRunning();
            foreach (var pump in _pumpsConfiguration.Pumps)
            {
                _pinService.SwitchPin(pump.Pin, false);
            }
            _statusService.StopRunning();
            return Ok();
        }

        [Route("initialize")]
        public async Task<ActionResult> InitializeLiquidFlow()
        {
            _statusService.StartRunning();
            foreach (var pump in _pumpsConfiguration.Pumps)
            {
                _pinService.SwitchPin(pump.Pin, true);
            }

            await _displayService.WriteFirstLineDisplay("System fill");
            await _displayService.Countdown(22);

            foreach (var pump in _pumpsConfiguration.Pumps)
            {
                _pinService.SwitchPin(pump.Pin, false);
            }
            _displayService.MachineReadyForUse();
            _statusService.StopRunning();

            return Ok();
        }
    }
}
