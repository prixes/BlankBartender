using BlankBartender.Shared;

namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface IDisplayService
    {
        Task Countdown(int time);
        Task PrepareStartDisplay(Drink drink);
        Task WriteFirstLineDisplay(string text);
        void CocktailReadyDisplay();
        void MachineReadyForUse();
    }
}
