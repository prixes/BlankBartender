
namespace BlankBartender.WebApi.Services.Interfaces;

public interface IConfigurationFileService
{
    string GetFullPath(string fileName);
    Task<T?> ReadJsonAsync<T>(string fileName, CancellationToken cancellationToken = default);
    Task WriteJsonAsync<T>(string fileName, T data, CancellationToken cancellationToken = default);
}