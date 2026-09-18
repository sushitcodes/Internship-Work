using Form.Interfaces;

namespace Form.FileStorage;

public class LocalFileStorageService(IWebHostEnvironment _env) : IFileStorageService
{
    private static readonly string[] AllowedExtensions =
    { ".pdf", ".jpg", ".jpeg", ".png", ".docx" };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName);

        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(
                $"File type '{ext}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        if (file.Length == 0)
            throw new InvalidOperationException("File is empty.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File exceeds the 10 MB limit.");

        var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        var uploadsFolder = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadsFolder);
        // Guid alone is the filename. We never trust or embed the client's
        // original filename — it's attacker-controlled and Path.Combine
        // doesn't sanitize it for us.
        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{uniqueFileName}"; // stored in DB as Submission.FileUrl
    }
}
