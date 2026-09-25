using Form.DTOs;

namespace Form.Interfaces;

public interface IBulkImportService
{
    Task<BulkImportResultDto> ImportStudentsFromExcelAsync(
        IFormFile file, Guid classRoomId);

    byte[] GenerateSampleExcelTemplate();
}