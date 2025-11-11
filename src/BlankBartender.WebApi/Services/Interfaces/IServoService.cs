
namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IServoService
    {
        public void MovePlatformToIceDispenser();
        public void MovePlatformFromIceToStart();
        public void MovePlatformToStirrer();
        public void MovePlatformToStart();
        public void MoveStirrerToGlass();
        public void MoveStirrerToStart();
        public void MoveAngleServo300();

    }
}
