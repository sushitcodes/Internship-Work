using Form.DTOs;
using Form.Interfaces;
using Form.Entities;
using Microsoft.AspNetCore.Http;
using static Form.Dtos.Class;

namespace Form.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _repository;
    private readonly IFileStorageService _fileStorage;

    // Constructor injection — this class doesn't know or care whether
    // _repository talks to SQL Server, Postgres, or an in-memory list.
    public SubmissionService(ISubmissionRepository repository, IFileStorageService fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request)
    {
        // Backend validation is the REAL validation — the frontend's checks
        // are only for UX and can be bypassed by any client.
        ValidateFile(request.File);

        var fileUrl = await _fileStorage.SaveFileAsync(request.File);

        var submission = new Submission
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            FileUrl = fileUrl,
            Education = request.Education.Select(e => new EducationEntry
            {
                Institution = e.Institution,
                Degree = e.Degree,
                Year = e.Year
            }).ToList()
        };

        var saved = await _repository.AddAsync(submission);
        return MapToDto(saved);
    }

    public async Task<List<SubmissionDto>> GetAllAsync()
    {
        var submissions = await _repository.GetAllAsync();
        return submissions.Select(MapToDto).ToList();
    }

    public async Task<SubmissionDto?> GetByIdAsync(Guid id)
    {
        var submission = await _repository.GetByIdAsync(id);
        return submission is null ? null : MapToDto(submission);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static void ValidateFile(IFormFile file)
    {
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("File type not allowed. Use PDF, JPG, or PNG.");

        const long maxSizeBytes = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxSizeBytes)
            throw new InvalidOperationException("File too large. Max size is 5MB.");
    }

    private static SubmissionDto MapToDto(Submission s) => new()
    {
        Id = s.Id,
        FullName = s.FullName,
        Email = s.Email,
        Phone = s.Phone,
        FileUrl = s.FileUrl,
        CreatedAt = s.CreatedAt,
        Education = s.Education.Select(e => new EducationEntryDto
        {
            Institution = e.Institution,
            Degree = e.Degree,
            Year = e.Year
        }).ToList()
    };
}
