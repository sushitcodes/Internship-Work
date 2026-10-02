// Interface/IFileStorageService.cs
namespace Form.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
    Task DeleteFileAsync(string fileUrl);
}