using BlankBartender.Shared;
using BlankBartender.WebApi.Services.Interfaces;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using System.Device.Gpio;
using System.Device.I2c;

namespace BlankBartender.WebApi.Services
{
    public class DisplayService : IDisplayService
    {
        private readonly I2cDevice i2c;
        private readonly Pcf8574 driver;
        private readonly Lcd2004 lcd;

        public DisplayService()
        {
#if !DEBUG
            i2c = I2cDevice.Create(new I2cConnectionSettings(2, 0x27));
            driver = new Pcf8574(i2c);
            lcd = new Lcd2004(registerSelectPin: 0,
                                        enablePin: 2,
                                        dataPins: new int[] { 4, 5, 6, 7 },
                                        backlightPin: 3,
                                        backlightBrightness: 0.1f,
                                        readWritePin: 1,
                                        controller: new GpioController(PinNumberingScheme.Logical, driver));
#endif
        }

        public async Task PrepareStartDisplay(string name)
        {
#if !DEBUG
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            Write($"Start making");
            lcd.SetCursorPosition(0, 1);
            Write($"{name}");
            await Task.Delay(3000);
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            Write($"{name}");
#endif
        }

        public async Task Clear()
        {
            lcd.Clear();
        }

        public async Task WriteFirstLineDisplay(string text)
        {
            lcd.SetCursorPosition(0, 0);
            Write($"{text}");
        }
        public async Task WriteSecondLineDisplay(string text)
        {
            lcd.SetCursorPosition(0, 1);
            Write($"{text}");
        }

        public void CocktailReadyDisplay()
        {
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            Write($"Cocktail is ready");
            lcd.SetCursorPosition(0, 1);
            Write($"Take your glass!");
        }

        public void MachineReadyForUse()
        {
            lcd.Clear();
            lcd.SetCursorPosition(0, 1);
            lcd.SetCursorPosition(0, 0);
            Write("Machine is ready");
            lcd.SetCursorPosition(0, 1);
            Write("for use");
        }


        public void PlaceGlassMessage()
        {
            lcd.Clear();
            lcd.SetCursorPosition(0, 1);
            lcd.SetCursorPosition(0, 0);
            Write("Please place");
            lcd.SetCursorPosition(0, 1);
            Write("glass in holder");
        }
        public async Task Countdown(int time)
        {
            for (int seconds = 0; seconds < time; seconds++)
            {
                lcd.SetCursorPosition(0, 1);
                string message = string.Format("{0,16}", $"{time - seconds} seconds left");
                lcd.Write(message);
                await Task.Delay(980);
            }
        }

        public void Write(string source)
        {
            int spaces = 16 - source.Length;
            int padLeft = spaces / 2 + source.Length;
            lcd.Write(source.PadLeft(padLeft).PadRight(16));
        }
    }
}
