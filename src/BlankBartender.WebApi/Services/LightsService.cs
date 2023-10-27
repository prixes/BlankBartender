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
        public short redPin;
        public short greenPin;
        public short bluePin;

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
                redPin = lights.Where(light => light.Name == "red").First().Pin;
                bluePin = lights.Where(light => light.Name == "blue").First().Pin;
                greenPin = lights.Where(light => light.Name == "green").First().Pin;
                redLightController = new GpioController();
                redLightController.OpenPin(redPin, PinMode.Output, PinValue.High);
                blueLightController = new GpioController();
                blueLightController.OpenPin(bluePin, PinMode.Output, PinValue.High);
                greenLightController = new GpioController();
                greenLightController.OpenPin(greenPin, PinMode.Output, PinValue.High);
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
                    if(redLightController.Read(redPin) != pinValue)
                        redLightController.Write(redPin, pinValue);
                    break;
                case "blue":
                    if (blueLightController.Read(bluePin) != pinValue)
                        blueLightController.Write(bluePin, pinValue);
                    break;
                case "green":
                    if (greenLightController.Read(greenPin) != pinValue)
                        greenLightController.Write(greenPin, pinValue);
                    break;
            }
        }
    }
}
