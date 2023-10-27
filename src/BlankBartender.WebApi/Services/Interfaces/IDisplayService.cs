using BlankBartender.Shared;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IDisplayService
    {
        Task Countdown(int time);
        Task PrepareStartDisplay(string name);
        Task WriteFirstLineDisplay(string text);
        public void PlaceGlassMessage();
        void CocktailReadyDisplay();
        void MachineReadyForUse();
    }
}
