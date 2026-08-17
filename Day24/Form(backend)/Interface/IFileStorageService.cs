using Microsoft.AspNetCore.Http;

namespace Form.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
}
