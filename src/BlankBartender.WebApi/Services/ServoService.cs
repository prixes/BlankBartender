using BlankBartender.WebApi.Services.Interfaces;
using Iot.Device.Pwm;
using System.Device.I2c;
using System.Device.Pwm;

namespace BlankBartender.WebApi.Services
{
    public class ServoService : IServoService
    {
        const int stirrerServoStop = 307;
        const int stirrerServoMaxClockwise = 215;  // Adjust based on actual direction
        const int stirrerServoMaxCounterClockwise = 388;  // Adjust based on actual direction

        const int platformServoMin = 102;
        const int platformServoMid = 307;
        const int platformServoMax = 512;

        I2cConnectionSettings i2cConnection = new I2cConnectionSettings(3, 0x40);
        I2cDevice i2cDevice;
        Pca9685 pca;

        double t, easedValue, dutyCycle;
        const int loopCount = 1000;
        const double seconds = 1;
        int delay = (int)(seconds * 1000) / loopCount;

        PwmChannel plaformPwmChannel;
        PwmChannel stirrerPwmChannel;

        public ServoService()
        {
            i2cDevice = I2cDevice.Create(i2cConnection);
            pca = new Pca9685(i2cDevice, 50);
            plaformPwmChannel = pca.CreatePwmChannel(0);
            stirrerPwmChannel = pca.CreatePwmChannel(1);
        }

        public void MovePlatformToStirrer()
        {
            for (int i = loopCount / 2; i <= loopCount; i++)
            {
                plaformPwmChannel.DutyCycle = CalculateDutyCycle(i) / 4096.0;
                Thread.Sleep(delay);
            }
        }
        public void MovePlatformToStart()
        {
            for (int i = loopCount; i >= loopCount / 2; i--)
            {
                plaformPwmChannel.DutyCycle = CalculateDutyCycle(i) / 4096.0;
                Thread.Sleep(delay);
            }
        }
        public void MoveStirrerToGlass() 
        {
            for (int i = loopCount; i >= 0; i--)
            {
                t = (double)i / loopCount;
                easedValue = (Math.Sin((t - 0.5) * Math.PI) + 1) / 2;
                dutyCycle = (1 - easedValue) * stirrerServoMaxClockwise + easedValue * stirrerServoStop;

                stirrerPwmChannel.DutyCycle = dutyCycle / 4096.0;
                Thread.Sleep((int)(seconds * 1000) / (2 * loopCount));
                Console.WriteLine(dutyCycle.ToString());
            }
        }

        public void MoveStirrerToStart() 
        {
            for (int i = 0; i <= loopCount; i++)
            {
                t = (double)i / loopCount;
                easedValue = (Math.Sin((t - 0.5) * Math.PI) + 1) / 2; // Eased value between 0 and 1.
                dutyCycle = (1 - easedValue) * stirrerServoMaxClockwise + easedValue * stirrerServoStop;

                stirrerPwmChannel.DutyCycle = dutyCycle / 4096.0; // Convert to a value between 0 and 1.
                Thread.Sleep((int)(seconds * 1000) / (2 * loopCount));
                Console.WriteLine(dutyCycle.ToString());
            }
        }

        private double CalculateDutyCycle(int i)
        {
            double t = (double)i / loopCount;
            double easedValue = (Math.Sin((t - 0.5) * Math.PI) + 1) / 2;
            return easedValue * platformServoMax + (1 - easedValue) * platformServoMin;
        }
    }
}
