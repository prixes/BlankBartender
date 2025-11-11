using BlankBartender.Shared;
using BlankBartender.WebApi.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

namespace BlankBartender.WebApi.Services
{
    public class LightsService : ILightsService
    {
        private const string LightConfigFileName = "lights-config.json";
#if !DEBUG
        private readonly SysFsDriver driverGpio;
        private readonly GpioController _gpioController;
#endif
        private readonly Dictionary<string, short> _lightPins = new Dictionary<string, short>();

        public LightsService()
        {
#if !DEBUG
             driverGpio = new SysFsDriver();
            _gpioController = new GpioController(PinNumberingScheme.Logical, driverGpio);
#endif

            var lightsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration", LightConfigFileName);
            if (!File.Exists(lightsFilePath)) return;

            var lightConfigJson = File.ReadAllText(lightsFilePath);
            if (string.IsNullOrEmpty(lightConfigJson)) return;

            var lights = JObject.Parse(lightConfigJson)["lights"]
                .Select(p => new Light
                {
                    Name = p["name"].ToString(),
                    Pin = short.Parse(p["pin"].ToString())
                }).ToList();

            foreach (var light in lights)
            {
                _lightPins[light.Name] = light.Pin;
#if !DEBUG
                _gpioController.OpenPin(light.Pin, PinMode.Output, PinValue.High);
#endif
            }
        }

        public void StartCocktailLights()
        {
            TurnLight("blue", true);
            TurnLight("green", true);
            TurnLight("red", true);
        }

        public void TurnLight(string light, bool on)
        {
            if (!_lightPins.TryGetValue(light, out short pin)) return;
#if !DEBUG
            var pinValue = on ? PinValue.Low : PinValue.High;
            if (_gpioController.Read(pin) != pinValue)
                _gpioController.Write(pin, pinValue);
#endif
        }
    }
}
