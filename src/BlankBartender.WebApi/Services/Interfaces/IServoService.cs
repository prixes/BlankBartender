
namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IServoService
    {
        public void MovePlatformToStirrer();
        public void MovePlatformToStart();
        public void MoveStirrerToGlass();
        public void MoveStirrerToStart();

    }
}
