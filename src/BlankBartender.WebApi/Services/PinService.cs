using BlankBartender.Shared;
using BlankBartender.WebApi.Services.Interfaces;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

namespace BlankBartender.WebApi.Services
{
    public class PinService : IPinService
    {
        private readonly GpioController gpioController;

        public PinService() 
        {
#if !DEBUG
            var driverGpio = new SysFsDriver();
            gpioController = new GpioController(PinNumberingScheme.Logical, driverGpio);
            Console.WriteLine("Pin service initialized");
#endif
        }
        public void SwitchPin(int pin, bool on)
        {
            if (!gpioController.IsPinOpen(pin))
                gpioController.OpenPin(pin, PinMode.Output);
            if (on)
            {
                gpioController.Write(pin, PinValue.Low);
            }
            else
            {
                gpioController.Write(pin, PinValue.High);
            }
            Console.WriteLine("Pin switch");
        }
    }
}
