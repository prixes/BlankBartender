using BlankBartender.WebApi.Services.Interfaces;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

namespace BlankBartender.WebApi.Services
{
    public class StirrerService : IStirrerService
    {
        private readonly GpioController _gpioController;
        public StirrerService() 
        {
            var driverGpio = new SysFsDriver();
            _gpioController = new GpioController(PinNumberingScheme.Logical, driverGpio);
        }

        public async Task StartStirrer() 
        {
            TurnStirrer(on:true);
            await Task.Delay(4000);
        }
        public void StopStirrer() 
        {
            TurnStirrer(on:false);
        }

        private void TurnStirrer(bool on)
        {
            var pinValue = on ? PinValue.Low : PinValue.High;
            if (_gpioController.Read(147) != pinValue)
                _gpioController.Write(147, pinValue);
        }
    }
}
