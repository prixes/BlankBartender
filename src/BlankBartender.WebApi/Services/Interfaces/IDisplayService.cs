using BlankBartender.Shared;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IDisplayService
    {
        public Task Countdown(int time);
        public Task PrepareStartDisplay(string name);
        public Task WriteFirstLineDisplay(string text);
        public Task WriteSecondLineDisplay(string text);
        public Task Clear();
        public void PlaceGlassMessage();
        public void CocktailReadyDisplay();
        public void MachineReadyForUse();
    }
}
