using BlankBartender.Shared;
using BlankBartender.WebApi.Services.Interfaces;
using Iot.Device.ExplorerHat;
using Newtonsoft.Json.Linq;
using System.Device.Gpio;

namespace BlankBartender.WebApi.Services
{
    public class LightsService : ILightsService
    {
        private const string _lightConfigFileName = "lights-config.json";
        private readonly string _lightConfigJson;
        private readonly GpioController redLightController;
        private readonly GpioController greenLightController;
        private readonly GpioController blueLightController;
        public List<Light> lights;

        public LightsService() 
        {
            var lightsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration", _lightConfigFileName);
            if (System.IO.File.Exists(lightsFilePath))
            {
                _lightConfigJson = System.IO.File.ReadAllText(lightsFilePath);
            }


            if (!string.IsNullOrEmpty(_lightConfigJson))
            {
                JObject lightJsonObject = JObject.Parse(_lightConfigJson);

                lights = lightJsonObject["lights"].Select(p => new Light
                {
                    Name = p["name"].ToString(),
                    Pin = short.Parse(p["pin"].ToString()),
                }).ToList();
                redLightController = new GpioController();
                redLightController.OpenPin(lights.Where(light => light.Name == "red").First().Pin, PinMode.Output, PinValue.High);
                blueLightController = new GpioController();
                blueLightController.OpenPin(lights.Where(light => light.Name == "blue").First().Pin, PinMode.Output, PinValue.High);
                greenLightController = new GpioController();
                greenLightController.OpenPin(lights.Where(light => light.Name == "green").First().Pin, PinMode.Output, PinValue.High);
            }
        }

        public void StartCocktailLights()
        {
            TurnLight("blue", false);
            TurnLight("green", false);
            TurnLight("red", true);
        }

        public void TurnLight(string light, bool on)
        {
            PinValue pinValue;
            if (on == true) pinValue = PinValue.Low; else pinValue = PinValue.High;

            switch (light)
            {
                case "red":
                    redLightController.Write(lights.Where(light => light.Name == "red").First().Pin, pinValue);
                    break;
                case "blue":
                    blueLightController.Write(lights.Where(light => light.Name == "blue").First().Pin, pinValue);
                    break;
                case "green":
                    greenLightController.Write(lights.Where(light => light.Name == "green").First().Pin, pinValue);
                    break;
            }
        }
    }
}
