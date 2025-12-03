namespace BlankBartender.WebApi.Repositories.Interfaces;

public interface ILiquidsRepository
{
    Task<List<string>> GetAllAsync();
    Task AddAsync(string liquidName);
    Task<bool> RemoveAsync(string liquidName);
}