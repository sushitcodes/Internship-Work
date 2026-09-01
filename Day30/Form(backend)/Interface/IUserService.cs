using Form.DTOs;
namespace Form.Interfaces;

public interface IUserService
{
    Task<List<UserSummaryDto>> GetStudentsAsync();
}