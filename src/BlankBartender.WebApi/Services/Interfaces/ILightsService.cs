namespace BlankBartender.WebApi.Services.Interfaces
{
    public interface ILightsService
    {
        void StartCocktailLights();

        void TurnLight(string lightName, bool mode);
    }
}
