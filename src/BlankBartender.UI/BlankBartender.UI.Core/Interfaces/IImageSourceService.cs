
namespace BlankBartender.UI.Core.Interfaces
{
    public interface IImageSourceService
    {
        Task<string> GetCocktailImageAsync(int id);
    }
}
