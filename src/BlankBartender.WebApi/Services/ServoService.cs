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
        const int loopCountPlatform = 1000;
        const double platformSeconds = 1;
        const int loopCountArmDown = 100;
        const int loopCountArmUp = 100;
        const double armDownSeconds = 2.7;
        const double armUpSeconds = 2.5;
        int delay = (int)(platformSeconds * 1000) / loopCountPlatform;

        PwmChannel plaformPwmChannel;
        PwmChannel stirrerPwmChannel;

        public ServoService()
        {
            i2cDevice = I2cDevice.Create(i2cConnection);
            pca = new Pca9685(i2cDevice, 50);
            plaformPwmChannel = pca.CreatePwmChannel(7);
            stirrerPwmChannel = pca.CreatePwmChannel(1);
        }

        public void MovePlatformToStirrer()
        {
            plaformPwmChannel.Start();
            for (int i = loopCountPlatform / 2; i <= loopCountPlatform; i++)
            {
                plaformPwmChannel.DutyCycle = CalculateDutyCycle(i) / 4096.0;
                Thread.Sleep(delay);
            }
            plaformPwmChannel.DutyCycle = 0;
            plaformPwmChannel.Stop();
        }
        public void MovePlatformToStart()
        {
            plaformPwmChannel.Start();
            for (int i = loopCountPlatform; i >= loopCountPlatform / 2; i--)
            {
                plaformPwmChannel.DutyCycle = CalculateDutyCycle(i) / 4096.0;
                Thread.Sleep(delay);
            }
            plaformPwmChannel.DutyCycle = 0;
            plaformPwmChannel.Stop();
        }
        public void MoveStirrerToGlass() 
        {
            stirrerPwmChannel.Start();
            for (int i = loopCountArmDown; i >= 0; i--)
            {
                t = (double)i / loopCountArmDown;
                easedValue = (Math.Sin((t - 0.85) * Math.PI) + 1) / 2;
                dutyCycle = (1 - easedValue) * stirrerServoMaxCounterClockwise + easedValue * stirrerServoStop;

                stirrerPwmChannel.DutyCycle = dutyCycle / 4096.0;
                Thread.Sleep((int)(armDownSeconds * 1000) / (2 * loopCountArmDown));
               // Console.WriteLine(dutyCycle.ToString());
            }
            for (int i = 0; i <= loopCountArmDown; i++)
            {
                t = (double)i / loopCountArmDown;
                easedValue = (Math.Sin((t - 0.85) * Math.PI) + 1) / 2; // Eased value between 0 and 1.
                dutyCycle = (1 - easedValue) * stirrerServoMaxCounterClockwise + easedValue * stirrerServoStop;

                stirrerPwmChannel.DutyCycle = dutyCycle / 4096.0; // Convert to a value between 0 and 1.
                Thread.Sleep((int)(armDownSeconds * 1000) / (2 * loopCountArmDown));
               // Console.WriteLine(dutyCycle.ToString());
            }
            stirrerPwmChannel.DutyCycle = 0;
            stirrerPwmChannel.Stop();
        }

        public void MoveStirrerToStart() 
        {
            stirrerPwmChannel.Start();
            for (int i = loopCountArmUp; i >= 0; i--)
            {
                t = (double)i / loopCountArmUp;
                easedValue = (Math.Sin((t - 0.7) * Math.PI) + 1) / 2;
                dutyCycle = (1 - easedValue) * stirrerServoMaxClockwise + easedValue * stirrerServoStop;

                stirrerPwmChannel.DutyCycle = dutyCycle / 4096.0;
                Thread.Sleep((int)(armUpSeconds * 1000) / (2 * loopCountArmUp));
               // Console.WriteLine(dutyCycle.ToString());
            }
            for (int i = 0; i <= loopCountArmUp; i++)
            {
                t = (double)i / loopCountArmUp;
                easedValue = (Math.Sin((t - 0.7) * Math.PI) + 1) / 2; // Eased value between 0 and 1.
                dutyCycle = (1 - easedValue) * stirrerServoMaxClockwise + easedValue * stirrerServoStop;

                stirrerPwmChannel.DutyCycle = dutyCycle / 4096.0; // Convert to a value between 0 and 1.
                Thread.Sleep((int)(armUpSeconds * 1000) / (2 * loopCountArmUp));
               // Console.WriteLine(dutyCycle.ToString());
            }
            stirrerPwmChannel.DutyCycle = 0;
            stirrerPwmChannel.Stop();
        }

        private double CalculateDutyCycle(int i)
        {
            double t = (double)i / loopCountPlatform;
            double easedValue = (Math.Sin((t - 0.5) * Math.PI) + 1) / 2;
            return easedValue * platformServoMax + (1 - easedValue) * platformServoMin;
        }
    }
}
