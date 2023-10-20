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
            gpioController = new GpioController();
            Console.WriteLine("Pin service initialized");
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
