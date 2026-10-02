using Form.Interfaces;

namespace Form.FileStorage;

public class LocalFileStorageService(IWebHostEnvironment _env) : IFileStorageService
{
    // OrdinalIgnoreCase: ".JPG" and ".jpg" are the same thing.
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".docx" };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    // OUTSIDE wwwroot: the static file middleware cannot serve this folder directly.
    private string UploadsFolder() => Path.Combine(_env.ContentRootPath, "App_Data", "uploads");

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(
                $"File type '{ext}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        if (file.Length == 0)
            throw new InvalidOperationException("File is empty.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File exceeds the 10 MB limit.");

        // The extension is just a label the client chose. Check the real first bytes too.
        if (!await HasValidSignatureAsync(file, ext))
            throw new InvalidOperationException("File content does not match its extension.");

        var uploadsFolder = UploadsFolder();
        Directory.CreateDirectory(uploadsFolder);

        // Guid alone is the filename. The client's original name is never used.
        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var stream = new FileStream(
            filePath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);
        await file.CopyToAsync(stream);

        return $"/uploads/{uniqueFileName}"; // stored in DB (Submission.FileUrl, UserProfile.AvatarUrl)
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        // Only ever delete a bare file name inside the uploads folder.
        // Never trust a stored path: GetFileName strips any "../" tricks.
        var name = Path.GetFileName(fileUrl);
        if (string.IsNullOrEmpty(name)) return Task.CompletedTask;

        var path = Path.Combine(UploadsFolder(), name);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private static async Task<bool> HasValidSignatureAsync(IFormFile file, string ext)
    {
        var h = new byte[12];
        await using var s = file.OpenReadStream();
        var read = await s.ReadAsync(h.AsMemory(0, h.Length));
        if (read < 4) return false;

        return ext switch
        {
            ".pdf" => h[0] == 0x25 && h[1] == 0x50 && h[2] == 0x44 && h[3] == 0x46,            // %PDF
            ".png" => h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47,            // \x89PNG
            ".jpg" or ".jpeg" => h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF,                  // JPEG
            ".webp" => read >= 12 && h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46
                       && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50,      // RIFF....WEBP
            ".docx" => h[0] == 0x50 && h[1] == 0x4B,                                           // zip container "PK"
            _ => false,
        };
    }
}