using BlankBartender.Shared;
using BlankBartender.WebApi.Services.Interfaces;
using System.Device.Gpio;

namespace BlankBartender.WebApi.Services
{
    public class PinService : IPinService
    {
        private readonly GpioController gpioController;

        public PinService() 
        {
#if !DEBUG
            gpioController = new GpioController();
#endif
            Console.WriteLine("Pin service initialized");
        }
        public void SwitchPin(int pin, bool on)
        {
#if !DEBUG
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
#endif
            Console.WriteLine("Pin switch");
        }
    }
}
