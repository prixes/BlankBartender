using BlankBartender.Shared;
using BlankBartender.WebApi.Configuration;
using BlankBartender.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using PumpsConfiguration = BlankBartender.Shared.PumpsConfiguration;

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

        private IEnumerable<string>? liquids;
        private readonly ILogger<DrinkController> _logger;
        private string? _pumpsConfigJson;

        private PumpsConfiguration _pumpsConfiguration;

        private readonly IDisplayService _displayService;
        private readonly IPinService _pinService;
        private readonly IStatusService _statusService;
        private readonly ISettingsService _settingsService;
        public ConfigurationController(ILogger<DrinkController> logger, IPinService pinService,
                                       IDisplayService displayService, IStatusService statusService,
                                       ISettingsService settingsService)
        {
            _logger = logger;
            _pinService = pinService;
            _pumpsConfiguration = new PumpsConfiguration();
            _displayService = displayService;
            _statusService = statusService;
            _settingsService = settingsService;

            _liquidsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", _liquidsConfigFileName);
            _pumpsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationData", _pumpConfigFileName);
            ReadCurrentLiquids();
            ReadCurrentPumpConfiguration();

        }

        [Route("liquids")]
        public ActionResult ReadCurrentLiquids()
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
        public ActionResult ReadCurrentPumpConfiguration()
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
                FlowRate = decimal.Parse(p["flowRate"].ToString()),
                Value = p["value"].ToString()
            }).ToList();
            return new JsonResult(new
            {
                _pumpsConfiguration.Pumps
            });
        }

        [Route("pump/change")]
        public async Task<ActionResult> ChangePumpLiquid(int pumpNumber, string liquid)
        {
            await _statusService.StartRunning();
            ReadCurrentPumpConfiguration();
            _pumpsConfiguration.Pumps[pumpNumber - 1].Value = liquid;
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(_pumpsConfiguration, serializeOptions);
            using StreamWriter file = new(_pumpsFilePath);
            await file.WriteLineAsync(json);
            await _statusService.StopRunning();

            return Ok();
        }

        [Route("pumps/all/start")]
        public async Task<ActionResult> StartAllPumps()
        {
            await _statusService.StartRunning();
            foreach (var pump in _pumpsConfiguration.Pumps)
            {
                _pinService.SwitchPin(pump.Pin, true);
            }
            await _statusService.StopRunning();
            return Ok();
        }

        [Route("pump/{pumpNumber}/start")]
        public async Task<ActionResult> StartPump(int pumpNumber)
        {
            await _statusService.StartRunning();
            var pump = _pumpsConfiguration.Pumps.First(x => x.Number == pumpNumber);
            _pinService.SwitchPin(pump.Pin, true);
            await _statusService.StopRunning();
            return Ok();
        }

        [Route("pumps/all/stop")]
        public async Task<ActionResult> StopAllPumps()
        {
            await _statusService.StartRunning();
            foreach (var pump in _pumpsConfiguration.Pumps)
            {
                _pinService.SwitchPin(pump.Pin, false);
            }
            await _statusService.StopRunning();
            return Ok();
        }

        [Route("pump/{pumpNumber}/stop")]
        public async Task<ActionResult> Stop(int pumpNumber)
        {
            await _statusService.StartRunning();
            var pump = _pumpsConfiguration.Pumps.First(x => x.Number == pumpNumber);
            _pinService.SwitchPin(pump.Pin, false);
            await _statusService.StopRunning();
            return Ok();
        }

        [Route("initialize")]
        public async Task<ActionResult> InitializeLiquidFlow()
        {
            await _statusService.StartRunning();
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
            await _statusService.StopRunning();

            return Ok();
        }

        [HttpGet]
        [Route("settings")]
        public ActionResult GetMachineSettings()
        {
            var settingsValues = _settingsService.GetMachineSettings();
            return new JsonResult(new
            {
                settingsValues.UseCameraAI,
                settingsValues.UseStirrer
            });
        }

        [HttpPut]
        [Route("settings")]
        public async Task<ActionResult> SetMachineSettings(bool useCameraAI, bool useStitter)
        {
            await _statusService.StartRunning();
            _settingsService.SetMachineSettings(useCameraAI, useStitter);
            await _statusService.StopRunning();

            return Ok();
        }
    }
}
